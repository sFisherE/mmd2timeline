using UnityEngine;

namespace mmd2timeline
{
    /// <summary>
    /// 导航器快照
    /// </summary>
    internal class NavigationRigSnapshot
    {
        private float _playerHeightAdjust;
        private Quaternion _rotation;
        private Vector3 _position;

        private Vector3 _monitorPosition;
        private Quaternion _monitorRotation;
        private Vector3 _monitorLocalScale;
        private float _monitorFOV;
        private bool _monitorOrthographic;

        /// <summary>
        /// 执行快照。保存当前镜头状态
        /// </summary>
        /// <returns></returns>
        public static NavigationRigSnapshot Snap()
        {
            var navigationRig = SuperController.singleton.navigationRig;
            var monitorCenterCamera = SuperController.singleton.MonitorCenterCamera;
            var monitorTransform = monitorCenterCamera.transform;
            return new NavigationRigSnapshot
            {
                _playerHeightAdjust = SuperController.singleton.playerHeightAdjust,
                _position = navigationRig.position,
                _rotation = navigationRig.rotation,
                _monitorPosition = monitorTransform.position,
                _monitorRotation = monitorTransform.rotation,
                _monitorLocalScale = monitorTransform.localScale,
                _monitorFOV = SuperController.singleton.monitorCameraFOV,
                _monitorOrthographic = monitorCenterCamera.orthographic,
            };
        }

        /// <summary>
        /// 恢复镜头状态
        /// </summary>
        public void Restore()
        {
            var navigationRig = SuperController.singleton.navigationRig;
            SuperController.singleton.playerHeightAdjust = _playerHeightAdjust;
            navigationRig.position = _position;
            navigationRig.rotation = _rotation;

            var monitorCenterCamera = SuperController.singleton.MonitorCenterCamera;
            var monitorTransform = monitorCenterCamera.transform;
            monitorTransform.position = _monitorPosition;
            monitorTransform.rotation = _monitorRotation;
            monitorTransform.localScale = _monitorLocalScale;
            SuperController.singleton.monitorCameraFOV = _monitorFOV;
            monitorCenterCamera.orthographic = _monitorOrthographic;
        }
    }
}
