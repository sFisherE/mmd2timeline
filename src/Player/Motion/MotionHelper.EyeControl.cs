using System;

namespace mmd2timeline
{
    internal partial class MotionHelper
    {
        #region 窗口相机
        /// <summary>
        /// WindowCamera原子
        /// </summary>
        private Atom _WindowCameraAtom;

        /// <summary>
        /// 获取WindowCamera原子
        /// </summary>
        protected Atom WindowCamera
        {
            get
            {
                if (_WindowCameraAtom == null)
                {
                    // 遍历所有原子，初始化镜头
                    foreach (var atom in SuperController.singleton.GetAtoms())
                    {
                        // 如果找到原子类型为WindowCamera的原子
                        if (atom.type == "WindowCamera")
                        {
                            // 将找到的原子赋值给窗口镜头原子变量
                            _WindowCameraAtom = atom;

                            //_CameraControl = (CameraControl)_WindowCameraAtom.GetStorableByID("CameraControl");
                            //_CameraTransform = _WindowCameraAtom.mainController.transform;

                            // 跳出
                            break;
                        }
                    }
                }
                return _WindowCameraAtom;
            }
        }
        #endregion

        /// <summary>
        /// 眼睛行为
        /// </summary>
        private EyesControl _eyeBehavior;
        /// <summary>
        /// 观看模式
        /// </summary>
        private EyesControl.LookMode _eyeBehaviorRestoreLookMode;

        /// <summary>
        /// 初始化眼动
        /// </summary>
        void InitEyeBehavior()
        {
            _eyeBehavior = (EyesControl)_PersonAtom.GetStorableByID("Eyes");
            if (_eyeBehavior == null) throw new NullReferenceException(nameof(_eyeBehavior));
            _eyeBehaviorRestoreLookMode = _eyeBehavior.currentLookMode;
        }

        /// <summary>
        /// 设置眼动目标
        /// </summary>
        void SetLookMode()
        {
            if (config.UseWindowCamera && config.AutoGazeToWindowCamera)
            {
                // 眼睛同步
                if (_eyeBehaviorRestoreLookMode == EyesControl.LookMode.Player && config.EnableCamera && config.CameraActive)
                {
                    _eyeBehavior.currentLookMode = EyesControl.LookMode.Target;

                    _eyeBehavior.lookAt = WindowCamera.mainController.transform;
                }
                else if (_eyeBehavior?.currentLookMode != _eyeBehaviorRestoreLookMode)
                {
                    _eyeBehavior.currentLookMode = _eyeBehaviorRestoreLookMode;
                }
            }
        }
    }
}
