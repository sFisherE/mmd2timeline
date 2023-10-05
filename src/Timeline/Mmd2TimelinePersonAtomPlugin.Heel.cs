using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using SimpleJSON;
using LibMMD.Unity3D;
using UnityEngine.UI;
using MVR.FileManagementSecure;
using LibMMD.Motion;
using LibMMD.Reader;
using LibMMD.Util;
namespace mmd2timeline
{
    partial class Mmd2TimelinePersonAtomPlugin : MVRScript
    {
        public List<UIDynamic> enableHeelUIElements = new List<UIDynamic>();
        public JSONStorableBool enableHeel;

        public JSONStorableFloat footJointDriveXTargetAdjust;
        public JSONStorableFloat toeJointDriveXTargetAdjust;
        public JSONStorableFloat holdRotationMaxForceAdjust;

        void InitHeelUI()
        {
            Debug.Log("InitHeelUI");

            CreateToggle(enableHeel);
            enableHeelUIElements.Add(CreateSlider(holdRotationMaxForceAdjust, false));
            enableHeelUIElements.Add(CreateSlider(footJointDriveXTargetAdjust, false));
            enableHeelUIElements.Add(CreateSlider(toeJointDriveXTargetAdjust, false));
        }


        void EnableHighHeel(bool v)
        {
            foreach (var item in enableHeelUIElements)
            {
                item.gameObject.SetActive(v);
            }
            foreach (var item in containingAtom.freeControllers)
            {
                if (item.name == "lToeControl" || item.name == "rToeControl")
                {
                    var p2 = item.GetFloatJSONParam("jointDriveXTarget");
                    if (enableHeel.val)
                    {
                        item.currentRotationState = FreeControllerV3.RotationState.Off;
                        item.currentPositionState = FreeControllerV3.PositionState.Off;

                        p2.val = toeJointDriveXTargetAdjust.val;
                    }
                    else
                    {
                        item.currentRotationState = FreeControllerV3.RotationState.On;
                        item.currentPositionState = FreeControllerV3.PositionState.On;


                        item.transform.localPosition = item.startingLocalPosition;
                        item.transform.localRotation = item.startingLocalRotation;

                        p2.val = p2.defaultVal;
                    }
                }
                else if (item.name == "lFootControl" || item.name == "rFootControl")
                {
                    var p1 = item.GetFloatJSONParam("holdRotationMaxForce");
                    var p2 = item.GetFloatJSONParam("jointDriveXTarget");
                    if (enableHeel.val)
                    {
                        p1.val = holdRotationMaxForceAdjust.val;
                        p2.val = footJointDriveXTargetAdjust.val;
                    }
                    else
                    {
                        p1.val = p1.defaultVal;
                        p2.val = p2.defaultVal;
                    }
                }
            }

        }

        public void SetJointDriveXAngle(float val)
        {
            foreach (var item in containingAtom.freeControllers)
            {
                if (item.name == "lFootControl" || item.name == "rFootControl")
                {
                    var p1 = item.GetFloatJSONParam("holdRotationMaxForce");
                    var p2 = item.GetFloatJSONParam("jointDriveXTarget");
                    if (enableHeel.val)
                    {
                        p1.val = holdRotationMaxForceAdjust.val;
                        p2.val = footJointDriveXTargetAdjust.val;
                    }
                }
                else if(item.name=="lToeControl" || item.name == "rToeControl")
                {
                    var p2 = item.GetFloatJSONParam("jointDriveXTarget");
                    if (enableHeel.val)
                    {
                        p2.val = toeJointDriveXTargetAdjust.val;
                    }
                }
            }
        }
    }
}
