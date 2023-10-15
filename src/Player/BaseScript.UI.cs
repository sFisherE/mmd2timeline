using MacGruber;
using System.Collections.Generic;
using System.Linq;

namespace mmd2timeline
{
    internal partial class BaseScript
    {
        /// <summary>
        /// 左侧
        /// </summary>
        protected const bool LeftSide = false;
        /// <summary>
        /// 右侧
        /// </summary>
        protected const bool RightSide = true;

        /// <summary>
        /// 空字符串选项
        /// </summary>
        internal const string noneString = "None";

        /// <summary>
        /// 空字符串列表
        /// </summary>
        internal readonly List<string> noneStrings = new List<string> { noneString };

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

        /// <summary>
        /// 创建字符串选择器
        /// </summary>
        /// <param name="label"></param>
        /// <param name="entries"></param>
        /// <param name="popupPanelHeight"></param>
        /// <param name="rightSide"></param>
        /// <returns></returns>
        internal JSONStorableStringChooser SetupStringChooser(string label, List<string> entries, float popupPanelHeight = 600f, bool rightSide = false)
        {
            string defaultEntry = entries.Count > 0 ? entries[0] : "";
            JSONStorableStringChooser storable = new JSONStorableStringChooser(label, entries, defaultEntry, Lang.Get(label));
            this.CreateScrollablePopup(storable, rightSide).popupPanelHeight = popupPanelHeight;
            this.RegisterStringChooser(storable);
            return storable;
        }

        /// <summary>
        /// 创建字符串选择器（不进行自动语言处理）
        /// </summary>
        /// <param name="label"></param>
        /// <param name="entries"></param>
        /// <param name="popupPanelHeight"></param>
        /// <param name="rightSide"></param>
        /// <returns></returns>
        internal JSONStorableStringChooser SetupStringChooserNoLang(string label, List<string> entries, float popupPanelHeight = 600f, bool rightSide = false)
        {
            string defaultEntry = entries.Count > 0 ? entries[0] : "";
            JSONStorableStringChooser storable = new JSONStorableStringChooser(label, entries, defaultEntry, label);
            this.CreateScrollablePopup(storable, rightSide).popupPanelHeight = popupPanelHeight;
            this.RegisterStringChooser(storable);
            return storable;
        }

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
