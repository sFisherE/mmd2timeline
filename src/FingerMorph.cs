using System;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace mmd2timeline
{
    class FingerMorph
    {
        public static string[] StorableNames= new string[] {
            "LeftHandFingerControl","RightHandFingerControl",
        };
        public static HashSet<string> setting = new HashSet<string>()
        {
            "indexProximalBend",
            "indexProximalSpread",
            "indexProximalTwist",
            "indexMiddleBend",
            "indexDistalBend",
            "middleProximalBend",
            "middleProximalSpread",
            "middleProximalTwist",
            "middleMiddleBend",
            "middleDistalBend",
            "ringProximalBend",
            "ringProximalSpread",
            "ringProximalTwist",
            "ringMiddleBend",
            "ringDistalBend",
            "pinkyProximalBend",
            "pinkyProximalSpread",
            "pinkyProximalTwist",
            "pinkyMiddleBend",
            "pinkyDistalBend",
            //大拇指最后处理，好像没用，主要timeline是先处理大拇指的。
            "thumbProximalBend",
            "thumbProximalSpread",
            "thumbProximalTwist",
            "thumbMiddleBend",
            "thumbDistalBend",
        };
        class MorphSetting
        {
            public string Name;
            public DAZMorphFormula[] Formulas;
        }

        public DAZMorphFormula CreateDAZMorphFormula(DAZMorphFormulaTargetType targetType, string target, float multiplier)
        {
            return new DAZMorphFormula() { 
                targetType = targetType, 
                target = target, 
                multiplier = multiplier, 
            };
        }
        //Pose Controls/Hands/Right/Fingers
        public void InitRight()
        {
            //Right Thumb Bend 大拇指
            var morph1 = new MorphSetting()
            {
                Name = "Right Thumb Bend",
                Formulas = new DAZMorphFormula[]
                {
                    CreateDAZMorphFormula(DAZMorphFormulaTargetType.RotationY,"rThumb1",-45),
                    CreateDAZMorphFormula(DAZMorphFormulaTargetType.RotationY,"rThumb2",-40),
                    CreateDAZMorphFormula(DAZMorphFormulaTargetType.RotationY,"rThumb3",-68),
                }
            };
            //Right Ring Finger Bend 无名指
            var morph2 = new MorphSetting()
            {
                Name = "Right Ring Finger Bend",
                Formulas = new DAZMorphFormula[]
                {
                    CreateDAZMorphFormula(DAZMorphFormulaTargetType.RotationZ,"rRing1",61),
                    CreateDAZMorphFormula(DAZMorphFormulaTargetType.RotationZ,"rRing2",90),
                    CreateDAZMorphFormula(DAZMorphFormulaTargetType.RotationZ,"rRing3",65),
                }
            };
            //Right Pinky Finger Bend 小指
            var morph3 = new MorphSetting()
            {
                Name = "Right Pinky Finger Bend",
                Formulas = new DAZMorphFormula[]
                {
                    CreateDAZMorphFormula(DAZMorphFormulaTargetType.RotationZ,"rPinky1",61),
                    CreateDAZMorphFormula(DAZMorphFormulaTargetType.RotationZ,"rPinky2",90),
                    CreateDAZMorphFormula(DAZMorphFormulaTargetType.RotationZ,"rPinky3",75),
                }
            };
            //Right Mid Finger Bend 中指
            var morph4 = new MorphSetting()
            {
                Name = "Right Mid Finger Bend",
                Formulas = new DAZMorphFormula[]
                {
                    CreateDAZMorphFormula(DAZMorphFormulaTargetType.RotationZ,"rMid1",61),
                    CreateDAZMorphFormula(DAZMorphFormulaTargetType.RotationZ,"rMid2",90),
                    CreateDAZMorphFormula(DAZMorphFormulaTargetType.RotationZ,"rMid3",57),
                }
            };
            //Right Index Finger Bend 食指
            var morph5 = new MorphSetting()
            {
                Name = "Right Index Finger Bend",
                Formulas = new DAZMorphFormula[]
                {
                    CreateDAZMorphFormula(DAZMorphFormulaTargetType.RotationZ,"rIndex1",61),
                    CreateDAZMorphFormula(DAZMorphFormulaTargetType.RotationZ,"rIndex2",100),
                    CreateDAZMorphFormula(DAZMorphFormulaTargetType.RotationZ,"rIndex3",62),
                }
            };
        }
        public void InitLeft()
        {
            //Left Thumb Bend 大拇指
            var morph1 = new MorphSetting()
            {
                Name = "Left Thumb Bend",
                Formulas = new DAZMorphFormula[]
                {
                    CreateDAZMorphFormula(DAZMorphFormulaTargetType.RotationY,"lThumb1",45),
                    CreateDAZMorphFormula(DAZMorphFormulaTargetType.RotationY,"lThumb2",40),
                    CreateDAZMorphFormula(DAZMorphFormulaTargetType.RotationY,"lThumb3",68),
                }
            };
            //Left Ring Finger Bend 无名指
            var morph2 = new MorphSetting()
            {
                Name = "Left Ring Finger Bend",
                Formulas = new DAZMorphFormula[]
                {
                    CreateDAZMorphFormula(DAZMorphFormulaTargetType.RotationZ,"lRing1",-61),
                    CreateDAZMorphFormula(DAZMorphFormulaTargetType.RotationZ,"lRing2",-90),
                    CreateDAZMorphFormula(DAZMorphFormulaTargetType.RotationZ,"lRing3",-65),
                }
            };
            //Left Pinky Finger Bend 小指
            var morph3 = new MorphSetting()
            {
                Name = "Left Pinky Finger Bend",
                Formulas = new DAZMorphFormula[]
                {
                    CreateDAZMorphFormula(DAZMorphFormulaTargetType.RotationZ,"lPinky1",-61),
                    CreateDAZMorphFormula(DAZMorphFormulaTargetType.RotationZ,"lPinky2",-90),
                    CreateDAZMorphFormula(DAZMorphFormulaTargetType.RotationZ,"lPinky3",-75),
                }
            };
            //Left Mid Finger Bend 中指
            var morph4 = new MorphSetting()
            {
                Name = "Left Mid Finger Bend",
                Formulas = new DAZMorphFormula[]
                {
                    CreateDAZMorphFormula(DAZMorphFormulaTargetType.RotationZ,"lMid1",-61),
                    CreateDAZMorphFormula(DAZMorphFormulaTargetType.RotationZ,"lMid2",-90),
                    CreateDAZMorphFormula(DAZMorphFormulaTargetType.RotationZ,"lMid3",-57),
                }
            };
            //Left Index Finger Bend 食指
            var morph5 = new MorphSetting()
            {
                Name = "Left Index Finger Bend",
                Formulas = new DAZMorphFormula[]
                {
                    CreateDAZMorphFormula(DAZMorphFormulaTargetType.RotationZ,"lIndex1",-61),
                    CreateDAZMorphFormula(DAZMorphFormulaTargetType.RotationZ,"lIndex2",-100),
                    CreateDAZMorphFormula(DAZMorphFormulaTargetType.RotationZ,"lIndex3",-62),
                }
            };

        }





       public static Vector3 ConvertToMorphRotation(DAZBone bone, ConfigurableJoint configurableJoint)
        {
            //角度
            Vector3 r = Quaternion2Angles.GetAngles(configurableJoint.targetRotation, bone.jointDriveTargetRotationOrder)* 57.29578f;

            Vector3 vector = new Vector3();
            Vector3 axis = configurableJoint.axis;
            if (axis.x == 1f)
            {
                vector.x = -r.x;
                Vector3 secondaryAxis = configurableJoint.secondaryAxis;
                if (secondaryAxis.y == 1f)
                {
                    vector.y = r.y;
                    vector.z = r.z;
                }
                else
                {
                    vector.z = r.y;
                    vector.y = r.z;
                }
            }
            else
            {
                Vector3 axis2 = configurableJoint.axis;
                if (axis2.y == 1f)
                {
                    vector.y = r.x;
                    Vector3 secondaryAxis2 = configurableJoint.secondaryAxis;
                    if (secondaryAxis2.x == 1f)
                    {
                        vector.x = -r.y;
                        vector.z = r.z;
                    }
                    else
                    {
                        vector.z = r.y;
                        vector.x = -r.z;
                    }
                }
                else
                {
                    vector.z = r.x;
                    Vector3 secondaryAxis3 = configurableJoint.secondaryAxis;
                    if (secondaryAxis3.x == 1f)
                    {
                        vector.x = -r.y;
                        vector.y = r.z;
                    }
                    else
                    {
                        vector.y = r.y;
                        vector.x = -r.z;
                    }
                }
            }
            return vector;
        }

        public static Quaternion EulerToQuaternion(Vector3 r, Quaternion2Angles.RotationOrder ro)
        {
            Quaternion quaternion = Quaternion.Euler(r.x, 0f, 0f);
            Quaternion quaternion2 = Quaternion.Euler(0f, r.y, 0f);
            Quaternion quaternion3 = Quaternion.Euler(0f, 0f, r.z);
            Quaternion result = quaternion;
            switch (ro)
            {
                case Quaternion2Angles.RotationOrder.XYZ:
                    result = quaternion * quaternion2 * quaternion3;
                    break;
                case Quaternion2Angles.RotationOrder.XZY:
                    result = quaternion * quaternion3 * quaternion2;
                    break;
                case Quaternion2Angles.RotationOrder.YXZ:
                    result = quaternion2 * quaternion * quaternion3;
                    break;
                case Quaternion2Angles.RotationOrder.YZX:
                    result = quaternion2 * quaternion3 * quaternion;
                    break;
                case Quaternion2Angles.RotationOrder.ZXY:
                    result = quaternion3 * quaternion * quaternion2;
                    break;
                case Quaternion2Angles.RotationOrder.ZYX:
                    result = quaternion3 * quaternion2 * quaternion;
                    break;
            }
            return result;
        }
    }
}
