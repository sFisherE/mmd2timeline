using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace mmd2timeline.Player
{
    internal abstract partial class BaseScript
    {
        /// <summary>
        /// 全宽度UI元素清单
        /// </summary>
        protected List<Transform> fullUIElements = new List<Transform>();

        /// <summary>
        /// 全宽度UI容器
        /// </summary>
        protected RectTransform fullUIContent;

        /// <summary>
        /// 初始化全宽度UI
        /// </summary>
        protected void InitFullWidthUI()
        {
            try
            {
                MVRScriptUI componentInChildren = UITransform.GetComponentInChildren<MVRScriptUI>();
                if (componentInChildren == null)
                {
                    return;
                }

                if (componentInChildren.fullWidthUIContent != null)
                {
                    fullUIContent = componentInChildren.fullWidthUIContent;

                    IEnumerator enumerator = fullUIContent.transform.GetEnumerator();

                    try
                    {
                        while (enumerator.MoveNext())
                        {
                            Transform transform = (Transform)enumerator.Current;
                            transform.SetParent(null);
                        }
                    }
                    finally
                    {
                        IDisposable disposable;
                        if ((disposable = enumerator as IDisposable) != null)
                        {
                            disposable.Dispose();
                        }
                    }

                    foreach (Transform element in fullUIElements)
                    {
                        element.gameObject?.SetActive(value: true);
                        element.SetParent(fullUIContent, worldPositionStays: false);
                    }
                }
            }
            catch// (Exception ex)
            {
                // 不清楚什么原因，所有代码执行完了还是会报异常。忽略掉
                //LogUtil.Debug(ex, "Director::InitFullWidthUI");
            }
        }

        /// <summary>
        /// 创建左右或全宽UI组件
        /// </summary>
        /// <param name="prefab"></param>
        /// <param name="rightSide"></param>
        /// <returns></returns>
        protected Transform CreateAllUIElement(Transform prefab, bool? rightSide)
        {
            if (rightSide.HasValue)
            {
                return CreateUIElement(prefab, rightSide.Value);
            }
            else
            {
                return CreateFullUIElement(prefab);
            }
        }

        /// <summary>
        /// 创建全长度的UI容器
        /// </summary>
        /// <param name="prefab"></param>
        /// <returns></returns>
        protected Transform CreateFullUIElement(Transform prefab)
        {
            Transform transform = null;
            if (prefab != null)
            {
                transform = UnityEngine.Object.Instantiate(prefab);
                bool flag = false;
                if (fullUIContent != null)
                {
                    flag = true;
                    transform.SetParent(fullUIContent, worldPositionStays: false);
                }

                if (flag)
                {
                    transform.gameObject.SetActive(value: true);
                }
                else
                {
                    transform.gameObject.SetActive(value: false);
                }

                fullUIElements.Add(transform);
            }

            return transform;
        }

        #region Slider
        /// <summary>
        /// 创建全宽度的Slider
        /// </summary>
        /// <param name="jsf"></param>
        /// <returns></returns>
        public UIDynamicSlider CreateFullSlider(JSONStorableFloat jsf)
        {
            UIDynamicSlider uIDynamicSlider = null;
            if (manager != null && manager.configurableSliderPrefab != null && jsf.slider == null)
            {
                Transform transform = CreateFullUIElement(manager.configurableSliderPrefab.transform);
                if (transform != null)
                {
                    uIDynamicSlider = transform.GetComponent<UIDynamicSlider>();
                    if (uIDynamicSlider != null)
                    {
                        uIDynamicSlider.Configure(jsf.name, jsf.min, jsf.max, jsf.val, jsf.constrained, "F2", showQuickButtons: true, showRangeAdjust: !jsf.constrained);
                        jsf.slider = uIDynamicSlider.slider;
                        sliderToJSONStorableFloat.Add(uIDynamicSlider, jsf);
                    }
                }
            }

            return uIDynamicSlider;
        }

        public new void RemoveSlider(JSONStorableFloat jsf)
        {
            if (jsf != null && jsf.slider != null)
            {
                UIDynamicSlider componentInParent = sliderToJSONStorableFloat.Where(s => s.Value == jsf).Select(s => s.Key).FirstOrDefault();//jsf.slider.GetComponentInParent<UIDynamicSlider>();
                if (componentInParent != null)
                {
                    this.RemoveSlider(componentInParent);
                }
                else
                {
                    LogUtil.Debug($"Settings:::RemoveSlider:false:{jsf.name}");
                }
            }
        }

        public new void RemoveSlider(UIDynamicSlider dslider)
        {
            Transform transform = dslider.transform;
            fullUIElements.Remove(transform);

            base.RemoveSlider(dslider);
        }
        #endregion

        #region Toggle
        public UIDynamicToggle CreateFullToggle(JSONStorableBool jsb)
        {
            UIDynamicToggle uIDynamicToggle = null;
            if (manager != null && manager.configurableTogglePrefab != null && jsb.toggle == null)
            {
                Transform transform = CreateFullUIElement(manager.configurableTogglePrefab.transform);
                if (transform != null)
                {
                    uIDynamicToggle = transform.GetComponent<UIDynamicToggle>();
                    if (uIDynamicToggle != null)
                    {
                        toggleToJSONStorableBool.Add(uIDynamicToggle, jsb);
                        uIDynamicToggle.label = jsb.name;
                        jsb.toggle = uIDynamicToggle.toggle;
                    }
                }
            }

            return uIDynamicToggle;
        }

        public new void RemoveToggle(JSONStorableBool jsb)
        {
            if (jsb != null && jsb.toggle != null)
            {
                Transform transform = jsb.toggle.transform;
                fullUIElements.Remove(transform);
            }

            base.RemoveToggle(jsb);
        }

        public new void RemoveToggle(UIDynamicToggle dtoggle)
        {
            JSONStorableBool value;

            if (toggleToJSONStorableBool.TryGetValue(dtoggle, out value))
            {
                Transform transform = value.toggle.transform;
                fullUIElements.Remove(transform);
            }

            base.RemoveToggle(dtoggle);
        }
        #endregion

        #region ColorPicker
        public UIDynamicColorPicker CreateFullColorPicker(JSONStorableColor jsc)
        {
            UIDynamicColorPicker uIDynamicColorPicker = null;
            if (manager != null && manager.configurableColorPickerPrefab != null && jsc.colorPicker == null)
            {
                Transform transform = CreateFullUIElement(manager.configurableColorPickerPrefab.transform);
                if (transform != null)
                {
                    uIDynamicColorPicker = transform.GetComponent<UIDynamicColorPicker>();
                    if (uIDynamicColorPicker != null)
                    {
                        colorPickerToJSONStorableColor.Add(uIDynamicColorPicker, jsc);
                        uIDynamicColorPicker.label = jsc.name;
                        jsc.colorPicker = uIDynamicColorPicker.colorPicker;
                    }
                }
            }

            return uIDynamicColorPicker;
        }

        public new void RemoveColorPicker(JSONStorableColor jsc)
        {
            if (jsc != null && jsc.colorPicker != null)
            {
                UIDynamicColorPicker componentInParent = jsc.colorPicker.GetComponentInParent<UIDynamicColorPicker>();
                if (componentInParent != null)
                {
                    Transform transform = componentInParent.transform;
                    fullUIElements.Remove(transform);
                }
            }

            base.RemoveColorPicker(jsc);
        }

        public new void RemoveColorPicker(UIDynamicColorPicker dcolor)
        {
            Transform transform = dcolor.transform;
            fullUIElements.Remove(transform);

            base.RemoveColorPicker(dcolor);
        }
        #endregion

        #region TextField
        public UIDynamicTextField CreateFullTextField(JSONStorableString jss)
        {
            UIDynamicTextField uIDynamicTextField = null;
            if (manager != null && manager.configurableTextFieldPrefab != null && jss.text == null)
            {
                Transform transform = CreateFullUIElement(manager.configurableTextFieldPrefab.transform);
                if (transform != null)
                {
                    uIDynamicTextField = transform.GetComponent<UIDynamicTextField>();
                    if (uIDynamicTextField != null)
                    {
                        textFieldToJSONStorableString.Add(uIDynamicTextField, jss);
                        jss.dynamicText = uIDynamicTextField;
                    }
                }
            }

            return uIDynamicTextField;
        }

        public new void RemoveTextField(JSONStorableString jss)
        {
            if (jss != null && jss.text != null)
            {
                UIDynamicTextField dynamicText = jss.dynamicText;
                if (dynamicText != null)
                {
                    Transform transform = dynamicText.transform;
                    fullUIElements.Remove(transform);
                }
            }

            base.RemoveTextField(jss);
        }

        public new void RemoveTextField(UIDynamicTextField dtext)
        {
            Transform transform = dtext.transform;
            fullUIElements.Remove(transform);

            base.RemoveTextField(dtext);
        }
        #endregion

        #region Popup
        public UIDynamicPopup CreateFullPopup(JSONStorableStringChooser jsc)
        {
            UIDynamicPopup uIDynamicPopup = null;
            if (manager != null && manager.configurablePopupPrefab != null && jsc.popup == null)
            {
                Transform transform = CreateFullUIElement(manager.configurablePopupPrefab.transform);
                if (transform != null)
                {
                    uIDynamicPopup = transform.GetComponent<UIDynamicPopup>();
                    if (uIDynamicPopup != null)
                    {
                        popupToJSONStorableStringChooser.Add(uIDynamicPopup, jsc);
                        uIDynamicPopup.label = jsc.name;
                        jsc.popup = uIDynamicPopup.popup;
                    }
                }
            }

            return uIDynamicPopup;
        }

        public UIDynamicPopup CreateFullScrollablePopup(JSONStorableStringChooser jsc)
        {
            UIDynamicPopup uIDynamicPopup = null;
            if (manager != null && manager.configurableScrollablePopupPrefab != null && jsc.popup == null)
            {
                Transform transform = CreateFullUIElement(manager.configurableScrollablePopupPrefab.transform);
                if (transform != null)
                {
                    uIDynamicPopup = transform.GetComponent<UIDynamicPopup>();
                    if (uIDynamicPopup != null)
                    {
                        popupToJSONStorableStringChooser.Add(uIDynamicPopup, jsc);
                        uIDynamicPopup.label = jsc.name;
                        jsc.popup = uIDynamicPopup.popup;
                    }
                }
            }

            return uIDynamicPopup;
        }

        public UIDynamicPopup CreateFullFilterablePopup(JSONStorableStringChooser jsc)
        {
            UIDynamicPopup uIDynamicPopup = null;
            if (manager != null && manager.configurableFilterablePopupPrefab != null && jsc.popup == null)
            {
                Transform transform = CreateFullUIElement(manager.configurableFilterablePopupPrefab.transform);
                if (transform != null)
                {
                    uIDynamicPopup = transform.GetComponent<UIDynamicPopup>();
                    if (uIDynamicPopup != null)
                    {
                        popupToJSONStorableStringChooser.Add(uIDynamicPopup, jsc);
                        uIDynamicPopup.label = jsc.name;
                        jsc.popup = uIDynamicPopup.popup;
                    }
                }
            }

            return uIDynamicPopup;
        }

        public new void RemovePopup(JSONStorableStringChooser jsc)
        {
            if (jsc != null && jsc.popup != null)
            {
                Transform transform = jsc.popup.transform;
                fullUIElements.Remove(transform);
            }

            base.RemovePopup(jsc);
        }

        public new void RemovePopup(UIDynamicPopup dpopup)
        {
            JSONStorableStringChooser value;

            if (popupToJSONStorableStringChooser.TryGetValue(dpopup, out value))
            {
                Transform transform = value.popup.transform;
                fullUIElements.Remove(transform);
            }

            base.RemovePopup(dpopup);
        }
        #endregion

        #region Button
        public UIDynamicButton CreateFullButton(string label)
        {
            UIDynamicButton uIDynamicButton = null;
            if (manager != null && manager.configurableButtonPrefab != null)
            {
                Transform transform = CreateFullUIElement(manager.configurableButtonPrefab.transform);
                if (transform != null)
                {
                    uIDynamicButton = transform.GetComponent<UIDynamicButton>();
                    if (uIDynamicButton != null)
                    {
                        uIDynamicButton.label = label;
                    }
                }
            }

            return uIDynamicButton;
        }

        public new void RemoveButton(UIDynamicButton button)
        {
            if (button != null)
            {
                fullUIElements.Remove(button.transform);
            }

            base.RemoveButton(button);
        }
        #endregion

        #region Spacer
        public UIDynamic CreateFullSpacer()
        {
            UIDynamic result = null;
            if (manager != null && manager.configurableSpacerPrefab != null)
            {
                Transform transform = CreateFullUIElement(manager.configurableSpacerPrefab.transform);
                if (transform != null)
                {
                    result = transform.GetComponent<UIDynamic>();
                }
            }

            return result;
        }

        public new void RemoveSpacer(UIDynamic spacer)
        {
            if (spacer != null)
            {
                fullUIElements.Remove(spacer.transform);
            }

            base.RemoveSpacer(spacer);
        }
        #endregion
    }
}
