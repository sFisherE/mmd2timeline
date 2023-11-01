using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace mmd2timeline
{
    internal partial class MotionHelper
    {
        internal string lowestControlName = "";
        /// <summary>
        /// 跪姿修正
        /// </summary>
        bool kneeFixed = false;
        /// <summary>
        /// 左膝修正
        /// </summary>
        bool lKneeFixed = false;
        /// <summary>
        /// 右膝修正
        /// </summary>
        bool rKneeFixed = false;

        bool heelFixed = false;

        /// <summary>
        /// 身体部位高度修正值字典
        /// </summary>
        Dictionary<string, float> controlFixValues = new Dictionary<string, float>
        {
            { "lFootControl",  0.0637f },
            { "rFootControl",  0.0637f },
            { "hipControl",  0.0642f },
            { "pelvisControl", 0.0727f },
            { "chestControl",  0.0435f },
            { "headControl",  0.135f },
            { "lHandControl",  0.03f },
            { "rHandControl",  0.03f },
            { "neckControl",  0.12f },
            { "lKneeControl",  0.06f },
            { "rKneeControl",  0.06f },
            { "lThighControl",  0.09f },
            { "rThighControl",  0.09f },
            { "abdomenControl", 0.05f },
            { "abdomen2Control",  0.05f },
        };

        /// <summary>
        /// 根据骨骼部位情况获得修正高度
        /// </summary>
        /// <param name="bones"></param>
        /// <param name="floorHeight"></param>
        /// <param name="horizon"></param>
        /// <returns></returns>
        private float GetFixHeight(GameObject[] bones)
        {
            var reviseY = 0f;

            //var wholeBodyFix = config.AutoFixHeightMode == AutoCorrectHeightMode.WholeBody;

            if (EnableHeel)
            {
                try
                {
                    // 查找高度最低的骨骼
                    var lowestBones = bones.Where(b =>
                    {
                        var name = validBoneNames[b.name];

                        // 忽略手部、膝盖、脚趾关节
                        if (string.IsNullOrEmpty(name) || name.EndsWith("HandControl")/* || name.EndsWith("ToeControl")*/)
                        {
                            return false;
                        }

                        return true;
                    }).OrderBy(b => b.transform.position.y).ToList();

                    var lowestBoneName = "";
                    heelFixed = false;

                    var lowestBone = lowestBones.FirstOrDefault();

                    // Y轴最小的骨骼名称
                    lowestBoneName = lowestBone.name;

                    if (validBoneNames.TryGetValue(lowestBoneName, out lowestControlName))
                    {
                        // 当启用高跟并且最低的控制器是脚趾，进行高跟高度修正的计算
                        if (lowestControlName.EndsWith("ToeControl")|| lowestControlName.EndsWith("FootControl"))
                        {
                            return _HeelHeightAdjustJSON.val;
                        }
                    }
                }
                catch (Exception e)
                {
                    LogUtil.LogError(e, $"FIX:::");
                }
            }
            else
            {
                reviseY = 0f;
            }

            return reviseY;
        }

        /// <summary>
        /// 释放脚部
        /// </summary>
        /// <param name="controller"></param>
        /// <param name="position"></param>
        /// <param name="right"></param>
        bool FreeFoot(FreeControllerV3 controller, Vector3 position, bool right = false)
        {
            var start = "l";

            if (right)
            {
                start = "r";
            }
            bool isOff = false;

            if (controller.name == $"{start}FootControl")
            {
                var knee = this.controllerLookup.Values.FirstOrDefault(c => c.name.EndsWith($"{start}KneeControl"));
                var thigh = this.controllerLookup.Values.FirstOrDefault(c => c.name.EndsWith($"{start}ThighControl"));

                var jointDriveXTargetJSON = controller.GetFloatJSONParam("jointDriveXTarget");
                var holdForceJSON = controller.GetFloatJSONParam("holdRotationMaxForce");

                // 如果脚部的Y轴高于设定高度
                if (position.y > config.FootFixHeight && config.EnableFootFree)
                {
                    if (!EnableHeel)
                    {
                        // 设定关节驱动X角
                        jointDriveXTargetJSON.val = config.FreeFootJointDriveXTarget * 1f;
                        holdForceJSON.val = config.FreeFootHoldRotationMaxForce;
                    }

                    // 如果脚部的高度高于设定值1.5倍
                    if (position.y > config.FootOffHeight)
                    {
                        if (config.EnableFootOff)
                        {
                            // 关闭脚部位置和旋转
                            controller.currentPositionState = FreeControllerV3.PositionState.Off;
                            controller.currentRotationState = FreeControllerV3.RotationState.Off;
                            isOff = true;
                        }
                        else
                        {
                            controller.currentPositionState = config.MotionPositionState;
                            controller.currentRotationState = config.MotionRotationState;
                        }
                        SetHoldMaxForce(knee, config.FreeKneeHoldRotationMaxForce);

                        // 如果脚部高度高于设定值2倍
                        if (position.y > config.FootOffHeight * 1.5f)
                        {
                            if (config.EnableKneeOff)
                            {
                                knee.currentPositionState = FreeControllerV3.PositionState.Off;
                                knee.currentRotationState = FreeControllerV3.RotationState.Off;
                                isOff = true;
                            }
                            else
                            {
                                knee.currentPositionState = config.MotionPositionState;
                                knee.currentRotationState = config.MotionRotationState;
                            }
                            SetHoldMaxForce(thigh, config.FreeThighHoldRotationMaxForce);
                        }
                    }
                }
                else
                {
                    controller.currentPositionState = config.MotionPositionState;
                    controller.currentRotationState = config.MotionRotationState;
                    if (!EnableHeel)
                    {
                        jointDriveXTargetJSON.SetValToDefault();
                        holdForceJSON.SetValToDefault();
                    }
                    SetHoldMaxForce(knee, null);
                    knee.currentPositionState = config.MotionPositionState;
                    knee.currentRotationState = config.MotionRotationState;
                    SetHoldMaxForce(thigh, null);
                }
            }

            return isOff;
        }

        /// <summary>
        /// 释放脚趾
        /// </summary>
        /// <param name="controller"></param>
        /// <param name="position"></param>
        /// <param name="right"></param>
        bool FreeToe(FreeControllerV3 controller, Vector3 position, bool right = false)
        {
            var start = "l";

            if (right)
            {
                start = "r";
            }
            var isOff = false;
            if (controller.name == $"{start}ToeControl")
            {
                var foot = this.controllerLookup.Values.FirstOrDefault(c => c.name.EndsWith($"{start}FootControl"));

                var jointDriveXTargetJSON = foot.GetFloatJSONParam("jointDriveXTarget");
                var holdForceJSON = foot.GetFloatJSONParam("holdRotationMaxForce");

                //如果脚趾高度 大于设定的脚趾高度，则脚趾的位置和旋转关闭，脚部的X轴旋转
                if (position.y > config.ToeOffHeight && config.EnableFootFree)
                {
                    controller.currentPositionState = FreeControllerV3.PositionState.Off;
                    controller.currentRotationState = FreeControllerV3.RotationState.Off;

                    isOff = true;

                    var rate = 1f;
                    if (position.y < config.FootFixHeight)
                    {
                        rate = position.y / config.FootFixHeight;
                    }

                    jointDriveXTargetJSON.val = config.FreeFootJointDriveXTarget * rate;
                    holdForceJSON.val = config.FreeFootHoldRotationMaxForce;
                }
                else
                {
                    controller.currentPositionState = config.MotionPositionState;
                    controller.currentRotationState = config.MotionRotationState;
                    jointDriveXTargetJSON.SetValToDefault();
                    holdForceJSON.SetValToDefault();
                }
            }
            return isOff;
        }
    }
}
