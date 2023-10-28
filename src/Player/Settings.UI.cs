using MacGruber;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace mmd2timeline
{
    internal partial class Settings : BaseScript
    {
        /// <summary>
        /// 镜头聚焦UI清单
        /// </summary>
        GroupUI _CameraFocusUI;

        List<JSONStorableBool> _StorableBools = new List<JSONStorableBool>();
        List<JSONStorableFloat> _StorableFloats = new List<JSONStorableFloat>();
        List<JSONStorableStringChooser> _StorableStrings = new List<JSONStorableStringChooser>();

        /// <summary>
        /// 脚部放松UI清单
        /// </summary>
        List<object> _FreeFootUI = new List<object>();

        /// <summary>
        /// 高度修正UI
        /// </summary>
        List<object> _HeightCorrectionUI = new List<object>();

        /// <summary>
        /// 镜头控制UI
        /// </summary>
        List<object> _CameraControlUI = new List<object>();

        /// <summary>
        /// 初始化设置UI
        /// </summary>
        protected void InitSettingUI()
        {
            InitGeneralSettingUI();
            InitPhysicsMeshUI();
            //InitAutoCorrectUI();
            InitCameraSettingUI();
            InitShortcutKeyUI();
            InitLanguageUI();

            Utils.SetupButton(this, Lang.Get("Reset All Settings To Default"), () =>
            {
                try
                {
                    config.ResetAll();

                    SetValuesToDefalut();
                }
                catch (Exception ex)
                {
                    LogUtil.LogError(ex, "InitSettingUI::Reset:");
                }
            }, RightSide);
        }

        /// <summary>
        /// 初始化快捷键UI
        /// </summary>
        void InitShortcutKeyUI()
        {
            CreateTitleUI("Shortcut Keys", LeftSide);

            SetupToggle(config.EnableSpacePlay, "Enable Space to Toggle Play/Pause", dft.EnableSpacePlay, v => config.EnableSpacePlay = v, LeftSide);
            SetupToggle(config.EnableRightArrow, "Enable Right Arrow to Forward 1s", dft.EnableRightArrow, v => config.EnableRightArrow = v, LeftSide);
            SetupToggle(config.EnableLeftArrow, "Enable Left Arrow to Back 1s", dft.EnableLeftArrow, v => config.EnableLeftArrow = v, LeftSide);
            SetupToggle(config.EnableUpArrow, "Enable Up Arrow to Play Previous", dft.EnableUpArrow, v => config.EnableUpArrow = v, LeftSide);
            SetupToggle(config.EnableDownArrow, "Enable Down Arrow to Play Next", dft.EnableDownArrow, v => config.EnableDownArrow = v, LeftSide);

            Utils.SetupInfoOneLine(this, Lang.Get("U - Toggle UI Hidden/Visible"), LeftSide);
            Utils.SetupInfoOneLine(this, Lang.Get("M - Toggle WindowCamera View"), LeftSide);

            Utils.SetupSpacer(this, 10f, LeftSide);
        }

        /// <summary>
        /// 初始化语言UI
        /// </summary>
        void InitLanguageUI()
        {
            CreateTitleUI("Language Settings", RightSide);

            Utils.SetupButton(this, Lang.Get("Generate Language Profile"), () =>
            {
                Lang.GenerateProfile();
            }, RightSide);

            var textInfo = Utils.SetupInfoTextNoScroll(this, Lang.Get("If you need to set your own language, press the \"Generate Language Profile\" button, it will generate the Saves\\PluginData\\mmd2timeline\\lang.json file, you can modify the content in it to get your own language profiles."), 180, RightSide);
            textInfo.text.alignment = TextAnchor.MiddleCenter;

            Utils.SetupSpacer(this, 10f, RightSide);
        }

        /// <summary>
        /// 初始化通用设置UI
        /// </summary>
        void InitGeneralSettingUI()
        {
            //var fpsSlider = Utils.SetupSliderInt(this, "FPS", 60, 0, 300, LeftSide);
            //fpsSlider.setCallbackFunction = v => UnityEngine.Application.targetFrameRate = (int)v;

            CreateTitleUI("General Settings", LeftSide);

            //SetupStaticEnumsChooser<MotionEngine>(MotionEngine.GetName(config.MotionEngineMode), "Motion Engine", MotionEngine.Names, MotionEngine.GetName(dft.MotionEngineMode), LeftSide, v => config.MotionEngineMode = MotionEngine.GetValue(v));

            SetupSliderFloat(config.Volume, "Play Volume", dft.Volume, 0f, 1f, v =>
            {
                config.Volume = v;
                AudioPlayHelper.GetInstance().SetVolume(config.Volume);
            }, LeftSide, "F4");

            //SetupToggle(config.ResetPhysicalWhenLoadMotion, "Reset Person Physical When Motion Reset", dft.ResetPhysicalWhenLoadMotion, v => config.ResetPhysicalWhenLoadMotion = v, LeftSide);

            //SetupToggle(config.LockPersonPosition, "Lock Person Position", dft.LockPersonPosition, v => config.LockPersonPosition = v, LeftSide);

            var motionModeChooser = SetupStaticEnumsChooser<MotionMode>("Motion Mode", MotionMode.Names, MotionMode.GetName(MotionMode.Normal), LeftSide, v => { SetValueByMode(MotionMode.GetValue(v)); });

            springSlider = SetupSliderFloat(config.AllJointsSpringPercent, "All Joints Spring Percent", dft.AllJointsSpringPercent, 0f, 1f, v =>
            {
                config.AllJointsSpringPercent = v;

                motionModeChooser.val = Lang.Get(MotionMode.GetName(config.CurrentMotionMode));

                SetAllPersonJointsSpringPercent(v);
            }, LeftSide, "F4");

            damperSlider = SetupSliderFloat(config.AllJointsDamperPercent, "All Joints Damper Percent", dft.AllJointsDamperPercent, 0f, 1f, v =>
            {
                config.AllJointsDamperPercent = v;

                motionModeChooser.val = Lang.Get(MotionMode.GetName(config.CurrentMotionMode));

                SetAllPersonJointsDamperPercent(v);
            }, LeftSide, "F4");

            SetupSliderFloat(config.AllJointsMaxVelocity, "All Joints Max Velocity", dft.AllJointsMaxVelocity, 0f, 100f, v =>
            {
                config.AllJointsMaxVelocity = v;
                SetAllPersonJointsMaxVelocity(v);
            }, LeftSide, "F4");

            SetupStaticEnumsChooser<PositionState>(PositionState.GetName((int)config.MotionPositionState), "Motion Position State", PositionState.Names, PositionState.GetName((int)dft.MotionPositionState), LeftSide, m =>
            {
                config.MotionPositionState = (FreeControllerV3.PositionState)PositionState.GetValue(m);
                //Player.ResetPersonControllerState();
            });

            SetupStaticEnumsChooser<RotationState>(RotationState.GetName((int)config.MotionRotationState), "Motion Rotation State", RotationState.Names, RotationState.GetName((int)dft.MotionRotationState), LeftSide, m =>
            {
                config.MotionRotationState = (FreeControllerV3.RotationState)RotationState.GetValue(m);
                //Player.ResetPersonControllerState();
            });

            //SetupSliderFloat(config.GlobalMotionScale, "Global Motion Scale", dft.GlobalMotionScale, 0f, 2f, v => config.GlobalMotionScale = v, LeftSide, "F4");
            // 是否启用初始动作修正
            SetupToggle(config.EnableInitialMotionAdjustment, "Enable Initial Motion Adjustment", dft.EnableInitialMotionAdjustment, (v) => config.EnableInitialMotionAdjustment = v, LeftSide);
            // 是否启用初始动作修正
            SetupToggle(config.ResetPhysicalWhenLoadMotion, "Reset Model Before Motion Start", dft.ResetPhysicalWhenLoadMotion, (v) => config.ResetPhysicalWhenLoadMotion = v, LeftSide);

            Utils.SetupSpacer(this, 10f, LeftSide);
        }

        /// <summary>
        /// 初始化物理网格UI
        /// </summary>
        void InitPhysicsMeshUI()
        {
            CreateTitleUI("Physics Mesh Settings", RightSide);

            SetupToggle(config.MouthPhysicsMesh, "Enable Mouth Physics Mesh", dft.MouthPhysicsMesh, (v) =>
            {
                config.MouthPhysicsMesh = v;

                SetAllPersonPhysicsMesh("MouthPhysicsMesh", v);
            }, RightSide);

            SetupToggle(config.BreastPhysicsMesh, "Enable Breast Physics Mesh", dft.BreastPhysicsMesh, (v) =>
            {
                config.BreastPhysicsMesh = v;

                SetAllPersonPhysicsMesh("BreastPhysicsMesh", v);
            }, RightSide);

            SetupToggle(config.LowerPhysicsMesh, "Enable Lower Physics Mesh", dft.LowerPhysicsMesh, (v) =>
            {
                config.LowerPhysicsMesh = v;
                SetAllPersonPhysicsMesh("LowerPhysicsMesh", v);
            }, RightSide);

            Utils.SetupSpacer(this, 10f, RightSide);
        }

        /// <summary>
        /// 刷新镜头UI
        /// </summary>
        void RefreshCameraUI()
        {
            if (config.UseOriginalCamera)
            {
                _CameraFocusUI.RefreshView(false);
                ShowUIElements(_CameraControlUI, false);

                CameraHelper.GetInstance().ShowFocusUI(false);
            }
            else
            {
                ShowUIElements(_CameraControlUI, true);
                _CameraFocusUI.RefreshView();
                CameraHelper.GetInstance().ShowFocusUI(true);
            }
        }

        /// <summary>
        /// 初始化镜头设置UI
        /// </summary>
        void InitCameraSettingUI()
        {
            _CameraFocusUI = new GroupUI(this);

            CreateTitleUI("Camera Settings", RightSide);

            SetupToggle(config.CameraEnabled, "Camera Enabled Non-VR", dft.CameraEnabled, (v) => config.CameraEnabled = v, RightSide);

            SetupToggle(config.CameraEnabledInVR, "Camera Enabled in VR", dft.CameraEnabledInVR, (v) => config.CameraEnabledInVR = v, RightSide);

            SetupToggle(config.CameraFOVEnabled, "FOV Enabled", dft.CameraFOVEnabled, (v) =>
            {
                config.CameraFOVEnabled = v;
            }, RightSide);

            //SetupToggle(config.AutoGazeToWindowCamera, "Auto Gaze to WindowCamera", dft.AutoGazeToWindowCamera, v => config.AutoGazeToWindowCamera = v, RightSide);

            SetupStaticEnumsChooser<CameraControlModes>(CameraControlModes.GetName(config.CameraControlMode), "Camera Control Mode", CameraControlModes.Names, CameraControlModes.GetName(dft.CameraControlMode), RightSide, v =>
            {
                config.CameraControlMode = CameraControlModes.GetValue(v);

                RefreshCameraUI();
            });

            _CameraControlUI.Add(SetupSliderFloat(config.CameraPositionSmoothing, "Camera Position Smoothing", dft.CameraPositionSmoothing, 0f, 1f, (v) => config.CameraPositionSmoothing = v, RightSide, "F4"));

            _CameraControlUI.Add(SetupSliderFloat(config.CameraRotationSmoothing, "Camera Rotation Smoothing", dft.CameraRotationSmoothing, 0f, 1f, (v) => config.CameraRotationSmoothing = v, RightSide, "F4"));

            //_CameraControlUI.Add(SetupToggle(config.DeactiveCameraWhenMainHUDOpened, "Deactive when MainHUD Opened", dft.DeactiveCameraWhenMainHUDOpened, (v) => config.DeactiveCameraWhenMainHUDOpened = v, RightSide));

            #region 窗口镜头同步

            var syncWindowCameraJSON = SetupToggle(config.SyncWindowCamera, "Play in WindowCamera", dft.SyncWindowCamera, RightSide);//_WindowCameraAtom

            _StorableBools.Add(syncWindowCameraJSON);
            _CameraControlUI.Add(syncWindowCameraJSON);

            syncWindowCameraJSON.setCallbackFunction = v =>
            {
                if (v)
                {
                    config.SyncWindowCamera = v;
                }
                else
                {
                    config.SyncWindowCamera = false;

                    if (v)
                    {
                        syncWindowCameraJSON.valNoCallback = false;
                    }
                }
            };

            #endregion

            var cameraHelper = CameraHelper.GetInstance();
            // 创建镜头聚焦UI
            cameraHelper.CreateFocusUI(this, RightSide);

            if (containingAtom.type == "Person")
            {
                cameraHelper.SetFocusTarget(containingAtom.uid, "neckControl");
            }

            Utils.SetupSpacer(this, 10f, RightSide);
        }

        /// <summary>
        /// 初始化自动高度修正UI
        /// </summary>
        void InitAutoCorrectUI()
        {
            CreateTitleUI("Motion Correction Setting", LeftSide);

            // 是否启用初始动作修正
            SetupToggle(config.EnableInitialMotionAdjustment, "Enable Initial Motion Adjustment", dft.EnableInitialMotionAdjustment, (v) => config.EnableInitialMotionAdjustment = v, LeftSide);

            // 是否允许跪姿修正
            SetupToggle(config.EnableKneeingCorrections, "Enable Kneeing Corrections", dft.EnableKneeingCorrections, (v) => config.EnableKneeingCorrections = v, LeftSide);

            SetupStaticEnumsChooser(AutoCorrectHeightMode.GetName(config.AutoFixHeightMode), "Height Correction Mode", AutoCorrectHeightMode.Names, AutoCorrectHeightMode.GetName(dft.AutoFixHeightMode), LeftSide, (StaticEnumsSetCallback<AutoCorrectHeightMode>)(m =>
            {
                config.AutoFixHeightMode = AutoCorrectHeightMode.GetValue(m);
                ShowHeightCorrectionUI();
            }));

            _HeightCorrectionUI.Add(SetupSliderFloat(config.AutoCorrectFixHeight, "Fix Height", dft.AutoCorrectFixHeight, 0f, 1f, v => config.AutoCorrectFixHeight = v, LeftSide, "F4"));

            _HeightCorrectionUI.Add(SetupSliderFloat(config.AutoCorrectFloorHeight, "Floor Height", dft.AutoCorrectFloorHeight, -1f, 1f, v => config.AutoCorrectFloorHeight = v, LeftSide, "F4"));

            _HeightCorrectionUI.Add(SetupToggle(config.EnableFootFree, "Enable Foot Free", dft.EnableFootFree, (v) =>
            {
                config.EnableFootFree = v;

                try
                {
                    ShowUIElements(_FreeFootUI, v);
                }
                catch (Exception e)
                {
                    LogUtil.LogError(e, $"Enable Foot Free");
                }
            }, LeftSide));

            var enableFootOffJSON = SetupToggle(config.EnableFootOff, "Enable Foot Off", dft.EnableFootOff, (v) => config.EnableFootOff = v, LeftSide);
            _FreeFootUI.Add(enableFootOffJSON);

            var enableKneeOffJSON = SetupToggle(config.EnableKneeOff, "Enable Knee Off", dft.EnableKneeOff, (v) => config.EnableKneeOff = v, LeftSide);
            _FreeFootUI.Add(enableKneeOffJSON);

            _FreeFootUI.Add(SetupSliderFloat(config.ToeOffHeight, "Toe Off Height", dft.ToeOffHeight, 0f, 1f, v => config.ToeOffHeight = v, LeftSide, "F4"));

            _FreeFootUI.Add(SetupSliderFloat(config.FootFixHeight, "Foot Fix Height", dft.FootFixHeight, 0f, 1f, v => config.FootFixHeight = v, LeftSide, "F4"));

            _FreeFootUI.Add(SetupSliderFloat(config.FootOffHeight, "Foot Off Height", dft.FootOffHeight, 0f, 1f, v => config.FootOffHeight = v, LeftSide, "F4"));

            _FreeFootUI.Add(SetupSliderFloat(config.FreeFootJointDriveXTarget, "Free Foot Joint Drive X Target", dft.FreeFootJointDriveXTarget, -65f, 40f, v => config.FreeFootJointDriveXTarget = v, LeftSide));

            _FreeFootUI.Add(SetupSliderFloat(config.FreeFootHoldRotationMaxForce, "Free Foot Hold Rotation Max Force", dft.FreeFootHoldRotationMaxForce, 0f, 1000f, v => config.FreeFootHoldRotationMaxForce = v, LeftSide));

            _FreeFootUI.Add(SetupSliderFloat(config.FreeKneeHoldRotationMaxForce, "Free Knee Hold Rotation Max Force", dft.FreeKneeHoldRotationMaxForce, 0f, 1000f, v => config.FreeKneeHoldRotationMaxForce = v, LeftSide));

            _FreeFootUI.Add(SetupSliderFloat(config.FreeThighHoldRotationMaxForce, "Free Thigh Hold Rotation Max Force", dft.FreeThighHoldRotationMaxForce, 0f, 1000f, v => config.FreeThighHoldRotationMaxForce = v, LeftSide));

            ShowHeightCorrectionUI();

            Utils.SetupSpacer(this, 10f, LeftSide);
        }

        /// <summary>
        /// 根据参数显示高度修正UI
        /// </summary>
        private void ShowHeightCorrectionUI()
        {
            try
            {
                // 隐藏UI
                if (config.AutoFixHeightMode == AutoCorrectHeightMode.None)
                {
                    ShowUIElements(_HeightCorrectionUI, false);
                    ShowUIElements(_FreeFootUI, false);
                }
                else
                {
                    ShowUIElements(_HeightCorrectionUI, true);
                    ShowUIElements(_FreeFootUI, config.EnableFootFree);
                }
            }
            catch (Exception e)
            {
                LogUtil.LogError(e, $"ShowHeightCorrectionUI");
            }
        }

        /// <summary>
        /// 设置到默认值
        /// </summary>
        private void SetValuesToDefalut()
        {
            foreach (var item in _StorableBools) { item.SetValToDefault(); }
            foreach (var item in _StorableFloats) { item.SetValToDefault(); }
            foreach (var item in _StorableStrings) { item.SetValToDefault(); }
        }

        /// <summary>
        /// 设置Slider
        /// </summary>
        /// <param name="label"></param>
        /// <param name="defaultValue"></param>
        /// <param name="minValue"></param>
        /// <param name="maxValue"></param>
        /// <param name="callback"></param>
        /// <param name="rightSide"></param>
        /// <param name="valueFormat"></param>
        private JSONStorableFloat SetupSliderFloat(float value, string label, float defaultValue, float minValue, float maxValue, Action<float> callback, bool rightSide, string valueFormat = "")
        {
            var slider = Utils.SetupSliderFloat(this, Lang.Get(label), defaultValue, minValue, maxValue, callback, rightSide, valueFormat);
            slider.val = value;

            _StorableFloats.Add(slider);

            return slider;
        }

        /// <summary>
        /// 设置Toggle
        /// </summary>
        /// <param name="label"></param>
        /// <param name="defaultValue"></param>
        /// <param name="callback"></param>
        /// <param name="rightSide"></param>
        private JSONStorableBool SetupToggle(bool value, string label, bool defaultValue, Action<bool> callback, bool rightSide)
        {
            var toggle = Utils.SetupToggle(this, Lang.Get(label), defaultValue, callback, rightSide);
            toggle.val = value;

            _StorableBools.Add(toggle);

            return toggle;
        }

        /// <summary>
        /// 设置Toggle
        /// </summary>
        /// <param name="label"></param>
        /// <param name="defaultValue"></param>
        /// <param name="rightSide"></param>
        /// <returns></returns>
        private JSONStorableBool SetupToggle(bool value, string label, bool defaultValue, bool rightSide)
        {
            var toggle = Utils.SetupToggle(this, Lang.Get(label), defaultValue, rightSide);
            toggle.val = value;

            _StorableBools.Add(toggle);

            return toggle;
        }

        /// <summary>
        /// 设置静态枚举选择器
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="value"></param>
        /// <param name="label"></param>
        /// <param name="names"></param>
        /// <param name="defaultValue"></param>
        /// <param name="rightSide"></param>
        /// <param name="callback"></param>
        /// <returns></returns>
        private JSONStorableStringChooser SetupStaticEnumsChooser<T>(string value, string label, List<string> names, string defaultValue, bool rightSide, StaticEnumsSetCallback<T> callback)
        {
            var chooser = SetupStaticEnumsChooser<T>(label, names, defaultValue, rightSide, callback);
            chooser.val = Lang.Get(value.ToString());

            _StorableStrings.Add(chooser);
            return chooser;
        }
    }
}
