using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace mmd2timeline
{
    internal partial class Config
    {
        public static string saveDataPath = "Saves\\PluginData\\mmd2timeline";

        public static string varPmxPath = null;//"sfishere.mmd2timeline.10:/Custom/Scripts/mmd2timeline/g2f.pmx";
        public static string pmxPath = "Custom/Scripts/mmd2timeline/g2f.pmx";

        public static bool s_Debug = false;
        public static bool s_OnlyFace = false;

        public static bool EnableHeelAjust = false;
    }
}
