using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SimpleJSON;

namespace mmd2timeline
{
    [System.Serializable]
    public class TimelineJson
    {
        public List<TimelineClipJson> Clips;
        public string AtomType = "Person";

        public JSONClass GetJSONClass()
        {
            var jc = new JSONClass
            {
                ["AtomType"] = AtomType,
            };
            var clips = new JSONArray();
            foreach (var clip in Clips) clips.Add(clip.GetJSONClass());
            jc.Add("Clips", clips);
            return jc;
        }
    }

    [System.Serializable]
    public class TimelineClipJson
    {
        public string AnimationName;
        public string AnimationLength;
        public string BlendDuration = "1";
        public string Loop = "1";
        public string NextAnimationRandomizeWeight = "1";
        public string AutoTransitionPrevious = "0";
        public string AutoTransitionNext = "0";
        public string SyncTransitionTime = "1";
        public string SyncTransitionTimeNL = "0";
        public string EnsureQuaternionContinuity = "1";
        public string AnimationLayer = "Main";
        public string Speed = "1";
        public string Weight = "1";
        public string Uninterruptible = "0";
        public string AnimationSegment = "Segment 1";
        public List<TimelineControlJson> Controllers;
        public List<FloatParamsJson> FloatParams;

        public JSONClass GetJSONClass()
        {
            var clipJSON = new JSONClass
            {
                { "AnimationName", AnimationName },
                { "AnimationLength", AnimationLength },
                { "BlendDuration", BlendDuration },
                { "Loop", Loop },
                { "NextAnimationRandomizeWeight", NextAnimationRandomizeWeight },
                { "AutoTransitionPrevious", AutoTransitionPrevious },
                { "AutoTransitionNext", AutoTransitionNext },
                { "SyncTransitionTime", SyncTransitionTime },
                { "SyncTransitionTimeNL", SyncTransitionTimeNL },
                { "EnsureQuaternionContinuity", EnsureQuaternionContinuity },
                { "AnimationLayer", AnimationLayer },
                { "Speed", Speed },
                { "Weight", Weight },
                { "Uninterruptible", Uninterruptible },
                { "AnimationSegment", AnimationSegment },
            };

            var controllers = new JSONArray();
            foreach (var clip in Controllers) controllers.Add(clip.GetJSONClass());
            clipJSON.Add("Controllers", controllers);

            var floatParams = new JSONArray();
            foreach (var clip in FloatParams) floatParams.Add(clip.GetJSONClass());
            clipJSON.Add("FloatParams", floatParams);
            return clipJSON;
        }

    }

    [System.Serializable]
    public class FloatParamsJson
    {
        public string Storable = "geometry";
        public string Name;
        public List<TimelineFrameJson> Value = new List<TimelineFrameJson>();
        public string Min = "0";
        public string Max = "1";

        public string Atom = null;

        [NonSerialized]
        public Dictionary<int, TimelineFrameJson> ValueLookup = new Dictionary<int, TimelineFrameJson>();

        public JSONClass GetJSONClass()
        {
            var clipJSON = new JSONClass
            {
                { "Storable", Storable },
                { "Name", Name },
                { "Min", Min },
                { "Max", Max },
            };
            if (!string.IsNullOrEmpty(Atom))
            {
                clipJSON.Add("Atom", Atom);
            }
            var val = new JSONArray();
            foreach (var frame in Value) val.Add(frame.GetJSONClass());
            clipJSON.Add("Value", val);
            return clipJSON;
        }
    }

    [System.Serializable]
    public class TimelineControlJson
    {
        public string Controller;
        public string ControlPosition = "1";
        public string ControlRotation = "1";
        public List<TimelineFrameJson> X = new List<TimelineFrameJson>();
        public List<TimelineFrameJson> Y = new List<TimelineFrameJson>();
        public List<TimelineFrameJson> Z = new List<TimelineFrameJson>();
        public List<TimelineFrameJson> RotX = new List<TimelineFrameJson>();
        public List<TimelineFrameJson> RotY = new List<TimelineFrameJson>();
        public List<TimelineFrameJson> RotZ = new List<TimelineFrameJson>();
        public List<TimelineFrameJson> RotW = new List<TimelineFrameJson>();

        public JSONClass GetJSONClass()
        {
            var clipJSON = new JSONClass
            {
                { "Controller", Controller },
                { "ControlPosition", ControlPosition },
                { "ControlRotation", ControlRotation },
            };
            var x = new JSONArray();
            foreach (var frame in X) x.Add(frame.GetJSONClass());
            clipJSON.Add("X", x);

            var y = new JSONArray();
            foreach (var frame in Y) y.Add(frame.GetJSONClass());
            clipJSON.Add("Y", y);

            var z = new JSONArray();
            foreach (var frame in Z) z.Add(frame.GetJSONClass());
            clipJSON.Add("Z", z);

            var rotX = new JSONArray();
            foreach (var frame in RotX) rotX.Add(frame.GetJSONClass());
            clipJSON.Add("RotX", rotX);
            var rotY = new JSONArray();
            foreach (var frame in RotY) rotY.Add(frame.GetJSONClass());
            clipJSON.Add("RotY", rotY);
            var rotZ = new JSONArray();
            foreach (var frame in RotZ) rotZ.Add(frame.GetJSONClass());
            clipJSON.Add("RotZ", rotZ);
            var rotW = new JSONArray();
            foreach (var frame in RotW) rotW.Add(frame.GetJSONClass());
            clipJSON.Add("RotW", rotW);

            return clipJSON;
        }
    }

    [System.Serializable]
    public class TimelineFrameJson
    {
        public string t;
        public string v;
        public string c;
        //public string i;
        //public string o;
        [NonSerialized]
        public int frame;
        [NonSerialized]
        public float value;
        public TimelineFrameJson(float _t, float _v, string _c)
        {
            t = _t.ToString();
            v = _v.ToString();
            c = _c;
        }

        public JSONClass GetJSONClass()
        {
            var clipJSON = new JSONClass
            {
                { "t", t },
                { "v", v },
                { "c", c },
            };
            return clipJSON;
        }
    }


}
