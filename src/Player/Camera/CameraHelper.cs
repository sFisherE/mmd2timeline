using LibMMD.Motion;
using LibMMD.Unity3D;
using mmd2timeline.Store;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace mmd2timeline
{
    /// <summary>
    /// 镜头动作控制器
    /// </summary>
    internal partial class CameraHelper
    {
        /// <summary>
        /// 配置数据
        /// </summary>
        protected static readonly Config config = Config.GetInstance();

        /// <summary>
        /// 镜头设定
        /// </summary>
        CameraSetting _CameraSetting;

        /// <summary>
        /// MMD镜头对象
        /// </summary>
        MmdCameraObject _MmdCamera;

        #region 窗口相机
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
        #endregion

        #region 单例
        private static CameraHelper _instance;
        private static object _lock = new object();

        /// <summary>
        /// 镜头控制器的单例
        /// </summary>
        public static CameraHelper GetInstance()
        {
            lock (_lock)
            {
                if (_instance == null)
                {
                    _instance = new CameraHelper();
                }

                return _instance;
            }
        }
        #endregion

        /// <summary>
        /// 实例化MMDCameraAtom
        /// </summary>
        /// <param name="cameraAtom"></param>
        private CameraHelper()
        {
            Init();
        }

        /// <summary>
        /// 初始化
        /// </summary>
        private void Init()
        {
            try
            {
                GameObject root = new GameObject("mmd2timeline camera root");
                root.transform.localPosition = Vector3.zero;
                root.transform.localScale = new Vector3(0.1f, 0.1f, 0.1f);
                root.transform.localEulerAngles = new Vector3(0, 180, 0);

                _MmdCamera = MmdCameraObject.CreateGameObject();
                _MmdCamera.transform.SetParent(root.transform);
                _MmdCamera.transform.localPosition = Vector3.zero;
                _MmdCamera.transform.localScale = Vector3.one;
                _MmdCamera.transform.localRotation = Quaternion.identity;

                _MmdCamera.CameraUpdated += UpdateCamera;

                // 获取拥有者
                _possessor = SuperController.singleton.centerCameraTarget.transform.GetComponent<Possessor>();
            }
            catch (Exception ex)
            {
                LogUtil.LogError(ex, $"CameraMotionController::Init:");
            }
        }

        /// <summary>
        /// 初始化播放参数
        /// </summary>
        /// <param name="choices"></param>
        /// <param name="displayChoices"></param>
        /// <param name="choice"></param>
        /// <param name="settings"></param>
        internal void InitPlay(List<string> choices, List<string> displayChoices, string choice, CameraSetting settings)
        {
            _CameraSetting = settings;

            _delay = _CameraSetting?.TimeDelay ?? 0f;

            if (_delay != 0f)
            {
                // 先设定范围
                SetDelayRange(Mathf.Abs(_delay));

                SetTimeDelay(_delay);
            }
            else
            {
                // 重置延迟范围
                SetDelayRange(0f);
            }

            SetChooser(displayChoices, choices, settings?.CameraPath);

            InitSettingValues();
        }

        /// <summary>
        /// 清理播放数据
        /// </summary>
        public void Clear()
        {
            ResetChooser();
        }

        /// <summary>
        /// 导入VMD文件
        /// </summary>
        /// <param name="path"></param>
        /// <param name="audioSource"></param>
        private void ImportVmd(string path, string audioSource = "")
        {
            try
            {
                if (path == noneString)
                {
                    path = null;
                }

                // 初始化MMD镜头的本地位置
                this._MmdCamera.transform.localPosition = Vector3.zero;
                // 初始化MMD镜头的本地缩放
                this._MmdCamera.transform.localScale = Vector3.one;
                // 初始化MMD镜头的本地旋转
                this._MmdCamera.transform.localRotation = Quaternion.identity;
                // 加载MMD镜头数据文件
                this._MmdCamera.LoadCameraMotion(path);

                var cameraMotion = this._MmdCamera._cameraMotion;

                // 获取MMD镜头的最后一帧
                int key = 0;
                if (cameraMotion != null && cameraMotion.KeyFrames.Count > 0)
                {
                    key = cameraMotion.KeyFrames.Max(k => k.Key);
                }

                // 更新最大播放时长
                MaxTime = (float)((double)key / 30.0);

                OnCameraMotionLoaded?.Invoke(MaxTime);
            }
            catch (Exception ex)
            {
                LogUtil.LogError(ex);
            }
        }
    }
}
