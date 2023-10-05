using MeshVR.Hands;
using UnityEngine;
namespace mmd2timeline
{
    static class FingerOutputExt
    {
        public static void ConvertRotation(this FingerOutput fingerOutput, DAZBone bone, ConfigurableJoint joint)
        {
            //fingerOutput.twistEnabled = true;
            //fingerOutput.spreadEnabled = true;
            //fingerOutput.bendEnabled = true;
            //var bone = fingerOutput.GetComponent<DAZBone>();
            Vector3 euler = Quaternion2Angles.GetAngles(joint.targetRotation, bone.jointDriveTargetRotationOrder) * 57.29578f;

            //var euler = joint.targetRotation.eulerAngles;

            if (fingerOutput.twistEnabled)
            {
                float num3 = 0;
                switch (fingerOutput.twistAxis)
                {
                    case Finger.Axis.X:
                        num3 = euler.x;
                        break;
                    case Finger.Axis.NegX:
                        num3 = -euler.x;
                        break;
                    case Finger.Axis.Y:
                        num3 = euler.y;
                        break;
                    case Finger.Axis.NegY:
                        num3 = -euler.y;
                        break;
                    case Finger.Axis.Z:
                        num3 = euler.z;
                        break;
                    case Finger.Axis.NegZ:
                        num3 = -euler.z;
                        break;
                }
                fingerOutput.currentTwist = num3 - fingerOutput.twistOffset;
            }
            if (fingerOutput.spreadEnabled)
            {
                float num2 = 0;
                switch (fingerOutput.spreadAxis)
                {
                    case Finger.Axis.X:
                        num2 = euler.x;
                        break;
                    case Finger.Axis.NegX:
                        num2 = -euler.x;
                        break;
                    case Finger.Axis.Y:
                        num2 = euler.y;
                        break;
                    case Finger.Axis.NegY:
                        num2 = -euler.y;
                        break;
                    case Finger.Axis.Z:
                        num2 = euler.z;
                        break;
                    case Finger.Axis.NegZ:
                        num2 = -euler.z;
                        break;
                }
                fingerOutput.currentSpread = num2 - fingerOutput.spreadOffset;
            }
            if (fingerOutput.bendEnabled)
            {
                float num = fingerOutput.currentBend + fingerOutput.bendOffset;
                switch (fingerOutput.bendAxis)
                {
                    case Finger.Axis.X:
                        num = euler.x;
                        break;
                    case Finger.Axis.NegX:
                        num = -euler.x;
                        break;
                    case Finger.Axis.Y:
                        num = euler.y;
                        break;
                    case Finger.Axis.NegY:
                        num = -euler.y;
                        break;
                    case Finger.Axis.Z:
                        num = euler.z;
                        break;
                    case Finger.Axis.NegZ:
                        num = -euler.z;
                        break;
                }
                fingerOutput.currentBend =num- fingerOutput.bendOffset;
            }

        }
    }
}
