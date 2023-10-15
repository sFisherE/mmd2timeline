using System;
using UnityEngine;
using UnityEngine.UI;

namespace mmd2timeline
{
    internal partial class Player
    {
        /// <summary>
        /// 状态文本
        /// </summary>
        public JSONStorableString hudText;

        /// <summary>
        /// HUD Canvas名称
        /// </summary>
        const string HUD_CANVAS_NAME = "HUDCanvas";

        /// <summary>
        /// 初始化HUD UI
        /// </summary>
        private void InitHUDUI()
        {
            try
            {
                LogUtil.Debug($"InitHUDUI::Start");

                hudText = new JSONStorableString("HUDText", "");

                Canvas canvas = new GameObject(HUD_CANVAS_NAME).AddComponent<Canvas>();
                canvas.renderMode = RenderMode.ScreenSpaceOverlay;
                SuperController.singleton.AddCanvas(canvas);
                canvas.gameObject.AddComponent<GraphicRaycaster>();
                CanvasScaler canvasScaler = canvas.gameObject.AddComponent<CanvasScaler>();
                canvasScaler.scaleFactor = 1f;
                canvasScaler.dynamicPixelsPerUnit = 1f;
                VerticalLayoutGroup verticalLayoutGroup = canvas.gameObject.AddComponent<VerticalLayoutGroup>();
                verticalLayoutGroup.childAlignment = TextAnchor.UpperRight;
                verticalLayoutGroup.childControlWidth = false;
                verticalLayoutGroup.childForceExpandHeight = false;
                verticalLayoutGroup.childForceExpandWidth = false;
                verticalLayoutGroup.spacing = 10f;
                verticalLayoutGroup.padding.top = 20;
                verticalLayoutGroup.padding.right = 20;
                verticalLayoutGroup.GetComponent<RectTransform>().sizeDelta = new Vector2(400f, 400f);

                UIDynamicTextField component = UnityEngine.Object.Instantiate(manager.configurableTextFieldPrefab).GetComponent<UIDynamicTextField>();
                component.backgroundColor = Color.clear;
                component.textColor = Color.white;
                component.UItext.alignment = TextAnchor.UpperRight;
                component.UItext.supportRichText = true;
                hudText.dynamicText = component;
                component.height = 400f;
                component.GetComponent<RectTransform>().sizeDelta = new Vector2(400f, component.height);
                component.transform.SetParent(verticalLayoutGroup.transform, worldPositionStays: false);
            }
            catch (Exception e)
            {
                LogUtil.LogError(e, $"InitHUDUI::");
            }
        }

        /// <summary>
        /// 显示HUD消息
        /// </summary>
        /// <param name="msg"></param>
        /// <param name="fontSize"></param>
        /// <param name="anchor"></param>
        internal void ShowHUDMessage(string msg = null, int fontSize = 24, TextAnchor anchor = TextAnchor.UpperRight)
        {
            if (string.IsNullOrEmpty(msg))
            {
                msg = $"{PLUGIN_NAME} v{VERSION}";
            }

            hudText.val = msg;
            hudText.text.alignment = anchor;
            hudText.text.fontSize = fontSize;

            ShowUIElement(hudText, true);
        }

        /// <summary>
        /// 隐藏HUD消息
        /// </summary>
        internal void HideHUDMessage()
        {
            ShowUIElement(hudText, false);
        }

        /// <summary>
        /// 销毁HUD UI
        /// </summary>
        void DestroyHUDUI()
        {
            UnityEngine.Object.Destroy(GameObject.Find(HUD_CANVAS_NAME));
        }
    }
}
