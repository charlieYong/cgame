using UnityEngine;
using System.Collections;
using UnityEditor;

namespace SMaker
{
    public class SMakerManagerWin : EditorWindow
    {

        [MenuItem("GameObject/MOMO %Q",false)]
        public static void test_uv()
        {
            if (Selection.activeTransform)
            {
                Transform trans = new GameObject("new GameObject").transform;
                if (Selection.activeTransform.parent)
                {
                    trans.parent = Selection.activeTransform.parent;
                }
                Selection.activeTransform.parent = trans;
                Selection.activeTransform = trans;
            }
        }



        [MenuItem("SMaker/SMakerWindows")]
        static void Init()
        {
            sMakerManagerWin = SMakerManagerWin.CreateInstance<SMakerManagerWin>();
            sMakerManagerWin.Show();
        }

        static SMakerManagerWin sMakerManagerWin;


        public void OnInit()
        {
            isInit = true;
            _sm = new GameObject("SMaker").AddComponent<SMakerManager>();
            Selection.selectionChanged =_sm.OnSelectInstance;
            _sm.setSelection = SetSelection;
            _sm.selectionTrans = () => { return Selection.activeTransform; };
            _sm.OnUpdate = OnUpdate;
            _sm.OnQuit = OnQuit;

        }

        void SetSelection(Transform trans)
        {
            Selection.activeTransform = trans;
        }

        void OnUpdate()
        {
            Repaint();
        }
        void OnQuit()
        {
            isInit = false;
            Selection.selectionChanged = null;
            SavePrefab();

        }

        public void SavePrefab()
        {
            if (_sm._prefabObj == null) return;
            _sm._originalObj.SetActive(true);
            PrefabUtility.ReplacePrefab(_sm._originalObj, _sm._prefabObj, ReplacePrefabOptions.ConnectToPrefab);
            AssetDatabase.SaveAssets();
        }

        SMakerManager _sm;

        bool isInit = false;

        void OnGUI()
        {
            if (!Application.isPlaying)
            {
                GUILayout.Label("请运行游戏");
                return;
            }

            if (!isInit)
            {
                OnInit();
            }


            if (Selection.activeGameObject == null || PrefabUtility.GetPrefabType(Selection.activeGameObject) != PrefabType.Prefab || Selection.activeGameObject != PrefabUtility.FindPrefabRoot(Selection.activeGameObject))
            {
                    GUILayout.Label("请选择预设");
            }
            else
            {
                if(GUILayout.Button("加载预设", GUILayout.Width(80f))){

                _sm.CleanFlow();
                SavePrefab();
                _sm._prefabObj = Selection.activeGameObject;
                CreatEffectOriginal();
                _sm.CreatEffectInstance();
                }
            }

            if(_sm._prefabObj == null){
                    return;
            }


            if (_sm._prefabObj == null)
            {
                return;
            }
            GUILayout.Space(30);
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("⊙", GUILayout.Width(20f)))
            {
                _sm.CreatEffectInstance();
            }
            GUILayout.HorizontalSlider(_sm._elapsedTime, 0, _sm._elapsedTimeMax);
            GUILayout.Label(_sm._elapsedTime.ToString("F2"));
            GUILayout.EndHorizontal();
            _sm._elapsedTimeMax = EditorGUILayout.FloatField("重置时间", _sm._elapsedTimeMax);
            GUILayout.Space(10);
            _sm.isSelectViewOnly = EditorGUILayout.Toggle("选中可视:", _sm.isSelectViewOnly);
            GUILayout.Space(10);
            GUILayout.BeginHorizontal();

            for (int i = 0; i < (int)SMakerMoveType.Max; i++)
            {
                SMakerMoveType s = (SMakerMoveType)i;
                string content = _sm.sMakerMoveType == s ? "[" + s.ToString() + "]" : s.ToString();
                if (GUILayout.Button(content,GUILayout.Width(80))) 
                {
                    _sm.sMakerMoveType = s;
                }
            }
            GUILayout.EndHorizontal();
            SetMoveTypeGroup(_sm.sMakerMoveType);



            GUILayout.Space(30);
            if (GUILayout.Button("重设", GUILayout.Width(40f)))
            {
                _sm.CleanFlow();
                CreatEffectOriginal();
                _sm.CreatEffectInstance();

            }
        }
        public void CreatEffectOriginal()
        {
            if (_sm._originalObj)
            {
                GameObject.Destroy(_sm._originalObj);
            }
            _sm._originalObj = (GameObject)PrefabUtility.InstantiatePrefab(_sm._prefabObj);
            _sm._originalObj.SetActive(false);

        }

        void SetMoveTypeGroup(SMakerMoveType s)
        {
            switch (s)
            {
                case SMakerMoveType.None:
                case SMakerMoveType.Max:
                    break;
                case SMakerMoveType.MoveZ:
                    _sm.specialParameters["speed"] = EditorGUILayout.FloatField("速度:", _sm.specialParameters["speed"]);
                    break;
                case SMakerMoveType.Circle:
                    _sm.specialParameters["speed"] = EditorGUILayout.FloatField("速度:", _sm.specialParameters["speed"]);
                    _sm.specialParameters["radius"] = EditorGUILayout.FloatField("半径:", _sm.specialParameters["radius"]);
                    break;

            }
        }
    }
}
