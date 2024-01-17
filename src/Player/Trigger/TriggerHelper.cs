using MacGruber;
using System;
using System.Collections.Generic;

namespace mmd2timeline
{
    internal partial class TriggerHelper
    {
        MVRScript _owner;
        MVRScript _container;

        internal MVRScript Owner { get { return _owner; } set { _owner = value; } }
        //EventTrigger _favoriteTrigger;
        //EventTrigger _unfavoriteTrigger;
        JSONStorableStringChooser editTriggerChoice;
        string editTriggerListDefault = "Select a trigger to edit";
        bool _isInited = false;
        bool _isUIInited = false;

        readonly Dictionary<string, CustomTrigger> _triggers = new Dictionary<string, CustomTrigger>();

        /// <summary>
        /// 初始化触发器
        /// </summary>
        public void InitTriggers(MVRScript owner)
        {
            // 只能初始化一次
            if (_isInited)
            {
                LogUtil.LogWarning($"TriggerHelper is Inited allready.");
                return;
            }

            // 载入触发器资产
            SimpleTriggerHandler.LoadAssets();

            _owner = owner;

            //_favoriteTrigger = new EventTrigger(_owner, "Favorited");
            //_unfavoriteTrigger = new EventTrigger(_owner, "UnFavorited");

            _isInited = true;

            // Adding the rename listener
            //SuperController.singleton.onAtomUIDRenameHandlers += OnAtomRename;
        }

        /// <summary>
        /// 原子更名的处理
        /// </summary>
        /// <param name="oldid"></param>
        /// <param name="newid"></param>
        void OnAtomRename(string oldid, string newid)
        {
            var allTriggers = this.GetAllTriggers();
            foreach (var et in allTriggers)
            {
                et.SyncAtomNames();
            }
        }

        /// <summary>
        /// 添加触发器
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        internal CustomTrigger AddTrigger(string name)
        {
            if (_triggers.ContainsKey(name))
            {
                return _triggers[name];
            }
            else
            {
                var trigger = new EventTrigger(_owner, name);
                _triggers.Add(name, trigger);
                return trigger;
            }
        }

        /// <summary>
        /// 添加浮点类型的触发器
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        internal CustomTrigger AddFloatTrigger(string name)
        {
            if (_triggers.ContainsKey(name))
            {
                return _triggers[name];
            }
            else
            {
                var trigger = new FloatTrigger(_owner, name);
                AddTrigger(name, trigger);
                return trigger;
            }
        }

        /// <summary>
        /// 添加指定名称的触发器
        /// </summary>
        /// <param name="name"></param>
        /// <param name="trigger"></param>
        void AddTrigger(string name, CustomTrigger trigger)
        {
            _triggers.Add(name, trigger);

            RefreshEditChoice();
        }

        /// <summary>
        /// 刷新编辑选项
        /// </summary>
        private void RefreshEditChoice()
        {
            if (!_isUIInited)
            {
                //LogUtil.LogWarning($"TriggerHelper UI is NOT Init.");
                return;
            }

            var list = _triggers.Keys.ToList();

            list.Insert(0, editTriggerListDefault);

            editTriggerChoice.choices = _triggers.Keys.ToList();
        }

        /// <summary>
        /// 获取触发器
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        internal CustomTrigger GetTrigger(string name)
        {
            CustomTrigger trigger;

            if (_triggers.TryGetValue(name, out trigger))
            {
                return trigger;
            }

            return null;
        }

        /// <summary>
        /// 触发
        /// </summary>
        /// <param name="name"></param>
        internal void Trigger(string name)
        {
            CustomTrigger trigger;

            if (_triggers.TryGetValue(name, out trigger))
            {
                ((EventTrigger)trigger).Trigger();
            }
        }

        /// <summary>
        /// 触发浮点类型的的触发器
        /// </summary>
        /// <param name="name"></param>
        /// <param name="v"></param>
        internal void Trigger(string name, float v)
        {
            CustomTrigger trigger;

            if (_triggers.TryGetValue(name, out trigger))
            {
                ((FloatTrigger)trigger).Trigger(v);
            }
        }

        /// <summary>
        /// 获取所有触发器
        /// </summary>
        /// <returns></returns>
        internal IEnumerable<CustomTrigger> GetAllTriggers()
        {
            return _triggers.Values.ToList();
        }

        /// <summary>
        /// 初始化触发器编辑UI
        /// </summary>
        internal void InitTriggerEditUI(MVRScript container)
        {
            _container = container;
            var editTriggersList = new List<string>
            {
                editTriggerListDefault
            };

            editTriggerChoice = new JSONStorableStringChooser("triggers", editTriggersList, editTriggerListDefault, "Triggers", TriggerEditChoice) { isStorable = true };
            container.RegisterStringChooser(editTriggerChoice);
            UIDynamicPopup editTriggerPopup = container.CreateScrollablePopup(editTriggerChoice, true);

            _isUIInited = true;

            RefreshEditChoice();
        }

        /// <summary>
        /// 选中编辑触发器
        /// </summary>
        /// <param name="choice"></param>
        void TriggerEditChoice(string choice)
        {
            if (!_isInited)
            {
                LogUtil.LogWarning($"TriggerHelper is NOT Init.");
                return;
            }

            var trigger = GetTrigger(choice);

            if (trigger != null)
            {
                trigger.OpenPanel(_container);
            }

            editTriggerChoice.val = editTriggerListDefault;
        }

        #region 单例
        private TriggerHelper(MVRScript script)
        {
            _owner = script;
        }

        private TriggerHelper()
        {
        }

        private static TriggerHelper _instance;
        private static object _lock = new object();

        /// <summary>
        /// 触发器助手的单例
        /// </summary>
        public static TriggerHelper GetInstance()
        {
            lock (_lock)
            {
                if (_instance == null)
                {
                    _instance = new TriggerHelper();
                }

                return _instance;
            }
        }
        #endregion

    }
}
