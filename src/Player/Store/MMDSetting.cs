using SimpleJSON;
using System.Collections.Generic;
using System.Text;

namespace mmd2timeline.Store
{
    internal class MMDSetting : JSONClass
    {
        /// <summary>
        /// 动作列表（多人）
        /// </summary>
        public List<PersonMotion> Motions
        {
            get;
            set;
        }

        /// <summary>
        /// 音频播放设置
        /// </summary>
        public AudioSetting AudioSetting
        {
            get { return (AudioSetting)this["AudioSetting"]; }
            set { this["AudioSetting"] = value; }
        }

        /// <summary>
        /// 镜头配置
        /// </summary>
        public CameraSetting CameraSetting
        {
            get { return (CameraSetting)this["CameraSetting"]; }
            set { this["CameraSetting"] = value; }
        }

        public MMDSetting()
        {
            this.Motions = new List<PersonMotion>();
            this.CameraSetting = new CameraSetting();
            this.AudioSetting = new AudioSetting();
        }

        public void LoadFromJSONClass(JSONClass setting)
        {
            // 处理动作
            if (setting.HasKey("Motions"))
            {
                foreach (JSONClass motion in setting["Motions"].AsArray)
                {
                    var personMotion = new PersonMotion();

                    foreach (var key in motion.Keys)
                    {
                        personMotion[key] = motion[key].Value;
                    }

                    // 处理文件清单，将旧的设定MotionPath，添加到文件清单
                    if (personMotion.HasKey("MotionPath"))
                    {
                        personMotion.Files.Add(personMotion["MotionPath"]);
                        personMotion.Remove("MotionPath");
                    }

                    // 处理文件清单，将旧的设定Expression1，添加到文件清单
                    if (personMotion.HasKey("Expression1"))
                    {
                        personMotion.Files.Add(personMotion["Expression1"]);
                        personMotion.Remove("Expression1");
                    }

                    // 处理文件清单，将旧的设定Expression2，添加到文件清单
                    if (personMotion.HasKey("Expression2"))
                    {
                        personMotion.Files.Add(personMotion["Expression2"]);
                        personMotion.Remove("Expression2");
                    }

                    // 获取保存的JSON
                    personMotion.LoadFiles();

                    if (personMotion.Files.Count > 0 || this.Motions.Count == 0)
                    {
                        this.Motions.Add(personMotion);
                    }
                }

                this.Remove("Motions");
            }

            // 处理音频设置[OLD]
            if (setting.HasKey("AudioSetting"))
            {
                var audioSettings = setting["AudioSetting"].AsObject;

                foreach (var key in audioSettings.Keys)
                {
                    this.AudioSetting[key] = audioSettings[key].Value;
                }
            }

            // 处理镜头设置[OLD]
            if (setting.HasKey("CameraSetting"))
            {
                var cameraSettings = setting["CameraSetting"].AsObject;

                foreach (var key in cameraSettings.Keys)
                {
                    this.CameraSetting[key] = cameraSettings[key].Value;
                }
            }
        }

        #region ToString相关的处理
        void BeforeToString()
        {
            if (this.Motions.Count > 0)
            {
                this["Motions"] = MMDEntity.ListToJSONArray(this.Motions);
            }
        }
        public override string ToString()
        {
            BeforeToString();
            return base.ToString();
        }
        public override string ToString(string aPrefix)
        {
            BeforeToString();
            return base.ToString(aPrefix);
        }
        public override void ToString(string aPrefix, StringBuilder sb)
        {
            BeforeToString();
            base.ToString(aPrefix, sb);
        }
        #endregion
    }
}
