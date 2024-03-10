using LibMMD.Motion;
using LibMMD.Reader;
using UnityEngine;

namespace LibMMD.Unity3D
{
    public class MmdCameraObject : MonoBehaviour
    {
        public CameraMotion _cameraMotion = null;
        public bool Playing = false;
        private double _playTime;
        private Camera _camera;
        public Camera[] cameras;
        /// <summary>
        /// 评估版
        /// </summary>
        public bool Evaluation = false;
        /// <summary>
        /// 新插值
        /// </summary>
        public bool NewInterpolate = true;
        //static MmdCameraObject s_Inst;
        //public static MmdCameraObject Inst
        //{
        //    get
        //    {
        //        if (s_Inst == null)
        //        {
        //            s_Inst = Camera.main.gameObject.AddComponent<MmdCameraObject>();
        //        }
        //        return s_Inst;
        //    }
        //}

        #region 镜头变动的事件
        public delegate void CameraUpdatedHandler(Vector3 position, Quaternion rotation, float fov, bool orthographic);

        public event CameraUpdatedHandler CameraUpdated;
        #endregion

        public static MmdCameraObject CreateGameObject(string name = "MMDCameraObject")
        {

            var obj = new GameObject(name);
            var mmdCameraObject = obj.AddComponent<MmdCameraObject>();

            var cameraObj = new GameObject("Camera");
            cameraObj.transform.SetParent(obj.transform);

            mmdCameraObject.m_CameraTf = cameraObj.transform;
            return mmdCameraObject;
        }
        public Transform m_CameraTf;
        public CameraControl m_CameraControl;
        public bool LoadCameraMotion(string path)
        {
            if (path == null)
            {
                _cameraMotion = null;
                return false;
            }
            _cameraMotion = new VmdReader2().ReadCameraMotion(path);
            if (_cameraMotion.KeyFrames.Count == 0)
            {
                return false;
            }
            ResetMotion();
            return true;
        }

        public void ResetMotion()
        {
            _playTime = 0.0;
            Playing = false;
            Refresh();
        }

        public void SetPlayPos(double pos)
        {
            _playTime = pos;
            Refresh();
        }

        private void Update()
        {
            if (!Playing || _cameraMotion == null)
            {
                return;
            }
            var deltaTime = Time.deltaTime;
            _playTime += deltaTime;
            Refresh();
        }
        Quaternion quat = new Quaternion(0, 1, 0, 0);
        public FreeControllerV3 m_Control;
        private void Refresh()
        {
            if (_cameraMotion == null)
            {
                return;
            }
            var cameraPose = _cameraMotion.GetCameraPose(_playTime, Evaluation, NewInterpolate);
            if (cameraPose == null)
            {
                return;
            }

            transform.localPosition = cameraPose.Position;/// 10;
            transform.localRotation = Quaternion.Euler(-180 / Mathf.PI * cameraPose.Rotation);// * quat;
            m_CameraTf.transform.localPosition = new Vector3(0.0f, 0.0f, cameraPose.FocalLength);///10);

            if (m_Control != null && m_CameraControl != null)
            {
                m_Control.transform.SetPositionAndRotation(m_CameraTf.position, m_CameraTf.rotation);
                m_CameraControl.cameraFOV = cameraPose.Fov;
            }

            // 触发镜头更新事件
            CameraUpdated?.Invoke(m_CameraTf.transform.position, m_CameraTf.transform.rotation, cameraPose.Fov, cameraPose.Orthographic);
        }
    }
}