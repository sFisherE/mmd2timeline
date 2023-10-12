using System.Linq;
using UnityEngine;

namespace mmd2timeline
{
    /// <summary>
    /// 镜头动作管理器
    /// </summary>
    internal partial class CameraHelper
    {
        #region 聚焦相关的代码

        /// <summary>
        /// 是否聚焦在原子上
        /// </summary>
        private bool _FocusOnAtom = false;

        /// <summary>
        /// 聚焦原子
        /// </summary>
        Atom _FocusAtom;

        /// <summary>
        /// 聚焦部位ID
        /// </summary>
        string _FocusTargetId;

        /// <summary>
        /// 目标之前的位置
        /// </summary>
        Vector3 targetPastPosition = Vector3.zero;
        /// <summary>
        /// 目标位置
        /// </summary>
        Vector3 targetPosition;
        Quaternion pastRotation;

        /// <summary>
        /// 设置聚焦目标
        /// </summary>
        /// <param name="focusAtom"></param>
        /// <param name="tagetId"></param>
        internal void SetFocusTarget(Atom focusAtom, string tagetId)
        {
            _FocusAtom = focusAtom;
            _FocusTargetId = tagetId;
        }

        /// <summary>
        /// 聚焦
        /// </summary>
        bool FocusOn()
        {
            if (_FocusOnAtom && _FocusAtom != null && !string.IsNullOrEmpty(_FocusTargetId))
            {
                var target = _FocusAtom.freeControllers.FirstOrDefault(c => c.name == _FocusTargetId);//.GetStorableByID(FocusReceiverJSON.val).transform.position;

                if (Config.GetInstance().UseWindowCamera)
                {
                    _CameraTransform.LookAt(target.transform);
                }
                else
                {
                    SuperController.singleton.FocusOnController(target);
                }
                return true;
            }
            return false;
        }

        /// <summary>
        /// 聚焦到控制器
        /// </summary>
        /// <param name="controller"></param>
        /// <param name="up"></param>
        void FocusOnController(FreeControllerV3 controller, Vector3 up)
        {
            if (!(SuperController.singleton.MonitorCenterCamera != null) || !(controller != null))
            {
                return;
            }

            SuperController.singleton.AlignRigFacingController(controller, true);
            SuperController.singleton.SyncMonitorRigPosition();
            Vector3 position;
            if (controller.focusPoint != null)
            {
                position = controller.focusPoint.position;
                SuperController.singleton.focusDistance = (controller.focusPoint.position - SuperController.singleton.MonitorCenterCamera.transform.position).magnitude;
            }
            else
            {
                position = controller.transform.position;
                SuperController.singleton.focusDistance = (controller.transform.position - SuperController.singleton.MonitorCenterCamera.transform.position).magnitude;
            }

            if (SuperController.singleton.MonitorCenterCamera != null)
            {
                SuperController.singleton.MonitorCenterCamera.transform.LookAt(position, up);
                Vector3 localEulerAngles = SuperController.singleton.MonitorCenterCamera.transform.localEulerAngles;
                localEulerAngles.y = 0f;
                localEulerAngles.z = 0f;
                SuperController.singleton.MonitorCenterCamera.transform.localEulerAngles = localEulerAngles;
            }

            SuperController.singleton.SyncMonitorRigPosition();
        }
        #endregion
    }
}
