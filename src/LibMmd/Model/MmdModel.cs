using System;
using System.Collections.Generic;
using UnityEngine;

namespace LibMMD.Model
{
    public class MmdModel
    {
        public string Name { get; set; }
        public string NameEn { get; set; }
        public string Description { get; set; }
        public string DescriptionEn { get; set; }
        public int ExtraUvNumber { get; set; }
        public int[] TriangleIndexes { get; set; }
        public Bone[] Bones { get; set; }
        public Morph[] Morphs { get; set; }

        public Dictionary<string, Vector3> m_BoneAdjust = new Dictionary<string, Vector3>();
    }
}