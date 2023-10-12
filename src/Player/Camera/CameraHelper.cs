using LibMMD.Motion;
using LibMMD.Unity3D;
using System;
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
        /// 镜头动作加载完成的回调委托
        /// </summary>
        /// <param name="length">返回音频长度</param>
        public delegate void CameraMotionLoadedCallback(float length);

        /// <summary>
        /// 镜头动作加载完成的事件
        /// </summary>
        public event CameraMotionLoadedCallback OnCameraMotionLoaded;

        /// <summary>
        /// 配置数据
        /// </summary>
        protected static readonly Config config = Config.GetInstance();

        /// <summary>
        /// 镜头镜头vmd路径
        /// </summary>
        public string cameraVmdPath;

        /// <summary>
        /// 播放延迟
        /// </summary>
        float _delay = 0f;

        /// <summary>
        /// MMD镜头对象
        /// </summary>
        private MmdCameraObject _mmdCamera;

        /// <summary>
        /// 镜头数据对象
        /// </summary>
        CameraMotion _cameraMotion;

        /// <summary>
        /// 获取MMD镜头对象
        /// </summary>
        public MmdCameraObject MMDCamera
        {
            get
            {
                return _mmdCamera;
            }
        }

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
                // 创建MMD镜头对象，名称为MMDCameraObject
                //this._MmdCamera = MmdCameraObject.CreateGameObject("MMDCameraObject").GetComponent<MmdCameraObject>();

                GameObject root = new GameObject("mmd2timeline camera root");
                root.transform.localPosition = Vector3.zero;
                root.transform.localScale = new Vector3(0.1f, 0.1f, 0.1f);
                root.transform.localEulerAngles = new Vector3(0, 180, 0);

                _mmdCamera = MmdCameraObject.CreateGameObject();
                _mmdCamera.transform.SetParent(root.transform);
                _mmdCamera.transform.localPosition = Vector3.zero;
                _mmdCamera.transform.localScale = Vector3.one;
                _mmdCamera.transform.localRotation = Quaternion.identity;

                _mmdCamera.CameraUpdated += MmdCamera_CameraUpdated;
            }
            catch (Exception ex)
            {
                LogUtil.LogError(ex, $"CameraMotionController::Init:");
            }
        }

        /// <summary>
        /// 镜头更新事件处理方法
        /// </summary>
        /// <param name="position"></param>
        /// <param name="rotation"></param>
        /// <param name="fov"></param>
        /// <param name="orthographic"></param>
        private void MmdCamera_CameraUpdated(Vector3 position, Quaternion rotation, float fov, bool orthographic)
        {
            this.UpdateCamera(position, rotation, fov, orthographic);
        }

        /// <summary>
        /// 开始
        /// </summary>
        private void Start()
        {
        }

        /// <summary>
        /// 初始化播放参数
        /// </summary>
        /// <param name="choices"></param>
        /// <param name="displayChoices"></param>
        /// <param name="choice"></param>
        /// <param name="settings"></param>
        internal void InitPlay(float delay, float positionOffsetX, float positionOffsetY, float positionOffsetZ,float rotationOffsetX, float rotationOffsetY, float rotationOffsetZ)
        {
            _delay = delay;

            // 初始化镜头偏移设定值
            SetPositionOffset(positionOffsetX, positionOffsetY, positionOffsetZ);
            SetRotationOffset(rotationOffsetX, rotationOffsetY, rotationOffsetZ);
        }

        /// <summary>
        /// 清理播放数据
        /// </summary>
        public void Clear()
        {
        }

        /// <summary>
        /// 进度变化的处理
        /// </summary>
        /// <param name="value"></param>
        internal void SetProgress(float value)
        {
            // 调用MMDCamera的播放进度
            _mmdCamera.SetPlayPos((double)value/*, config.CameraOnlyKeyFrame*/);
        }

        /// <summary>
        /// 导入VMD文件
        /// </summary>
        /// <param name="path"></param>
        /// <param name="audioSource"></param>
        internal void ImportVmd(string path)
        {
            try
            {
                // 初始化MMD镜头的本地位置
                _mmdCamera.transform.localPosition = Vector3.zero;
                // 初始化MMD镜头的本地缩放
                _mmdCamera.transform.localScale = Vector3.one;
                // 初始化MMD镜头的本地旋转
                _mmdCamera.transform.localRotation = Quaternion.identity;
                // 加载MMD镜头数据文件
                _mmdCamera.LoadCameraMotion(path);

                this._cameraMotion = _mmdCamera._cameraMotion;

                // 获取MMD镜头的最后一帧
                int key = 0;
                if (_cameraMotion != null && _cameraMotion.KeyFrames.Count > 0)
                {
                    key = _cameraMotion.KeyFrames.Max(k => k.Key);
                }

                OnCameraMotionLoaded.Invoke((float)key / 30.0f);
            }
            catch (Exception ex)
            {
                LogUtil.LogError(ex);
            }
        }

        /// <summary>
        /// 清理
        /// </summary>
        public void Dispose()
        {
            this.Clear();

            DisposeFollow();

            _mmdCamera = null;

            _instance = null;
        }
    }
}
