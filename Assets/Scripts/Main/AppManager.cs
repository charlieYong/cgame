using UnityEngine;
using System.Collections;

using ZR_MasterTools;
using Sgame.UI;


namespace Sgame
{
    public class AppManager : MonoBehaviour
    {

        public static AppManager Instance { get; private set; }
        GameObject _gameObject;


        void Awake() {
            if (null != Instance) {
                Debug.LogWarning("Duplicated AppManager Instance!");
                Destroy(gameObject);
                return;
            }
            Instance = this;
            _gameObject = gameObject;
            // 手机平台设置帧率
            if (Application.isMobilePlatform) {
                Application.targetFrameRate = 45;
            }
            DontDestroyOnLoad(_gameObject);
            InitMemory();
            InitGameSpecificLogic();
            InitGameLogicMgr();
            StartCoroutine(OpenLoginUI());
        }

        // 提前分配一块内存，提高小内存的使用率
        void InitMemory() {
            System.Object[] tmp = new System.Object[1024];
            for (int i = 0; i < 1024; i++) {
                tmp[i] = new byte[1024];
            }
            tmp = null;
        }

        void Start() {

        }

        IEnumerator OpenLoginUI() {
            while (null == Sgame.UI.UIManager.Instance) {
                yield return null;
            }
           // Sgame.UI.UIManager.Instance.OpenWin(WinID.UILogin);
            Sgame.UI.UIManager.Instance.OpenWin(WinID.UIMainTemp);
        }

        void InitGameSpecificLogic() {
            _gameObject.AddComponent<NetworkMgr>();
            NetworkMgr.Instance.Setup(new GameSpecificLogic(), new ProtobufDecode());
            NetworkMgr.Instance.onRetryFail += NetworkHelper.HandleNetworkTimeout;
        }

        void InitGameLogicMgr() {
            _gameObject.AddComponent<AuthMgr>();
            _gameObject.AddComponent<PlayerMgr>();
            _gameObject.AddComponent<PilotMgr>();
        }



        public static void ClearOnExitApp() {

        }


        // 需要提前加载的配置文件数量
        public int TotalPreloadFileNum { get; private set; }
        // 当前已加载的配置文件数量
        public int CurLoadedFileNum { get; private set; }
        public IEnumerator LoadConfigFiles() {
            TotalPreloadFileNum = 1;
            CurLoadedFileNum = 0;

            ClientLocalConfig.Instance.Load();
            CurLoadedFileNum++;
            yield return null;
        }
    }
}