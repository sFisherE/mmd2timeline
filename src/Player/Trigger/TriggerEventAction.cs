using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace mmd2timeline
{
    /// <summary>
    /// 触发器事件动作
    /// </summary>
    internal partial class TriggerEventAction : IDisposable
    {
        /// <summary>
        /// 名称更改的事件委托
        /// </summary>
        /// <param name="list"></param>
        public delegate void ChangedCallback(TriggerEventAction sender);
        /// <summary>
        /// 名称更改的事件
        /// </summary>
        public event ChangedCallback OnChanged;

        TriggerEvent _trigger;

        private TriggerEventAction() { }

        public TriggerEventAction(TriggerEvent trigger)
        {
            _trigger = trigger;
        }

        /// <summary>
        /// 动作名称
        /// </summary>
        public string Name
        {
            get
            {
                if (string.IsNullOrEmpty(AtomUID))
                {
                    return "undefined";
                }
                else
                {
                    return $"{AtomUID}:{ReceiverName}:{Target}";
                }
            }
        }

        string _atomUID;
        public string AtomUID
        {
            get { return _atomUID; }
        }

        /// <summary>
        /// 设置目标原子UID
        /// </summary>
        /// <param name="uid"></param>
        public void SetAtomUID(string uid)
        {
            _atomUID = uid;
        }

        /// <summary>
        /// 获取目标原子
        /// </summary>
        public Atom Atom
        {
            get
            {
                if (string.IsNullOrEmpty(_atomUID))
                    return null;
                return SuperController.singleton.GetAtomByUid(_atomUID);
            }
        }

        string _receiverName;
        public string ReceiverName
        {
            get { return _receiverName; }
        }

        /// <summary>
        /// 设置接收者名称
        /// </summary>
        /// <param name="id"></param>
        public void SetReceiverName(string id)
        {
            _receiverName = id;

            // 重置目标类型
            _targetType = null;
        }
        /// <summary>
        /// 接收器
        /// </summary>
        public JSONStorable Receiver
        {
            get
            {
                if (string.IsNullOrEmpty(ReceiverName))
                    return null;

                var targetAtom = Atom;
                if (targetAtom == null)
                {
                    return null;
                }
                else
                {
                    return targetAtom.GetStorableByID(_receiverName);
                }
            }
        }

        bool _valueCustom = false;
        /// <summary>
        /// 自定义属性
        /// </summary>
        public bool ValueCustom
        {
            get { return _valueCustom; }
            private set
            {
                if (_valueCustom != value)
                {
                    _valueCustom = value;
                }
            }
        }

        /// <summary>
        /// 设置自定义
        /// </summary>
        /// <param name="on"></param>
        public void SetValueCustom(bool on)
        {
            _valueCustom = on;
        }

        int? _targetType;
        /// <summary>
        /// 目标类型
        /// </summary>
        public int TargetType
        {
            get
            {
                if (_targetType.HasValue && string.IsNullOrEmpty(Value))
                {
                    return _targetType.Value;
                }
                else
                {
                    _targetType = GetTargetType();
                    return _targetType.Value;
                }
            }
        }

        /// <summary>
        /// 获取目标类型
        /// </summary>
        /// <returns></returns>
        private int GetTargetType(int tag = 0)
        {
            var targetName = _target;
            var receiver = this.Receiver;

            if (receiver == null)
                return TriggerEventType.Other;

            if (receiver.IsBoolJSONParam(targetName))
            {
                return TriggerEventType.Bool;
            }
            else if (receiver.IsFloatJSONParam(targetName))
            {
                return TriggerEventType.Float;
            }
            else if (receiver.IsStringJSONParam(targetName))
            {
                return TriggerEventType.String;
            }
            else if (receiver.IsStringChooserJSONParam(targetName))
            {
                return TriggerEventType.Chooser;
            }
            else if (receiver.IsAction(targetName))
            {
                return TriggerEventType.Action;
            }
            else
            {
                return TriggerEventType.Other;
            }
        }

        /// <summary>
        /// 目标
        /// </summary>
        string _target;
        public string Target
        {
            get { return _target; }
        }

        /// <summary>
        /// 设置目标
        /// </summary>
        /// <param name="target"></param>
        public void SetTarget(string target)
        {
            _target = target;
            // 重置目标类型
            _targetType = null;
        }

        bool _valueOfBool;
        /// <summary>
        /// 布尔值
        /// </summary>
        public bool ValueOfBool
        {
            get
            {
                if (!string.IsNullOrEmpty(Value))
                {
                    if (bool.TryParse(Value, out _valueOfBool))
                    {
                        Value = null;
                    }
                }

                return _valueOfBool;
            }
        }
        string _valueOfString;
        /// <summary>
        /// 字符串值
        /// </summary>
        public string ValueOfString
        {
            get
            {
                if (!string.IsNullOrEmpty(Value))
                {
                    _valueOfString = Value;

                    Value = null;
                }
                return _valueOfString;
            }
        }
        float _valueOfFloat;
        /// <summary>
        /// 浮点值
        /// </summary>
        public float ValueOfFloat
        {
            get
            {
                if (!string.IsNullOrEmpty(Value))
                {
                    if (float.TryParse(Value, out _valueOfFloat))
                    {
                        Value = null;
                    }
                }
                return _valueOfFloat;
            }
        }

        /// <summary>
        /// 临时存储的未知类型值
        /// </summary>
        public string Value;

        /// <summary>
        /// 设置值
        /// </summary>
        /// <param name="value"></param>
        public void SetValue(bool value)
        {
            _valueOfBool = value;
        }

        /// <summary>
        /// 设置值
        /// </summary>
        /// <param name="value"></param>
        public void SetValue(float value)
        {
            _valueOfFloat = value;
        }

        /// <summary>
        /// 设置值
        /// </summary>
        /// <param name="value"></param>
        public void SetValue(string value)
        {
            _valueOfString = value;
        }

        /// <summary>
        /// 设置动作值
        /// </summary>
        /// <param name="atom"></param>
        /// <param name="receiver"></param>
        /// <param name="target"></param>
        /// <param name="targetType"></param>
        /// <param name="custom"></param>
        /// <param name="valueBool"></param>
        /// <param name="valueFloat"></param>
        /// <param name="valueString"></param>
        public void SetValue(string atom, string receiver, string target, int targetType = TriggerEventType.Bool, bool custom = false, bool valueBool = false, float valueFloat = 0f, string valueString = null)
        {
            SetAtomUID(atom);
            SetReceiverName(receiver);
            SetTarget(target);
            _targetType = null;
            SetValueCustom(custom);
            SetValue(valueBool);
            SetValue(valueFloat);
            SetValue(valueString);
        }

        /// <summary>
        /// 更新值
        /// </summary>
        public void UpdateValue()
        {
            OnChanged?.Invoke(this);
        }

        /// <summary>
        /// 同步更新原子名称
        /// </summary>
        public void SyncUpdateNames(string oldId, string newId)
        {
            if (_atomUID == oldId)
            {
                _atomUID = newId;
            }
        }

        /// <summary>
        /// 运行动作
        /// </summary>
        /// <param name="value"></param>
        public void Trigger(string value)
        {
            if (Receiver != null)
            {
                // 如果是动作或其他类型的目标，直接执行
                if (TargetType == TriggerEventType.Action || TargetType == TriggerEventType.Other)
                {
                    Receiver.CallAction(Target);

                    return;
                }

                var actionParamJSON = Receiver.GetParam(Target);

                if (actionParamJSON == null)
                {
                    LogUtil.LogWarning("Trigger Event Action (" + this.Name + ") was unable to load target (" + Target + ")");
                }
                else
                {
                    var useValue = (TargetType == _trigger.EventType && !ValueCustom);

                    switch (TargetType)
                    {
                        case TriggerEventType.Bool:
                            var actionBoolJSON = actionParamJSON as JSONStorableBool;
                            if (useValue)
                            {
                                bool v;

                                if (bool.TryParse(value, out v))
                                {
                                    actionBoolJSON.val = v;
                                }
                            }
                            else
                            {
                                actionBoolJSON.val = ValueOfBool;
                            }
                            break;
                        case TriggerEventType.Float:
                            var actionFloatJSON = actionParamJSON as JSONStorableFloat;
                            if (useValue)
                            {
                                float f;

                                if (float.TryParse(value, out f))
                                {
                                    actionFloatJSON.val = f;
                                }
                            }
                            else
                            {
                                actionFloatJSON.val = ValueOfFloat;
                            }
                            break;
                        case TriggerEventType.String:
                            var actionStringJSON = actionParamJSON as JSONStorableString;
                            if (useValue)
                            {
                                actionStringJSON.val = value;
                            }
                            else
                            {
                                actionStringJSON.val = ValueOfString;
                            }
                            break;
                        case TriggerEventType.Chooser:
                            var actionChooserJSON = actionParamJSON as JSONStorableStringChooser;
                            if (useValue)
                            {
                                actionChooserJSON.val = value;
                            }
                            else
                            {
                                actionChooserJSON.val = ValueOfString;
                            }
                            break;
                        default:
                            LogUtil.LogWarning("Trigger Event Action Type (" + TriggerEventType.GetName(TargetType) + ") was unable to load target (" + Target + ")");
                            break;
                    }
                }
            }
        }

        public void Dispose()
        {
            //_actionList
        }
    }
}
