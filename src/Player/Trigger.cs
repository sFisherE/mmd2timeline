using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace mmd2timeline
{
    internal partial class Trigger : BaseScript
    {
        public override bool ShouldIgnore()
        {
            return false;
        }

        public override void OnEnable()
        {
            _triggerEventHelper.OnTriggerListChanged += OnTriggerListChanged;

            base.OnEnable();
        }

        public override void OnDisable()
        {
            _triggerEventHelper.OnTriggerListChanged -= OnTriggerListChanged;

            base.OnDisable();
        }

        public override void OnDestroy()
        {
            _triggerEventHelper.OnTriggerListChanged -= OnTriggerListChanged;

            CurrrentTriggerName = null;

            _triggerEventHelper.Dispose();

            base.OnDestroy();
        }
    }
}
