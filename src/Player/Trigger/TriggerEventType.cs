using System.Collections.Generic;

namespace mmd2timeline
{
    /// <summary>
    /// 触发器事件类型
    /// </summary>
    internal class TriggerEventType
    {
        public const int Other = 0;
        public const int Bool = 1;
        public const int Float = 2;
        public const int String = 3;
        public const int Chooser = 4;
        public const int Action = 5;

        /// <summary>
        /// 初始化枚举类
        /// </summary>
        private static EnumClass enums = new EnumClass("Other", "Bool", "Float", "String", "Chooser", "Action");

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
