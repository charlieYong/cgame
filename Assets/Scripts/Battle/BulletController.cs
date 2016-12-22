using UnityEngine;
using System.Collections;
using System.Collections.Generic;

using ZR_MasterTools;

namespace Sgame.Battle {
    // 子弹控制器
    public class BulletController : MonoBehaviour {
        // 子弹速度需要配置不？
        public const float DefaultSpeed = 15f;

        GameObject _go;
        Transform _transform;

        TBullet _config = null;
        BattleUnit _caster = null;
        BattleSkill _skill = null;

        // 子弹移动方向
        Vector3 _direction = Vector3.zero;
        // 子弹命中后执行的ai列表
        List<int> _aiListOnHit = null;

        void Awake () {
            _go = gameObject;
            _transform = transform;
        }

        public void Init (BattleUnit caster, BattleSkill skill, TBullet config) {
            _caster = caster;
            _skill = skill;
            _config = config;
        }

        public void Fire (Vector3 direction, List<int> aiListOnHit=null) {
            _direction = direction;
            _aiListOnHit = aiListOnHit;
        }

        void Update () {
            _transform.position += (_direction * DefaultSpeed * Time.deltaTime);
            DestroyOnOffScreen ();
        }

        void DestroyOnOffScreen () {
            if (!Utils.IsOnScreen (BattleManager.Instance.BattleCamera, _transform.position, 0f)) {
                Destroy (_go);
            }
        }

        // 获取发生碰撞对象的顶层对象
        // TODO 可以通过在碰撞体上挂一个脚本，awake时就索引到自己的root parent，这样直接获变量即可
        GameObject GetRootGo (GameObject go) {
            Transform t = go.transform;
            Transform root = t;
            while (null != root.parent) {
                root = root.parent;
            }
            return root.gameObject;
        }

        // 碰撞处理
        void OnTriggerEnter (Collider co) {
            if (null == co.gameObject) {
                return;
            }
            GameObject go = GetRootGo (co.gameObject);
            BattleUnit target = go.GetComponent<BattleUnit> ();
            if (null == target || target.IsDead || !_caster.IsTarget (target)) {
                return;
            }
            if (_skill.HitOnTarget (target)) {
                // 播放子弹命中特效
                GameObject effect = BattleResManager.Instance.CreateEffect (_config.effectOnHit);
                Utils.SetTransform (target.Trans, effect.transform, true);
                // TODO 处理命中后的ai列表

            }
            // TODO 默认命中后就销毁。穿透等类型子弹后面再扩展。
            Destroy (_go);
        }
    }
}
