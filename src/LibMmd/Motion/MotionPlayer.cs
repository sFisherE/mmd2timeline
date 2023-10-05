using System;
using System.Collections.Generic;
using LibMMD.Model;
using LibMMD.Util;
using UnityEngine;

namespace LibMMD.Motion
{
    public class MotionPlayer
    {
        public MotionPlayer(MmdMotion motion, Poser poser)
        {
            _motion = motion;
            _poser = poser;
            var model = poser.Model;
            for (var i = 0; i < model.Bones.Length; ++i)
            {
                var name = model.Bones[i].Name;
                if (motion.IsBoneRegistered(name))
                {
                    _boneMap.Add(new KeyValuePair<string, int>(name, i));
                }
            }
            //Debug.Log("Morphs " + model.Morphs.Length);
            for (var i = 0; i < model.Morphs.Length; ++i)
            {
                var name = model.Morphs[i].Name;
                if (motion.IsMorphRegistered(name))
                {
                    _morphMap.Add(new KeyValuePair<string, int>(name, i));
                }
            }
        }

        public void SeekFrame(int frame)
        {
            foreach (var entry in _morphMap)
            {
                _poser.SetMorphPose(entry.Value, _motion.GetMorphPose(entry.Key, frame));
            }
            foreach (var entry in _boneMap)
            {
                _poser.SetBonePose(entry.Value, _motion.GetBonePose(entry.Key, frame));
            }
        }

        public void SeekTime(double time, float motionScale = 1f)
        {
            var frame = (int)(time * 30f);

            foreach (var entry in _morphMap)
            {
                _poser.SetMorphPose(entry.Value, _motion.GetMorphPose(entry.Key, frame));
            }
            foreach (var entry in _boneMap)
            {
                //poser中不是所有bone都有设置动画的，比如有些动画没有ik，有些ik帧只有一帧的
                _poser.SetBonePose(entry.Value, _motion.GetBonePose(entry.Key, frame), motionScale);
            }
        }
        private static void UpdateVertexOffsetByMorph(MmdModel model, int index, float rate, Vector3[] output)
        {
            if (rate < Tools.MmdMathConstEps)
            {
                return;
            }
            var morph = model.Morphs[index];
            switch (morph.Type)
            {
                case Morph.MorphTypeGroup:
                    foreach (var morphData in morph.MorphDatas)
                    {
                        var data = (Morph.GroupMorph)morphData;
                        UpdateVertexOffsetByMorph(model, data.MorphIndex, data.MorphRate * rate, output);
                    }
                    break;
                case Morph.MorphTypeVertex:
                    foreach (var morphData in morph.MorphDatas)
                    {
                        var data = (Morph.VertexMorph)morphData;
                        output[data.VertexIndex] = output[data.VertexIndex] + data.Offset * rate;
                    }
                    break;
            }
        }

        public double GetMotionTimeLength()
        {
            return _motion.Length * 30.0;
        }

        public int GetMotionFrameLength()
        {
            return _motion.Length;
        }

        private readonly List<KeyValuePair<string, int>> _boneMap = new List<KeyValuePair<string, int>>();
        private readonly List<KeyValuePair<string, int>> _morphMap = new List<KeyValuePair<string, int>>();

        private readonly MmdMotion _motion;
        private readonly Poser _poser;
    }
}