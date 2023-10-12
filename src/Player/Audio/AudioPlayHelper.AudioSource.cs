using UnityEngine;

namespace mmd2timeline
{
    internal partial class AudioPlayHelper
    {
        /// <summary>
        /// 音源
        /// </summary>
        AudioSource _AudioSource;

        /// <summary>
        /// 是否正在播放
        /// </summary>
        public bool IsPlaying
        {
            get { return _AudioSource.isPlaying; }
        }

        /// <summary>
        /// 设置音量
        /// </summary>
        /// <param name="volume"></param>
        public void SetVolume(float volume)
        {
            this._AudioSource.volume = volume;
        }

        /// <summary>
        /// 获取音频的播放时间和进度
        /// </summary>
        /// <returns></returns>
        public float GetAudioTime()
        {
            return _AudioSource.time;
        }

        /// <summary>
        /// 设置音频源的播放时间
        /// </summary>
        /// <param name="time"></param>
        public void SetAudioTime(float time, bool hardUpdate = false)
        {
            if (time < _startTime) { time = _startTime; }

            if (_AudioSource != null && (hardUpdate || Mathf.Abs(_AudioSource.time - time) > 1f))
            {
                _AudioSource.time = time;
            }
        }

        /// <summary>
        /// 停止播放
        /// </summary>
        public void Stop(int test = 0)
        {
            if (_isPaused)
            {
                LogUtil.Debug($"Stop::AudioUnPause:{test}");

                this.AudioUnPause();
            }

            if (_AudioSource != null && _AudioSource.isPlaying)
            {
                _AudioSource.Stop();
            }
        }

        /// <summary>
        /// 暂停播放
        /// </summary>
        public void AudioPause()
        {
            if (_AudioSource != null)
            {
                _isPaused = true;
                _AudioSource.Pause();
            }
        }

        /// <summary>
        /// 取消暂停
        /// </summary>
        public void AudioUnPause()
        {
            if (_AudioSource != null)
            {
                _AudioSource.UnPause();
                _isPaused = false;
            }
        }

        /// <summary>
        /// 设置播放速度
        /// </summary>
        /// <param name="speed"></param>
        public void SetPlaySpeed(float speed)
        {
            //_AudioSource.pitch = speed;

            //if (speed == 1f)
            //{
            //    _AudioSource.velocityUpdateMode = AudioVelocityUpdateMode.Auto;
            //}
            //else
            //{
            //    _AudioSource.velocityUpdateMode = AudioVelocityUpdateMode.Fixed;
            //}
        }

        /// <summary>
        /// 播放音频
        /// </summary>
        /// <param name="time"></param>
        /// <returns></returns>
        public float Play()
        {
            if (!_AudioSource.isPlaying && PlayingAudio != null)
            {
                if (PlayingAudio?.clipToPlay?.loadState == AudioDataLoadState.Loaded)
                {
                    _AudioSource.clip = PlayingAudio.sourceClip;
                    _AudioSource.Play();
                }
            }

            return _progress;
        }

    }
}
