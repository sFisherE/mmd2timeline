using System.Collections.Generic;
using System.Linq;

namespace mmd2timeline
{
    /// <summary>
    /// 保存动作模式值的结构体
    /// </summary>
    internal struct MotionModeValue
    {
        public int Mode;
        public float Spring;
        public float Damper;
    }

    /// <summary>
    /// 配置类。
    /// </summary>
    /// <remarks>此类中的数据除部分实时参数外，将被自动保存在数据文件中，每次插件加载时会从数据文件中重新加载数据。</remarks>
    internal partial class Config : MSJSONClass
    {
        /// <summary>
        /// 默认动作模式配置
        /// </summary>
        internal readonly List<MotionModeValue> MotionModes = new List<MotionModeValue> {
            new MotionModeValue { Mode = MotionMode.Fatigue,Spring = 0.1f,Damper=0.7f},
            new MotionModeValue { Mode = MotionMode.Weak,Spring = 0.15f,Damper=0.7f},
            new MotionModeValue { Mode = MotionMode.Smooth,Spring = 0.15f,Damper=0.5f},
            new MotionModeValue { Mode = MotionMode.Normal,Spring = 0.2f,Damper=0.2f},
            new MotionModeValue { Mode = MotionMode.Strong,Spring = 0.3f,Damper=0.5f},
            new MotionModeValue { Mode = MotionMode.Dexterity,Spring = 0.5f,Damper=0.2f},
            new MotionModeValue { Mode = MotionMode.Agile,Spring = 0.7f,Damper=0.2f}
        };

        /// <summary>
        /// 获取当前动作模式
        /// </summary>
        internal int CurrentMotionMode
        {
            get
            {
                if (this.MotionModes.Any(m => m.Damper == this.AllJointsDamperPercent && m.Spring == this.AllJointsSpringPercent))
                {
                    var mode = this.MotionModes.FirstOrDefault(m => m.Damper == this.AllJointsDamperPercent && m.Spring == this.AllJointsSpringPercent);

                    return mode.Mode;
                }

                return MotionMode.Custom;
            }
        }

        #region 镜头控制相关的设定

        /// <summary>
        /// 判定是否在VR中
        /// </summary>
        public bool IsInVR
        {
            get
            {
                return (SuperController.singleton.isOpenVR || SuperController.singleton.isOVR) && !SuperController.singleton.IsMonitorRigActive;
            }
        }

        ///// <summary>
        ///// 镜头是否只使用关键帧
        ///// </summary>
        //public bool CameraOnlyKeyFrame
        //{
        //    get
        //    {
        //        if (this.HasKey("CameraOnlyKeyFrame"))
        //            return this["CameraOnlyKeyFrame"].AsBool;
        //        else return false;
        //    }
        //    set
        //    {
        //        if (this.CameraOnlyKeyFrame != value)
        //        {
        //            this["CameraOnlyKeyFrame"].AsBool = value;
        //            this.Save();
        //        }
        //    }
        //}

        /// <summary>
        /// 获取是否启用镜头
        /// </summary>
        public bool EnableCamera
        {
            get
            {
                if (this.IsInVR)
                {
                    return this.CameraEnabledInVR;
                }
                else
                {
                    return this.CameraEnabled;
                }
            }
        }

        /// <summary>
        /// 是否启用镜头
        /// </summary>
        public bool CameraEnabled
        {
            get
            {
                if (this.HasKey("CameraEnabled"))
                    return this["CameraEnabled"].AsBool;
                else return true;
            }
            set
            {
                if (this.CameraEnabled != value)
                {
                    this["CameraEnabled"].AsBool = value;
                    this.Save();
                }
            }
        }

        /// <summary>
        /// 是否启用镜头FOV
        /// </summary>
        public bool CameraFOVEnabled
        {
            get
            {
                if (this.HasKey("CameraFOVEnabled"))
                    return this["CameraFOVEnabled"].AsBool;
                else return true;
            }
            set
            {
                if (this.CameraFOVEnabled != value)
                {
                    this["CameraFOVEnabled"].AsBool = value;
                    this.Save();
                }
            }
        }

        ///// <summary>
        ///// 是否镜头FOV比例
        ///// </summary>
        //public float CameraFOVRate
        //{
        //    get
        //    {
        //        if (this.HasKey("CameraFOVRate"))
        //            return this["CameraFOVRate"].AsFloat;
        //        else return 1f;
        //    }
        //    set
        //    {
        //        if (this.CameraFOVRate != value)
        //        {
        //            this["CameraFOVRate"].AsFloat = value;
        //            this.Save();
        //        }
        //    }
        //}

        /// <summary>
        /// 是否在窗口相机模式控制眼睛行为
        /// </summary>
        public bool AutoGazeToWindowCamera
        {
            get
            {
                if (this.HasKey("AutoGazeToWindowCamera"))
                    return this["AutoGazeToWindowCamera"].AsBool;
                else return true;
            }
            set
            {
                if (this.AutoGazeToWindowCamera != value)
                {
                    this["AutoGazeToWindowCamera"].AsBool = value;
                    this.Save();
                }
            }
        }

        /// <summary>
        /// 在VR下是否启用镜头
        /// </summary>
        public bool CameraEnabledInVR
        {
            get
            {
                if (this.HasKey("CameraEnabledInVR"))
                    return this["CameraEnabledInVR"].AsBool;
                else return false;
            }
            set
            {
                if (this.CameraEnabledInVR != value)
                {
                    this["CameraEnabledInVR"].AsBool = value;
                    this.Save();
                }
            }
        }

        /// <summary>
        /// 当主HUD打开时不激活镜头
        /// </summary>
        public bool DeactiveCameraWhenMainHUDOpened
        {
            get
            {
                if (this.HasKey("DeactiveCameraWhenMainHUDOpened"))
                    return this["DeactiveCameraWhenMainHUDOpened"].AsBool;
                else
                    return true;
            }
            set
            {
                if (this.DeactiveCameraWhenMainHUDOpened != value)
                {
                    this["DeactiveCameraWhenMainHUDOpened"].AsBool = value;
                    this.Save();
                }
            }
        }

        float? _CameraPositionSmoothing;
        /// <summary>
        /// 位置变动平滑
        /// </summary>
        public float CameraPositionSmoothing
        {
            get
            {
                if (!_CameraPositionSmoothing.HasValue && this.HasKey("CameraPositionSmoothing"))
                {
                    _CameraPositionSmoothing = this["CameraPositionSmoothing"].AsFloat;
                }

                if (_CameraPositionSmoothing.HasValue)
                {
                    return _CameraPositionSmoothing.Value;
                }
                else return 0.0f;//默认值
            }
            set
            {
                if (CameraPositionSmoothing != value)
                {
                    _CameraPositionSmoothing = value;
                    this["CameraPositionSmoothing"].AsFloat = value;
                    this.Save();
                }
            }
        }

        float? _CameraRotationSmoothing;
        /// <summary>
        /// 方向变动平滑
        /// </summary>
        public float CameraRotationSmoothing
        {
            get
            {
                if (!_CameraRotationSmoothing.HasValue && this.HasKey("CameraRotationSmoothing"))
                {
                    _CameraRotationSmoothing = this["CameraRotationSmoothing"].AsFloat;
                }

                if (_CameraRotationSmoothing.HasValue)
                {
                    return (_CameraRotationSmoothing.Value);
                }
                else
                    return 0.0f;
            }
            set
            {
                if (this.CameraRotationSmoothing != value)
                {
                    _CameraRotationSmoothing = value;
                    this["CameraRotationSmoothing"].AsFloat = value;
                    this.Save();
                }
            }
        }

        /// <summary>
        /// 获取是否使用原始相机
        /// </summary>
        internal bool UseOriginalCamera
        {
            get
            {
                return this.CameraControlMode == CameraControlModes.Original;
            }
        }

        /// <summary>
        /// 镜头模式
        /// </summary>
        public int CameraControlMode
        {
            get
            {
                if (this.HasKey("CameraControlMode"))
                    return this["CameraControlMode"].AsInt;
                else
                    return CameraControlModes.Custom;
            }
            set
            {
                if (this.CameraControlMode != value)
                {
                    this["CameraControlMode"].AsInt = value;
                    this.Save();
                }
            }
        }

        #endregion

        #region 动作控制相关的设置

        /// <summary>
        /// 动作位置状态
        /// </summary>
        public FreeControllerV3.PositionState MotionPositionState
        {
            get
            {
                if (this.HasKey("MotionPositionState"))
                    return (FreeControllerV3.PositionState)this["MotionPositionState"].AsInt;
                else return FreeControllerV3.PositionState.On;
            }
            set
            {
                if (this.MotionPositionState != value)
                {
                    this["MotionPositionState"].AsInt = (int)value;
                    this.Save();
                }
            }
        }

        /// <summary>
        /// 动作方向状态
        /// </summary>
        public FreeControllerV3.RotationState MotionRotationState
        {
            get
            {
                if (this.HasKey("MotionRotationState"))
                    return (FreeControllerV3.RotationState)this["MotionRotationState"].AsInt;
                else return FreeControllerV3.RotationState.On;
            }
            set
            {
                if (this.MotionRotationState != value)
                {
                    this["MotionRotationState"].AsInt = (int)value;
                    this.Save();
                }
            }
        }

        ///// <summary>
        ///// 全局动作缩放
        ///// </summary>
        //public float GlobalMotionScale
        //{
        //    get
        //    {
        //        if (this.HasKey("GlobalMotionScale"))
        //            return this["GlobalMotionScale"].AsFloat;
        //        else return 1f;
        //    }
        //    set
        //    {
        //        if (this.GlobalMotionScale != value)
        //        {
        //            this["GlobalMotionScale"].AsFloat = value;
        //            this.Save();
        //        }
        //    }
        //}

        /// <summary>
        /// 全局弹簧百分比
        /// </summary>
        public float AllJointsSpringPercent
        {
            get
            {
                if (this.HasKey("AllJointsSpringPercent"))
                    return this["AllJointsSpringPercent"].AsFloat;
                else return 0.2f;
            }
            set
            {
                if (this.AllJointsSpringPercent != value)
                {
                    this["AllJointsSpringPercent"].AsFloat = value;
                    this.Save();
                }
            }
        }

        /// <summary>
        /// 全局阻尼百分比
        /// </summary>
        public float AllJointsDamperPercent
        {
            get
            {
                if (this.HasKey("AllJointsDamperPercent"))
                    return this["AllJointsDamperPercent"].AsFloat;
                else return 0.2f;
            }
            set
            {
                if (this.AllJointsDamperPercent != value)
                {
                    this["AllJointsDamperPercent"].AsFloat = value;
                    this.Save();
                }
            }
        }

        /// <summary>
        /// 全局最大关节速度百分比
        /// </summary>
        public float AllJointsMaxVelocity
        {
            get
            {
                if (this.HasKey("AllJointsMaxVelocity"))
                    return this["AllJointsMaxVelocity"].AsFloat;
                else return 0.5f;
            }
            set
            {
                if (this.AllJointsMaxVelocity != value)
                {
                    this["AllJointsMaxVelocity"].AsFloat = value;
                    this.Save();
                }
            }
        }
        #endregion

        #region 软体物理相关的设置
        /// <summary>
        /// 指示是否关闭嘴部物理网格
        /// </summary>
        public bool MouthPhysicsMesh
        {
            get
            {
                if (this.HasKey("MouthPhysicsMesh"))
                {
                    return this["MouthPhysicsMesh"].AsBool;
                }
                else return false;
            }
            set
            {
                if (this.MouthPhysicsMesh != value)
                {
                    this["MouthPhysicsMesh"].AsBool = value;
                    this.Save();
                }
            }
        }

        /// <summary>
        /// 指示是否关闭臀部物理网格
        /// </summary>
        public bool LowerPhysicsMesh
        {
            get
            {
                if (this.HasKey("LowerPhysicsMesh"))
                {
                    return this["LowerPhysicsMesh"].AsBool;
                }
                else return true;
            }
            set
            {
                if (this.LowerPhysicsMesh != value)
                {
                    this["LowerPhysicsMesh"].AsBool = value;
                    this.Save();
                }
            }
        }

        /// <summary>
        /// 指示是否关闭胸部物理网格
        /// </summary>
        public bool BreastPhysicsMesh
        {
            get
            {
                if (this.HasKey("BreastPhysicsMesh"))
                {
                    return this["BreastPhysicsMesh"].AsBool;
                }
                else return true;
            }
            set
            {
                if (this.BreastPhysicsMesh != value)
                {
                    this["BreastPhysicsMesh"].AsBool = value;
                    this.Save();
                }
            }
        }


        #endregion

        #region AutoCorrectYHeight
        /// <summary>
        /// 自动修正高度模式
        /// </summary>
        public int AutoFixHeightMode
        {
            get
            {
                if (this.HasKey("AutoFixHeightMode"))
                {
                    return this["AutoFixHeightMode"].AsInt;
                }
                else
                {
                    return AutoCorrectHeightMode.None;
                }
            }
            set
            {
                if (this.AutoFixHeightMode != value)
                {
                    this["AutoFixHeightMode"].AsInt = (int)value;
                    this.Save();
                }
            }
        }

        /// <summary>
        /// 自动修正Y轴高度，低于此高度的将被修正
        /// </summary>
        public float AutoCorrectFixHeight
        {
            get
            {
                if (this.HasKey("AutoCorrectFixHeight"))
                {
                    return this["AutoCorrectFixHeight"].AsFloat;
                }
                else return 0.2f;
            }
            set
            {
                if (this.AutoCorrectFixHeight != value)
                {
                    this["AutoCorrectFixHeight"].AsFloat = value;
                    this.Save();
                }
            }
        }

        /// <summary>
        /// 自动修正Y轴高度的地板高度
        /// </summary>
        public float AutoCorrectFloorHeight
        {
            get
            {
                if (this.HasKey("AutoCorrectFloorHeight"))
                {
                    return this["AutoCorrectFloorHeight"].AsFloat;
                }
                else return 0.0005f;
            }
            set
            {
                if (this.AutoCorrectFloorHeight != value)
                {
                    this["AutoCorrectFloorHeight"].AsFloat = value;
                    this.Save();
                }
            }
        }

        #endregion

        #region FreeFoot
        /// <summary>
        /// 允许脚部放松
        /// </summary>
        public bool EnableFootFree
        {
            get
            {
                if (this.HasKey("EnableFootFree"))
                {
                    return this["EnableFootFree"].AsBool;
                }
                else return false;
            }
            set
            {
                if (this.EnableFootFree != value)
                {
                    this["EnableFootFree"].AsBool = value;
                    this.Save();
                }
            }
        }

        /// <summary>
        /// 允许脚部关闭控制
        /// </summary>
        public bool EnableFootOff
        {
            get
            {
                if (this.HasKey("EnableFootOff"))
                {
                    return this["EnableFootOff"].AsBool;
                }
                else return false;
            }
            set
            {
                if (this.EnableFootOff != value)
                {
                    this["EnableFootOff"].AsBool = value;
                    this.Save();
                }
            }
        }

        /// <summary>
        /// 脚趾关闭高度
        /// </summary>
        public float ToeOffHeight
        {
            get
            {
                if (this.HasKey("ToeOffHeight"))
                {
                    return this["ToeOffHeight"].AsFloat;
                }
                else return 0.05f;
            }
            set
            {
                if (this.ToeOffHeight != value)
                {
                    this["ToeOffHeight"].AsFloat = value;
                    this.Save();
                }
            }
        }

        /// <summary>
        /// 脚部修正高度
        /// </summary>
        public float FootFixHeight
        {
            get
            {
                if (this.HasKey("FootFixHeight"))
                {
                    return this["FootFixHeight"].AsFloat;
                }
                else return 0.15f;
            }
            set
            {
                if (this.FootFixHeight != value)
                {
                    this["FootFixHeight"].AsFloat = value;
                    this.Save();
                }
            }
        }

        /// <summary>
        /// 脚部位置关闭高度
        /// </summary>
        public float FootOffHeight
        {
            get
            {
                if (this.HasKey("FootOffHeight"))
                {
                    return this["FootOffHeight"].AsFloat;
                }
                else return 0.3f;
            }
            set
            {
                if (this.FootOffHeight != value)
                {
                    this["FootOffHeight"].AsFloat = value;
                    this.Save();
                }
            }
        }

        /// <summary>
        /// 脚部放松关节驱动X目标
        /// </summary>
        public float FreeFootJointDriveXTarget
        {
            get
            {
                if (this.HasKey("FreeFootJointDriveXTarget"))
                {
                    return this["FreeFootJointDriveXTarget"].AsFloat;
                }
                else return -45f;
            }
            set
            {
                if (this.FreeFootJointDriveXTarget != value)
                {
                    this["FreeFootJointDriveXTarget"].AsFloat = value;
                    this.Save();
                }
            }
        }

        /// <summary>
        /// 脚部放松保持角度最大力
        /// </summary>
        public float FreeFootHoldRotationMaxForce
        {
            get
            {
                if (this.HasKey("FreeFootHoldRotationMaxForce"))
                {
                    return this["FreeFootHoldRotationMaxForce"].AsFloat;
                }
                else return 20f;
            }
            set
            {
                if (this.FreeFootHoldRotationMaxForce != value)
                {
                    this["FreeFootHoldRotationMaxForce"].AsFloat = value;
                    this.Save();
                }
            }
        }

        /// <summary>
        /// 允许关闭膝盖
        /// </summary>
        public bool EnableKneeOff
        {
            get
            {
                if (this.HasKey("EnableKneeOff"))
                {
                    return this["EnableKneeOff"].AsBool;
                }
                else return false;
            }
            set
            {
                if (this.EnableKneeOff != value)
                {
                    this["EnableKneeOff"].AsBool = value;
                    this.Save();
                }
            }
        }

        /// <summary>
        /// 放松膝盖保持角度最大力
        /// </summary>
        public float FreeKneeHoldRotationMaxForce
        {
            get
            {
                if (this.HasKey("FreeKneeHoldRotationMaxForce"))
                {
                    return this["FreeKneeHoldRotationMaxForce"].AsFloat;
                }
                else return 50f;
            }
            set
            {
                if (this.FreeKneeHoldRotationMaxForce != value)
                {
                    this["FreeKneeHoldRotationMaxForce"].AsFloat = value;
                    this.Save();
                }
            }
        }

        /// <summary>
        /// 放松大腿保持角度最大力
        /// </summary>
        public float FreeThighHoldRotationMaxForce
        {
            get
            {
                if (this.HasKey("FreeThighHoldRotationMaxForce"))
                {
                    return this["FreeThighHoldRotationMaxForce"].AsFloat;
                }
                else return 200f;
            }
            set
            {
                if (this.FreeThighHoldRotationMaxForce != value)
                {
                    this["FreeThighHoldRotationMaxForce"].AsFloat = value;
                    this.Save();
                }
            }
        }

        #endregion

        /// <summary>
        /// 音量
        /// </summary>
        public float Volume
        {
            get
            {
                if (this.HasKey("Volume"))
                {
                    return this["Volume"].AsFloat;
                }
                else return 1f;
            }
            set
            {
                if (this.Volume != value)
                {
                    this["Volume"].AsFloat = value;
                    this.Save();
                }
            }
        }

        #region AutoCheckDuplicateFiles

        /// <summary>
        /// 是否自动检查重复文件
        /// </summary>
        public bool AutoCheckDuplicates
        {
            get
            {
                if (this.HasKey("AutoCheckDuplicates"))
                {
                    return this["AutoCheckDuplicates"].AsBool;
                }
                else return true;
            }
            set
            {
                if (this.AutoCheckDuplicates != value)
                {
                    this["AutoCheckDuplicates"].AsBool = value;
                    this.Save();
                }
            }
        }

        /// <summary>
        /// 是否在发现重复文件时停止播放
        /// </summary>
        public bool StopPlayWhenDuplicatesFound
        {
            get
            {
                if (this.HasKey("StopPlayWhenDuplicatesFound"))
                {
                    return this["StopPlayWhenDuplicatesFound"].AsBool;
                }
                else return false;
            }
            set
            {
                if (this.StopPlayWhenDuplicatesFound != value)
                {
                    this["StopPlayWhenDuplicatesFound"].AsBool = value;
                    this.Save();
                }
            }
        }

        #endregion

        /// <summary>
        /// 每次加载动作时重置物理
        /// </summary>
        public bool ResetPhysicalWhenLoadMotion
        {
            get
            {
                if (this.HasKey("ResetPhysicalWhenLoadMotion"))
                {
                    return this["ResetPhysicalWhenLoadMotion"].AsBool;
                }
                else return true;
            }
            set
            {
                if (this.ResetPhysicalWhenLoadMotion != value)
                {
                    this["ResetPhysicalWhenLoadMotion"].AsBool = value;
                    this.Save();
                }
            }
        }

        ///// <summary>
        ///// 锁定人物位置
        ///// </summary>
        //public bool LockPersonPosition
        //{
        //    get
        //    {
        //        if (this.HasKey("LockPersonPosition"))
        //        {
        //            return this["LockPersonPosition"].AsBool;
        //        }
        //        else return false;
        //    }
        //    set
        //    {
        //        if (this.LockPersonPosition != value)
        //        {
        //            this["LockPersonPosition"].AsBool = value;
        //            this.Save();
        //        }
        //    }
        //}

        /// <summary>
        /// 是否使用窗口摄像机
        /// </summary>
        public bool UseWindowCamera
        {
            get
            {
                if (this.IsInVR)
                {
                    return false;
                }

                return UseOriginalCamera || (this.SyncWindowCamera);
            }
        }

        /// <summary>
        /// 同步WindowCarmera
        /// </summary>
        public bool SyncWindowCamera
        {
            get
            {
                if (this.HasKey("SyncWindowCamera"))
                {
                    return this["SyncWindowCamera"].AsBool;
                }
                else return false;
            }
            set
            {
                if (this.SyncWindowCamera != value)
                {
                    this["SyncWindowCamera"].AsBool = value;
                    this.Save();
                }
            }
        }

        #region 快捷键管理
        /// <summary>
        /// 开启空格切换播放/暂停
        /// </summary>
        public bool EnableSpacePlay
        {
            get
            {
                if (this.HasKey("EnableSpacePlay"))
                {
                    return this["EnableSpacePlay"].AsBool;
                }
                else return true;
            }
            set
            {
                if (this.EnableSpacePlay != value)
                {
                    this["EnableSpacePlay"].AsBool = value;
                    this.Save();
                }
            }
        }
        /// <summary>
        /// 开启右箭头前进1秒
        /// </summary>
        public bool EnableRightArrow
        {
            get
            {
                if (this.HasKey("EnableRightArrow"))
                {
                    return this["EnableRightArrow"].AsBool;
                }
                else return true;
            }
            set
            {
                if (this.EnableRightArrow != value)
                {
                    this["EnableRightArrow"].AsBool = value;
                    this.Save();
                }
            }
        }
        /// <summary>
        /// 开启左箭头后退1秒
        /// </summary>
        public bool EnableLeftArrow
        {
            get
            {
                if (this.HasKey("EnableLeftArrow"))
                {
                    return this["EnableLeftArrow"].AsBool;
                }
                else return true;
            }
            set
            {
                if (this.EnableLeftArrow != value)
                {
                    this["EnableLeftArrow"].AsBool = value;
                    this.Save();
                }
            }
        }

        /// <summary>
        /// 开启上箭头切上一首
        /// </summary>
        public bool EnableUpArrow
        {
            get
            {
                if (this.HasKey("EnableUpArrow"))
                {
                    return this["EnableUpArrow"].AsBool;
                }
                else return true;
            }
            set
            {
                if (this.EnableUpArrow != value)
                {
                    this["EnableUpArrow"].AsBool = value;
                    this.Save();
                }
            }
        }
        /// <summary>
        /// 开启下箭头切下一首
        /// </summary>
        public bool EnableDownArrow
        {
            get
            {
                if (this.HasKey("EnableDownArrow"))
                {
                    return this["EnableDownArrow"].AsBool;
                }
                else return true;
            }
            set
            {
                if (this.EnableDownArrow != value)
                {
                    this["EnableDownArrow"].AsBool = value;
                    this.Save();
                }
            }
        }

        #endregion

        /// <summary>
        /// 启用初始动作调整
        /// </summary>
        public bool EnableInitialMotionAdjustment
        {
            get
            {
                if (this.HasKey("EnableInitialMotionAdjustment"))
                {
                    return this["EnableInitialMotionAdjustment"].AsBool;
                }
                else return true;
            }
            set
            {
                if (this.EnableInitialMotionAdjustment != value)
                {
                    this["EnableInitialMotionAdjustment"].AsBool = value;
                    this.Save();
                }
            }
        }

        /// <summary>
        /// 允许跪姿修正
        /// </summary>
        public bool EnableKneeingCorrections
        {
            get
            {
                if (this.HasKey("EnableKneeingCorrections"))
                {
                    return this["EnableKneeingCorrections"].AsBool;
                }
                else return false;
            }
            set
            {
                if (this.EnableKneeingCorrections != value)
                {
                    this["EnableKneeingCorrections"].AsBool = value;
                    this.Save();
                }
            }
        }

        /// <summary>
        /// 重置所有设置
        /// </summary>
        public void ResetAll()
        {
            var keys = this.Keys.ToArray();

            foreach (var key in keys)
            {
                this.Remove(key);
            }

            this.Save();
        }

        private Config() { }

        /// <summary>
        /// 配置文件保存路径
        /// </summary>
        private static string ConfigFilePath
        {
            get
            {
                return Config.saveDataPath + "\\config.json";
            }
        }
        private static Config _instance;
        private static object _lock = new object();
        private static Config _default;
        /// <summary>
        /// 获取配置
        /// </summary>
        public static Config GetInstance()
        {
            lock (_lock)
            {
                if (_instance == null)
                {
                    _instance = new Config();

                    _instance.Load(ConfigFilePath);
                }

                return _instance;
            }
        }

        /// <summary>
        /// 获取默认值
        /// </summary>
        /// <returns></returns>
        public static Config GetDefault()
        {
            lock (_lock)
            {
                if (_default == null)
                {
                    _default = new Config();
                }

                return _default;
            }
        }

        protected override void BeforeToString()
        {

        }
    }
}
