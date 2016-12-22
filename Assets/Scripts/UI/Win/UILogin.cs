using UnityEngine;
using System.Collections;

using SLMS;
using ZR_MasterTools;

namespace Sgame.UI
{
    public class UILogin : UIWin
    {

        enum LoginSliderStatus
        {
            Default = 0,
            UpdateResources,
            LoadConfig,
            Ok,
        }

        UIGrid _loginButtonGrid;
        UIButton _weixinLoginButton;
        UIButton _qqLoginButton;
        UIButton _touristLoginButton;
        UIButton _enterGameButton;
        UITexture _backgroundTexture;
        GameObject _enterGameTipsGO;
        GameObject _noticeGO;

        UISlider _processSlider;
        UILabel _optionTipsLabel;
        UILabel _processTipsLabel;

        LoginSliderStatus _sliderStatus = LoginSliderStatus.Default;


        protected override void OnInit() {
            base.OnInit();
            InitUI();
        }

        void InitUI() {
            for (int i = 0; i < _transform.childCount; i++) {
                Transform child = _transform.GetChild(i);
                switch (child.name) {
                    case "LoginButtons":
                        _loginButtonGrid = child.GetComponent<UIGrid>();
                        _touristLoginButton = child.Find("TouristLogin").GetComponent<UIButton>();
                        _weixinLoginButton = child.Find("WeiXinLogin").GetComponent<UIButton>();
                        _qqLoginButton = child.Find("QQLogin").GetComponent<UIButton>();
                        EventDelegate.Set(_touristLoginButton.onClick, OnLogin);
                        break;
                    case "BG":
                        _backgroundTexture = child.GetComponent<UITexture>();
                        _enterGameButton = child.GetComponent<UIButton>();
                        EventDelegate.Set(_enterGameButton.onClick, EnterGame);
                        break;
                    case "EnterGameTips":
                        _enterGameTipsGO = child.gameObject;
                        break;
                    case "Notice":
                        _noticeGO = child.gameObject;
                        EventDelegate.Set(child.GetComponent<UIButton>().onClick, OnOpenNotice);
                        break;
                    case "OptionProcess":
                        _processSlider = child.GetComponent<UISlider>();
                        _optionTipsLabel = child.Find("OptionTips").GetComponent<UILabel>();
                        _processTipsLabel = child.Find("ProcessTips").GetComponent<UILabel>();
                        break;
                }
            }
        }

        protected override void OnMsgInit() {
            base.OnMsgInit();
            // Login
            TriggerEvent<bool>.AddListener(TriggerEventType.LoginSDK_Result, OnLoginSDKResult);
            TriggerEvent<int>.AddListener(TriggerEventType.CheckLogin_Result, OnCheckLoginResult);
            // EnterGame
            TriggerEvent<ResultCode>.AddListener(TriggerEventType.LoginGame_Result, OnLoginToGameResult);
            TriggerEvent<ResultCode>.AddListener(TriggerEventType.EnterGame_Result, OnEnterGameResult);
        }
        protected override void OnMsgRemove() {
            base.OnMsgRemove();
            TriggerEvent<bool>.RemoveListener(TriggerEventType.LoginSDK_Result, OnLoginSDKResult);
            TriggerEvent<int>.RemoveListener(TriggerEventType.CheckLogin_Result, OnCheckLoginResult);
            TriggerEvent<ResultCode>.RemoveListener(TriggerEventType.LoginGame_Result, OnLoginToGameResult);
            TriggerEvent<ResultCode>.RemoveListener(TriggerEventType.EnterGame_Result, OnEnterGameResult);
        }

        protected override void OnOpen(WinParamBase paramObject) {
            base.OnOpen(paramObject);
            _backgroundTexture.mainTexture = BundleMgr.Instance.LoadResource<Texture>(GlobalConfig.LoginTexturePath + "08018");
            EnableProcessSlider(false, LoginSliderStatus.Default);
            EnableEnterGameButton(false);
            EnableLoginButton(false);
            StartCoroutine(CheckResources());
        }


        void EnableLoginButton(bool isEnable) {
            EnableLoginButton(_touristLoginButton, isEnable);
            EnableLoginButton(_weixinLoginButton, isEnable);
            EnableLoginButton(_qqLoginButton, isEnable);
        }

        void EnableLoginButton(UIButton loginButton, bool isEnable) {
            if (isEnable && !loginButton.gameObject.activeSelf) {
                loginButton.gameObject.SetActive(true);
            }
            loginButton.isEnabled = isEnable;
        }

        void EnableEnterGameButton(bool isEnable) {
            _enterGameButton.isEnabled = isEnable;
            _enterGameTipsGO.SetActive(isEnable);
        }


        /// 检查是否需要更新资源
        IEnumerator CheckResources() {
            SceneMgr.Instance.ShowConnecting(true);
            yield return StartCoroutine(BundleMgr.Instance.WaitForFetchPatchList());
            SceneMgr.Instance.ShowConnecting(false);
            if (!BundleMgr.Instance.IsNeedDownloadPatch()) {
                StartCoroutine(GetReadyToLogin());
                yield break;
            }
            string tips = string.Format(ConfigTableGameText.Instance.GetText("ConfirmUpdate"), BundleMgr.Instance.GetTotalSizeText());
            UIConfirmOption win = (UIConfirmOption)UIManager.Instance.OpenWin(
                WinID.UIConfirmOption, new WinParam_ConfirmOption(tips, OnUpdateComfirm)
            );
            win.DisableMaskClick();
        }

        /// 在这里去做配置文件的异步提前加载
        IEnumerator GetReadyToLogin() {
            SceneMgr.Instance.ShowConnecting(false);
            EnableProcessSlider(true, LoginSliderStatus.LoadConfig);
            yield return StartCoroutine(AppManager.Instance.LoadConfigFiles());
            EnableProcessSlider(false, LoginSliderStatus.Ok);
            EnableLoginButton(true);
        }

        void EnableProcessSlider(bool enable, LoginSliderStatus status) {
            _sliderStatus = status;
            _processSlider.gameObject.SetActive(enable);
            _noticeGO.SetActive(!enable);
        }

        void OnUpdateComfirm(bool yes) {
            if (!yes) {
                Application.Quit();
                return;
            }
            StartCoroutine(UpdateBundleResources());
        }

        // 等待资源更新资源完成
        IEnumerator UpdateBundleResources() {
            EnableProcessSlider(true, LoginSliderStatus.UpdateResources);
            yield return StartCoroutine(BundleMgr.Instance.UpdateResources());
            EnableProcessSlider(false, LoginSliderStatus.Ok);
            yield return null;
            StartCoroutine(GetReadyToLogin());
        }

        void Update() {
            switch (_sliderStatus) {
                case LoginSliderStatus.UpdateResources:
                    ShowUpdateProgress();
                    break;
                case LoginSliderStatus.LoadConfig:
                    ShowLoadingConfigProgress();
                    break;
            }
        }

        void ShowLoadingConfigProgress() {
            float process = AppManager.Instance.CurLoadedFileNum / (float)AppManager.Instance.TotalPreloadFileNum;
            _processSlider.value = process;
            _optionTipsLabel.text = string.Format(ConfigTableGameText.Instance.GetText("LoadingConfigTips"), (process * 100));
            _processTipsLabel.text = string.Empty;
        }

        void ShowUpdateProgress() {
            _processSlider.value = BundleMgr.Instance.CurUpdateProgress;
            _optionTipsLabel.text = string.Format(ConfigTableGameText.Instance.GetText("UpdatingVersion"), VersionConfig.Instance.CurVersion);
            _processTipsLabel.text = BundleMgr.Instance.GetUpdateProcessText();
        }


        void OnOpenNotice() {
            UITips.ShowTips(ConfigTableGameText.Instance.GetText("NoNotice"));

            //UIManager.Instance.EnableInteractive(false);
            //SceneMgr.Instance.ShowConnecting(true);
            //MyWWW www = new MyWWW(ClientLocalConfig.Instance.LoginNoticePath, 10F);
            //StartCoroutine(www.WaitForFinish(OnLoginNoticeDownloaded));
        }


        void OnLoginNoticeDownloaded(MyWWW www, bool isTimeOut) {
            UIManager.Instance.EnableInteractive(true);
            SceneMgr.Instance.ShowConnecting(false);
            if (isTimeOut || !string.IsNullOrEmpty(www.CurWWW.error)) {
                UITips.ShowTips(ConfigTableGameText.Instance.GetText("DownloadNoticeTimeout"));
                return;
            }
            string content = System.Text.Encoding.UTF8.GetString(www.CurWWW.bytes).Trim();
            // 内容为空，则不展示
            if (string.IsNullOrEmpty(content)) {
                UITips.ShowTips(ConfigTableGameText.Instance.GetText("NoNotice"));
                return;
            }
            //Manager.Instance.OpenWin(WinID.UILoginNotice, null, content);
        }



        void OnLogin() {
            AuthMgr.Instance.LoginToSDK();
        }

        void OnLoginSDKResult(bool result) {
            if (!result) {
                EnableLoginButton(true);
            }
        }

        void OnCheckLoginResult(int result) {
            EnableLoginButton(true);
            if (0 != result) {
                UIManager.Instance.OpenWin(WinID.UIOK, new WinParam_Text(ConfigTableGameText.Instance.GetText("AuthFail")));
                return;
            }
            if (!AuthMgr.Instance.HasUsablePartition()) {
                UIManager.Instance.OpenWin(WinID.UIOK, new WinParam_Text(ConfigTableGameText.Instance.GetText("NoPart")));
                return;
            }
            EnableEnterGameButton(true);
            _loginButtonGrid.gameObject.SetActive(false);
        }

        void EnterGame() {
            EnableEnterGameButton(false);
            AuthMgr.Instance.LoginToGame(1);
        }

        void OnLoginToGameResult(ResultCode result) {
            if (ResultCode.Success != result) {
                EnableEnterGameButton(true);
                UIManager.Instance.OpenWin(WinID.UIOK, new WinParam_Text(NetworkHelper.GetErrorInfo(result)));
                return;
            }
            if (AuthMgr.Instance.IsNewUser()) {
                UIManager.Instance.OpenWin(WinID.UICreateRole);
            }
        }

        void OnEnterGameResult(ResultCode result) {
            if (ResultCode.Success != result) {
                EnableEnterGameButton(true);
                UIManager.Instance.OpenWin(WinID.UIOK, new WinParam_Text(NetworkHelper.GetErrorInfo(result)));
                return;
            }
            SceneMgr.Instance.GoToMainScene();
        }
    }
}