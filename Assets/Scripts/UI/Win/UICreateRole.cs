using UnityEngine;
using System.Collections;

using SLMS;
using ZR_MasterTools;


namespace Sgame.UI
{
    public class UICreateRole : UIWin
    {


        UIInput _nameInput;
        UIButton _createButton;

        UILabel _npcWordLabel;
        TweenAlpha _npcWordTween;


        protected override void OnInit() {
            base.OnInit();
            InitUI();
        }

        void InitUI() {
            for (int i = 0; i < _transform.childCount; i++) {
                Transform child = _transform.GetChild(i);
                switch (child.name) {
                    case "Create":
                        _createButton = child.GetComponent<UIButton>();
                        EventDelegate.Set(_createButton.onClick, CreateRole);
                        break;
                    case "Name":
                        _nameInput = child.GetComponent<UIInput>();
                        EventDelegate.Set(_nameInput.onSubmit, CheckInputLength);
                        EventDelegate.Set(child.Find("Random").GetComponent<UIButton>().onClick, RandonName);
                        break;
                    case "Npc":
                        _npcWordLabel = child.Find("Word").GetComponent<UILabel>();
                        _npcWordTween = child.Find("Word").GetComponent<TweenAlpha>();
                        break;
                }
            }

        }

        protected override void OnMsgInit() {
            base.OnMsgInit();
            TriggerEvent<ResultCode>.AddListener(TriggerEventType.CreateRole_Result, OnCreateRoleResult);
            TriggerEvent<ResultCode>.AddListener(TriggerEventType.EnterGame_Result, OnEnterGameResult);
        }
        protected override void OnMsgRemove() {
            base.OnMsgRemove();
            TriggerEvent<ResultCode>.RemoveListener(TriggerEventType.CreateRole_Result, OnCreateRoleResult);
            TriggerEvent<ResultCode>.RemoveListener(TriggerEventType.EnterGame_Result, OnEnterGameResult);
        }

        protected override void OnOpen(WinParamBase paramObject) {
            base.OnOpen(paramObject);
            _npcWordLabel.cachedGameObject.SetActive(false);
            _nameInput.value = SDKProxy.Instance.SDK.UserName;
        }


        void CreateRole() {
            if (_nameInput.value.Length < GlobalConfig.MinNameLength) {
                UITips.ShowTips(string.Format(ConfigTableGameText.Instance.GetText("PlayerNameMinLength"), GlobalConfig.MinNameLength));
                return;
            }
            if (IsInputLengthOutOfLimit()) {
                return;
            }
            _createButton.isEnabled = false;
            AuthMgr.Instance.CreateRole(true, _nameInput.value);
        }

        void OnCreateRoleResult(ResultCode result) {
            switch (result) {
                case ResultCode.Success:
                    return;
                case ResultCode.ERR_RoleNameExist:
                case ResultCode.ERR_RoleNameNotLegal:
                    ShowNpcWord(NetworkHelper.GetErrorInfo(result));
                    break;
                default:
                    UIManager.Instance.OpenWin(WinID.UIOK, new WinParam_Text(NetworkHelper.GetErrorInfo(result)));
                    break;
            }
            _createButton.isEnabled = true;
        }

        void OnEnterGameResult(ResultCode result) {
            if (ResultCode.Success != result) {
                UIManager.Instance.OpenWin(WinID.UIOK, new WinParam_Text(NetworkHelper.GetErrorInfo(result)));
                return;
            }
            SceneMgr.Instance.GoToMainScene();
        }


        void ShowNpcWord(string word) {
            _npcWordLabel.cachedGameObject.SetActive(true);
            _npcWordLabel.text = word;
            _npcWordTween.ResetToBeginning();
            _npcWordTween.PlayForward();
        }

        void CheckInputLength() {
            Debug.Log("CheckInputLength");
            IsInputLengthOutOfLimit();
        }

        bool IsInputLengthOutOfLimit() {
            if (_nameInput.value.Length <= GlobalConfig.MaxNameLength) {
                return false;
            }
            _nameInput.value = _nameInput.value.Substring(0, GlobalConfig.MaxNameLength);
            UITips.ShowTips(string.Format(ConfigTableGameText.Instance.GetText("PlayerNameMaxLength"), GlobalConfig.MaxNameLength));
            return true;
        }

        void RandonName() {
            _nameInput.value = RandomUserName.GetName(!SDKProxy.Instance.SDK.IsFemale);
        }
    }
}