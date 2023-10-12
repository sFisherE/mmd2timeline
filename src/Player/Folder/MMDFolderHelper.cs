using mmd2timeline.Store;
using MVR.FileManagement;
using MVR.FileManagementSecure;
using System;
using System.Collections;
using System.Linq;
using UnityEngine;

namespace mmd2timeline
{
    /// <summary>
    /// MMD文件夹助手
    /// </summary>
    internal class MMDFolderHelper
    {
        #region 单例的处理
        private static MMDFolderHelper _instance;
        private static object _lock = new object();

        /// <summary>
        /// 播放列表管理员的单例
        /// </summary>
        public static MMDFolderHelper GetInstance()
        {
            lock (_lock)
            {
                if (_instance == null)
                {
                    _instance = new MMDFolderHelper();
                }

                return _instance;
            }
        }
        #endregion

        /// <summary>
        /// 读取的文件类型
        /// </summary>
        private const string FILE_FORMAT = "vmd|mp3|wav|ogg";

        /// <summary>
        /// 默认MMD存储路径
        /// </summary>
        private readonly string _MMDStorePath = FileManagerSecure.GetFullPath("MMD");

        /// <summary>
        /// MMD目录存储路径
        /// </summary>
        string _MMDFolder;

        /// <summary>
        /// 临时MMDEntity
        /// </summary>
        internal MMDEntity tempMMDEntity = null;

        /// <summary>
        /// MMD选中的回调函数
        /// </summary>
        /// <param name="entity"></param>
        public delegate void MMDSelectedCallback(MMDEntity entity, bool addListImmediately = false);

        /// <summary>
        /// MMD被选中的回调函数
        /// </summary>
        public event MMDSelectedCallback OnMMDSelected;

        /// <summary>
        /// MMD导入的回调函数
        /// </summary>
        public event Action<MMDEntity, string, int, int> OnMMDImported;

        /// <summary>
        /// 实例化MMD目录管理员
        /// </summary>
        private MMDFolderHelper()
        {
            Init();
        }

        /// <summary>
        /// 初始化
        /// </summary>
        private void Init()
        {
            if (FileManagerSecure.DirectoryExists(_MMDStorePath))
            {
                _MMDFolder = _MMDStorePath;
            }
            else
            {
                var folder = Application.dataPath.Replace("/", "\\");
                _MMDFolder = folder;
            }
        }

        /// <summary>
        /// 加载目录。本操作将通过目录浏览器加载一个目录，之后调用ImportFolder方法将目录中的文件导入左侧的配置区
        /// </summary>
        /// <exception cref="NotImplementedException"></exception>
        internal void LoadFolder()
        {
            try
            {
                SuperController.singleton.ShowMainHUDAuto();
                SuperController.singleton.directoryBrowserUI.shortCuts = null;
                SuperController.singleton.directoryBrowserUI.showFiles = true;
                SuperController.singleton.directoryBrowserUI.fileFormat = FILE_FORMAT;
                SuperController.singleton.directoryBrowserUI.defaultPath = _MMDFolder;
                SuperController.singleton.directoryBrowserUI.SetTextEntry(b: true);
                SuperController.singleton.directoryBrowserUI.Show(path =>
                {
                    if (!string.IsNullOrEmpty(path))
                    {
                        _MMDFolder = path;

                        // 加载目录时，强制更新默认配置
                        this.tempMMDEntity = ImportFolder(path, forceUpdate: true);

                        OnMMDSelected?.Invoke(this.tempMMDEntity);
                    }
                });
            }
            catch (Exception ex)
            {
                LogUtil.Debug(ex, $"LoadFolder Error");
            }
        }

        /// <summary>
        /// 加载文件
        /// </summary>
        internal void LoadFile(MMDEntity entity = null)
        {
            try
            {
                SuperController.singleton.ShowMainHUDAuto();
                SuperController.singleton.GetMediaPathDialog(path =>
                {
                    if (!string.IsNullOrEmpty(path))
                    {
                        ImportFile(path, entity);
                    }
                }, FILE_FORMAT, _MMDFolder);

            }
            catch (Exception ex)
            {
                LogUtil.Debug(ex, $"LoadFile Error");
            }
        }

        /// <summary>
        /// 从目录加载MMD
        /// </summary>
        internal void ImportFromFolder()
        {
            try
            {
                SuperController.singleton.ShowMainHUDAuto();
                SuperController.singleton.directoryBrowserUI.shortCuts = null;
                SuperController.singleton.directoryBrowserUI.showFiles = true;
                SuperController.singleton.directoryBrowserUI.fileFormat = FILE_FORMAT;
                SuperController.singleton.directoryBrowserUI.defaultPath = _MMDFolder;
                SuperController.singleton.directoryBrowserUI.SetTextEntry(b: true);
                SuperController.singleton.directoryBrowserUI.Show(path =>
                {
                    if (!string.IsNullOrEmpty(path))
                    {
                        _MMDFolder = path;

                        SuperController.singleton.StartCoroutine(ImportFromFolder(path));
                    }
                });
            }
            catch (Exception ex)
            {
                LogUtil.Debug(ex, $"ImportFromFolder Error");
            }
        }

        /// <summary>
        /// 从目录中逐个导入MMD配置
        /// </summary>
        /// <param name="path"></param>
        IEnumerator ImportFromFolder(string path)
        {
            var paths = FileManagerSecure.GetDirectories(path);

            for (var i = 0; i < paths.Length; i++)
            {
                var p = paths[i];
                // 逐个目录加载时不强制更新默认配置
                var entity = ImportFolder(p, forceUpdate: false);
                OnMMDImported?.Invoke(entity, $"{entity.Title}", i, paths.Length);
                yield return null;
            }

            // 执行结束后触发最后一个导入完成事件
            OnMMDImported?.Invoke(null, $"The path is\n\n{path}.", paths.Length, paths.Length);

            this.tempMMDEntity = null;
        }

        /// <summary>
        /// 检查目录是否安全可读取
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        private bool CheckSecureRead(string path)
        {
            return FileManager.IsSecureReadPath(path);
        }

        /// <summary>
        /// 检查目录是否安全可写入
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        private bool CheckSecureWrite(string path)
        {
            return FileManager.IsSecureWritePath(path);
        }

        /// <summary>
        /// 导入目录中的文件。
        /// </summary>
        /// <param name="path"></param>
        /// <exception cref="NotImplementedException"></exception>
        /// <remarks>此方法将读取一个目录中的文件清单，并根据文件扩展名允许用户将其配置到MMD数据。（根据VAM的插件安全规则，目录只能是VAM工作做目录才能进行操作）</remarks>
        private MMDEntity ImportFolder(string path, bool forceUpdate = false)
        {
            try
            {
                // 初始化临时MMDEntity对象
                var entity = MMDEntity.LoadFromStorByPath(path);

                // 获取文件列表
                string[] files = GetFiles(path).Where(f =>
                {
                    var lower = f.ToLower();
                    return (lower.EndsWith(".vmd") || lower.EndsWith(".mp3") || lower.EndsWith(".ogg") || lower.EndsWith(".wav"));
                }).ToArray();

                if (files.Length > 0)
                {
                    foreach (string file in files)
                    {
                        var mmdFile = entity.Files.FirstOrDefault(f => f.FileName == file);

                        if (mmdFile == null)
                        {
                            mmdFile = new MMDFile(file);

                            entity.Files.Add(mmdFile);

                            entity.NeedSave = true;
                        }
                    }
                }

                entity.InitMotionFiles(forceUpdate);

                if (entity.NeedSave)
                {
                    entity.Save();
                }
                return entity;
            }
            catch (Exception e)
            {
                LogUtil.LogError(e, "ImportFolder::");
            }

            return null;
        }

        /// <summary>
        /// 导入文件
        /// </summary>
        /// <param name="file"></param>
        void ImportFile(string file, MMDEntity entity)
        {
            try
            {
                file = SuperController.singleton.NormalizeLoadPath(file);

                LogUtil.Debug($"ImportFile:{file}");

                if (entity != null)
                {
                    this.tempMMDEntity = entity.Clone();
                }

                var path = FileManagerSecure.GetDirectoryName(file);
                _MMDFolder = path;

                // 如果没有找到MMD对象
                if (tempMMDEntity == null)
                {
                    tempMMDEntity = MMDEntity.LoadFromStorByPath(path);
                }

                if (!tempMMDEntity.Files.Any(f => f.FileNameContains(file)))
                {
                    tempMMDEntity.Files.Add(new MMDFile(file));
                    tempMMDEntity.InitMotionFiles(forceUpdate: true);
                    tempMMDEntity.NeedSave = false;
                }

                OnMMDSelected?.Invoke(tempMMDEntity);
            }
            catch (Exception e)
            {
                LogUtil.LogError(e);
            }
        }

        /// <summary>
        /// 从指定目录中获取文件清单
        /// </summary>
        /// <param name="path">目录地址</param>
        /// <param name="depth">深度。代表最多只获取多少层目录</param>
        /// <returns></returns>
        private static string[] GetFiles(string path, int currentDepth = 0, int depth = 3)
        {
            // 获取目录中的文件
            var files = FileManagerSecure.GetFiles(path);

            if (currentDepth > depth)
            {
                return files;
            }

            // 获取子目录
            var dirs = FileManagerSecure.GetDirectories(path);

            if (dirs.Length > 0)
            {
                currentDepth++;
                foreach (var dir in dirs)
                {
                    // 获取目录中的文件
                    var dirFiles = GetFiles(dir, currentDepth, depth);

                    files = files.Concat(dirFiles).ToArray();
                }
            }

            return files;
        }

        /// <summary>
        /// 清除临时配置信息
        /// </summary>
        internal void Clear()
        {
            tempMMDEntity = null;
        }

        /// <summary>
        /// 销毁
        /// </summary>
        internal void Dispose()
        {
            this.Clear();

            _instance = null;
        }
    }
}
