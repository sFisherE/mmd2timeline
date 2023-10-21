using System;
using System.Collections.Generic;
using UnityEngine;
namespace mmd2timeline
{
    class Utility
    {
        public static string GameObjectHead = "mmd2timeline.";
        public static void CleanGameObjects()
        {
            var gos = UnityEngine.SceneManagement.SceneManager.GetActiveScene().GetRootGameObjects();
            foreach (var item in gos)
            {
                if (item.name.StartsWith(Utility.GameObjectHead))
                {
                    GameObject.Destroy(item);
                }
            }
        }
        public static  Quaternion quat = new Quaternion(0, 1, 0, 0);

        public static void ResetHandControl(HandControl c)
        {
            ResetFinger(c.thumbDistalBone);
            ResetFinger(c.thumbMiddlaBone);
            ResetFinger(c.thumbProximalBone);
            ResetFinger(c.indexDistalBone);
            ResetFinger(c.indexMiddlaBone);
            ResetFinger(c.indexProximalBone);
            ResetFinger(c.middleDistalBone);
            ResetFinger(c.middleMiddlaBone);
            ResetFinger(c.middleProximalBone);
            ResetFinger(c.ringDistalBone);
            ResetFinger(c.ringMiddlaBone);
            ResetFinger(c.ringProximalBone);
            ResetFinger(c.pinkyDistalBone);
            ResetFinger(c.pinkyMiddlaBone);
            ResetFinger(c.pinkyProximalBone);
        }
        public static void ResetFinger(DAZBone dazBone)
        {
            //dazBone.ResetToStartingLocalPositionRotation();

            //var joint = dazBone.GetComponent<ConfigurableJoint>();

            //joint.SetTargetRotationLocal(dazBone.transform.localRotation, dazBone.startingLocalRotation);

            //var fingerOutput = dazBone.GetComponent<MeshVR.Hands.FingerOutput>();
            //fingerOutput.ConvertRotation(dazBone, joint);

            var fingerOutput = dazBone.GetComponent<MeshVR.Hands.FingerOutput>();
            fingerOutput.currentBend = 0;
            fingerOutput.currentSpread = 0;
            fingerOutput.currentTwist = 0;
            fingerOutput.UpdateOutput();
        }

        public static void RecordController(float time, FreeControllerV3 freeController, TimelineControlJson json)
        {
            Transform target = freeController.transform;

            TimelineFrameJson x = new TimelineFrameJson(time, target.localPosition.x,"3");
            json.X.Add(x);

            TimelineFrameJson y = new TimelineFrameJson(time, target.localPosition.y,"3");
            json.Y.Add(y);

            TimelineFrameJson z = new TimelineFrameJson(time, target.localPosition.z,"3");
            json.Z.Add(z);

            TimelineFrameJson rx = new TimelineFrameJson(time, target.localRotation.x,"3");
            json.RotX.Add(rx);

            TimelineFrameJson ry = new TimelineFrameJson(time, target.localRotation.y,"3");
            json.RotY.Add(ry);

            TimelineFrameJson rz = new TimelineFrameJson(time, target.localRotation.z,"3");
            json.RotZ.Add(rz);

            TimelineFrameJson rw = new TimelineFrameJson(time, target.localRotation.w,"3");
            json.RotW.Add(rw);
        }

        public static GameObject FindLoop(Transform root, string name)
        {
            if (root.name == name)
            {
                return root.gameObject;
            }
            for (int i = 0; i < root.childCount; i++)
            {
                GameObject gameObject = FindLoop(root.GetChild(i), name);
                if (gameObject != null)
                {
                    return gameObject;
                }
            }
            return null;
        }

    }
}
