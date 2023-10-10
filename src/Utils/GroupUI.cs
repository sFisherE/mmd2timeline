using System.Collections.Generic;

namespace mmd2timeline.Player
{
    internal class GroupUI
    {
        internal BaseScript _Script;

        /// <summary>
        /// 元素
        /// </summary>
        public List<object> Elements = new List<object>();
        /// <summary>
        /// 其他元素
        /// </summary>
        public List<object> OtherElements = new List<object>();
        /// <summary>
        /// 子组
        /// </summary>
        public List<GroupUI> ChildGroups = new List<GroupUI>();

        ///// <summary>
        ///// 显示切换组件
        ///// </summary>
        //public UIDynamicToggle VisibleToggle;
        /// <summary>
        /// 显示切换组件对应的JSONStorable对象
        /// </summary>
        public JSONStorableBool ToggleBool;

        private GroupUI() { }

        /// <summary>
        /// UI组
        /// </summary>
        /// <param name="script"></param>
        public GroupUI(BaseScript script)
        {
            this._Script = script;
        }

        /// <summary>
        /// 更新UI
        /// </summary>
        /// <param name="on"></param>
        public void RefreshView(bool? on = null)
        {
            //refresh view
            var isOn = ToggleBool?.val;

            if (on.HasValue)
            {
                isOn = on.Value;
            }

            var open = isOn ?? false;

            _Script?.ShowUIElements(Elements, open);

            // 更新子UI组
            foreach (var child in ChildGroups)
            {
                if (open)
                {
                    child.RefreshView();
                }
                else
                {
                    child.RefreshView(false);
                }
            }
        }
    }
}
