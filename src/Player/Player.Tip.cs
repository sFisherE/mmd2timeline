using UnityEngine;

namespace mmd2timeline
{
    internal partial class Player
    {
        /// <summary>
        /// 显示提示条
        /// </summary>
        /// <param name="message"></param>
        /// <param name="height"></param>
        /// <param name="alignment"></param>
        /// <param name="action"></param>
        internal override void ShowTip(string message = null, float height = 620, /*float step = 0, float? total = null, Action action = null, string buttonText = null,*/ TextAnchor alignment = TextAnchor.MiddleCenter)
        {
            base.ShowTip(message, height, /*step, total, action, buttonText, */alignment);

            // 隐藏编辑UI
            ShowEditUIs(false);
            // 隐藏MMDUI
            ShowMMDUIs(false);
            // 隐藏播放UI
            ShowPlayUIs(false);
            // 隐藏加载UI
            ShowLoadUIs(false);
            // 隐藏调试UI
            ShowDebugUIs(false);
            // 隐藏加载文件和目录的按钮
            ShowLoadButtons(false);
            _LoadTips.gameObject.SetActive(false);

            CurrentUIMode = PlayerUIMode.Load;
        }

        /// <summary>
        /// 显示导入进度
        /// </summary>
        /// <param name="info"></param>
        /// <param name="step"></param>
        /// <param name="total"></param>
        void ShowImportProgress(string info, int step, int total)
        {
            if (step < total)
            {
                var msg = "\n" +
                $"<size=35><b>{Lang.Get("Importing")}... {step + 1}/{total}</b></size>" +
                $"\n" +
                $"{info}" +
                $"\n";

                this.ShowTip(message: msg/*, step: step, total: total*/);
            }
            else
            {
                var msg = "\n" +
                $"<size=35><b>{Lang.Get("Import Completed")}</b></size>" +
                $"\n" +
                $"\n" +
                $"{info}" +
                $"\n" +
                $"\n" +
                $"{total} {Lang.Get("MMDs Imported.")}" +
                //$"\n" +
                //$"\n" +
                //$"{Lang.Get("Press OK button to Start the Show.")}" +
                $"\n" +
                $"\n";

                this.ShowTip(message: msg);
            }
        }
    }
}
