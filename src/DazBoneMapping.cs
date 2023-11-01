using LibMMD.Unity3D;
using UnityEngine;
using System.Collections.Generic;
using LibMMD.Model;
namespace mmd2timeline
{
    class DazBoneMapping
    {
        /// <summary>
        /// 查看是否是右侧骨骼
        /// </summary>
        static Dictionary<string, bool> IsRightSideBoneLookup = new Dictionary<string, bool>();
        public static bool IsRightSideBone(string boneName)
        {
            if (IsRightSideBoneLookup.ContainsKey(boneName))
                return IsRightSideBoneLookup[boneName];
            var result = boneName.StartsWith("r") || boneName.StartsWith("右");
            IsRightSideBoneLookup.Add(boneName, result);
            return result;
        }

        static void MatchNode(Atom atom, Transform child,string childBoneName,string parentBoneName,Transform target)
        {
            Transform parentBone = SearchObjName(target, parentBoneName);
            if (parentBoneName == "Genesis2Female")
            {
                parentBone = atom.mainController.transform;
            }
            Transform childBone = SearchObjName(target, childBoneName);
            float dis2Parent = Vector3.Distance(parentBone.position, childBone.position)*10;

            Transform parent = child.parent;
            float diff = Vector3.Distance(child.position, parent.position)- dis2Parent;

            var dir = (parent.position-child.position).normalized;
            child.position += dir * diff;
        }
        static void MatchNode2(Atom atom, Transform child, Transform another, string childBoneName, string parentBoneName, Transform target)
        {
            Transform parentBone = SearchObjName(target, parentBoneName);
            if(parentBoneName== "Genesis2Female")
            {
                parentBone = atom.mainController.transform;
            }

            Transform childBone = SearchObjName(target, childBoneName);
            float dis2Parent = Vector3.Distance(parentBone.position, childBone.position) * 10;

            Transform parent = another;
            float diff = Vector3.Distance(child.position, parent.position) - dis2Parent;
            var dir = (parent.position - child.position).normalized;
            child.position += dir * diff;
        }
        static void MatchNode3(Atom atom, Transform child, string childBoneName, string parentBoneName,float ratio, Transform target)
        {
            Transform parentBone = SearchObjName(target, parentBoneName);
            if (parentBoneName == "Genesis2Female")
            {
                parentBone = atom.mainController.transform;
            }

            Transform childBone = SearchObjName(target, childBoneName);
            float dis2Parent = Vector3.Distance(parentBone.position, childBone.position) * 10*ratio;

            Transform parent = child.parent;
            float diff = Vector3.Distance(child.position, parent.position) - dis2Parent;
            var dir = (parent.position - child.position).normalized;
            child.position += dir * diff;
        }
        static void FollowNode( Transform node, Transform other)
        {
            node.position = other.position;
        }
        public static void MatchTarget(Atom atom, MmdGameObject mmdGameObject, Transform target)
        {
            //Dictionary<string, float> lookup = new Dictionary<string, float>();
            Dictionary<string, Transform> mmdBones = new Dictionary<string, Transform>();
            for (int i = 0; i < mmdGameObject._bones.Length; i++)
            {
                var bone = mmdGameObject._bones[i];
                mmdBones.Add(bone.name, bone.transform);
            }
            //hip Genesis2Female
            //pelvis hip
            //rThigh pelvis
            //lThigh pelvis
            //rShin rThigh
            //lShin lThigh
            //rFoot rShin
            //lFoot lShin
            //rToe rFoot
            //lToe lFoot
            MatchNode2(atom,mmdBones["腰"], mmdBones["全ての親"], "hip", "Genesis2Female",target);
            MatchNode(atom, mmdBones["下半身"], "pelvis", "hip", target);

            MatchNode(atom,mmdBones["左足"], "lThigh", "pelvis", target);
            MatchNode(atom,mmdBones["左ひざ"], "lShin", "lThigh", target);
            MatchNode(atom,mmdBones["左足首"], "lFoot", "lShin", target);
            MatchNode(atom, mmdBones["左つま先"], "lToe", "lFoot", target);

            MatchNode(atom,mmdBones["右足"], "rThigh", "pelvis", target);
            MatchNode(atom,mmdBones["右ひざ"], "rShin", "rThigh", target);
            MatchNode(atom,mmdBones["右足首"], "rFoot", "rShin", target);
            MatchNode(atom, mmdBones["右つま先"], "rToe", "rFoot", target);

            MatchNode(atom,mmdBones["左足D"], "lThigh", "pelvis", target);
            MatchNode(atom,mmdBones["左ひざD"], "lShin", "lThigh", target);
            MatchNode(atom,mmdBones["左足首D"], "lFoot", "lShin", target);
            MatchNode(atom,mmdBones["左足先EX"], "lToe", "lFoot", target);

            MatchNode(atom,mmdBones["右足D"], "rThigh", "pelvis", target);
            MatchNode(atom,mmdBones["右ひざD"], "rShin", "rThigh", target);
            MatchNode(atom,mmdBones["右足首D"], "rFoot", "rShin", target);
            MatchNode(atom, mmdBones["右足先EX"], "rToe", "rFoot", target);

            FollowNode(mmdBones["左足IK親"], mmdBones["左足"]);
            FollowNode(mmdBones["左足ＩＫ"], mmdBones["左足"]);
            FollowNode(mmdBones["左つま先ＩＫ"], mmdBones["左つま先"]);

            FollowNode(mmdBones["右足IK親"], mmdBones["右足"]);
            FollowNode(mmdBones["右足ＩＫ"], mmdBones["右足"]);
            FollowNode(mmdBones["右つま先ＩＫ"], mmdBones["右つま先"]);

            //abdomen hip
            //abdomen2 abdomen
            //chest abdomen2
            //neck chest
            //head neck
            MatchNode(atom,mmdBones["上半身"], "abdomen", "hip", target);
            MatchNode(atom,mmdBones["上半身2"], "abdomen2", "abdomen", target);
            MatchNode(atom,mmdBones["上半身3"], "chest", "abdomen2", target);
            MatchNode(atom,mmdBones["首"], "neck", "chest", target);
            MatchNode(atom, mmdBones["頭"], "head", "neck", target);

            //lCollar chest
            //lShldr lCollar
            //lForeArm lShldr
            //lHand lForeArm
            MatchNode(atom, mmdBones["左肩P"], "lCollar", "chest", target);
            FollowNode(mmdBones["左肩"], mmdBones["左肩P"]);
            MatchNode(atom, mmdBones["左肩C"], "lShldr", "lCollar", target);
            FollowNode(mmdBones["左腕"], mmdBones["左肩C"]);
            MatchNode3(atom, mmdBones["左腕捩"], "lForeArm", "lShldr", 0.5f, target);
            MatchNode2(atom, mmdBones["左ひじ"], mmdBones["左腕"], "lForeArm", "lShldr", target);
            MatchNode3(atom, mmdBones["左手捩"], "lHand", "lForeArm", 0.5f, target);
            MatchNode2(atom, mmdBones["左手首"], mmdBones["左ひじ"], "lHand", "lForeArm", target);

            MatchNode(atom, mmdBones["右肩P"], "rCollar", "chest", target);
            FollowNode(mmdBones["右肩"], mmdBones["右肩P"]);
            MatchNode(atom, mmdBones["右肩C"], "rShldr", "rCollar", target);
            FollowNode(mmdBones["右腕"], mmdBones["右肩C"]);
            //右腕捩
            MatchNode3(atom, mmdBones["右腕捩"], "rForeArm", "rShldr",0.5f, target);
            MatchNode2(atom, mmdBones["右ひじ"], mmdBones["右腕"], "rForeArm", "rShldr", target);
            //右手捩
            MatchNode3(atom, mmdBones["右手捩"], "rHand", "rForeArm", 0.5f, target);
            MatchNode2(atom, mmdBones["右手首"], mmdBones["右ひじ"], "rHand", "rForeArm", target);

            var bones = mmdGameObject._model.Bones;
            foreach (var item in bones)
            {
                item.Position = mmdBones[item.Name].position;
            }

        }


        public static Dictionary<string, Transform> CreateFakeBones(Transform root, Transform refTf)
        {
            List<string> names = new List<string>()
        {
            "Shldr","ForeArm","Hand",
            "Carpal1","Index1","Index2","Index3","Mid1","Mid2","Mid3",
            "Carpal2","Pinky1","Pinky2","Pinky3","Ring1","Ring2","Ring3",
            "Thumb1","Thumb2","Thumb3",
        };
            Dictionary<string, string> lookup = new Dictionary<string, string>();
            lookup.Add("ForeArm", "Shldr");
            lookup.Add("Hand", "ForeArm");
            lookup.Add("Carpal1", "Hand");
            lookup.Add("Index1", "Carpal1"); lookup.Add("Index2", "Index1"); lookup.Add("Index3", "Index2");
            lookup.Add("Mid1", "Carpal1"); lookup.Add("Mid2", "Mid1"); lookup.Add("Mid3", "Mid2");
            lookup.Add("Carpal2", "Hand");
            lookup.Add("Pinky1", "Carpal2"); lookup.Add("Pinky2", "Pinky1"); lookup.Add("Pinky3", "Pinky2");
            lookup.Add("Ring1", "Carpal2"); lookup.Add("Ring2", "Ring1"); lookup.Add("Ring3", "Ring2");
            lookup.Add("Thumb1", "Hand"); lookup.Add("Thumb2", "Thumb1"); lookup.Add("Thumb3", "Thumb2");
            string[] type = new string[] { "l", "r" };
            Dictionary<string, Transform> cache = new Dictionary<string, Transform>();
            foreach (var t in type)
            {
                foreach (var item in names)
                {
                    GameObject child = new GameObject(t + item);
                    if (Config.s_Debug)
                    {
                        var go = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                        go.transform.parent = child.transform;
                        go.transform.localPosition = Vector3.zero;
                        go.transform.localScale = new Vector3(0.1f, 0.1f, 0.1f);
                        go.GetComponent<MeshRenderer>().material.color = Color.green;
                        var col = go.GetComponent<Collider>();
                        Component.DestroyImmediate(col);
                    }
                    cache.Add(t + item, child.transform);

                    if (lookup.ContainsKey(item))
                    {
                        child.transform.parent = cache[t + lookup[item]];
                    }
                    else
                    {
                        child.transform.parent = root;
                    }
                }
                foreach (var item in cache)
                {
                    Transform find = SearchObjName(refTf, item.Key);
                    item.Value.position = find.position;
                }
            }
            cache["lShldr"].eulerAngles = new Vector3(0, 0, 36);
            cache["rShldr"].eulerAngles = new Vector3(0, 0, -36);

            return cache;
        }

        public static Dictionary<string, string> vamControlLookup = new Dictionary<string, string>()
        {
            { "右つま先","rToeControl"},
            { "左つま先","lToeControl"},
            { "右足首","rFootControl"},
            { "左足首","lFootControl"},
            { "右足","rThighControl"},
            { "左足","lThighControl"},
            { "右ひざ","rKneeControl"},
            { "左ひざ","lKneeControl"},
        };

        public static List<string> vamControls = new List<string>()
    {
        "control",
        "hipControl",
        "pelvisControl",
        "chestControl",
        "headControl",
        "rHandControl",
        "lHandControl",
        "rFootControl",
        "lFootControl",
        "neckControl",

        "rElbowControl",
        "lElbowControl",
        "rKneeControl",
        "lKneeControl",
        "rToeControl",
        "lToeControl",
        "abdomenControl",
        "abdomen2Control",
        "rThighControl",
        "lThighControl",

        "rArmControl",
        "lArmControl",
        "rShoulderControl",
        "lShoulderControl",
    };
        public static List<string> AssetBindControlNames = new List<string>()
        {
            "lHandControl",//左手首
            "rHandControl",//右手首
        };
        public static Dictionary<string, string> useBoneNames = new Dictionary<string, string>()
        {
            { "腰","hip"},//hipControl
				{ "下半身","pelvis"},//pelvisControl

				{ "上半身","abdomen"},//abdomenControl
				{ "上半身2","abdomen2"},//abdomen2Control
				{ "上半身3","chest"},//chestControl

				{ "首", "neck"},//neckControl
				{ "頭","head"},//headControl

				{ "左肩","lCollar"},//lShoulderControl
				{ "右肩","rCollar"},//rShoulderControl
				{ "左腕","lShldr"},//lArmControl
				{ "右腕","rShldr"},//rArmControl
				{ "左ひじ","lForeArm"},//lElbowControl
				{ "右ひじ","rForeArm"},//rElbowControl
				{ "左手首","lHand"},//lHandControl
				{ "右手首","rHand"},//rHandControl

				{ "左足","lThigh"},//lThighControl
				{ "右足","rThigh"},//rThighControl
				{ "左ひざ","lShin"},//lKneeControl
				{ "右ひざ","rShin"},//rKneeControl
				{ "左足首","lFoot"},//lFootControl
				{ "右足首","rFoot"},//rFootControl
				{ "左つま先","lToe"},//lToeControl
				{ "右つま先","rToe"},//rToeControl
                { "左親指０","lThumb1"},
                { "左親指１","lThumb2"},
                { "左親指２","lThumb3"},
                { "左人指１","lIndex1"},
                { "左人指２","lIndex2"},
                { "左人指３","lIndex3"},
                { "左中指１","lMid1"},
                { "左中指２","lMid2"},
                { "左中指３","lMid3"},
                { "左薬指１","lRing1" },
                { "左薬指２","lRing2" },
                { "左薬指３","lRing3" },
                { "左小指１","lPinky1"},
                { "左小指２","lPinky2"},
                { "左小指３","lPinky3"},

                { "右親指０","rThumb1"},
                { "右親指１","rThumb2"},
                { "右親指２","rThumb3"},
                { "右人指１","rIndex1"},
                { "右人指２","rIndex2"},
                { "右人指３","rIndex3"},
                { "右中指１","rMid1"  },
                { "右中指２","rMid2"  },
                { "右中指３","rMid3"  },
                { "右薬指１","rRing1" },
                { "右薬指２","rRing2" },
                { "右薬指３","rRing3" },
                { "右小指１","rPinky1"},
                { "右小指２","rPinky2"},
                { "右小指３","rPinky3"},
        };
        public static Dictionary<string, string> boneNames = new Dictionary<string, string>() {
        //todo
                { "全ての親","Genesis2Female"},
                { "センター","Genesis2Female|hip|0.8"},//hipControl
                { "グルーブ","Genesis2Female|hip|0.8"},//hipControl

                { "舌親","tongueBase"},
                { "舌1","tongue01"},
                { "舌2","tongue02"},
                { "舌3","tongue03"},
                { "舌4","tongue04"},
                { "舌5","tongue05"},
                { "舌6","tongueTip"},
                { "左目","lEye"},
                { "右目","rEye"},
                { "右胸上2","rPectoral"},
                { "左胸上2","lPectoral"},


                {"upperJaw","upperJaw" },
                {"lowerJaw","lowerJaw" },

                { "腰","hip"},//hipControl
				{ "下半身","pelvis"},//pelvisControl

				{ "上半身","abdomen"},//abdomenControl
				{ "上半身2","abdomen2"},//abdomen2Control
				{ "上半身3","chest"},//chestControl

				{ "首", "neck"},//neckControl
				{ "頭","head"},//headControl

				{ "左肩","lCollar"},//lShoulderControl
				{ "右肩","rCollar"},//rShoulderControl
				{ "左腕","lShldr"},//lArmControl
				{ "右腕","rShldr"},//rArmControl
				{ "左ひじ","lForeArm"},//lElbowControl
				{ "右ひじ","rForeArm"},//rElbowControl
				{ "左手首","lHand"},//lHandControl
				{ "右手首","rHand"},//rHandControl

				{ "左足D","lThigh"},//lThighControl
				{ "右足D","rThigh"},//rThighControl
				{ "左ひざD","lShin"},//lKneeControl
				{ "右ひざD","rShin"},//rKneeControl
				{ "左足首D","lFoot"},//lFootControl
				{ "右足首D","rFoot"},//rFootControl
				{ "左足先EX","lToe"},//lToeControl
				{ "右足先EX","rToe"},//rToeControl

                { "左足","lThigh"},
                { "左ひざ","lShin"},
                { "左足首","lFoot"},
                { "左つま先","lToe"},
                { "lBigToe","lBigToe"},
                { "lSmallToe1","lSmallToe1"},
                { "lSmallToe2","lSmallToe2"},
                { "lSmallToe3","lSmallToe3"},
                { "lSmallToe4","lSmallToe4"},

                { "右足","rThigh"},
                { "右ひざ","rShin"},
                { "右足首","rFoot"},
                { "右つま先","rToe"},
                { "rBigToe","rBigToe"},
                { "rSmallToe1","rSmallToe1"},
                { "rSmallToe2","rSmallToe2"},
                { "rSmallToe3","rSmallToe3"},
                { "rSmallToe4","rSmallToe4"},

                { "左足IK親","lFoot"},
                { "左足ＩＫ","lFoot"},
                { "左つま先ＩＫ","lToe"},

                { "右足IK親","rFoot"},
                { "右足ＩＫ","rFoot"},
                { "右つま先ＩＫ","rToe"},

                { "右腕捩","rShldr|rForeArm|0.5"},//rArmControl
				{ "右腕捩1","rShldr|rForeArm|0.25"},//rArmControl
				{ "右腕捩2","rShldr|rForeArm|0.5"},//rArmControl
				{ "右腕捩3","rShldr|rForeArm|0.75"},//rArmControl
                { "右肩P","rCollar"},//rShoulderControl
				{ "右肩C","rCollar"},//rShoulderControl
                { "右手捩","rForeArm|rHand|0.5"},//rElbowControl
                { "右手捩1","rForeArm|rHand|0.25"},//rElbowControl
                { "右手捩2","rForeArm|rHand|0.5"},//rElbowControl
                { "右手捩3","rForeArm|rHand|0.75"},//rElbowControl

                { "左腕捩","lShldr|lForeArm|0.5"},//rArmControl
				{ "左腕捩1","lShldr|lForeArm|0.25"},//rArmControl
				{ "左腕捩2","lShldr|lForeArm|0.5"},//rArmControl
				{ "左腕捩3","lShldr|lForeArm|0.75"},//rArmControl
                { "左肩P","lCollar"},//rShoulderControl
				{ "左肩C","lCollar"},//rShoulderControl
                { "左手捩","lForeArm|lHand|0.5"},//rElbowControl
                { "左手捩1","lForeArm|lHand|0.25"},//rElbowControl
                { "左手捩2","lForeArm|lHand|0.5"},//rElbowControl
                { "左手捩3","lForeArm|lHand|0.75"},//rElbowControl


                {"rCarpal1","rCarpal1" },
                {"rCarpal2","rCarpal2" },
                {"lCarpal1","lCarpal1" },
                {"lCarpal2","lCarpal2" },
				//手指动作

                {"右ダミー","rCarpal1|rMid1|0.5" },
                {"左ダミー","lCarpal1|lMid1|0.5" },


                { "左親指０","lThumb1"},
                { "左親指１","lThumb2"},
                { "左親指２","lThumb3"},
                { "左人指１","lIndex1"},
                { "左人指２","lIndex2"},
                { "左人指３","lIndex3"},
                { "左中指１","lMid1"},
                { "左中指２","lMid2"},
                { "左中指３","lMid3"},
                { "左薬指１","lRing1" },
                { "左薬指２","lRing2" },
                { "左薬指３","lRing3" },
                { "左小指１","lPinky1"},
                { "左小指２","lPinky2"},
                { "左小指３","lPinky3"},

                { "右親指０","rThumb1"},
                { "右親指１","rThumb2"},
                { "右親指２","rThumb3"},
                { "右人指１","rIndex1"},
                { "右人指２","rIndex2"},
                { "右人指３","rIndex3"},
                { "右中指１","rMid1"  },
                { "右中指２","rMid2"  },
                { "右中指３","rMid3"  },
                { "右薬指１","rRing1" },
                { "右薬指２","rRing2" },
                { "右薬指３","rRing3" },
                { "右小指１","rPinky1"},
                { "右小指２","rPinky2"},
                { "右小指３","rPinky3"},
        };
        public static HashSet<string> ignoreUpdateBoneNames = new HashSet<string>()
        {
            "舌親","舌1","舌2","舌3","舌4","舌5","舌6","左目","右目",
            "右胸上2","左胸上2",//mmd没有控制胸的，必须忽略
            "upperJaw","lowerJaw",
            //"右ダミー","左ダミー",

            //手部的这几根骨骼，如果设置位置的话，会跳的很厉害
            "rCarpal1","rCarpal2",
            "lCarpal1","lCarpal2",
            //"左親指０","左親指１","左親指２",
            //"左人指１","左人指２","左人指３",
            //"左中指１","左中指２","左中指３",
            //"左薬指１","左薬指２","左薬指３",
            //"左小指１","左小指２","左小指３",
            //"右親指０","右親指１","右親指２",
            //"右人指１","右人指２","右人指３",
            //"右中指１","右中指２","右中指３",
            //"右薬指１","右薬指２","右薬指３",
            //"右小指１","右小指２","右小指３",
        };

        public static HashSet<string> armBones = new HashSet<string>()
    {
        "左肩","左腕","左ひじ","左手首",
        "右肩","右腕","右ひじ","右手首",
        "rCarpal1","rCarpal2",
        "lCarpal1","lCarpal2",
                "左親指０","左親指１","左親指２",
        "左人指１","左人指２","左人指３",
        "左中指１","左中指２","左中指３",
        "左薬指１","左薬指２","左薬指３",
        "左小指１","左小指２","左小指３",
        "右親指０","右親指１","右親指２",
        "右人指１","右人指２","右人指３",
        "右中指１","右中指２","右中指３",
        "右薬指１","右薬指２","右薬指３",
        "右小指１","右小指２","右小指３",
    };

        public static HashSet<string> fingerBoneNames = new HashSet<string> {
        "左親指０","左親指１","左親指２",
        "左人指１","左人指２","左人指３",
        "左中指１","左中指２","左中指３",
        "左薬指１","左薬指２","左薬指３",
        "左小指１","左小指２","左小指３",
        "右親指０","右親指１","右親指２",
        "右人指１","右人指２","右人指３",
        "右中指１","右中指２","右中指３",
        "右薬指１","右薬指２","右薬指３",
        "右小指１","右小指２","右小指３",
    };
        public static HashSet<string> thumbBoneNames = new HashSet<string> {
        "左親指０","左親指１","左親指２",
        "右親指０","右親指１","右親指２",
    };
        public static Transform SearchObjName(Transform root, string name)
        {
            GameObject gameObject = Utility.FindLoop(root, name);
            if (gameObject != null)
            {
                return gameObject.transform;
            }
            if (name == "Genesis2Female")
            {
                gameObject = Utility.FindLoop(root, "Genesis2Male");
                if (gameObject != null)
                {
                    return gameObject.transform;
                }
            }
            Debug.Log("Try to find " + name + " from " + root.name + " but not found.");
            return null;
        }

        static Quaternion quat = new Quaternion(0, 1, 0, 0);
    }
}

