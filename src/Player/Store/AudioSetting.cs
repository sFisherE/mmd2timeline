using SimpleJSON;
using System.Linq;

namespace mmd2timeline
{
    /// <summary>
    /// 音频播放的设置
    /// </summary>
    internal class AudioSetting : JSONClass
    {
        /// <summary>
        /// 音乐文件路径
        /// </summary>
        public string AudioPath
        {
            get
            {
                if (this.HasKey("MusicPath"))
                {
                    this.AudioPath = this["MusicPath"];

                    this.Remove("MusicPath");
                }
                return this["AudioPath"];
            }
            set
            {
                this["AudioPath"] = value;
            }
        }
        /// <summary>
        /// 音频播放开始时间
        /// </summary>
        public float StartTime
        {
            get
            {
                if (this.HasKey("StartTime"))
                    return this["StartTime"].AsFloat;
                else
                    return 0f;
            }
            set
            {
                this["StartTime"].AsFloat = value;
            }
        }
        /// <summary>
        /// 音频播放结束时间
        /// </summary>
        public float EndTime
        {
            get
            {
                if (this.HasKey("EndTime"))
                    return this["EndTime"].AsFloat;
                else
                    return 0f;
            }
            set
            {
                this["EndTime"].AsFloat = value;
            }
        }

        /// <summary>
        /// 对比两个音频配置是否一致
        /// </summary>
        /// <param name="target"></param>
        /// <returns></returns>
        public bool Compare(AudioSetting target)
        {
            var audioSettingKeys = this.Keys.ToList();

            // 音频设定对比
            bool isAudioSame = true;

            if (target.Keys.Count() == audioSettingKeys.Count)
            {
                foreach (var key in audioSettingKeys)
                {
                    if (this[key].Value != target[key].Value)
                    {
                        isAudioSame = false;
                        break;
                    }
                }
            }
            else
            {
                isAudioSame = false;
            }

            audioSettingKeys.Clear();
            audioSettingKeys = null;

            return isAudioSame;
        }
    }
}
