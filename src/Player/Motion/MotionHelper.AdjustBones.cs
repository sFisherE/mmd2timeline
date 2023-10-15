using System.Collections.Generic;
using UnityEngine;

namespace mmd2timeline
{
    internal partial class MotionHelper
    {
        /// <summary>
        /// 根据骨骼名称获取骨骼数据
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        internal Vector3 GetVectorByBoneName(string name)
        {
            if (_MmdPersonGameObject?._model?.m_BoneAdjust.ContainsKey(name) ?? false)
            {
                return _MmdPersonGameObject._model.m_BoneAdjust[name];
            }
            else
            {
                return Vector3.zero;
            }
        }

        /// <summary>
        /// 获取所有骨骼设定值
        /// </summary>
        /// <returns></returns>
        internal Dictionary<string, Vector3> GetBoneAdjustValues()
        {
            if (_MmdPersonGameObject?._model?.m_BoneAdjust.Count > 0)
            {
                return _MmdPersonGameObject._model.m_BoneAdjust;
            }
            else { return new Dictionary<string, Vector3>(); };
        }

        /// <summary>
        /// 获取骨骼调整值
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        Vector3? GetBoneAdjustValue(string name)
        {
            if (_MmdPersonGameObject == null || _MmdPersonGameObject._model == null) return null;

            if (!_MmdPersonGameObject._model.m_BoneAdjust.ContainsKey(name))
            {
                _MmdPersonGameObject._model.m_BoneAdjust.Add(name, new Vector3());
            }

            return _MmdPersonGameObject._model.m_BoneAdjust[name];
        }

        /// <summary>
        /// 设置骨骼参数
        /// </summary>
        /// <param name="name"></param>
        /// <param name="v"></param>
        internal void SetBoneAdjust(string name, Vector3 v)
        {
            var val = GetBoneAdjustValue(name);

            if (!val.HasValue) return;

            _MmdPersonGameObject._model.m_BoneAdjust[name] = v;
            _MmdPersonGameObject.Refresh();
        }

        /// <summary>
        /// 设置骨骼参数X
        /// </summary>
        /// <param name="name"></param>
        /// <param name="v"></param>
        internal void SetBoneAdjustX(string name, float v)
        {
            var val = GetBoneAdjustValue(name);

            if (!val.HasValue) return;

            _MmdPersonGameObject._model.m_BoneAdjust[name] = new Vector3(v, val.Value.y, val.Value.z);
            _MmdPersonGameObject.Refresh();
        }
        /// <summary>
        /// 设置骨骼参数Y
        /// </summary>
        /// <param name="name"></param>
        /// <param name="v"></param>
        internal void SetBoneAdjustY(string name, float v)
        {
            var val = GetBoneAdjustValue(name);

            if (!val.HasValue) return;

            _MmdPersonGameObject._model.m_BoneAdjust[name] = new Vector3(val.Value.x, v, val.Value.z);
            _MmdPersonGameObject.Refresh();
        }
        /// <summary>
        /// 设置骨骼参数Z
        /// </summary>
        /// <param name="name"></param>
        /// <param name="v"></param>
        internal void SetBoneAdjustZ(string name, float v)
        {
            var val = GetBoneAdjustValue(name);

            if (!val.HasValue) return;

            _MmdPersonGameObject._model.m_BoneAdjust[name] = new Vector3(val.Value.x, val.Value.y, v);
            _MmdPersonGameObject.Refresh();
        }

        /// <summary>
        /// 清除所有骨骼方向修正设置
        /// </summary>
        internal void ClearBoneRotationAdjust()
        {
            if (_MmdPersonGameObject == null || _MmdPersonGameObject._model == null) return;
            _MmdPersonGameObject._model.m_BoneAdjust.Clear();
            _MmdPersonGameObject.Refresh();
        }
    }
}
