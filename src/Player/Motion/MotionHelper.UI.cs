using MacGruber;
using System;
using System.Collections.Generic;

namespace mmd2timeline
{
    internal partial class MotionHelper
    {
        /// <summary>
        /// 默认空字符串
        /// </summary>
        private const string noneString = "None";

        /// <summary>
        /// 空字符串列表
        /// </summary>
        internal readonly List<string> noneStrings = new List<string> { noneString };

        #region 属性
        /// <summary>
        /// 是否启用高跟
        /// </summary>
        internal bool EnableHeel
        {
            get
            {
                if (_EnableHeelJSON != null)
                    return _EnableHeelJSON.val;
                return false;
            }
        }
        /// <summary>
        /// 获取UI是否已完成创建
        /// </summary>
        internal bool UICreated
        {
            get
            {
                return _MotionSettingsUI != null;
            }
        }
        #endregion

        /// <summary>
        /// 人物动作设置UI
        /// </summary>
        GroupUI _MotionSettingsUI;

        /// <summary>
        /// 进度
        /// </summary>
        JSONStorableFloat _ProgressJSON;

        /// <summary>
        /// 延迟
        /// </summary>
        JSONStorableFloat _TimeDelayJSON;

        /// <summary>
        /// 关闭下身骨骼的参数
        /// </summary>
        JSONStorableBool _CloseLowerBonesJSON;

        /// <summary>
        /// 是否使用全部关节设置
        /// </summary>
        JSONStorableBool _UseAllJointsSettingsJSON;
        /// <summary>
        /// 所有关节阻尼比例
        /// </summary>
        JSONStorableFloat _AllJointsDamperPercentJSON;
        /// <summary>
        /// 所有关节弹簧比例
        /// </summary>
        JSONStorableFloat _AllJointsSpringPercentJSON;
        /// <summary>
        /// 所有关节最大速度比例
        /// </summary>
        JSONStorableFloat _AllJointsMaxVelocityJSON;
        /// <summary>
        /// 动作缩放
        /// </summary>
        JSONStorableFloat _MotionScaleJSON;
        /// <summary>
        /// 开启表情
        /// </summary>
        JSONStorableBool _EnableFaceJSON;

        /// <summary>
        /// 开启高跟
        /// </summary>
        JSONStorableBool _EnableHeelJSON;

        /// <summary>
        /// 脚部关节驱动X目标调整
        /// </summary>
        JSONStorableFloat _FootJointDriveXTargetAdjust;

        /// <summary>
        /// 脚趾关节驱动X目标调整
        /// </summary>
        JSONStorableFloat _ToeJointDriveXTargetAdjust;

        /// <summary>
        /// 保持角度最大力调整
        /// </summary>
        JSONStorableFloat _HoldRotationMaxForceAdjust;

        /// <summary>
        /// 高跟高度修正
        /// </summary>
        JSONStorableFloat _HeelHeightAdjustJSON;

        /// <summary>
        /// 动作选择器清单
        /// </summary>
        List<JSONStorableStringChooser> _MotionChoosers = new List<JSONStorableStringChooser>();

        ///// <summary>
        ///// 位置X
        ///// </summary>
        //JSONStorableFloat _PositionX;
        ///// <summary>
        ///// 位置Y
        ///// </summary>
        //JSONStorableFloat _PositionY;
        ///// <summary>
        ///// 位置Z
        ///// </summary>
        //JSONStorableFloat _PositionZ;

        ///// <summary>
        ///// 方向X
        ///// </summary>
        //JSONStorableFloat _RotationX;
        ///// <summary>
        ///// 方向Y
        ///// </summary>
        //JSONStorableFloat _RotationY;
        ///// <summary>
        ///// 方向Z
        ///// </summary>
        //JSONStorableFloat _RotationZ;

        /// <summary>
        /// 获取统一格式的Label文字
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        string GetLabelText(string text)
        {
            return $"{_PersonAtom.uid}\\{Lang.Get(text)}";
        }

        /// <summary>
        /// 创建人物动作UI
        /// </summary>
        /// <param name="self"></param>
        /// <param name="rightSide"></param>
        internal void CreatePersonMotionUI(BaseScript self, bool rightSide = false)
        {
            _MotionSettingsUI = new GroupUI(self);
            var allJointsSettingsGroup = new GroupUI(self);
            var heelSettingsGroup = new GroupUI(self);

            _MotionSettingsUI.ChildGroups.Add(allJointsSettingsGroup);
            _MotionSettingsUI.ChildGroups.Add(heelSettingsGroup);

            // 生成动作选择器
            for (var i = 0; i < MAX_MOTION_COUNT; i++)
            {
                var motionChooser = self.SetupStringChooserNoLang($"{Lang.Get("Motion")} {(i + 1)}", self.noneStrings, rightSide: rightSide);
                motionChooser.setCallbackFunction = UpdateMotionValue;
                _MotionSettingsUI.OtherElements.Add(motionChooser);
                _MotionChoosers.Add(motionChooser);
            }

            // 显示动作设置
            var toggleJSON = Utils.SetupToggle(self, $"{GetLabelText("Show Motion Settings")}", false, v =>
            {
                _MotionSettingsUI.RefreshView(v);
            }, rightSide);

            _MotionSettingsUI.ToggleBool = toggleJSON;

            // 动作播放进度
            _ProgressJSON = Utils.SetupSliderFloat(self, Lang.Get("Progress"), 0f, 0f, 0f, v =>
            {
                // TODO 更新动作
                //_MmdPersonGameObject?.SetMotionPos(v, true, motionScale: motionScaleRate);
            }, rightSide);
            _MotionSettingsUI.Elements.Add(_ProgressJSON);

            _TimeDelayJSON = Utils.SetupSliderFloat(self, Lang.Get("Motion Delay"), 0f, 0f, 0f, v =>
            {
                // 设置时间延迟
                SetTimeDelay(v);
            }, rightSide);
            _MotionSettingsUI.Elements.Add(_TimeDelayJSON);

            // 是否启用表情
            _EnableFaceJSON = Utils.SetupToggle(self, $"{GetLabelText("Enable Face")}", true, v =>
            {
                _MotionSetting.IgnoreFace = !v;
                ReUpdateMotion();
            }, rightSide);
            _MotionSettingsUI.Elements.Add(_EnableFaceJSON);
            #region 位置设置UI

            //// 位置
            //_PositionX = Utils.SetupSliderFloatWithRange(self, $"{GetLabelText("Position X")}", 0f, -10f, 10f, v =>
            //{
            //    // 位置调整的处理
            //    //_MotionSetting.PositionX = v;
            //    UpdatePositionAndRotation();
            //}, rightSide, "F4");
            //_MotionSettingsUI.Elements.Add(_PositionX);

            //_PositionY = Utils.SetupSliderFloatWithRange(self, $"{GetLabelText("Position Y")}", 0f, -10f, 10f, v =>
            //{
            //    // 位置调整的处理
            //    //_MotionSetting.PositionY = v;
            //    UpdatePositionAndRotation();
            //}, rightSide, "F4");
            //_MotionSettingsUI.Elements.Add(_PositionY);

            //_PositionZ = Utils.SetupSliderFloatWithRange(self, $"{GetLabelText("Position Z")}", 0f, -10f, 10f, v =>
            //{
            //    // 位置调整的处理
            //    //_MotionSetting.PositionZ = v;
            //    UpdatePositionAndRotation();
            //}, rightSide, "F4");
            //_MotionSettingsUI.Elements.Add(_PositionZ);

            ////角度
            //_RotationX = Utils.SetupSliderFloat(self, $"{GetLabelText("Rotation X")}", 0f, -180f, 180f, v =>
            //{
            //    // 角度调整的处理
            //    //_MotionSetting.RotationX = v;
            //    UpdatePositionAndRotation();
            //}, rightSide);
            //_MotionSettingsUI.Elements.Add(_RotationX);

            //_RotationY = Utils.SetupSliderFloat(self, $"{GetLabelText("Rotation Y")}", 0f, -180f, 180f, v =>
            //{
            //    // 角度调整的处理
            //    //_MotionSetting.RotationY = v;
            //    UpdatePositionAndRotation();
            //}, rightSide);
            //_MotionSettingsUI.Elements.Add(_RotationY);

            //_RotationZ = Utils.SetupSliderFloat(self, $"{GetLabelText("Rotation Z")}", 0f, -180f, 180f, v =>
            //{
            //    // 角度调整的处理
            //    //_MotionSetting.RotationZ = v;
            //    UpdatePositionAndRotation();
            //}, rightSide);
            //_MotionSettingsUI.Elements.Add(_RotationZ);

            #endregion
            _MotionScaleJSON = Utils.SetupSliderFloat(self, $"{GetLabelText("Motion Scale")}", 1f, 0.1f, 2f, v =>
            {
                // 动作缩放的处理
                _MotionSetting.MotionScale = v;
                ReUpdateMotion();
            }, rightSide);
            _MotionSettingsUI.Elements.Add(_MotionScaleJSON);

            #region 全部关节设置
            _UseAllJointsSettingsJSON = Utils.SetupToggle(self, $"{GetLabelText("Use Joints Settings")}", false, v =>
            {
                // 处理开启所有关节设置
                _MotionSetting.UseAllJointsSettings = v;
                allJointsSettingsGroup.RefreshView(v);
                SetPersonAllJoints();
            }, rightSide);
            _MotionSettingsUI.Elements.Add(_UseAllJointsSettingsJSON);
            allJointsSettingsGroup.ToggleBool = _UseAllJointsSettingsJSON;

            _AllJointsSpringPercentJSON = Utils.SetupSliderFloat(self, $"{GetLabelText("Joints Spring Percent")}", config.AllJointsSpringPercent, 0f, 1f, v =>
            {
                // 处理关节弹簧百分比变更
                if (_MotionSetting.UseAllJointsSettings)
                {
                    _MotionSetting.AllJointsSpringPercent = v;
                    this.SetPersonAllJointsSpringPercent(v);
                }
            }, rightSide, "F4");
            allJointsSettingsGroup.Elements.Add(_AllJointsSpringPercentJSON);

            _AllJointsDamperPercentJSON = Utils.SetupSliderFloat(self, $"{GetLabelText("Joints Damper Percent")}", config.AllJointsDamperPercent, 0f, 1f, v =>
            {
                // 处理关节阻尼百分比变更
                if (_MotionSetting.UseAllJointsSettings)
                {
                    _MotionSetting.AllJointsDamperPercent = v;
                    this.SetPersonAllJointsDamperPercent(v);
                }
            }, rightSide, "F4");
            allJointsSettingsGroup.Elements.Add(_AllJointsDamperPercentJSON);

            _AllJointsMaxVelocityJSON = Utils.SetupSliderFloat(self, $"{GetLabelText("Joints Max Velocity")}", config.AllJointsMaxVelocity, 0f, 100f, v =>
            {
                // 处理关节最大速度变更
                if (_MotionSetting.UseAllJointsSettings)
                {
                    _MotionSetting.AllJointsMaxVelocity = v;
                    this.SetPersonAllJointsMaxVelocity(v);
                }
            }, rightSide, "F4");
            allJointsSettingsGroup.Elements.Add(_AllJointsMaxVelocityJSON);

            #endregion

            #region 骨骼旋转微调UI
            CreateBoneRotationAdjustUI(self, rightSide);
            #endregion

            #region 高跟设置UI

            // 是否启用高跟鞋
            _EnableHeelJSON = Utils.SetupToggle(self, $"{GetLabelText("Enable High Heel")}", false, v =>
            {
                heelSettingsGroup.RefreshView(v);
                UpdateEnableHighHeel();
            }, rightSide);
            _MotionSettingsUI.OtherElements.Add(_EnableHeelJSON);
            heelSettingsGroup.ToggleBool = _EnableHeelJSON;

            _HoldRotationMaxForceAdjust = Utils.SetupSliderFloat(self, $"{GetLabelText("Foot Hold Rotation Max Force")}", 0f, 0f, 1000f, v =>
            {
                UpateHeelJointDriveXAngle();
            }, rightSide);
            heelSettingsGroup.Elements.Add(_HoldRotationMaxForceAdjust);

            _FootJointDriveXTargetAdjust = Utils.SetupSliderFloat(self, $"{GetLabelText("Foot Joint Drive X Angle")}", -45f, -65f, 40f, v =>
            {
                this.UpateHeelJointDriveXAngle();
            }, rightSide);
            heelSettingsGroup.Elements.Add(_FootJointDriveXTargetAdjust);

            _ToeJointDriveXTargetAdjust = Utils.SetupSliderFloat(self, $"{GetLabelText("Toe Joint Drive X Angle")}", 35f, -40f, 75f, v =>
            {
                UpateHeelJointDriveXAngle();
            }, rightSide);
            heelSettingsGroup.Elements.Add(_ToeJointDriveXTargetAdjust);

            _HeelHeightAdjustJSON = Utils.SetupSliderFloat(self, $"{GetLabelText("Heel Height Fixing")}", 0.075f, 0f, 1f, v =>
            {
                UpateHeelJointDriveXAngle();
            }, rightSide, "F4");
            heelSettingsGroup.Elements.Add(_HeelHeightAdjustJSON);

            #endregion

            // 关闭下半身骨骼
            _CloseLowerBonesJSON = Utils.SetupToggle(self, $"{GetLabelText("Close Lower Bones")}", false, v =>
            {
                CloseLowerBones(v);
            }, rightSide);
            _MotionSettingsUI.OtherElements.Add(_CloseLowerBonesJSON);

            _MotionSettingsUI.RefreshView();
        }

        /// <summary>
        /// 更新动作值
        /// </summary>
        /// <param name="v"></param>
        void UpdateMotionValue(string v)
        {
            if (!string.IsNullOrEmpty(v))
            {
                // 选中动作文件的处理
                ReloadMotions(1);
            }
        }

        /// <summary>
        /// 移除人物动作UI
        /// </summary>
        /// <param name="controller"></param>
        /// <param name="person"></param>
        internal void RemovePersonMotionUI()
        {
            _MotionChoosers.Clear();
            _MotionSettingsUI.Clear();
        }

        /// <summary>
        /// 显示或隐藏人物动作UI
        /// </summary>
        /// <param name="show"></param>
        internal void ShowPersonMotionUI(bool show)
        {
            _MotionSettingsUI.Show(show);
        }

        /// <summary>
        /// 重置音频选择器
        /// </summary>
        void ResetChoosers()
        {
            SetChoosers(noneStrings, noneStrings, null);
        }

        /// <summary>
        /// 设置选择器
        /// </summary>
        /// <param name="displayChoices"></param>
        /// <param name="choices"></param>
        /// <param name="targets"></param>
        void SetChoosers(List<string> displayChoices, List<string> choices, List<string> targets)
        {
            for (var i = 0; i < _MotionChoosers.Count; i++)
            {
                var choice = noneString;

                if (targets?.Count > i)
                {
                    choice = targets[i];
                }

                var chooser = _MotionChoosers[i];
                chooser.valNoCallback = "";
                chooser.choices = choices.ToArray().ToList();
                chooser.displayChoices = displayChoices.ToArray().ToList();
                chooser.valNoCallback = choice;
            }

            ReloadMotions(2);
        }

        /// <summary>
        /// 初始化设定值
        /// </summary>
        void InitSettingValues()
        {
            if (_MotionSettingsUI == null)
            {
                return;
            }

            //// 忽略脸部
            _EnableFaceJSON.val = !_MotionSetting.IgnoreFace;
            _UseAllJointsSettingsJSON.val = _MotionSetting.UseAllJointsSettings;

            // 如果使用全部关节设置
            if (_MotionSetting.UseAllJointsSettings)
            {
                _AllJointsSpringPercentJSON.val = _MotionSetting.AllJointsSpringPercent;
                _AllJointsDamperPercentJSON.val = _MotionSetting.AllJointsDamperPercent;
                _AllJointsMaxVelocityJSON.val = _MotionSetting.AllJointsMaxVelocity;
            }

            //if (!config.LockPersonPosition)
            //{
            //    _PositionX.val = _MotionSetting.PositionX;
            //    _PositionY.val = _MotionSetting.PositionY;
            //    _PositionZ.val = _MotionSetting.PositionZ;

            //    _RotationX.val = _MotionSetting.RotationX;
            //    _RotationY.val = _MotionSetting.RotationY;
            //    _RotationZ.val = _MotionSetting.RotationZ;
            //}

            // 未关闭下半身骨骼的情况下更新动作缩放
            if (!_CloseLowerBonesJSON.val)
            {
                // 设置动作缩放
                _MotionScaleJSON.val = _MotionSetting.MotionScale;
            }
        }
    }
}
