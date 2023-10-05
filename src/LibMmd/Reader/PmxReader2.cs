using System;
//using System.IO;
using System.Text;
using LibMMD.Model;
using LibMMD.Util;
using UnityEngine;
using mmd2timeline;

namespace LibMMD.Reader
{
    public class PmxReader2 : ModelReader2
    {
        
        public override MmdModel Read(BufferBinaryReader reader)
        {
            var pmxHeader = ReadMeta(reader);
            if (!"PMX ".Equals(pmxHeader.Magic) || Math.Abs(pmxHeader.Version - 2.0f) > 0.0001f || pmxHeader.FileFlagSize !=8)
            {
                throw new MmdFileParseException("File is not a PMX 2.0 file");
            }

            var model = new MmdModel();
            var pmxConfig = ReadPmxConfig(reader, model);
            ReadModelNameAndDescription(reader, model, pmxConfig);
            ReadVertices(reader, model, pmxConfig);
            ReadTriangles(reader, model, pmxConfig);
            ReadTextureList(reader, pmxConfig);
            ReadParts(reader, model, pmxConfig);
            ReadBones(reader, model, pmxConfig);
            ReadMorphs(reader, model, pmxConfig);
            ReadEntries(reader, pmxConfig);
            ReadRigidBodies(reader, model, pmxConfig);
            ReadConstraints(reader, model, pmxConfig);
            return model;
        }

        private static void ReadConstraints(BufferBinaryReader reader, MmdModel model, PmxConfig pmxConfig)
        {
            var constraintNum = reader.ReadInt32();
            for (var i = 0; i < constraintNum; ++i)
            {
                MmdReaderUtil2.ReadSizedString(reader, pmxConfig.Encoding);
                MmdReaderUtil2.ReadSizedString(reader, pmxConfig.Encoding);
                var dofType = reader.ReadByte();
                if (dofType == 0)
                {
                    MmdReaderUtil2.ReadIndex(reader, pmxConfig.RigidBodyIndexSize);
                    MmdReaderUtil2.ReadIndex(reader, pmxConfig.RigidBodyIndexSize);
                    MmdReaderUtil2.ReadVector3(reader);
                    MmdReaderUtil2.ReadVector3(reader);
                    MmdReaderUtil2.ReadVector3(reader);
                    MmdReaderUtil2.ReadVector3(reader);
                    MmdReaderUtil2.ReadVector3(reader);
                    MmdReaderUtil2.ReadVector3(reader);
                    MmdReaderUtil2.ReadVector3(reader);
                    MmdReaderUtil2.ReadVector3(reader);
                }
                else
                {
                    throw new MmdFileParseException("Only 6DOF spring joints are supported.");
                }
            }
        }

        private static PmxConfig ReadPmxConfig(BufferBinaryReader reader, MmdModel model)
        {
            var pmxConfig = new PmxConfig();
            pmxConfig.Utf8Encoding = reader.ReadByte() != 0;
            pmxConfig.ExtraUvNumber = reader.ReadSByte();
            pmxConfig.VertexIndexSize = reader.ReadSByte();
            pmxConfig.TextureIndexSize = reader.ReadSByte();
            pmxConfig.MaterialIndexSize = reader.ReadSByte();
            pmxConfig.BoneIndexSize = reader.ReadSByte();
            pmxConfig.MorphIndexSize = reader.ReadSByte();
            pmxConfig.RigidBodyIndexSize = reader.ReadSByte();

            model.ExtraUvNumber = pmxConfig.ExtraUvNumber;
            pmxConfig.Encoding = pmxConfig.Utf8Encoding ? Encoding.UTF8 : Encoding.Unicode;
            return pmxConfig;
        }

        private static void ReadRigidBodies(BufferBinaryReader reader, MmdModel model, PmxConfig pmxConfig)
        {
            var rigidBodyNum = reader.ReadInt32();
            for (var i = 0; i < rigidBodyNum; ++i)
            {
                MmdReaderUtil2.ReadSizedString(reader, pmxConfig.Encoding);
                MmdReaderUtil2.ReadSizedString(reader, pmxConfig.Encoding);
                MmdReaderUtil2.ReadIndex(reader, pmxConfig.BoneIndexSize);
                reader.ReadByte();
                reader.ReadUInt16();
                reader.ReadByte();
                MmdReaderUtil2.ReadVector3(reader);
                MmdReaderUtil2.ReadVector3(reader);
                MmdReaderUtil2.ReadVector3(reader);
                reader.ReadSingle();
                reader.ReadSingle();
                reader.ReadSingle();
                reader.ReadSingle();
                reader.ReadSingle();
                reader.ReadByte();
            }
        }

        //unused data
        private static void ReadEntries(BufferBinaryReader reader, PmxConfig pmxConfig)
        {
            var entryItemNum = reader.ReadInt32();
            for (var i = 0; i < entryItemNum; ++i)
            {
                MmdReaderUtil2.ReadSizedString(reader, pmxConfig.Encoding); //entryItemName
                MmdReaderUtil2.ReadSizedString(reader, pmxConfig.Encoding); //entryItemNameEn
                reader.ReadByte(); //isSpecial
                var elementNum = reader.ReadInt32();
                for (var j = 0; j < elementNum; ++j)
                {
                    var isMorph = reader.ReadByte() == 1;
                    if (isMorph)
                    {
                        MmdReaderUtil2.ReadIndex(reader, pmxConfig.MorphIndexSize); //morphIndex
                    }
                    else
                    {
                        MmdReaderUtil2.ReadIndex(reader, pmxConfig.BoneIndexSize); //boneIndex
                    }
                }
            }
        }

        private static void ReadMorphs(BufferBinaryReader reader, MmdModel model, PmxConfig pmxConfig)
        {
            var morphNum = reader.ReadInt32();
            int? baseMorphIndex = null;
            model.Morphs = new Morph[morphNum];
            for (var i = 0; i < morphNum; ++i)
            {
                var morph = new Morph
                {
                    Name = MmdReaderUtil2.ReadSizedString(reader, pmxConfig.Encoding),
                    NameEn = MmdReaderUtil2.ReadSizedString(reader, pmxConfig.Encoding),
                    Category = reader.ReadByte()
                };
                if (morph.Category == Morph.MorphCatSystem)
                {
                    baseMorphIndex = i;
                }
                morph.Type = reader.ReadByte();
                var morphDataNum = reader.ReadInt32();
                morph.MorphDatas = new Morph.MorphData[morphDataNum];
                switch (morph.Type)
                {
                    case Morph.MorphTypeGroup:
                        for (var j = 0; j < morphDataNum; ++j)
                        {
                            var morphData =
                                new Morph.GroupMorph
                                {
                                    MorphIndex = MmdReaderUtil2.ReadIndex(reader, pmxConfig.MorphIndexSize),
                                    MorphRate = reader.ReadSingle()
                                };
                            morph.MorphDatas[j] = morphData;
                        }
                        break;
                    case Morph.MorphTypeVertex:
                        for (var j = 0; j < morphDataNum; ++j)
                        {
                            var morphData =
                                new Morph.VertexMorph
                                {
                                    VertexIndex = MmdReaderUtil2.ReadIndex(reader, pmxConfig.VertexIndexSize),
                                    Offset = MmdReaderUtil2.ReadVector3(reader)
                                };
                            morph.MorphDatas[j] = morphData;
                        }
                        break;
                    case Morph.MorphTypeBone:
                        for (var j = 0; j < morphDataNum; ++j)
                        {
                            var morphData =
                                new Morph.BoneMorph
                                {
                                    BoneIndex = MmdReaderUtil2.ReadIndex(reader, pmxConfig.BoneIndexSize),
                                    Translation = MmdReaderUtil2.ReadVector3(reader),
                                    Rotation = MmdReaderUtil2.ReadQuaternion(reader)
                                };
                            morph.MorphDatas[j] = morphData;
                        }

                        break;
                    case Morph.MorphTypeUv:
                    case Morph.MorphTypeExtUv1:
                    case Morph.MorphTypeExtUv2:
                    case Morph.MorphTypeExtUv3:
                    case Morph.MorphTypeExtUv4:
                        for (var j = 0; j < morphDataNum; ++j)
                        {
                            var morphData =
                                new Morph.UvMorph
                                {
                                    VertexIndex = MmdReaderUtil2.ReadIndex(reader, pmxConfig.VertexIndexSize),
                                    Offset = MmdReaderUtil2.ReadVector4(reader)
                                };
                            morph.MorphDatas[j] = morphData;
                        }

                        break;
                    case Morph.MorphTypeMaterial:
                        for (var j = 0; j < morphDataNum; j++)
                        {
                            var morphData = new Morph.MaterialMorph();
                            MmdReaderUtil2.ReadIndex(reader, pmxConfig.MaterialIndexSize);
                            morphData.Method = reader.ReadByte();
                            morphData.Diffuse = MmdReaderUtil2.ReadColor(reader, true);
                            morphData.Specular = MmdReaderUtil2.ReadColor(reader, false);
                            morphData.Shiness = reader.ReadSingle();
                            morphData.Ambient = MmdReaderUtil2.ReadColor(reader, false);
                            morphData.EdgeColor = MmdReaderUtil2.ReadColor(reader, true);
                            morphData.EdgeSize = reader.ReadSingle();
                            morphData.Texture = MmdReaderUtil2.ReadVector4(reader);
                            morphData.SubTexture = MmdReaderUtil2.ReadVector4(reader);
                            morphData.ToonTexture = MmdReaderUtil2.ReadVector4(reader);
                            morph.MorphDatas[j] = morphData;
                        }
                        break;
                    default:
                        throw new MmdFileParseException("invalid morph type " + morph.Type);
                }
                if (baseMorphIndex != null)
                {
                    //TODO rectify system-reserved category
                }

                model.Morphs[i] = morph;
            }
        }

        private static void ReadBones(BufferBinaryReader reader, MmdModel model, PmxConfig pmxConfig)
        {
            var boneNum = reader.ReadInt32();
            model.Bones = new Bone[boneNum];
            for (var i = 0; i < boneNum; ++i)
            {
                var bone = new Bone
                {
                    Name = MmdReaderUtil2.ReadSizedString(reader, pmxConfig.Encoding),
                    NameEn = MmdReaderUtil2.ReadSizedString(reader, pmxConfig.Encoding),
                    Position = MmdReaderUtil2.ReadVector3(reader)
                };
                bone.InitPosition = bone.Position;

                var parentIndex = MmdReaderUtil2.ReadIndex(reader, pmxConfig.BoneIndexSize);
                if (parentIndex < boneNum && parentIndex >= 0)
                {
                    bone.ParentIndex = parentIndex;
                }
                else
                {
                    bone.ParentIndex = -1;
                }
                bone.TransformLevel = reader.ReadInt32();
                var flag = reader.ReadUInt16();
                bone.ChildBoneVal.ChildUseId = (flag & PmxBoneFlags.PmxBoneChildUseId) != 0;
                bone.Rotatable = (flag & PmxBoneFlags.PmxBoneRotatable) != 0;
                bone.Movable = (flag & PmxBoneFlags.PmxBoneMovable) != 0;
                bone.Visible = (flag & PmxBoneFlags.PmxBoneVisible) != 0;
                bone.Controllable = (flag & PmxBoneFlags.PmxBoneControllable) != 0;
                bone.HasIk = (flag & PmxBoneFlags.PmxBoneHasIk) != 0;
                bone.AppendRotate = (flag & PmxBoneFlags.PmxBoneAcquireRotate) != 0;
                bone.AppendTranslate = (flag & PmxBoneFlags.PmxBoneAcquireTranslate) != 0;
                bone.RotAxisFixed = (flag & PmxBoneFlags.PmxBoneRotAxisFixed) != 0;
                bone.UseLocalAxis = (flag & PmxBoneFlags.PmxBoneUseLocalAxis) != 0;
                bone.PostPhysics = (flag & PmxBoneFlags.PmxBonePostPhysics) != 0;
                bone.ReceiveTransform = (flag & PmxBoneFlags.PmxBoneReceiveTransform) != 0;
                if (bone.ChildBoneVal.ChildUseId)
                {
                    bone.ChildBoneVal.Index = MmdReaderUtil2.ReadIndex(reader, pmxConfig.BoneIndexSize);
                }
                else
                {
                    bone.ChildBoneVal.Offset = MmdReaderUtil2.ReadVector3(reader);
                }
                if (bone.RotAxisFixed)
                {
                    bone.RotAxis = MmdReaderUtil2.ReadVector3(reader);
                }
                if (bone.AppendRotate || bone.AppendTranslate)
                {
                    bone.AppendBoneVal.Index = MmdReaderUtil2.ReadIndex(reader, pmxConfig.BoneIndexSize);
                    bone.AppendBoneVal.Ratio = reader.ReadSingle();
                }
                if (bone.UseLocalAxis)
                {
                    var localX = MmdReaderUtil2.ReadVector3(reader);
                    var localZ = MmdReaderUtil2.ReadVector3(reader);
                    var localY = Vector3.Cross(localX, localZ);
                    localZ = Vector3.Cross(localX, localY);
                    localX.Normalize();
                    localY.Normalize();
                    localZ.Normalize();
                    bone.LocalAxisVal.AxisX = localX;
                    bone.LocalAxisVal.AxisY = localY;
                    bone.LocalAxisVal.AxisZ = localZ;
                }
                if (bone.ReceiveTransform)
                {
                    bone.ExportKey = reader.ReadInt32();
                }
                if (bone.HasIk)
                {
                    ReadBoneIk(reader, bone, pmxConfig.BoneIndexSize);
                }

                model.Bones[i] = bone;

                //if (MmdTarget.Inst != null)
                //{
                //    //Debug.Log(bone.Name);
                //    if (MmdTarget.Inst.positionLookup.ContainsKey(bone.Name))
                //    {
                //        bone.Position = MmdTarget.Inst.positionLookup[bone.Name];
                //    }
                //}
            }
        }

        private static void ReadBoneIk(BufferBinaryReader reader, Bone bone, int boneIndexSize)
        {
            bone.IkInfoVal = new Bone.IkInfo();
            bone.IkInfoVal.IkTargetIndex = MmdReaderUtil2.ReadIndex(reader, boneIndexSize);
            bone.IkInfoVal.CcdIterateLimit = reader.ReadInt32();
            bone.IkInfoVal.CcdAngleLimit = reader.ReadSingle();
            var ikLinkNum = reader.ReadInt32();
            bone.IkInfoVal.IkLinks = new Bone.IkLink[ikLinkNum];
            for (var j = 0; j < ikLinkNum; ++j)
            {
                var link = new Bone.IkLink();
                link.LinkIndex = MmdReaderUtil2.ReadIndex(reader, boneIndexSize);
                link.HasLimit = reader.ReadByte() != 0;
                if (link.HasLimit)
                {
                    link.LoLimit = MmdReaderUtil2.ReadVector3(reader);
                    link.HiLimit = MmdReaderUtil2.ReadVector3(reader);
                }
                bone.IkInfoVal.IkLinks[j] = link;
            }
        }

        private static void ReadParts(BufferBinaryReader reader, MmdModel model, PmxConfig pmxConfig)
        {
            var partNum = reader.ReadInt32();
            var partBaseShift = 0;
            for (var i = 0; i < partNum; i++)
            {
                ReadMaterial(reader, pmxConfig.Encoding, pmxConfig.TextureIndexSize);
                var partTriangleIndexNum = reader.ReadInt32();
                if (partTriangleIndexNum % 3 != 0)
                {
                    throw new MmdFileParseException("part" + i + " triangle index count " + partTriangleIndexNum +
                                                    " is not multiple of 3");
                }
                partBaseShift += partTriangleIndexNum;
            }
        }

        private static void ReadTextureList(BufferBinaryReader reader, PmxConfig pmxConfig)
        {
            var textureNum = reader.ReadInt32();
            for (var i = 0; i < textureNum; ++i)
            {
                var texturePathEncoding = pmxConfig.Utf8Encoding ? Encoding.UTF8 : Encoding.Unicode;
                MmdReaderUtil2.ReadSizedString(reader, texturePathEncoding);
            }
        }

        private static void ReadTriangles(BufferBinaryReader reader, MmdModel model, PmxConfig pmxConfig)
        {
            var triangleIndexCount = reader.ReadInt32();
            model.TriangleIndexes = new int[triangleIndexCount];
            if (triangleIndexCount % 3 != 0)
            {
                throw new MmdFileParseException("triangle index count " + triangleIndexCount + " is not multiple of 3");
            }
            for (var i = 0; i < triangleIndexCount; ++i)
            {
                model.TriangleIndexes[i] = MmdReaderUtil2.ReadIndex(reader, pmxConfig.VertexIndexSize);
            }
        }

        private static void ReadVertices(BufferBinaryReader reader, MmdModel model, PmxConfig pmxConfig)
        {
            var vertexNum = reader.ReadInt32();
            for (uint i = 0; i < vertexNum; ++i)
            {
                ReadVertex(reader, pmxConfig);
            }
        }

        private static void ReadModelNameAndDescription(BufferBinaryReader reader, MmdModel model, PmxConfig pmxConfig)
        {
            model.Name = MmdReaderUtil2.ReadSizedString(reader, pmxConfig.Encoding);
            model.NameEn = MmdReaderUtil2.ReadSizedString(reader, pmxConfig.Encoding);
            model.Description = MmdReaderUtil2.ReadSizedString(reader, pmxConfig.Encoding);
            model.DescriptionEn = MmdReaderUtil2.ReadSizedString(reader, pmxConfig.Encoding);
        }

        private static void ReadVertex(BufferBinaryReader reader, PmxConfig pmxConfig)
        {
            ReadVertexBasic(reader);
            if (pmxConfig.ExtraUvNumber > 0)
            {
                var extraUv = new Vector4[pmxConfig.ExtraUvNumber];
                for (var ei = 0; ei < pmxConfig.ExtraUvNumber; ++ei)
                {
                    extraUv[ei] = MmdReaderUtil2.ReadVector4(reader);
                }
            }

            var op = new SkinningOperator();
            var skinningType = reader.ReadByte();
            op.Type = skinningType;

            switch (skinningType)
            {
                case SkinningOperator.SkinningBdef1:
                    var bdef1 = new SkinningOperator.Bdef1();
                    bdef1.BoneId = MmdReaderUtil2.ReadIndex(reader, pmxConfig.BoneIndexSize);
                    op.Param = bdef1;
                    break;
                case SkinningOperator.SkinningBdef2:
                    var bdef2 = new SkinningOperator.Bdef2();
                    bdef2.BoneId[0] = MmdReaderUtil2.ReadIndex(reader, pmxConfig.BoneIndexSize);
                    bdef2.BoneId[1] = MmdReaderUtil2.ReadIndex(reader, pmxConfig.BoneIndexSize);
                    bdef2.BoneWeight = reader.ReadSingle();
                    op.Param = bdef2;
                    break;
                case SkinningOperator.SkinningBdef4:
                    var bdef4 = new SkinningOperator.Bdef4();
                    for (var j = 0; j < 4; ++j)
                    {
                        bdef4.BoneId[j] = MmdReaderUtil2.ReadIndex(reader, pmxConfig.BoneIndexSize);
                    }
                    for (var j = 0; j < 4; ++j)
                    {
                        bdef4.BoneWeight[j] = reader.ReadSingle();
                    }
                    op.Param = bdef4;
                    break;
                case SkinningOperator.SkinningSdef:
                    var sdef = new SkinningOperator.Sdef();
                    sdef.BoneId[0] = MmdReaderUtil2.ReadIndex(reader, pmxConfig.BoneIndexSize);
                    sdef.BoneId[1] = MmdReaderUtil2.ReadIndex(reader, pmxConfig.BoneIndexSize);
                    sdef.BoneWeight = reader.ReadSingle();
                    sdef.C = MmdReaderUtil2.ReadVector3(reader);
                    sdef.R0 = MmdReaderUtil2.ReadVector3(reader);
                    sdef.R1 = MmdReaderUtil2.ReadVector3(reader);
                    op.Param = sdef;
                    break;
                default:
                    throw new MmdFileParseException("invalid skinning type: " + skinningType);
            }
            reader.ReadSingle();
        }

        private static void ReadMaterial(BufferBinaryReader reader,  Encoding encoding,
            int textureIndexSize)
        {
            MmdReaderUtil2.ReadSizedString(reader, encoding);
            MmdReaderUtil2.ReadSizedString(reader, encoding);
            MmdReaderUtil2.ReadColor(reader, true);
            MmdReaderUtil2.ReadColor(reader, false);
            reader.ReadSingle();
            MmdReaderUtil2.ReadColor(reader, false);
            reader.ReadByte();
            MmdReaderUtil2.ReadColor(reader, true);
            reader.ReadSingle();
            MmdReaderUtil2.ReadIndex(reader, textureIndexSize);
            MmdReaderUtil2.ReadIndex(reader, textureIndexSize);
            reader.ReadByte();
            var useGlobalToon = reader.ReadByte() != 0;
            if (useGlobalToon)
            {
                reader.ReadByte();
            }
            else
            {
                MmdReaderUtil2.ReadIndex(reader, textureIndexSize);
            }
            MmdReaderUtil2.ReadSizedString(reader, encoding);
        }

        private static PmxMeta ReadMeta(BufferBinaryReader reader)
        {
            PmxMeta ret;
            ret.Magic = MmdReaderUtil2.ReadStringFixedLength(reader, 4, Encoding.ASCII);
            ret.Version = reader.ReadSingle();
            ret.FileFlagSize = reader.ReadByte();
            return ret;
        }

        private static PmxVertexBasic ReadVertexBasic(BufferBinaryReader reader)
        {
            PmxVertexBasic ret;
            ret.Coordinate = MmdReaderUtil2.ReadVector3(reader);
            ret.Normal = MmdReaderUtil2.ReadVector3(reader);
            ret.UvCoordinate = MmdReaderUtil2.ReadVector2(reader);
            return ret;
        }


        private struct PmxMeta
        {
            public string Magic;
            public float Version;
            public byte FileFlagSize;
        }

        private struct PmxVertexBasic
        {
            public Vector3 Coordinate;
            public Vector3 Normal;
            public Vector2 UvCoordinate;
        }

        private class PmxConfig
        {
            public bool Utf8Encoding { get; set; }
            public Encoding Encoding { get; set; }
            public int ExtraUvNumber { get; set; }
            public int VertexIndexSize { get; set; }
            public int TextureIndexSize { get; set; }
            public int MaterialIndexSize{ get; set; }
            public int BoneIndexSize { get; set; }
            public int MorphIndexSize  { get; set; }
            public int RigidBodyIndexSize { get; set; }
        }

        private abstract class PmxMaterialDrawFlags
        {
            public const byte PmxMaterialDrawDoubleFace = 0x01;
            public const byte PmxMaterialDrawGroundShadow = 0x02;
            public const byte PmxMaterialCastSelfShadow = 0x04;
            public const byte PmxMaterialDrawSelfShadow = 0x08;
            public const byte PmxMaterialDrawEdge = 0x10;
        }

        private abstract class PmxBoneFlags
        {
            public const ushort PmxBoneChildUseId = 0x0001;
            public const ushort PmxBoneRotatable = 0x0002;
            public const ushort PmxBoneMovable = 0x0004;
            public const ushort PmxBoneVisible = 0x0008;
            public const ushort PmxBoneControllable = 0x0010;
            public const ushort PmxBoneHasIk = 0x0020;
            public const ushort PmxBoneAcquireRotate = 0x0100;
            public const ushort PmxBoneAcquireTranslate = 0x0200;
            public const ushort PmxBoneRotAxisFixed = 0x0400;
            public const ushort PmxBoneUseLocalAxis = 0x0800;
            public const ushort PmxBonePostPhysics = 0x1000;
            public const ushort PmxBoneReceiveTransform = 0x2000;
        }
    }
}