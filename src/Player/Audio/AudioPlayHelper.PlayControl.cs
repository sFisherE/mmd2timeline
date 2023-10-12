using UnityEngine;

namespace mmd2timeline
{
    internal partial class AudioPlayHelper
    {
        float _startTime = 0f;
        float _endTime = 0f;
        float _maxTime = 0f;
        float _progress = 0f;

        float _speed = 1f;

        bool _isPaused = false;

        /// <summary>
        /// 是否正在等待播放
        /// </summary>
        internal bool isWaitingPlay = false;

        /// <summary>
        /// 获取或设置开始时间
        /// </summary>
        /// <param name="time"></param>
        internal float StartTime
        {
            get
            {
                return _startTime;
            }
            set
            {
                if (value < 0)
                {
                    value = 0f;
                }
                else if (value > _endTime)
                {
                    value = _endTime;
                }

                _startTime = value;

                if (_AudioSource.time < _startTime)
                {
                    this.SetAudioTime(_startTime);
                }
            }
        }

        /// <summary>
        /// 获取或设置结束时间
        /// </summary>
        /// <param name="time"></param>
        internal float EndTime
        {
            get
            {
                return _endTime;
            }
            set
            {
                if (value < 0)
                {
                    value = 0f;
                }
                else if (value < _startTime)
                {
                    value = _startTime;
                }

                _endTime = value;

                if (_AudioSource.time > _endTime)
                {
                    this.SetAudioTime(_endTime);
                }
            }
        }

        /// <summary>
        /// 获取是否已经播放到末尾
        /// </summary>
        internal bool IsEnd
        {
            get
            {
                return _progress >= _endTime;
            }
        }

        /// <summary>
        /// 更新进度数据
        /// </summary>
        internal void Update()
        {
            var deltaTime = Time.deltaTime * _speed;

            _progress += deltaTime;

            if (!IsEnd)
            {
                // 如果正在播放，使用音频时间更新进度条
                if (this.IsPlaying)
                {
                    // 如果是正常速度，则取音频的播放时间，否则根据计算时间更新进度
                    if (_speed != 1f)
                    {
                        // 更新音频时间进度
                        SetAudioTime(_progress);
                    }
                    else
                    {
                        // 使用音频播放进度同步进度条的进度
                        _progress = _AudioSource.time;
                    }
                }
            }
        }
    }
}
