using UnityEngine;

namespace mmd2timeline
{
    internal partial class BaseScript
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
        /// 初始化Tip
        /// </summary>
        /// <param name="name"></param>
        protected void InitTip(string name)
        {
            _TipsJSON = new JSONStorableString("Tips", $"{name} {Lang.Get("is Ready.\nPlease select the Content to play in the Player.")}");
            _TipField = CreateFullTextField(_TipsJSON);
            _TipField.backgroundColor = new Color(1f, 0.92f, 0.016f, 0f);
            _TipField.UItext.fontSize = 40;
        }

        /// <summary>
        /// 显示Tip信息
        /// </summary>
        /// <param name="message"></param>
        /// <param name="action"></param>
        internal virtual void ShowTip(string message = null,
            float height = 620f,
            UnityEngine.TextAnchor alignment = UnityEngine.TextAnchor.MiddleCenter)
        {
            if (_TipField == null)
            {
                LogUtil.LogWarning("The Tip is Not Inited.");
                return;
            }
            if (message != null)
            {
                _TipsJSON.val = message;
            }

            _TipField.height = height;
            _TipField.gameObject.SetActive(true);
            _TipField.UItext.alignment = alignment;
        }

        /// <summary>
        /// 隐藏Tip
        /// </summary>
        protected virtual void HideTip()
        {
            if (_TipField == null)
            {
                LogUtil.LogWarning("The Tip is Not Inited.");
                return;
            }
            _TipField.gameObject.SetActive(false);
        }
    }
}
