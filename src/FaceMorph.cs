using System.Collections.Generic;

namespace mmd2timeline
{
    class FaceMorph
    {
        public static Dictionary<string, MorphSetting[]> s_Setting;
        public static Dictionary<string, MorphSetting[]> setting
        {
            get
            {
                if(s_Setting==null)
                {
                    InitSetting();
                }
                return s_Setting;
            }
        }
        public class MorphSetting
        {
            public string name;
            public float min;
            public float max;
            public MorphSetting(string p0, float p1 = 0, float p2 = 1)
            {
                name = p0;
                min = p1;
                max = p2;
            }
        }
        public static void InitSetting()
        {
           var setting = new Dictionary<string, MorphSetting[]>();
            s_Setting = setting;
            //眉
            setting.Add("真面目", new MorphSetting[] { new MorphSetting("Brow Down") });
            setting.Add("上", new MorphSetting[] { new MorphSetting("Brow Up") });
            setting.Add("下", new MorphSetting[] { new MorphSetting("Brow Down") });
            setting.Add("眉頭左", new MorphSetting[] {
                new MorphSetting("Brow Up",0,0),
                new MorphSetting("Brow Down",0,1),
                new MorphSetting("Brow Down Left",0,1),
                new MorphSetting("Brow Up Right",0,1)

            });
            setting.Add("眉頭右", new MorphSetting[] {
                new MorphSetting("Brow Up",0,0) ,
                new MorphSetting("Brow Down") ,
                new MorphSetting("Brow Up Left") ,
                new MorphSetting("Brow Down Right") ,
            });
            setting.Add("困る", new MorphSetting[] {
                new MorphSetting("Brow Inner Up"),
                new MorphSetting("Brow Outer Down"),
            });
            setting.Add("にこり", new MorphSetting[] {
                new MorphSetting("Brow Up",0,0.5f) ,
                new MorphSetting("Brow Inner Down",0,0.5f) ,
            });
            setting.Add("怒り", new MorphSetting[] {
                new MorphSetting("Brow Outer Up") ,
                new MorphSetting("Brow Inner Down") ,
            });

            //目
            setting.Add("まばたき", new MorphSetting[] {
                new MorphSetting("Eyes Closed"),
            });
            setting.Add("笑顔", new MorphSetting[] { new MorphSetting("Eyes Squint") });
            setting.Add("笑い", new MorphSetting[] { new MorphSetting("Eyes Squint") });
            setting.Add("ウィンク", new MorphSetting[] {
                new MorphSetting("Eyes Closed",0,0) ,
                new MorphSetting("Eyes Closed Right",0,0.6f) ,

            });
            setting.Add("ウィンク２", new MorphSetting[] {
                new MorphSetting("Eyes Closed",0,0) ,
                new MorphSetting("Eyes Closed Right",0,0.6f) ,
            });
            setting.Add("ウィンク左", new MorphSetting[] {
                new MorphSetting("Eyes Closed",0,0) ,
                new MorphSetting("Eyes Closed Right",0,0.6f) ,
            });
            setting.Add("ウィンク右", new MorphSetting[] {
                new MorphSetting("Eyes Closed",0,0),
                new MorphSetting("Eyes Closed Left",0,0.6f),
            });
            setting.Add("ウィンク１右", new MorphSetting[] {
                new MorphSetting("Eyes Closed",0,0) ,
                new MorphSetting("Eyes Closed Left",0,0.6f) ,
            });
            setting.Add("ｳｨﾝｸ２右", new MorphSetting[] {
                new MorphSetting("Eyes Closed",0,0) ,
                new MorphSetting("Eyes Closed Left",0,0.6f) ,
            });
            setting.Add("ウィンク２右", new MorphSetting[] {
                new MorphSetting("Eyes Closed",0,0),
                new MorphSetting("Eyes Closed Left",0,0.6f),
            });
            setting.Add("びっくり", new MorphSetting[] { new MorphSetting("Eyes Squint", 0, -1) });
            setting.Add("じと目", new MorphSetting[] {
                new MorphSetting("Eyes Closed",0,0.1f),
                new MorphSetting("Eyes Squint",0,-0.1f),
            });
            setting.Add("下瞼上", new MorphSetting[] { new MorphSetting("Eyelids Bottom Up Right") });
            setting.Add("右下瞼上", new MorphSetting[] { new MorphSetting("Eyelids Bottom Up Left") });

            //口
            setting.Add("あ", new MorphSetting[] {
                new MorphSetting("AA"),
                new MorphSetting("Mouth Open",0,0.5f),
            });
            setting.Add("あ２", new MorphSetting[] {
                new MorphSetting("AA"),
                new MorphSetting("Mouth Open",0,0.6f),
            });
            setting.Add("い", new MorphSetting[] { new MorphSetting("IY") });
            setting.Add("う", new MorphSetting[] { new MorphSetting("UW", 0, 0.5f) });
            setting.Add("え", new MorphSetting[] {
                new MorphSetting("EH") ,
                new MorphSetting("Mouth Open",0,0.3f) ,
            });
            setting.Add("お", new MorphSetting[] {
                new MorphSetting("OW",0,0.5f),
                new MorphSetting("Mouth Open",0,0.3f),
            });
            setting.Add("にやり", new MorphSetting[] { new MorphSetting("Mouth Smile Simple") });
            setting.Add("にやり２", new MorphSetting[] { new MorphSetting("Mouth Smile") });
            setting.Add("口横広げ", new MorphSetting[] { new MorphSetting("Mouth Open", 0, -1) });
            setting.Add("ん", new MorphSetting[] {
                new MorphSetting("Mouth Open", 0, -1) ,
                new MorphSetting("Mouth Narrow", 0, 0.5f)
            });

            setting.Add("w", new MorphSetting[] {
                new MorphSetting("Mouth Open",0,-1) ,
                new MorphSetting("Mouth Corner Up-Down") ,
            });
            setting.Add("oms", new MorphSetting[] {
                new MorphSetting("Mouth Open",0,1) ,
                new MorphSetting("Mouth Corner Up-Down"),
            });
            setting.Add("口角上げ", new MorphSetting[] {
                new MorphSetting("Mouth Open",0,-1) ,
                new MorphSetting("Mouth Corner Up-Down") ,
            });
            setting.Add("口角下げ", new MorphSetting[] {
                new MorphSetting("Mouth Open",0,0) ,
                new MorphSetting("Mouth Corner Up-Down")
            });
            setting.Add("∧", new MorphSetting[] {
                new MorphSetting("Mouth Open",0,-1) ,
                new MorphSetting("Mouth Corner Up-Down",0,0),
                new MorphSetting("Mouth Narrow",0,0.8f) ,
            });
            setting.Add("▲", new MorphSetting[] {
                new MorphSetting("Mouth Open",0,0) ,
                new MorphSetting("Mouth Corner Up-Down",0,0),
                new MorphSetting("Mouth Narrow",0,0.8f) ,
            });
        }

    }
}
