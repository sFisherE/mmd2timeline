using System;
using System.Collections.Generic;
using System.Linq;

namespace mmd2timeline
{
    internal partial class Player
    {
        /// <summary>
        /// 存储播放列表的对象。建议调用时使用Playlist对象
        /// </summary>
        private Playlist _playlist;

        /// <summary>
        /// 获取播放列表
        /// </summary>
        internal Playlist Playlist
        {
            get
            {
                if (_playlist == null)
                {
                    _playlist = new Playlist { script = this };

                    InitPlaylistEvent();
                }

                return _playlist;
            }
        }

        /// <summary>
        /// 选择播放项目
        /// </summary>
        /// <param name="value"></param>
        void SelectPlayItem(string value)
        {
            try
            {
                this.Playlist.Select(value);
            }
            catch (Exception e)
            {
                LogUtil.Debug(e, "Player::SelectPlayItem:");
            }
        }

        /// <summary>
        /// 初始化播放列表的事件绑定
        /// </summary>
        void InitPlaylistEvent()
        {
            Playlist.PlayItemAdded += Playlist_PlayItemAdded;
            Playlist.PlayItemRemoved += Playlist_PlayItemRemoved;
            Playlist.PlayItemRepeat += Playlist_PlayItemRepeat;
            Playlist.PlayItemSelected += Playlist_PlayItemSelected;
            Playlist.PlaylistUpdated += OnPlaylistUpdated;

            Playlist.PlayItemImportProgressChanged += Playlist_PlayItemImportProgressChanged;
        }

        /// <summary>
        /// 移除播放列表的事件绑定
        /// </summary>
        void RemovePlaylistEvent()
        {
            Playlist.PlayItemAdded -= Playlist_PlayItemAdded;
            Playlist.PlayItemRemoved -= Playlist_PlayItemRemoved;
            Playlist.PlayItemRepeat -= Playlist_PlayItemRepeat;
            Playlist.PlayItemSelected -= Playlist_PlayItemSelected;
            Playlist.PlaylistUpdated -= OnPlaylistUpdated;
            Playlist.PlayItemImportProgressChanged -= Playlist_PlayItemImportProgressChanged;
        }

        private void Playlist_PlayItemImportProgressChanged(string msg, int step, int total)
        {
            ShowImportProgress(msg, step, total);
        }

        /// <summary>
        /// 当播放列表更新的事件发生时，更新播放列表
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="list"></param>
        private void OnPlaylistUpdated(Playlist sender, List<PlaylistItem> list)
        {
            LogUtil.Debug($"Playlist_PlaylistUpdated::{list.Count}");

            if (list.Count > 0)
            {
                var playlistNames = list.Select(x => x.Name).ToList();
                _PlaylistChooser.displayChoices = playlistNames;

                var playlistGUIDs = list.Select(x => x.GUID).ToList();
                _PlaylistChooser.choices = playlistGUIDs;

                ShowPlayUI();

                this.Playlist.First();
            }
            else
            {
                ClearPlayList();
            }
        }

        /// <summary>
        /// 当播放项目被选定时的事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="arg"></param>
        private void Playlist_PlayItemSelected(Playlist sender, PlaylistItem arg)
        {
            if (arg == null)
            {
                LogUtil.Debug($"Player::Playlist_PlayItemSelected:arg==null");
                return;
            }

            if (_PlaylistChooser.val != arg.GUID)
            {
                _PlaylistChooser.SetVal(arg.GUID);
            }
            else
            {
                _PlaylistChooser.valNoCallback = arg.GUID;
            }

            LoadMMD(this.Playlist.CurrentMMD);
        }

        /// <summary>
        /// 播放项目重新播放的事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="arg"></param>
        private void Playlist_PlayItemRepeat(Playlist sender, PlaylistItem arg)
        {
            StartCoroutine(Repeat());
        }

        /// <summary>
        /// 播放项目移除事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="arg"></param>
        private void Playlist_PlayItemRemoved(Playlist sender, PlaylistItem arg)
        {
            var index = _PlaylistChooser.choices.IndexOf(arg.GUID);

            var choices = _PlaylistChooser.choices.ToArray().ToList();
            choices.RemoveAt(index);
            var displayChoices = _PlaylistChooser.displayChoices.ToArray().ToList();
            displayChoices.RemoveAt(index);

            if (choices.Count > 0)
            {
                _PlaylistChooser.choices = choices;
                _PlaylistChooser.displayChoices = displayChoices;
            }
            else
            {
                ClearPlayList();
            }
        }

        /// <summary>
        /// 播放项目添加的事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="arg"></param>
        private void Playlist_PlayItemAdded(Playlist sender, PlaylistItem arg)
        {
            if (!_PlaylistChooser.choices.Contains(arg.GUID))
            {
                var choices = _PlaylistChooser.choices.ToArray().ToList();
                choices.Add(arg.GUID);

                var displayChoices = _PlaylistChooser.displayChoices.ToArray().ToList();
                displayChoices.Add(arg.Name);

                // 如果列表中有None，则移除之
                var indexNone = choices.IndexOf(noneString);

                if (indexNone > -1)
                {
                    choices.RemoveRange(indexNone, 1);
                    displayChoices.RemoveRange(indexNone, 1);
                }

                _PlaylistChooser.choices = choices;
                _PlaylistChooser.displayChoices = displayChoices;
            }

            //if (_PlaylistChooser.val != arg.GUID)
            //{
            SelectPlayItem(arg.GUID);
            //}
        }
    }
}
