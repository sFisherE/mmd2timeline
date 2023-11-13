/* /////////////////////////////////////////////////////////////////////////////////////////////////
Utils 2021-03-13 by MacGruber
Collection of various utility functions.
https://www.patreon.com/MacGruber_Laboratory

Licensed under CC BY-SA after EarlyAccess ended. (see https://creativecommons.org/licenses/by-sa/4.0/)

///////////////////////////////////////////////////////////////////////////////////////////////// */

using AssetBundles;
using SimpleJSON;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using Request = MeshVR.AssetLoader.AssetBundleFromFileRequest;

namespace MacGruber
{
    public static class Utils
    {
        // VaM Plugins can contain multiple Scripts, if you load them via a *.cslist file. This function allows you to get
        // an instance of another script within the same plugin, allowing you directly interact with it by reading/writing
        // data, calling functions, etc.
        public static T FindWithinSamePlugin<T>(MVRScript self) where T : MVRScript
        {
            int i = self.name.IndexOf('_');
            if (i < 0)
                return null;
            string prefix = self.name.Substring(0, i + 1);
            string scriptName = prefix + typeof(T).FullName;
            return self.containingAtom.GetStorableByID(scriptName) as T;
        }

        // Get spawned prefab from CustomUnityAsset atom. Note that these are loaded asynchronously,
        // this function returns null while the prefab is not yet there.
        public static GameObject GetCustomUnityAsset(Atom atom, string prefabName)
        {
            Transform t = atom.transform.Find("reParentObject/object/rescaleObject/" + prefabName + "(Clone)");
            if (t == null)
                return null;
            else
                return t.gameObject;
        }

        // Get directory path where the plugin is located. Based on Alazi's & VAMDeluxe's method.
        public static string GetPluginPath(MVRScript self)
        {
            string id = self.name.Substring(0, self.name.IndexOf('_'));
            string filename = self.manager.GetJSON()["plugins"][id].Value;
            return filename.Substring(0, filename.LastIndexOfAny(new char[] { '/', '\\' }));
        }

        // Get path prefix of the package that contains our plugin.
        public static string GetPackagePath(MVRScript self)
        {
            string id = self.name.Substring(0, self.name.IndexOf('_'));
            string filename = self.manager.GetJSON()["plugins"][id].Value;
            int idx = filename.IndexOf(":/");
            if (idx >= 0)
                return filename.Substring(0, idx + 2);
            else
                return string.Empty;
        }

        // Check if our plugin is running from inside a package
        public static bool IsInPackage(MVRScript self)
        {
            string id = self.name.Substring(0, self.name.IndexOf('_'));
            string filename = self.manager.GetJSON()["plugins"][id].Value;
            return filename.IndexOf(":/") >= 0;
        }

        // ===========================================================================================

        // Create VaM-UI Toggle button
        public static JSONStorableBool SetupToggle(MVRScript script, string label, bool defaultValue, bool rightSide)
        {
            JSONStorableBool storable = new JSONStorableBool(label, defaultValue);
            storable.storeType = JSONStorableParam.StoreType.Full;
            script.CreateToggle(storable, rightSide);
            script.RegisterBool(storable);
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
        public static JSONStorableBool SetupToggle(MVRScript script, string label, bool defaultValue, Action<bool> callback, bool rightSide)
        {
            JSONStorableBool storable = SetupToggle(script, label, defaultValue, rightSide);
            storable.setCallbackFunction = v => callback(v);
            return storable;
        }

        // Create VaM-UI Float slider
        public static JSONStorableFloat SetupSliderFloat(MVRScript script, string label, float defaultValue, float minValue, float maxValue, bool rightSide, string valueFormat = "")
        {
            JSONStorableFloat storable = new JSONStorableFloat(label, defaultValue, minValue, maxValue, true, true);
            storable.storeType = JSONStorableParam.StoreType.Full;
            var slider = script.CreateSlider(storable, rightSide);
            if (!string.IsNullOrEmpty(valueFormat))
            {
                slider.valueFormat = valueFormat;
            }
            //script.RegisterFloat(storable);
            return storable;
        }

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
        public static JSONStorableFloat SetupSliderFloat(MVRScript script, string label, float defaultValue, float minValue, float maxValue, Action<float> callback, bool rightSide, string valueFormat = "")
        {
            JSONStorableFloat storable = SetupSliderFloat(script, label, defaultValue, minValue, maxValue, rightSide, valueFormat);
            storable.setCallbackFunction = v => callback(v);
            return storable;
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
        public static JSONStorableFloat SetupSliderFloatWithRange(MVRScript script, string label, float defaultValue, float minValue, float maxValue, Action<float> callback, bool rightSide, string valueFormat = "")
        {
            JSONStorableFloat storable = SetupSliderFloatWithRange(script, label, defaultValue, minValue, maxValue, rightSide, valueFormat);
            storable.setCallbackFunction = v => callback(v);
            return storable;
        }

        // Create VaM-UI Float slider
        public static JSONStorableFloat SetupSliderFloatWithRange(MVRScript script, string label, float defaultValue, float minValue, float maxValue, bool rightSide, string valueFormat = "")
        {
            JSONStorableFloat storable = new JSONStorableFloat(label, defaultValue, minValue, maxValue, true, true);
            storable.storeType = JSONStorableParam.StoreType.Full;
            storable.constrained = false;
            UIDynamicSlider slider = script.CreateSlider(storable, rightSide);
            slider.rangeAdjustEnabled = true;
            if (!string.IsNullOrEmpty(valueFormat))
            {
                slider.valueFormat = valueFormat;
            }
            //script.RegisterFloat(storable);
            return storable;
        }

        // Create VaM-UI Float slider
        public static JSONStorableFloat SetupSliderInt(MVRScript script, string label, int defaultValue, int minValue, int maxValue, bool rightSide)
        {
            JSONStorableFloat storable = new JSONStorableFloat(label, defaultValue, minValue, maxValue, true, true);
            storable.storeType = JSONStorableParam.StoreType.Full;
            UIDynamicSlider slider = script.CreateSlider(storable, rightSide);
            slider.slider.wholeNumbers = true;
            slider.valueFormat = "F0";
            //script.RegisterFloat(storable);
            return storable;
        }

        // Create VaM-UI ColorPicker
        public static JSONStorableColor SetupColor(MVRScript script, string label, Color color, bool rightSide)
        {
            HSVColor hsvColor = HSVColorPicker.RGBToHSV(color.r, color.g, color.b);
            JSONStorableColor storable = new JSONStorableColor(label, hsvColor);
            storable.storeType = JSONStorableParam.StoreType.Full;
            script.CreateColorPicker(storable, rightSide);
            //script.RegisterColor(storable);
            return storable;
        }

        // Create VaM-UI StringChooser
        public static JSONStorableStringChooser SetupStringChooser(MVRScript self, string label, List<string> entries, bool rightSide)
        {
            string defaultEntry = entries.Count > 0 ? entries[0] : "";
            JSONStorableStringChooser storable = new JSONStorableStringChooser(label, entries, defaultEntry, label);
            self.CreateScrollablePopup(storable, rightSide);
            //self.RegisterStringChooser(storable);
            return storable;
        }

        // Create VaM-UI StringChooser
        public static JSONStorableStringChooser SetupStringChooser(MVRScript self, string label, List<string> entries, int defaultIndex, bool rightSide)
        {
            string defaultEntry = (defaultIndex >= 0 && defaultIndex < entries.Count) ? entries[defaultIndex] : "";
            JSONStorableStringChooser storable = new JSONStorableStringChooser(label, entries, defaultEntry, label);
            self.CreateScrollablePopup(storable, rightSide);
            //self.RegisterStringChooser(storable);
            return storable;
        }

        // Create VaM-UI StringChooser for Enum
        public static JSONStorableStringChooser SetupEnumChooser<TEnum>(MVRScript self, string label, TEnum defaultValue, bool rightSide, EnumSetCallback<TEnum> callback)
            where TEnum : struct, IComparable, IConvertible, IFormattable
        {
            List<string> names = Enum.GetNames(typeof(TEnum)).ToList();
            JSONStorableStringChooser storable = new JSONStorableStringChooser(label, names, defaultValue.ToString(), label);
            storable.setCallbackFunction += (string name) =>
            {
                TEnum v = (TEnum)Enum.Parse(typeof(TEnum), name);
                callback(v);
            };
            self.CreateScrollablePopup(storable, rightSide);
            //self.RegisterStringChooser(storable);
            return storable;
        }

        // Create VaM-UI TextureChooser. Note that you are responsible for destroying the texture when you don't need it anymore.
        public static JSONStorableUrl SetupTexture2DChooser(MVRScript self, string label, string defaultValue, bool rightSide, TextureSettings settings, TextureSetCallback callback)
        {
            JSONStorableUrl storable = new JSONStorableUrl(label, string.Empty, (string url) => { QueueLoadTexture(url, settings, callback); }, "jpg|png|tif|tiff");
            self.RegisterUrl(storable);
            UIDynamicButton button = self.CreateButton("Browse " + label, false);
            UIDynamicTextField textfield = self.CreateTextField(storable, false);
            textfield.UItext.alignment = TextAnchor.MiddleRight;
            textfield.UItext.horizontalOverflow = HorizontalWrapMode.Overflow;
            textfield.UItext.verticalOverflow = VerticalWrapMode.Truncate;
            LayoutElement layout = textfield.GetComponent<LayoutElement>();
            layout.preferredHeight = layout.minHeight = 35;
            textfield.height = 35;
            if (!string.IsNullOrEmpty(defaultValue))
                storable.SetFilePath(defaultValue);
            //storable.RegisterFileBrowseButton(button.button);
            return storable;
        }

        // Create VaM-UI AssetBundleChooser.
        public static JSONStorableUrl SetupAssetBundleChooser(MVRScript self, string label, string defaultValue, bool rightSide, string fileExtensions)
        {
            JSONStorableUrl storable = new JSONStorableUrl(label, defaultValue, fileExtensions);
            self.RegisterUrl(storable);
            UIDynamicButton button = self.CreateButton("Select " + label, false);
            UIDynamicTextField textfield = self.CreateTextField(storable, false);
            textfield.UItext.alignment = TextAnchor.MiddleRight;
            textfield.UItext.horizontalOverflow = HorizontalWrapMode.Overflow;
            textfield.UItext.verticalOverflow = VerticalWrapMode.Truncate;
            LayoutElement layout = textfield.GetComponent<LayoutElement>();
            layout.preferredHeight = layout.minHeight = 35;
            textfield.height = 35;
            if (!string.IsNullOrEmpty(defaultValue))
                storable.SetFilePath(defaultValue);
            //storable.RegisterFileBrowseButton(button.button);
            return storable;
        }

        // Create VaM-UI InfoText field
        public static JSONStorableString SetupInfoText(MVRScript script, string text, float height, bool rightSide)
        {
            JSONStorableString storable = new JSONStorableString("Info", text);
            UIDynamic textfield = script.CreateTextField(storable, rightSide);
            textfield.height = height;
            return storable;
        }

        public static UIDynamic SetupSpacer(MVRScript script, float height, bool rightSide)
        {
            UIDynamic spacer = script.CreateSpacer(rightSide);
            spacer.height = height;
            return spacer;
        }

        // Create VaM-UI button
        public static UIDynamicButton SetupButton(MVRScript script, string label, UnityAction callback, bool rightSide)
        {
            UIDynamicButton button = script.CreateButton(label, rightSide);
            button.button.onClick.AddListener(callback);
            return button;
        }

        // Create input action trigger
        public static JSONStorableAction SetupAction(MVRScript script, string name, JSONStorableAction.ActionCallback callback)
        {
            JSONStorableAction action = new JSONStorableAction(name, callback);
            script.RegisterAction(action);
            return action;
        }


        // ===========================================================================================
        // Custom UI system with new UI elements and the ability to easily add/remove UI at runtime
        //
        // Usage instructions:
        // - Before using the custom UI elements, call from your MVRScript:
        //       Utils.OnInitUI(CreateUIElement);
        // - When your MVRScript receives the OnDestroy message call:
        //       Utils.OnDestroyUI();

        // Create one-line text input with label
        public static UIDynamicLabelInput SetupTextInput(MVRScript script, string label, JSONStorableString storable, bool? rightSide)
        {
            if (ourLabelWithInputPrefab == null)
            {
                ourLabelWithInputPrefab = new GameObject("LabelInput");
                ourLabelWithInputPrefab.SetActive(false);
                RectTransform rt = ourLabelWithInputPrefab.AddComponent<RectTransform>();
                rt.anchorMax = new Vector2(0, 1);
                rt.anchorMin = new Vector2(0, 1);
                rt.offsetMax = new Vector2(535, -500);
                rt.offsetMin = new Vector2(10, -600);
                LayoutElement le = ourLabelWithInputPrefab.AddComponent<LayoutElement>();
                le.flexibleWidth = 1;
                le.minHeight = 40;
                le.minWidth = 350;
                le.preferredHeight = 40;
                le.preferredWidth = 500;

                RectTransform backgroundTransform = script.manager.configurableScrollablePopupPrefab.transform.Find("Background") as RectTransform;
                backgroundTransform = UnityEngine.Object.Instantiate(backgroundTransform, ourLabelWithInputPrefab.transform);
                backgroundTransform.name = "Background";
                backgroundTransform.anchorMax = new Vector2(1, 1);
                backgroundTransform.anchorMin = new Vector2(0, 0);
                backgroundTransform.offsetMax = new Vector2(0, 0);
                backgroundTransform.offsetMin = new Vector2(0, -10);

                RectTransform labelTransform = script.manager.configurableScrollablePopupPrefab.transform.Find("Button/Text") as RectTransform; ;
                labelTransform = UnityEngine.Object.Instantiate(labelTransform, ourLabelWithInputPrefab.transform);
                labelTransform.name = "Text";
                labelTransform.anchorMax = new Vector2(0, 1);
                labelTransform.anchorMin = new Vector2(0, 0);
                labelTransform.offsetMax = new Vector2(155, -5);
                labelTransform.offsetMin = new Vector2(5, 0);
                Text labelText = labelTransform.GetComponent<Text>();
                labelText.text = "Name";
                labelText.color = Color.white;

                RectTransform inputTransform = script.manager.configurableTextFieldPrefab.transform as RectTransform;
                inputTransform = UnityEngine.Object.Instantiate(inputTransform, ourLabelWithInputPrefab.transform);
                inputTransform.anchorMax = new Vector2(1, 1);
                inputTransform.anchorMin = new Vector2(0, 0);
                inputTransform.offsetMax = new Vector2(-5, -5);
                inputTransform.offsetMin = new Vector2(160, -5);
                UIDynamicTextField textfield = inputTransform.GetComponent<UIDynamicTextField>();
                textfield.backgroundColor = Color.white;
                LayoutElement layout = textfield.GetComponent<LayoutElement>();
                layout.preferredHeight = layout.minHeight = 35;
                InputField inputfield = textfield.gameObject.AddComponent<InputField>();
                inputfield.textComponent = textfield.UItext;

                RectTransform textTransform = textfield.UItext.rectTransform;
                textTransform.anchorMax = new Vector2(1, 1);
                textTransform.anchorMin = new Vector2(0, 0);
                textTransform.offsetMax = new Vector2(-5, -5);
                textTransform.offsetMin = new Vector2(10, -5);

                UnityEngine.Object.Destroy(textfield);

                UIDynamicLabelInput uid = ourLabelWithInputPrefab.AddComponent<UIDynamicLabelInput>();
                uid.label = labelText;
                uid.input = inputfield;
            }

            {
                var createUIElement = GetCreateUIElement(script);

                Transform t = createUIElement(ourLabelWithInputPrefab.transform, rightSide);
                UIDynamicLabelInput uid = t.gameObject.GetComponent<UIDynamicLabelInput>();
                storable.inputField = uid.input;
                uid.label.text = label;
                t.gameObject.SetActive(true);
                return uid;
            }
        }

        // Create label that as an X button on the right side.
        public static UIDynamicLabelXButton SetupLabelXButton(MVRScript script, string label, UnityAction callback, bool? rightSide)
        {
            if (ourLabelWithXButtonPrefab == null)
            {
                ourLabelWithXButtonPrefab = new GameObject("LabelXButton");
                ourLabelWithXButtonPrefab.SetActive(false);
                RectTransform rt = ourLabelWithXButtonPrefab.AddComponent<RectTransform>();
                rt.anchorMax = new Vector2(0, 1);
                rt.anchorMin = new Vector2(0, 1);
                rt.offsetMax = new Vector2(535, -500);
                rt.offsetMin = new Vector2(10, -600);
                LayoutElement le = ourLabelWithXButtonPrefab.AddComponent<LayoutElement>();
                le.flexibleWidth = 1;
                le.minHeight = 50;
                le.minWidth = 350;
                le.preferredHeight = 50;
                le.preferredWidth = 500;

                RectTransform backgroundTransform = script.manager.configurableScrollablePopupPrefab.transform.Find("Background") as RectTransform;
                backgroundTransform = UnityEngine.Object.Instantiate(backgroundTransform, ourLabelWithXButtonPrefab.transform);
                backgroundTransform.name = "Background";
                backgroundTransform.anchorMax = new Vector2(1, 1);
                backgroundTransform.anchorMin = new Vector2(0, 0);
                backgroundTransform.offsetMax = new Vector2(0, 0);
                backgroundTransform.offsetMin = new Vector2(0, -10);

                RectTransform buttonTransform = script.manager.configurableScrollablePopupPrefab.transform.Find("Button") as RectTransform;
                buttonTransform = UnityEngine.Object.Instantiate(buttonTransform, ourLabelWithXButtonPrefab.transform);
                buttonTransform.name = "Button";
                buttonTransform.anchorMax = new Vector2(1, 1);
                buttonTransform.anchorMin = new Vector2(1, 0);
                buttonTransform.offsetMax = new Vector2(0, 0);
                buttonTransform.offsetMin = new Vector2(-60, -10);
                Button buttonButton = buttonTransform.GetComponent<Button>();
                Text buttonText = buttonTransform.Find("Text").GetComponent<Text>();
                buttonText.text = "X";

                RectTransform labelTransform = buttonText.rectTransform;
                labelTransform = UnityEngine.Object.Instantiate(labelTransform, ourLabelWithXButtonPrefab.transform);
                labelTransform.name = "Text";
                labelTransform.anchorMax = new Vector2(1, 1);
                labelTransform.anchorMin = new Vector2(0, 0);
                labelTransform.offsetMax = new Vector2(-65, 0);
                labelTransform.offsetMin = new Vector2(5, -10);
                Text labelText = labelTransform.GetComponent<Text>();
                labelText.verticalOverflow = VerticalWrapMode.Overflow;

                UIDynamicLabelXButton uid = ourLabelWithXButtonPrefab.AddComponent<UIDynamicLabelXButton>();
                uid.label = labelText;
                uid.button = buttonButton;
            }

            {
                var createUIElement = GetCreateUIElement(script);

                Transform t = createUIElement(ourLabelWithXButtonPrefab.transform, rightSide);
                UIDynamicLabelXButton uid = t.gameObject.GetComponent<UIDynamicLabelXButton>();
                uid.label.text = label;
                uid.button.onClick.AddListener(callback);
                t.gameObject.SetActive(true);
                return uid;
            }
        }

        public static UIDynamicTextInfo SetupInfoTextNoScroll(MVRScript script, string text, float height, bool? rightSide)
        {
            if (ourTextInfoPrefab == null)
            {
                ourTextInfoPrefab = new GameObject("TextInfo");
                ourTextInfoPrefab.SetActive(false);
                RectTransform rt = ourTextInfoPrefab.AddComponent<RectTransform>();
                rt.anchorMax = new Vector2(0, 1);
                rt.anchorMin = new Vector2(0, 1);
                rt.offsetMax = new Vector2(535, -500);
                rt.offsetMin = new Vector2(10, -600);
                LayoutElement le = ourTextInfoPrefab.AddComponent<LayoutElement>();
                le.flexibleWidth = 1;
                le.minHeight = 35;
                le.minWidth = 350;
                le.preferredHeight = 35;
                le.preferredWidth = 500;

                RectTransform backgroundTransform = script.manager.configurableScrollablePopupPrefab.transform.Find("Background") as RectTransform;
                backgroundTransform = UnityEngine.Object.Instantiate(backgroundTransform, ourTextInfoPrefab.transform);
                backgroundTransform.name = "Background";
                backgroundTransform.anchorMax = new Vector2(1, 1);
                backgroundTransform.anchorMin = new Vector2(0, 0);
                backgroundTransform.offsetMax = new Vector2(0, 0);
                backgroundTransform.offsetMin = new Vector2(0, -10);

                RectTransform labelTransform = script.manager.configurableScrollablePopupPrefab.transform.Find("Button/Text") as RectTransform; ;
                labelTransform = UnityEngine.Object.Instantiate(labelTransform, ourTextInfoPrefab.transform);
                labelTransform.name = "Text";
                labelTransform.anchorMax = new Vector2(1, 1);
                labelTransform.anchorMin = new Vector2(0, 0);
                labelTransform.offsetMax = new Vector2(-5, 0);
                labelTransform.offsetMin = new Vector2(5, 0);
                Text labelText = labelTransform.GetComponent<Text>();
                labelText.alignment = TextAnchor.UpperLeft;

                UIDynamicTextInfo uid = ourTextInfoPrefab.AddComponent<UIDynamicTextInfo>();
                uid.text = labelText;
                uid.layout = le;
                uid.background = backgroundTransform;
            }

            {
                var createUIElement = GetCreateUIElement(script);

                Transform t = createUIElement(ourTextInfoPrefab.transform, rightSide);
                UIDynamicTextInfo uid = t.gameObject.GetComponent<UIDynamicTextInfo>();
                uid.text.text = text;

                uid.height = height;
                //uid.layout.minHeight = height;
                //uid.layout.preferredHeight = height;
                t.gameObject.SetActive(true);
                return uid;
            }
        }

        public static UIDynamicTextInfo SetupInfoTextNoScroll(MVRScript script, JSONStorableString storable, float height, bool? rightSide)
        {
            UIDynamicTextInfo uid = SetupInfoTextNoScroll(script, storable.val, height, rightSide);
            storable.setCallbackFunction = (string text) =>
            {
                if (uid != null && uid.text != null)
                    uid.text.text = text;
            };
            return uid;
        }

        public static UIDynamicTextInfo SetupInfoOneLine(MVRScript script, string text, bool? rightSide)
        {
            UIDynamicTextInfo uid = SetupInfoTextNoScroll(script, text, 35, rightSide);
            uid.background.offsetMin = new Vector2(0, 0);
            return uid;
        }

        public static UIDynamicTextInfo SetupInfoOneLine(MVRScript script, JSONStorableString storable, bool? rightSide)
        {
            UIDynamicTextInfo uid = SetupInfoTextNoScroll(script, storable, 35, rightSide);
            uid.background.offsetMin = new Vector2(0, 0);
            return uid;
        }

        public static UIDynamicTwinButton SetupTwinButton(MVRScript script, string leftLabel, UnityAction leftCallback, string rightLabel, UnityAction rightCallback, bool? rightSide)
        {
            if (ourTwinButtonPrefab == null)
            {
                ourTwinButtonPrefab = new GameObject("TwinButton");
                ourTwinButtonPrefab.SetActive(false);
                RectTransform rt = ourTwinButtonPrefab.AddComponent<RectTransform>();
                rt.anchorMax = new Vector2(0, 1);
                rt.anchorMin = new Vector2(0, 1);
                rt.offsetMax = new Vector2(535, -500);
                rt.offsetMin = new Vector2(10, -600);
                LayoutElement le = ourTwinButtonPrefab.AddComponent<LayoutElement>();
                le.flexibleWidth = 1;
                le.minHeight = 50;
                le.minWidth = 350;
                le.preferredHeight = 50;
                le.preferredWidth = 500;

                RectTransform buttonTransform = script.manager.configurableScrollablePopupPrefab.transform.Find("Button") as RectTransform;
                buttonTransform = UnityEngine.Object.Instantiate(buttonTransform, ourTwinButtonPrefab.transform);
                buttonTransform.name = "ButtonLeft";
                buttonTransform.anchorMax = new Vector2(0.5f, 1.0f);
                buttonTransform.anchorMin = new Vector2(0.0f, 0.0f);
                buttonTransform.offsetMax = new Vector2(-3, 0);
                buttonTransform.offsetMin = new Vector2(0, 0);
                Button buttonLeft = buttonTransform.GetComponent<Button>();
                Text labelLeft = buttonTransform.Find("Text").GetComponent<Text>();

                buttonTransform = UnityEngine.Object.Instantiate(buttonTransform, ourTwinButtonPrefab.transform);
                buttonTransform.name = "ButtonRight";
                buttonTransform.anchorMax = new Vector2(1.0f, 1.0f);
                buttonTransform.anchorMin = new Vector2(0.5f, 0.0f);
                buttonTransform.offsetMax = new Vector2(0, 0);
                buttonTransform.offsetMin = new Vector2(3, 0);
                Button buttonRight = buttonTransform.GetComponent<Button>();
                Text labelRight = buttonTransform.Find("Text").GetComponent<Text>();

                UIDynamicTwinButton uid = ourTwinButtonPrefab.AddComponent<UIDynamicTwinButton>();
                uid.labelLeft = labelLeft;
                uid.labelRight = labelRight;
                uid.buttonLeft = buttonLeft;
                uid.buttonRight = buttonRight;
            }

            {
                var createUIElement = GetCreateUIElement(script);

                Transform t = createUIElement(ourTwinButtonPrefab.transform, rightSide);
                UIDynamicTwinButton uid = t.GetComponent<UIDynamicTwinButton>();
                uid.labelLeft.text = leftLabel;
                uid.labelRight.text = rightLabel;
                uid.buttonLeft.onClick.AddListener(leftCallback);
                uid.buttonRight.onClick.AddListener(rightCallback);
                t.gameObject.SetActive(true);
                return uid;
            }
        }

        public static UIDynamicTwinToggle SetupTwinToggle(MVRScript script, string leftLabel, UnityAction leftCallback, string rightLabel, UnityAction rightCallback, bool? rightSide)
        {
            if (ourTwinTogglePrefab == null)
            {
                ourTwinTogglePrefab = new GameObject("TwinToggle");
                ourTwinTogglePrefab.SetActive(false);
                RectTransform rt = ourTwinTogglePrefab.AddComponent<RectTransform>();
                rt.anchorMax = new Vector2(0, 1);
                rt.anchorMin = new Vector2(0, 1);
                rt.offsetMax = new Vector2(535, -500);
                rt.offsetMin = new Vector2(10, -600);
                LayoutElement le = ourTwinTogglePrefab.AddComponent<LayoutElement>();
                le.flexibleWidth = 1;
                le.minHeight = 50;
                le.minWidth = 350;
                le.preferredHeight = 50;
                le.preferredWidth = 500;

                Transform togglePrefab = script.manager.configurableTogglePrefab.transform;
                // foreach(Transform child in togglePrefab) child.name.Print();
                RectTransform labelTransform = togglePrefab.Find("Label") as RectTransform;
                // foreach(Transform child in labelTransform) child.name.Print();
                // labelTransform.NullCheck();
                labelTransform = UnityEngine.Object.Instantiate(labelTransform, ourTwinTogglePrefab.transform);
                labelTransform.name = "LabelLeft1";
                labelTransform.anchorMax = new Vector2(0.5f, 1.0f);
                labelTransform.anchorMin = new Vector2(0.0f, 0.0f);
                labelTransform.offsetMax = new Vector2(-3, 0);
                labelTransform.offsetMin = new Vector2(0, 0);
                // foreach(var comp in labelTransform.GetComponents(typeof(Component))) comp.GetType().Print();
                // labelTransform.NullCheck();
                // Toggle toggleLeft = labelTransform.GetComponent<Toggle>();labelTransform.NullCheck();
                Text labelLeft = labelTransform.GetComponent<Text>();

                RectTransform panelTransform = togglePrefab.Find("Panel") as RectTransform;
                // foreach(Transform child in labelTransform) child.name.Print();
                // labelTransform.NullCheck();
                panelTransform = UnityEngine.Object.Instantiate(panelTransform, ourTwinTogglePrefab.transform);
                panelTransform.name = "PanelLeft";
                panelTransform.anchorMax = new Vector2(.1f, 1f);
                // panelTransform.anchorMin = new Vector2(0.0f, 0.0f);
                // panelTransform.offsetMax = new Vector2(-3, 0);
                // panelTransform.offsetMin = new Vector2(0, 0);
                // foreach(var comp in labelTransform.GetComponents(typeof(Component))) comp.GetType().Print();
                // labelTransform.NullCheck();
                // Toggle toggleLeft = labelTransform.GetComponent<Toggle>();labelTransform.NullCheck();
                // Text panelLeft = panelTransform.GetComponent<Text>();

                RectTransform backGroundTransform = togglePrefab.Find("Panel") as RectTransform;
                // foreach(Transform child in labelTransform) child.name.Print();
                // labelTransform.NullCheck();
                backGroundTransform = UnityEngine.Object.Instantiate(backGroundTransform, ourTwinTogglePrefab.transform);
                backGroundTransform.name = "BackGroundLeft";
                backGroundTransform.anchorMax = new Vector2(.5f, 1f);
                // panelTransform.anchorMin = new Vector2(0.0f, 0.0f);
                // panelTransform.offsetMax = new Vector2(-3, 0);
                // panelTransform.offsetMin = new Vector2(0, 0);
                foreach (var comp in backGroundTransform.GetComponents(typeof(Component))) comp.GetType();
                // labelTransform.NullCheck();
                // Toggle toggleLeft = labelTransform.GetComponent<Toggle>();labelTransform.NullCheck();
                // Text panelLeft = panelTransform.GetComponent<Text>();




                labelTransform = UnityEngine.Object.Instantiate(labelTransform, ourTwinTogglePrefab.transform);
                labelTransform.name = "LabelRight1";
                labelTransform.anchorMax = new Vector2(1.0f, 1.0f);
                labelTransform.anchorMin = new Vector2(0.5f, 0.0f);
                labelTransform.offsetMax = new Vector2(0, 0);
                labelTransform.offsetMin = new Vector2(3, 0);
                // foreach(var comp in labelTransform.GetComponents(typeof(Component))) comp.GetType().Print();
                // labelTransform.NullCheck();
                // Toggle toggleLeft = labelTransform.GetComponent<Toggle>();labelTransform.NullCheck();

                Text labelRight = labelTransform.GetComponent<Text>();


                // labelTransform = UnityEngine.Object.Instantiate(labelTransform, ourTwinTogglePrefab.transform);
                // labelTransform.name = "ButtonRight";
                // labelTransform.anchorMax = new Vector2(1.0f, 1.0f);
                // labelTransform.anchorMin = new Vector2(0.5f, 0.0f);
                // labelTransform.offsetMax = new Vector2(0, 0);
                // labelTransform.offsetMin = new Vector2(3, 0);
                // Toggle toggleRight = labelTransform.GetComponent<Toggle>();
                // Text labelRight = labelTransform.Find("Text").GetComponent<Text>();

                UIDynamicTwinToggle uid = ourTwinTogglePrefab.AddComponent<UIDynamicTwinToggle>();
                uid.labelLeft = labelLeft;
                uid.labelLeft = labelLeft;
                uid.labelRight = labelRight;
                // uid.toggleLeft = toggleLeft;
                // uid.toggleRight = toggleRight;
            }

            {
                var createUIElement = GetCreateUIElement(script);

                Transform t = createUIElement(ourTwinTogglePrefab.transform, rightSide);
                UIDynamicTwinToggle uid = t.GetComponent<UIDynamicTwinToggle>();
                uid.labelLeft.text = leftLabel;
                uid.labelRight.text = rightLabel;
                // uid.toggleLeft.onClick.AddListener(leftCallback);
                // uid.toggleRight.onClick.AddListener(rightCallback);
                t.gameObject.SetActive(true);
                return uid;
            }
        }

        // Call to remove a list of UI elements before rebuilding your UI.
        public static void RemoveUIElements(MVRScript script, List<object> menuElements)
        {
            for (int i = 0; i < menuElements.Count; ++i)
            {
                if (menuElements[i] is JSONStorableParam)
                {
                    JSONStorableParam jsp = menuElements[i] as JSONStorableParam;

                    if (jsp is JSONStorableFloat)
                        script.RemoveSlider(jsp as JSONStorableFloat);
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

        public delegate Transform CreateUIElement(Transform prefab, bool? rightSide);
        public static void OnInitUI(MVRScript script, CreateUIElement createUIElementCallback)
        {
            if (script != null)
            {
                ourCreateUIElements[script] = createUIElementCallback;
            }
            else
            {
                ourCreateUIElement = createUIElementCallback;
            }
        }

        public static void OnDestroyUI(MVRScript script)
        {
            if (script != null)
            {
                if (ourCreateUIElements.ContainsKey(script))
                {
                    ourCreateUIElements.Remove(script);
                }
            }

            SafeDestroy(ref ourLabelWithInputPrefab);
            SafeDestroy(ref ourLabelWithXButtonPrefab);
            SafeDestroy(ref ourTextInfoPrefab);
            SafeDestroy(ref ourTwinButtonPrefab);
        }

        private static void SafeDestroy(ref GameObject go)
        {
            if (go != null)
            {
                UnityEngine.Object.Destroy(go);
                go = null;
            }
        }

        private static CreateUIElement GetCreateUIElement(MVRScript script)
        {
            if (script != null)
            {
                if (ourCreateUIElements.ContainsKey(script))
                {
                    return ourCreateUIElements[script];
                }
            }

            if (ourCreateUIElement != null)
            {
                return ourCreateUIElement;
            }

            throw new Exception("Using the custom UI elements, call from your MVRScript: Utils.OnInitUI(this,CreateUIElement);");
        }

        private static CreateUIElement ourCreateUIElement;
        private static Dictionary<MVRScript, CreateUIElement> ourCreateUIElements = new Dictionary<MVRScript, CreateUIElement>();
        private static GameObject ourLabelWithInputPrefab;
        private static GameObject ourLabelWithXButtonPrefab;
        private static GameObject ourTextInfoPrefab;
        private static GameObject ourTwinButtonPrefab;
        private static GameObject ourTwinTogglePrefab;

        // ===========================================================================================

        // Helper to add a component if missing.
        public static T GetOrAddComponent<T>(Component c) where T : Component
        {
            T t = c.GetComponent<T>();
            if (t == null)
                t = c.gameObject.AddComponent<T>();
            return t;
        }

        // Adjust slider max range to next power of 10 (1, 10, 100, 1000, ...) from slider value
        public static void AdjustSliderRange(JSONStorableFloat slider)
        {
            float m = Mathf.Log10(slider.val);
            m = Mathf.Max(Mathf.Ceil(m), 1);
            slider.max = Mathf.Pow(10, m);
        }

        // Adjust maxSlider value and max range after minSlider was changed to ensure minSlider <= maxSlider.
        public static void AdjustMaxSliderFromMin(float minValue, JSONStorableFloat maxSlider)
        {
            if (maxSlider.slider != null)
                maxSlider.max = maxSlider.slider.maxValue; // slider sometimes does not update the storable

            float v = Mathf.Max(minValue, maxSlider.val);
            float m = Mathf.Max(v, maxSlider.max);
            m = Mathf.Max(Mathf.Ceil(Mathf.Log10(m)), 1);
            maxSlider.max = Mathf.Pow(10, m);
            maxSlider.valNoCallback = v;
        }

        // ===========================================================================================

        private static void QueueLoadTexture(string url, TextureSettings settings, TextureSetCallback callback)
        {
            if (ImageLoaderThreaded.singleton == null)
                return;
            if (string.IsNullOrEmpty(url))
                return;

            ImageLoaderThreaded.QueuedImage queuedImage = new ImageLoaderThreaded.QueuedImage();
            queuedImage.imgPath = url;
            queuedImage.forceReload = true;
            queuedImage.skipCache = true;
            queuedImage.compress = settings.compress;
            queuedImage.createMipMaps = settings.createMipMaps;
            queuedImage.isNormalMap = settings.isNormalMap;
            queuedImage.linear = settings.linearColor;
            queuedImage.createAlphaFromGrayscale = settings.createAlphaFromGrayscale;
            queuedImage.createNormalFromBump = settings.createNormalFromBump;
            queuedImage.bumpStrength = settings.bumpStrength;
            queuedImage.isThumbnail = false;
            queuedImage.fillBackground = false;
            queuedImage.invert = false;
            queuedImage.callback = (ImageLoaderThreaded.QueuedImage qi) =>
            {
                Texture2D tex = qi.tex;
                if (tex != null)
                {
                    tex.wrapMode = settings.wrapMode;
                    tex.filterMode = settings.filterMode;
                    tex.anisoLevel = settings.anisoLevel;
                }
                callback(tex);
            };
            ImageLoaderThreaded.singleton.QueueImage(queuedImage);
        }





        public static void LogTransform(string message, Transform t)
        {
            StringBuilder b = new StringBuilder();
            b.Append(message).Append("\n");
            LogTransformInternal(t, 0, b);
            SuperController.LogMessage(b.ToString());
        }

        private static void LogTransformInternal(Transform t, int indent, StringBuilder b)
        {
            b.Append(' ', indent * 4).Append(t.name).Append(" (active: ").Append(t.gameObject.activeSelf).Append(")\n");

            Component[] comps = t.GetComponents<Component>();
            if (comps.Length > 0)
            {
                b.Append(' ', indent * 4 + 2).Append("Components:\n");
                for (int i = 0; i < comps.Length; ++i)
                {
                    Component c = comps[i];
                    b.Append(' ', indent * 4 + 4).Append(c.GetType().FullName).Append("\n");

                    if (c is RectTransform)
                    {
                        RectTransform rt = c as RectTransform;
                        b.Append(' ', indent * 4 + 8).Append("anchoredPosition=").Append(rt.anchoredPosition).Append("\n");
                        b.Append(' ', indent * 4 + 8).Append("anchorMax=").Append(rt.anchorMax).Append("\n");
                        b.Append(' ', indent * 4 + 8).Append("anchorMin=").Append(rt.anchorMin).Append("\n");
                        b.Append(' ', indent * 4 + 8).Append("offsetMax=").Append(rt.offsetMax).Append("\n");
                        b.Append(' ', indent * 4 + 8).Append("offsetMin=").Append(rt.offsetMin).Append("\n");
                        b.Append(' ', indent * 4 + 8).Append("pivot=").Append(rt.pivot).Append("\n");
                        b.Append(' ', indent * 4 + 8).Append("rect=").Append(rt.rect).Append("\n");
                    }
                    else if (c is LayoutElement)
                    {
                        LayoutElement le = c as LayoutElement;
                        b.Append(' ', indent * 4 + 8).Append("flexibleHeight=").Append(le.flexibleHeight).Append("\n");
                        b.Append(' ', indent * 4 + 8).Append("flexibleWidth=").Append(le.flexibleWidth).Append("\n");
                        b.Append(' ', indent * 4 + 8).Append("ignoreLayout=").Append(le.ignoreLayout).Append("\n");
                        b.Append(' ', indent * 4 + 8).Append("layoutPriority=").Append(le.layoutPriority).Append("\n");
                        b.Append(' ', indent * 4 + 8).Append("minHeight=").Append(le.minHeight).Append("\n");
                        b.Append(' ', indent * 4 + 8).Append("minWidth=").Append(le.minWidth).Append("\n");
                        b.Append(' ', indent * 4 + 8).Append("preferredHeight=").Append(le.preferredHeight).Append("\n");
                        b.Append(' ', indent * 4 + 8).Append("preferredWidth=").Append(le.preferredWidth).Append("\n");
                    }
                    else if (c is Image)
                    {
                        Image img = c as Image;
                        b.Append(' ', indent * 4 + 8).Append("mainTexture=").Append(img.mainTexture?.name).Append("\n");
                        b.Append(' ', indent * 4 + 8).Append("sprite=").Append(img.sprite?.name).Append("\n");
                        b.Append(' ', indent * 4 + 8).Append("color=").Append(img.color).Append("\n");
                    }
                }
            }
            if (t.childCount > 0)
            {
                b.Append(' ', indent * 4 + 2).Append("Children:\n");
                for (int i = 0; i < t.childCount; ++i)
                {
                    Transform c = t.GetChild(i);
                    LogTransformInternal(c, indent + 1, b);
                }
            }
        }
    }

    // ===========================================================================================

    public delegate void EnumSetCallback<TEnum>(TEnum v);
    public delegate void TextureSetCallback(Texture2D tex);

    public class TextureSettings
    {
        public bool compress = false;
        public bool createMipMaps = true;
        public bool isNormalMap = false;
        public bool linearColor = true; // Using linear or sRGB color space.
        public bool createAlphaFromGrayscale = false;
        public bool createNormalFromBump = false;
        public float bumpStrength = 1.0f;
        public TextureWrapMode wrapMode = TextureWrapMode.Repeat;
        public FilterMode filterMode = FilterMode.Trilinear;
        public int anisoLevel = 5; // 0: Forced off, 1: Off, quality setting can override, 2-9: Anisotropic filtering levels.
    }

    // ===========================================================================================

    public class AssetBundleAudioClip : NamedAudioClip
    {
        public AssetBundleAudioClip(Request aRequest, string aPath, string aName)
        {
            manager = null;
            sourceClip = aRequest.assetBundle?.LoadAsset<AudioClip>(aPath + aName);
            uid = aName;
            displayName = aName;
            category = "AssetBundle";
            destroyed = false;
        }
    }

    // ===========================================================================================

    // TriggerHandler implementation for easier handling of custom triggers.
    // Essentially call this in your plugin init code:
    //     StartCoroutine(SimpleTriggerHandler.LoadAssets());
    //
    // Credit to AcidBubbles for figuring out how to do custom triggers.
    public class SimpleTriggerHandler : TriggerHandler
    {
        public static bool Loaded { get; private set; }

        private static SimpleTriggerHandler myInstance;

        private RectTransform myTriggerActionsPrefab;
        private RectTransform myTriggerActionMiniPrefab;
        private RectTransform myTriggerActionDiscretePrefab;
        private RectTransform myTriggerActionTransitionPrefab;

        public static SimpleTriggerHandler Instance
        {
            get
            {
                if (myInstance == null)
                    myInstance = new SimpleTriggerHandler();
                return myInstance;
            }
        }

        public static void LoadAssets()
        {
            SuperController.singleton.StartCoroutine(Instance.LoadAssetsInternal());
        }

        private IEnumerator LoadAssetsInternal()
        {
            foreach (var x in LoadAsset("z_ui2", "TriggerActionsPanel", p => myTriggerActionsPrefab = p))
                yield return x;
            foreach (var x in LoadAsset("z_ui2", "TriggerActionMiniPanel", p => myTriggerActionMiniPrefab = p))
                yield return x;
            foreach (var x in LoadAsset("z_ui2", "TriggerActionDiscretePanel", p => myTriggerActionDiscretePrefab = p))
                yield return x;
            foreach (var x in LoadAsset("z_ui2", "TriggerActionTransitionPanel", p => myTriggerActionTransitionPrefab = p))
                yield return x;

            Loaded = true;
        }

        private IEnumerable LoadAsset(string assetBundleName, string assetName, Action<RectTransform> assign)
        {
            AssetBundleLoadAssetOperation request = AssetBundleManager.LoadAssetAsync(assetBundleName, assetName, typeof(GameObject));
            if (request == null)
                throw new NullReferenceException($"Request for {assetName} in {assetBundleName} assetbundle failed: Null request.");
            yield return request;
            GameObject go = request.GetAsset<GameObject>();
            if (go == null)
                throw new NullReferenceException($"Request for {assetName} in {assetBundleName} assetbundle failed: Null GameObject.");
            RectTransform prefab = go.GetComponent<RectTransform>();
            if (prefab == null)
                throw new NullReferenceException($"Request for {assetName} in {assetBundleName} assetbundle failed: Null RectTansform.");
            assign(prefab);
        }


        void TriggerHandler.RemoveTrigger(Trigger t)
        {
            // nothing to do
        }

        void TriggerHandler.DuplicateTrigger(Trigger t)
        {
            throw new NotImplementedException();
        }

        RectTransform TriggerHandler.CreateTriggerActionsUI()
        {
            return UnityEngine.Object.Instantiate(myTriggerActionsPrefab);
        }

        RectTransform TriggerHandler.CreateTriggerActionMiniUI()
        {
            return UnityEngine.Object.Instantiate(myTriggerActionMiniPrefab);
        }

        RectTransform TriggerHandler.CreateTriggerActionDiscreteUI()
        {
            return UnityEngine.Object.Instantiate(myTriggerActionDiscretePrefab);
        }

        RectTransform TriggerHandler.CreateTriggerActionTransitionUI()
        {
            RectTransform rt = UnityEngine.Object.Instantiate(myTriggerActionTransitionPrefab);
            rt.GetComponent<TriggerActionTransitionUI>().startWithCurrentValToggle.gameObject.SetActive(false);
            return rt;
        }

        void TriggerHandler.RemoveTriggerActionUI(RectTransform rt)
        {
            UnityEngine.Object.Destroy(rt?.gameObject);
        }
    }

    // Base class for easier handling of custom triggers.
    public abstract class CustomTrigger : Trigger
    {
        public string Name
        {
            get { return name; }
            set { name = value; myNeedInit = true; }
        }

        public string SecondaryName
        {
            get { return secondaryName; }
            set { secondaryName = value; myNeedInit = true; }
        }

        public MVRScript Owner
        {
            get; private set;
        }

        private string name;
        private string secondaryName;
        private bool myNeedInit = true;

        public CustomTrigger(MVRScript owner, string name, string secondary = null)
        {
            Name = name;
            SecondaryName = secondary;
            Owner = owner;
            handler = SimpleTriggerHandler.Instance;
        }

        public CustomTrigger(CustomTrigger other)
        {
            Name = other.Name;
            SecondaryName = other.SecondaryName;
            Owner = other.Owner;
            handler = SimpleTriggerHandler.Instance;

            JSONClass jc = other.GetJSON(Owner.subScenePrefix);
            base.RestoreFromJSON(jc, Owner.subScenePrefix, false);
        }

        public void OpenPanel()
        {
            if (!SimpleTriggerHandler.Loaded)
            {
                SuperController.LogError("CustomTrigger: You need to call SimpleTriggerHandler.LoadAssets() before use.");
                return;
            }

            triggerActionsParent = Owner.UITransform;
            InitTriggerUI();
            OpenTriggerActionsPanel();
            if (myNeedInit)
            {
                Transform panel = triggerActionsPanel.Find("Panel");
                panel.Find("Header Text").GetComponent<Text>().text = Name;
                Transform secondaryHeader = panel.Find("Trigger Name Text");
                secondaryHeader.gameObject.SetActive(!string.IsNullOrEmpty(SecondaryName));
                secondaryHeader.GetComponent<Text>().text = SecondaryName;

                InitPanel();
                myNeedInit = false;
            }
        }

        protected abstract void InitPanel();

        public void RestoreFromJSON(JSONClass jc, string subScenePrefix, bool isMerge, bool setMissingToDefault)
        {
            if (jc.HasKey(Name))
            {
                JSONClass tc = jc[Name].AsObject;
                if (tc != null)
                    base.RestoreFromJSON(tc, subScenePrefix, isMerge);
            }
            else if (setMissingToDefault)
            {
                base.RestoreFromJSON(new JSONClass());
            }
        }
    }

    // Wrapper for easier handling of custom event triggers.
    public class EventTrigger : CustomTrigger
    {
        public EventTrigger(MVRScript owner, string name, string secondary = null)
            : base(owner, name, secondary)
        {
        }

        public EventTrigger(EventTrigger other)
            : base(other)
        {
        }

        protected override void InitPanel()
        {
            Transform content = triggerActionsPanel.Find("Content");
            content.Find("Tab1/Label").GetComponent<Text>().text = "Event Actions";
            content.Find("Tab2").gameObject.SetActive(false);
            content.Find("Tab3").gameObject.SetActive(false);
        }

        public void Trigger()
        {
            active = true;
            active = false;
        }

        public void Trigger(List<TriggerActionDiscrete> actionsNeedingUpdateOut)
        {
            Trigger();
            for (int i = 0; i < discreteActionsStart.Count; ++i)
            {
                if (discreteActionsStart[i].timerActive)
                    actionsNeedingUpdateOut.Add(discreteActionsStart[i]);
            }
        }
    }

    // Wrapper for easier handling of custom float triggers.
    public class FloatTrigger : CustomTrigger
    {
        public FloatTrigger(MVRScript owner, string name, string secondary = null)
            : base(owner, name, secondary)
        {
        }

        public FloatTrigger(FloatTrigger other)
            : base(other)
        {
        }

        protected override void InitPanel()
        {
            Transform content = triggerActionsPanel.Find("Content");
            content.Find("Tab2/Label").GetComponent<Text>().text = "Value Actions";
            content.Find("Tab3/Label").GetComponent<Text>().text = "Event Actions";
            content.Find("Tab2").GetComponent<Toggle>().isOn = true;
            content.Find("Tab1").gameObject.SetActive(false);
        }

        public void Trigger(float v)
        {
            _transitionInterpValue = Mathf.Clamp01(v);
            if (transitionInterpValueSlider != null)
                transitionInterpValueSlider.value = _transitionInterpValue;
            for (int i = 0; i < transitionActions.Count; ++i)
                transitionActions[i].TriggerInterp(_transitionInterpValue, true);
            // for (int i=0; i<discreteActionsEnd.Count; ++i)
            // 	discreteActionsEnd[i].Trigger();
        }

        public void Trigger()
        {
            for (int i = 0; i < discreteActionsEnd.Count; ++i)
                discreteActionsEnd[i].Trigger();
        }

        public void Trigger(float v, List<TriggerActionDiscrete> actionsNeedingUpdateOut)
        {
            Trigger(v);
            for (int i = 0; i < discreteActionsEnd.Count; ++i)
            {
                if (discreteActionsEnd[i].timerActive)
                    actionsNeedingUpdateOut.Add(discreteActionsEnd[i]);
            }
        }
    }

    // ===========================================================================================

    public class UIDynamicUtils : UIDynamic
    {
    }

    public class UIDynamicLabelInput : UIDynamicUtils
    {
        public Text label;
        public InputField input;
    }

    public class UIDynamicLabelXButton : UIDynamicUtils
    {
        public Text label;
        public Button button;
    }

    public class UIDynamicTwinButton : UIDynamicUtils
    {
        public Text labelLeft;
        public Text labelRight;
        public Button buttonLeft;
        public Button buttonRight;
    }

    public class UIDynamicTwinToggle : UIDynamicUtils
    {
        public Text labelLeft;
        public Text labelRight;
        public Toggle toggleLeft;
        public Toggle toggleRight;
    }

    public class UIDynamicTextInfo : UIDynamicUtils
    {
        public Text text;
        public LayoutElement layout;
        public RectTransform background;

        float _height = 0f;
        public new float height
        {
            get
            {
                return _height;
            }
            set
            {
                if (_height != value)
                {
                    _height = value;
                    this.layout.minHeight = value;
                    this.layout.preferredHeight = value;
                }
            }
        }
    }
}
