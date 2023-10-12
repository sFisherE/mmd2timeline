using mmd2timeline.Store;
using MVR.FileManagementSecure;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace mmd2timeline
{
    internal partial class Playlist
    {
        /// <summary>
        /// 执行的插件脚本
        /// </summary>
        internal MVRScript script;

        /// <summary>
        /// 获取收藏内容列表的文件名称
        /// </summary>
        internal static string FavoriteFileName
        {
            get
            {
                return $"{PLAYLIST_PATH}\\fav.{PLAYLIST_SAVE_EXT}";
            }
        }

        /// <summary>
        /// 获取默认播放列表文件名称
        /// </summary>
        internal static string DefaultFileName
        {
            get
            {
                return $"{PLAYLIST_PATH}\\defalut.{PLAYLIST_SAVE_EXT}";
            }
        }

        /// <summary>
        /// 开始协程
        /// </summary>
        /// <param name="enumerator"></param>
        /// <returns></returns>
        Coroutine StartCoroutine(IEnumerator enumerator)
        {
            if (script != null)
            {
                return script.StartCoroutine(enumerator);
            }
            else
            {
                return SuperController.singleton.StartCoroutine(enumerator);
            }
        }

        /// <summary>
        /// 开始导入播放列表。呼出文件窗口
        /// </summary>
        public void BeginImport()
        {
            FileManagerSecure.CreateDirectory(PLAYLIST_PATH);
            var shortcuts = FileManagerSecure.GetShortCutsForDirectory(PLAYLIST_PATH);
            SuperController.singleton.GetMediaPathDialog(HandleLoadPlayListPreset, PLAYLIST_SAVE_EXT, PLAYLIST_PATH, false, true, false, null, false, shortcuts);
        }

        /// <summary>
        /// 开始导出播放列表。呼出文件窗口
        /// </summary>
        public void BeginExport()
        {
            FileManagerSecure.CreateDirectory(PLAYLIST_PATH);
            var fileBrowserUI = SuperController.singleton.fileBrowserUI;
            fileBrowserUI.SetTitle("Save Playlist preset");
            fileBrowserUI.fileRemovePrefix = null;
            fileBrowserUI.hideExtension = false;
            fileBrowserUI.keepOpen = false;
            fileBrowserUI.fileFormat = PLAYLIST_SAVE_EXT;
            fileBrowserUI.defaultPath = PLAYLIST_PATH;
            fileBrowserUI.showDirs = true;
            fileBrowserUI.shortCuts = null;
            fileBrowserUI.browseVarFilesAsDirectories = false;
            fileBrowserUI.SetTextEntry(true);
            fileBrowserUI.Show(HandleSavePlayListPreset);
            fileBrowserUI.fileEntryField.text = (int)(DateTime.UtcNow - new DateTime(1970, 1, 1)).TotalSeconds + "." + PLAYLIST_SAVE_EXT;
            fileBrowserUI.ActivateFileNameField();
        }

        /// <summary>
        /// 从文件加载播放列表
        /// </summary>
        /// <param name="filePath"></param>
        void HandleLoadPlayListPreset(string filePath)
        {
            try
            {
                if (string.IsNullOrEmpty(filePath))
                {
                    return;
                }

                this.StartCoroutine(ImportFromFile(filePath));
            }
            catch (Exception ex)
            {
                LogUtil.Debug(ex, $"LoadList Error");
            }
        }

        /// <summary>
        /// 从文件导入
        /// </summary>
        /// <param name="filename"></param>
        /// <returns></returns>
        IEnumerator ImportFromFile(string filename)
        {
            yield return this.LoadFromFile(filename);
        }

        bool isListLoaded = false;

        /// <summary>
        /// 从文件加载数据到当前列表
        /// </summary>
        /// <param name="filename"></param>
        new IEnumerator LoadFromFile(string filename)
        {
            if (string.IsNullOrEmpty(filename))
            {
                yield break;
            }

            // 先清理掉当前播放列表中的内容
            this.StartCoroutine(this.ClearList());
            yield return null;

            // 从默认播放列表文件加载数据
            this.Load(filename);

            // 移除播放模式
            if (this.HasKey("PlayMode"))
            {
                this.Remove("PlayMode");
            }

            this.StartCoroutine(this.LoadList());

            yield return new WaitWhile(() =>
            {

                if (isListLoaded)
                {
                    OnPlaylistUpdated();
                }

                return !isListLoaded;
            });
        }

        /// <summary>
        /// 保存播放列表
        /// </summary>
        /// <param name="filePath"></param>
        void HandleSavePlayListPreset(string filePath)
        {
            try
            {
                SuperController.singleton.fileBrowserUI.fileFormat = null;

                LogUtil.Debug($"HandleSavePlayListPreset::filePath:{filePath}");
                LogUtil.Debug($"HandleSavePlayListPreset::FavoriteFileName:{FavoriteFileName}");

                if (string.IsNullOrEmpty(filePath) || filePath == FavoriteFileName)
                {
                    return;
                }

                if (!filePath.ToLower().EndsWith(PLAYLIST_SAVE_EXT.ToLower()))
                {
                    filePath += "." + PLAYLIST_SAVE_EXT;
                }

                this.Save(filePath);
            }
            catch (Exception ex)
            {
                LogUtil.Debug(ex, $"SaveList Error ");
            }
        }

        /// <summary>
        /// 从默认播放列表文件导入数据到当前播放列表
        /// </summary>
        public IEnumerator LoadFromDefalut()
        {
            yield return this.LoadFromFile(DefaultFileName);
        }

        /// <summary>
        /// 从收藏夹加载数据到当前播放列表
        /// </summary>
        public IEnumerator LoadFromFavorite()
        {
            yield return this.LoadFromFile(FavoriteFileName);
        }

        /// <summary>
        /// 加载收藏列表到当前列表对象
        /// </summary>
        private void LoadFavorite()
        {
            this.Load(FavoriteFileName);
            this.GetList();
        }

        /// <summary>
        /// 加载默认播放列表数据到当前列表对象
        /// </summary>
        public void LoadDefault()
        {
            this.Load(FavoriteFileName);
            this.GetList();
        }

        void GetList()
        {
            if (this.IsNewPlaylist)
            {
                if (this.HasKey("List"))
                {
                    var list = this["List"].AsArray;

                    var total = list.Count;

                    for (var i = 0; i < total; i++)
                    {
                        var item = list[i].AsObject;

                        this.AddPlayItem(new PlaylistItem().LoadFromJSON<PlaylistItem>(item), notify: false);
                    }

                    this.Remove("List");
                }
            }
        }

        /// <summary>
        /// 加载所有内容到当前播放列表
        /// </summary>
        public IEnumerator LoadAll()
        {
            // 先清理掉当前播放列表中的内容
            this.StartCoroutine(this.ClearList());
            yield return null;

            // 获取仓库的所有子目录
            var guids = MMDEntity.GetAllGUID();
            yield return null;

            LogUtil.Debug($"LoadAll::{guids.Count}");

            var i = 0;

            foreach (var item in guids)
            {
                var entity = MMDEntity.LoadFromStoreByGUID(item);

                if (entity != null)
                {
                    this.AddPlayItem(new PlaylistItem
                    {
                        GUID = entity.GUID,
                        Name = entity.Title,
                        Setting = entity.SettingIndex
                    }, notify: false);

                    PlayItemImportProgressChanged?.Invoke($"\n\nLoading All.\n\n{entity.Title}", i, guids.Count);
                }
                i++;
                yield return null;
            }

            this.List = this.List.OrderBy(e => e.Name).ToList();

            yield return null;

            this.OnPlaylistUpdated();
        }

        /// <summary>
        /// 从JSONClass加载数据
        /// </summary>
        /// <param name="json"></param>
        /// <returns></returns>
        IEnumerator LoadList()
        {
            isListLoaded = false;
            var fileName = FileManagerSecure.GetFileName(SaveFileName);

            LogUtil.Debug($"Playlist::LoadList:{fileName}");

            if (this.IsNewPlaylist)
            {
                if (this.HasKey("List"))
                {
                    var list = this["List"].AsArray;

                    var total = list.Count;

                    for (var i = 0; i < total; i++)
                    {
                        var item = list[i].AsObject;

                        this.AddPlayItem(new PlaylistItem().LoadFromJSON<PlaylistItem>(item), notify: false);

                        PlayItemImportProgressChanged?.Invoke($"\n\nLoading {fileName} Playlist.\n\n{item["Name"]}", i, total);
                        yield return null;
                    }

                    this.Remove("List");
                }
            }
            else
            {
                // 开始导入旧的播放列表格式
                if (this.HasKey("list"))
                {
                    var list = this["list"].AsArray;

                    var total = list.Count;

                    var entities = new List<MMDEntity>();

                    // 将文件中的列表数据转化为MMDEntity
                    for (var i = 0; i < total; i++)
                    {
                        var item = list[i].AsObject;

                        var entity = MMDEntity.LoadFromJSONClass(item);

                        var savedEntity = MMDEntity.LoadFromStoreByGUID(entity.GUID);
                        yield return null;

                        // 如果已经存在保存过的数据
                        if (savedEntity != null)
                        {
                            // 将新的数据与保存过的数据将进行合并
                            savedEntity.Union(entity);

                            this.AddPlayItem(savedEntity, notify: false);

                            savedEntity.NeedSave = true;
                            entities.Add(savedEntity);
                            PlayItemImportProgressChanged?.Invoke($"\n\nLoading {fileName} Playlist.\n\n{savedEntity.Title}", i, total);

                            // 将新的数据保存
                            savedEntity.Save();
                        }
                        else
                        {
                            this.AddPlayItem(entity, notify: false);

                            entity.NeedSave = true;
                            entities.Add(entity);
                            PlayItemImportProgressChanged?.Invoke($"\n\nLoading {fileName} Playlist.\n\n{entity.Title}", i, total);

                            // 将新的数据保存
                            entity.Save();
                        }
                        yield return null;
                    }

                    this.Remove("list");
                }
            }

            isListLoaded = true;
        }
    }
}
