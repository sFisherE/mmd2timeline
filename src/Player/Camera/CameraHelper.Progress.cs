using System;
using UnityEngine;

namespace mmd2timeline
{
    internal partial class CameraHelper
    {
        /// <summary>
        /// 镜头动作加载完成的回调委托
        /// </summary>
        /// <param name="length">镜头动作长度</param>
        public delegate void CameraMotionLoadedCallback(float length);

        /// <summary>
        /// 镜头动作加载完成的事件
        /// </summary>
        public event CameraMotionLoadedCallback OnCameraMotionLoaded;

        /// <summary>
        /// 最大时间
        /// </summary>
        float maxTime = 0f;

        /// <summary>
        /// 最大时间
        /// </summary>
        internal float MaxTime
        {
            get
            {
                return maxTime;
            }
            set
            {
                maxTime = value;

                if (_progressJSON != null)
                    _progressJSON.max = value;

                // 设置延迟范围
                SetDelayRange(value);
            }
        }

        /// <summary>
        /// 获取镜头是否有动作数据
        /// </summary>
        internal bool HasMotion
        {
            get { return maxTime > 0f; }
        }

        /// <summary>
        /// 检查是否启用了延迟设定
        /// </summary>
        internal bool IsDelay
        {
            get
            {
                return _CameraSetting?.TimeDelay != 0f;
            }
        }

        /// <summary>
        /// 设定的延迟时间
        /// </summary>
        float _delay = 0f;

        /// <summary>
        /// 设置延迟时间
        /// </summary>
        /// <param name="delay"></param>
        internal virtual void SetTimeDelay(float delay)
        {
            // 更新设定条但不触发回调含税
            _timeDelayJSON.valNoCallback = delay;

            // 更新数据
            _CameraSetting.TimeDelay = delay;
        }

        /// <summary>
        /// 获得延迟的进度条进度
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        protected float GetDelayedProgress(float value)
        {
            return value - _CameraSetting.TimeDelay;
        }

        /// <summary>
        /// 上次更新的进度
        /// </summary>
        float lastProgress = 0f;

        /// <summary>
        /// 设置进度
        /// </summary>
        /// <param name="value"></param>
        internal void SetProgress(float value)
        {
            var progress = this.GetDelayedProgress(value);

            // 如果进度在0和最大值之外，说明现在不需要进行播放，停止播放
            if (progress < 0 || progress > maxTime)
            {
                return;
            }

            //***************************************************************************************************************//
            // 如果更新进度条，然后通过进度条更新事件触发镜头动作更新，镜头抖动会比较轻。这是因为进度条更新精度原因，某些帧不会被更新。
            //***************************************************************************************************************//

            // 限制最大帧数，刷新太快可能造成镜头抖动
            // 暂时不开启这个处理，便于底端程序优化镜头动作算法
            if (Mathf.Abs(lastProgress - progress) < 1f / 60f)
                return;

            lastProgress = progress;

            if (_progressJSON != null)
                _progressJSON.valNoCallback = progress;

            try
            {
                //_MmdCamera.Evaluation = config.CameraControlMode == CameraControlModes.Evaluation1 || config.CameraControlMode == CameraControlModes.Evaluation2;
                //_MmdCamera.NewInterpolate = config.CameraControlMode == CameraControlModes.Evaluation2;
                // 调用MMDCamera的播放进度
                _MmdCamera.SetPlayPos((double)progress);
            }
            catch (Exception e)
            {
                LogUtil.Debug(e);
            }
        }
    }
}
