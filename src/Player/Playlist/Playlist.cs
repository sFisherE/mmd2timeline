using mmd2timeline.Store;
using System;

namespace mmd2timeline
{
    /// <summary>
    /// 播放列表对象
    /// </summary>
    internal partial class Playlist : MSJSONClass
    {
        #region 属性
        /// <summary>
        /// 播放列表名称
        /// </summary>
        public string Name
        {
            get
            {
                if (this.HasKey("Name"))
                {
                    return this["Name"];
                }
                else
                {
                    return null;
                }
            }
            set
            {
                if (this.Name != value)
                {
                    this["Name"] = value;
                }
            }
        }
        /// <summary>
        /// 播放列表简介
        /// </summary>
        public string Description
        {
            get
            {
                if (this.HasKey("Description"))
                {
                    return this["Description"];
                }
                else
                {
                    return null;
                }
            }
            set
            {
                if (this.Description != value)
                {
                    this["Description"] = value;
                }
            }
        }

        int _PlayMode = MMDPlayMode.Default;
        /// <summary>
        /// 播放模式
        /// </summary>
        public int PlayMode
        {
            get
            {
                return _PlayMode;
            }
            set
            {
                if (_PlayMode != value)
                {
                    var fromRepeat = _PlayMode == MMDPlayMode.Repeat;
                    var toRepeat = value == MMDPlayMode.Repeat;

                    LogUtil.Debug($"_FromPlayMode::{_PlayMode}");
                    LogUtil.Debug($"_ToPlayMode::{value}");

                    _PlayMode = value;

                    // 如果不是切换到重播模式或从重播模式切换到其他模式
                    if (!fromRepeat && !toRepeat)
                    {
                        // 修改播放模式会重置播放列表
                        ResetPlayList();
                    }
                }
            }
        }
        #endregion

        /// <summary>
        /// 检查是否是新的播放列表模式
        /// </summary>
        private bool IsNewPlaylist
        {
            get
            {
                LogUtil.Debug($"Playlist::IsNewPlaylist:{this.HasKey("MMDShowPlaylist")}");

                if (this.HasKey("MMDShowPlaylist"))
                {
                    return this["MMDShowPlaylist"].Value == "v2.0";
                }
                return false;
            }
        }

        protected override void BeforeToString()
        {
            this["MMDShowPlaylist"] = "v2.0";
            this["List"] = MMDEntity.ListToJSONArray(List);
        }
    }

    /// <summary>
    /// 播放项目
    /// </summary>
    internal class PlaylistItem : MSJSONClass
    {
        public PlaylistItem()
        {
            AddTime = DateTime.UtcNow;
        }

        /// <summary>
        /// 项目名称
        /// </summary>
        public string Name
        {
            get
            {
                if (this.HasKey("Name"))
                {
                    return this["Name"];
                }
                else return null;
            }
            set
            {
                if (this.Name != value)
                {
                    this["Name"] = value;
                }
            }
        }

        /// <summary>
        /// 项目的GUID
        /// </summary>
        public string GUID
        {
            get
            {
                if (this.HasKey("GUID"))
                {
                    return this["GUID"];
                }
                else return null;
            }
            set
            {
                if (this.GUID != value)
                {
                    this["GUID"] = value;
                }
            }
        }

        /// <summary>
        /// 使用项目的配置序号
        /// </summary>
        public int Setting
        {
            get
            {
                if (this.HasKey("Setting"))
                {
                    return this["Setting"].AsInt;
                }
                else return 0;
            }
            set
            {
                if (this.Setting != value)
                {
                    this["Setting"].AsInt = value;
                }
            }
        }

        public DateTime AddTime
        {
            get
            {
                if (this.HasKey("AddTime"))
                {
                    return DateTime.Parse(this["AddTime"]);
                }
                else return DateTime.UtcNow;
            }
            set
            {
                if (this.AddTime != value)
                {
                    this["AddTime"] = value.ToString();
                }
            }
        }

        protected override void BeforeToString()
        {
        }
    }
}
