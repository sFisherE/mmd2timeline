using System;
using UnityEngine;

namespace mmd2timeline.Player
{
    internal abstract partial class BaseScript
    {
        /// <summary>
        /// 提示信息
        /// </summary>
        protected JSONStorableString _TipsJSON;

        /// <summary>
        /// 提示信息显示器
        /// </summary>
        protected UIDynamicTextField _TipField;

        /// <summary>
        /// 提示信息的确认按钮
        /// </summary>
        protected UIDynamicButton _TipButton;

        protected UIDynamicSlider _Slider;

        protected JSONStorableFloat _ProgressJSON;

        /// <summary>
        /// 初始化Tip
        /// </summary>
        /// <param name="name"></param>
        protected void InitTip(string name)
        {
            LogUtil.Debug($"::InitTip::{name}");

            _ProgressJSON = new JSONStorableFloat("TipProgress", 0, 0, 0, true, false);

            _Slider = CreateFullSlider(_ProgressJSON);
            _Slider.quickButtonsEnabled = false;
            _Slider.rangeAdjustEnabled = false;
            _Slider.defaultButtonEnabled = false;

            _TipsJSON = new JSONStorableString("Tips", $"{name} {Lang.Get("is Ready.\nPlease select the Content to play in the Player.")}");
            _TipField = CreateFullTextField(_TipsJSON);
            _TipField.backgroundColor = new Color(1f, 0.92f, 0.016f, 0f);
            _TipField.UItext.fontSize = 40;

            _TipButton = CreateFullButton("OK");
        }

        /// <summary>
        /// 显示Tip信息
        /// </summary>
        /// <param name="message"></param>
        /// <param name="action"></param>
        internal virtual void ShowTip(string message = null,
            float height = 620f,
            float step = 0,
            float? total = null,
            Action action = null,
            string buttonText = null,
            UnityEngine.TextAnchor alignment = UnityEngine.TextAnchor.MiddleCenter)
        {
            if (_TipButton == null)
            {
                LogUtil.LogError("The Tip is Not Inited.");
                return;
            }
            if (message != null)
            {
                _TipsJSON.val = message;
            }

            _TipField.height = height;
            _TipField.gameObject.SetActive(true);
            _TipField.UItext.alignment = alignment;

            if (total.HasValue)
            {
                _Slider.gameObject.SetActive(false);
                _ProgressJSON.max = total.Value;
                _ProgressJSON.val = step;
            }
            else
            {
                _ProgressJSON.max = 0;
                _Slider.gameObject.SetActive(false);
            }

            if (action != null)
            {
                _TipButton.gameObject.SetActive(true);
                _TipButton.button.onClick.AddListener(() =>
                {
                    action?.Invoke();
                });

                if (string.IsNullOrEmpty(buttonText))
                {
                    buttonText = Lang.Get("OK");
                }

                _TipButton.buttonText.text = buttonText;
            }
            else
            {
                _TipButton.gameObject.SetActive(false);
            }
        }

        /// <summary>
        /// 隐藏Tip
        /// </summary>
        protected virtual void HideTip()
        {
            if (_TipButton == null)
            {
                LogUtil.LogError("The Tip is Not Inited.");
                return;
            }
            _TipButton.button.onClick.RemoveAllListeners();

            _TipButton.gameObject.SetActive(false);
            _TipField.gameObject.SetActive(false);
            _Slider.gameObject.SetActive(false);
        }
    }
}
