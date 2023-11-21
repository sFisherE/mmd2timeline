using mmd2timeline.Store;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace mmd2timeline
{
    /// <summary>
    /// 动作助手组
    /// </summary>
    internal partial class MotionHelperGroup
    {
        /// <summary>
        /// 存储动作助手的字典
        /// </summary>
        Dictionary<Atom, MotionHelper> _group = new Dictionary<Atom, MotionHelper>();

        private MotionHelperGroup() { }

        /// <summary>
        /// 获取所有动作助手
        /// </summary>
        internal MotionHelper[] Helpers
        {
            get { return _group.Values.ToArray(); }
        }

        /// <summary>
        /// 根据人物原子获取已经初始化过的动作助手
        /// </summary>
        /// <param name="person"></param>
        /// <returns></returns>
        /// <remarks>执行此方法时如果没有初始化过此助手会新建一个实例。</remarks>
        internal bool TryGetInitedMotionHelper(Atom person, out MotionHelper motionHelper)
        {
            if (_group.TryGetValue(person, out motionHelper))
            {
                return true;
            }
            else
            {
                if (person.type == "Person")
                {
                    motionHelper = new MotionHelper(person);

                    motionHelper.ResetAtom();

                    _group.Add(person, motionHelper);
                }

                return false;
            }
        }

        /// <summary>
        /// 获取已实例化的动作助手
        /// </summary>
        /// <param name="person"></param>
        /// <returns></returns>
        internal MotionHelper GetMotionHelper(Atom person)
        {
            MotionHelper helper;

            if (_group.TryGetValue(person, out helper))
            {
                return helper;
            }

            return null;
        }

        /// <summary>
        /// 移除动作助手
        /// </summary>
        /// <param name="person"></param>
        internal MotionHelper RemoveMotionHelper(Atom person)
        {
            if (_group.ContainsKey(person))
            {
                var motionHelper = _group[person];

                motionHelper.RemovePersonMotionUI();

                _group.Remove(person);

                motionHelper.OnDestroy();

                return motionHelper;
            }

            return null;
        }

        /// <summary>
        /// 同步动作进度
        /// </summary>
        /// <param name="progress"></param>
        internal void SyncMotionProgress(float progress)
        {
            foreach (var pair in _group)
            {
                pair.Value.SetProgress(progress);
            }
        }

        /// <summary>
        /// 同步动作进度
        /// </summary>
        /// <param name="progress"></param>
        internal void ShowMotionUI(bool show)
        {
            foreach (var pair in _group)
            {
                pair.Value.ShowPersonMotionUI(show);
            }
        }

        /// <summary>
        /// 重置
        /// </summary>
        internal void ResetMotions()
        {
            foreach (var pair in _group)
            {
                pair.Value.Reset();
            }
        }

        /// <summary>
        /// 重新更新动作
        /// </summary>
        internal void ReUpdateMotion()
        {
            foreach (var pair in _group)
            {
                pair.Value.ReUpdateMotion();
            }
        }

        /// <summary>
        /// 更新设定值到设置
        /// </summary>
        internal void UpdateValuesToSettings()
        {
            foreach (var pair in _group)
            {
                pair.Value.UpdateBoneRotationAdjustValuesToSettings();
                pair.Value.UpdateMotionsToSettings();
            }
        }

        /// <summary>
        /// 检查是否全部已经完成准备
        /// </summary>
        /// <returns></returns>
        internal bool AllIsReady()
        {
            foreach (var pair in _group)
            {
                // 有任何一个没有准备好的返回false
                if (!pair.Value.IsReady)
                {
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// 获取所有原子是否都已经完成初始化
        /// </summary>
        /// <returns></returns>
        internal bool AllHasAtomInited()
        {
            foreach (var pair in _group)
            {
                // 有任何一个没有准备好的返回false
                if (!pair.Value.HasAtomInited)
                {
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// 更新所有人物位置
        /// </summary>
        internal void UpdateTransform()
        {
            foreach (var pair in _group)
            {
                pair.Value.UpdateTransform();
            }
        }

        ///// <summary>
        ///// 所有人物进行准备
        ///// </summary>
        //internal void Ready()
        //{
        //    foreach (var pair in _group)
        //    {
        //        pair.Value.Ready();
        //    }
        //}

        ///// <summary>
        ///// 设定指定人物准备
        ///// </summary>
        ///// <param name="person"></param>
        //internal void Ready(Atom person)
        //{
        //    if (_group.ContainsKey(person))
        //    {
        //        var motionHelper = _group[person];

        //        motionHelper.Ready();
        //    }
        //}

        ///// <summary>
        ///// 所有人物完成准备
        ///// </summary>
        //internal IEnumerator MakeReady()
        //{
        //    foreach (var pair in _group)
        //    {
        //        yield return pair.Value.MakeReady();
        //    }
        //    yield break;
        //}

        ///// <summary>
        ///// 设定指定人物完成准备
        ///// </summary>
        ///// <param name="person"></param>
        //internal IEnumerator MakeReady(Atom person)
        //{
        //    if (_group.ContainsKey(person))
        //    {
        //        var motionHelper = _group[person];

        //        yield return motionHelper.MakeReady();
        //    }

        //    yield break;
        //}

        ///// <summary>
        ///// 初始化人物列表的设置
        ///// </summary>
        ///// <param name="persons"></param>
        ///// <param name="motions"></param>
        ///// <param name="choices"></param>
        ///// <param name="displayChoices"></param>
        //internal void InitSettings(List<Atom> persons, List<PersonMotion> motions, List<string> choices, List<string> displayChoices)
        //{
        //    for (var i = 0; i < persons.Count; i++)
        //    {
        //        var person = persons[i];
        //        PersonMotion motion = null;
        //        if (motions.Count > i)
        //        {
        //            motion = motions[i];
        //        }

        //        InitSettings(person, motion, choices, displayChoices);
        //    }
        //}

        ///// <summary>
        ///// 初始化指定让人物的设置
        ///// </summary>
        ///// <param name="person"></param>
        ///// <param name="motion"></param>
        ///// <param name="choices"></param>
        ///// <param name="displayChoices"></param>
        //void InitSettings(Atom person, PersonMotion motion, List<string> choices, List<string> displayChoices)
        //{
        //    if (_group.ContainsKey(person))
        //    {
        //        var motionHelper = _group[person];

        //        motionHelper.InitSettings(choices, displayChoices, motion);
        //    }
        //}

        #region 单例
        private static MotionHelperGroup _instance;
        private static object _lock = new object();

        /// <summary>
        /// 镜头控制器的单例
        /// </summary>
        public static MotionHelperGroup GetInstance()
        {
            lock (_lock)
            {
                if (_instance == null)
                {
                    _instance = new MotionHelperGroup();
                }

                return _instance;
            }
        }

        internal void ResetControlState()
        {
            foreach (var pair in _group)
            {
                pair.Value.ResetControlState();
            }
        }
        #endregion


        public void OnDisable()
        {
            foreach (var pair in _group)
            {
                pair.Value.OnDisable();
            }
        }

        public void OnEnable()
        {

        }

        public void OnDestroy()
        {
            foreach (var pair in _group)
            {
                pair.Value.OnDestroy();
            }

            _group.Clear();

            _instance = null;
        }
    }
}
