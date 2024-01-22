using MacGruber;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace mmd2timeline
{
    internal partial class Trigger
    {
        /// <summary>
        /// 触发器事件助手
        /// </summary>
        TriggerEventHelper _triggerEventHelper = TriggerEventHelper.GetInstance();

        /// <summary>
        /// 触发器UI容器
        /// </summary>
        Dictionary<string, object> _trigerButtons = new Dictionary<string, object>();
        /// <summary>
        /// 动作UI
        /// </summary>
        List<object> _actionUI = new List<object>();
        /// <summary>
        /// 值UI容器
        /// </summary>
        List<object> _valueUI = new List<object>();

        #region 变量

        /// <summary>
        /// 默认触发器设置标题
        /// </summary>
        const string _DEFAULT_TRIGGER_SETTINGS_TITLE = "Select trigger on the left side";
        /// <summary>
        /// 触发器设置标题
        /// </summary>
        UIDynamicTextInfo _triggerSettingsTitle;
        /// <summary>
        /// 动作标题
        /// </summary>
        UIDynamicTextInfo _actionTitle;
        /// <summary>
        /// 动作选择器
        /// </summary>
        JSONStorableStringChooser _actionChooser;
        /// <summary>
        /// 目标原子选择器
        /// </summary>
        JSONStorableStringChooser _atomChooser;
        /// <summary>
        /// 接收者选择器
        /// </summary>
        JSONStorableStringChooser _receiverChooser;
        /// <summary>
        /// 目标选择器
        /// </summary>
        JSONStorableStringChooser _targetChooser;
        /// <summary>
        /// 值类型选择器
        /// </summary>
        JSONStorableStringChooser _valueSourceChooser;
        /// <summary>
        /// 布尔值设置组件
        /// </summary>
        JSONStorableStringChooser _valueBoolSetter;
        /// <summary>
        /// 字符串选择器设置组件
        /// </summary>
        JSONStorableStringChooser _valueStringChooserSetter;
        /// <summary>
        /// 浮点值设置组件
        /// </summary>
        JSONStorableFloat _valueFloatSetter;
        /// <summary>
        /// 字符串值设置组件
        /// </summary>
        UIDynamicLabelInput _valueStringSetter;
        /// <summary>
        /// 字符串值保存对象
        /// </summary>
        JSONStorableString _valueStringJSON;

        /// <summary>
        /// 当前触发器
        /// </summary>
        TriggerEvent _currentTrigger;

        /// <summary>
        /// 当前触发器名称
        /// </summary>
        string _currentTriggerName;
        /// <summary>
        /// 当前触发器名称
        /// </summary>
        string CurrrentTriggerName
        {
            get
            {
                return _currentTriggerName;
            }
            set
            {
                if (_currentTriggerName != value)
                {
                    // 如果存在当前触发器，则清除事件绑定
                    if (_currentTrigger != null)
                    {
                        SetTriggerButtonColor(_currentTriggerName, false);
                        _currentTrigger.OnActionListChanged -= OnActionListChanged;
                        _currentTrigger.OnActionValueChanged -= OnActionValueChanged;
                        _currentTrigger.OnActionAdded -= OnActionAdded;
                        _currentTrigger.OnActionRemoved -= OnActionRemoved;
                        _currentTrigger.OnActionMoved -= OnActionMoved;
                    }

                    _currentTriggerName = value;

                    SetTriggerButtonColor(_currentTriggerName, true);

                    TriggerSelected();
                }
            }
        }

        private void OnActionValueChanged(TriggerEventAction action, int index)
        {
            //var displayChoices = _actionChooser.displayChoices.ToList();
            //displayChoices[index] = action.Name;
            //var choices = _actionChooser.choices.ToList();

            //var tempString = "TEMP";

            //choices.Add(tempString);
            //_actionChooser.choices = choices;
            //_actionChooser.valNoCallback = "TEMP";
            //choices.Remove("TEMP");

            //// TODO 当前动作变更
            //_actionChooser.displayChoices = displayChoices;
            //_actionChooser.choices = choices;
            //_actionChooser.valNoCallback = null;
            //_actionChooser.valNoCallback = $"{index}";

            if (_atomChooser.val != action.AtomUID)
            {
                // TODO 原子ID更改
                LogUtil.Debug($"OnActionChanged::AtomUID:{_atomChooser.val} <to> {action.AtomUID}");
            }

            if (_receiverChooser.val != action.ReceiverName)
            {
                // TODO 接收器被更改
                LogUtil.Debug($"OnActionChanged::ReceiverName:{_receiverChooser.val} <to> {action.ReceiverName}");
            }

            if (_targetChooser.val != action.Target)
            {
                // 目标更改
                LogUtil.Debug($"OnActionChanged::Target:{_targetChooser.val} <to> {action.Target}");
            }

            var iscustom = _valueSourceChooser.val != _valueSourceChooser.defaultVal;

            if (action.ValueCustom != iscustom)
            {
                //自定义更改
                LogUtil.Debug($"OnActionChanged::ValueCustom:{iscustom} <to> {action.ValueCustom}");
            }

            if (iscustom)
            {
                switch (action.TargetType)
                {
                    case TriggerEventType.Bool:
                        if (bool.Parse(_valueBoolSetter.val) != action.ValueOfBool)
                        {
                            //布尔值更改
                            LogUtil.Debug($"OnActionChanged::ValueOfBool:{_valueBoolSetter.val} <to> {action.ValueOfBool}");
                        }
                        break;
                    case TriggerEventType.Float:
                        if (_valueFloatSetter.val != action.ValueOfFloat)
                        {
                            //浮点值更改
                            LogUtil.Debug($"OnActionChanged::ValueOfFloat:{_valueFloatSetter.val} <to> {action.ValueOfFloat}");
                        }
                        break;
                    case TriggerEventType.Chooser:
                        if (_valueStringChooserSetter.val != action.ValueOfString)
                        {
                            //选择器更改
                            LogUtil.Debug($"OnActionChanged::ValueOfString:{_valueStringChooserSetter.val} <to> {action.ValueOfString}");
                        }
                        break;
                    case TriggerEventType.String:
                        if (_valueStringJSON.val != action.ValueOfString)
                        {
                            //字符串值更改
                            LogUtil.Debug($"OnActionChanged::ValueOfString:{_valueStringJSON.val} <to> {action.ValueOfString}");
                        }
                        break;
                }
            }
        }

        ///// <summary>
        ///// 当前目标类型
        ///// </summary>
        //int _currentTargetType = TriggerEventType.Other;
        /// <summary>
        /// 当前动作序号
        /// </summary>
        int _currentActionIdex = 0;
        #endregion

        public override void Init()
        {
            base.Init();
            InitScript();
            InitActionsUI();
        }

        void Start()
        {
            ShowActionUI(false);
            ShowValueUI(false);
            OnTriggerListChanged();
        }

        /// <summary>
        /// 显示或隐藏动作设置UI
        /// </summary>
        /// <param name="show"></param>
        void ShowActionUI(bool show = true)
        {
            // 默认隐藏掉所有值UI
            ShowUIElements(_actionUI, show);
        }

        /// <summary>
        /// 显示或隐藏值UI
        /// </summary>
        /// <param name="show"></param>
        void ShowValueUI(bool show = true)
        {
            // 隐藏UI时重置值设置
            if (!show)
            {
                _valueBoolSetter.valNoCallback = _valueBoolSetter.defaultVal;
                _valueStringChooserSetter.valNoCallback = null;
                _valueFloatSetter.valNoCallback = 0f;
                _valueStringJSON.valNoCallback = null;
            }
            // 隐藏掉所有值设置相关的UI
            ShowUIElements(_valueUI, show);
        }

        /// <summary>
        /// 初始化动作UI
        /// </summary>
        void InitActionsUI()
        {
            _triggerSettingsTitle = CreateTitleUI(_DEFAULT_TRIGGER_SETTINGS_TITLE, RightSide);

            // 动作选择器
            _actionChooser = Utils.SetupStringChooser(this, $"Trigger Actions", new List<string>(), RightSide);
            _actionChooser.setCallbackFunction = a =>
            {
                ResetActionsUI();

                _currentActionIdex = int.Parse(a);
                _currentTriggerEventAction = _currentTrigger.GetAction(_currentActionIdex);

                if (_currentTriggerEventAction != null)
                {
                    _actionTitle.text.text = _currentTriggerEventAction.Name;

                    var choices = SuperController.singleton.GetAtomUIDs().ToList();
                    _atomChooser.choices = choices;

                    if (choices.Contains(_currentTriggerEventAction.AtomUID))
                    {
                        _atomChooser.valNoCallback = _currentTriggerEventAction.AtomUID;
                    }
                    else
                    {
                        _atomChooser.valNoCallback = choices.First();
                        _currentTriggerEventAction.SetAtomUID(_atomChooser.val);
                    }

                    RefreshReceivers();
                }
                else
                {
                    _actionTitle.text.text = $"None";
                }
            };
            // 添加移除动作按钮
            var addAndRemoveActionsButton = Utils.SetupTwinButton(this, "Add New Action", AddActionToTrigger, "Remove Action", RemoveActionFromTrigger, RightSide);
            // 上下移动动作按钮
            var upAndDownActionsButton = Utils.SetupTwinButton(this, "Up", ActionMoveUp, "Down", ActionMoveDown, RightSide);

            //_actionTitle = CreateTitleUI($"", RightSide);

            _actionTitle = Utils.SetupInfoTextNoScroll(this, $"", 180f, RightSide);
            _actionTitle.text.alignment = UnityEngine.TextAnchor.MiddleCenter;
            _actionTitle.text.fontStyle = UnityEngine.FontStyle.Bold;


            // 目标原子选择器
            _atomChooser = Utils.SetupStringChooser(this, $"Atom", new List<string>(), RightSide);
            _atomChooser.setCallbackFunction = uid =>
            {
                _currentTriggerEventAction.SetAtomUID(uid);
                RefreshReceivers();
            };

            _receiverChooser = Utils.SetupStringChooser(this, $"Receiver", new List<string>(), RightSide);
            _receiverChooser.setCallbackFunction = storeId =>
            {
                _currentTriggerEventAction.SetReceiverName(storeId);
                RefreshTargets();
            };

            _targetChooser = Utils.SetupStringChooser(this, $"Target", new List<string>(), RightSide);
            _targetChooser.setCallbackFunction = target =>
            {
                _currentTriggerEventAction.SetTarget(target);
                RefreshValueSource();
            };

            _valueSourceChooser = Utils.SetupStringChooser(this, $"Value Source", new List<string> { $"From Plugin", $"Custom" }, RightSide);
            _valueSourceChooser.setCallbackFunction = type =>
            {
                // 更新值
                _currentTriggerEventAction.SetValueCustom(type != _valueSourceChooser.defaultVal);
                RefreshValues();
            };

            _valueBoolSetter = Utils.SetupStringChooser(this, $"Bool Value", new List<string> { $"True", $"False" }, RightSide);
            _valueBoolSetter.setCallbackFunction = BoolValueChanged;
            _valueStringChooserSetter = Utils.SetupStringChooser(this, $"String Chooser Value", new List<string>(), RightSide);
            _valueStringChooserSetter.setCallbackFunction = ChooserValueChanged;
            _valueFloatSetter = Utils.SetupSliderFloat(this, $"Float Value", 0f, 0f, 1f, RightSide);
            _valueFloatSetter.setCallbackFunction = FloatValueChanged;
            _valueStringJSON = new JSONStorableString($"Setted String Value", null);
            _valueStringJSON.setCallbackFunction = StringValueChanged;
            _valueStringSetter = Utils.SetupTextInput(this, $"String Value", _valueStringJSON, RightSide);
            _valueStringSetter.height = 100f;
            _valueStringSetter.input.textComponent.alignment = UnityEngine.TextAnchor.MiddleCenter;
            _valueStringSetter.input.textComponent.fontStyle = UnityEngine.FontStyle.Bold;

            var spacer = Utils.SetupSpacer(this, 60f, RightSide);

            var testButton = Utils.SetupButton(this, $"Test Action", () =>
            {
                _currentTriggerEventAction.Trigger($"True");
            }, RightSide);

            _actionUI = new List<object> { _actionChooser, addAndRemoveActionsButton, upAndDownActionsButton, _actionTitle, _atomChooser, _receiverChooser, _targetChooser, spacer, testButton };

            _valueUI = new List<object> { _valueSourceChooser, _valueBoolSetter, _valueStringChooserSetter, _valueFloatSetter, _valueStringSetter };
        }

        void BoolValueChanged(string value)
        {
            _currentTriggerEventAction.SetValue(bool.Parse(value));
            // 设置动作值
            _currentTriggerEventAction.UpdateValue();
        }

        void ChooserValueChanged(string value)
        {
            // 设置动作值
            _currentTriggerEventAction.SetValue(value);
            _currentTriggerEventAction.UpdateValue();
        }

        void FloatValueChanged(float value)
        {
            // 设置动作值
            _currentTriggerEventAction.SetValue(value);
            _currentTriggerEventAction.UpdateValue();
        }

        void StringValueChanged(string value)
        {
            // 设置动作值
            _currentTriggerEventAction.SetValue(value);
            _currentTriggerEventAction.UpdateValue();
        }

        /// <summary>
        /// 重置动作UI
        /// </summary>
        void ResetActionsUI()
        {
            _atomChooser.valNoCallback = null;
            _receiverChooser.valNoCallback = null;
            _targetChooser.valNoCallback = null;
            _valueSourceChooser.valNoCallback = _valueSourceChooser.defaultVal;
            _valueBoolSetter.valNoCallback = _valueBoolSetter.defaultVal;
            _valueStringChooserSetter.valNoCallback = null;
            _valueFloatSetter.valNoCallback = 0f;
            _valueStringJSON.valNoCallback = null;
        }

        /// <summary>
        /// 事件列表更改的事件处理函数
        /// </summary>
        private void OnTriggerListChanged()
        {
            CurrrentTriggerName = null;

            var triggers = _triggerEventHelper.GetTriggerNames();

            foreach (var trigger in triggers)
            {
                SetTriggerButton(trigger.Key, trigger.Value);
            }
        }

        Color _buttonDefaultColor = new Color(1f, 1f, 1f, 0.3f);
        Color _buttonActiveColor = new Color(1f, 1f, 0f, 0.3f);

        /// <summary>
        /// 创建事件按钮
        /// </summary>
        /// <param name="name"></param>
        /// <param name="value"></param>
        void SetTriggerButton(string name, int count)
        {
            var tip = count > 0 ? $"{count}" : $"off";

            var button = GetTriggerButton(name);

            if (button == null)
            {
                button = Utils.SetupButton(this, $"{name} ({tip})", () => { CurrrentTriggerName = name; }, LeftSide);
                button.buttonColor = _buttonDefaultColor;
                _trigerButtons.Add(name, button);
            }
            else
            {
                button.buttonText.text = $"{name} ({tip})";
            }
        }

        /// <summary>
        /// 设置触发器按钮颜色
        /// </summary>
        /// <param name="name"></param>
        /// <param name="active"></param>
        void SetTriggerButtonColor(string name, bool active = true)
        {
            var button = GetTriggerButton(name);
            if (active)
            {
                button.buttonColor = _buttonActiveColor;
            }
            else
            {
                button.buttonColor = _buttonDefaultColor;
            }
        }

        /// <summary>
        /// 获取当前触发器按钮
        /// </summary>
        /// <returns></returns>
        UIDynamicButton GetTriggerButton(string name = null)
        {
            object button;

            if (_trigerButtons.TryGetValue(name ?? _currentTriggerName, out button))
            {
                return (UIDynamicButton)button;
            }

            return null;
        }

        /// <summary>
        /// 同步触发器选择
        /// </summary>
        /// <param name="name"></param>
        void TriggerSelected()
        {
            try
            {
                if (string.IsNullOrEmpty(_currentTriggerName))
                {
                    _triggerSettingsTitle.text.text = _DEFAULT_TRIGGER_SETTINGS_TITLE;
                    ShowActionUI(false);
                    ShowValueUI(false);
                    return;
                }

                var trigger = _triggerEventHelper.GetTrigger(_currentTriggerName);

                if (trigger != null)
                {
                    _currentTrigger = trigger;
                    _currentTrigger.OnActionListChanged -= OnActionListChanged;
                    _currentTrigger.OnActionValueChanged -= OnActionValueChanged;
                    _currentTrigger.OnActionAdded -= OnActionAdded;
                    _currentTrigger.OnActionRemoved -= OnActionRemoved;
                    _currentTrigger.OnActionMoved -= OnActionMoved;

                    _currentTrigger.OnActionListChanged += OnActionListChanged;
                    _currentTrigger.OnActionValueChanged += OnActionValueChanged;
                    _currentTrigger.OnActionAdded += OnActionAdded;
                    _currentTrigger.OnActionRemoved += OnActionRemoved;
                    _currentTrigger.OnActionMoved += OnActionMoved;

                    _triggerSettingsTitle.text.text = $"{_currentTriggerName} Settings";
                    ShowActionUI(true);
                    var actions = _currentTrigger.GetActions();
                    SetTriggerButton(trigger.Name, actions.Count);
                    RefreshActionList(actions);
                }
                else
                {
                    _currentTrigger = null;
                    ShowActionUI(false);
                    ShowValueUI(false);
                }
            }
            catch (Exception ex)
            {
                LogUtil.LogError(ex);
            }
        }

        private void OnActionMoved(TriggerEvent sender, TriggerEventAction action, int index)
        {
            var actions = sender.GetActions();
            SetTriggerButton(sender.Name, actions.Count);

            if (sender == _currentTrigger)
            {
                RefreshActionList(actions, index);
            }
        }

        private void OnActionRemoved(TriggerEvent sender, TriggerEventAction action, int index)
        {
            var actions = sender.GetActions();
            SetTriggerButton(sender.Name, actions.Count);

            if (sender == _currentTrigger)
            {
                RefreshActionList(actions, index - 1);
            }
        }

        private void OnActionAdded(TriggerEvent sender, TriggerEventAction action, int index)
        {
            var actions = sender.GetActions();
            SetTriggerButton(sender.Name, actions.Count);

            if (sender == _currentTrigger)
            {
                RefreshActionList(actions, index);
            }
        }

        /// <summary>
        /// 触发器动作列表更改事件处理方法
        /// </summary>
        /// <param name="list"></param>
        private void OnActionListChanged(TriggerEvent sender, List<TriggerEventAction> list)
        {
            SetTriggerButton(sender.Name, list.Count);

            RefreshActionList(list);
        }

        /// <summary>
        /// 刷新动作列表
        /// </summary>
        /// <param name="actions"></param>
        void RefreshActionList(List<TriggerEventAction> actions, int selectIndex = -1)
        {
            var actionNameList = new List<string>();
            var actionIndexList = new List<string>();

            for (var i = 0; i < actions.Count; i++)
            {
                actionNameList.Add($"Action {i}");
                actionIndexList.Add($"{i}");
            }

            _actionChooser.choices = actionIndexList;
            _actionChooser.displayChoices = actionNameList;

            if (selectIndex < 0)
            {
                var index = _actionChooser.val;
                _actionChooser.valNoCallback = null;

                if (string.IsNullOrEmpty(index))
                {
                    index = actionIndexList.FirstOrDefault();
                }
                _actionChooser.val = index;
            }
            else
            {
                _actionChooser.valNoCallback = null;

                _actionChooser.val = $"{selectIndex}";
            }
        }

        /// <summary>
        /// 刷新值
        /// </summary>
        private void RefreshValues()
        {
            var isCustom = _valueSourceChooser.val != _valueSourceChooser.defaultVal;

            ShowValueUI(false);

            var needShowUIs = new List<object>();
            if (CurrentTargetCanSetByPlugin)
            {
                needShowUIs.Add(_valueSourceChooser);
            }

            try
            {
                bool needUpdateValue = false;
                var targetType = _currentTriggerEventAction.TargetType;
                // 自定义值
                if (isCustom)
                {
                    object valueUI = null;
                    var target = _targetChooser.val;

                    var receiver = _currentTriggerEventAction.Receiver;

                    switch (targetType)
                    {
                        case TriggerEventType.Bool:
                            valueUI = _valueBoolSetter;
                            _valueBoolSetter.val = _currentTriggerEventAction.ValueOfBool ? _valueBoolSetter.choices.FirstOrDefault() : _valueBoolSetter.choices.Last();
                            break;
                        case TriggerEventType.Float:
                            valueUI = _valueFloatSetter;
                            var targetFloat = receiver.GetFloatJSONParam(target);
                            if (targetFloat != null)
                                _valueFloatSetter.val = _currentTriggerEventAction.ValueOfFloat != 0f ? _currentTriggerEventAction.ValueOfFloat : targetFloat.val;
                            break;
                        case TriggerEventType.String:
                            valueUI = _valueStringSetter;
                            var targetString = receiver.GetStringJSONParam(target);
                            if (targetString != null)
                                _valueStringSetter.input.text = _currentTriggerEventAction.ValueOfString ?? targetString.val;
                            break;
                        case TriggerEventType.Chooser:
                            valueUI = _valueStringChooserSetter;

                            var targetChooser = receiver.GetStringChooserJSONParam(target);
                            if (targetChooser != null)
                            {
                                _valueStringChooserSetter.choices = targetChooser.choices;
                                _valueStringChooserSetter.val = _currentTriggerEventAction.ValueOfString ?? targetChooser.val;
                            }
                            break;
                        default:
                            needUpdateValue = true;
                            break;
                    }

                    if (valueUI != null)
                    {
                        needShowUIs.Add(valueUI);
                    }
                }
                else
                {
                    needUpdateValue = true;
                }

                if (needUpdateValue)
                {
                    // 设置动作值
                    _currentTriggerEventAction.UpdateValue();
                }
            }
            catch (Exception ex)
            {
                LogUtil.LogError(ex);
            }
            ShowUIElements(needShowUIs, true);
        }

        /// <summary>
        /// 获取当前目标是否可以使用Plugin进行设置
        /// </summary>
        bool CurrentTargetCanSetByPlugin
        {
            get
            {
                var targetType = _currentTriggerEventAction.TargetType;
                switch (targetType)
                {
                    case TriggerEventType.Bool:
                    case TriggerEventType.Float:
                    case TriggerEventType.String:
                    case TriggerEventType.Chooser:
                        return _currentTrigger.EventType == targetType;
                    default:
                        return false;
                }
            }
        }

        /// <summary>
        /// 刷新值来源
        /// </summary>
        private void RefreshValueSource()
        {
            var isCustom = _currentTriggerEventAction.ValueCustom;

            if (isCustom || !CurrentTargetCanSetByPlugin)
            {
                _valueSourceChooser.valNoCallback = _valueSourceChooser.choices.Last();
            }
            else
            {
                _valueSourceChooser.valNoCallback = _valueSourceChooser.choices.First();
            }

            // 是否设定为自定义
            var setCustom = _valueSourceChooser.val != _valueSourceChooser.defaultVal;

            if (isCustom != setCustom)
            {
                _currentTriggerEventAction.SetValueCustom(setCustom);
            }

            RefreshValues();
        }

        /// <summary>
        /// 刷新目标
        /// </summary>
        private void RefreshTargets()
        {
            var choices = GetActionTargets(_currentTriggerEventAction.Receiver);

            if (_targetChooser != null)
            {
                _targetChooser.choices = new List<string>();
                _targetChooser.choices = choices;

                if (choices.Contains(_currentTriggerEventAction.Target))
                {
                    _targetChooser.valNoCallback = _currentTriggerEventAction.Target;
                }
                else
                {
                    _targetChooser.valNoCallback = choices.First();
                    _currentTriggerEventAction.SetTarget(_targetChooser.val);
                }

                RefreshValueSource();
            }
        }

        /// <summary>
        /// 获取动作目标
        /// </summary>
        /// <param name="receiver"></param>
        /// <param name="noActionOption"></param>
        /// <param name="noAudioClipActions"></param>
        /// <returns></returns>
        List<string> GetActionTargets(JSONStorable receiver)
        {
            List<string> targetChoices = new List<string>();

            var boolChoices = receiver.GetBoolParamNames();
            var floatChoices = receiver.GetFloatParamNames();
            var stringChooserChoices = receiver.GetStringChooserParamNames();
            var stringChoices = receiver.GetStringParamNames();
            var actionChoices = receiver.GetActionNames();

            List<string> tempChoices = new List<string>();
            List<string> temp2Choices = new List<string>();

            foreach (string choice in actionChoices)
            {
                if ((choice != "SaveToStore1" && choice != "SaveToStore2" && choice != "SaveToStore3" && choice != "RestoreAllFromStore1" && choice != "RestoreAllFromStore2" && choice != "RestoreAllFromStore3" && choice != "RestorePhysicsFromStore1" && choice != "RestorePhysicsFromStore2" && choice != "RestorePhysicsFromStore3" && choice != "RestoreAppearanceFromStore1" && choice != "RestoreAppearanceFromStore2" && choice != "RestoreAppearanceFromStore3" && choice != "RestoreAllFromDefaults" && choice != "RestorePhysicalFromDefaults" && choice != "RestoreAppearanceFromDefaults"))
                {
                    if (AddTargetChoices(choice))
                    {
                        targetChoices.Add(choice);
                    }
                }
            }
            foreach (string choice in boolChoices)
            {
                if (AddTargetChoices(choice))
                {
                    if (choice.Length >= 6 && choice.Substring(choice.Length - 6, 6) == "Result")
                    {
                        tempChoices.Add(choice);
                    }
                    else
                    {
                        temp2Choices.Add(choice);
                    }
                }
            }
            foreach (string choice in stringChooserChoices)
            {
                if (AddTargetChoices(choice))
                {
                    if (choice.Length >= 6 && choice.Substring(choice.Length - 6, 6) == "Result")
                    {
                        tempChoices.Add(choice);
                    }
                    else
                    {
                        temp2Choices.Add(choice);
                    }
                }
            }
            foreach (string choice in floatChoices)
            {
                if (AddTargetChoices(choice))
                {
                    if (choice.Length >= 6 && choice.Substring(choice.Length - 6, 6) == "Result")
                    {
                        tempChoices.Add(choice);
                    }
                    else
                    {
                        temp2Choices.Add(choice);
                    }
                }
            }
            foreach (string choice in tempChoices)
            {
                targetChoices.Add(choice);
            }
            foreach (string choice in stringChoices)
            {
                if (AddTargetChoices(choice))
                {
                    targetChoices.Add(choice);
                }
            }
            temp2Choices.Sort();
            foreach (string choice in temp2Choices)
            {
                targetChoices.Add(choice);
            }

            if (targetChoices.Count == 0)
            {
                targetChoices.Add("None");
            }
            return targetChoices;
        }

        protected bool AddTargetChoices(string choice)
        {
            Atom atom = _currentTriggerEventAction.Atom;

            if (atom != null)
            {
                if (atom.category == "People" && _receiverChooser.val == "geometry")
                {
                    if (choice.Contains("clothing:") || choice.Contains("toggle:") || choice.Contains("hair") || choice.Contains("Colliders"))
                    {
                        return (true);
                    }
                }
                else
                {
                    return (true);
                }
            }
            return (false);

        }

        /// <summary>
        /// 刷新接收者
        /// </summary>
        /// <param name="atomUID"></param>
        private void RefreshReceivers()
        {
            List<string> choices = GetAtomReceivers(_currentTriggerEventAction.AtomUID);

            if (_receiverChooser != null)
            {
                _receiverChooser.choices = new List<string>();
                _receiverChooser.choices = choices;

                if (choices.Contains(_currentTriggerEventAction.ReceiverName))
                {
                    _receiverChooser.valNoCallback = _currentTriggerEventAction.ReceiverName;
                }
                else
                {
                    _receiverChooser.valNoCallback = choices.First();
                    _currentTriggerEventAction.SetReceiverName(_receiverChooser.val);
                }

                RefreshTargets();
            }
        }

        /// <summary>
        /// 获取原子的接收器列表
        /// </summary>
        /// <param name="atomUID"></param>
        /// <returns></returns>
        List<string> GetAtomReceivers(string atomUID)
        {
            List<string> receivers = new List<string>();
            var atom = SuperController.singleton.GetAtomByUid(atomUID);

            if (atom == null)
            {
                LogUtil.LogWarning("Atom is null in GetAtomReceivers: " + atomUID);
            }
            else
            {
                foreach (string receiverChoice in atom.GetStorableIDs())
                {
                    if (atom.GetStorableByID(receiverChoice)?.GetActionNames()?.Count > 0 || atom.GetStorableByID(receiverChoice)?.GetBoolParamNames()?.Count > 0 || atom.GetStorableByID(receiverChoice)?.GetStringChooserParamNames()?.Count > 0 || atom.GetStorableByID(receiverChoice)?.GetFloatParamNames()?.Count > 0 || atom.GetStorableByID(receiverChoice)?.GetColorParamNames()?.Count > 0)
                    {
                        receivers.Add(receiverChoice);
                    }
                }
            }
            return receivers;
        }

        TriggerEventAction _currentTriggerEventAction;

        void AddActionToTrigger()
        {
            _currentTrigger?.AddAction();
        }
        void RemoveActionFromTrigger()
        {
            _currentTrigger?.RemoveAction(_currentActionIdex);
        }

        void ActionMoveUp()
        {
            _currentTrigger?.MoveAction(_currentActionIdex, up: true);
        }

        void ActionMoveDown()
        {
            _currentTrigger?.MoveAction(_currentActionIdex, up: false);
        }
    }
}
