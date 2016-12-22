using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace SMaker
{
    public class SMakerManager : MonoBehaviour
    {

        // Use this for initialization

        public System.Action OnUpdate;
        public System.Action OnQuit;

        public delegate Transform SelectionGo();
        public SelectionGo selectionTrans;

        public delegate void SetSelection(Transform tran);
        public SetSelection setSelection;


        public float _time = 0;
        public float _elapsedTime;
        public float _elapsedTimeMax = 1;

        public GameObject _prefabObj;
        public GameObject _originalObj;
        public GameObject _instanceObj;

        public Transform _showGroup;

        public SMakerMoveType sMakerMoveType = SMakerMoveType.None;

        public Dictionary<string, float> specialParameters = new Dictionary<string, float>();

        public bool isSelectViewOnly
        {
            get
            {
                return _isSelectViewOnly;
            }
            set
            {

                _isSelectViewOnly = value;
                if (_isSelectViewOnlyCache != _isSelectViewOnly)
                {
                    CreatEffectInstance();
                    _isSelectViewOnlyCache = _isSelectViewOnly;
                }

            }
        }
        bool _isSelectViewOnly = false;
        bool _isSelectViewOnlyCache = false;
        // Update is called once per frame

        void Start()
        {
            _showGroup = new GameObject("showGroup").transform;
            _showGroup.hideFlags = HideFlags.HideInHierarchy;
            InitSpecialParameters();
        }



        void InitSpecialParameters()
        {
            specialParameters.Add("radius", 1);
            specialParameters.Add("speed", 1);
        }
        void Update()
        {

            if (OnUpdate != null)
                OnUpdate();

            if (!_prefabObj || !_instanceObj)
            {
                return;
            }

            if (_elapsedTime >= _elapsedTimeMax)
            {
                CreatEffectInstance();
            }

            _elapsedTime += Time.deltaTime;
        }

        void LateUpdate()
        {
            if (selectItemInInstance != null)
            {
                selectItemInInstance.localPosition = selectItemInOriginal.localPosition;
                selectItemInInstance.localRotation = selectItemInOriginal.localRotation;
                selectItemInInstance.localScale = selectItemInOriginal.localScale;
            }

            if (_instanceObj != null)
            {
                _instanceObj.transform.localPosition = GetSMakerMove();
            }
        }

        void OnApplicationQuit()
        {
            if (OnQuit != null)
                OnQuit();
        }


        public List<int> GetSelectPath(Transform target, Transform judge)
        {
            List<int> pathIndexes = new List<int>();
            while (target != judge && target != null)
            {
                Transform cur = target;
                target = target.parent;
                for (int i = 0; i < target.childCount; i++)
                {
                    if (target.GetChild(i) == cur)
                    {
                        pathIndexes.Insert(0, i);
                        break;
                    }
                }
            }
            return pathIndexes;

        }

        public Transform FindBySelectPath(Transform root, List<int> pathIndexes)
        {
            for (int i = 0; i < pathIndexes.Count; i++)
            {
                root = root.GetChild(pathIndexes[i]);
            }
            return root;
        }

        List<int> _selectPath = new List<int>();
        public void CreatEffectInstance()
        {
            bool isSelectItemInInstance = selectItemInInstance != null;
            if (_instanceObj)
            {
                GameObject.Destroy(_instanceObj);
            }
            _elapsedTime = 0;
            _instanceObj = GameObject.Instantiate(_originalObj);
            _instanceObj.name = _originalObj.name;
            _instanceObj.transform.SetParent(_showGroup);
            _instanceObj.SetActive(true);
            if (isSelectItemInInstance)
            {
                selectItemInInstance = FindBySelectPath(_instanceObj.transform, _selectPath);
            }
            SetViewOnly();
        }

        Transform selectItemInOriginal;
        Transform selectItemInInstance;
        bool isSafeChange = false;
        public void OnSelectInstance()
        {

            if (selectionTrans() != null)
            {
                if (selectionTrans().root == _showGroup || selectionTrans().root == _originalObj.transform)
                {
                    _selectPath = GetSelectPath(selectionTrans(), selectionTrans().root == _showGroup ? _instanceObj.transform : _originalObj.transform);
                    SetViewOnly();
                }
            }

            if (selectionTrans() == null || selectionTrans().root != _showGroup)
            {
                if (isSafeChange == true) { isSafeChange = false; return; }
                selectItemInInstance = null;
                selectItemInOriginal = null;
            }
            else
            {
                selectItemInOriginal = FindBySelectPath(_originalObj.transform, _selectPath);
                selectItemInInstance = FindBySelectPath(_instanceObj.transform, _selectPath);
                StartCoroutine(SetSelectionTransform(selectItemInOriginal));
                isSafeChange = true;

            }
        }

        public void SetViewOnly()
        {
            if (isSelectViewOnly)
            {
                SetObjView(_instanceObj.transform, true);
                Transform cacheChild = null;
                for (int i = 0; i < _selectPath.Count; i++)
                {
                    Transform root = i == 0 ? _instanceObj.transform : cacheChild;
                    cacheChild = root.GetChild(_selectPath[i]);
                    foreach (Transform child in root)
                    {
                        child.gameObject.SetActive(child == cacheChild);
                    }
                }
            }
        }

        void SetObjView(Transform root, bool isView)
        {
            foreach (Transform item in root)
            {
                SetObjView(item, isView);
            }
            root.gameObject.SetActive(isView);
        }

        public void CleanFlow()
        {
            selectItemInOriginal = null;
            selectItemInInstance = null;
        }

        IEnumerator SetSelectionTransform(Transform trans)
        {
            yield return new WaitForEndOfFrame();
            if (trans)
                setSelection(trans);
        }

        public Vector3 GetSMakerMove()
        {
            switch (sMakerMoveType)
            {
                case SMakerMoveType.None:
                    return Vector3.zero;
                case SMakerMoveType.MoveZ:
                    return new Vector3(0, 0, specialParameters["speed"] * _elapsedTime);
                case SMakerMoveType.Circle:
                    float radius = specialParameters["radius"];
                    return new Vector3(Mathf.Sin(specialParameters["speed"] * _elapsedTime), 0, Mathf.Cos(specialParameters["speed"] * _elapsedTime)) * radius;
            }
            return Vector3.zero;
        }

    }

    public enum SMakerMoveType
    {
        None, MoveZ, Circle, Max
    }
}

