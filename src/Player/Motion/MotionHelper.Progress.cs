using mmd2timeline.Store;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace mmd2timeline
{
    internal partial class MotionHelper
    {
        /// <summary>
        /// 动作加载完成的回调委托
        /// </summary>
        /// <param name="length">返回动作长度</param>
        public delegate void MotionLoadedCallback(float length);

        /// <summary>
        /// 动作加载完成的事件
        /// </summary>
        public event MotionLoadedCallback OnMotionLoaded;

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

                if (_ProgressJSON != null)
                    _ProgressJSON.max = value;

                // 设置延迟范围
                SetDelayRange(value);
            }
        }

        /// <summary>
        /// 设置延迟数值的范围
        /// </summary>
        /// <param name="length"></param>
        void SetDelayRange(float length)
        {
            if (_TimeDelayJSON != null)
            {
                _TimeDelayJSON.min = 0 - length;
                _TimeDelayJSON.max = length;
            }
        }

        /// <summary>
        /// 获取是否有动作数据
        /// </summary>
        internal bool HasMotion
        {
            get { return maxTime > 0f; }
        }

        /// <summary>
        /// 检查音频播放器是否启用了延迟设定
        /// </summary>
        internal bool IsDelay
        {
            get
            {
                return _MotionSetting?.TimeDelay != 0f;
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
            //_delay = delay;
            // 更新设定条但不触发回调含税
            _TimeDelayJSON.valNoCallback = delay;

            // 更新数据
            _MotionSetting.TimeDelay = delay;
        }

        /// <summary>
        /// 获得延迟的进度条进度
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        protected float GetDelayedProgress(float value)
        {
            return value - _MotionSetting.TimeDelay;
        }

        float lastProgress = 0f;

        /// <summary>
        /// 设置进度
        /// </summary>
        /// <param name="value"></param>
        internal void SetProgress(float value)
        {
            var progress = this.GetDelayedProgress(value);

            //// 如果进度在0和最大值之外，说明现在不需要进行播放，停止播放
            //if (progress < 0 || progress > maxTime)
            //{
            //    //Stop();
            //    return;
            //}
            //LogUtil.Log($"{progress}");

            //// 限制最大帧数，刷新太快可能造成镜头抖动
            //if (Mathf.Abs(lastProgress - progress) < 1f / 60f)
            //    return;

            lastProgress = progress;

            //_progress = progress;

            if (_ProgressJSON != null)
                _ProgressJSON.valNoCallback = progress;

            //LogUtil.Log($"({play})isPlaying:{_AudioSource.isPlaying} - PlayingAudio:{PlayingAudio?.displayName} - {progress} to {GetAudioTime()}");

            //if (play && HasAudio)//&& IsWaitingPlay
            //{
            //    this.Play();

            //    //IsWaitingPlay = !this.IsPlaying;
            //}

            // 设置音频播放进度
            //SetAudioTime(progress);

            // 调用MMDCamera的播放进度
            //_MmdCamera.SetPlayPos((double)value/*, config.CameraOnlyKeyFrame*/);
            _MmdPersonGameObject?.SetMotionPos(value, true, motionScale: motionScaleRate);
        }
    }
}
