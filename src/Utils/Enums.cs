using System.Collections.Generic;

namespace mmd2timeline
{
    /// <summary>
    /// 枚举类型处理类
    /// </summary>
    /// <remarks>用于统一进行假枚举类型的处理</remarks>
    internal class EnumClass
    {
        private List<string> dict = new List<string>();

        private EnumClass() { }

        /// <summary>
        /// 初始化枚举类
        /// </summary>
        /// <param name="items"></param>
        public EnumClass(params string[] items)
        {
            foreach (var item in items)
            {
                dict.Add(item);
            }
        }

        /// <summary>
        /// 获取名称列表
        /// </summary>
        public List<string> Names
        {
            get
            {
                return dict.ToArray().ToList();
            }
        }

        /// <summary>
        /// 根据名称获取值
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public int GetValue(string name)
        {
            return dict.IndexOf(name);
        }

        /// <summary>
        /// 根据值获取名称
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public string GetName(int value)
        {
            if (value < 0 || value >= dict.Count)
            {
                value = 0;
            }
            return dict[value];
        }
    }

    /// <summary>
    /// 相机控制模式
    /// </summary>
    internal class CameraControlModes
    {
        public const int Original = 0;
        public const int Custom = 1;

        /// <summary>
        /// 初始化枚举类
        /// </summary>
        private static EnumClass enums = new EnumClass("Original", "Custom");

        /// <summary>
        /// 获取名称列表
        /// </summary>
        public static List<string> Names
        {
            get
            {
                return enums.Names;
            }
        }

        /// <summary>
        /// 根据名称获取值
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public static int GetValue(string name)
        {
            return enums.GetValue(name);
        }

        /// <summary>
        /// 根据值获取名称
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static string GetName(int value)
        {
            return enums.GetName(value);
        }
    }

    /// <summary>
    /// 调整骨骼
    /// </summary>
    internal class AdjustBones
    {
        /// <summary>
        /// 肩膀
        /// </summary>
        public const int Shoulder = 0;
        /// <summary>
        /// 手臂
        /// </summary>
        public const int Arm = 1;
        /// <summary>
        /// 肘部
        /// </summary>
        public const int Elbow = 2;
        /// <summary>
        /// 手
        /// </summary>
        public const int Hand = 3;
        /// <summary>
        /// 胯
        /// </summary>
        public const int Thigh = 4;
        /// <summary>
        /// 膝盖
        /// </summary>
        public const int Knee = 5;
        /// <summary>
        /// 足
        /// </summary>
        public const int Foot = 6;
        /// <summary>
        /// 趾
        /// </summary>
        public const int Toe = 7;

        /// <summary>
        /// 初始化枚举类
        /// </summary>
        private static EnumClass enums = new EnumClass("Shoulder", "Arm", "Elbow", "Hand", "Thigh", "Knee", "Foot", "Toe");

        /// <summary>
        /// 获取名称列表
        /// </summary>
        public static List<string> Names
        {
            get
            {
                return enums.Names;
            }
        }

        /// <summary>
        /// 根据名称获取值
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public static int GetValue(string name)
        {
            return enums.GetValue(name);
        }

        /// <summary>
        /// 根据值获取名称
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static string GetName(int value)
        {
            return enums.GetName(value);
        }
    }

    /// <summary>
    /// 播放模式
    /// </summary>
    internal class MMDPlayMode
    {
        /// <summary>
        /// 默认
        /// </summary>
        public const int Default = 0;
        /// <summary>
        /// 随机
        /// </summary>
        public const int Random = 1;
        /// <summary>
        /// 重复播放
        /// </summary>
        public const int Repeat = 2;

        /// <summary>
        /// 初始化枚举类
        /// </summary>
        private static EnumClass enums = new EnumClass("Default", "Random", "Repeat");

        /// <summary>
        /// 获取名称列表
        /// </summary>
        public static List<string> Names
        {
            get
            {
                return enums.Names;
            }
        }

        /// <summary>
        /// 根据名称获取值
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public static int GetValue(string name)
        {
            return enums.GetValue(name);
        }

        /// <summary>
        /// 根据值获取名称
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static string GetName(int value)
        {
            return enums.GetName(value);
        }
    }

    /// <summary>
    /// MMD文件类型
    /// </summary>
    internal class MMDFileType
    {
        public const int Music = 0;
        public const int Camera = 1;
        public const int Motion = 2;
        public const int Expression = 3;
        public const int VMD = 4;
        public const int Other = 5;

        /// <summary>
        /// 初始化枚举类
        /// </summary>
        private static EnumClass enums = new EnumClass("Music", "Camera", "Motion", "Expression", "VMD", "Other");

        /// <summary>
        /// 获取名称列表
        /// </summary>
        public static List<string> Names
        {
            get
            {
                return enums.Names;
            }
        }

        /// <summary>
        /// 根据名称获取值
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public static int GetValue(string name)
        {
            return enums.GetValue(name);
        }

        /// <summary>
        /// 根据值获取名称
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static string GetName(int value)
        {
            return enums.GetName(value);
        }
    }

    /// <summary>
    /// 自动调整高度模式
    /// </summary>
    internal class AutoCorrectHeightMode
    {
        public const int None = 0;
        public const int PartOnly = 1;
        public const int WholeBody = 2;

        /// <summary>
        /// 初始化枚举类
        /// </summary>
        private static EnumClass enums = new EnumClass("None", "PartOnly", "WholeBody");

        /// <summary>
        /// 获取名称列表
        /// </summary>
        public static List<string> Names
        {
            get
            {
                return enums.Names;
            }
        }

        /// <summary>
        /// 根据名称获取值
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public static int GetValue(string name)
        {
            return enums.GetValue(name);
        }

        /// <summary>
        /// 根据值获取名称
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static string GetName(int value)
        {
            return enums.GetName(value);
        }
    }

    /// <summary>
    /// 动作模式
    /// </summary>
    internal class MotionMode
    {
        public const int Custom = 0;// 自定义
        public const int Fatigue = 1; // 乏力 0.1,0.7
        public const int Weak = 2; // 柔弱 0.15,0.7
        public const int Smooth = 3; // 轻柔0.15,0.5
        public const int Normal = 4; // 普通 0.2,0.2
        public const int Strong = 5; // 强壮 0.3,0.5
        public const int Dexterity = 6; // 灵巧0.5,0.2
        public const int Agile = 7; // 敏捷0.7,0.2

        /// <summary>
        /// 初始化枚举类
        /// </summary>
        private static EnumClass enums = new EnumClass("Custom", "Fatigue", "Weak", "Smooth", "Normal", "Strong", "Dexterity", "Agile");

        /// <summary>
        /// 获取名称列表
        /// </summary>
        public static List<string> Names
        {
            get
            {
                return enums.Names;
            }
        }

        /// <summary>
        /// 根据名称获取值
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public static int GetValue(string name)
        {
            return enums.GetValue(name);
        }

        /// <summary>
        /// 根据值获取名称
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static string GetName(int value)
        {
            return enums.GetName(value);
        }
    }

    public class PositionState
    {
        public const int On = 0;
        public const int Off = 1;
        public const int Following = 2;
        public const int Hold = 3;
        public const int Lock = 4;
        public const int ParentLink = 5;
        public const int PhysicsLink = 6;
        public const int Comply = 7;

        /// <summary>
        /// 初始化枚举类
        /// </summary>
        private static EnumClass enums = new EnumClass("On", "Off", "Following", "Hold", "Lock", "ParentLink", "PhysicsLink", "Comply");

        /// <summary>
        /// 获取名称列表
        /// </summary>
        public static List<string> Names
        {
            get
            {
                return enums.Names;
            }
        }

        /// <summary>
        /// 根据名称获取值
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public static int GetValue(string name)
        {
            return enums.GetValue(name);
        }

        /// <summary>
        /// 根据值获取名称
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static string GetName(int value)
        {
            return enums.GetName(value);
        }
    }

    public class RotationState
    {
        public const int On = 0;
        public const int Off = 1;
        public const int Following = 2;
        public const int Hold = 3;
        public const int Lock = 4;
        public const int LookAt = 5;
        public const int ParentLink = 6;
        public const int PhysicsLink = 7;
        public const int Comply = 8;

        /// <summary>
        /// 初始化枚举类
        /// </summary>
        private static EnumClass enums = new EnumClass("On", "Off", "Following", "Hold", "Lock", "LookAt", "ParentLink", "PhysicsLink", "Comply");

        /// <summary>
        /// 获取名称列表
        /// </summary>
        public static List<string> Names
        {
            get
            {
                return enums.Names;
            }
        }

        /// <summary>
        /// 根据名称获取值
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public static int GetValue(string name)
        {
            return enums.GetValue(name);
        }

        /// <summary>
        /// 根据值获取名称
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static string GetName(int value)
        {
            return enums.GetName(value);
        }
    }

}
