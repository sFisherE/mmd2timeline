namespace mmd2timeline
{
    /// <summary>
    /// 音频播放控制器
    /// </summary>
    /// <remarks>用于控制音频播放过程</remarks>
    internal partial class AudioPlayHelper
    {
        /// <summary>
        /// 音频加载完成的回调委托
        /// </summary>
        /// <param name="length">返回音频长度</param>
        public delegate void AudioLoadedCallback(float length);

        /// <summary>
        /// 音频加载完成的事件
        /// </summary>
        public event AudioLoadedCallback OnAudioLoaded;

        /// <summary>
        /// 音频暂停回调委托
        /// </summary>
        /// <param name="isPaused"></param>
        public delegate void AudioIsPausedCallback(bool isPaused);

        /// <summary>
        /// 当播放暂停变化的事件
        /// </summary>
        public AudioIsPausedCallback OnAudioIsPaused;

        /// <summary>
        /// 音频片段处理器
        /// </summary>
        AudioClipHelper _AudioClipHelper = AudioClipHelper.GetInstance();

        /// <summary>
        /// 获取音频是否可以播放
        /// </summary>
        public bool CanPlay
        {
            get
            {
                return _AudioClipHelper.IsLoaded;
            }
        }

        /// <summary>
        /// 是否正在加载
        /// </summary>
        public bool IsLoading
        {
            get
            {
                if (_AudioClipHelper.Inited)
                {
                    return _AudioClipHelper.IsLoading;
                }

                return true;
            }
        }

        /// <summary>
        /// 是否有音频剪辑
        /// </summary>
        public bool HasAudio
        {
            get { return _AudioClipHelper.HasAudio; }
        }

        /// <summary>
        /// 播放中的音频剪辑
        /// </summary>
        public NamedAudioClip PlayingAudio
        {
            get
            {
                return _AudioClipHelper.PlayingAudio;
            }
        }

        /// <summary>
        /// 音频播放控制器的单例
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
        /// <summary>
        /// 音频播放控制器
        /// </summary>
        private AudioPlayHelper()
        {
            // 初始化音频源
            _AudioSource = URLAudioClipManager.singleton.testAudioSource;

            _AudioClipHelper.OnAudioClipLoaded += _AudioClipHelper_AudioClipLoaded;
        }

        /// <summary>
        /// 音频片段加载完成的事件处理函数
        /// </summary>
        /// <param name="length">加载的音频长度</param>
        private void _AudioClipHelper_AudioClipLoaded(float length)
        {
            // 更新音频最大事件
            _maxTime = length;

            // 音频长度大于0为可播放状态
            isWaitingPlay = length > 0f;

            // 触发音频加载完成的事件
            OnAudioLoaded?.Invoke(length);
        }

        /// <summary>
        /// 初始化开始和结束时间
        /// </summary>
        /// <param name="start"></param>
        /// <param name="end"></param>
        internal void InitPlay(float start, float end)
        {
            StartTime = start;
            EndTime = end;
        }

        /// <summary>
        /// 加载音频
        /// </summary>
        /// <param name="path"></param>
        internal void LoadAudio(string path)
        {
            this.Clear(false);

            _AudioClipHelper.LoadAudio(path);
        }

        /// <summary>
        /// 清空播放内容
        /// </summary>
        public void Clear(bool cleanChooser = true)
        {
            this.Stop(2);

            _AudioClipHelper.Clear();

            URLAudioClipManager.singleton.RemoveAllClips();
            URLAudioClipManager.singleton.RestoreAllFromDefaults();
        }

        /// <summary>
        /// 销毁时执行的函数
        /// </summary>
        public void Dispose()
        {
            this.Clear();

            this.OnAudioIsPaused = null;

            this._AudioSource = null;

            _instance = null;
        }
    }
}
