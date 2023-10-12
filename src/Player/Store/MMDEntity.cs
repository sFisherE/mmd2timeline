using MVR.FileManagementSecure;
using SimpleJSON;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace mmd2timeline.Store
{
    /// <summary>
    /// MMD数据实体对象
    /// </summary>
    internal partial class MMDEntity : JSONClass
    {
        const string noneString = "None";

        public MMDEntity()
        {
            this.Files = new List<MMDFile>();
            // 初始化播放配置。初始列表中包含一个新的配置实例
            this.Settings = new List<MMDSetting>() { new MMDSetting() };
        }

        /// <summary>
        /// 指示对象是否被更改过，需要保存
        /// </summary>
        public bool NeedSave = false;

        /// <summary>
        /// MMD的唯一标识。一般用路径标识
        /// </summary>
        public string Key
        {
            get { return this["Key"]; }
            set
            {
                if (this.Key != value)
                {
                    this["Key"] = value;
                    NeedSave = true;
                }
            }
        }
        /// <summary>
        /// 标题
        /// </summary>
        public string Title
        {
            get { return this["Title"]; }
            set
            {
                if (this.Title != value)
                {
                    this["Title"] = value;
                    NeedSave = true;
                }
            }
        }
        /// <summary>
        /// 简介
        /// </summary>
        public string Description
        {
            get { return this["Description"]; }
            set
            {
                if (this.Description != value)
                {
                    this["Description"] = value;
                    NeedSave = true;
                }
            }
        }

        /// <summary>
        /// 获取或设置默认配置的序号
        /// </summary>
        public int SettingIndex
        {
            get { return this["SettingIndex"].AsInt; }
            set
            {
                if (this.SettingIndex != value)
                {
                    this["SettingIndex"].AsInt = value;
                    NeedSave = true;
                }
            }
        }

        /// <summary>
        /// 播放设置列表。一个MMD可以保存多个配置
        /// </summary>
        public List<MMDSetting> Settings
        {
            get;
            private set;
        }

        /// <summary>
        /// 此MMD相关的文件清单
        /// </summary>
        public List<MMDFile> Files
        {
            get;
            set;
        }

        /// <summary>
        /// 获取当前MMD的默认播放配置
        /// </summary>
        public MMDSetting CurrentSetting
        {
            get
            {
                if (this.SettingIndex >= this.Settings.Count)
                {
                    this.SettingIndex = 0;
                }
                return Settings[this.SettingIndex];
            }
        }

        /// <summary>
        /// 获取当前配置的人物动作清单
        /// </summary>
        public List<PersonMotion> Motions
        {
            get
            {
                return this.CurrentSetting.Motions;
            }
        }

        /// <summary>
        /// 获取当前配置的音频设置
        /// </summary>
        public AudioSetting AudioSetting
        {
            get
            {
                return this.CurrentSetting.AudioSetting;
            }
        }

        /// <summary>
        /// 获取当前配置的镜头设置
        /// </summary>
        public CameraSetting CameraSetting
        {
            get
            {
                return this.CurrentSetting.CameraSetting;
            }
        }

        string _GUID;

        /// <summary>
        /// 获取GUID
        /// </summary>
        public string GUID
        {
            get
            {
                if (_GUID == null)
                {
                    using (var md5 = new MD5CryptoServiceProvider())
                        _GUID = new Guid(md5.ComputeHash(Encoding.Default.GetBytes(this.Key))).ToString();
                }

                return _GUID;
            }
        }

        /// <summary>
        /// 获取mmd文件数据
        /// </summary>
        /// <returns></returns>
        public FilesData GetFileData()
        {
            var musicList = new List<String>(new string[1] { noneString });
            var motionList = new List<String>(new string[1] { noneString });

            var musicNameList = new List<String>(new string[1] { noneString });
            var motionNameList = new List<String>(new string[1] { noneString });

            var defaultMusic = noneString;
            var defaultCamera = noneString;
            var defaultMotion = noneString;

            var candidateMotion = new List<string>();

            // 轮询MMD文件，并对文件自动选择进行预处理
            foreach (MMDFile file in this.Files)
            {
                var fullFileName = file.FileName;
                var fileName = FileManagerSecure.GetFileName(fullFileName);

                if (file.FileType == MMDFileType.Music)
                {
                    musicList.Add(fullFileName);
                    musicNameList.Add(fileName);

                    if (defaultMusic == noneString)
                    {
                        defaultMusic = fullFileName;
                    }
                }
                else if (fullFileName.ToLower().EndsWith(".vmd"))
                {
                    motionList.Add(fullFileName);
                    motionNameList.Add(fileName);

                    if (file.FileType == MMDFileType.Camera)
                    {
                        if (defaultCamera == noneString)
                        {
                            defaultCamera = fullFileName;
                        }
                    }
                    else if (file.FileType == MMDFileType.Motion)
                    {
                        // 将认定为动作的vmd文件放在候选动作列表的前端
                        candidateMotion.Insert(0, fullFileName);
                    }
                    else if (file.FileType == MMDFileType.VMD)
                    {
                        // 将位置vmd文件放在首选动作列表的末尾
                        candidateMotion.Add(fullFileName);
                    }
                }
            }

            // 如果默认动作为None并且候选动作列表不为空
            if (defaultMotion == noneString && candidateMotion.Count > 0)
            {
                defaultMotion = candidateMotion.FirstOrDefault();
            }

            return new FilesData()
            {
                AudioPaths = musicList,
                AudioNames = musicNameList,
                MotionPaths = motionList,
                MotionNames = motionNameList,

                DefaultAudio = defaultMusic,
                DefaultCamera = defaultCamera,
                DefaultMotion = defaultMotion,
            };
        }

        /// <summary>
        /// 初始化默认动作文件
        /// </summary>
        internal void InitMotionFiles(bool forceUpdate = false)
        {
            var data = this.GetFileData();

            if (string.IsNullOrEmpty(this.CameraSetting.CameraPath)
                || this.CameraSetting.CameraPath == noneString
                || (forceUpdate && this.CameraSetting.CameraPath != data.DefaultCamera))
            {
                this.CameraSetting.CameraPath = data.DefaultCamera;
            }

            if (string.IsNullOrEmpty(this.AudioSetting.AudioPath) || this.AudioSetting.AudioPath == noneString
                || (forceUpdate && this.AudioSetting.AudioPath != data.DefaultAudio))
            {
                this.AudioSetting.AudioPath = data.DefaultAudio;
            }

            if (data.DefaultMotion != noneString)
            {
                if (this.Motions.Count == 0)
                {
                    var mmdPersonMotion = new PersonMotion();
                    mmdPersonMotion.Files.Add(data.DefaultMotion);
                    this.Motions.Add(mmdPersonMotion);
                }
                else if (forceUpdate && !this.Motions.Any(m => m.Files.Count(f => f != noneString) > 0))
                {
                    var motion = this.Motions.FirstOrDefault();

                    if (motion != null)
                    {
                        motion.Files.Clear();
                        motion.Files.Add(data.DefaultMotion);
                    }
                }
            }
        }

        /// <summary>
        /// 将当前对象克隆为一个新的对象
        /// </summary>
        /// <returns></returns>
        internal MMDEntity Clone()
        {
            return MMDEntity.LoadFromJSONClass(JSONClass.Parse(this.ToString()) as JSONClass);
        }

        #region Store相关的方法

        /// <summary>
        /// 将当前对象保存到仓库
        /// </summary>
        internal static void SaveToStore(MMDEntity entity)
        {
            FileManagerSecure.CreateDirectory(entity.StorePath);

            SuperController.singleton.SaveJSON(entity, entity.StoreFile);
        }

        /// <summary>
        /// 从仓库加载对象
        /// </summary>
        /// <param name="fileName"></param>
        /// <returns></returns>
        internal static MMDEntity LoadFromStore(string guid)
        {
            var fileName = $"{STORE_PATH}\\{guid}\\entity.{SAVE_EXT}";

            return LoadFromJSONClass(SuperController.singleton.LoadJSON(fileName).AsObject);
        }

        #endregion

        #region ToString相关的处理
        void BeforToString()
        {
            this["Settings"] = ListToJSONArray(this.Settings);
            this["Files"] = ListToJSONArray(this.Files);
        }

        /// <summary>
        /// 将JSON对象转换为字符串
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            BeforToString();
            return base.ToString();
        }

        /// <summary>
        /// 将对象转换为stringbuilder
        /// </summary>
        /// <param name="aPrefix"></param>
        /// <param name="sb"></param>
        public override void ToString(string aPrefix, StringBuilder sb)
        {
            BeforToString();
            base.ToString(aPrefix, sb);
        }

        /// <summary>
        /// 将对象转换为字符串
        /// </summary>
        /// <param name="aPrefix"></param>
        /// <returns></returns>
        public override string ToString(string aPrefix)
        {
            BeforToString();
            return base.ToString(aPrefix);
        }

        #endregion
        /// <summary>
        /// 将列表转换为JSONArray
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="list"></param>
        /// <returns></returns>
        public static JSONArray ListToJSONArray<T>(List<T> list) where T : JSONNode
        {
            var array = new JSONArray();
            foreach (var setting in list)
            {
                array.Add(setting);
            }
            return array;
        }

        /// <summary>
        /// 从JSONClass对象中获取数据并生成MMDEntiy对象
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        public static MMDEntity LoadFromJSONClass(JSONClass item)
        {
            var entity = new MMDEntity()
            {
                Key = item["Key"].Value,
                Title = item["Title"].Value,
                SettingIndex = item["SettingIndex"].AsInt
            };

            if (item.HasKey("Description"))
            {
                entity.Description = item["Description"].Value;
            }

            // 处理文件清单
            if (item.HasKey("Files"))
            {
                foreach (JSONClass file in item["Files"].AsArray)
                {
                    var mmdfile = new MMDFile(file["FileName"]);

                    foreach (var key in file.Keys)
                    {
                        if (key == "FileType")
                        {
                            mmdfile.FileType = MMDFile.ConvertFileType(file[key]);
                        }
                        else if (key != "FileName")
                        {
                            mmdfile[key] = file[key].Value;
                        }
                    }

                    entity.Files.Add(mmdfile);
                }
            }

            // 处理配置信息
            if (item.HasKey("Settings"))
            {
                var settingCount = entity.Settings.Count;

                for (int i = 0; i < item["Settings"].AsArray.Count; i++)
                {
                    var setting = item["Settings"][i] as JSONClass;

                    MMDSetting targetSetting;

                    if (settingCount > i)
                    {
                        targetSetting = entity.Settings[i];
                    }
                    else
                    {
                        targetSetting = new MMDSetting();
                        entity.Settings.Add(targetSetting);
                    }

                    targetSetting.LoadFromJSONClass(setting);
                }
            }

            #region 兼容旧的保存文件格式的处理
            // 处理旧版配置的音频文件[OLD]
            if (item.HasKey("MusicPath"))
            {
                entity.CurrentSetting.AudioSetting.AudioPath = FileNameConvert(item["MusicPath"].Value);
            }

            // 处理旧版配置的镜头文件[OLD]
            if (item.HasKey("CameraPath"))
            {
                entity.CurrentSetting.CameraSetting.CameraPath = FileNameConvert(item["CameraPath"].Value);
            }

            // 处理动作路径[OLD]
            if (item.HasKey("MotionPath")
                && item["MotionPath"] != "None"
                && !String.IsNullOrEmpty(item["MotionPath"]))
            {
                var mmdPersonMotion = new PersonMotion();
                mmdPersonMotion.Files.Add(FileNameConvert(item["MotionPath"]));
                entity.CurrentSetting.Motions.Add(mmdPersonMotion);
            }

            // 兼容旧版本配置文件，加载配置到默认配置中
            entity.CurrentSetting.LoadFromJSONClass(item);

            #endregion

            return entity;
        }

        /// <summary>
        /// 文件名称转换
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        internal static string FileNameConvert(string path)
        {
            path = path.Replace("/", @"\").Replace(@"\\", @"\");

            var chars = new List<Char>();

            foreach (int num in path)
            {
                if (num == 34 || num == 60 || num == 62 || num == 124 || num < 32)
                {
                }
                else
                {
                    chars.Add((char)num);
                }
            }

            var to = new string(chars.ToArray());

            return to;
        }

        /// <summary>
        /// 文件数据
        /// </summary>
        internal struct FilesData
        {
            internal List<string> AudioPaths;
            internal List<string> AudioNames;

            internal List<string> MotionPaths;
            internal List<string> MotionNames;

            internal string DefaultAudio;
            internal string DefaultMotion;
            internal string DefaultCamera;
        }
    }

    public static class TSourceExt
    {
        /// <summary>
        /// 根据参数去重
        /// </summary>
        /// <typeparam name="TSource"></typeparam>
        /// <typeparam name="TKey"></typeparam>
        /// <param name="source"></param>
        /// <param name="selector"></param>
        /// <returns></returns>
        public static IEnumerable<TSource> DistinctBy<TSource, TKey>(this IEnumerable<TSource> source, Func<TSource, TKey> selector)
        {
            HashSet<TKey> result = new HashSet<TKey>();
            foreach (TSource element in source)
            {
                if (result.Add(selector(element)))
                {
                    yield return element;
                }
            }
        }
    }
}
