using MacGruber;
using System;
using System.Collections;
using UnityEngine;

namespace mmd2timeline
{
    /// <summary>
    /// 基础脚本类
    /// </summary>
    /// <remarks>主要提供可继承的通用的UI处理方法</remarks>
    internal partial class BaseScript : MVRScript
    {
        public const string PLUGIN_NAME = "MMD2TimelinePlayer";

        public const string VERSION = "1.2";

        /// <summary>
        /// 获取插件的版本号
        /// </summary>
        protected virtual string Version
        {
            get
            {
                return VERSION;
            }
        }

        /// <summary>
        /// 插件目录
        /// </summary>
        string _PluginPath;

        /// <summary>
        /// 获取插件目录
        /// </summary>
        protected string PluginPath
        {
            get
            {
                if (_PluginPath == null)
                {
                    _PluginPath = Utils.GetPluginPath(this);
                }

                return _PluginPath;
            }
        }

        /// <summary>
        /// 默认配置
        /// </summary>
        protected static readonly Config dft = Config.GetDefault();
        /// <summary>
        /// 配置
        /// </summary>
        protected static readonly Config config = Config.GetInstance();

        /// <summary>
        /// 忽略这个基类
        /// </summary>
        /// <returns></returns>
        public override bool ShouldIgnore()
        {
            return true;
        }

        /// <summary>
        /// 初始化脚本
        /// </summary>
        protected void InitScript()
        {
            Lang.Init(PluginPath);

            Utils.OnInitUI(this, CreateAllUIElement);

            InitFullWidthUI();
        }

        /// <summary>
        /// 等待n秒后执行方法
        /// </summary>
        /// <param name="s"></param>
        /// <param name="action"></param>
        /// <returns></returns>
        protected static IEnumerator WaitForSecondsRealtime(float s, Action action = null)
        {
            yield return new WaitForSecondsRealtime(s);
            action?.Invoke();
        }

        /// <summary>
        /// 当启用时执行的方法
        /// </summary>
        public virtual void OnEnable()
        {

        }

        /// <summary>
        /// 当禁用时执行的方法
        /// </summary>
        public virtual void OnDisable()
        {

        }

        /// <summary>
        /// 当销毁时调用的方法
        /// </summary>
        public virtual void OnDestroy()
        {
            Utils.OnDestroyUI(this);
            Utility.CleanGameObjects();
        }
    }
}
