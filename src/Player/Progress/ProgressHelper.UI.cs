using MacGruber;
using System.Collections.Generic;

namespace mmd2timeline
{
    internal partial class ProgressHelper
    {
        /// <summary>
        /// 默认空字符串
        /// </summary>
        private const string noneString = "None";

        /// <summary>
        /// 空字符串列表
        /// </summary>
        internal readonly List<string> noneStrings = new List<string> { noneString };

        /// <summary>
        /// 设置UI
        /// </summary>
        GroupUI _SettingsUI;

        /// <summary>
        /// 进度
        /// </summary>
        JSONStorableFloat _progressJSON;

        /// <summary>
        /// 最大长度
        /// </summary>
        JSONStorableFloat _maxLengthJSON;

        //TriggerEvent _progressFloatTrigger;

        /// <summary>
        /// 初始化设置UI
        /// </summary>
        internal void CreateUI(BaseScript self, bool rightSide = false)
        {
            if (_SettingsUI != null)
                return;

            _SettingsUI = new GroupUI(self);

            //_progressFloatTrigger = TriggerEventHelper.GetInstance().AddTrigger("Progress Triger");

            // 播放进度
            _progressJSON = self.SetupSliderFloat("Progress", 0f, 0f, 0f, v => SetProgress(v), rightSide);
            self.RegisterFloat(_progressJSON);
            _SettingsUI.Elements.Add(_progressJSON);

            _maxLengthJSON = self.SetupSliderFloat("Max Length", 0f, 0f, 0f, v => SetMaxLength(v), rightSide);
            _SettingsUI.Elements.Add(_maxLengthJSON);
        }

        /// <summary>
        /// 显示设置UI
        /// </summary>
        /// <param name="show"></param>
        internal void ShowUI(bool show)
        {
            _SettingsUI?.Show(show);
        }
    }
}
