using MacGruber;
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
        bool EnableHeel
        {
            get
            {
                if (_enableHeelJSON != null)
                    return _enableHeelJSON.val;
                return false;
            }
        }
        /// <summary>
        /// 获取UI是否已完成创建
        /// </summary>
        bool UICreated
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
        JSONStorableFloat _progressJSON;

        /// <summary>
        /// 延迟
        /// </summary>
        JSONStorableFloat _timeDelayJSON;

        /// <summary>
        /// 关闭下身骨骼的参数
        /// </summary>
        public JSONStorableBool CloseLowerBonesJSON;

        /// <summary>
        /// 是否使用全部关节设置
        /// </summary>
        public JSONStorableBool UseAllJointsSettingsJSON;
        /// <summary>
        /// 所有关节阻尼比例
        /// </summary>
        public JSONStorableFloat AllJointsDamperPercentJSON;
        /// <summary>
        /// 所有关节弹簧比例
        /// </summary>
        public JSONStorableFloat AllJointsSpringPercentJSON;
        /// <summary>
        /// 所有关节最大速度比例
        /// </summary>
        public JSONStorableFloat AllJointsMaxVelocityJSON;
        /// <summary>
        /// 动作缩放
        /// </summary>
        JSONStorableFloat _motionScaleJSON;
        /// <summary>
        /// 开启表情
        /// </summary>
        JSONStorableBool _enableFaceJSON;

        /// <summary>
        /// 开启高跟
        /// </summary>
        JSONStorableBool _enableHeelJSON;

        /// <summary>
        /// 脚部关节驱动X目标调整
        /// </summary>
        public JSONStorableFloat footJointDriveXTargetAdjust;

        /// <summary>
        /// 脚趾关节驱动X目标调整
        /// </summary>
        public JSONStorableFloat toeJointDriveXTargetAdjust;

        /// <summary>
        /// 保持角度最大力调整
        /// </summary>
        public JSONStorableFloat holdRotationMaxForceAdjust;

        /// <summary>
        /// 高跟高度修正
        /// </summary>
        JSONStorableFloat _heelHeightAdjustJSON;

        /// <summary>
        /// 动作选择器清单
        /// </summary>
        public List<JSONStorableStringChooser> MotionChoosers = new List<JSONStorableStringChooser>();

        /// <summary>
        /// 位置X
        /// </summary>
        public JSONStorableFloat positionX;
        /// <summary>
        /// 位置Y
        /// </summary>
        public JSONStorableFloat positionY;
        /// <summary>
        /// 位置Z
        /// </summary>
        public JSONStorableFloat positionZ;

        /// <summary>
        /// 方向X
        /// </summary>
        public JSONStorableFloat rotationX;
        /// <summary>
        /// 方向Y
        /// </summary>
        public JSONStorableFloat rotationY;
        /// <summary>
        /// 方向Z
        /// </summary>
        public JSONStorableFloat rotationZ;

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
                motionChooser.setCallbackFunction = v =>
                {
                    // TODO 选中动作文件的处理
                    ReloadMotions();
                };
                _MotionSettingsUI.OtherElements.Add(motionChooser);
                MotionChoosers.Add(motionChooser);
            }

            // 显示动作设置
            var toggleJSON = Utils.SetupToggle(self, $"{GetLabelText("Show Motion Settings")}", false, v =>
            {
                _MotionSettingsUI.RefreshView(v);
            }, rightSide);

            _MotionSettingsUI.ToggleBool = toggleJSON;

            // 动作播放进度
            _progressJSON = Utils.SetupSliderFloat(self, $"Progress", 0f, 0f, 0f, v =>
            {
                // TODO 更新动作
                //_MmdPersonGameObject?.SetMotionPos(v, true, motionScale: motionScaleRate);
            }, rightSide);
            _MotionSettingsUI.Elements.Add(_progressJSON);

            _timeDelayJSON = Utils.SetupSliderFloat(self, $"Motion {Lang.Get("Delay")}", 0f, 0f, 0f, v =>
            {
                // 设置时间延迟
                SetTimeDelay(v);
            }, rightSide);
            _MotionSettingsUI.Elements.Add(_timeDelayJSON);

            // 是否启用表情
            _enableFaceJSON = Utils.SetupToggle(self, $"{GetLabelText("Enable Face")}", true, v =>
            {
                _MotionSetting.IgnoreFace = !v;
                ReUpdateMotion();
            }, rightSide);
            _MotionSettingsUI.Elements.Add(_enableFaceJSON);
            #region 位置设置UI

            // 位置
            positionX = Utils.SetupSliderFloatWithRange(self, $"{GetLabelText("Position X")}", 0f, -10f, 10f, v =>
            {
                // TODO 位置调整的处理
                _MotionSetting.PositionX = v;
                UpdatePositionAndRotation();
            }, rightSide, "F4");
            _MotionSettingsUI.Elements.Add(positionX);

            positionY = Utils.SetupSliderFloatWithRange(self, $"{GetLabelText("Position Y")}", 0f, -10f, 10f, v =>
            {
                // TODO 位置调整的处理
                _MotionSetting.PositionY = v;
                UpdatePositionAndRotation();
            }, rightSide, "F4");
            _MotionSettingsUI.Elements.Add(positionY);

            positionZ = Utils.SetupSliderFloatWithRange(self, $"{GetLabelText("Position Z")}", 0f, -10f, 10f, v =>
            {
                // TODO 位置调整的处理
                _MotionSetting.PositionZ = v;
                UpdatePositionAndRotation();
            }, rightSide, "F4");
            _MotionSettingsUI.Elements.Add(positionZ);

            //角度
            rotationX = Utils.SetupSliderFloat(self, $"{GetLabelText("Rotation X")}", 0f, -180f, 180f, v =>
            {
                // TODO 角度调整的处理
                _MotionSetting.RotationX = v;
                UpdatePositionAndRotation();
            }, rightSide);
            _MotionSettingsUI.Elements.Add(rotationX);

            rotationY = Utils.SetupSliderFloat(self, $"{GetLabelText("Rotation Y")}", 0f, -180f, 180f, v =>
            {
                // TODO 角度调整的处理
                _MotionSetting.RotationY = v;
                UpdatePositionAndRotation();
            }, rightSide);
            _MotionSettingsUI.Elements.Add(rotationY);

            rotationZ = Utils.SetupSliderFloat(self, $"{GetLabelText("Rotation Z")}", 0f, -180f, 180f, v =>
            {
                // TODO 角度调整的处理
                _MotionSetting.RotationZ = v;
                UpdatePositionAndRotation();
            }, rightSide);
            _MotionSettingsUI.Elements.Add(rotationZ);

            #endregion
            _motionScaleJSON = Utils.SetupSliderFloat(self, $"{GetLabelText("Motion Scale")}", 1f, 0.1f, 2f, v =>
            {
                // TODO 动作缩放的处理
                _MotionSetting.MotionScale = v;
                ReUpdateMotion();
            }, rightSide);
            _MotionSettingsUI.Elements.Add(_motionScaleJSON);

            #region 全部关节设置
            UseAllJointsSettingsJSON = Utils.SetupToggle(self, $"{GetLabelText("Use Joints Settings")}", false, v =>
            {
                // TODO 处理开启所有关节设置
                _MotionSetting.UseAllJointsSettings = v;
                allJointsSettingsGroup.RefreshView(v);
                SetPersonAllJoints();
            }, rightSide);
            _MotionSettingsUI.Elements.Add(UseAllJointsSettingsJSON);
            allJointsSettingsGroup.ToggleBool = UseAllJointsSettingsJSON;

            AllJointsSpringPercentJSON = Utils.SetupSliderFloat(self, $"{GetLabelText("Joints Spring Percent")}", config.AllJointsSpringPercent, 0f, 1f, v =>
            {
                // TODO 处理关节弹簧百分比变更
                if (_MotionSetting.UseAllJointsSettings)
                {
                    _MotionSetting.AllJointsSpringPercent = v;
                    this.SetPersonAllJointsSpringPercent(v);
                }
            }, rightSide, "F4");
            allJointsSettingsGroup.Elements.Add(AllJointsSpringPercentJSON);

            AllJointsDamperPercentJSON = Utils.SetupSliderFloat(self, $"{GetLabelText("Joints Damper Percent")}", config.AllJointsDamperPercent, 0f, 1f, v =>
            {
                // TODO 处理关节阻尼百分比变更
                if (_MotionSetting.UseAllJointsSettings)
                {
                    _MotionSetting.AllJointsDamperPercent = v;
                    this.SetPersonAllJointsDamperPercent(v);
                }
            }, rightSide, "F4");
            allJointsSettingsGroup.Elements.Add(AllJointsDamperPercentJSON);

            AllJointsMaxVelocityJSON = Utils.SetupSliderFloat(self, $"{GetLabelText("Joints Max Velocity")}", config.AllJointsMaxVelocity, 0f, 100f, v =>
            {
                // TODO 处理关节最大速度变更
                if (_MotionSetting.UseAllJointsSettings)
                {
                    _MotionSetting.AllJointsMaxVelocity = v;
                    this.SetPersonAllJointsMaxVelocity(v);
                }
            }, rightSide, "F4");
            allJointsSettingsGroup.Elements.Add(AllJointsMaxVelocityJSON);

            #endregion

            #region 骨骼旋转微调UI
            CreateBoneRotationAdjustUI(self, rightSide);
            #endregion

            #region 高跟设置UI

            // 是否启用高跟鞋
            _enableHeelJSON = Utils.SetupToggle(self, $"{GetLabelText("Enable High Heel")}", false, v =>
            {
                heelSettingsGroup.RefreshView(v);
                UpdateEnableHighHeel();
            }, rightSide);
            _MotionSettingsUI.OtherElements.Add(_enableHeelJSON);
            heelSettingsGroup.ToggleBool = _enableHeelJSON;

            holdRotationMaxForceAdjust = Utils.SetupSliderFloat(self, $"{GetLabelText("Foot Hold Rotation Max Force")}", 0f, 0f, 1000f, v =>
            {
                UpateHeelJointDriveXAngle();
            }, rightSide);
            heelSettingsGroup.Elements.Add(holdRotationMaxForceAdjust);

            footJointDriveXTargetAdjust = Utils.SetupSliderFloat(self, $"{GetLabelText("Foot Joint Drive X Angle")}", -45f, -65f, 40f, v =>
            {
                this.UpateHeelJointDriveXAngle();
            }, rightSide);
            heelSettingsGroup.Elements.Add(footJointDriveXTargetAdjust);

            toeJointDriveXTargetAdjust = Utils.SetupSliderFloat(self, $"{GetLabelText("Toe Joint Drive X Angle")}", 35f, -40f, 75f, v =>
            {
                UpateHeelJointDriveXAngle();
            }, rightSide);
            heelSettingsGroup.Elements.Add(toeJointDriveXTargetAdjust);

            _heelHeightAdjustJSON = Utils.SetupSliderFloat(self, $"{GetLabelText("Heel Height Fixing")}", 0.075f, 0f, 1f, v =>
            {
                UpateHeelJointDriveXAngle();
            }, rightSide, "F4");
            heelSettingsGroup.Elements.Add(_heelHeightAdjustJSON);

            #endregion

            // 关闭下半身骨骼
            CloseLowerBonesJSON = Utils.SetupToggle(self, $"{GetLabelText("Close Lower Bones")}", false, v =>
            {
                CloseLowerBones(v);
            }, rightSide);
            _MotionSettingsUI.OtherElements.Add(CloseLowerBonesJSON);

            _MotionSettingsUI.RefreshView();
        }

        /// <summary>
        /// 移除人物动作UI
        /// </summary>
        /// <param name="controller"></param>
        /// <param name="person"></param>
        internal void RemovePersonMotionUI()
        {
            MotionChoosers.Clear();
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
            for (var i = 0; i < MotionChoosers.Count; i++)
            {
                var chooser = MotionChoosers[i];

                chooser.choices = choices.ToArray().ToList();
                chooser.displayChoices = displayChoices.ToArray().ToList();

                var choice = noneString;

                if (targets?.Count > i)
                {
                    choice = targets[i];
                }

                chooser.valNoCallback = choice;
            }

            ReloadMotions();
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
            _enableFaceJSON.val = !_MotionSetting.IgnoreFace;
            UseAllJointsSettingsJSON.val = _MotionSetting.UseAllJointsSettings;

            // 如果使用全部关节设置
            if (_MotionSetting.UseAllJointsSettings)
            {
                AllJointsSpringPercentJSON.val = _MotionSetting.AllJointsSpringPercent;
                AllJointsDamperPercentJSON.val = _MotionSetting.AllJointsDamperPercent;
                AllJointsMaxVelocityJSON.val = _MotionSetting.AllJointsMaxVelocity;
            }

            if (!config.LockPersonPosition)
            {
                positionX.val = _MotionSetting.PositionX;
                positionY.val = _MotionSetting.PositionY;
                positionZ.val = _MotionSetting.PositionZ;

                rotationX.val = _MotionSetting.RotationX;
                rotationY.val = _MotionSetting.RotationY;
                rotationZ.val = _MotionSetting.RotationZ;
            }

            //this.MakeReady();

            // 未关闭下半身骨骼的情况下更新动作缩放
            if (!CloseLowerBonesJSON.val)
            {
                // 设置动作缩放
                _motionScaleJSON.val = _MotionSetting.MotionScale;
            }
        }
    }
}
