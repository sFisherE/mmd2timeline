using MacGruber;
using System.Collections.Generic;
using System.Linq;

namespace mmd2timeline.Player
{
    internal abstract partial class BaseScript
    {
        // 左侧
        protected const bool LeftSide = false;
        // 右侧
        protected const bool RightSide = true;

        protected readonly string noneString = "None";

        protected readonly List<string> noneStrings = new List<string>(new string[1] { "None" });

        /// <summary>
        /// 创建标题UI
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        protected UIDynamicTextInfo CreateTitleUI(string text, bool rightSide = false)
        {
            var title = Utils.SetupInfoOneLine(this, Lang.Get(text), rightSide);
            title.text.alignment = UnityEngine.TextAnchor.MiddleCenter;
            title.text.fontStyle = UnityEngine.FontStyle.Bold;

            return title;
        }

        /// <summary>
        /// 创建一个空UI控件
        /// </summary>
        /// <param name="height"></param>
        /// <param name="right"></param>
        /// <returns></returns>
        protected UIDynamic CreateUISpacer(bool right = false, float height = 13f)
        {
            var spacer = this.CreateSpacer(right);
            spacer.height = height;
            return spacer;
        }

        /// <summary>
        /// 根据JSONStorableBool对象获取UIDynamicToggle对象
        /// </summary>
        /// <param name="json"></param>
        /// <returns></returns>
        protected UIDynamicToggle GetUIDynamicToggleByJSONStorableBool(JSONStorableBool json)
        {
            return toggleToJSONStorableBool.Where(t => t.Value == json).Select(t => t.Key).FirstOrDefault();
        }

        ///// <summary>
        ///// 创建带语言适配的 StringChooser for Enum
        ///// </summary>
        ///// <typeparam name="TEnum"></typeparam>
        ///// <param name="self"></param>
        ///// <param name="label"></param>
        ///// <param name="defaultValue"></param>
        ///// <param name="rightSide"></param>
        ///// <param name="callback"></param>
        ///// <returns></returns>
        //public JSONStorableStringChooser SetupEnumChooser<TEnum>(string label, TEnum defaultValue, bool rightSide, EnumSetCallback<TEnum> callback)
        //    where TEnum : struct, IComparable, IConvertible, IFormattable
        //{
        //    label = Lang.Get(label);

        //    List<string> names = Enum.GetNames(typeof(TEnum)).Select(e => Lang.Get(e)).ToList();
        //    JSONStorableStringChooser storable = new JSONStorableStringChooser(label, names, Lang.Get(defaultValue.ToString()), label);
        //    storable.setCallbackFunction += (string name) =>
        //    {
        //        TEnum v = (TEnum)Enum.Parse(typeof(TEnum), Lang.From(name));
        //        callback(v);
        //    };
        //    this.CreateScrollablePopup(storable, rightSide);
        //    this.RegisterStringChooser(storable);
        //    return storable;
        //}

        /// <summary>
        /// 静态枚举类回调函数
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="v"></param>
        public delegate void StaticEnumsSetCallback<T>(string v);

        /// <summary>
        /// 根据静态枚举类创建选择器
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="label"></param>
        /// <param name="names"></param>
        /// <param name="defaultValue"></param>
        /// <param name="rightSide"></param>
        /// <param name="callback"></param>
        /// <returns></returns>
        public JSONStorableStringChooser SetupStaticEnumsChooser<T>(string label, List<string> names, string defaultValue, bool rightSide, StaticEnumsSetCallback<T> callback)
        {
            label = Lang.Get(label);

            JSONStorableStringChooser storable = new JSONStorableStringChooser(label, names.Select(n => Lang.Get(n)).ToList(), Lang.Get(defaultValue), label);
            storable.setCallbackFunction += (string name) =>
            {
                callback(Lang.From(name));
            };
            this.CreateScrollablePopup(storable, rightSide);
            this.RegisterStringChooser(storable);
            return storable;
        }
    }
}
