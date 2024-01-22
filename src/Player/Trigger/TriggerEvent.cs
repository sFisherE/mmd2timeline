using Leap.Unity;
using System;
using System.Collections.Generic;

namespace mmd2timeline
{
    /// <summary>
    /// 触发器事件
    /// </summary>
    internal partial class TriggerEvent : IDisposable
    {
        /// <summary>
        /// 动作列表更改的事件委托
        /// </summary>
        /// <param name="list"></param>
        public delegate void ActionListChangedCallback(TriggerEvent sender, List<TriggerEventAction> list);
        /// <summary>
        /// 动作列表更改的事件
        /// </summary>
        public event ActionListChangedCallback OnActionListChanged;

        public delegate void ActionListChangeCallback(TriggerEvent sender, TriggerEventAction action, int index);
        public event ActionListChangeCallback OnActionAdded;
        public event ActionListChangeCallback OnActionRemoved;
        public event ActionListChangeCallback OnActionMoved;

        /// <summary>
        /// 动作更改的事件委托
        /// </summary>
        /// <param name="list"></param>
        public delegate void ActionChangedCallback(TriggerEventAction action, int index);
        /// <summary>
        /// 动作更改事件
        /// </summary>
        public event ActionChangedCallback OnActionValueChanged;

        /// <summary>
        /// 事件类型
        /// </summary>
        public int EventType = TriggerEventType.Bool;
        /// <summary>
        /// 事件名称
        /// </summary>
        public string Name = "";

        /// <summary>
        /// 动作清单
        /// </summary>
        readonly List<TriggerEventAction> _actionList = new List<TriggerEventAction>();

        /// <summary>
        /// 获取动作数量
        /// </summary>
        public int ActionCount
        {
            get { return _actionList.Count; }
        }

        /// <summary>
        /// 初始化触发器事件对象
        /// </summary>
        /// <param name="name"></param>
        /// <param name="type"></param>
        public TriggerEvent(string name, int type)
        {
            Name = name;
            EventType = type;

            SuperController.singleton.onAtomUIDRenameHandlers += OnAtomChange;
        }

        /// <summary>
        /// 原子名称更改的事件
        /// </summary>
        /// <param name="oldName"></param>
        /// <param name="newName"></param>
        void OnAtomChange(string oldName, string newName)
        {
            this.SyncUpdateNames(oldName, newName);
        }

        /// <summary>
        /// 获取动作列表
        /// </summary>
        public List<TriggerEventAction> GetActions()
        {
            if (_actionList.Count == 0)
            {
                AddAction();
            }
            return _actionList.ToList();
        }

        /// <summary>
        /// 根据序号获得动作对象
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        public TriggerEventAction GetAction(int index)
        {
            if (_actionList.Count > index)
            {
                return _actionList[index];
            }
            return null;
        }

        TriggerEventAction CreateAction()
        {
            var action = new TriggerEventAction(this);
            return action;
        }

        /// <summary>
        /// 动作更改事件处理函数
        /// </summary>
        /// <param name="sender"></param>
        private void OnActionChange(TriggerEventAction sender)
        {
            OnActionValueChanged?.Invoke(sender, _actionList.IndexOf(sender));
        }

        /// <summary>
        /// 添加新的动作并返回动作序号
        /// </summary>
        /// <returns></returns>
        public int AddAction()
        {
            var action = CreateAction();

            return AddAction(action);
        }

        /// <summary>
        /// 添加动作
        /// </summary>
        /// <returns></returns>
        public int AddAction(TriggerEventAction action)
        {
            action.OnChanged -= OnActionChange;
            action.OnChanged += OnActionChange;
            _actionList.Add(action);

            var index = _actionList.IndexOf(action);
            OnActionAdded?.Invoke(this, action, index);
            return index;
        }

        /// <summary>
        /// 清空动作
        /// </summary>
        public void ClearActions()
        {
            _actionList.Clear();
        }

        /// <summary>
        /// 移除指定序号的动作
        /// </summary>
        /// <param name="index"></param>
        public void RemoveAction(int index)
        {
            RemoveAction(GetAction(index));
        }

        /// <summary>
        /// 移除动作
        /// </summary>
        /// <param name="action"></param>
        public void RemoveAction(TriggerEventAction action)
        {
            if (action == null)
                return;

            var index = _actionList.IndexOf(action);
            _actionList.Remove(action);
            OnActionRemoved?.Invoke(this, action, index);
        }

        /// <summary>
        /// 移动动作
        /// </summary>
        /// <param name="index"></param>
        /// <param name="up"></param>
        public void MoveAction(int index, bool up = true)
        {
            if (_actionList.Count > index)
            {
                var action = _actionList[index];
                _actionList.Remove(action);

                var newIndex = index + 1;
                if (up)
                {
                    newIndex = index - 1;
                }

                if (newIndex < 0)
                {
                    newIndex = 0;
                }

                if (newIndex >= _actionList.Count)
                {
                    _actionList.Add(action);
                }
                else
                {
                    _actionList.Insert(newIndex, action);
                }
                OnActionMoved?.Invoke(this, action, -1);
            }
        }

        /// <summary>
        /// 运行事件
        /// </summary>
        /// <param name="value"></param>
        public void Trigger()
        {
            foreach (var action in _actionList)
            {
                action.Trigger();
            }
        }

        /// <summary>
        /// 运行事件
        /// </summary>
        /// <param name="value"></param>
        public void Trigger(bool value)
        {
            foreach (var action in _actionList)
            {
                if (action.TargetType == TriggerEventType.Bool)
                {
                    action.Trigger(value);
                }
                else
                {
                    action.Trigger();
                }
            }
        }

        /// <summary>
        /// 运行事件
        /// </summary>
        /// <param name="value"></param>
        public void Trigger(string value)
        {
            foreach (var action in _actionList)
            {
                if (action.TargetType == TriggerEventType.String)
                {
                    action.Trigger(value);
                }
                else
                {
                    action.Trigger();
                }
            }
        }

        /// <summary>
        /// 触发Choose类型的事件
        /// </summary>
        /// <param name="value"></param>
        /// <param name="choices"></param>
        public void Trigger(string value, List<string> choices)
        {
            foreach (var action in _actionList)
            {
                if (action.TargetType == TriggerEventType.Chooser)
                {
                    action.Trigger(value, choices);
                }
                else
                {
                    action.Trigger();
                }
            }
        }

        /// <summary>
        /// 触发Float事件
        /// </summary>
        /// <param name="value"></param>
        /// <param name="min"></param>
        /// <param name="max"></param>
        public void Trigger(float value, float min = 0f, float max = 1f)
        {
            foreach (var action in _actionList)
            {
                if (action.TargetType == TriggerEventType.Float)
                {
                    action.Trigger(value, min, max);
                }
                else
                {
                    action.Trigger();
                }
            }
        }

        /// <summary>
        /// 同步更新名称
        /// </summary>
        public void SyncUpdateNames(string oldId, string newId)
        {
            foreach (var action in _actionList)
            {
                action.SyncUpdateNames(oldId, newId);
            }
        }

        public void Dispose()
        {
            SuperController.singleton.onAtomUIDRenameHandlers -= OnAtomChange;

            foreach (var action in _actionList)
            {
                action.OnChanged -= OnActionChange;

                action.Dispose();
            }

            _actionList.Clear();
        }
    }
}
