namespace mmd2timeline.Store
{
    internal partial class MMDEntity
    {
        /// <summary>
        /// 收藏
        /// </summary>
        internal void Favorite()
        {
            // 收藏前保存以下当前对象
            this.Save();

            Playlist.Favorite(this);
        }

        /// <summary>
        /// 取消收藏
        /// </summary>
        internal void UnFavorite()
        {
            Playlist.UnFavorite(this);
        }

        /// <summary>
        /// 检查项目是否包含在收藏列表中
        /// </summary>
        internal bool InFavorite
        {
            get
            {
                return null != Playlist.Favorites.GetPlayItem(this.GUID, this.SettingIndex);
            }
        }
    }
}
