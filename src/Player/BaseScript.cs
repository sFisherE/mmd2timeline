using MacGruber;
using System;
using System.Collections;
using UnityEngine;

namespace mmd2timeline.Player
{
    /// <summary>
    /// 基础脚本类
    /// </summary>
    /// <remarks>主要提供可继承的通用的UI处理方法</remarks>
    internal abstract partial class BaseScript : MVRScript
    {
        public const string PLUGIN_NAME = "Player";

        public const string VERSION = "2.1.6";

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

        //Player _Player;

        ///// <summary>
        ///// 获取Player
        ///// </summary>
        //protected Player Player
        //{
        //    get
        //    {
        //        if (_Player == null)
        //        {
        //            _Player = MacGruber.Utils.FindWithinSamePlugin<Player>(this);

        //            if (_Player == null)
        //            {
        //                LogUtil.Debug("Plugin 'Player' not found.");
        //            }
        //        }
        //        return _Player;
        //    }
        //}

        /// <summary>
        /// 忽略这个基类
        /// </summary>
        /// <returns></returns>
        public override bool ShouldIgnore()
        {
            return true;
        }

        public override void InitUI()
        {
            //if (LibMMD.Util.Settings.varPmxPath == null && this.name.IndexOf('_') > 0)
            //{
            //    LibMMD.Util.Settings.varPmxPath = MacGruber.Utils.GetPluginPath(this) + "/g2f.pmx";
            //    LogUtil.Debug("pmx path:" + LibMMD.Util.Settings.varPmxPath);
            //}

            base.InitUI();
            InitFullWidthUI();
        }

        ///// <summary>
        ///// 在后台运行
        ///// </summary>
        ///// <param name="action"></param>
        //protected void RunInBackground(ThreadStart action)
        //{
        //    Thread loadThread = new Thread(action);
        //    loadThread.IsBackground = true;
        //    loadThread.Start();
        //}

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
        }
    }
}
