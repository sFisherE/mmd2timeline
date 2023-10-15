using MacGruber;
using System;
using System.Collections.Generic;
using System.Linq;

namespace mmd2timeline
{
    internal partial class Settings : BaseScript
    {
        #region UI控件列表，用于存储UI上设定的参数，便于参数重置
        List<JSONStorableBool> _StorableBools = new List<JSONStorableBool>();
        List<JSONStorableFloat> _StorableFloats = new List<JSONStorableFloat>();
        List<JSONStorableStringChooser> _StorableStrings = new List<JSONStorableStringChooser>();
        #endregion

        public override bool ShouldIgnore()
        {
            return false;
        }

        /// <summary>
        /// 首次调用Update之前调用的方法
        /// </summary>
        public virtual void Start()
        {
            try
            {
                InitScript();

                InitSettingUI();
            }
            catch (Exception ex)
            {
                LogUtil.LogError(ex, "GeneralSettings::Start:");
            }
        }

        private Atom _WindowCameraAtom;

        /// <summary>
        /// 获取WindowCamera原子
        /// </summary>
        protected Atom WindowCameraAtom
        {
            get
            {
                if (_WindowCameraAtom == null)
                {
                    _WindowCameraAtom = SuperController.singleton.GetAtoms().FirstOrDefault(a => a.type == "WindowCamera");
                }
                return _WindowCameraAtom;
            }
        }

        /// <summary>
        /// 设置到默认值
        /// </summary>
        private void SetValuesToDefalut()
        {
            foreach (var item in _StorableBools) { item.SetValToDefault(); }
            foreach (var item in _StorableFloats) { item.SetValToDefault(); }
            foreach (var item in _StorableStrings) { item.SetValToDefault(); }
        }

        #region 设置UI控件的方法，包装一层主要用于参数重置
        /// <summary>
        /// 设置Slider
        /// </summary>
        /// <param name="label"></param>
        /// <param name="defaultValue"></param>
        /// <param name="minValue"></param>
        /// <param name="maxValue"></param>
        /// <param name="callback"></param>
        /// <param name="rightSide"></param>
        /// <param name="valueFormat"></param>
        private JSONStorableFloat SetupSliderFloat(float value, string label, float defaultValue, float minValue, float maxValue, Action<float> callback, bool rightSide, string valueFormat = "")
        {
            var slider = Utils.SetupSliderFloat(this, Lang.Get(label), defaultValue, minValue, maxValue, callback, rightSide, valueFormat);
            slider.val = value;

            _StorableFloats.Add(slider);

            return slider;
        }

        /// <summary>
        /// 设置Toggle
        /// </summary>
        /// <param name="label"></param>
        /// <param name="defaultValue"></param>
        /// <param name="callback"></param>
        /// <param name="rightSide"></param>
        private JSONStorableBool SetupToggle(bool value, string label, bool defaultValue, Action<bool> callback, bool rightSide)
        {
            var toggle = Utils.SetupToggle(this, Lang.Get(label), defaultValue, callback, rightSide);
            toggle.val = value;

            _StorableBools.Add(toggle);

            return toggle;
        }

        /// <summary>
        /// 设置Toggle
        /// </summary>
        /// <param name="label"></param>
        /// <param name="defaultValue"></param>
        /// <param name="rightSide"></param>
        /// <returns></returns>
        private JSONStorableBool SetupToggle(bool value, string label, bool defaultValue, bool rightSide)
        {
            var toggle = Utils.SetupToggle(this, Lang.Get(label), defaultValue, rightSide);
            toggle.val = value;

            _StorableBools.Add(toggle);

            return toggle;
        }

        ///// <summary>
        ///// 设置选择器
        ///// </summary>
        ///// <typeparam name="TEnum"></typeparam>
        ///// <param name="self"></param>
        ///// <param name="label"></param>
        ///// <param name="defaultValue"></param>
        ///// <param name="rightSide"></param>
        ///// <param name="callback"></param>
        //private JSONStorableStringChooser SetupEnumChooser<TEnum>(TEnum value, string label, TEnum defaultValue, bool rightSide, EnumSetCallback<TEnum> callback)
        //    where TEnum : struct, IComparable, IConvertible, IFormattable
        //{
        //    var chooser = SetupEnumChooser(label, defaultValue, rightSide, callback);
        //    chooser.val = Lang.Get(value.ToString());

        //    _StorableStrings.Add(chooser);
        //    return chooser;
        //}

        /// <summary>
        /// 设置静态枚举类的选择器
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="value"></param>
        /// <param name="label"></param>
        /// <param name="names"></param>
        /// <param name="defaultValue"></param>
        /// <param name="rightSide"></param>
        /// <param name="callback"></param>
        /// <returns></returns>
        private JSONStorableStringChooser SetupStaticEnumsChooser<T>(string value, string label, List<string> names, string defaultValue, bool rightSide, StaticEnumsSetCallback<T> callback)
        {
            var chooser = SetupStaticEnumsChooser<T>(label, names, defaultValue, rightSide, callback);
            chooser.val = Lang.Get(value.ToString());

            _StorableStrings.Add(chooser);
            return chooser;
        }
        #endregion
    }
}
