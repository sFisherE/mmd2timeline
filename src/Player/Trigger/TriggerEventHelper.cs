using SimpleJSON;
using System;
using System.Collections.Generic;
using System.Linq;

namespace mmd2timeline
{
    internal partial class TriggerEventHelper : IDisposable
    {
        internal const string TRIGGER_SCRIPT_LOADED = "Script Loaded Trigger";

        internal const string TRIGGER_START_PLAYING = "Start Playing Trigger";
        internal const string TRIGGER_PLAY_NEXT = "Play Next Trigger";
        internal const string TRIGGER_IS_END = "Is End Trigger";

        internal const string TRIGGER_FAVORITED = "Favorited Trigger";
        internal const string TRIGGER_UNFAVORITED = "Not Favorite Trigger";

        internal const string TRIGGER_PLAYMODE_INIT = "In Init Mode Trigger";
        internal const string TRIGGER_PLAYMODE_PLAY = "In Play Mode Trigger";
        internal const string TRIGGER_PLAYMODE_EDIT = "In Edit Mode Trigger";
        internal const string TRIGGER_PLAYMODE_LOAD = "In Load Mode Trigger";

        internal const string TRIGGER_CAMERA_ACTIVATED = "Camera Activated Trigger";
        internal const string TRIGGER_CAMERA_DEACTIVATED = "Camera Deactivated Trigger";

        internal const string TRIGGER_PROGRESS_CHANGE = "Progress Trigger";

        internal const string TRIGGER_PLAYING = "Playing Status Trigger";

        internal const string TRIGGER_PLAYMODE_CHANGED = "Play Mode Changed Trigger";

        //const string TRIGGER_CAMERA_DEACTIVATED = "Camera Deactivated Trigger";

        //MVRScript _owner;
        //internal MVRScript Owner { get { return _owner; } set { _owner = value; } }
        bool _isInited = false;
        readonly Dictionary<string, TriggerEvent> _triggers = new Dictionary<string, TriggerEvent>() {
            {TRIGGER_SCRIPT_LOADED, new TriggerEvent(TRIGGER_SCRIPT_LOADED, TriggerEventType.Bool)},
            {TRIGGER_START_PLAYING, new TriggerEvent(TRIGGER_START_PLAYING, TriggerEventType.String)},
            {TRIGGER_PLAY_NEXT, new TriggerEvent(TRIGGER_PLAY_NEXT, TriggerEventType.Action)},
            {TRIGGER_IS_END, new TriggerEvent(TRIGGER_IS_END, TriggerEventType.Action)},
            {TRIGGER_FAVORITED, new TriggerEvent(TRIGGER_FAVORITED, TriggerEventType.Action)},
            {TRIGGER_UNFAVORITED, new TriggerEvent(TRIGGER_UNFAVORITED, TriggerEventType.Action)},
            {TRIGGER_PLAYMODE_INIT, new TriggerEvent(TRIGGER_PLAYMODE_INIT, TriggerEventType.Action)},
            {TRIGGER_PLAYMODE_PLAY, new TriggerEvent(TRIGGER_PLAYMODE_PLAY, TriggerEventType.Action)},
            {TRIGGER_PLAYMODE_EDIT, new TriggerEvent(TRIGGER_PLAYMODE_EDIT, TriggerEventType.Action)},
            {TRIGGER_PLAYMODE_LOAD, new TriggerEvent(TRIGGER_PLAYMODE_LOAD, TriggerEventType.Action)},
            {TRIGGER_CAMERA_ACTIVATED, new TriggerEvent(TRIGGER_CAMERA_ACTIVATED, TriggerEventType.Action)},
            {TRIGGER_CAMERA_DEACTIVATED, new TriggerEvent(TRIGGER_CAMERA_DEACTIVATED, TriggerEventType.Action)},
            {TRIGGER_PROGRESS_CHANGE,new TriggerEvent(TRIGGER_PROGRESS_CHANGE, TriggerEventType.Float)},
            {TRIGGER_PLAYING,new TriggerEvent(TRIGGER_PLAYING,TriggerEventType.Bool)},
            {TRIGGER_PLAYMODE_CHANGED,new TriggerEvent(TRIGGER_PLAYMODE_CHANGED,TriggerEventType.String)},
        };

        internal delegate void TriggerListChangedCallback();
        internal event TriggerListChangedCallback OnTriggerListChanged;

        /// <summary>
        /// 初始化触发器
        /// </summary>
        public void InitTriggers(MVRScript owner)
        {
            // 只能初始化一次
            if (_isInited)
            {
                LogUtil.LogWarning($"TriggerEventHelper is Inited allready.");
                return;
            }

            // 载入触发器资产
            //SimpleTriggerHandler.LoadAssets();

            //_owner = owner;

            //_favoriteTrigger = new EventTrigger(_owner, "Favorited");
            //_unfavoriteTrigger = new EventTrigger(_owner, "UnFavorited");

            _isInited = true;

            // Adding the rename listener
            //SuperController.singleton.onAtomUIDRenameHandlers += OnAtomRename;
        }

        /// <summary>
        /// 获取事件名称列表及事件数量
        /// </summary>
        /// <returns></returns>
        internal Dictionary<string, int> GetTriggerNames()
        {
            return _triggers.ToDictionary(t => t.Key, t => t.Value.ActionCount);
        }

        ///// <summary>
        ///// 添加触发器
        ///// </summary>
        ///// <param name="name"></param>
        ///// <returns></returns>
        //internal TriggerEvent AddTrigger(string name, int? type = null)
        //{
        //    var triggerType = TriggerEventType.Bool;

        //    if (type.HasValue)
        //    {
        //        triggerType = type.Value;
        //    }

        //    if (_triggers.ContainsKey(name))
        //    {
        //        var trigger = _triggers[name];

        //        if (type.HasValue)
        //        {
        //            trigger.EventType = triggerType;
        //        }

        //        return trigger;
        //    }
        //    else
        //    {
        //        var trigger = new TriggerEvent(name, triggerType);
        //        _triggers.Add(name, trigger);

        //        OnTriggerListChanged?.Invoke();
        //        return trigger;
        //    }
        //}

        /// <summary>
        /// 移除触发器
        /// </summary>
        /// <param name="name"></param>
        internal void RemoveTrigger(string name)
        {
            if (_triggers.ContainsKey(name))
            {
                _triggers.Remove(name);
                OnTriggerListChanged?.Invoke();
            }
        }
        /// <summary>
        /// 获取触发器
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        internal TriggerEvent GetTrigger(string name)
        {
            TriggerEvent trigger;

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
            var trigger = GetTrigger(name);

            if (trigger == null)
                return;

            // 触发事件
            trigger.Trigger();
        }

        /// <summary>
        /// 触发
        /// </summary>
        /// <param name="name"></param>
        internal void Trigger(string name, string value)
        {
            var trigger = GetTrigger(name);

            if (trigger == null)
                return;

            // 只有字符串类型的触发器才会传递string值
            if (trigger.EventType == TriggerEventType.String)
            {
                trigger.Trigger(value);
            }
            else
            {
                trigger.Trigger();
            }
        }

        /// <summary>
        /// 触发类型为Float的事件
        /// </summary>
        /// <param name="name"></param>
        /// <param name="value"></param>
        /// <param name="min"></param>
        /// <param name="max"></param>
        internal void Trigger(string name, bool value)
        {
            var trigger = GetTrigger(name);

            if (trigger == null)
                return;

            // 只有布尔类型的触发器才会传递bool值
            if (trigger.EventType == TriggerEventType.Bool)
            {
                trigger.Trigger(value);
            }
            else
            {
                trigger.Trigger();
            }
        }

        /// <summary>
        /// 触发类型为Float的事件
        /// </summary>
        /// <param name="name"></param>
        /// <param name="value"></param>
        /// <param name="min"></param>
        /// <param name="max"></param>
        internal void Trigger(string name, float value, float min = 0f, float max = 1f)
        {
            var trigger = GetTrigger(name);

            if (trigger == null)
                return;

            // 只有浮点类型的触发器才会传递float值
            if (trigger.EventType == TriggerEventType.Float)
            {
                trigger.Trigger(value, min, max);
            }
            else
            {
                trigger.Trigger();
            }
        }

        /// <summary>
        /// 触发Chooser类型的事件
        /// </summary>
        /// <param name="name"></param>
        /// <param name="value"></param>
        /// <param name="choices"></param>
        internal void Trigger(string name, string value, List<string> choices)
        {
            var trigger = GetTrigger(name);

            if (trigger == null)
                return;

            // 只有字符串类型的触发器才会传递string值
            if (trigger.EventType == TriggerEventType.Chooser)
            {
                trigger.Trigger(value, choices);
            }
            else
            {
                trigger.Trigger();
            }
        }

        string triggerKey = "MMDTriggers";

        /// <summary>
        /// 获取保存用的JSON
        /// </summary>
        /// <returns></returns>
        public JSONClass GetJSON(JSONClass json)
        {
            var js = new JSONClass();

            json[triggerKey] = js;

            foreach (var item in _triggers)
            {
                var name = item.Key;
                var trigger = item.Value;

                var trigerJSON = new JSONClass();
                //trigerJSON["name"] = trigger.Name;
                //trigerJSON["type"].AsInt = trigger.EventType;

                foreach (var action in trigger.GetActions())
                {
                    var actionJSON = new JSONClass();
                    //actionJSON["name"] = action.Name;
                    actionJSON["atom"] = action.AtomUID;
                    actionJSON["receiver"] = action.ReceiverName;
                    actionJSON["target"] = action.Target;
                    //actionJSON["type"].AsInt = action.TargetType;
                    actionJSON["set"].AsBool = action.ValueCustom;

                    if (action.ValueCustom)
                    {
                        switch (action.TargetType)
                        {
                            case TriggerEventType.Bool:
                                actionJSON["value"].AsBool = action.ValueOfBool;
                                break;
                            case TriggerEventType.Float:
                                actionJSON["value"].AsFloat = action.ValueOfFloat;
                                break;
                            case TriggerEventType.String:
                            case TriggerEventType.Chooser:
                                actionJSON["value"] = action.ValueOfString;
                                break;
                        }
                    }

                    trigerJSON[$"{action.Name}"] = actionJSON;
                }

                js[$"{name}"] = trigerJSON;
            }

            return json;
        }

        /// <summary>
        /// 从JSON恢复数据
        /// </summary>
        /// <param name="jc"></param>
        /// <param name="subScenePrefix"></param>
        /// <param name="isMerge"></param>
        /// <param name="setMissingToDefault"></param>
        public void RestoreFromJSON(JSONClass jc)
        {
            if (!jc.HasKey(triggerKey))
                return;

            var json = jc[triggerKey].AsObject;

            foreach (var item in _triggers)
            {
                var name = item.Key;
                var trigger = item.Value;

                if (json.HasKey(name))
                {
                    var tc = json[name].AsObject;

                    if (tc != null)
                    {
                        foreach (var actionKey in tc.Keys)
                        {
                            var actionJSON = tc[actionKey].AsObject;

                            if (actionJSON == null || !actionJSON.HasKey("atom"))
                                continue;

                            var atomUID = actionJSON["atom"];

                            if (string.IsNullOrEmpty(atomUID))
                                continue;

                            if (!actionJSON.HasKey("receiver"))
                                continue;

                            var receiverKey = actionJSON["receiver"];

                            if (!string.IsNullOrEmpty(receiverKey))
                            {
                                var action = new TriggerEventAction(trigger);
                                var target = "";
                                var custom = false;

                                if (actionJSON.HasKey("target"))
                                    target = actionJSON["target"];
                                if (actionJSON.HasKey("set"))
                                    custom = actionJSON["set"].AsBool;

                                // 设置动作值
                                action.SetAtomUID(atomUID);
                                action.SetReceiverName(receiverKey);
                                action.SetTarget(target);
                                action.SetValueCustom(custom);

                                if (actionJSON.HasKey("value"))
                                {
                                    switch (action.TargetType)
                                    {
                                        case TriggerEventType.Bool:
                                            action.SetValue(actionJSON["value"].AsBool);
                                            break;
                                        case TriggerEventType.Float:
                                            action.SetValue(actionJSON["value"].AsFloat);
                                            break;
                                        case TriggerEventType.String:
                                        case TriggerEventType.Chooser:
                                            action.SetValue(actionJSON["value"]);
                                            break;
                                        default:
                                            action.Value = actionJSON["value"];
                                            break;
                                    }
                                }
                                trigger.AddAction(action);
                            }
                        }
                    }
                }
            }

            OnTriggerListChanged?.Invoke();
        }

        #region 单例
        //private TriggerEventHelper(MVRScript script)
        //{
        //    _owner = script;
        //}

        private TriggerEventHelper()
        {
        }

        private static TriggerEventHelper _instance;
        private static object _lock = new object();

        /// <summary>
        /// 触发器助手的单例
        /// </summary>
        public static TriggerEventHelper GetInstance()
        {
            lock (_lock)
            {
                if (_instance == null)
                {
                    _instance = new TriggerEventHelper();
                }

                return _instance;
            }
        }

        public void Dispose()
        {
            foreach (var item in _triggers)
            {
                item.Value.Dispose();
            }

            _triggers.Clear();
        }
        #endregion

    }
}
