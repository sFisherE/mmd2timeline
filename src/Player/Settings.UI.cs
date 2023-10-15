using MacGruber;
using System;
using UnityEngine;

namespace mmd2timeline
{
    internal partial class Settings : BaseScript
    {
        /// <summary>
        /// 初始化设置UI
        /// </summary>
        protected void InitSettingUI()
        {
            InitGeneralSettingUI();
            InitPhysicsMeshUI();
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
            CreateTitleUI("Shortcut Keys", RightSide);

            SetupToggle(config.EnableSpacePlay, "Enable Space to Toggle Play/Pause", dft.EnableSpacePlay, RightSide);
            SetupToggle(config.EnableRightArrow, "Enable Right Arrow to Forward 1s", dft.EnableRightArrow, RightSide);
            SetupToggle(config.EnableLeftArrow, "Enable Left Arrow to Back 1s", dft.EnableLeftArrow, RightSide);
            SetupToggle(config.EnableUpArrow, "Enable Up Arrow to Play Previous", dft.EnableUpArrow, RightSide);
            SetupToggle(config.EnableDownArrow, "Enable Down Arrow to Play Next", dft.EnableDownArrow, RightSide);

            Utils.SetupInfoOneLine(this, Lang.Get("U - Toggle UI Hidden/Visible"), RightSide);
            Utils.SetupInfoOneLine(this, Lang.Get("M - Toggle WindowCamera View"), RightSide);

            Utils.SetupSpacer(this, 10f, RightSide);
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

            var textInfo = Utils.SetupInfoTextNoScroll(this, Lang.Get("If you need to set your own language, press the \"Generate Language Profile\" button, it will generate the Saves\\PluginData\\mmd2timeline\\lang.json file, you can modify the content in it to get your own language profiles."), 240, RightSide);
            textInfo.text.alignment = TextAnchor.MiddleCenter;

            Utils.SetupSpacer(this, 10f, RightSide);
        }

        /// <summary>
        /// 初始化通用设置UI
        /// </summary>
        void InitGeneralSettingUI()
        {
            CreateTitleUI("General Settings", LeftSide);

            var motionModeChooser = SetupStaticEnumsChooser<MotionMode>("Motion Mode", MotionMode.Names, MotionMode.GetName(MotionMode.Normal), LeftSide, v => { SetValueByMode(MotionMode.GetValue(v)); });

            springSlider = SetupSliderFloat(config.AllJointsSpringPercent, "All Joints Spring Percent", dft.AllJointsSpringPercent, 0f, 1f, v =>
            {
                config.AllJointsSpringPercent = v;

                motionModeChooser.valNoCallback = Lang.Get(MotionMode.GetName(config.CurrentMotionMode));

                SetAllPersonJointsSpringPercent(v);
            }, LeftSide, "F4");

            damperSlider = SetupSliderFloat(config.AllJointsDamperPercent, "All Joints Damper Percent", dft.AllJointsDamperPercent, 0f, 1f, v =>
            {
                config.AllJointsDamperPercent = v;

                motionModeChooser.valNoCallback = Lang.Get(MotionMode.GetName(config.CurrentMotionMode));

                SetAllPersonJointsDamperPercent(v);
            }, LeftSide, "F4");

            SetupSliderFloat(config.AllJointsMaxVelocity, "All Joints Max Velocity", dft.AllJointsMaxVelocity, 0f, 100f, v =>
            {
                config.AllJointsMaxVelocity = v;
                SetAllPersonJointsMaxVelocity(v);
            }, LeftSide, "F4");

            //SetupStaticEnumsChooser<PositionState>(PositionState.GetName((int)config.MotionPositionState), "Motion Position State", PositionState.Names, PositionState.GetName((int)dft.MotionPositionState), LeftSide, m =>
            //{
            //    config.MotionPositionState = (FreeControllerV3.PositionState)PositionState.GetValue(m);
            //    //Player.ResetPersonControllerState();
            //});

            //SetupStaticEnumsChooser<RotationState>(RotationState.GetName((int)config.MotionRotationState), "Motion Rotation State", RotationState.Names, RotationState.GetName((int)dft.MotionRotationState), LeftSide, m =>
            //{
            //    config.MotionRotationState = (FreeControllerV3.RotationState)RotationState.GetValue(m);
            //    //Player.ResetPersonControllerState();
            //});

            //SetupSliderFloat(config.GlobalMotionScale, "Global Motion Scale", dft.GlobalMotionScale, 0f, 2f, v => config.GlobalMotionScale = v, LeftSide, "F4");

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
    }
}
