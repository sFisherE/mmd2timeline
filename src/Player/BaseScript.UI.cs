using MacGruber;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

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
        internal UIDynamicTextInfo CreateTitleUI(string text, bool rightSide = false)
        {
            return CreateTitleUINoLang(Lang.Get(text), rightSide);
        }

        /// <summary>
        /// 创建标题UI（不进行语言处理）
        /// </summary>
        /// <param name="text"></param>
        /// <param name="rightSide"></param>
        /// <returns></returns>
        internal UIDynamicTextInfo CreateTitleUINoLang(string text, bool rightSide = false)
        {
            var title = Utils.SetupInfoOneLine(this, text, rightSide);
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
        internal JSONStorableStringChooser SetupStringChooser(string paramName, string label, List<string> entries, float popupPanelHeight = 600f, bool rightSide = false)
        {
            string defaultEntry = entries.Count > 0 ? entries[0] : "";
            JSONStorableStringChooser storable = new JSONStorableStringChooser(paramName, entries, defaultEntry, Lang.Get(label));
            this.CreateScrollablePopup(storable, rightSide).popupPanelHeight = popupPanelHeight;
            this.RegisterStringChooser(storable);
            return storable;
        }

        internal JSONStorableStringChooser SetupStringChooser(string label, List<string> entries, float popupPanelHeight = 600f, bool rightSide = false)
        {
            return SetupStringChooser(label, label, entries, popupPanelHeight, rightSide);
        }

        /// <summary>
        /// 创建字符串选择器（不进行自动语言处理）
        /// </summary>
        /// <param name="label"></param>
        /// <param name="entries"></param>
        /// <param name="popupPanelHeight"></param>
        /// <param name="rightSide"></param>
        /// <returns></returns>
        internal JSONStorableStringChooser SetupStringChooserNoLang(string paramName, string label, List<string> entries, float popupPanelHeight = 600f, bool rightSide = false)
        {
            string defaultEntry = entries.Count > 0 ? entries[0] : "";
            JSONStorableStringChooser storable = new JSONStorableStringChooser(paramName, entries, defaultEntry, label);
            this.CreateScrollablePopup(storable, rightSide).popupPanelHeight = popupPanelHeight;
            this.RegisterStringChooser(storable);
            return storable;
        }

        // Create VaM-UI Toggle button
        internal JSONStorableBool SetupToggle(string paramName, string label, bool defaultValue, bool rightSide)
        {
            JSONStorableBool storable = new JSONStorableBool(paramName, defaultValue);
            storable.storeType = JSONStorableParam.StoreType.Full;
            var toggle = CreateToggle(storable, rightSide);
            toggle.label = label;
            RegisterBool(storable);
            return storable;
        }

        /// <summary>
        /// 创建带回调函数的Toggle
        /// </summary>
        /// <param name="script"></param>
        /// <param name="label"></param>
        /// <param name="defaultValue"></param>
        /// <param name="callback"></param>
        /// <param name="rightSide"></param>
        /// <returns></returns>
        internal JSONStorableBool SetupToggle(string paramName, string label, bool defaultValue, Action<bool> callback, bool rightSide)
        {
            JSONStorableBool storable = SetupToggle(paramName, label, defaultValue, rightSide);
            storable.setCallbackFunction = v => callback(v);
            return storable;
        }

        // Create VaM-UI Float slider
        internal JSONStorableFloat SetupSliderFloat(string paramName, string label, float defaultValue, float minValue, float maxValue, Action<float> callback, bool rightSide, string valueFormat = "")
        {
            JSONStorableFloat storable = new JSONStorableFloat(paramName, defaultValue, minValue, maxValue, true, true);
            storable.storeType = JSONStorableParam.StoreType.Full;
            storable.setCallbackFunction = v => callback?.Invoke(v);
            var slider = CreateSlider(storable, rightSide);
            slider.label = label;
            if (!string.IsNullOrEmpty(valueFormat))
            {
                slider.valueFormat = valueFormat;
            }
            RegisterFloat(storable);
            return storable;
        }

        ///// <summary>
        ///// 根据UISlider获取对应的Storable对象
        ///// </summary>
        ///// <param name="slider"></param>
        ///// <returns></returns>
        //internal JSONStorableFloat GetJSONStorableFloat(UIDynamicSlider slider)
        //{
        //    JSONStorableFloat storable;
        //    sliderToJSONStorableFloat.TryGetValue(slider, out storable);

        //    return storable;
        //}

        /// <summary>
        /// 创建带回调函数的Slider
        /// </summary>
        /// <param name="script"></param>
        /// <param name="label"></param>
        /// <param name="defaultValue"></param>
        /// <param name="minValue"></param>
        /// <param name="maxValue"></param>
        /// <param name="callback"></param>
        /// <param name="rightSide"></param>
        /// <param name="valueFormat"></param>
        /// <returns></returns>
        internal JSONStorableFloat SetupSliderFloat(string paramName, string label, float defaultValue, float minValue, float maxValue, bool rightSide, string valueFormat = "")
        {
            return SetupSliderFloat(paramName, label, defaultValue, minValue, maxValue, null, rightSide, valueFormat);
        }

        /// <summary>
        /// 创建带回调函数的范围可变Slider
        /// </summary>
        /// <param name="script"></param>
        /// <param name="label"></param>
        /// <param name="defaultValue"></param>
        /// <param name="minValue"></param>
        /// <param name="maxValue"></param>
        /// <param name="callback"></param>
        /// <param name="rightSide"></param>
        /// <param name="valueFormat"></param>
        /// <returns></returns>
        internal JSONStorableFloat SetupSliderFloatWithRange(string paramName, string label, float defaultValue, float minValue, float maxValue, Action<float> callback, bool rightSide, string valueFormat = "")
        {
            JSONStorableFloat storable = SetupSliderFloatWithRange(paramName, label, defaultValue, minValue, maxValue, rightSide, valueFormat);
            storable.setCallbackFunction = v => callback(v);
            return storable;
        }

        // Create VaM-UI Float slider
        internal JSONStorableFloat SetupSliderFloatWithRange(string paramName, string label, float defaultValue, float minValue, float maxValue, bool rightSide, string valueFormat = "")
        {
            JSONStorableFloat storable = new JSONStorableFloat(paramName, defaultValue, minValue, maxValue, true, true);
            storable.storeType = JSONStorableParam.StoreType.Full;
            storable.constrained = false;
            UIDynamicSlider slider = CreateSlider(storable, rightSide);
            slider.label = label;
            slider.rangeAdjustEnabled = true;
            if (!string.IsNullOrEmpty(valueFormat))
            {
                slider.valueFormat = valueFormat;
            }
            RegisterFloat(storable);
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

        public JSONStorableStringChooser SetupStaticEnumsChooser<T>(string paramName, string label, List<string> names, string defaultValue, bool rightSide, StaticEnumsSetCallback<T> callback)
        {
            label = Lang.Get(label);

            JSONStorableStringChooser storable = new JSONStorableStringChooser(paramName, names.Select(n => Lang.Get(n)).ToList(), Lang.Get(defaultValue), label);
            storable.setCallbackFunction += (string name) =>
            {
                callback(Lang.From(name));
            };
            this.CreateScrollablePopup(storable, rightSide);
            this.RegisterStringChooser(storable);
            return storable;
        }

        /// <summary>
        /// 移除指定列表中的所有UI元素
        /// </summary>
        /// <param name="script"></param>
        /// <param name="menuElements"></param>
        /// <remarks>此方法来自于MacGruber_Utils，但其无法删除slider控件，因此复制至此</remarks>
        public static void RemoveUIElements(BaseScript script, List<object> menuElements)
        {
            for (int i = 0; i < menuElements.Count; ++i)
            {
                if (menuElements[i] is JSONStorableParam)
                {
                    JSONStorableParam jsp = menuElements[i] as JSONStorableParam;

                    if (jsp is JSONStorableFloat)
                    {
                        script.RemoveSlider(jsp as JSONStorableFloat);
                    }
                    else if (jsp is JSONStorableBool)
                        script.RemoveToggle(jsp as JSONStorableBool);
                    else if (jsp is JSONStorableColor)
                        script.RemoveColorPicker(jsp as JSONStorableColor);
                    else if (jsp is JSONStorableString)
                        script.RemoveTextField(jsp as JSONStorableString);
                    else if (jsp is JSONStorableStringChooser)
                    {
                        // Workaround for VaM not cleaning its panels properly.
                        JSONStorableStringChooser jssc = jsp as JSONStorableStringChooser;
                        RectTransform popupPanel = jssc.popup?.popupPanel;
                        script.RemovePopup(jssc);
                        if (popupPanel != null)
                            UnityEngine.Object.Destroy(popupPanel.gameObject);
                    }
                }
                else if (menuElements[i] is UIDynamic)
                {
                    UIDynamic uid = menuElements[i] as UIDynamic;
                    if (uid is UIDynamicButton)
                        script.RemoveButton(uid as UIDynamicButton);
                    else if (uid is UIDynamicUtils)
                        script.RemoveSpacer(uid);
                    else if (uid is UIDynamicSlider)
                        script.RemoveSlider(uid as UIDynamicSlider);
                    else if (uid is UIDynamicToggle)
                        script.RemoveToggle(uid as UIDynamicToggle);
                    else if (uid is UIDynamicColorPicker)
                        script.RemoveColorPicker(uid as UIDynamicColorPicker);
                    else if (uid is UIDynamicTextField)
                        script.RemoveTextField(uid as UIDynamicTextField);
                    else if (uid is UIDynamicPopup)
                    {
                        // Workaround for VaM not cleaning its panels properly.
                        UIDynamicPopup uidp = uid as UIDynamicPopup;
                        RectTransform popupPanel = uidp.popup?.popupPanel;
                        script.RemovePopup(uidp);
                        if (popupPanel != null)
                            UnityEngine.Object.Destroy(popupPanel.gameObject);
                    }
                    else
                        script.RemoveSpacer(uid);
                }
            }

            menuElements.Clear();
        }
    }
}
