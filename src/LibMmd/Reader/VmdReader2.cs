using System.Collections.Generic;
//using System.IO;
using System.Linq;
using LibMMD.Motion;
using LibMMD.Util;
using UnityEngine;
using MVR.FileManagementSecure;
using mmd2timeline;

namespace LibMMD.Reader
{
    public class VmdReader2
    {
        public MmdMotion Read(string path)
        {
            byte[] bytes = FileManagerSecure.ReadAllBytes(path);
            var binaryReader = new BufferBinaryReader(bytes);
            return Read(binaryReader);
        }

        public MmdMotion Read(BufferBinaryReader reader, bool forCamera = false)
        {
            var motion = new TempMmdMotion();
            try
            {
                var magic = MmdReaderUtil2.ReadStringFixedLength(reader, 30);
                if (!"Vocaloid Motion Data 0002".Equals(magic))
                {
                    throw new MmdFileParseException("File is not a VMD file.");
                }
                motion.Name = MmdReaderUtil2.ReadStringFixedLength(reader, 20);
                Debug.LogWarning("read " + motion.Name);
                var boneMotionNum = reader.ReadInt32();
                for (var i = 0; i < boneMotionNum; ++i)
                {
                    var b = ReadVmdBone(reader);
                    var keyFrame = motion.GetOrCreateBoneKeyFrame(b.BoneName, b.NFrame);
                    keyFrame.Translation = b.Translation;
                    keyFrame.Rotation = b.Rotation;

                    Vector2 c0, c1;
                    const float r = 1.0f / 127.0f;
                    c0.x = b.XInterpolator[0] * r;
                    c0.y = b.XInterpolator[4] * r;
                    c1.x = b.XInterpolator[8] * r;
                    c1.y = b.XInterpolator[12] * r;
                    keyFrame.XInterpolator = new Interpolator();
                    keyFrame.XInterpolator.SetC(c0, c1);

                    c0.x = b.YInterpolator[0] * r;
                    c0.y = b.YInterpolator[4] * r;
                    c1.x = b.YInterpolator[8] * r;
                    c1.y = b.YInterpolator[12] * r;
                    keyFrame.YInterpolator = new Interpolator();
                    keyFrame.YInterpolator.SetC(c0, c1);

                    c0.x = b.ZInterpolator[0] * r;
                    c0.y = b.ZInterpolator[4] * r;
                    c1.x = b.ZInterpolator[8] * r;
                    c1.y = b.ZInterpolator[12] * r;
                    keyFrame.ZInterpolator = new Interpolator();
                    keyFrame.ZInterpolator.SetC(c0, c1);

                    c0.x = b.RInterpolator[0] * r;
                    c0.y = b.RInterpolator[4] * r;
                    c1.x = b.RInterpolator[8] * r;
                    c1.y = b.RInterpolator[12] * r;
                    keyFrame.RInterpolator = new Interpolator();
                    keyFrame.RInterpolator.SetC(c0, c1);
                }

                var morphMotionNum = reader.ReadInt32();
                for (var i = 0; i < morphMotionNum; ++i)
                {
                    var vmdMorph = ReadVmdMorph(reader);
                    var keyFrame = motion.GetOrCreateMorphKeyFrame(vmdMorph.MorphName, vmdMorph.NFrame);
                    keyFrame.Weight = vmdMorph.Weight;
                    keyFrame.WInterpolator = new Interpolator();
                }

                //如果是相机的话，到这里就可以了
                if (forCamera) return null;
                try
                {
                    if (reader.CanRead())
                    {
                        var CameraListCapacity = reader.ReadInt32();
                        for (int k = 0; k < CameraListCapacity; k++)
                        {
                            //VmdCamera
                            var FrameIndex = reader.ReadInt32();
                            var Distance = reader.ReadSingle();
                            var PositionX = reader.ReadSingle();
                            var PositionY = reader.ReadSingle();
                            var PositionZ = reader.ReadSingle();
                            var RotateX = reader.ReadSingle();
                            var RotateY = reader.ReadSingle();
                            var RotateZ = reader.ReadSingle();
                            var VmdCameraIPL = reader.ReadBytes(24);
                            var Angle = reader.ReadSingle();
                        }
                    }
                    if (reader.CanRead())
                    {
                        var LightListCapacity = reader.ReadInt32();
                        for (int m = 0; m < LightListCapacity; m++)
                        {
                            var FrameIndex = reader.ReadInt32();
                            var Red = reader.ReadSingle();
                            var Green = reader.ReadSingle();
                            var Blue = reader.ReadSingle();
                            var DirectionX = reader.ReadSingle();
                            var DirectionY = reader.ReadSingle();
                            var DirectionZ = reader.ReadSingle();
                        }
                    }
                    if (reader.CanRead())
                    {
                        var SelfShadowListCapacity = reader.ReadInt32();
                        for (int n = 0; n < SelfShadowListCapacity; n++)
                        {
                            var FrameIndex = reader.ReadInt32();
                            var Mode = reader.ReadByte();
                            var Distance = reader.ReadSingle();
                        }
                    }
                }
                catch (System.Exception e)
                {
                    Debug.LogError("1.parse vmd error:" + e.ToString());
                }
                var ret = motion.BuildMmdMotion();
                try
                {
                    if (reader.CanRead())
                    {
                        var VisibleIKListCapacity = reader.ReadInt32();
                        for (int num4 = 0; num4 < VisibleIKListCapacity; num4++)
                        {
                            VmdVisibleIK vmdVisibleIK = new VmdVisibleIK();
                            vmdVisibleIK.FrameIndex = reader.ReadInt32();
                            Debug.LogWarning("ik visible frame:" + vmdVisibleIK.FrameIndex);

                            vmdVisibleIK.Visible = reader.ReadByte() != 0;
                            int num2 = reader.ReadInt32();
                            for (int i = 0; i < num2; i++)
                            {
                                VmdVisibleIK.IK iK = new VmdVisibleIK.IK();
                                iK.IKName = MmdReaderUtil2.ReadStringFixedLength(reader, 20);
                                iK.Enable = reader.ReadByte() != 0;
                                Debug.LogWarning("ikname:" + iK.IKName + " " + iK.Enable.ToString());
                                vmdVisibleIK.IKList.Add(iK);
                            }
                            ret.VisibleIKList.Add(vmdVisibleIK);
                        }
                    }
                }
                catch (System.Exception e)
                {
                    Debug.LogError("2.parse vmd error:" + e.ToString());
                }
                return ret;
            }
            finally
            {
                motion.Clear();
            }
        }

        public CameraMotion ReadCameraMotion(string path)
        {
            byte[] bytes = FileManagerSecure.ReadAllBytes(path);
            var binaryReader = new BufferBinaryReader(bytes);
            return ReadCameraMotion(binaryReader);
        }

        public CameraMotion ReadCameraMotion(BufferBinaryReader reader, bool motionReadAlready = false)
        {
            if (!motionReadAlready)
            {
                Read(reader, true);
            }
            var ret = new CameraMotion();
            var cameraMotionNum = reader.ReadInt32();
            Dictionary<int, CameraKeyframe> keyframes = new Dictionary<int, CameraKeyframe>();
            for (var i = 0; i < cameraMotionNum; ++i)
            {

                var nFrame = reader.ReadInt32();
                var focalLength = reader.ReadSingle();
                var position = MmdReaderUtil2.ReadVector3(reader);
                var rotation = MmdReaderUtil2.ReadVector3(reader);
                var interpolator = reader.ReadBytes(24);
                var fov = reader.ReadUInt32();
                var orthographic = reader.ReadByte();
                var keyframe = new CameraKeyframe
                {
                    Fov = fov,
                    FocalLength = focalLength,
                    Orthographic = orthographic != 0,
                    Position = position,
                    Rotation = rotation,
                    Interpolation = interpolator
                };
                keyframes[nFrame] = keyframe;
            }
            var frameList = keyframes.Select(entry => entry).ToList().OrderBy(kv => kv.Key).ToList();
            ret.KeyFrames = frameList;
            keyframes.Clear();
            return ret;
        }

        private VmdBone ReadVmdBone(BufferBinaryReader reader)
        {
            return new VmdBone
            {
                BoneName = MmdReaderUtil2.ReadStringFixedLength(reader, 15),
                NFrame = reader.ReadInt32(),
                Translation = MmdReaderUtil2.ReadVector3(reader),
                Rotation = MmdReaderUtil2.ReadQuaternion(reader),
                XInterpolator = reader.ReadBytes(16),
                YInterpolator = reader.ReadBytes(16),
                ZInterpolator = reader.ReadBytes(16),
                RInterpolator = reader.ReadBytes(16)
            };
        }

        private VmdMorph ReadVmdMorph(BufferBinaryReader reader)
        {
            return new VmdMorph
            {
                MorphName = MmdReaderUtil2.ReadStringFixedLength(reader, 15),
                NFrame = reader.ReadInt32(),
                Weight = reader.ReadSingle()
            };
        }

        private class VmdBone
        {
            public string BoneName { get; set; } //15
            public int NFrame { get; set; }
            public Vector3 Translation { get; set; }
            public Quaternion Rotation { get; set; }
            public byte[] XInterpolator { get; set; } //16
            public byte[] YInterpolator { get; set; } //16
            public byte[] ZInterpolator { get; set; } //16
            public byte[] RInterpolator { get; set; } //16
        }

        private class VmdMorph
        {
            public string MorphName { get; set; } //15
            public int NFrame { get; set; }
            public float Weight { get; set; }
        }

        private class TempMmdMotion
        {
            public string Name { get; set; }

            public int Length { get; private set; }

            public Dictionary<string, Dictionary<int, BoneKeyframe>> BoneMotions { get; set; }
            public Dictionary<string, Dictionary<int, MorphKeyframe>> MorphMotions { get; set; }

            public TempMmdMotion()
            {
                BoneMotions = new Dictionary<string, Dictionary<int, BoneKeyframe>>();
                MorphMotions = new Dictionary<string, Dictionary<int, MorphKeyframe>>();
            }

            public BoneKeyframe GetOrCreateBoneKeyFrame(string boneName, int frame)
            {
                Dictionary<int, BoneKeyframe> framesForBone;
                if (frame > Length)
                {
                    Length = frame;
                }
                if (!BoneMotions.TryGetValue(boneName, out framesForBone))
                {
                    framesForBone = new Dictionary<int, BoneKeyframe>();
                    BoneMotions.Add(boneName, framesForBone);
                }
                BoneKeyframe boneKeyframe;
                if (!framesForBone.TryGetValue(frame, out boneKeyframe))
                {
                    boneKeyframe = new BoneKeyframe();
                    framesForBone.Add(frame, boneKeyframe);
                }
                return boneKeyframe;
            }

            public MorphKeyframe GetOrCreateMorphKeyFrame(string boneName, int frame)
            {
                Dictionary<int, MorphKeyframe> framesForBone;
                if (frame > Length)
                {
                    Length = frame;
                }
                if (!MorphMotions.TryGetValue(boneName, out framesForBone))
                {
                    framesForBone = new Dictionary<int, MorphKeyframe>();
                    MorphMotions.Add(boneName, framesForBone);
                }
                MorphKeyframe morphKeyframe;
                if (!framesForBone.TryGetValue(frame, out morphKeyframe))
                {
                    morphKeyframe = new MorphKeyframe();
                    framesForBone.Add(frame, morphKeyframe);
                }
                return morphKeyframe;
            }

            public MmdMotion BuildMmdMotion()
            {
                var ret = new MmdMotion();
                ret.BoneMotions = new Dictionary<string, List<KeyValuePair<int, BoneKeyframe>>>();
                foreach (var entry in BoneMotions)
                {
                    var value = entry.Value.ToList();
                    value = value.OrderBy(kv => kv.Key).ToList();
                    ret.BoneMotions.Add(entry.Key, value);
                }
                ret.MorphMotions = new Dictionary<string, List<KeyValuePair<int, MorphKeyframe>>>();
                foreach (var entry in MorphMotions)
                {
                    //Debug.Log("MorphMotions " + entry.Key);
                    var value = entry.Value.ToList();
                    value = value.OrderBy(kv => kv.Key).ToList();
                    ret.MorphMotions.Add(entry.Key, value);
                }
                ret.Length = Length;
                ret.Name = Name;
                return ret;
            }

            public void Clear()
            {
                BoneMotions.Clear();
                MorphMotions.Clear();
            }
        }
    }
}