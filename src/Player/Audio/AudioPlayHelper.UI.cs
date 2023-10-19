using MacGruber;
using System.Collections.Generic;

namespace mmd2timeline
{
    internal partial class AudioPlayHelper
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
        /// 选择器UI
        /// </summary>
        GroupUI _ChooserUI;

        /// <summary>
        /// 延迟
        /// </summary>
        JSONStorableFloat _timeDelayJSON;

        /// <summary>
        /// 音频选择器
        /// </summary>
        JSONStorableStringChooser _chooser;

        /// <summary>
        /// 初始化设置UI
        /// </summary>
        internal void CreateSettingsUI(BaseScript self, bool rightSide = false)
        {
            if (_SettingsUI != null)
                return;

            _SettingsUI = new GroupUI(self);

            _timeDelayJSON = Utils.SetupSliderFloat(self, Lang.Get("Audio Delay"), 0f, 0f, 0f, rightSide);
            _timeDelayJSON.setCallbackFunction = v =>
            {
                // 更新延迟
                SetTimeDelay(v);
            };
            _SettingsUI.Elements.Add(_timeDelayJSON);
        }

        /// <summary>
        /// 设置延迟数值的范围
        /// </summary>
        /// <param name="length"></param>
        void SetDelayRange(float length)
        {
            _timeDelayJSON.min = 0 - length;
            _timeDelayJSON.max = length;
        }

        /// <summary>
        /// 显示设置UI
        /// </summary>
        /// <param name="show"></param>
        internal void ShowSettingUI(bool show)
        {
            if (_SettingsUI == null)
            {
                LogUtil.LogWarning($"_SettingsUI:::NOT INITED!");
            }
            _SettingsUI?.Show(show);
        }

        /// <summary>
        /// 显示选择器UI
        /// </summary>
        /// <param name="show"></param>
        internal void ShowChooserUI(bool show)
        {
            if (_ChooserUI == null)
            {
                LogUtil.LogWarning($"_ChooserUI:::NOT INITED!");
            }
            _ChooserUI?.Show(show);
        }

        /// <summary>
        /// 创建选择器UI
        /// </summary>
        /// <param name="self"></param>
        /// <param name="rightSide"></param>
        internal void CreateChooserUI(BaseScript self, bool rightSide = false)
        {
            if (_ChooserUI != null)
                return;

            _ChooserUI = new GroupUI(self);

            // 音频选择
            _chooser = self.SetupStringChooser("Music", noneStrings, 600f, rightSide);
            _chooser.setCallbackFunction = v =>
            {
                LoadAudio(v);
                SetAudioPath(v);
            };

            _ChooserUI.Elements.Add(_chooser);
        }

        /// <summary>
        /// 设置音频路径
        /// </summary>
        /// <param name="path"></param>
        void SetAudioPath(string path)
        {
            if (_AudioSetting == null)
            {
                LogUtil.LogWarning($"_AudioSetting:::AudioPlayerHelper Is Not Inited.");
                return;
            }

            if (path == noneString)
            {
                path = null;
            }

            // 音频选中的处理
            _AudioSetting.AudioPath = path;
        }

        /// <summary>
        /// 重置音频选择器
        /// </summary>
        void ResetChooser()
        {
            SetChooser(noneStrings, noneStrings, noneString);
        }

        /// <summary>
        /// 设置音频选择器
        /// </summary>
        /// <param name="displayChoices"></param>
        /// <param name="choices"></param>
        /// <param name="choice"></param>
        void SetChooser(List<string> displayChoices, List<string> choices, string choice)
        {
            _chooser.choices = choices.ToArray().ToList();
            _chooser.displayChoices = displayChoices.ToArray().ToList();

            if (string.IsNullOrEmpty(choice))
            {
                choice = _chooser.defaultVal;
            }

            _chooser.valNoCallback = choice;

            LoadAudio(choice);
        }
    }
}
