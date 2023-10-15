namespace mmd2timeline
{
    internal partial class AudioPlayHelper
    {
        /// <summary>
        /// 音频加载完成的回调委托
        /// </summary>
        /// <param name="length">音频长度</param>
        public delegate void AudioLoadedCallback(float length);

        /// <summary>
        /// 音频加载完成的事件
        /// </summary>
        public event AudioLoadedCallback OnAudioLoaded;

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

                // 设置延迟范围
                SetDelayRange(value);
            }
        }

        /// <summary>
        /// 检查音频播放器是否启用了延迟设定
        /// </summary>
        internal bool IsDelay
        {
            get
            {
                return _AudioSetting?.TimeDelay != 0f;
            }
        }

        /// <summary>
        /// 记录设定的延迟时间
        /// </summary>
        float _delay = 0f;

        /// <summary>
        /// 设置延迟时间
        /// </summary>
        /// <param name="delay"></param>
        internal virtual void SetTimeDelay(float delay)
        {
            // 更新设定条但不触发回调函数
            _timeDelayJSON.valNoCallback = delay;

            // 更新数据
            _AudioSetting.TimeDelay = delay;
        }

        /// <summary>
        /// 获得延迟的进度条进度
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        protected float GetDelayedProgress(float value)
        {
            return value - _AudioSetting.TimeDelay;
        }

        /// <summary>
        /// 设置进度
        /// </summary>
        /// <param name="value"></param>
        internal void SetProgress(float value, bool play = true)
        {
            var progress = this.GetDelayedProgress(value);

            // 如果进度在0和最大值之外，说明现在不需要进行播放，停止播放
            if (progress < 0 || progress > maxTime)
            {
                Stop(0);
                return;
            }

            if (play && HasAudio)
            {
                this.Play();
            }

            // 设置音频播放进度
            SetAudioTime(progress);
        }
    }
}
