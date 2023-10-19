using System;
using System.Collections.Generic;
using System.Linq;
using LibMMD.Model;
using LibMMD.Util;
using UnityEngine;
using Tools = LibMMD.Util.Tools;

namespace LibMMD.Motion
{
    public class Poser
    {
        public bool ForceDisableIK = false;
        public BoneImage[] EnumIKBone()
        {
            List<BoneImage> list = new List<BoneImage>();
            foreach (var boneImage in BoneImages)
            {
                if (boneImage.HasIk)
                {
                    list.Add(boneImage);
                }
            }
            return list.ToArray();
        }
        Dictionary<string, BoneImage> BoneImageLookup = new Dictionary<string, BoneImage>();
        public BoneImage GetBoneImage(string name)
        {
            if (BoneImageLookup.ContainsKey(name))
                return BoneImageLookup[name];
            return null;
        }
        public Poser(MmdModel model)
        {
            Model = model;
            /***** Create Pose Image *****/
            var boneNum = model.Bones.Length;
            BoneImages = new BoneImage[boneNum];
            for (var i = 0; i < boneNum; ++i)
            {
                BoneImages[i] = new BoneImage();
            }

            for (var i = 0; i < boneNum; ++i)
            {
                var bone = model.Bones[i];
                _boneNameMap[bone.Name] = i;
                var image = BoneImages[i];
                image.Name = bone.Name;

                image.GlobalOffsetMatrix.m03 = -bone.Position[0];
                image.GlobalOffsetMatrix.m13 = -bone.Position[1];
                image.GlobalOffsetMatrix.m23 = -bone.Position[2];
                image.GlobalOffsetMatrixInv.m03 = bone.Position[0];
                image.GlobalOffsetMatrixInv.m13 = bone.Position[1];
                image.GlobalOffsetMatrixInv.m23 = bone.Position[2];

                image.Parent = bone.ParentIndex;
                if (image.Parent < boneNum && image.Parent >= 0)
                {
                    image.HasParent = true;
                    image.LocalOffset = bone.Position - model.Bones[image.Parent].Position;
                }
                else
                {
                    image.HasParent = false;
                    image.LocalOffset = bone.Position;
                }

                image.AppendRotate = bone.AppendRotate;
                image.AppendTranslate = bone.AppendTranslate;
                image.HasAppend = false;

                if (image.AppendRotate || image.AppendTranslate)
                {
                    image.AppendParent = bone.AppendBoneVal.Index;
                    if (image.AppendParent < boneNum)
                    {
                        image.HasAppend = true;
                        image.AppendRatio = bone.AppendBoneVal.Ratio;
                    }
                }


                image.HasIk = bone.HasIk;
                if (image.HasIk)
                {
                    var ikLinkNum = bone.IkInfoVal.IkLinks.Length;
                    image.IkLinks = new int[ikLinkNum];
                    image.IkFixTypes = new int[ikLinkNum];
                    image.IkTransformOrders = Enumerable.Repeat(BoneImage.OrderYzx, ikLinkNum)
                        .ToArray();
                    image.IkLinkLimited = new bool [ikLinkNum];
                    image.IkLinkLimitsMin = new Vector3[ikLinkNum];
                    image.IkLinkLimitsMax = new Vector3[ikLinkNum];

                    for (var j = 0; j < ikLinkNum; ++j)
                    {
                        var ikLink = bone.IkInfoVal.IkLinks[j];
                        image.IkLinks[j] = ikLink.LinkIndex;
                        image.IkLinkLimited[j] = ikLink.HasLimit;
                        if (image.IkLinkLimited[j])
                        {
                            for (var k = 0; k < 3; ++k)
                            {
                                var lo = ikLink.LoLimit[k];
                                var hi = ikLink.HiLimit[k];
                                image.IkLinkLimitsMin[j][k] = Math.Min(lo, hi);
                                image.IkLinkLimitsMax[j][k] = Math.Max(lo, hi);
                            }
                            if (image.IkLinkLimitsMin[j].x > -Math.PI * 0.5 && image.IkLinkLimitsMax[j].x < Math.PI * 0.5)
                            {
                                image.IkTransformOrders[j] = BoneImage.OrderZxy;
                            }
                            else if (image.IkLinkLimitsMin[j].y > -Math.PI * 0.5 && image.IkLinkLimitsMax[j].y < Math.PI * 0.5)
                            {
                                image.IkTransformOrders[j] = BoneImage.OrderXyz;
                            }
                            if (Math.Abs(image.IkLinkLimitsMin[j].x) < Tools.MmdMathConstEps &&
                                Math.Abs(image.IkLinkLimitsMax[j].x) < Tools.MmdMathConstEps
                                && Math.Abs(image.IkLinkLimitsMin[j].y) < Tools.MmdMathConstEps &&
                                Math.Abs(image.IkLinkLimitsMax[j].y) < Tools.MmdMathConstEps
                                && Math.Abs(image.IkLinkLimitsMin[j].z) < Tools.MmdMathConstEps &&
                                Math.Abs(image.IkLinkLimitsMax[j].z) < Tools.MmdMathConstEps)
                            {
                                image.IkFixTypes[j] = BoneImage.FixAll;
                            }
                            else if (Math.Abs(image.IkLinkLimitsMin[j].y) < Tools.MmdMathConstEps &&
                                     Math.Abs(image.IkLinkLimitsMax[j].y) < Tools.MmdMathConstEps
                                     && Math.Abs(image.IkLinkLimitsMin[j].z) < Tools.MmdMathConstEps &&
                                     Math.Abs(image.IkLinkLimitsMax[j].z) < Tools.MmdMathConstEps)
                            {
                                image.IkFixTypes[j] = BoneImage.FixX;
                            }
                            else if (Math.Abs(image.IkLinkLimitsMin[j].x) < Tools.MmdMathConstEps &&
                                     Math.Abs(image.IkLinkLimitsMax[j].x) < Tools.MmdMathConstEps &&
                                     Math.Abs(image.IkLinkLimitsMin[j].z) < Tools.MmdMathConstEps &&
                                     Math.Abs(image.IkLinkLimitsMax[j].z) < Tools.MmdMathConstEps)
                            {
                                image.IkFixTypes[j] = BoneImage.FixY;
                            }
                            else if (Math.Abs(image.IkLinkLimitsMin[j].x) < Tools.MmdMathConstEps &&
                                     Math.Abs(image.IkLinkLimitsMax[j].x) < Tools.MmdMathConstEps
                                     && Math.Abs(image.IkLinkLimitsMin[j].y) < Tools.MmdMathConstEps &&
                                     Math.Abs(image.IkLinkLimitsMax[j].y) < Tools.MmdMathConstEps)
                            {
                                image.IkFixTypes[j] = BoneImage.FixZ;
                            }
                        }
                        BoneImages[image.IkLinks[j]].IkLink = true;
                    }
                    image.CcdAngleLimit = bone.IkInfoVal.CcdAngleLimit;
                    image.CcdIterateLimit = Math.Min(bone.IkInfoVal.CcdIterateLimit, 256);
                    image.IkTarget = bone.IkInfoVal.IkTargetIndex;
                }
                if (bone.PostPhysics)
                {
                    _postPhysicsBones.Add(i);
                }
                else
                {
                    _prePhysicsBones.Add(i);
                }
            }

            foreach(var item in BoneImages)
            {
                BoneImageLookup.Add(item.Name, item);
            }

            //需要排序
            BoneImage.TransformOrder order = new BoneImage.TransformOrder(model);
            _prePhysicsBones.Sort(order);
            _postPhysicsBones.Sort(order);

            //var materialNum = model.Parts.Length;
            //MaterialMulImages = new MaterialImage[materialNum];
            //MaterialAddImages = new MaterialImage[materialNum];
            //for (var i = 0; i < materialNum; ++i)
            //{
            //    MaterialMulImages[i] = new MaterialImage(1.0f);
            //    MaterialAddImages[i] = new MaterialImage(0.0f);
            //}

            var morphNum = model.Morphs.Length;
            MorphRates = new float[morphNum];
            for (var i = 0; i < morphNum; ++i)
            {
                var morph = model.Morphs[i];
                _morphNameMap[morph.Name] = i;
            }

            ResetPosing();
            //Debug.LogFormat("morphIndex count = {0}, vertex count = {1}", morphIndex.Count, model.Vertices.Length);
            //Debug.LogFormat("morphIndex count = {0}, vertex count = {1}", morphIndex.Count, model.Vertices.Length);
        }

        public void SetBonePose(int index, BonePose bonePose, float motionScale = 1f)
        {
            //LogUtil.Debug($"Poser::SetBonePose:{index},{bonePose.Translation},{bonePose.Rotation},{motionScale}");

            //直接从bonepose里面设置的
            BoneImages[index].Translation = bonePose.Translation * motionScale;
            BoneImages[index].Rotation = bonePose.Rotation;
        }
        public void SetMorphPose(int index, MorphPose morphPose)
        {
            MorphRates[index] = morphPose.Weight;
        }

        private void UpdateBoneAppendTransform(int index)
        {
            var image = BoneImages[index];
            if (!image.HasAppend) return;

            if (image.AppendRotate)
            {
                image.TotalRotation = image.TotalRotation *
                                      Quaternion.SlerpUnclamped(Quaternion.identity,
                                          BoneImages[image.AppendParent].TotalRotation, image.AppendRatio);
            }
            if (image.AppendTranslate)
            {
                image.TotalTranslation = image.TotalTranslation +
                                         image.AppendRatio * BoneImages[image.AppendParent].TotalTranslation;
            }
            
            UpdateLocalMatrix(image);
        }

        private void UpdateLocalMatrix(BoneImage image)
        {
            image.LocalMatrix = MathUtil.QuaternionToMatrix4X4(image.TotalRotation);
            MathUtil.SetTransToMatrix4X4(image.TotalTranslation + image.LocalOffset, ref image.LocalMatrix);

            if (image.HasParent)
            {
                image.LocalMatrix = BoneImages[image.Parent].LocalMatrix * image.LocalMatrix;
            }
        }

        static Dictionary<string, string[]> s_AdjustBones;

        static void InitAdjustBones()
        {
            if (s_AdjustBones != null) return;
            s_AdjustBones = new Dictionary<string, string[]>();
            s_AdjustBones.Add("Arm", new string[] { "左腕", "右腕" });
            s_AdjustBones.Add("Shoulder", new string[] { "左肩", "右肩" });
            s_AdjustBones.Add("Elbow", new string[] { "左ひじ", "右ひじ" });
            s_AdjustBones.Add("Hand", new string[] { "左手首", "右手首" });
            s_AdjustBones.Add("Thigh", new string[] { "左足", "右足" });
            s_AdjustBones.Add("Knee", new string[] { "左ひざ", "右ひざ" });
            s_AdjustBones.Add("Foot", new string[] { "左足首", "右足首" });
            s_AdjustBones.Add("Toe", new string[] { "左つま先", "右つま先" });
        }

        private void UpdateBoneSelfTransform(int index)
        {
            var image = BoneImages[index];

            image.TotalRotation = image.MorphRotation * image.Rotation;
            image.TotalTranslation = image.MorphTranslation + image.Translation;
            bool mark = image.Name.StartsWith("左") ? true : false;

            InitAdjustBones();
            foreach(var item in s_AdjustBones)
            {
                string boneName = item.Key;
                if (Model.m_BoneAdjust.ContainsKey(boneName))
                {
                    var names = s_AdjustBones[boneName];
                    if (image.Name == names[0] || image.Name == names[1])
                    {
                        Vector3 v = Model.m_BoneAdjust[boneName];
                        v = new Vector3(v.x, mark ? v.y : -v.y, mark ? v.z : -v.z);
                        image.TotalRotation = Quaternion.Euler(v) * image.TotalRotation;
                    }
                }
            }

            if (image.IkLink)
            {
                image.PreIkRotation = image.TotalRotation;
                image.TotalRotation = image.IkRotation * image.TotalRotation;
            }

            UpdateLocalMatrix(image);

            if (!image.HasIk) return;
            if (ForceDisableIK) return;

            if (!image.IKEnable)
            {
                return;
            }
            //左足ＩＫ,左つま先ＩＫ,右足ＩＫ,右つま先ＩＫ
            //这4跟骨头会进入下面的计算
            //左足ＩＫ:     IkLinks:左ひざ,左足    IkTarget:左足首 
            //右足ＩＫ:     IkLinks:右ひざ,右足    IkTarget:右足首 
            //左つま先ＩＫ: IkLinks:左足首         IkTarget:左つま先 
            //右つま先ＩＫ: IkLinks:右足首         IkTarget:右つま先 

            var ikLinkNum = image.IkLinks.Length;
            for (var i = 0; i < ikLinkNum; ++i)
            {
                BoneImages[image.IkLinks[i]].IkRotation = Quaternion.identity;
            }
            var ikPosition = MathUtil.GetTransFromMatrix4X4(image.LocalMatrix);
            UpdateBoneSelfTransform(image.IkTarget);
            var targetPosition = MathUtil.GetTransFromMatrix4X4(BoneImages[image.IkTarget].LocalMatrix);
            var ikError = ikPosition - targetPosition;
            if (Vector3.Dot(ikError, ikError) < Tools.MmdMathConstEps)
            {
                return;
            }

            var ikt = image.CcdIterateLimit / 2;
            for (var i = 0; i < image.CcdIterateLimit; ++i)
            {
                for (var j = 0; j < ikLinkNum; ++j)
                {
                    if (image.IkFixTypes[j] == BoneImage.FixAll) continue;
                    var ikImage = BoneImages[image.IkLinks[j]];
                    var ikLinkPosition = MathUtil.GetTransFromMatrix4X4(ikImage.LocalMatrix);
                    var targetDirection = ikLinkPosition - targetPosition;
                    var ikDirection = ikLinkPosition - ikPosition;

                    targetDirection.Normalize();
                    ikDirection.Normalize();

                    var ikRotateAxis = Vector3.Cross(targetDirection, ikDirection);
                    for (var k = 0; k < 3; ++k)
                    {
                        if (Math.Abs(ikRotateAxis[k]) < Tools.MmdMathConstEps)
                        {
                            ikRotateAxis[k] = Tools.MmdMathConstEps;
                        }
                    }
                    var localizationMatrix = ikImage.HasParent ? BoneImages[ikImage.Parent].LocalMatrix : Matrix4x4.identity;
                    if (image.IkLinkLimited[j] && image.IkFixTypes[j] != BoneImage.FixNone &&
                        i < ikt)
                    {
                        switch (image.IkFixTypes[j])
                        {
                            case BoneImage.FixX:
                                ikRotateAxis.x =
                                    Nabs(Vector3.Dot(ikRotateAxis,
                                        MathUtil.Matrix4x4ColDowngrade(localizationMatrix, 0)));
                                ikRotateAxis.y = ikRotateAxis.z = 0.0f;
                                break;
                            case BoneImage.FixY:
                                ikRotateAxis.y =  
                                    Nabs(Vector3.Dot(ikRotateAxis,
                                        MathUtil.Matrix4x4ColDowngrade(localizationMatrix, 1)));
                                ikRotateAxis.x = ikRotateAxis.z = 0.0f;
                                break;
                            case BoneImage.FixZ:
                                ikRotateAxis.z =
                                    Nabs(Vector3.Dot(ikRotateAxis,
                                        MathUtil.Matrix4x4ColDowngrade(localizationMatrix, 2)));
                                ikRotateAxis.x = ikRotateAxis.y = 0.0f;
                                break;
                        }
                    }
                    else
                    { 
                        ikRotateAxis = Matrix4x4.Transpose(localizationMatrix).MultiplyVector(ikRotateAxis);
                        ikRotateAxis.Normalize();
                    }
                    var ikRotateAngle =
                        Mathf.Min(Mathf.Acos(Mathf.Clamp(Vector3.Dot(targetDirection, ikDirection), -1.0f, 1.0f)),
                            image.CcdAngleLimit * (j + 1));
                    ikImage.IkRotation =
                        Quaternion.AngleAxis((float) (ikRotateAngle / Math.PI * 180.0), ikRotateAxis) * ikImage.IkRotation;

                    if (image.IkLinkLimited[j])
                    {
                        var localRotation = ikImage.IkRotation * ikImage.PreIkRotation;
                        switch (image.IkTransformOrders[j])
                        {
                            case BoneImage.OrderZxy:
                                {
                                    var eularAngle = MathUtil.QuaternionToZxy(localRotation);
                                    eularAngle = LimitEularAngle(eularAngle, image.IkLinkLimitsMin[j],
                                        image.IkLinkLimitsMax[j], i < ikt);
                                    localRotation = MathUtil.ZxyToQuaternion(eularAngle);
                                    break;
                                }
                            case BoneImage.OrderXyz:
                                {
                                    var eularAngle = MathUtil.QuaternionToXyz(localRotation);
                                    eularAngle = LimitEularAngle(eularAngle, image.IkLinkLimitsMin[j],
                                        image.IkLinkLimitsMax[j], i < ikt);
                                    localRotation = MathUtil.XyzToQuaternion(eularAngle);
                                    break;
                                }
                            case BoneImage.OrderYzx:
                                {
                                    var eularAngle = MathUtil.QuaternionToYzx(localRotation);
                                    eularAngle = LimitEularAngle(eularAngle, image.IkLinkLimitsMin[j],
                                        image.IkLinkLimitsMax[j], i < ikt);
                                    localRotation = MathUtil.YzxToQuaternion(eularAngle);
                                    break;
                                }
                            default:
                                throw new ArgumentOutOfRangeException();
                        }
                        ikImage.IkRotation = localRotation * Quaternion.Inverse(ikImage.PreIkRotation);
                    }
                    for (var k = 0; k <= j; ++k)
                    {
                        var linkImage = BoneImages[image.IkLinks[j - k]];
                        linkImage.TotalRotation = linkImage.IkRotation * linkImage.PreIkRotation;
                        UpdateLocalMatrix(linkImage);
                    }
                    UpdateBoneSelfTransform(image.IkTarget);
                    targetPosition = MathUtil.Matrix4x4ColDowngrade(BoneImages[image.IkTarget].LocalMatrix, 3);
                }
                ikError = ikPosition - targetPosition;
                if (Vector3.Dot(ikError, ikError) < Tools.MmdMathConstEps)
                {
                    return;
                }
            }
        }

        private static float Nabs(float x)
        {
            if (x > 0.0f)
            {
                return 1.0f;
            }
            return -1.0f;
        }

        private static Vector3 LimitEularAngle(Vector3 eular, Vector3 eularMin, Vector3 eularMax, bool ikt)
        {
            var result = eular;
            for (var i = 0; i < 3; ++i)
            {
                if (result[i] < eularMin[i])
                {
                    var tf = 2 * eularMin[i] - result[i];
                    if (tf <= eularMax[i] && ikt)
                    {
                        result[i] = tf;
                    }
                    else
                    {
                        result[i] = eularMin[i];
                    }
                }
                if (result[i] > eularMax[i])
                {
                    var tf = 2 * eularMax[i] - result[i];
                    if (tf >= eularMin[i] && ikt)
                    {
                        result[i] = tf;
                    }
                    else
                    {
                        result[i] = eularMax[i];
                    }
                }
            }
            return result;
        }

        public void ResetPosing()
        {
            for (var i = 0; i < MorphRates.Length; ++i)
            {
                MorphRates[i] = 0.0f;
            }
            foreach (var boneImage in BoneImages)
            {
                boneImage.Rotation = new Quaternion(0.0f, 0.0f, 0.0f, 1.0f);
                boneImage.Translation = Vector4.zero;
            }

            PrePhysicsPosing();
            PostPhysicsPosing();
        }

        public void PrePhysicsPosing(bool calculateMorph = true)
        {
            foreach (var boneImage in BoneImages)
            {
                boneImage.MorphTranslation = Vector3.zero;
                boneImage.MorphRotation = Quaternion.identity;
                boneImage.LocalMatrix = Matrix4x4.identity;
                boneImage.PreIkRotation = Quaternion.identity;
                boneImage.IkRotation = Quaternion.identity;
                boneImage.TotalRotation = Quaternion.identity;
                boneImage.TotalTranslation = Vector3.zero;
            }
            UpdateBoneTransform(_prePhysicsBones);
            UpdateBoneSkinningMatrix(_prePhysicsBones);
        }

        private void UpdateBoneSkinningMatrix(List<int> indexList)
        {
            foreach (var index in indexList)
            {
                var image = BoneImages[index];
                image.SkinningMatrix = image.LocalMatrix * image.GlobalOffsetMatrix;
            }
        }

        private void UpdateBoneTransform(List<int> indexList)
        {
            foreach (var index in indexList)
            {
                UpdateBoneSelfTransform(index);
            }
            foreach (var index in indexList)
            {
                UpdateBoneAppendTransform(index);
            }
            foreach (var index in indexList)
            {
                UpdateLocalMatrix(BoneImages[index]);
            }
        }

        public void PostPhysicsPosing()
        {
            UpdateBoneTransform(_postPhysicsBones);
            UpdateBoneSkinningMatrix(_postPhysicsBones);
        }

        //public Vector3[] VertexImages { get; set; }
        public BoneImage[] BoneImages { get; set; }
        //public MaterialImage[] MaterialMulImages { get; set; }
        //public MaterialImage[] MaterialAddImages { get; set; }

        public float[] MorphRates { get; set; }

        public MmdModel Model { get; set; }

        private readonly Dictionary<string, int> _boneNameMap = new Dictionary<string, int>();
        private readonly Dictionary<string, int> _morphNameMap = new Dictionary<string, int>();

        private readonly List<int> _prePhysicsBones = new List<int>();
        private readonly List<int> _postPhysicsBones = new List<int>();
    }
}