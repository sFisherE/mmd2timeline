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

        ///// <summary>
        ///// 选择器UI
        ///// </summary>
        //GroupUI _ChooserUI;

        /// <summary>
        /// 进度
        /// </summary>
        JSONStorableFloat _progressJSON;

        /// <summary>
        /// 最大长度
        /// </summary>
        JSONStorableFloat _maxLengthJSON;

        /// <summary>
        /// 初始化设置UI
        /// </summary>
        internal void CreateUI(BaseScript self, bool rightSide = false)
        {
            if (_SettingsUI != null)
                return;

            _SettingsUI = new GroupUI(self);

            // 播放进度
            _progressJSON = Utils.SetupSliderFloat(self, Lang.Get("Progress"), 0f, 0f, 0f, v => SetProgress(v), rightSide);
            _SettingsUI.Elements.Add(_progressJSON);

            _maxLengthJSON = Utils.SetupSliderFloat(self, Lang.Get("Max Length"), 0f, 0f, 0f, v => SetLength(v), rightSide);
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

        ///// <summary>
        ///// 显示选择器UI
        ///// </summary>
        ///// <param name="show"></param>
        //internal void ShowChooserUI(bool show)
        //{
        //    _ChooserUI?.ShowSelf(show);
        //}

        ///// <summary>
        ///// 创建选择器UI
        ///// </summary>
        ///// <param name="self"></param>
        ///// <param name="rightSide"></param>
        //internal void CreateChooserUI(BaseScript self, bool rightSide = false)
        //{
        //    if (_ChooserUI != null)
        //        return;

        //    _ChooserUI = new GroupUI(self);

        //    // 音频选择
        //    var musicPopup = self.SetupStringChooser("Music", noneStrings, 600f, rightSide);
        //    musicPopup.setCallbackFunction = v =>
        //    {
        //        // TODO 音频选中的处理
        //        _audioSetting.AudioPath = v;

        //        LoadAudio(v);
        //    };

        //    _ChooserUI.Elements.Add(musicPopup);
        //}
    }
}
