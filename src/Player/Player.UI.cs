using MacGruber;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace mmd2timeline
{
    internal partial class Player
    {
        #region UI相关变量
        /// <summary>
        /// 调试信息
        /// </summary>
        JSONStorableString _DebugInfo;
        /// <summary>
        /// 是否显示调试信息
        /// </summary>
        internal JSONStorableBool _ShowDebugInfoJSON;

        /// <summary>
        /// 播放设置UI清单
        /// </summary>
        List<object> _PlayUIs = new List<object>();

        /// <summary>
        /// 编辑UI
        /// </summary>
        List<object> _EditUIs = new List<object>();

        /// <summary>
        /// 动作播放相关的UI
        /// </summary>
        List<object> _MMDUIs = new List<object>();

        /// <summary>
        /// 加载UI
        /// </summary>
        List<object> _LoadUIs = new List<object>();

        /// <summary>
        /// 导入文件和目录的按钮
        /// </summary>
        List<object> _LoadButtons = new List<object>();

        /// <summary>
        /// 调试UI
        /// </summary>
        GroupUI _DebugUIs;

        /// <summary>
        /// 播放按钮
        /// </summary>
        UIDynamicButton _UIPlayButton;

        /// <summary>
        /// 播放模式选择器
        /// </summary>
        JSONStorableStringChooser _playModeChooser;

        /// <summary>
        /// 播放列表选择器
        /// </summary>
        JSONStorableStringChooser _PlaylistChooser;

        /// <summary>
        /// 加载双按钮
        /// </summary>
        UIDynamicTwinButton _LoadTwinButton;

        ///// <summary>
        ///// 播放速度
        ///// </summary>
        //JSONStorableFloat _PlaySpeedJSON;

        /// <summary>
        /// 加载提示信息
        /// </summary>
        UIDynamicTextField _LoadTips;

        /// <summary>
        /// 插件信息
        /// </summary>
        UIDynamicTextField _PluginInfo;

        /// <summary>
        /// 收藏按钮标签
        /// </summary>
        Text _FavoriteLabel;
        #endregion

        /// <summary>
        /// 加载收藏项目
        /// </summary>
        void LoadFavorite()
        {
            StartCoroutine(this.Playlist.LoadFromFavorite());
        }

        /// <summary>
        /// 加载全部项目
        /// </summary>
        void LoadAll()
        {
            StartCoroutine(this.Playlist.LoadAll());
        }

        /// <summary>
        /// 创建UI
        /// </summary>
        public void CreateUI()
        {
            _DebugUIs = new GroupUI(this);

            // Left UIs
            _LoadUIs.Add(Utils.SetupTwinButton(this, Lang.Get("Load Playlist"), () => StopPlayAndRun(this.Playlist.BeginImport), Lang.Get("Import From Folder"), () => StopPlayAndRun(_MMDFolderHelper.ImportFromFolder), LeftSide));

            var buttons = Utils.SetupTwinButton(this, "✓" + Lang.Get("Save Settings"), () => StartCoroutine(SaveSettings(CurrentItem)), Lang.Get("Favorite"), ToggleFavorite, LeftSide);
            var saveButton = buttons.labelLeft;
            saveButton.color = new Color(0f, 0.6f, 0.3f, 1f);
            _FavoriteLabel = buttons.labelRight;
            _MMDUIs.Add(buttons);

            InitTip("Player");

            // Right UIs
            // 编辑模式切换按钮，默认不显示，进入编辑模式后显示，点击此按钮退出编辑模式
            var testButton = CreateButton($"<< {Lang.Get("Exit Edit Mode")}", RightSide);
            testButton.button.onClick.AddListener(OutEditMode);
            testButton.height = 100;
            testButton.buttonColor = Color.gray;
            _EditUIs.Add(testButton);

            _LoadButtons.Add(Utils.SetupTwinButton(this, Lang.Get("Load Folder"), () => StopPlayAndRun(_MMDFolderHelper.LoadFolder), Lang.Get("Load File"), () => _MMDFolderHelper.LoadFile(this.Playlist.CurrentMMD), RightSide));

            #region 播放列表UI

            _playModeChooser = SetupStaticEnumsChooser<MMDPlayMode>("Play Mode", MMDPlayMode.Names, MMDPlayMode.GetName(MMDPlayMode.Default), RightSide, m =>
            {
                _triggerHelper.Trigger(TriggerEventHelper.TRIGGER_PLAYMODE_CHANGED, m);
                this.Playlist.PlayMode = MMDPlayMode.GetValue(m);
            });
            RegisterStringChooser(_playModeChooser);
            _PlayUIs.Add(_playModeChooser);

            //_PlayUIs.Add(SetupStaticEnumsChooser<ProgressSyncMode>("Sync Mode", ProgressSyncMode.Names, ProgressSyncMode.GetName(ProgressSyncMode.SyncWithAudio), RightSide, m =>
            //{
            //    this._ProgressHelper.SyncMode = ProgressSyncMode.GetValue(m);
            //}));

            _MMDUIs.Add(_UIPlayButton = Utils.SetupButton(this, Lang.Get("Play"), () => TogglePlaying(), RightSide));

            _UIPlayButton.height = 60f;

            _PlayUIs.Add(Utils.SetupTwinButton(this, Lang.Get("Prev"), this.Prev, Lang.Get("Next"), this.Next, RightSide));

            _PlaylistChooser = new JSONStorableStringChooser("playlist", noneStrings, noneString, Lang.Get("Playlist"));// Utils.SetupStringChooser(this, Lang.Get("Playlist"), noneStrings, RightSide);
            _PlaylistChooser.setCallbackFunction = SelectPlayItem;
            _PlaylistChooser.isStorable = false;
            _PlaylistChooser.isRestorable = false;
            CreateFilterablePopup(_PlaylistChooser, RightSide);
            RegisterStringChooser(_PlaylistChooser);

            _PlayUIs.Add(_PlaylistChooser);

            _LoadTwinButton = Utils.SetupTwinButton(this, Lang.Get("Load Favorite"), () => StopPlayAndRun(LoadFavorite), Lang.Get("Load All"), () => StopPlayAndRun(LoadAll), RightSide);
            _LoadUIs.Add(_LoadTwinButton);

            _PlayUIs.Add(Utils.SetupTwinButton(this, Lang.Get("Remove Current"), this.Playlist.RemovePlayItem, Lang.Get("Clear All"), () => StopPlayAndRun(() => this.StartCoroutine(this.Playlist.ClearList())), RightSide));
            _PlayUIs.Add(Utils.SetupButton(this, Lang.Get("Export Playlist"), this.Playlist.BeginExport, RightSide));
            #endregion

            #region 加载提示
            _LoadTips = CreateTextField(new JSONStorableString("Load Tips",
              Lang.Get("<color=#000><size=24>Click the buttons above to prepare to start your Dance!</size></color>" +
               "\n" +
               "\n" +
               "<b>Load Playlist</b> - Import the playlist settings from a saved file." +
               "\n" +
               "\n" +
               "<b>Import From Folder</b> - Load multiple MMDs to the playlist. You should select the upper directory where the MMD files are stored. " +
               "\n" +
               "\n" +
               "<b>Load Folder</b> - Load an MMD into temporary memory and You can test or adjust its play settings.If it works well, you can add it to the playlist." +
               "\n" +
               "\n" +
               "<b>Load File(vmd/wav/mp3/ogg)</b> - Load a file to the current MMD." +
               "\n" +
               "\n" +
               "<b>Load Favorites</b> - Load your favorite MMDs to the current Playlist." +
               "\n" +
               "\n" +
               "<b>Load All</b> - Load All MMDs to the current Playlist." +
               "")),
               LeftSide);
            _LoadTips.backgroundColor = new Color(1f, 0.92f, 0.016f, 0f);
            _LoadTips.height = 1024f;
            #endregion

            // 创建进度条UI
            _ProgressHelper.CreateUI(this, LeftSide);

            // 创建音频控制UI
            _AudioPlayHelper.CreateSettingsUI(this, LeftSide);
            // 创建音频选择UI
            _AudioPlayHelper.CreateChooserUI(this, LeftSide);

            // 镜头选择UI
            _CameraHelper.CreateChooserUI(this, LeftSide);
            // 创建镜头设置UI
            _CameraHelper.CreateSettingsUI(this, RightSide);

            //#region 播放速度UI配置
            //var speedParamName = "Play Speed";
            //_PlaySpeedJSON = new JSONStorableFloat(speedParamName, 1f, 0f, 2f);
            //_PlaySpeedJSON.setCallbackFunction = s =>
            //{
            //    // Time.timeScale = s;
            //    //_AudioPlayController.SetPlaySpeed(s);

            //    _ProgressHelper.SetPlaySpeed(s);
            //};
            //RegisterFloat(_PlaySpeedJSON);

            //var playSpeedSlider = CreateSlider(_PlaySpeedJSON, RightSide);
            //playSpeedSlider.ConfigureQuickButtons(-0.01f, -0.10f, -0.25f, -0.50f, 0.01f, 0.10f, 0.25f, 0.5f);
            //playSpeedSlider.label = Lang.Get(speedParamName);
            //_PlayUIs.Add(_PlaySpeedJSON);
            //#endregion

            // 锁定人物位置
            //_MMDUIs.Add(Utils.SetupToggle(this, Lang.Get("Lock Person Position"), config.LockPersonPosition, v => config.LockPersonPosition = v, RightSide));

            //_MMDUIs.Add(CreateUISpacer(LeftSide));

            #region 版本信息
            var versionStr = $"<color=#000><size=35><b>{PLUGIN_NAME}</b></size></color>";
            var pluginInfo = "\n" +
                versionStr +
                "\n\n<b>" +
                $"v{this.Version}" +
                $"</b>" +
                $"\n" +
                $"\n<size=32><b>" +
                Lang.Get("IMPORTANT!!!") +
                "</b></size>\n<size=24><b>" +
                Lang.Get("The MMD Folder Must In VAM Path.If Your MMD Folder In Other Path, Use The CMD Command 'mklink' Link it to VAM Path.") +
                "</b></size>\n" +
                "\n" +
                "\n" +
                "<size=24>" +
                //Lang.Get("Technical References Catalog:") +
                //"\n" +
                //"https://github.com/x3bits/libmmd-for-unity" +
                //"\n" +
                //Lang.Get("mmd2timeline has been improved based on Libmmd-for-Unity.") +
                //"\n" +
                //"\n" +
                //"https://hub.virtamate.com/resources/macgruber-essentials.160/" +
                //"\n" +
                //Lang.Get("Some of UI processing uses MacGruber_Utils from MacGruber Essentials, and we've made a few feature tweaks and added some functionality. " +
                //"The Capturer module, which we haven't publicly released yet, uses the SuperShot module from that plugin, and we modified its operation rules " +
                //"and added memory release handling to meet the needs of frame-by-frame recording.That Plugin available for free under \"CC BY-SA\" license.") +
                //"\n" +
                //"\n" +
                //"https://github.com/acidbubbles/vam-passenger" +
                //"\n" +
                //Lang.Get("The follow-the-camera feature is learned from Passenger, Passenger complies with the MIT license.") +
                //"\n" +
                //"\n" +
                Lang.Get("This is a free open source VAM plugin. \nThe Source Code is licensed under the GPL-3.0 license.") +
                 "\n" +
                 "\n" +
                 Lang.Get("<b>Follow the address below to get the latest code or progress information.</b>\nhttps://github.com/sFisherE/mmd2timeline") +
                "</size>" +
                "\n";
            var pluginInfoText = Utils.SetupInfoText(this, pluginInfo, 286f - 35f, RightSide);

            _PluginInfo = pluginInfoText.dynamicText;
            _PluginInfo.UItext.alignment = TextAnchor.MiddleCenter;
            _DebugUIs.OuterElements.Add(_PluginInfo);
            #endregion

            var recoverButton = Utils.SetupButton(this, Lang.Get("Reset Motion Model\n(Fix motion issues with bone changes)"), () =>
            {
                StartCoroutine(ResetAllPersonMotion());
            }, RightSide);

            recoverButton.height = 93;
            recoverButton.buttonColor = new Color(1f, 0f, 0f, 0.5f);

            _MMDUIs.Add(recoverButton);

            #region DEBUG
            _ShowDebugInfoJSON = new JSONStorableBool(Lang.Get("Show Debug Info"), false);
            _ShowDebugInfoJSON.setCallbackFunction = v =>
            {
                // 隐藏HUD信息
                if (config.showDebugInfo && !v)
                {
                    this.HideHUDMessage();
                }

                config.showDebugInfo = v;
                _DebugUIs.RefreshView(v);
            };
            var debugToggle = CreateToggle(_ShowDebugInfoJSON, RightSide);

            _DebugUIs.ToggleBool = _ShowDebugInfoJSON;

            _DebugInfo = new JSONStorableString("DebugInfo", "DEBUG");
            var debugField = CreateTextField(_DebugInfo, RightSide);
            debugField.height = 300;

            _DebugUIs.Elements.Add(debugField);

            _DebugUIs.RefreshView();
            #endregion
        }

        /// <summary>
        /// 切换播放模式
        /// </summary>
        void TogglePlayMode()
        {
            var current = _playModeChooser.val;
            var choices = _playModeChooser.choices;

            var index = choices.IndexOf(current);

            index++;

            if (index >= choices.Count)
            {
                index = 0;
            }

            var next = choices[index];

            _playModeChooser.val = next;
        }

        /// <summary>
        /// 停止播放并执行目标方法
        /// </summary>
        /// <param name="action"></param>
        /// <returns></returns>
        private void StopPlayAndRun(UnityAction action)
        {
            this.StopPlaying();
            action?.Invoke();
        }

        #region UI显示控制

        /// <summary>
        /// 当前UI模式
        /// </summary>
        int _currentUIMode = PlayerUIMode.Init;

        int CurrentUIMode
        {
            get
            {
                return _currentUIMode;
            }
            set
            {
                if (_currentUIMode != value)
                {
                    _currentUIMode = value;

                    switch (_currentUIMode)
                    {
                        case PlayerUIMode.Init:
                            _triggerHelper.Trigger(TriggerEventHelper.TRIGGER_PLAYMODE_INIT);
                            break;
                        case PlayerUIMode.Edit:
                            _triggerHelper.Trigger(TriggerEventHelper.TRIGGER_PLAYMODE_EDIT);
                            break;
                        case PlayerUIMode.Play:
                            _triggerHelper.Trigger(TriggerEventHelper.TRIGGER_PLAYMODE_PLAY);
                            break;
                        case PlayerUIMode.Load:
                            _triggerHelper.Trigger(TriggerEventHelper.TRIGGER_PLAYMODE_LOAD);
                            break;
                    }
                }
            }
        }

        /// <summary>
        /// 刷新UI模式
        /// </summary>
        void RefreshUIByCurrentUIMode()
        {
            if (CurrentUIMode == PlayerUIMode.Edit)
            {
                ShowEditUI();
            }
            else
            {
                ShowInitLoadUI();
            }
        }

        /// <summary>
        /// 显示初始UI
        /// </summary>
        internal void ShowInitLoadUI()
        {
            try
            {
                if (this.Playlist.MMDCount > 0)
                {
                    this.ShowPlayUI();
                }
                else
                {
                    // 隐藏提示UI
                    HideTip();
                    //// 隐藏导入UI
                    //ShowImportUIs(false);
                    // 隐藏编辑UI
                    ShowEditUIs(false);
                    // 隐藏MMDUI
                    ShowMMDUIs(false);
                    // 隐藏播放UI
                    ShowPlayUIs(false);
                    // 显示加载UI
                    ShowLoadUIs(true);
                    // 显示加载文件和目录的按钮
                    ShowLoadButtons(true);
                    // 显示加载提示
                    _LoadTips.gameObject.SetActive(true);

                    _PluginInfo.height = 1000f;

                    CurrentUIMode = PlayerUIMode.Init;
                }
            }
            catch (Exception ex)
            {
                LogUtil.LogError(ex);
            }
        }

        /// <summary>
        /// 显示普通播放UI
        /// </summary>
        private void ShowPlayUI()
        {
            try
            {
                // 隐藏提示UI
                HideTip();
                // 隐藏编辑UI
                ShowEditUIs(false);
                // 显示MMDUI
                ShowMMDUIs(true);
                // 显示播放UI
                ShowPlayUIs(true);
                // 显示加载UI
                ShowLoadUIs(true);
                // 显示加载文件和目录的按钮
                ShowLoadButtons(true);
                _LoadTips.gameObject.SetActive(false);
                ShowDebugUIs(true);

                _PluginInfo.height = 286f + 30f + 65f + 115f;

                CurrentUIMode = PlayerUIMode.Play;
            }
            catch (Exception ex)
            {
                LogUtil.LogError(ex);
            }
        }

        /// <summary>
        /// 显示编辑模式UI
        /// </summary>
        private void ShowEditUI()
        {
            try
            {
                // 隐藏提示UI
                HideTip();
                // 显示编辑UI
                ShowEditUIs(true);
                // 显示MMDUI
                ShowMMDUIs(true);
                // 显示加载文件和目录的按钮
                ShowLoadButtons(true);
                // 隐藏播放UI
                ShowPlayUIs(false);
                // 隐藏加载UI
                ShowLoadUIs(false);

                _LoadTips.gameObject.SetActive(false);
                ShowDebugUIs(true);
                _PluginInfo.height = 745f + 30f + 70f + 115f;

                CurrentUIMode = PlayerUIMode.Edit;
            }
            catch (Exception ex)
            {
                LogUtil.LogError(ex);
            }
        }

        /// <summary>
        /// 显示动作UI
        /// </summary>
        /// <param name="show"></param>
        void ShowMMDUIs(bool show)
        {
            // 显示所有MMDUI
            ShowUIElements(_MMDUIs, show);

            _ProgressHelper.ShowUI(show);

            _AudioPlayHelper.ShowChooserUI(show);
            _AudioPlayHelper.ShowSettingUI(show);

            _CameraHelper.ShowChooserUI(show);
            _CameraHelper.ShowSettingUI(show);

            _MotionHelperGroup.ShowMotionUI(show);
        }

        /// <summary>
        /// 显示播放UI
        /// </summary>
        /// <param name="show"></param>
        void ShowPlayUIs(bool show)
        {
            ShowUIElements(_PlayUIs, show);
        }

        /// <summary>
        /// 显示编辑UI
        /// </summary>
        /// <param name="show"></param>
        void ShowEditUIs(bool show)
        {
            ShowUIElements(_EditUIs, show);
        }

        /// <summary>
        /// 显示调试UI
        /// </summary>
        /// <param name="show"></param>
        void ShowDebugUIs(bool show)
        {
            ShowUIElement(_DebugUIs.ToggleBool, show);
            ShowUIElements(_DebugUIs.OuterElements, show);
            if (show)
            {
                _DebugUIs.RefreshView();
            }
            else
            {
                _DebugUIs.RefreshView(false);
            }
        }

        /// <summary>
        /// 显示加载UI
        /// </summary>
        /// <param name="show"></param>
        void ShowLoadUIs(bool show)
        {
            ShowUIElements(_LoadUIs, show);
        }

        /// <summary>
        /// 显示加载文件和目录的按钮
        /// </summary>
        /// <param name="show"></param>
        void ShowLoadButtons(bool show)
        {
            ShowUIElements(_LoadButtons, show);
        }

        /// <summary>
        /// 进入编辑模式
        /// </summary>
        void InEditMode()
        {
            isEditing = true;

            this.ShowEditUI();
        }

        /// <summary>
        /// 退出编辑模式
        /// </summary>
        void OutEditMode()
        {
            this.StopPlaying();

            isEditing = false;

            if (this.Playlist.MMDCount > 0)
            {
                this.ShowPlayUI();
            }
            else
            {
                ClearPlayList();
            }
            _MMDFolderHelper.Clear();
        }
        #endregion
    }
}
