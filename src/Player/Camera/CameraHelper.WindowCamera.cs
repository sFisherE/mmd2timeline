using UnityEngine;

namespace mmd2timeline
{
    internal partial class CameraHelper
    {
        /// <summary>
        /// 镜头控制器
        /// </summary>
        protected CameraControl _CameraControl;
        /// <summary>
        /// 镜头Transform
        /// </summary>
        protected Transform _CameraTransform;

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

                            _CameraControl = (CameraControl)_WindowCameraAtom.GetStorableByID("CameraControl");
                            _CameraTransform = _WindowCameraAtom.mainController.transform;

                            // 跳出
                            break;
                        }
                    }
                }
                return _WindowCameraAtom;
            }
        }
    }
}
