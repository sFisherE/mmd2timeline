using mmd2timeline.Store;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace mmd2timeline
{
    internal partial class Playlist
    {
        /// <summary>
        /// 播放列表的保存目录
        /// </summary>
        private static string PLAYLIST_PATH
        {
            get
            {
                return Config.saveDataPath + "";
            }
        }
        private const string PLAYLIST_SAVE_EXT = "list";

        /// <summary>
        /// 播放列表变化的事件
        /// </summary>
        public event Action<Playlist, List<PlaylistItem>> PlaylistUpdated;
        /// <summary>
        /// 播放项目选中的事件
        /// </summary>
        public event Action<Playlist, PlaylistItem> PlayItemSelected;
        /// <summary>
        /// 播放项目重播的事件
        /// </summary>
        public event Action<Playlist, PlaylistItem> PlayItemRepeat;
        /// <summary>
        /// 播放项目添加的事件
        /// </summary>
        public event Action<Playlist, PlaylistItem> PlayItemAdded;
        /// <summary>
        /// 播放项目移除的事件
        /// </summary>
        public event Action<Playlist, PlaylistItem> PlayItemRemoved;

        public event Action<string, int, int> PlayItemImportProgressChanged;

        /// <summary>
        /// 播放列表中收录的播放项目
        /// </summary>
        protected List<PlaylistItem> List = new List<PlaylistItem>();
        /// <summary>
        /// 播放列表
        /// </summary>
        private List<PlaylistItem> _PlayList;
        /// <summary>
        /// 获取播放列表
        /// </summary>
        protected List<PlaylistItem> PlayList
        {
            get
            {
                if (_PlayList == null)
                {
                    ResetPlayList();
                }
                return _PlayList;
            }
        }

        PlaylistItem _CurrentItem;
        /// <summary>
        /// 当前播放项目
        /// </summary>
        public PlaylistItem CurrentItem
        {
            get
            {
                return _CurrentItem;
            }
            set
            {
                if (_CurrentItem != value)
                {
                    _CurrentItem = value;

                    PlayItemSelected?.Invoke(this, _CurrentItem);
                }
            }
        }

        /// <summary>
        /// 获得当前的MMD数据
        /// </summary>
        public MMDEntity CurrentMMD
        {
            get
            {
                if (CurrentItem != null)
                {
                    return MMDEntity.LoadFromStoreByGUID(CurrentItem.GUID);
                }
                else
                {
                    return null;
                }
            }
        }

        /// <summary>
        /// 获取播放列表中的内容条数
        /// </summary>
        internal int MMDCount
        {
            get
            {
                return List.Count;
            }
        }

        /// <summary>
        /// 获取播放列表
        /// </summary>
        /// <returns></returns>
        public List<PlaylistItem> GetPlayList()
        {
            return this.List;
        }

        /// <summary>
        /// 重置随机播放列表
        /// </summary>
        /// <returns></returns>
        private List<PlaylistItem> ResetPlayList()
        {
            _PlayList = null;

            // 如果是随机模式，则重新组织
            if (this.PlayMode == MMDPlayMode.Random)
            {
                var dict = new Dictionary<PlaylistItem, int>();

                foreach (var item in List)
                {
                    dict.Add(item, UnityEngine.Random.Range(0, 100000));
                }

                _PlayList = dict.OrderBy(d => d.Value).Select(d => d.Key).ToList();
            }
            else
            {
                _PlayList = List.ToArray().ToList();
            }

            return _PlayList;
        }

        #region Select
        /// <summary>
        /// 根据GUID选中播放列表中的项目
        /// </summary>
        /// <param name="GUID"></param>
        public void Select(string guid)
        {
            var targetItem = PlayList.FirstOrDefault(l => l.GUID == guid);

            if (targetItem == null)
            {
                return;
            }

            CurrentItem = targetItem;
        }

        /// <summary>
        /// 根据播放项目选择播放列表中的项目
        /// </summary>
        /// <param name="item"></param>
        public void Select(PlaylistItem item)
        {
            this.Select(item.GUID);
        }

        /// <summary>
        /// 根据名称选择播放列表中的内容
        /// </summary>
        /// <param name="name"></param>
        public void SelectByName(string name)
        {
            var targetItem = PlayList.FirstOrDefault(l => l.Name == name);

            if (targetItem == null)
            {
                return;
            }

            CurrentItem = targetItem;
        }
        #endregion

        /// <summary>
        /// 第一个
        /// </summary>
        public void First()
        {
            var firstItem = PlayList.FirstOrDefault();

            if (firstItem != null && firstItem.GUID == CurrentItem?.GUID)
            {
                PlayItemRepeat?.Invoke(this, CurrentItem);
            }
            else
            {
                CurrentItem = firstItem;
            }
        }

        /// <summary>
        /// 选择上一个内容
        /// </summary>
        /// <returns></returns>
        public void Prev()
        {
            this.NextOrPrev(prev: true);
        }

        /// <summary>
        /// 选择下一个内容
        /// </summary>
        public void Next()
        {
            this.NextOrPrev(prev: false);
        }

        /// <summary>
        /// 选择下一个或上一个播放内容
        /// </summary>
        /// <param name="prev">是否选择上一个，默认false，即默认选择下一个</param>
        /// <returns></returns>
        public void NextOrPrev(bool prev = false)
        {
            if (PlayList.Count == 0)
            {
                return;
            }

            if (CurrentItem == null)
            {
                CurrentItem = PlayList.FirstOrDefault();

                return;
            }

            var nextItem = _CurrentItem;

            if (this.PlayMode != MMDPlayMode.Repeat)
            {
                int index = GetNextIndex(prev);
                nextItem = PlayList[index];
            }

            // 如果下一个内容与当前内容一致，则触发重播事件，否则更新当前项目
            if (nextItem == _CurrentItem)
            {
                PlayItemRepeat?.Invoke(this, nextItem);
            }
            else
            {
                CurrentItem = nextItem;
            }
        }

        /// <summary>
        /// 根据选择上一个还是下一个选择序列
        /// </summary>
        /// <param name="next"></param>
        /// <returns></returns>
        private int GetNextIndex(bool prev = false)
        {
            var index = PlayList.IndexOf(CurrentItem);

            // 如果没有找到，则返回第一条
            if (index < 0)
            {
                return 0;
            }

            if (prev)
            {
                index--;
                if (index < 0)
                {
                    index = PlayList.Count - 1;
                }
            }
            else
            {
                index++;
                if (index >= PlayList.Count)
                {
                    index = 0;
                }
            }
            return index;
        }

        /// <summary>
        /// 将MMDentity添加到播放列表
        /// </summary>
        /// <param name="entity"></param>
        public void AddPlayItem(MMDEntity entity, bool notify = true)
        {
            this.AddPlayItem(new PlaylistItem
            {
                GUID = entity.GUID,
                Name = entity.Title,
                Setting = entity.SettingIndex
            }, notify);
        }

        /// <summary>
        /// 向播放列表添加项目
        /// </summary>
        /// <param name="item"></param>
        public void AddPlayItem(PlaylistItem item, bool notify = true)
        {
            // 获取GUID与配置一致的项目
            var targetItem = this.List.FirstOrDefault(i => i.GUID == item.GUID && i.Setting == item.Setting);

            // 如果项目为空，则将将新的项目添加到列表
            if (targetItem == null)
            {
                this.List.Add(item);
                this.PlayList.Add(item);

                if (notify)
                {
                    PlayItemAdded?.Invoke(this, item);
                }
            }
        }

        /// <summary>
        /// 清理播放列表，以备列表更新后重新生成播放列表
        /// </summary>
        public void ClearPlayList()
        {
            _PlayList?.Clear();
            _PlayList = null;
        }

        /// <summary>
        /// 清理播放列表
        /// </summary>
        public IEnumerator ClearList()
        {
            try
            {
                this.List.Clear();
                this.ClearPlayList();

                var keys = this.Keys.ToList();

                foreach (var key in keys)
                {
                    this.Remove(key);
                }

                keys.Clear();
                keys = null;

                _CurrentItem = null;
            }
            catch (Exception e)
            {
                LogUtil.Debug(e, $"PlayerList::ClearList:");
            }

            OnPlaylistUpdated();

            yield return null;
        }

        /// <summary>
        /// 播放列表更新
        /// </summary>
        private void OnPlaylistUpdated()
        {
            this.ClearPlayList();

            PlaylistUpdated?.Invoke(this, this.List);
        }

        /// <summary>
        /// 从播放列表中删除内容
        /// </summary>
        /// <param name="item"></param>
        public void RemovePlayItem(PlaylistItem item)
        {
            RemovePlayItem(item.GUID, item.Setting);
        }

        /// <summary>
        /// 移除当前播放内容
        /// </summary>
        public void RemovePlayItem()
        {
            this.RemovePlayItem(CurrentItem);
        }

        /// <summary>
        /// 根据GUID获取播放列表项
        /// </summary>
        /// <param name="guid"></param>
        /// <param name="setting"></param>
        /// <returns></returns>
        public PlaylistItem GetPlayItem(string guid, int setting = 0)
        {
            return this.List.FirstOrDefault(i => i.GUID == guid && i.Setting == setting);
        }

        /// <summary>
        /// 根据GUID移除播放项目
        /// </summary>
        /// <param name="guid"></param>
        public void RemovePlayItem(string guid, int setting = 0)
        {
            var targetItem = GetPlayItem(guid, setting);

            if (targetItem != null)
            {
                // 如果目标项目ID与当前项目ID一致，则选择下一项
                if (targetItem.GUID == CurrentItem?.GUID)
                {
                    this.Next();
                }

                this.List?.Remove(targetItem);
                this.PlayList?.Remove(targetItem);

                PlayItemRemoved?.Invoke(this, targetItem);
            }
        }

        static Playlist _Favorites;

        /// <summary>
        /// 获取收藏夹
        /// </summary>
        public static Playlist Favorites
        {
            get
            {
                if (_Favorites == null)
                {
                    _Favorites = new Playlist
                    {
                        Name = "Favorite",
                        Description = "This playlist saves the list of your favorite MMDs.",
                    };

                    _Favorites.LoadFavorite();
                }

                return _Favorites;
            }
        }

        internal static void Favorite(MMDEntity entity)
        {
            try
            {
                var favList = Favorites;

                favList.AddPlayItem(entity);

                favList.Save();
            }
            catch (Exception ex)
            {
                LogUtil.Debug(ex, "Favorite");
            }
        }

        internal static void UnFavorite(MMDEntity entity)
        {
            var favList = Favorites;

            favList.RemovePlayItem(new PlaylistItem()
            {
                Name = entity.Title,
                GUID = entity.GUID,
                Setting = entity.SettingIndex
            });

            favList.Save();
        }

        /// <summary>
        /// 获取默认的播放列表
        /// </summary>
        /// <returns></returns>
        internal static Playlist GetDefault()
        {
            var list = new Playlist
            {
                Name = "Defalut",
                Description = "This playlist saves the list of your default MMDs."
            };

            list.LoadDefault();

            return list;
        }

        /// <summary>
        /// 保存当前配置到默认播放列表
        /// </summary>
        internal void SaveToDefault()
        {
            this.SaveTo(DefaultFileName);
        }

        /// <summary>
        /// 保存到默认播放列表
        /// </summary>
        /// <param name="playlist"></param>
        internal static void SaveToDefault(Playlist playlist)
        {
            playlist.Save(DefaultFileName);
        }
    }
}
