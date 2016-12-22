using UnityEngine;
using System.Collections;
using System.Collections.Generic;

using ZR_MasterTools;


namespace Sgame.UI
{
    public class TopBar : MonoBehaviour
    {
        Transform _transform;
        GameObject _gameObject;
        public GameObject CacheGameObject { get { return _gameObject; } }
        // 资源
        UILabel _energyLabel;
        UILabel _goldLabel;
        UILabel _diamondLabel;
        // 返回按钮
        GameObject _goBackGO;
        UILabel _systemNameLabel;

        bool _isInitialShow = true;
        int _energy;
        int _gold;
        int _diamond;


        void Awake() {
            InitUI();
        }

        void InitUI() {
            _transform = transform;
            _gameObject = gameObject;
            for (int i = 0; i < _transform.childCount; i++) {
                Transform child = _transform.GetChild(i);
                switch (child.name) {
                    case "GoBack":
                        _goBackGO = child.gameObject;
                        EventDelegate.Set(child.GetComponent<UIButton>().onClick, OnClickGoBack);
                        _systemNameLabel = child.Find("SystemName").GetComponent<UILabel>();
                        break;
                    case "Diamond":
                        _diamondLabel = child.Find("Value").GetComponent<UILabel>();
                        EventDelegate.Set(child.Find("Add").GetComponent<UIButton>().onClick, () => { UITips.ShowTips(); });
                        break;
                    case "Gold":
                        _goldLabel = child.Find("Value").GetComponent<UILabel>();
                        EventDelegate.Set(child.Find("Add").GetComponent<UIButton>().onClick, () => { UITips.ShowTips(); });
                        break;
                    case "Energy":
                        _energyLabel = child.Find("Value").GetComponent<UILabel>();
                        EventDelegate.Set(child.Find("Add").GetComponent<UIButton>().onClick, () => { UITips.ShowTips(); });
                        break;
                }
            }
        }


        void Start() {
            SetPosition();
            if (null != PlayerMgr.Instance) {
                ShowResourceData(PlayerMgr.Instance.player);
            }
        }

        void SetPosition() {
            _transform.localPosition = new Vector3(0F, 325F, 0F);
        }

        void UpdateResourceData() {
            Player player = PlayerMgr.Instance.player;
            _energy = player.energy;
            _gold = player.goldNum;
            _diamond = player.diamondNum;
        }

        void ShowResourceData(Player player) {
            if (_isInitialShow || !_gameObject.activeSelf) {
                _energyLabel.text = string.Format("{0}/x", player.energy);
                _goldLabel.text = player.goldNum.ToString();
                _diamondLabel.text = player.diamondNum.ToString();
                UpdateResourceData();
                _isInitialShow = false;
                return;
            }
            // Show Tween
            ResetLabelState();
            if (player.energy != _energy) {
                StartCoroutine(RefreshResourceText(player.energy, _energy, _energyLabel, true));
            }
            if (player.goldNum != _gold) {
                StartCoroutine(RefreshResourceText(player.goldNum, _gold, _goldLabel, false));
            }
            if (player.diamondNum != _diamond) {
                StartCoroutine(RefreshResourceText(player.diamondNum, _diamond, _diamondLabel, false));
            }
            UpdateResourceData();
        }

        void ResetLabelState() {
            StopAllCoroutines();
            StopTween(_energyLabel);
            StopTween(_goldLabel);
            StopTween(_diamondLabel);
        }

        void StopTween(UILabel label) {
            TweenScale tween = label.GetComponent<TweenScale>();
            if (null != tween) {
                tween.enabled = false;
            }
            label.cachedTransform.localScale = Vector3.one;
        }

        readonly float _RefreshDuration = 1.0F;
        readonly int _MaxCount = 50;
        IEnumerator RefreshResourceText(int newValue, int oldValue, UILabel label, bool isEnergy) {
            int factor = newValue > oldValue ? 1 : -1;
            int count = Mathf.Min(Mathf.Abs(newValue - oldValue), _MaxCount);   // 控制最大次数，避免持续时间过长
            float duration = _RefreshDuration / count;
            factor *= (Mathf.Abs(newValue - oldValue) / count);
            for (int i = 0; i < count; i++) {
                if (i == count - 1) { // 防止精度丢失
                    oldValue = newValue;
                }
                else {
                    oldValue += factor;
                }
                if (isEnergy) {
                    label.text = string.Format("{0}/x", oldValue);
                }
                else {
                    label.text = oldValue.ToString();
                }
                yield return new WaitForSeconds(duration);
            }
            StartCoroutine(Utils.TweenGameObjectScale(label.gameObject, Vector3.one, 0.3F, 2.0F));
        }



        EventDelegate.Callback _goBackEvent;
        static Dictionary<WinID, string> _systemNameKeyDict= new Dictionary<WinID, string>{
            { WinID.UIPilot, "SystemName_Pilot" },
        };

        public void SetupGoBackInfo(EventDelegate.Callback goBackEvent = null) {
            if (!_systemNameKeyDict.ContainsKey(UIManager.Instance.curWinID)) {
                _goBackGO.SetActive(false);
                return;
            }
            _goBackGO.SetActive(true);
            _systemNameLabel.text = ConfigTableGameText.Instance.GetText(_systemNameKeyDict[UIManager.Instance.curWinID]);
            _goBackEvent = goBackEvent;
        }

        void OnClickGoBack() {
            if (null != _goBackEvent) {
                _goBackEvent();
            }
        }
    }
}