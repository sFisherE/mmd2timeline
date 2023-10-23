using SimpleJSON;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace mmd2timeline.Store
{
    /// <summary>
    /// 人物动作设置
    /// </summary>
    internal class PersonMotion : JSONClass
    {
        internal PersonMotion()
        {
        }

        List<string> _Files = new List<string>();

        /// <summary>
        /// 动作文件路径
        /// </summary>
        public List<string> Files
        {
            get
            {
                return _Files;
            }
            set
            {
                _Files = value;
            }
        }

        /// <summary>
        /// 忽略脸部
        /// </summary>
        public bool IgnoreFace
        {
            get
            {
                if (this.HasKey("IgnoreFace"))
                    return this["IgnoreFace"].AsBool;
                else return false;
            }
            set
            {
                this["IgnoreFace"].AsBool = value;
            }
        }

        /// <summary>
        /// 直腿. 0-1
        /// </summary>
        public float StraightLeg
        {
            get
            {
                if (this.HasKey("StraightLeg"))
                    return this["StraightLeg"].AsFloat;
                else return 0f;
            }
            set { this["StraightLeg"].AsFloat = value; }
        }

        /// <summary>
        /// 直腿工作角度 60-170
        /// </summary>
        public float StraightLegWorkAngle
        {
            get
            {
                if (this.HasKey("StraightLegWorkAngle"))
                    return this["StraightLegWorkAngle"].AsFloat;
                else
                    return 140f;
            }
            set { this["StraightLegWorkAngle"].AsFloat = value; }
        }

        #region 人物位置设置（此数据为运行时数据，不保存）

        ///// <summary>
        ///// 位置X。-1 - 1
        ///// </summary>
        //public float PositionX
        //{
        //    get
        //    {
        //        if (this.HasKey("PositionX"))
        //            return this["PositionX"].AsFloat;
        //        else
        //            return 0f;
        //    }
        //    set { this["PositionX"].AsFloat = value; }
        //}

        ///// <summary>
        ///// 位置Y。-1 - 1
        ///// </summary>
        //public float PositionY
        //{
        //    get
        //    {
        //        if (this.HasKey("PositionY"))
        //            return this["PositionY"].AsFloat;
        //        else
        //            return 0f;
        //    }
        //    set { this["PositionY"].AsFloat = value; }
        //}

        ///// <summary>
        ///// 位置Z。-1 - 1
        ///// </summary>
        //public float PositionZ
        //{
        //    get
        //    {
        //        if (this.HasKey("PositionZ"))
        //            return this["PositionZ"].AsFloat;
        //        else
        //            return 0f;
        //    }
        //    set { this["PositionZ"].AsFloat = value; }
        //}

        ///// <summary>
        ///// 角度X。-1 - 1
        ///// </summary>
        //public float RotationX
        //{
        //    get
        //    {
        //        if (this.HasKey("RotationX"))
        //            return this["RotationX"].AsFloat;
        //        else
        //            return 0f;
        //    }
        //    set { this["RotationX"].AsFloat = value; }
        //}

        ///// <summary>
        ///// 角度Y。-1 - 1
        ///// </summary>
        //public float RotationY
        //{
        //    get
        //    {
        //        if (this.HasKey("RotationY"))
        //            return this["RotationY"].AsFloat;
        //        else
        //            return 0f;
        //    }
        //    set { this["RotationY"].AsFloat = value; }
        //}

        ///// <summary>
        ///// 角度Z。-1 - 1
        ///// </summary>
        //public float RotationZ
        //{
        //    get
        //    {
        //        if (this.HasKey("RotationZ"))
        //            return this["RotationZ"].AsFloat;
        //        else
        //            return 0f;
        //    }
        //    set { this["RotationZ"].AsFloat = value; }
        //}
        #endregion

        /// <summary>
        /// 动作缩放0.1-2
        /// </summary>
        public float MotionScale
        {
            get
            {
                if (this.HasKey("MotionScale"))
                    return this["MotionScale"].AsFloat;
                else return 1f;
            }
            set { this["MotionScale"].AsFloat = value; }
        }

        /// <summary>
        /// 手臂平滑调节0-1
        /// </summary>
        public float SmoothArm
        {
            get
            {
                if (this.HasKey("SmoothArm"))
                    return this["SmoothArm"].AsFloat;
                else return 0f;
            }
            set { this["SmoothArm"].AsFloat = value; }
        }

        /// <summary>
        /// 是否使用所有关节配置
        /// </summary>
        public bool UseAllJointsSettings
        {
            get
            {
                if (this.HasKey("UseAllJointsSettings"))
                    return this["UseAllJointsSettings"].AsBool;
                else return false;
            }
            set { this["UseAllJointsSettings"].AsBool = value; }
        }

        /// <summary>
        /// 所有关节弹簧比例
        /// </summary>
        public float AllJointsSpringPercent
        {
            get
            {
                if (this.HasKey("AllJointsSpringPercent"))
                    return this["AllJointsSpringPercent"].AsFloat;
                else return 0.15f;
            }
            set { this["AllJointsSpringPercent"].AsFloat = value; }
        }

        /// <summary>
        /// 所有关节阻尼比例
        /// </summary>
        public float AllJointsDamperPercent
        {
            get
            {
                if (this.HasKey("AllJointsDamperPercent"))
                    return this["AllJointsDamperPercent"].AsFloat;
                else return 0.5f;
            }
            set { this["AllJointsDamperPercent"].AsFloat = value; }
        }

        /// <summary>
        /// 所有关节最大速度比例
        /// </summary>
        public float AllJointsMaxVelocity
        {
            get
            {
                if (this.HasKey("AllJointsMaxVelocity"))
                    return this["AllJointsMaxVelocity"].AsFloat;
                else return 0.5f;
            }
            set { this["AllJointsMaxVelocity"].AsFloat = value; }
        }

        /// <summary>
        /// 动作开始时间
        /// </summary>
        public float TimeDelay
        {
            get
            {
                if (this.HasKey("TimeDelay"))
                    return this["TimeDelay"].AsFloat;
                else return 0f;
            }
            set { this["TimeDelay"].AsFloat = value; }
        }

        /// <summary>
        /// 载入文件
        /// </summary>
        internal void LoadFiles()
        {
            if (this.HasKey("Files"))
            {
                IEnumerable<string> sFiles = this["Files"].Value.Split(new string[] { $"*?*" }, StringSplitOptions.RemoveEmptyEntries);

                sFiles = sFiles.Where(f => !string.IsNullOrEmpty(f) && f != "None").Distinct();

                this.Files = this.Files.Concat(sFiles).ToList();
            }
        }

        /// <summary>
        /// 初始化动作数据
        /// </summary>
        /// <param name="data"></param>
        internal void InitMotion(MMDEntity.FilesData data)
        {
            this.Files.Clear();

            // 设定默认动作文件
            if (data.DefaultMotion != MMDEntity.noneString)
            {
                this.Files.Add(data.DefaultMotion);
            }

            // 设定默认表情
            if (data.Expressions != null && data.Expressions.Count > 0)
            {
                // 默认最大只能选择3个表情文件
                var maxExpressions = Math.Min(3, data.Expressions.Count);

                for (var i = 0; i < maxExpressions; i++)
                {
                    this.Files?.Add(data.Expressions[i]);
                }
            }
        }

        #region ToString相关的处理
        void BeforeToString()
        {
            var files = _Files.Where(f => !string.IsNullOrEmpty(f) && f != "None").Distinct();

            if (files.Count() > 0)
            {
                this["Files"] = String.Join($"*?*", files.ToArray());
            }
            else
            {
                this.Remove("Files");
            }
        }

        public override string ToString()
        {
            BeforeToString();
            return base.ToString();
        }

        public override string ToString(string aPrefix)
        {
            BeforeToString();
            return base.ToString(aPrefix);
        }

        public override void ToString(string aPrefix, StringBuilder sb)
        {
            BeforeToString();
            base.ToString(aPrefix, sb);
        }
        #endregion
    }
}
