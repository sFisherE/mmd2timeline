using System.Collections.Generic;
using UnityEngine;

namespace mmd2timeline
{
    /// <summary>
    /// 音频播放控制器
    /// </summary>
    internal partial class AudioPlayHelper
    {
        /// <summary>
        /// 音频设定
        /// </summary>
        AudioSetting _AudioSetting;

        /// <summary>
        /// 音频片段控制器
        /// </summary>
        AudioClipHelper _AudioClipHelper;

        /// <summary>
        /// 音源
        /// </summary>
        AudioSource _AudioSource;

        /// <summary>
        /// 是否正在加载
        /// </summary>
        public bool IsLoading
        {
            get
            {
                if (AudioCliper != null)
                {
                    if (AudioCliper.Inited)
                    {
                        return AudioCliper.IsLoading;
                    }
                }

                return true;
            }
        }

        /// <summary>
        /// 是否有音频
        /// </summary>
        public bool HasAudio
        {
            get { return AudioCliper.HasAudio; }
        }

        /// <summary>
        /// 播放中的音频
        /// </summary>
        public NamedAudioClip PlayingAudio
        {
            get
            {
                return AudioCliper.PlayingAudio;
            }
        }

        /// <summary>
        /// 获取音频是否正在播放
        /// </summary>
        public bool IsPlaying
        {
            get { return _AudioSource.isPlaying; }
        }

        /// <summary>
        /// 设置音源音量
        /// </summary>
        /// <param name="volume"></param>
        public void SetVolume(float volume)
        {
            _AudioSource.volume = volume;
        }

        /// <summary>
        /// 获取音频片段助手的实例
        /// </summary>
        private AudioClipHelper AudioCliper
        {
            get
            {
                if (_AudioClipHelper == null)
                {
                    _AudioClipHelper = new AudioClipHelper();
                }
                return _AudioClipHelper;
            }
        }

        #region 单例
        /// <summary>
        /// 音频播放助手的单例
        /// </summary>
        private static AudioPlayHelper _instance;
        private static object _lock = new object();

        /// <summary>
        /// 音频播放控制器的单例
        /// </summary>
        public static AudioPlayHelper GetInstance()
        {
            lock (_lock)
            {
                if (_instance == null)
                {
                    _instance = new AudioPlayHelper();
                }

                return _instance;
            }
        }
        #endregion

        /// <summary>
        /// 音频播放控制器
        /// </summary>
        private AudioPlayHelper()
        {
            // 初始化音频源
            _AudioSource = URLAudioClipManager.singleton.testAudioSource;
        }

        /// <summary>
        /// 初始化开始和结束时间
        /// </summary>
        /// <param name="start"></param>
        /// <param name="end"></param>
        internal void InitPlay(AudioSetting settings, List<string> choices, List<string> displayChoices)
        {
            _AudioSetting = settings;

            // 记录设定中的延迟值
            _delay = _AudioSetting.TimeDelay;

            // 先设定范围
            SetDelayRange(Mathf.Abs(_delay));
            if (_delay != 0f)
            {
                SetTimeDelay(_delay);
            }

            SetChooser(displayChoices, choices, settings?.AudioPath);
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
        /// 获取音频的播放时间和进度
        /// </summary>
        /// <returns></returns>
        public float GetAudioTime()
        {
            return _AudioSource.time;
        }

        /// <summary>
        /// 音频停止
        /// </summary>
        public void Stop(int test = 0)
        {
            if (_AudioSource != null && _AudioSource.isPlaying)
            {
                _AudioSource.Stop();
            }
        }

        /// <summary>
        /// 设置音频源的播放时间
        /// </summary>
        /// <param name="time"></param>
        public void SetAudioTime(float time, bool hardUpdate = false)
        {
            if (_AudioSource != null && (hardUpdate || Mathf.Abs(_AudioSource.time - time) > 0.1f))
            {
                _AudioSource.time = time;
            }
        }

        /// <summary>
        /// 播放音频
        /// </summary>
        /// <param name="time"></param>
        /// <returns></returns>
        public void Play()
        {
            if (!_AudioSource.isPlaying && PlayingAudio != null)
            {
                if (PlayingAudio?.clipToPlay?.loadState == AudioDataLoadState.Loaded)
                {
                    _AudioSource.clip = PlayingAudio.sourceClip;
                    _AudioSource.Play();
                }
            }
        }

        /// <summary>
        /// 检查音频的加载状态
        /// </summary>
        internal void CheckLoad()
        {
            if (!HasAudio)
                return;

            var length = AudioCliper.TryGetAudioLength();

            if (length.HasValue)
            {
                MaxTime = length.Value;

                OnAudioLoaded?.Invoke(length.Value);
            }
        }

        /// <summary>
        /// 加载指定地址的音频
        /// </summary>
        /// <param name="audioPath"></param>
        private void LoadAudio(string audioPath)
        {
            Clear(false);

            if (audioPath == noneString)
            {
                audioPath = null;
            }

            AudioCliper.LoadAudio(audioPath);
        }

        /// <summary>
        /// 清空播放内容
        /// </summary>
        public void Clear(bool cleanChooser = true)
        {
            Stop(2);

            AudioCliper.Clear();

            if (cleanChooser)
            {
                MaxTime = 0;
                ResetChooser();
            }

            URLAudioClipManager.singleton.RemoveAllClips();
            URLAudioClipManager.singleton.RestoreAllFromDefaults();
        }

        /// <summary>
        /// 销毁时执行的函数
        /// </summary>
        public void OnDestroy()
        {
            Clear(true);

            _AudioSource = null;
            _AudioClipHelper = null;
            _AudioSetting = null;

            _instance = null;
        }

        /// <summary>
        /// 禁用时执行的函数
        /// </summary>
        public void OnDisable()
        {
            Stop(3);
        }

        /// <summary>
        /// 启用时执行的函数
        /// </summary>
        public void OnEnable()
        {

        }
    }
}
