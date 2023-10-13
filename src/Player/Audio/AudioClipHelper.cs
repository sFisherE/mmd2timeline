using System;
using UnityEngine;

namespace mmd2timeline
{
    /// <summary>
    /// 音频片段控制器
    /// </summary>
    internal class AudioClipHelper
    {
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
        /// 播放中的音频片段
        /// </summary>
        public NamedAudioClip PlayingAudio
        {
            get
            {
                return _AudioClip;
            }
        }

        /// <summary>
        /// 音频片段文件名称
        /// </summary>
        public string AudioFileName
        {
            get
            {
                return _AudioFileName;
            }
        }

        /// <summary>
        /// 实例化音频片段助手
        /// </summary>
        public AudioClipHelper()
        {
            Inited = false;
            IsLoading = false;
        }

        /// <summary>
        /// 加载指定地址的音频
        /// </summary>
        /// <param name="audioPath"></param>
        public void LoadAudio(string audioPath)
        {
            try
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
                }
                else
                {
                    IsLoading = false;
                }
            }
            catch (Exception ex)
            {
                LogUtil.LogError(ex, $"LoadAudio::");
            }
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
        /// 清理至初始状态
        /// </summary>
        public void Clear()
        {
            if (_AudioClip != null)
            {
                _AudioClip = null;
            }

            Inited = false;
            IsLoading = false;
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
    }
}
