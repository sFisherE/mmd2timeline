using System;
using System.Collections;
using UnityEngine;

namespace mmd2timeline
{
    /// <summary>
    /// 音频片段控制器
    /// </summary>
    /// <remarks>用于加载音频文件，获取音频文件的长度</remarks>
    internal class AudioClipHelper
    {
        /// <summary>
        /// 音频片段加载完成的回调委托
        /// </summary>
        /// <param name="length">返回音频长度</param>
        public delegate void AudioClipLoadedCallback(float length);

        /// <summary>
        /// 音频片段加载完成的事件
        /// </summary>
        public event AudioClipLoadedCallback OnAudioClipLoaded;

        /// <summary>
        /// 音频片段
        /// </summary>
        NamedAudioClip _AudioClip;

        /// <summary>
        /// 音频文件名称
        /// </summary>
        string _AudioFileName;

        /// <summary>
        /// 是否正在加载
        /// </summary>
        public bool IsLoading
        {
            get;
            private set;
        }

        /// <summary>
        /// 是否已经加载音频
        /// </summary>
        public bool IsLoaded
        {
            get
            {
                return this.Length > 0f;
            }
        }

        /// <summary>
        /// 指示控制器是否进行了初始运行
        /// </summary>
        public bool Inited
        {
            get; private set;
        }

        /// <summary>
        /// 获取音频长度
        /// </summary>
        public float Length
        {
            get
            {
                return _AudioClip?.sourceClip?.length ?? 0f;
            }
        }

        /// <summary>
        /// 是否有音频片段
        /// </summary>
        public bool HasAudio
        {
            get { return _AudioClip != null; }
        }

        /// <summary>
        /// 播放中的音频片段对象
        /// </summary>
        public NamedAudioClip PlayingAudio
        {
            get
            {
                return _AudioClip;
            }
        }

        /// <summary>
        /// 获取当前音频片段文件名称
        /// </summary>
        public string AudioFileName
        {
            get
            {
                return _AudioFileName;
            }
        }

        /// <summary>
        /// 实例化音频片段控制器
        /// </summary>
        private AudioClipHelper()
        {
            Inited = false;
            IsLoading = false;
        }

        /// <summary>
        /// 加载指定地址的音频
        /// </summary>
        /// <param name="audioPath"></param>
        public IEnumerator LoadAudio(string audioPath)
        {
            Inited = true;
            IsLoading = true;

            _AudioFileName = audioPath;

            if (String.IsNullOrEmpty(audioPath))
            {
                _AudioClip = null;
            }
            else
            {
                _AudioClip = GetAudio(audioPath);
            }

            if (_AudioClip != null)
            {
                _AudioClip?.sourceClip?.LoadAudioData();

                // 等待到加载完成
                while (IsLoading)
                {
                    if (!CheckAudioLoaded())
                    {
                        break;
                    }

                    // 等一帧再检查
                    yield return null;
                }
            }
            else
            {
                IsLoading = false;
            }

            // 触发事件，通知音频偏度按加载完成
            OnAudioClipLoaded?.Invoke(this.Length);
        }

        /// <summary>
        /// 获取音频长度
        /// </summary>
        /// <returns></returns>
        public float? TryGetAudioLength()
        {
            // 加载中
            if (IsLoading)
            {
                if (!CheckAudioLoaded())
                {
                    return null;
                }
            }

            return this.Length;
        }

        /// <summary>
        /// 清理音频片段
        /// </summary>
        public void Clear()
        {
            Inited = false;
            IsLoading = false;
            if (_AudioClip != null)
            {
                _AudioClip.manager?.RemoveAllClips();

                _AudioClip = null;
            }
            _AudioFileName = null;
        }

        /// <summary>
        /// 检查音频的加载状态
        /// </summary>
        private bool CheckAudioLoaded()
        {
            if (_AudioClip == null)
            {
                this.IsLoading = false;
                return true;
            }

            switch (_AudioClip?.sourceClip?.loadState)
            {
                case AudioDataLoadState.Loaded:
                    this.IsLoading = false;
                    return true;
                case AudioDataLoadState.Loading:
                    this.IsLoading = true;
                    return false;
                case AudioDataLoadState.Failed:
                    this.IsLoading = false;
                    return true;
                case AudioDataLoadState.Unloaded:
                default:
                    this.IsLoading = true;
                    _AudioClip?.sourceClip?.LoadAudioData();
                    return false;
            }
        }

        /// <summary>
        /// 载入音频。
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        private NamedAudioClip GetAudio(string path)
        {
            var localPath = SuperController.singleton.NormalizeLoadPath(path);
            var existing = URLAudioClipManager.singleton.GetClip(localPath);
            if (existing != null)
            {
                return existing;
            }

            var clip = URLAudioClipManager.singleton.QueueClip(SuperController.singleton.NormalizeMediaPath(path));
            if (clip == null)
            {
                return null;
            }

            var nac = URLAudioClipManager.singleton.GetClip(clip.uid);
            if (nac == null)
            {
                return null;
            }

            return nac;
        }

        /// <summary>
        /// 音频片段控制器单例
        /// </summary>
        private static AudioClipHelper _instance;
        private static object _lock = new object();

        /// <summary>
        /// 获取音频片段控制器的单例
        /// </summary>
        public static AudioClipHelper GetInstance()
        {
            lock (_lock)
            {
                if (_instance == null)
                {
                    _instance = new AudioClipHelper();
                }

                return _instance;
            }
        }
    }
}
