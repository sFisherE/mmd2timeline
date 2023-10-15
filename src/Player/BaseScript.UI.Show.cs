using MacGruber;
using System.Collections.Generic;
using System.Linq;

namespace mmd2timeline
{
    // 此部分代码实现显示隐藏UI元素的便捷方法
    internal partial class BaseScript
    {
        public void ShowSlider(JSONStorableFloat jsf, bool show = true)
        {
            if (jsf != null && jsf.slider != null)
            {
                UIDynamicSlider componentInParent = sliderToJSONStorableFloat.Where(s => s.Value == jsf).Select(s => s.Key).FirstOrDefault();//jsf.slider.GetComponentInParent<UIDynamicSlider>();
                if (componentInParent != null)
                {
                    this.ShowSlider(componentInParent, show);
                }
                else
                {
                    LogUtil.Debug($"Settings:::ShowSlider:false:{jsf.name}");
                }
            }
        }

        public void ShowSlider(UIDynamicSlider dslider, bool show = true)
        {
            dslider.gameObject.SetActive(show);
        }

        public void ShowToggle(JSONStorableBool jsb, bool show = true)
        {
            if (jsb != null && jsb.toggle != null)
            {
                var target = GetUIDynamicToggleByJSONStorableBool(jsb);

                if (target != null)
                {
                    this.ShowToggle(target, show);
                }
            }
        }

        public void ShowToggle(UIDynamicToggle dtoggle, bool show = true)
        {
            dtoggle.gameObject.SetActive(show);
        }

        public void ShowColorPicker(JSONStorableColor jsc, bool show = true)
        {
            if (jsc != null && jsc.colorPicker != null)
            {
                UIDynamicColorPicker componentInParent = jsc.colorPicker.GetComponentInParent<UIDynamicColorPicker>();
                if (componentInParent != null)
                {
                    ShowColorPicker(componentInParent, show);
                }
            }
        }

        public void ShowColorPicker(UIDynamicColorPicker dcolor, bool show = true)
        {
            dcolor.gameObject.SetActive(show);
        }

        public void ShowTextField(JSONStorableString jss, bool show = true)
        {
            if (jss != null && jss.text != null)
            {
                UIDynamicTextField dynamicText = jss.dynamicText;
                if (dynamicText != null)
                {
                    ShowTextField(dynamicText, show);
                }
            }
        }

        public void ShowTextField(UIDynamicTextField dtext, bool show = true)
        {
            dtext.gameObject.SetActive(show);
        }

        public void ShowPopup(JSONStorableStringChooser jsc, bool show = true)
        {
            if (jsc != null && jsc.popup != null)
            {
                var dynamic = popupToJSONStorableStringChooser.Where(p => p.Value == jsc).Select(p => p.Key).FirstOrDefault();
                if (dynamic != null)
                {
                    ShowPopup(dynamic, show);
                }
            }
        }

        public void ShowPopup(UIDynamicPopup dpopup, bool show = true)
        {
            dpopup.gameObject.SetActive(show);
        }


        public void ShowButton(UIDynamicButton button, bool show = true)
        {
            button.gameObject.SetActive(show);
        }


        public void ShowSpacer(UIDynamic spacer, bool show = true)
        {
            spacer.gameObject.SetActive(show);
        }

        /// <summary>
        /// 根据参数显示或隐藏UI清单
        /// </summary>
        /// <param name="menuElements"></param>
        /// <param name="show"></param>
        internal void ShowUIElements(List<object> menuElements, bool show = true)
        {
            for (int i = 0; i < menuElements.Count; ++i)
            {
                ShowUIElement(menuElements[i], show);
            }
        }

        /// <summary>
        /// 显示或隐藏指定UI元素
        /// </summary>
        /// <param name="element">指定的UI元素对象</param>
        /// <param name="show">是否显示</param>
        internal void ShowUIElement(object element, bool show = true)
        {
            if (element is JSONStorableParam)
            {
                JSONStorableParam jsp = element as JSONStorableParam;

                if (jsp is JSONStorableFloat)
                    ShowSlider(jsp as JSONStorableFloat, show);
                else if (jsp is JSONStorableBool)
                    ShowToggle(jsp as JSONStorableBool, show);
                else if (jsp is JSONStorableColor)
                    ShowColorPicker(jsp as JSONStorableColor, show);
                else if (jsp is JSONStorableString)
                    ShowTextField(jsp as JSONStorableString, show);
                else if (jsp is JSONStorableStringChooser)
                {
                    // Workaround for VaM not cleaning its panels properly.
                    JSONStorableStringChooser jssc = jsp as JSONStorableStringChooser;
                    //RectTransform popupPanel = jssc.popup?.popupPanel;
                    ShowPopup(jssc, show);
                    //if (popupPanel != null)
                    //    popupPanel.gameObject.SetActive(show);
                }
            }
            else if (element is UIDynamic)
            {
                UIDynamic uid = element as UIDynamic;
                if (uid is UIDynamicButton)
                    ShowButton(uid as UIDynamicButton, show);
                else if (uid is UIDynamicUtils)
                    ShowSpacer(uid, show);
                else if (uid is UIDynamicSlider)
                    ShowSlider(uid as UIDynamicSlider, show);
                else if (uid is UIDynamicToggle)
                    ShowToggle(uid as UIDynamicToggle, show);
                else if (uid is UIDynamicColorPicker)
                    ShowColorPicker(uid as UIDynamicColorPicker, show);
                else if (uid is UIDynamicTextField)
                    ShowTextField(uid as UIDynamicTextField, show);
                else if (uid is UIDynamicPopup)
                {
                    // Workaround for VaM not cleaning its panels properly.
                    UIDynamicPopup uidp = uid as UIDynamicPopup;
                    //RectTransform popupPanel = uidp.popup?.popupPanel;
                    ShowPopup(uidp, show);
                    //if (popupPanel != null)
                    //    popupPanel.gameObject.SetActive(show);
                }
                else
                    ShowSpacer(uid, show);
            }
        }
    }
}
