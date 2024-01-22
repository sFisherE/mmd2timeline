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
        /// 触发浮点类型的动作
        /// </summary>
        /// <param name="value"></param>
        /// <param name="min"></param>
        /// <param name="max"></param>
        public void Trigger(float value, float min, float max)
        {
            if (Receiver != null)
            {
                if (TargetType == TriggerEventType.Float)
                {
                    var actionFloatJSON = Receiver.GetFloatJSONParam(Target);

                    if (actionFloatJSON != null)
                    {
                        if (ValueCustom)
                        {
                            actionFloatJSON.val = ValueOfFloat;
                        }
                        else
                        {
                            var m = value / (max - min);

                            actionFloatJSON.val = m;
                        }
                    }
                }
            }
        }

        /// <summary>
        /// 触发选择器事件
        /// </summary>
        /// <param name="value"></param>
        /// <param name="choices"></param>
        public void Trigger(string value, List<string> choices)
        {
            if (Receiver != null)
            {
                if (TargetType == TriggerEventType.Chooser)
                {
                    var actionChooserJSON = Receiver.GetStringChooserJSONParam(Target);
                    if (actionChooserJSON != null)
                    {
                        if (ValueCustom)
                        {
                            actionChooserJSON.val = ValueOfString;
                        }
                        else
                        {
                            actionChooserJSON.choices = choices;
                            actionChooserJSON.val = value;
                        }
                    }
                }
            }
        }

        /// <summary>
        /// 触发布尔事件
        /// </summary>
        /// <param name="value"></param>
        /// <param name="choices"></param>
        public void Trigger(bool value)
        {
            if (Receiver != null)
            {
                if (TargetType == TriggerEventType.Bool)
                {
                    var actionBoolJSON = Receiver.GetBoolJSONParam(Target);

                    if (actionBoolJSON != null)
                    {
                        if (ValueCustom)
                        {
                            actionBoolJSON.val = ValueOfBool;
                        }
                        else
                        {
                            actionBoolJSON.val = value;
                        }
                    }
                }
            }
        }

        /// <summary>
        /// 触发动作事件
        /// </summary>
        /// <param name="value"></param>
        /// <param name="choices"></param>
        public void Trigger()
        {
            if (Receiver != null)
            {
                if (TargetType == TriggerEventType.Action || TargetType == TriggerEventType.Other)
                {
                    Receiver.CallAction(Target);

                    return;
                }
                var actionParam = Receiver.GetParam(Target);

                if (actionParam == null)
                    return;

                switch (TargetType)
                {
                    case TriggerEventType.Bool:
                        var actionBoolJSON = actionParam as JSONStorableBool;
                        if (actionBoolJSON != null)
                        {
                            actionBoolJSON.val = ValueOfBool;
                        }
                        break;
                    case TriggerEventType.Float:
                        var actionFloatJSON = actionParam as JSONStorableFloat;
                        if (actionFloatJSON != null)
                        {
                            actionFloatJSON.val = ValueOfFloat;
                        }
                        break;
                    case TriggerEventType.Chooser:
                        var actionChooserJSON = actionParam as JSONStorableStringChooser;
                        if (actionChooserJSON != null)
                        {
                            actionChooserJSON.val = ValueOfString;
                        }
                        break;
                    case TriggerEventType.String:
                        var actionStringJSON = actionParam as JSONStorableString;
                        if (actionStringJSON != null)
                        {
                            actionStringJSON.val = ValueOfString;
                        }
                        break;
                }
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
                if (TargetType == TriggerEventType.String)
                {
                    var actionStringJSON = Receiver.GetStringJSONParam(Target);
                    if (actionStringJSON != null)
                    {
                        if (ValueCustom)
                        {
                            actionStringJSON.val = ValueOfString;
                        }
                        else
                        {
                            actionStringJSON.val = value;
                        }
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
