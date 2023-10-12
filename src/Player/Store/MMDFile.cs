using MVR.FileManagementSecure;
using SimpleJSON;

namespace mmd2timeline.Store
{
    /// <summary>
    /// 文件信息
    /// </summary>
    internal class MMDFile : JSONClass
    {
        private MMDFile() { }

        public MMDFile(string fileName)
        {
            this.FileName = MMDEntity.FileNameConvert(fileName);
        }

        /// <summary>
        /// 文件名称
        /// </summary>
        public string FileName
        {
            get
            {
                return this["FileName"];
            }
            private set
            {
                this["FileName"] = value;
                this.FileType = GetFileType(this.Name);
            }
        }

        /// <summary>
        /// 对比文件名称是否一致
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public bool FileNameContains(string value)
        {
            return this.FileName.Contains(MMDEntity.FileNameConvert(value));
        }

        /// <summary>
        /// 名称
        /// </summary>
        public string Name
        {
            get
            {
                return FileManagerSecure.GetFileName(this.FileName);
            }
        }

        /// <summary>
        /// 文件类型
        /// </summary>
        public int FileType
        {
            get
            {
                var fileType = this["FileType"];

                return fileType.AsInt;
            }
            internal set { this["FileType"].AsInt = (int)value; }
        }

        /// <summary>
        /// 文件长度
        /// </summary>
        public float Length
        {
            get
            {
                if (this.HasKey("Length"))
                    return this["Length"].AsFloat;
                else return 0f;
            }
            internal set { this["Length"].AsFloat = value; }
        }

        /// <summary>
        /// 指示此文件是否被选中
        /// </summary>
        public bool Selected
        {
            get
            {
                if (this.HasKey("Selected"))
                    return this["Selected"].AsBool;
                else return false;
            }
            internal set { this["Selected"].AsBool = value; }
        }

        #region 文件类型判定函数

        /// <summary>
        /// 根据文件名称获取文件类型
        /// </summary>
        /// <param name="file"></param>
        /// <returns></returns>
        private int GetFileType(string file)
        {
            return this.IsMusic(file) ? MMDFileType.Music :
                this.IsCam(file) ? MMDFileType.Camera :
                this.IsMotion(file) ? MMDFileType.Motion :
                this.IsFace(file) ? MMDFileType.Expression :
                file.ToLower().EndsWith(".vmd") ? MMDFileType.VMD : MMDFileType.Other;
        }

        /// <summary>
        /// 判断一个文件名是否是音乐文件
        /// </summary>
        /// <param name="s"></param>
        /// <returns></returns>
        private bool IsMusic(string s)
        {
            string path = s.ToLower();

            return path.EndsWith(".mp3")
                || path.EndsWith(".wav")
                || path.EndsWith(".ogg");
        }

        /// <summary>
        /// 判定文件是否为表情文件
        /// </summary>
        /// <param name="s"></param>
        /// <returns></returns>
        bool IsFace(string s)
        {
            string path = s.ToLower();
            return path.EndsWith(".vmd")
                && (path.Contains("facial")
                || path.Contains("eye")
                || path.Contains("express")
                || path.Contains("face")
                || path.Contains("lip")
                || path.Contains("表情")
                || path.Contains("口型"));
        }

        /// <summary>
        /// 判定文件是否为动作文件
        /// </summary>
        /// <param name="s"></param>
        /// <returns></returns>
        bool IsMotion(string s)
        {
            string path = s.ToLower();
            return path.EndsWith(".vmd")
                && (path.Contains("motion")
                || path.Contains("动作")
                || path.Contains("이동")
                || path.Contains("ムーブメント"));
        }

        /// <summary>
        /// 判定文件是否为镜头文件
        /// </summary>
        /// <param name="s"></param>
        /// <returns></returns>
        bool IsCam(string s)
        {
            string path = s.ToLower();
            return path.EndsWith(".vmd")
                && (path.Contains("cam")
                || path.Contains("相机")
                || path.Contains("镜头")
                || path.Contains("カメラ")
                || path.Contains("카메라"));
        }
        #endregion

        ///// <summary>
        ///// 文件类型名称清单
        ///// </summary>
        //public static List<string> TypeNames
        //{
        //    get
        //    {

        //        return Enum.GetNames(typeof(MMDFileType)).ToList();
        //    }
        //}

        //private static List<string> _TypeValues = null;

        ///// <summary>
        ///// 文件类型值清单
        ///// </summary>
        //public static List<string> TypeValues
        //{
        //    get
        //    {
        //        if (_TypeValues == null)
        //        {
        //            _TypeValues = new List<string>();
        //            var values = Enum.GetValues(typeof(MMDFileType));

        //            foreach (var v in values)
        //            {
        //                _TypeValues.Add($"{v}");
        //            }
        //        }

        //        return _TypeValues;
        //    }
        //}

        ///// <summary>
        ///// 从字符串获取类型
        ///// </summary>
        ///// <param name="s"></param>
        ///// <returns></returns>
        //public static MMDFileType GetTypeFromString(string s)
        //{
        //    return (MMDFileType)Enum.Parse(typeof(MMDFileType), s);
        //}

        /// <summary>
        /// 转换文件类型数据
        /// </summary>
        /// <param name="fileType"></param>
        /// <returns></returns>
        internal static int ConvertFileType(JSONNode fileType)
        {
            if (fileType.AsInt > 0 || fileType == "0")
            {
                return fileType.AsInt;
            }
            else //字符串检查
            {
                switch (fileType)
                {
                    case "cam":
                        return MMDFileType.Camera;
                    case "0":
                    case "music":
                        return MMDFileType.Music;
                    case "motion":
                        return MMDFileType.Motion;
                    case "face":
                        return MMDFileType.Expression;
                    case "vmd":
                        return MMDFileType.VMD;
                    default:
                        return MMDFileType.Other;
                }
            }
        }
    }
}
