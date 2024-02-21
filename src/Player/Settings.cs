using System;
using System.Linq;
using static SuperController;

namespace mmd2timeline
{
    internal partial class Settings : BaseScript
    {
        /// <summary>
        /// 忽略这个基类
        /// </summary>
        /// <returns></returns>
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

            RefreshCameraUI();
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

        public override void OnDestroy()
        {
            SuperController.singleton.onAtomAddedHandlers -= OnAtomChanged;
            SuperController.singleton.onAtomRemovedHandlers -= OnAtomChanged;

            base.OnDestroy();
        }

        public override void OnEnable()
        {
            SuperController.singleton.onAtomAddedHandlers += OnAtomChanged;
            SuperController.singleton.onAtomRemovedHandlers += OnAtomChanged;

            base.OnEnable();
        }

        public override void OnDisable()
        {
            SuperController.singleton.onAtomAddedHandlers -= OnAtomChanged;
            SuperController.singleton.onAtomRemovedHandlers -= OnAtomChanged;

            base.OnDisable();
        }
    }
}
