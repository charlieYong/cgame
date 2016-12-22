using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using System.Collections.Generic;

using ZR_MasterTools;

namespace Sgame.Battle {
    // 战斗管理类
    public class BattleManager : MonoBehaviour {
        // 战斗管理类
        public static BattleManager Instance {get; private set;}
        // 战斗数据管理类
        BattleDataManager DataMgr {get {return BattleDataManager.Instance;}}

        // 战斗协程的时间管理器
        public readonly OneTimeTimerManager TimerMgr = new OneTimeTimerManager ();
        // update信号
        public readonly OneTimeSignal UpdateSignal = new OneTimeSignal ();

        Transform _transform;

        public Transform BattleCameraTrans {get; private set;}
        public Camera BattleCamera {get; private set;}

        // 移动的场景对象
        Transform _sceneRootTrans = null;
        // 当前移动值
        public float TotalMoveDistance {get; private set;}
        // 战斗是否暂停
        public bool IsPause {get; private set;}

        TBattleField _battleField = null;
        BattleFighter _playerFighter;
    
        public static void Start () {
            if (null != Instance) {
                Debug.LogWarning ("BattleManager already exists!!!");
                return;
            }
            GameObject go = GameObject.Instantiate (
                Resources.Load<GameObject> ("GamePrefabs/BattleRoot"),
                Vector3.zero,
                Quaternion.identity
            ) as GameObject;
            DontDestroyOnLoad (go);
            Instance = go.AddComponent<BattleManager> ();
            Instance.StartBattle ();
        }

        public static void Destroy () {
            if (null != Instance) {
                Destroy (Instance.gameObject);
                Instance = null;
            }
        }

        void Awake () {
            _transform = transform;
            TotalMoveDistance = 0f;
            IsPause = true;
        }

        void SetupBattleFieldData () {
            if (BattleType.Endless == DataMgr.CurBattleType) {
                _battleField = (DataMgr.Params.config as TEndlessPve).GetRandomBattleField ((null == _battleField) ? -1 : _battleField.id);
            }
        }

        void StartBattle () {
            BattleCameraTrans = _transform.Find ("BattleCamera");
            BattleCamera = BattleCameraTrans.GetComponent<Camera> ();
            SetupBattleFieldData ();
            //SceneMgr.Instance.ChangeScene (_curPve.scene, OnBattleReady);
            SceneManager.LoadScene (_battleField.BattleScene.scene);
            StartCoroutine (OnBattleReady ());
        }

        IEnumerator OnBattleReady () {
            // 测试代码 等场景加载完毕
            yield return new WaitForSeconds (0.1f);
            // 获取场景移动的对象
            _sceneRootTrans = GameObject.Find ("/" + _battleField.BattleScene.moveRootGo).transform;
            // 1）按配置加载场景组件
            LoadSceneComponentConfig ();
            // 2）创建战机，加载战机配置，技能道具等
            //CreatePlayerFighter ();
            // 3）按配置创建场景怪物
            LoadSceneMonsterConfig ();
            IsPause = false;
        }

        void CreatePlayerFighter () {
            // 创建模型
            GameObject go = GameObject.Instantiate (Resources.Load<GameObject> ("Models/Player/Player001")) as GameObject;
            Transform t = go.transform;
            t.parent = _transform;
            t.localPosition = new Vector3 (8, 0f, 0f);
            t.Rotate (new Vector3 (0f, -90f, 0f));
            _playerFighter = go.AddComponent<BattleFighter> ();
            _playerFighter.InitForBattle (DataMgr.Params.fighter);
            _playerFighter.StartBattle ();
        }

        SceneMonsterManager _sceneMonsterMgr = null;

        void LoadSceneMonsterConfig () {
            _sceneMonsterMgr = new SceneMonsterManager (_battleField.monsterConfigID);
        }

        public void CreateMonster (MonsterItemOnScene item) {
            TMonster config = ConfigTableMonster.Instance.GetConfigByID (item.monsterid);
            GameObject go = BattleResManager.Instance.CreateMonsterGo (config.prefab);
            Transform t = go.transform;
            t.position = item.position;
            t.eulerAngles = item.eulerAngels;
            float scale = config.scale/1000f;
            t.localScale = new Vector3 (scale, scale, scale);
            BattleMonster unit = go.AddComponent<BattleMonster> ();
            unit.InitForBattle (config);
            //unit.StartBattle ();
        }

        SceneComponentManager _sceneComponentMgr = null;

        void LoadSceneComponentConfig () {
            _sceneComponentMgr = new SceneComponentManager (_battleField.componentConfigId);
            _sceneComponentMgr.SetTransformRoot (GameObject.Find ("/" + _battleField.BattleScene.componentRootGo).transform);
        }

        public void CreateSceneMonster (int monsterid, Transform parent) {
            TMonster config = ConfigTableMonster.Instance.GetConfigByID (monsterid);
            GameObject go = BattleResManager.Instance.CreateMonsterGo (config.prefab);
            Utils.SetTransform (parent, go.transform, true);
            BattleMonster unit = go.AddComponent<BattleMonster> ();
            unit.InitForBattle (config);
        }

        void Update () {
            if (IsPause) {
                return;
            }
            TimerMgr.Update ();
            MoveSceneOnUpdate ();
            _sceneComponentMgr.Update ();
            _sceneMonsterMgr.Update ();
            UpdateSignal.Call ();
        }

        void MoveSceneOnUpdate () {
            // 移动场景
            float deltaX = _battleField.BattleScene.moveSpeed * Time.deltaTime;
            _sceneRootTrans.position += (Vector3.right * deltaX);
            TotalMoveDistance += deltaX;
        }
    }
}
