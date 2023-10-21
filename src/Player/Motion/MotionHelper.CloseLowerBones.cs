using System.Collections.Generic;

namespace mmd2timeline
{
    /// <summary>
    /// 人物动作控制器。
    /// 关闭下半身骨骼。
    /// 此功能的创意和初始代码来自于：洋之上（uugg账号：shangdi1234）
    /// </summary>
    internal partial class MotionHelper
    {
        /// <summary>
        /// 下身控制器名称
        /// </summary>
        List<string> lowerControls = new List<string> {

            "Thigh",
            "Foot",
            "Toe",
            "Knee",
            "hip"
        };

        /// <summary>
        /// 下身骨骼字典
        /// </summary>
        Dictionary<string, string> lowerBones = new Dictionary<string, string>
        {
            { "センター","hip"},//hipControl
            { "グルーブ","hip"},//hipControl
            {
                "左足D",
                "lThigh"
            },
            {
                "右足D",
                "rThigh"
            },
            {
                "左ひざD",
                "lShin"
            },
            {
                "右ひざD",
                "rShin"
            },
            {
                "左足首D",
                "lFoot"
            },
            {
                "右足首D",
                "rFoot"
            },
            {
                "左足先EX",
                "lToe"
            },
            {
                "右足先EX",
                "rToe"
            },
            {
                "左足",
                "lThigh"
            },
            {
                "左ひざ",
                "lShin"
            },
            {
                "左足首",
                "lFoot"
            },
            {
                "左つま先",
                "lToe"
            },
            {
                "lBigToe",
                "lBigToe"
            },
            {
                "lSmallToe1",
                "lSmallToe1"
            },
            {
                "lSmallToe2",
                "lSmallToe2"
            },
            {
                "lSmallToe3",
                "lSmallToe3"
            },
            {
                "lSmallToe4",
                "lSmallToe4"
            },
            {
                "右足",
                "rThigh"
            },
            {
                "右ひざ",
                "rShin"
            },
            {
                "右足首",
                "rFoot"
            },
            {
                "右つま先",
                "rToe"
            },
            {
                "rBigToe",
                "rBigToe"
            },
            {
                "rSmallToe1",
                "rSmallToe1"
            },
            {
                "rSmallToe2",
                "rSmallToe2"
            },
            {
                "rSmallToe3",
                "rSmallToe3"
            },
            {
                "rSmallToe4",
                "rSmallToe4"
            },
            {
                "左足IK親",
                "lFoot"
            },
            {
                "左足ＩＫ",
                "lFoot"
            },
            {
                "左つま先ＩＫ",
                "lToe"
            },
            {
                "右足IK親",
                "rFoot"
            },
            {
                "右足ＩＫ",
                "rFoot"
            },
            {
                "右つま先ＩＫ",
                "rToe"
            }
        };

        /// <summary>
        /// 关闭下半身骨骼
        /// </summary>
        /// <param name="close"></param>
        private void CloseLowerBones(bool close)
        {
            if (close)
            {
                _MotionScaleJSON.val = 0.1f;
            }
            else
            {
                if (_MotionSetting != null)
                {
                    _MotionScaleJSON.val = _MotionSetting.MotionScale;
                }
                else
                {
                    _MotionScaleJSON.val = 1f;
                }
            }

            SetLowerBonesComply(close);
        }

        /// <summary>
        /// 设置下半身骨骼为遵从
        /// </summary>
        private void SetLowerBonesComply(bool close)
        {
            foreach (var bone in lowerBones)
            {
                var cacheBoneKey = GetCacheBoneKey(bone.Key);

                if (cachedBoneLookup.ContainsKey(cacheBoneKey))
                {
                    var transform = cachedBoneLookup[cacheBoneKey];

                    if (this.controllerLookup.ContainsKey(transform))
                    {
                        var controller = this.controllerLookup[transform];
                        if (close)
                        {
                            controller.currentPositionState = FreeControllerV3.PositionState.Comply;
                            controller.currentRotationState = FreeControllerV3.RotationState.Comply;
                        }
                        else
                        {
                            controller.currentPositionState = config.MotionPositionState;
                            controller.currentRotationState = config.MotionRotationState;
                        }
                    }
                }
            }
        }
    }
}
