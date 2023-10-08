using System;
using System.Collections.Generic;
using LibMMD.Model;
using LibMMD.Util;
using UnityEngine;

namespace LibMMD.Motion
{
    public class MotionPlayer
    {
        private Dictionary<string, int> m_boneNameTable;
        private Dictionary<int, string> m_boneNameTable2;

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

            m_boneNameTable = new Dictionary<string, int>();
            m_boneNameTable2 = new Dictionary<int, string>();
            int j = 0;
            foreach(var item in _motion.BoneMotions)
            {
                m_boneNameTable.Add(item.Key, j);
                m_boneNameTable2.Add(j, item.Key);
                j++;
            }

            VisibleIKKeys = new VmdVisibleIKKeyList();

            VmdVisibleIKKey vmdVisibleIKKey = new VmdVisibleIKKey();
            vmdVisibleIKKey.FrameIndex = 0;
            vmdVisibleIKKey.Visible = true;

            BoneImage[] array = _poser.EnumIKBone();
            vmdVisibleIKKey.IKEnable = new VmdVisibleIKKey.IK[array.Length];
            for (int k = 0; k < array.Length; k++)
            {
                string name = array[k].Name;
                vmdVisibleIKKey.IKEnable[k] = new VmdVisibleIKKey.IK();
                if (motion.IsBoneRegistered(name))
                {
                    vmdVisibleIKKey.IKEnable[k].IKBoneIndex = m_boneNameTable[name];
                    vmdVisibleIKKey.IKEnable[k].Enable = true;
                }
                else
                {
                    vmdVisibleIKKey.IKEnable[k].IKBoneIndex = -1;
                    vmdVisibleIKKey.IKEnable[k].Enable = false;
                }
            }
            //先初始化0帧的ik状态
            VisibleIKKeys.Add(vmdVisibleIKKey);

            var num2 = _motion.VisibleIKList.Count;
            for (int n = 0; n < num2; n++)
            {
                VmdVisibleIK vik = _motion.VisibleIKList[n];
                VmdVisibleIKKey vmdVisibleIKKey2 = VmdVisibleIKKey.FromVmdVisibleIK(vik, m_boneNameTable);
                vmdVisibleIKKey2.FrameIndex *= 30;//不确定这个值是啥
                VmdVisibleIKKeyList visibleIKKeys = VisibleIKKeys;
                int num8 = visibleIKKeys.FindFrameIndex(vmdVisibleIKKey2.FrameIndex);
                if (num8 >= 0)
                {
                    visibleIKKeys.RemoveAt(num8);
                }
                visibleIKKeys.Add(vmdVisibleIKKey2);
                //num3 = Math.Max(num3, vmdVisibleIKKey2.FrameIndex);
            }

        }
        public VmdVisibleIKKeyList VisibleIKKeys;
        public void SeekFrame(int frame, float motionScale)
        {
            foreach (var entry in _morphMap)
            {
                _poser.SetMorphPose(entry.Value, _motion.GetMorphPose(entry.Key, frame));
            }
            UpdateIKEnable(frame);
            foreach (var entry in _boneMap)
            {
                _poser.SetBonePose(entry.Value, _motion.GetBonePose(entry.Key, frame),motionScale);
            }
        }

        public void SeekTime(double time, float motionScale)
        {

            foreach (var entry in _morphMap)
            {
                _poser.SetMorphPose(entry.Value, _motion.GetMorphPose(entry.Key, time));
            }
            var frame = (int)(time * 30f);
            UpdateIKEnable(frame);
            foreach (var entry in _boneMap)
            {
                //poser中不是所有bone都有设置动画的，比如有些动画没有ik，有些ik帧只有一帧的
                _poser.SetBonePose(entry.Value, _motion.GetBonePose(entry.Key, time), motionScale);
            }
        }
        public bool UpdateIKEnable(int frameIndex)
        {
            VmdVisibleIKKey state = VisibleIKKeys.GetState(frameIndex);
            bool result = false;
            for (int i = 0; i < state.IKEnable.Length; i++)
            {
                VmdVisibleIKKey.IK iK = state.IKEnable[i];
                //根据数据设置ik的状态
                var image = _poser.GetBoneImage(m_boneNameTable2[iK.IKBoneIndex]);
                if (image != null && image.IKEnable != iK.Enable)
                {
                    image.IKEnable = iK.Enable;
                    result = true;
                }
            }
            return result;
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