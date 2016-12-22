using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using System.Security.Cryptography;
using System.IO;


namespace ZR_MasterTools
{
    public delegate void VoidCallback();
    public delegate void Callback<T>(T arg);


    public class Utils
    {
        // 临时配置值（战斗伤害计算时使用）
        public const int LevelFactor = 4;
        public const int HitDelta = 60;
        public const int DamageDelta = 6;
        
        #region 3D部分
        /// 3D相机照到的宽度（修改参数时需要同步修改）
        public static float ScreenWidth3D = 42f;
        /// 3D相机照到的高度
        public static float ScreenHeight3D = 28f;
        /// <summary>
        /// 将世界坐标转换为屏幕坐标
        /// </summary>
        public static Vector3 ConvertPosFrom3DTo2D(Vector3 position, Camera cam3d, Camera cam2d) {
            Vector3 p = cam3d.WorldToScreenPoint(position);
            p = cam2d.ScreenToWorldPoint(p);
            p.z = 0;
            return p;
        }
        /// <summary>
        /// 将用户点击位置映射到3D战斗位置
        /// </summary>
        public static Vector3 PickMousePositionToWorldPosition(Camera cam, Vector3 touch, Transform t) {
            Vector3 a = new Vector3(1, 0, t.position.z);
            Vector3 b = new Vector3(0, 0, t.position.z);
            Vector3 c = new Vector3(0, 1, t.position.z);
            Plane plane = new Plane(a, b, c);
            return PickMousePositionToWorldPosition(cam, touch, plane);
        }

        // 拾取
        public static Vector3 PickMousePositionToWorldPosition(Camera cam, Vector3 mousePosition, Plane groundPlane) {
            Ray ray = cam.ScreenPointToRay(mousePosition);
            float rayDistance;
            Vector3 groundPosition = Vector3.zero;
            if (groundPlane.Raycast(ray, out rayDistance)) {
                groundPosition = ray.GetPoint(rayDistance);
            }
            return groundPosition;
        }

        // converts a screen-space position to a world-space position constrained to the current drag plane type
        // returns false if it was unable to get a valid world-space position
        // 将屏幕坐标（手指点击位置）转变为世界坐标（相对于相机的平面，XY轴）
        public static bool ProjectScreenPointToCamPlane(Camera cam, Vector3 refPos, Vector2 screenPos, out Vector3 worldPos) {
            worldPos = refPos;
            Transform tran = cam.transform;
            // create a plane passing through refPos and facing toward the camera
            // 创建投影平面（垂直于Z轴，通过物体的一个面）
            Plane plane = new Plane(-tran.forward, refPos);
            Ray ray = cam.ScreenPointToRay(screenPos);
            // 投影
            float t = 0;
            if (!plane.Raycast(ray, out t)) {
                return false;
            }
            worldPos = ray.GetPoint(t);
            return true;
        }

        public static string ConvertVectorToString(Vector3 v) {
            return string.Format("{0},{1},{2}", v.x, v.y, v.z);
        }

        public static Vector3 ConvertStringToVector(string s) {
            string[] items = s.Split(',');
            return new Vector3(float.Parse(items[0]), float.Parse(items[1]), float.Parse(items[2]));
        }

        // Transform => "$localPosition|$localEulerAngles|$localScale"
        public static string ConvertTransformToString(Transform t) {
            return string.Format(
                "{0}|{1}|{2}",
                ConvertVectorToString(t.localPosition),
                ConvertVectorToString(t.localEulerAngles),
                ConvertVectorToString(t.localScale)
            );
        }

        // s: "$localPosition;$localEulerAngles;$localScale"
        public static void ConvertStringToTransform(string s, Transform t) {
            string[] items = s.Split('|');
            t.localPosition = ConvertStringToVector(items[0]);
            t.localEulerAngles = ConvertStringToVector(items[1]);
            t.localScale = ConvertStringToVector(items[2]);
        }
        public static void ConvertStringToTransformMonster(string s, Transform t) {
            string[] items = s.Split(';');
            t.localPosition = ConvertStringToVector(items[1]);
            t.localEulerAngles = ConvertStringToVector(items[2]);
            t.localScale = ConvertStringToVector(items[3]);
        }
        public static void ConvertStringToTransformMonster(Vector3 p, float s, Transform t) {
            t.localPosition = p;
            t.localEulerAngles = new Vector3(0, 90, 0);
            t.localScale = Vector3.one * s;
        }

        public static float GetPosXFromTransInfo(string s) {
            string[] items = s.Split('|');
            return ConvertStringToVector(items[0]).x;
        }

        static bool IsOnViewport(Vector3 p) {
            return (p.x > 0 && p.x < 1 && p.y > 0 && p.y < 1);
        }

        /// 判断3D物体是否在屏幕内，如果不在屏幕内，则返回屏幕边界的法向量，以用于计算反射角
        public static bool IsOnScreen(Camera cam, Vector3 p, float r, out Vector3 normal) {
            normal = Vector3.zero;
            // up
            Vector3 vp = cam.WorldToViewportPoint(p + Vector3.up * r);
            if (!IsOnViewport(vp)) {
                normal = Vector3.down;
                return false;
            }
            // down
            vp = cam.WorldToViewportPoint(p + Vector3.down * r);
            if (!IsOnViewport(vp)) {
                normal = Vector3.up;
                return false;
            }
            // right
            vp = cam.WorldToViewportPoint(p + Vector3.right * r);
            if (!IsOnViewport(vp)) {
                normal = Vector3.left;
                return false;
            }
            // left
            vp = cam.WorldToViewportPoint(p + Vector3.left * r);
            if (!IsOnViewport(vp)) {
                normal = Vector3.right;
                return false;
            }
            return true;
        }

        /// 判断3D物体是否在屏幕内
        public static bool IsOnScreen(Camera cam, Vector3 p, float r) {
            Vector3 normal;
            return IsOnScreen(cam, p, r, out normal);
        }
        #endregion

        #region 系统相关
        /// 判断Application.persistentDataPath是否存在
        public static bool IsPersistentDataPathExists() {
            string path = Application.persistentDataPath;
            return !string.IsNullOrEmpty(path) && Directory.Exists(path);
        }

        static string _deviceID = string.Empty;
        /// 设备号
        public static string DeviceID {
            get {
                if (string.IsNullOrEmpty(_deviceID)) {
                    // 使用unity的唯一id
                    _deviceID = SystemInfo.deviceUniqueIdentifier;
                }
                return _deviceID;
            }
        }

        static string _deviceIDFA = string.Empty;
        /// 苹果设备广告id (如果为空则取设备号)
        public static string DeviceIDFA {
            get {
                if (string.IsNullOrEmpty(_deviceIDFA)) {
#if UNITY_IOS
                if (Application.platform == RuntimePlatform.IPhonePlayer) {
                    _deviceIDFA = iPhone.advertisingIdentifier;
                }
                if (string.IsNullOrEmpty (_deviceIDFA)) {
                    _deviceIDFA = DeviceID;
                }
#else
                    _deviceIDFA = DeviceID;
#endif
                }
                return _deviceIDFA;
            }
        }
        #endregion

        #region 节点、物体、预设操作
        /// 删除指定父节点的所有子节点
        public static void RemoveAllChildNode(Transform parent) {
            if (null == parent || parent.childCount <= 0) {
                return;
            }
            List<GameObject> list = new List<GameObject>();
            foreach (Transform child in parent) {
                list.Add(child.gameObject);
            }
            for (int i = 0; i < list.Count; i++) {
                NGUITools.Destroy(list[i]);
            }
            list.Clear();
        }

        /// 删除指定父节点的部分指定子节点
        public static void RemoveSomeChildNode(Transform parent, int startIndex, int endIndex) {
            if (null == parent || parent.childCount <= 0) {
                return;
            }
            List<GameObject> list = new List<GameObject>();
            foreach (Transform child in parent) {
                list.Add(child.gameObject);
            }
            for (int i = endIndex; i >= startIndex; i--) {
                NGUITools.Destroy(list[i]);
            }
            list.Clear();
        }

        /// 加载UI Item预设
        public static GameObject LoadUIItemPrefab(string itemName) {
            if (string.IsNullOrEmpty(itemName)) {
                return null;
            }
            GameObject go = Resources.Load<GameObject>(string.Format("UI/Item/{0}", itemName));
            if (null == go) {
                Debug.LogError(string.Format("Resource load fail, name : {0}", itemName));
                return null;
            }
            return go;
        }

        // 改变对象Layer层级
        public static void ChangeGoLayerRecursively(Transform t, int layer) {
            t.gameObject.layer = layer;
            for (int i = 0, imax = t.childCount; i < imax; i++) {
                ChangeGoLayerRecursively(t.GetChild(i), layer);
            }
        }

        public static void ChangeGoLayerRecursively(Transform t, string layerName) {
            ChangeGoLayerRecursively(t, LayerMask.NameToLayer(layerName));
        }

        public static GameObject LoadModel(string modelPath, GameObject parent) {
            GameObject prefab = BundleMgr.Instance.LoadResource<GameObject>(modelPath);
            if (null == prefab) {
                Debug.LogWarning("模型预设不存在：" + modelPath);
                return null;
            }
            GameObject model = NGUITools.AddChild(parent, prefab);
            if (null == model) {
                Debug.LogWarning("加载模型失败：" + modelPath);
                return null;
            }
            ParticleSystem[] pss = model.GetComponentsInChildren<ParticleSystem>();
            float scaleFactor = model.transform.lossyScale.x;
            foreach (ParticleSystem ps in pss) {
                ps.startSize *= scaleFactor;
                ps.startSpeed *= scaleFactor;
                ps.gravityModifier *= scaleFactor;
                ps.Clear();
            }
            TrailRenderer[] trails = model.GetComponentsInChildren<TrailRenderer>();
            foreach (TrailRenderer trail in trails) {
                trail.startWidth *= scaleFactor;
                trail.endWidth *= scaleFactor;
            }
            return model;
        }
        #endregion

        #region 配置相关
        /// 读取整个配置文件
        public static string ReadConfigFile(string path) {
#if UNITY_STANDALONE_WIN
        string fileName = GlobalConfig.TestConfigPath + path + ".txt";
        string text = File.ReadAllText (fileName);
#else
            TextAsset asset = BundleMgr.Load<TextAsset>("Configs/" + path);
            if (null == asset) {
                return string.Empty;
            }
            string text = asset.text;
#endif
            return text;
        }

        /// 解析配置表行内容，格式："name=value"
        public static string[] ParseConfigRow(string row) {
            if (string.IsNullOrEmpty(row.Trim()) || '#' == row[0]) {
                return null;
            }
            string[] items = row.Trim().Split(new char[] { '=' });
            if (items.Length != 2) {
                Debug.LogWarning("错误的配置：" + row);
                return null;
            }
            return items;
        }
        #endregion

        #region 数据相关
        /// 对字符串做MD5加密
        public static string MD5String(string s) {
            byte[] data = System.Text.ASCIIEncoding.ASCII.GetBytes(s);
            MD5 md5 = new MD5CryptoServiceProvider();
            return BitConverter.ToString(md5.ComputeHash(data)).Replace("-", string.Empty);
        }
        #endregion

        #region 表现相关
        // 物体缩放动画
        public static IEnumerator TweenGameObjectScale(GameObject go, Vector3 realScale, float duration = 0.3F, float multiple = 1.5F) {
            if (null == go) {
                yield break;
            }
            go.transform.localScale = realScale;
            Vector3 scale = realScale * multiple;
            TweenScale.Begin(go, duration, scale).method = UITweener.Method.EaseIn;
            yield return new WaitForSeconds(duration);
            if (null == go) {
                yield break;
            }
            TweenScale.Begin(go, duration, realScale).method = UITweener.Method.EaseInOut;
            yield return new WaitForSeconds(duration);
        }

        // 根据文本字数设置间隔
        public static void SetLableContentAndSpacing(UILabel label, string content, bool isBold = false, int baseSpacing = 5) {
            if (content.Length <= 2) {
                label.spacingX = baseSpacing * 4;
            }
            else if (content.Length <= 4) {
                label.spacingX = baseSpacing * 2;
            }
            else {
                label.spacingX = baseSpacing;
            }
            label.text = isBold ? BBcodeHelper.BoldText(content) : content;
        }

        /// 设置特效对象的sortingOrder值，使其展示在本Panel的UI上
        /// 注：在创建特效对象时UIEffectManager.Instance.CreateEffect的第三个参数需为false；
        /// 在该Panel之后打开的窗口需要调用UIWin的SetBaseRenderQueueSortOrder函数设置sortingOrder，使新窗口显示在该特效上
        public static IEnumerator SetEffectRenderQueueSortOrder(GameObject effectGO) {
            yield return null;   // 至少等一帧，有些特效需要等一帧才能正常显示
            UIPanel parentPanel = effectGO.GetComponentInParent<UIPanel>();
            while (0 == parentPanel.drawCalls.Count) {
                yield return null;
            }
            int referRenderQueue = parentPanel.sortingOrder;
            foreach (UIDrawCall dc in parentPanel.drawCalls) {
                referRenderQueue = Mathf.Max(referRenderQueue, dc.renderQueue);
            }
            Renderer[] renders = effectGO.GetComponentsInChildren<Renderer>();
            foreach (Renderer r in renders) {
                r.sortingOrder = referRenderQueue + 1;
            }
        }

        /// 数字列表转化成字符串 [1,2,11] => "1;2;11"
        public static string NumberListToString(string sep, List<int> numberList) {
            return NumberArrayToString(sep, numberList.ToArray());
        }

        /// 数字数组转化成字符串 [1,2,11] => "1;2;11"
        public static string NumberArrayToString(string sep, int[] numberList) {
            if (null == numberList || numberList.Length <= 0) {
                return string.Empty;
            }
            string[] strList = new string[numberList.Length];
            for (int i = 0; i < numberList.Length; i++) {
                strList[i] = numberList[i].ToString();
            }
            return string.Join(sep, strList);
        }

        /// 字符串转化数字列表 "1;2;3" => [1,2,3]
        public static List<int> StringToNumberList(char sep, string str) {
            List<int> numberList = new List<int>();
            if (string.IsNullOrEmpty(str)) {
                return numberList;
            }
            string[] items = str.Split(sep);
            for (int i = 0; i < items.Length; i++) {
                numberList.Add(int.Parse(items[i]));
            }
            return numberList;
        }

        #endregion

        // 设置t的位置信息
        public static void SetTransform (Transform parent, Transform t, bool setParent) {
            if (parent == null) {
                return;
            }
            if (!setParent) {
                t.position = parent.position;
                t.rotation = parent.rotation;
                t.localScale = parent.localScale;
            }
            else {
                t.parent = parent;
                t.localPosition = Vector3.zero;
                t.localRotation = Quaternion.identity;
                t.localScale = Vector3.one;
                if (t.gameObject.layer != parent.gameObject.layer) {
                    Utils.ChangeGoLayerRecursively (t, parent.gameObject.layer);
                }
            }
        }

        public static void DestroyGo (GameObject go) {
            if (null != go) {
                GameObject.Destroy (go);
                go = null;
            }
        }
    }
}