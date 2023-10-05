using System.Collections.Generic;
using UnityEngine;

namespace LibMMD.Model
{
    public class Morph
    {
        //MorphCategory
        public const int MorphCatSystem = 0x00;
        public const int MorphCatEyebrow = 0x01;
        public const int MorphCatEye = 0x02;
        public const int MorphCatMouth = 0x03;
        public const int MorphCatOther = 0x04;

        //MorphType
        public const int MorphTypeGroup = 0x00;
        public const int MorphTypeVertex = 0x01;
        public const int MorphTypeBone = 0x02;
        public const int MorphTypeUv = 0x03;
        public const int MorphTypeExtUv1 = 0x04;
        public const int MorphTypeExtUv2 = 0x05;
        public const int MorphTypeExtUv3 = 0x06;
        public const int MorphTypeExtUv4 = 0x07;
        public const int MorphTypeMaterial = 0x08;

        public abstract class MorphData
        {
        }

        public class GroupMorph : MorphData
        {
            public int MorphIndex { get; set; }
            public float MorphRate { get; set; }
        }

        public class VertexMorph : MorphData
        {
            public int VertexIndex { get; set; }
            public Vector3 Offset { get; set; }
        }

        public class BoneMorph : MorphData
        {
            public int BoneIndex { get; set; }
            public Vector3 Translation { get; set; }
            public Quaternion Rotation { get; set; }
        }

        public class UvMorph : MorphData
        {
            public int VertexIndex { get; set; }
            public Vector4 Offset { get; set; }
        }

        public class MaterialMorph : MorphData
        {
            //MaterialMorphMethod
            public const int MorphMatMul = 0x00;
            public const int MorphMatAdd = 0x01;

            public int MaterialIndex { get; set; }
            public bool Global { get; set; }
            public int Method { get; set; }
            public Color Diffuse { get; set; }
            public Color Specular { get; set; }
            public Color Ambient { get; set; }
            public float Shiness { get; set; }
            public Color EdgeColor { get; set; }
            public float EdgeSize { get; set; }
            public Vector4 Texture { get; set; }
            public Vector4 SubTexture { get; set; }
            public Vector4 ToonTexture { get; set; }
        }

        public string Name { get; set; }
        public string NameEn { get; set; }
        public int Category { get; set; }
        public int Type { get; set; }
        public MorphData[] MorphDatas { get; set; }
    }
}