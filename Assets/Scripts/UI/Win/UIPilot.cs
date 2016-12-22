using UnityEngine;
using System.Collections;
using System.Collections.Generic;

using ZR_MasterTools;
using Sgame;
using SLMS;


namespace Sgame.UI
{
    public class UIPilot : UIWin
    {
        // 头像
        UIGrid _iconGrid;
        // 特性
        UITexture _radarMapTexture;
        GameObject _radarMaoTipsGO;
        UIScrollView _buffIntroScroll;
        UILabel _buffIntroLabel;
        // 基础信息
        UITexture _nameTexture;
        UITexture _voiceActorTexture;
        // 友好度
        UISlider _intimacySlider;
        UILabel _intimacyLevelLabel;
        // 介绍
        UIScrollView _introScroll;
        UILabel _introLabel;
        // 等级
        UISlider _levelSlider;
        UILabel _levelLabel;
        // 招募
        GameObject _recruitGO;
        UILabel _recruitWayLabel;
        UILabel _recruitCostLabel;
        UISprite _recruitIconSprite;
        UIButton _recruitButton;
        // 左右按钮
        UIButton _leftButton;
        UIButton _rightButton;
        // 模型
        GameObject _modelParentGO;
        GameObject _modelGO;
        // 手指控件
        FingerDownDetector _fingerDown;
        FingerUpDetector _fingerUp;
        bool _hasFingerDown = false;

        GameObject _iconItemPrefab;


        List<Pilot> _pilotList = new List<Pilot>();
        Pilot _currentPilot;
        int _currentIndex;
        List<PilotIconItem> _iconItemList = new List<PilotIconItem>();


        protected override void OnInit() {
            base.OnInit();
            _iconItemPrefab = Utils.LoadUIItemPrefab("PilotIconItem");
            InitFingerDetector();
            InitUI();
            CachePilotList();
        }

        void InitFingerDetector() {
            _fingerDown = FingerGestures.Instance.GetComponent<FingerDownDetector>();
            _fingerUp = FingerGestures.Instance.GetComponent<FingerUpDetector>();
        }

        void InitUI() {
            for (int i = 0; i < _transform.childCount; i++) {
                Transform child = _transform.GetChild(i);
                switch (child.name) {
                    case "IconList":
                        _iconGrid = child.Find("Scroll/Grid").GetComponent<UIGrid>();
                        break;
                    case "Character":
                        _radarMapTexture = child.Find("RadarMap/Value").GetComponent<UITexture>();
                        _radarMaoTipsGO = child.Find("RadarMap/Tips").gameObject;
                        Transform buffTrans = child.Find("Buff/Scroll");
                        _buffIntroScroll = buffTrans.GetComponent<UIScrollView>();
                        _buffIntroLabel = buffTrans.Find("Text").GetComponent<UILabel>();
                        break;
                    case "Model":
                        _modelParentGO = child.gameObject;
                        break;
                    case "BaseInfo":
                        InitBaseInfoUI(child);
                        break;
                    case "Left":
                        _leftButton = child.GetComponent<UIButton>();
                        EventDelegate.Set(_leftButton.onClick, () => { OnClickSwitchButton(-1); });
                        break;
                    case "Right":
                        _rightButton = child.GetComponent<UIButton>();
                        EventDelegate.Set(_rightButton.onClick, () => { OnClickSwitchButton(1); });
                        break;
                }
            }
        }

        void InitBaseInfoUI(Transform parent) {
            for (int i = 0; i < parent.childCount; i++) {
                Transform child = parent.GetChild(i);
                switch (child.name) {
                    case "Name":
                        _nameTexture = child.GetComponent<UITexture>();
                        break;
                    case "VoiceActor":
                        _voiceActorTexture = child.GetComponent<UITexture>();
                        break;
                    case "Intro":
                        _introScroll = child.GetComponent<UIScrollView>();
                        _introLabel = child.Find("Text").GetComponent<UILabel>();
                        break;
                    case "Level":
                        _levelSlider = child.GetComponent<UISlider>();
                        _levelLabel = child.Find("Value").GetComponent<UILabel>();
                        break;
                    case "Intimacy":
                        _intimacySlider = child.GetComponent<UISlider>();
                        _intimacyLevelLabel = child.Find("Level").GetComponent<UILabel>();
                        break;
                    case "Recruit":
                        _recruitGO = child.gameObject;
                        _recruitWayLabel = child.Find("GetWay").GetComponent<UILabel>();
                        Transform buttonTrans = child.Find("Get");
                        _recruitButton = buttonTrans.GetComponent<UIButton>();
                        EventDelegate.Set(_recruitButton.onClick, OnRecruitPilot);
                        _recruitCostLabel = buttonTrans.Find("Cost").GetComponent<UILabel>();
                        _recruitIconSprite = buttonTrans.Find("Icon").GetComponent<UISprite>();
                        break;
                }
            }
        }

        void OnClickSwitchButton(int offset) {
            int newIndex = _currentIndex + offset;
            if (newIndex < 0) {
                newIndex = _pilotList.Count - 1;
            }
            if (newIndex > _pilotList.Count - 1) {
                newIndex = 0;
            }
            _iconItemList[newIndex].SetSelected();
        }

        void CachePilotList() {
            foreach (Pilot pilot in PilotMgr.Instance.pilotDict.Values) {
                _pilotList.Add(pilot);
            }
            Dictionary<int, TPilot> configDict = ConfigTablePilot.Instance.GetConfigDict();
            foreach (TPilot pilot in configDict.Values) {
                if (!PilotMgr.Instance.pilotDict.ContainsKey(pilot.id)) {
                    _pilotList.Add(new Pilot(pilot.id));
                }
            }
            _pilotList.Sort((a, b) => {
                if (a.id == b.id) {
                    return 0;
                }
                if (a.level != b.level) {
                    return (a.level > b.level ? -1 : 1);
                }
                return (a.id < b.id ? -1 : 1);
            });
        }

        int GetPilotIndex(Pilot pilot) {
            for (int i = 0; i < _pilotList.Count; i++) {
                if (pilot.id == _pilotList[i].id) {
                    return i;
                }
            }
            return 0;
        }


        protected override void OnMsgInit() {
            base.OnMsgInit();
            _fingerDown.OnFingerDown += OnFingerDown;
            _fingerUp.OnFingerUp += OnFingerUp;
        }
        protected override void OnMsgRemove() {
            base.OnMsgRemove();
            _fingerDown.OnFingerDown -= OnFingerDown;
            _fingerUp.OnFingerUp -= OnFingerUp;
        }

        void OnFingerDown(FingerDownEvent e) {
            if (e.Selection == _radarMapTexture.cachedGameObject) {
                _hasFingerDown = true;
                _radarMaoTipsGO.SetActive(true);
            }
        }

        void OnFingerUp(FingerUpEvent e) {
            if (!_hasFingerDown) {
                return;
            }
            _radarMaoTipsGO.SetActive(false);
        }

        protected override void OnStart() {
            base.OnStart();
            UIManager.Instance.topBar.SetupGoBackInfo(UIManager.Instance.OpenHomeWin);
        }


        protected override void OnOpen(WinParamBase paramObject) {
            base.OnOpen(paramObject);
            ShowSwitchButton();
            LoadIcons();
            LoadPilotInfo(0);
        }

        void ShowSwitchButton() {
            _leftButton.gameObject.SetActive(_pilotList.Count > 1);
            _rightButton.gameObject.SetActive(_pilotList.Count > 1);
        }

        void LoadIcons() {
            for (int i = 0; i < _pilotList.Count; i++) {
                GameObject go = NGUITools.AddChild(_iconGrid.gameObject, _iconItemPrefab);
                PilotIconItem item = go.GetComponent<PilotIconItem>();
                item.SetItem(_pilotList[i], OnChangePilot);
                _iconItemList.Add(item);
            }
            _iconGrid.repositionNow = true;
            _iconItemList[0].SetSelected();
        }

        void OnChangePilot(Pilot selectedPilot) {
            LoadPilotInfo(GetPilotIndex(selectedPilot));
        }

        void LoadPilotInfo(int index) {
            _currentIndex = index;
            _currentPilot = _pilotList[_currentIndex];
            LoadModel();
            SetBaseInfo();
            SetIntimacyInfo();
            SetLevelInfo();
            SetRecruitInfo();
            SetCharacterInfo();
        }

        void LoadModel() {
            if (null != _modelGO) {
                if (_modelGO.GetComponent<RotateModel>().PilotID == _currentPilot.id) {
                    return;
                }
                Destroy(_modelGO);
            }
            _modelGO = Utils.LoadModel(_currentPilot.ModelPath, _modelParentGO);
            if (null == _modelGO) {
                return;
            }
            _modelGO.AddComponent<RotateModel>().PilotID = _currentPilot.id;
        }

        void SetBaseInfo() {
            TPilot config = _currentPilot.Config;
            _nameTexture.mainTexture = BundleMgr.Instance.LoadResource<Texture>(GlobalConfig.PilotTexturePath + config.nameTextureID);
            _nameTexture.MakePixelPerfect();
            _voiceActorTexture.mainTexture = BundleMgr.Instance.LoadResource<Texture>(GlobalConfig.PilotTexturePath + config.voiceActorTextureID);
            _voiceActorTexture.MakePixelPerfect();
            _introLabel.text = config.intro;
            _introScroll.ResetPosition();
        }

        void SetIntimacyInfo() {
            int needExp = ConfigTablePilotIntimacy.Instance.GetConfigByLevel(_currentPilot.intimacyLevel).levelUpExp;
            _intimacySlider.value = (0 == needExp ? 1F : (float)_currentPilot.intimacyExp / needExp);
            _intimacyLevelLabel.text = BBcodeHelper.BoldText(_currentPilot.intimacyLevel);
        }

        void SetLevelInfo() {
            if (!PilotMgr.Instance.HasOwnPilot(_currentPilot.id)) {
                _levelSlider.gameObject.SetActive(false);
                return;
            }
            _levelSlider.gameObject.SetActive(true);
            _levelLabel.text = BBcodeHelper.BoldText(string.Format("LV.{0}", _currentPilot.level));
            int needExp = ConfigTablePilotGrowth.Instance.GetConfigByLevel(_currentPilot.level).levelUpExp;
            _levelSlider.value = (0 == needExp ? 1F : (float)_currentPilot.exp / needExp);
        }

        void SetRecruitInfo() {
            if (PilotMgr.Instance.HasOwnPilot(_currentPilot.id)) {
                _recruitGO.SetActive(false);
                return;
            }
            _recruitGO.SetActive(true);
            bool enoughMilitaryRank = (PlayerMgr.Instance.player.militaryRank >= _currentPilot.Config.needMilitaryRank);
            _recruitButton.isEnabled = enoughMilitaryRank;
            _recruitWayLabel.text = enoughMilitaryRank ? string.Empty :
                string.Format(ConfigTableGameText.Instance.GetText("EnlistNeedMilitaryRank"), ConfigTableMilitaryRank.Instance.GetConfigByLevel(_currentPilot.Config.needMilitaryRank).name);
            TCostRes costConfig = _currentPilot.Config.recruitCost;
            _recruitIconSprite.spriteName = UseResourceHelper.GetResIconID(costConfig.type);
            _recruitIconSprite.MakePixelPerfect();
            _recruitCostLabel.text = BBcodeHelper.ColorText_Resource(costConfig.needNum, UseResourceHelper.GetResNum(costConfig.type), costConfig.needNum, true);
        }

        void SetCharacterInfo() {
            TPilot config = _currentPilot.Config;
            _radarMapTexture.mainTexture = BundleMgr.Instance.LoadResource<Texture>(GlobalConfig.PilotTexturePath + config.radarMapID);
            _radarMapTexture.MakePixelPerfect();
            _buffIntroLabel.text = _currentPilot.GetCharacterDescription();
            _buffIntroScroll.ResetPosition();
        }



        void OnRecruitPilot() {
            TPilot config = _currentPilot.Config;
            WinParam_ConfirmUseResource costInfo = new WinParam_ConfirmUseResource(config.recruitCost.type, config.recruitCost.needNum,
                string.Format(ConfigTableGameText.Instance.GetText("ConfirmRecruitPilot"), config.name), OnConfirmRecruitPilot);
            UseResourceHelper.UseResource(costInfo);
        }

        void OnConfirmRecruitPilot() {
            Debug.Log("招募" + _currentPilot.Config.name);
        }
    }
}