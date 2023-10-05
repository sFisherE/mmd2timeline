using System.Collections.Generic;
using LibMMD.Model;
using UnityEngine;

namespace LibMMD.Motion
{
    public class BoneImage
    {
        public BoneImage()
        {
            Rotation = Quaternion.identity;
            Translation = Vector3.zero;
            MorphRotation = Quaternion.identity;
            MorphTranslation = Vector3.zero;
            GlobalOffsetMatrix = Matrix4x4.identity;
            GlobalOffsetMatrixInv = Matrix4x4.identity;
        }

        public string Name;

        public Quaternion Rotation { get; set; }
        public Vector3 Translation { get; set; }

        public bool SkipForIk = false;

        public Quaternion MorphRotation { get; set; }
        public Vector3 MorphTranslation { get; set; }

        public bool HasParent { get; set; }
        public int Parent { get; set; }

        public bool HasAppend { get; set; }
        public bool AppendRotate { get; set; }
        public bool AppendTranslate { get; set; }

        public int AppendParent { get; set; }
        public float AppendRatio { get; set; }

        public bool HasIk { get; set; }
        public bool IkLink { get; set; }

        public float CcdAngleLimit { get; set; }
        public int CcdIterateLimit { get; set; }

        public int[] IkLinks { get; set; }

        //AxisFixType
        public const int FixNone=0;
        public const int FixX=1;
        public const int FixY=2;
        public const int FixZ=3;
        public const int FixAll=4;

        //AxisTransformOrder
        public const int OrderZxy = 0;
        public const int OrderXyz = 1;
        public const int OrderYzx = 2;

        public int[] IkFixTypes { get; set; }
        public int[] IkTransformOrders { get; set; }

        public bool[] IkLinkLimited { get; set; }
        public Vector3[] IkLinkLimitsMin { get; set; }
        public Vector3[] IkLinkLimitsMax { get; set; }

        public int IkTarget { get; set; }

        public Quaternion PreIkRotation { get; set; }
        public Quaternion IkRotation { get; set; }

        public Quaternion TotalRotation { get; set; }
        public Vector3 TotalTranslation { get; set; }

        public Vector3 LocalOffset { get; set; }

        public Matrix4x4 GlobalOffsetMatrix;
        public Matrix4x4 GlobalOffsetMatrixInv;
        public Matrix4x4 LocalMatrix;

        public Matrix4x4 SkinningMatrix;

        public class TransformOrder : IComparer<int>
        {
            public TransformOrder(MmdModel model)
            {
                _model = model;
            }

            public int Compare(int a, int b)
            {
                if (_model.Bones[a].TransformLevel == _model.Bones[b].TransformLevel)
                {
                    return a.CompareTo(b);
                }
                return _model.Bones[a].TransformLevel.CompareTo(_model.Bones[b].TransformLevel);
            }

            private readonly MmdModel _model;
        };
    }
}