﻿using System.Linq;

namespace mmd2timeline
{
    internal partial class Settings : BaseScript
    {
        /// <summary>
        /// 弹簧滑块
        /// </summary>
        JSONStorableFloat springSlider;
        /// <summary>
        /// 阻尼滑块
        /// </summary>
        JSONStorableFloat damperSlider;

        /// <summary>
        /// 根据动作模式设置弹簧和阻尼的值
        /// </summary>
        /// <param name="v"></param>
        void SetValueByMode(int v)
        {
            LogUtil.Debug($"SetValueByMode::{v}");

            if (springSlider == null || damperSlider == null)
            {
                return;
            }

            if (config.MotionModes.Any(m => m.Mode == v))
            {
                var mode = config.MotionModes.FirstOrDefault(m => m.Mode == v);

                springSlider.val = mode.Spring;
                damperSlider.val = mode.Damper;
            }
        }

        /// <summary>
        /// 设定场景中所有人物的关节弹簧百分比
        /// </summary>
        /// <param name="v"></param>
        void SetAllPersonJointsSpringPercent(float v)
        {
            var persons = SuperController.singleton.GetAtoms().Where(a => a.type == "Person").ToArray();

            foreach (var person in persons)
            {
                var allJointsController = person.GetComponentInChildren<AllJointsController>();

                var springPercentJSON = allJointsController.GetFloatJSONParam("springPercent");
                springPercentJSON.val = v;
                allJointsController.SetAllJointsPercentHoldSpring();
            }
        }

        /// <summary>
        /// 设定场景中所有人物的关节阻尼百分比
        /// </summary>
        /// <param name="v"></param>
        void SetAllPersonJointsDamperPercent(float v)
        {
            var persons = SuperController.singleton.GetAtoms().Where(a => a.type == "Person").ToArray();

            foreach (var person in persons)
            {
                var allJointsController = person.GetComponentInChildren<AllJointsController>();

                var damperPercentJSON = allJointsController.GetFloatJSONParam("damperPercent");
                damperPercentJSON.val = v;
                allJointsController.SetAllJointsPercentHoldDamper();
            }
        }
        /// <summary>
        /// 设定场景中所有人物的关节最大速度
        /// </summary>
        /// <param name="v"></param>
        void SetAllPersonJointsMaxVelocity(float v)
        {
            var persons = SuperController.singleton.GetAtoms().Where(a => a.type == "Person").ToArray();

            foreach (var person in persons)
            {
                var allJointsController = person.GetComponentInChildren<AllJointsController>();

                var maxVelocityJSON = allJointsController.GetFloatJSONParam("maxVeloctiy");

                if (maxVelocityJSON == null)
                {
                    maxVelocityJSON = allJointsController.GetFloatJSONParam("maxVelocity");
                }

                maxVelocityJSON.val = v;
                allJointsController.SetAllJointsMaxVelocity();
            }
        }

        void SetAllPersonPhysicsMesh(string name, bool on)
        {
            var persons = SuperController.singleton.GetAtoms().Where(a => a.type == "Person").ToArray();

            foreach (var person in persons)
            {
                var characterRun = person.GetComponentInChildren<DAZCharacterRun>();

                foreach (var mesh in characterRun.physicsMeshes)
                {
                    if (name == mesh.name)
                    {
                        mesh.on = on;

                        break;
                    }
                }
            }
        }
    }
}
