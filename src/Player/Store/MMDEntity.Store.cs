using MVR.FileManagementSecure;
using SimpleJSON;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace mmd2timeline.Store
{
    internal partial class MMDEntity
    {
        /// <summary>
        /// 数据存储目录
        /// </summary>
        internal static string STORE_PATH
        {
            get
            {
                return mmd2timeline.Config.saveDataPath + "\\Store";
            }
        }
        internal static readonly string SAVE_EXT = "json";

        /// <summary>
        /// 获取存储路径
        /// </summary>
        public string StorePath
        {
            get
            {
                return STORE_PATH + "\\" + this.GUID;
            }
        }

        /// <summary>
        /// 获取存储文件名称
        /// </summary>
        public string StoreFile
        {
            get
            {
                return this.StorePath + "\\" + "entity" + "." + SAVE_EXT;
            }
        }
        #region Store相关的方法

        /// <summary>
        /// 将当前对象保存到仓库
        /// </summary>
        public void Save()
        {
            try
            {
                // 只有不在包中的数据才能保存
                if (!InPackage)
                {
                    FileManagerSecure.CreateDirectory(this.StorePath);

                    SuperController.singleton.SaveJSON(this, this.StoreFile);
                }

                NeedSave = false;
            }
            catch (Exception ex)
            {
                LogUtil.Debug(ex, "MMDEnity::Save:");
            }
        }

        /// <summary>
        /// 将目标对象的值合并到本对象
        /// </summary>
        /// <param name="source"></param>
        public void Union(MMDEntity source)
        {
            bool somethingChanged = false;

            // 进行文件列表合并
            foreach (var file in source.Files)
            {
                var mmdFile = this.Files.FirstOrDefault(f => f.Name == file.Name);

                if (mmdFile == null && file.FileType != MMDFileType.Other)
                {
                    this.Files.Add(file);

                    somethingChanged = true;
                }
            }

            // 循环检查目标设定
            foreach (var setting in source.Settings)
            {
                bool isAudioSame = false;
                bool isCameraSame = false;
                bool isMotionSame = false;

                // 找到音频设定一致的设定
                var mmdSettings = this.Settings.Where(s => s.AudioSetting.AudioPath == setting.AudioSetting.AudioPath);//.ToList();

                if (mmdSettings.Count() > 0)
                {
                    var savedSettings = mmdSettings.ToList();
                    foreach (var savedSetting in savedSettings)
                    {
                        // 进行音频设定对比
                        isAudioSame = savedSetting.AudioSetting.Compare(setting.AudioSetting);

                        if (isAudioSame)
                        {
                            // 进行镜头设定对比
                            isCameraSame = savedSetting.CameraSetting.Compare(setting.CameraSetting);

                            if (isCameraSame)
                            {
                                // 人物动作数量一致则进行进一步判定，否则直接判定为不一致
                                if (savedSetting.Motions.Count == setting.Motions.Count)
                                {
                                    // 如果动作配置中有动作文件配置
                                    var motions = setting.Motions.Where(m => m.Files.Count > 0).ToArray();

                                    // 循环检查人物动作设置
                                    foreach (var motion in motions)
                                    {
                                        // 检查是否有相同的配置
                                        isMotionSame = savedSetting.Motions.Where(m => m.Files.SequenceEqual(motion.Files)).Any();

                                        // 检查通过继续进行下一个动作的检查
                                        if (isMotionSame)
                                        {
                                            continue;
                                        }
                                        else
                                        {
                                            break;
                                        }
                                    }
                                }
                                else
                                {
                                    isMotionSame = false;
                                    break;
                                }
                            }
                        }

                        // 如果找到一致的设置，跳出循环
                        if (isAudioSame && isCameraSame && isMotionSame)
                        {
                            break;
                        }
                    }
                }
                else
                {
                    isAudioSame = false;
                }

                // 如果任意一项不相同，则添加配置
                if (!isAudioSame || !isCameraSame || !isMotionSame)
                {
                    this.Settings.Add(setting);
                    somethingChanged = true;
                }
            }

            // 如果有内容被更改了，则进行保存
            if (somethingChanged)
            {
                this.NeedSave = true;
            }
        }

        /// <summary>
        /// 从仓库加载对象
        /// </summary>
        /// <param name="fileName"></param>
        /// <returns></returns>
        internal static MMDEntity LoadFromStoreByGUID(string guid, string packageName = null)
        {
            var fileName = $"{STORE_PATH}\\{guid}\\entity.{SAVE_EXT}";

            if (!string.IsNullOrEmpty(packageName))
            {
                fileName = packageName + ":/" + fileName;
            }

            if (!string.IsNullOrEmpty(packageName) || FileManagerSecure.FileExists(fileName))
            {
                var json = SuperController.singleton.LoadJSON(fileName);

                var entity = LoadFromJSONClass(json as JSONClass);

                if (entity != null && !string.IsNullOrEmpty(packageName))
                {
                    entity.PackageName = packageName;

                    foreach (var setting in entity.Settings)
                    {
                        if (setting.AudioSetting.AudioPath.StartsWith("SELF"))
                        {
                            setting.AudioSetting.AudioPath = setting.AudioSetting.AudioPath.Replace("SELF", packageName);
                        }
                        else
                        {
                            setting.AudioSetting.AudioPath = packageName + ":/" + setting.AudioSetting.AudioPath;
                        }

                        setting.CameraSetting.CameraPath = packageName + ":/" + setting.CameraSetting.CameraPath;

                        foreach (var motion in setting.Motions)
                        {
                            motion.Files = motion.Files.Select(f => packageName + ":/" + f).ToList();
                        }
                    }
                }

                return entity;
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// 根据路径加载MMDEntiy
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        internal static MMDEntity LoadFromStorByPath(string path)
        {
            var key = FileManagerSecure.NormalizePath(path);

            var entity = LoadFromStoreByGUID(GetGUIDByPath(key));

            if (entity == null)
            {
                entity = new MMDEntity
                {
                    Key = key,
                    Title = FileManagerSecure.GetFileName(path)
                };
            }

            return entity;
        }

        /// <summary>
        /// 根据路径获取GUID
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        internal static string GetGUIDByPath(string path)
        {
            using (var md5 = new MD5CryptoServiceProvider())
                return new Guid(md5.ComputeHash(Encoding.Default.GetBytes(path))).ToString();
        }

        /// <summary>
        /// 获取所有GUID的列表
        /// </summary>
        /// <returns></returns>
        internal static List<string> GetAllGUID()
        {
            // 获取仓库的所有子目录
            var dirs = FileManagerSecure.GetDirectories(MMDEntity.STORE_PATH);

            return dirs.Select(d => FileManagerSecure.GetFileName(d)).ToList();
        }

        #endregion
    }
}
