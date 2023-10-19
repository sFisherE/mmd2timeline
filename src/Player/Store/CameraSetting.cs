using SimpleJSON;
using System.Linq;

namespace mmd2timeline.Store
{
    /// <summary>
    /// 镜头设置
    /// </summary>
    internal class CameraSetting : JSONClass
    {
        /// <summary>
        /// 镜头文件路径
        /// </summary>
        public string CameraPath
        {
            get
            {
                if (this.HasKey("CameraPath"))
                    return this["CameraPath"];
                else return null;
            }
            set
            {
                this["CameraPath"] = value;
            }
        }
        /// <summary>
        /// 位置偏移X
        /// </summary>
        public float PositionOffsetX
        {
            get
            {
                if (this.HasKey("PositionOffsetX"))
                    return this["PositionOffsetX"].AsFloat;
                else return 0f;
            }
            set
            {
                this["PositionOffsetX"].AsFloat = value;
            }
        }
        /// <summary>
        /// 位置偏移Y
        /// </summary>
        public float PositionOffsetY
        {
            get
            {
                if (this.HasKey("PositionOffsetY"))
                    return this["PositionOffsetY"].AsFloat;
                else return -0.1f;
            }
            set { this["PositionOffsetY"].AsFloat = value; }
        }
        /// <summary>
        /// 位置偏移Z
        /// </summary>
        public float PositionOffsetZ
        {
            get
            {
                if (this.HasKey("PositionOffsetZ"))
                    return this["PositionOffsetZ"].AsFloat;
                else
                    return 0f;
            }
            set
            {
                this["PositionOffsetZ"].AsFloat = value;
            }
        }
        /// <summary>
        /// 方向偏移X
        /// </summary>
        public float RotationOffsetX
        {
            get
            {
                if (this.HasKey("RotationOffsetX"))
                    return this["RotationOffsetX"].AsFloat;
                else
                    return 0f;
            }
            set
            {
                this["RotationOffsetX"].AsFloat = value;
            }
        }
        /// <summary>
        /// 方向偏移Y
        /// </summary>
        public float RotationOffsetY
        {
            get
            {
                if (this.HasKey("RotationOffsetY"))
                    return this["RotationOffsetY"].AsFloat;
                else
                    return 0f;
            }
            set
            {
                this["RotationOffsetY"].AsFloat = value;
            }
        }
        /// <summary>
        /// 方向偏移Z
        /// </summary>
        public float RotationOffsetZ
        {
            get
            {
                if (this.HasKey("RotationOffsetZ"))
                    return this["RotationOffsetZ"].AsFloat;
                else
                    return 0f;
            }
            set
            {
                this["RotationOffsetZ"].AsFloat = value;
            }
        }

        /// <summary>
        /// 镜头动作开始时间
        /// </summary>
        public float TimeDelay
        {
            get
            {
                if (this.HasKey("TimeDelay"))
                    return this["TimeDelay"].AsFloat;
                else
                    return 0f;
            }
            set
            {
                this["TimeDelay"].AsFloat = value;
            }
        }

        /// <summary>
        /// 镜头缩放
        /// </summary>
        public float CameraScale
        {
            get
            {
                if (this.HasKey("CameraScale"))
                    return this["CameraScale"].AsFloat;
                else
                    return 1f;
            }
            set
            {
                this["CameraScale"].AsFloat = value;
            }
        }

        /// <summary>
        /// 对比两个镜头配置是否一致
        /// </summary>
        /// <param name="target"></param>
        /// <returns></returns>
        public bool Compare(CameraSetting target)
        {
            bool isCameraSame = true;

            var cameraSettingKeys = this.Keys.ToList();

            if (target.Keys.Count() == cameraSettingKeys.Count)
            {
                foreach (var key in cameraSettingKeys)
                {
                    if (this[key].Value != target[key].Value)
                    {
                        isCameraSame = false;
                        break;
                    }
                }
            }
            else
            {
                isCameraSame = false;
            }

            cameraSettingKeys.Clear();
            cameraSettingKeys = null;

            return isCameraSame;
        }
    }
}
