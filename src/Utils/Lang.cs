using MVR.FileManagementSecure;
using System;
using UnityEngine;

namespace mmd2timeline
{
    /// <summary>
    /// 语言处理类
    /// </summary>
    internal class Lang : MSJSONClass
    {
        private static string _PluginPath = "";

        /// <summary>
        /// 初始化语言处理组件，使插件可以读取语言配置文件
        /// </summary>
        /// <param name="pluginPath"></param>
        public static void Init(string pluginPath)
        {
            _PluginPath = pluginPath;
        }

        /// <summary>
        /// 获取指定字符串的翻译
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public static string Get(string key)
        {
            return GetInstance().GetValue(key);
        }

        /// <summary>
        /// 获取指定翻译的原始字符串
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static string From(string value)
        {
            return GetInstance().FromKey(value);
        }

        /// <summary>
        /// 生成语言配置文件
        /// </summary>
        public static void GenerateProfile()
        {
            GetInstance().GenerateLangProfile();
        }

        /// <summary>
        /// 获取语言参数
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        private string GetValue(string key)
        {
            if (this.HasKey(key))
            {
                return this[key];
            }
            else
            {
                this[key] = key;

                return key;
            }
        }

        /// <summary>
        /// 获取原始语言
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        private string FromKey(string value)
        {
            foreach (var key in this.Keys)
            {
                var v = this[key].Value;

                if (value == v)
                {
                    return key;
                }
            }

            return value;
        }

        /// <summary>
        /// 语言处理对象
        /// </summary>
        private Lang()
        {

        }

        /// <summary>
        /// 生成语言配置文件
        /// </summary>
        private void GenerateLangProfile()
        {
            this.Save(LangFilePath);

            // 保存完成后打开保存目录
            SuperController.singleton.OpenFolderInExplorer(Config.saveDataPath);
        }

        /// <summary>
        /// 加载语言配置文件
        /// </summary>
        private void LoadProfile()
        {
            // 如果文件存在，则加载语言配置文件
            if (FileManagerSecure.FileExists(LangFilePath))
            {
                this.Load(LangFilePath);
            }
        }

        /// <summary>
        /// 根据系统语言加载语言字典
        /// </summary>
        private void LoadByLanguage()
        {
            var path = Config.saveDataPath;

            if (!string.IsNullOrEmpty(_PluginPath))
            {
                path = _PluginPath;
            }

            var fileName = $"{Application.systemLanguage}";

            switch (Application.systemLanguage)
            {
                case SystemLanguage.Chinese:
                case SystemLanguage.ChineseSimplified:
                case SystemLanguage.ChineseTraditional:
                    fileName = "Chinese";
                    break;
                default:
                    break;
            }

            var langFile = path + $"/Lang/{fileName}.json";

            if (!FileManagerSecure.FileExists(langFile))
            {
                langFile = path + $"/{fileName}.json";
            }

            if (FileManagerSecure.FileExists(langFile))
            {
                try
                {
                    this.Load(langFile);
                }
                catch (Exception ex)
                {
                    LogUtil.LogError(ex);
                }
            }
            else if (fileName.Equals("Chinese"))
            {
                LoadChinese();
            }
        }

        private static string LangFilePath
        {
            get
            {
                return Config.saveDataPath + "\\lang.json";
            }
        }
        private static Lang _instance;
        private static object _lock = new object();

        /// <summary>
        /// 语言处理类的单例
        /// </summary>
        private static Lang GetInstance()
        {
            lock (_lock)
            {
                if (_instance == null)
                {
                    _instance = new Lang();

                    _instance.LoadByLanguage();

                    _instance.LoadProfile();
                }

                if (string.IsNullOrEmpty(_PluginPath))
                {
                    LogUtil.LogError($"The language module is not initialized and the multi-language support feature does not work.");
                }

                return _instance;
            }
        }

        protected override void BeforeToString()
        {

        }

        /// <summary>
        /// 加载中文
        /// </summary>
        void LoadChinese()
        {
            this["is Ready.\nPlease select the Content to play in the Player."] = "准备完毕\n请在Player中选择要播放的内容。";
            this["<color=#606060><size=40><b>Capturer</b></size>\nBased on MacGruber's SuperShot v13\nSave sequence screenshots for generating video files.</color>"] = "<color=#606060><size=40><b>Capturer</b></size>\n基于 MacGruber's SuperShot v13\n保存序列截图以生成视频文件。</color>";
            this["Depending on the settings, this plugin can be <b>extremely demanding</b> in terms of graphics memory! Reduce the resolution multiplier and/or MSAA when you notice trouble like incomplete images or low performance. \n\nMemory is only allocated for the moment when taking a screenshot and released immediately afterwards."] = "根据不同设置，此插件对显存的要求可能 <b>极高</b>！当发现图像不完整或性能低下等问题时，请降低分辨率乘数和/或 多重采样抗锯齿数值。 \n\n内存只在截图时分配，截图后立即释放。";
            this["Capture Records"] = "采集记录";
            this["Capture Record Info"] = "采集记录信息";
            this["Begin Frame"] = "起始帧";
            this["Start Capture"] = "开始采集";
            this["Recapture"] = "重新采集";
            this["Continue Capture"] = "继续采集";
            this["Render FPS"] = "渲染FPS";
            this["Aspect Ratio"] = "宽高比";
            this["Resolution Output"] = "输出分辨率";
            this["Portait Mode (Flip AspectRatio)"] = "竖屏模式（翻转纵横比）";
            this["Resolution Multiplier"] = "分辨率倍数";
            this["Downscale Method"] = "缩小比例方法";
            this["MSAA Override"] = "多重采样抗锯齿覆盖";
            this["Image Format"] = "图像格式";
            this["<b>General Options</b>"] = "<b>常规选项</b>";
            this["Preview UI scale"] = "预览 UI 比例";
            this["Lock Z Rotation"] = "锁定 Z 轴旋转";
            this["Flashlight"] = "闪光灯";
            this["Setting"] = "设置";
            this["Add New"] = "添加新的";
            this["Remove Current"] = "移除当前的";
            this["Set To Default"] = "设为默认";
            this["Person"] = "人物";
            this["Add Person"] = "添加人物";
            this["Remove Person"] = "移除人物";
            this["Play"] = "播放";
            this["Pause"] = "暂停";
            this["Prev"] = "上一个";
            this["Next"] = "下一个";
            this["Remove from Playlist"] = "从播放列表移除";
            this["Save Settings"] = "保存设置";
            this["Duplicate Files Check"] = "重复文件检查";
            this["Auto Check"] = "自动检查";
            this["Stop Playing When Found"] = "发现重复文件停止播放";
            this["Checking Progress"] = "检查进度";
            this["Camera Motion"] = "镜头动作";
            this["Progress"] = "进度";
            this["Delay"] = "延迟";
            this["Sample Mode"] = "采样模式";
            this["EveryFrame"] = "所有帧";
            this["KeyFrame"] = "关键帧";
            this["Camera Scale"] = "镜头缩放";
            this["Enabled Position Control"] = "开启位置控制";
            this["Enabled Rotation Control"] = "开启方向控制";
            this["Focus On ..."] = "聚焦...";
            this["Position X"] = "位置 X";
            this["Position Y"] = "位置 Y";
            this["Position Z"] = "位置 Z";
            this["Rotation X"] = "方向 X";
            this["Rotation Y"] = "方向 Y";
            this["Rotation Z"] = "方向 Z";
            this["Position Offset X"] = "位置偏移 X";
            this["Position Offset Y"] = "位置偏移 Y";
            this["Position Offset Z"] = "位置偏移 Z";
            this["Rotation Offset X"] = "方向偏移 X";
            this["Rotation Offset Y"] = "方向偏移 Y";
            this["Rotation Offset Z"] = "方向偏移 Z";
            this["Target Atom"] = "目标原子";
            this["Target Receiver"] = "目标位置";
            this["Start Time"] = "开始时间";
            this["End Time"] = "结束时间";
            this["Music Path:"] = "音频路径";
            this["Music"] = "音频";
            this["Load Playlist"] = "加载播放列表";
            this["Import From Folder"] = "从文件夹载入";
            this["Favorite"] = "收藏";
            this["UnFavorite"] = "取消收藏";
            this["Load Folder"] = "加载文件夹";
            this["Load File"] = "加载文件";
            this["Play Mode"] = "播放模式";
            this["Sync Mode"] = "同步模式";
            this["SyncWithGame"] = "与游戏帧同步";
            this["SyncWithAudio"] = "与音频同步";
            this["Playlist"] = "播放列表";
            this["Load Favorite"] = "加载收藏";
            this["Load All"] = "加载所有";
            this["Clear All"] = "清空所有";
            this["Export Playlist"] = "导出播放列表";
            this["<color=#000><size=24>Click the buttons above to prepare to start your Dance!</size></color>\n\n<b>Load Playlist</b> - Import the playlist settings from a saved file.\n\n<b>Import From Folder</b> - Load multiple MMDs to the playlist. You should select the upper directory where the MMD files are stored. \n\n<b>Load Folder</b> - Load an MMD into temporary memory and You can test or adjust its play settings.If it works well, you can add it to the playlist.\n\n<b>Load File(vmd/wav/mp3/ogg)</b> - Load a file to the current MMD.\n\n<b>Load Favorites</b> - Load your favorite MMDs to the current Playlist.\n\n<b>Load All</b> - Load All MMDs to the current Playlist."]
                = "<color=#000><size=24>点击上面的按钮，准备开始表演！</size></color>\n\n<b>加载播放列表</b> - 从保存的文件中导入播放列表设置。\n\n<b>从文件夹载入</b> - 将多个 MMD 载入播放列表。请选择存储 MMD 文件的上层目录。 \n\n<b>加载文件夹</b> - 将 MMD 文件夹载入临时存储器，您可以测试或调整其播放设置，设置好后可以将其添加到播放列表中。\n\n<b>加载文件</b> - 加载一个文件到当前 MMD。\n\n<b>加载收藏</b> - 将您收藏的 MMD 载入到当前播放列表。\n\n<b>加载所有</b> - 将所有 MMD 载入当前播放列表。";
            this["Show Camera Settings"] = "显示镜头设置";
            this["Play Speed"] = "播放速度";
            //this["\n\nhttps://space.bilibili.com/1016760606\n\n<b>Follow the page get latest versions.</b>\n\n<size=24><b>IMPORTANT!!!</b></size>\n<size=24>The MMD Folder Must In VAM Path.If Your MMD Folder In Other Path, Use The CMD Command 'mklink' Link it to VAM Path.</size>\n"]
            //    = "\n\nhttps://space.bilibili.com/1016760606\n\n<b>关注以上页面获取最新版本</b>\n\n<size=24><b>重要！！！</b></size>\n<size=24>MMD 文件夹必须位于 VAM 路径下。如果您的 MMD 文件夹位于其他路径下，请使用 CMD 命令 \"mklink \"将其链接到 VAM 目录中。</size>\n";
            this["IMPORTANT!!!"] = "注意";
            this["The MMD Folder Must In VAM Path.If Your MMD Folder In Other Path, Use The CMD Command 'mklink' Link it to VAM Path."] = "MMD 文件夹必须位于 VAM 路径下。如果您的 MMD 文件夹位于其他路径下，请使用 CMD 命令 \"mklink \"将其链接到 VAM 目录中。";
            this["This is a free open source VAM plugin. \nThe Source Code is licensed under the GPL-3.0 license."] = "这是一款免费开源的 VAM 插件\n源代码采用 GPL-3.0 许可";
            this["<b>Follow the address below to get the latest code or progress information.</b>\nhttps://github.com/sFisherE/mmd2timeline"] = "<b>关注以下地址获取最新代码或进展信息</b>\nhttps://github.com/sFisherE/mmd2timeline\nhttps://space.bilibili.com/1016760606";
            this["Show Debug Info"] = "显示调试信息";
            this["Motion Scale"] = "动作缩放";
            this["Sample Speed"] = "采样速度";
            this["Use All Joints Settings"] = "开启所有关节设置";
            this["All Joints Spring Percent"] = "所有关节弹簧倍数";
            this["All Joints Damper Percent"] = "所有关节阻尼倍数";
            this["All Joints Max Velocity"] = "所有关节最大速度";
            this["Use Joints Settings"] = "开启关节设置";
            this["Joints Spring Percent"] = "关节弹簧倍数";
            this["Joints Damper Percent"] = "关节阻尼倍数";
            this["Joints Max Velocity"] = "关节最大速度";
            this["Enable Face"] = "启用表情";
            this["Show Motion Settings"] = "显示动作设置";
            this["General Settings"] = "通用设置";
            this["Play Volume"] = "播放音量";
            this["Lock Person Position"] = "锁定人物位置";
            this["Motion Position State"] = "动作关节位置设定";
            this["Motion Rotation State"] = "动作关节方向设定";
            this["Physics Mesh Settings"] = "柔性物理网格设置";
            this["Enable Mouth Physics Mesh"] = "启用嘴部柔性物理";
            this["Enable Breast Physics Mesh"] = "启用胸部柔性物理";
            this["Enable Lower Physics Mesh"] = "启用下身柔性物理";
            this["Auto-Correct Height Settings"] = "自动修正高度设置";
            this["Auto-Correct Mode"] = "修正模式";
            this["Fix Height"] = "修正高度";
            this["Floor Height"] = "地板高度";
            this["Enable Foot Free"] = "启用脚部放松";
            this["Enable Foot Off"] = "允许脚部关闭控制";
            this["Enable Knee Off"] = "允许膝盖关闭控制";
            this["Toe Off Height"] = "脚趾关闭高度";
            this["Foot Fix Height"] = "脚部修正高度";
            this["Foot Off Height"] = "脚部关闭高度";
            this["Free Foot Joint Drive X Target"] = "放松脚部关节驱动 X 目标";
            this["Free Foot Hold Rotation Max Force"] = "放松脚部保持旋转最大力";
            this["Free Knee Hold Rotation Max Force"] = "放松膝盖保持旋转最大力";
            this["Free Thigh Hold Rotation Max Force"] = "放松大腿保持旋转最大力";
            this["Camera Settings"] = "镜头设置";
            this["Camera Enabled Non-VR"] = "开启镜头（非VR）";
            this["Camera Enabled in VR"] = "开启镜头（VR）";
            this["Camera Position Smoothing"] = "镜头位置平滑";
            this["Camera Rotation Smoothing"] = "镜头角度平滑";
            this["Deactive when MainHUD Opened"] = "主界面打开时停止镜头";
            this["Only Keyframes"] = "镜头播放只使用关键帧";
            this["Play in WindowCamera"] = "在WindowCamera中播放";
            this["<color=#FF0000><b>Plugin VRAM usage: ~"] = "<color=#FF0000><b>插件 VRAM 使用量: ~";
            this[" GB.</b></color>"] = " GB.</b></color>";
            this["Importing"] = "导入中";
            this["Position"] = "位置";
            this["Close Lower Bones"] = "关闭下身骨骼";
            this["File Type"] = "文件类型";
            this["Length"] = "长度";
            this["Delete"] = "删除";
            this["Remove"] = "移除";
            this["Generate Language Profile"] = "生成语言配置文件";
            this["If you need to set your own language, press the \"Generate Language Profile\" button, it will generate the Saves\\PluginData\\mmd2timeline\\lang.json file, you can modify the content in it to get your own language profiles."]
                = "如果你需要自己来设定语言，按下“生成语言配置文件”按钮，将会生成Saves\\PluginData\\mmd2timeline\\lang.json文件，修改里头的内容来获得你自己的语言配置文件。";
            this["Exit Edit Mode"] = "退出编辑模式";
            this["Global Motion Scale"] = "全局动作缩放";
            this["Enable High Heel"] = "启用高跟";
            this["Foot Hold Rotation Max Force"] = "脚部保持最大力";
            this["Foot Joint Drive X Angle"] = "脚部关节X角度";
            this["Toe Joint Drive X Angle"] = "脚趾关节X角度";
            this["Reset Person Physical When Motion Reset"] = "动作重置时重设人物物理";
            this["Reset All Settings To Default"] = "重置所有设置到默认值";
            this["Press ESC to Stop Capture."] = "按ESC键停止采集";
            this["Duplicate Files"] = "重复文件";
            this["Select the files you want to keep and click \"Delete Others\" to add the remaining files to PrepareToDeleteFile (You can run "]
                = "选中你要保存的文件，点击 \"删除其他\" 按钮将其余文件添加到待删除文件清单（你可以运行 ";
            this[" to remove them from the disk.) or click \"Remove Others\" to remove them from the settings."] = " 来把它们从硬盘删除。）或点击 \"移除其他\" 按钮来把他们从设置中移除。";
            this["Delete Others"] = "删除其他";
            this["Remove Others"] = "移除其他";
            this["Capture Completed."] = "录制完成。";
            this["Execute"] = "运行";
            this["in"] = "在";
            this["to create your video file"] = "来创建视频文件。";
            this["Ready For Capture.."] = "准备录制..";
            this["Capturing..."] = "录制中...";
            this["Capture Progress:"] = "录制进度：";
            this["Motion Mode"] = "动作模式";
            this["Custom"] = "自定义";
            this["Fatigue"] = "乏力";
            this["Weak"] = "柔弱";
            this["Smooth"] = "轻柔";
            this["Normal"] = "一般";
            this["Strong"] = "强壮";
            this["Dexterity"] = "灵巧";
            this["Agile"] = "敏捷";
            this["On"] = "开";
            this["Off"] = "关";
            this["Following"] = "跟随";
            this["Hold"] = "保持";
            this["Lock"] = "锁定";
            this["ParentLink"] = "父连接";
            this["PhysicsLink"] = "物理连接";
            this["Comply"] = "遵守";
            this["LookAt"] = "看向";
            this["None"] = "无";
            this["PartOnly"] = "仅超范围部位";
            this["WholeBody"] = "全身";
            this["Default"] = "默认";
            this["Random"] = "随机";
            this["Repeat"] = "重复";
            this["Once"] = "单次";
            this["Music"] = "音频";
            this["Camera"] = "镜头";
            this["Motion"] = "动作";
            this["Expression"] = "表情";
            this["VMD"] = "VMD";
            this["Other"] = "其他";
            this["Reset Motion Model\n(Fix motion issues with bone changes)"] = "重置动作模型\n（修复因骨骼变动引起的动作异常）";
            this["Import Completed"] = "导入完成";
            this["MMDs Imported."] = "MMD导入完成";
            this["Press OK button to Start the Show."] = "按下OK按钮开始表演";
            this["OK"] = "OK";
            this["Language Settings"] = "语言设置";
            this["Enable Space to Toggle Play/Pause"] = "启用<空格键>切换播放/暂停";
            this["Enable Right Arrow to Forward 1s"] = "启用<右箭头>前进1秒";
            this["Enable Left Arrow to Back 1s"] = "启用<左箭头>后退1秒";
            this["Enable Up Arrow to Play Previous"] = "启用<上箭头>切换上一首";
            this["Enable Down Arrow to Play Next"] = "启用<下箭头>切换下一首";
            this["U - Toggle UI Hidden/Visible"] = "U键 - 切换UI显示/隐藏";
            this["M - Toggle WindowCamera View"] = "M键 - 切换窗口相机视图";
            this["Shortcut Keys"] = "快捷键";
            this["Max Length"] = "播放时长";
            this["Audio Delay"] = "音频延迟";
            this["Motion Delay"] = "动作延迟";
            this["FOV Enabled"] = "开启FOV";
            this["Heel Height Fixing"] = "高跟身体高度修正";
            this["Motion Correction Setting"] = "动作修正设置";
            this["Enable Initial Motion Adjustment"] = "启用初始动作调整";
            this["Enable Kneeing Corrections"] = "启用跪姿矫正";
            this["Height Correction Mode"] = "高度修正模式";
            this["Bone Rotation Adjust"] = "骨骼旋转调节";
            this["Shoulder"] = "肩";
            this["Arm"] = "臂";
            this["Elbow"] = "肘";
            this["Hand"] = "手";
            this["Thigh"] = "胯";
            this["Knee"] = "膝";
            this["Foot"] = "足";
            this["Toe"] = "趾";
            this["Adjust Bone Rotation X"] = "调节骨骼旋转 X";
            this["Adjust Bone Rotation Y"] = "调节骨骼旋转 Y";
            this["Adjust Bone Rotation Z"] = "调节骨骼旋转 Z";
            this["Clear Bone Rotation Adjust"] = "清除骨骼旋转调节";
            this["Camera Control Mode"] = "镜头\n控制模式";
            this["Original"] = "原始";
            this["Camera Progress"] = "镜头进度";
            this["Motion Progress"] = "动作进度";
            this["Camera Delay"] = "镜头延迟";
            this["Motion Delay"] = "动作延迟";
            this["Force Disable IK"] = "强制关闭IK";
            this["Motion Settings"] = "动作设置";
            this["Reset Model Before Motion Start"] = "动作开始前重置动作模型";
            this["Generic Motion Scale"] = "全局动作缩放";

            this["Select trigger on the left side"] = "从左侧列表中选择触发器进行设置";
            this["Actions"] = "动作";
            this["Add New Action"] = "添加动作";
            this["Remove Action"] = "移除动作";
            this["Up"] = "往上";
            this["Down"] = "向下";
            this["Atom"] = "原子";
            this["Receiver"] = "接收器";
            this["Target"] = "目标";
            this["Value Source"] = "值来源";
            this["From Plugin"] = "来自插件";
            this["Bool Value"] = "布尔值";
            this["String Chooser Value"] = "选择器值";
            this["Float Value"] = "浮点值";
            this["String Value"] = "字符串值";
            this["Test Action"] = "测试动作";
            this["off"] = "关";
            this["Script Loaded Trigger"] = "插件加载完成触发器";
            this["Start Playing Trigger"] = "开始播放触发器";
            this["Play Next Trigger"] = "播放下一个触发器";
            this["Is End Trigger"] = "播放结束触发器";
            this["Favorited Trigger"] = "已收藏触发器";
            this["Not Favorite Trigger"] = "未收藏触发器";
            this["In Init Mode Trigger"] = "进入等待模式触发器";
            this["In Play Mode Trigger"] = "进入播放模式触发器";
            this["In Edit Mode Trigger"] = "进入编辑模式触发器";
            this["In Load Mode Trigger"] = "进入加载模式触发器";
            this["Camera Activated Trigger"] = "镜头已启用触发器";
            this["Camera Deactivated Trigger"] = "镜头未启用触发器";
            this["Progress Trigger"] = "播放进度触发器";
            this["Playing Status Trigger"] = "播放状态触发器";
            this["Play Mode Changed Trigger"] = "播放模式改变触发器";
            this["Motion 1"] = "动作 1";
            this["Motion 2"] = "动作 2";
            this["Motion 3"] = "动作 3";
            this["Motion 4"] = "动作 4";
            this["Settings"] = "设置";
            this["Action"] = "动作";
        }
    }
}
