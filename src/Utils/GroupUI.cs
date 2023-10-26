using System.Collections.Generic;

namespace mmd2timeline
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
        public List<object> OuterElements = new List<object>();
        /// <summary>
        /// 子组
        /// </summary>
        public List<GroupUI> ChildGroups = new List<GroupUI>();
        /// <summary>
        /// 外部子组
        /// </summary>
        public List<GroupUI> OuterChildGroups = new List<GroupUI>();

        /// <summary>
        /// 显示切换组件对应的JSONStorable对象
        /// </summary>
        public JSONStorableBool ToggleBool;

        string Name;

        private GroupUI() { }

        /// <summary>
        /// UI组
        /// </summary>
        /// <param name="script"></param>
        public GroupUI(BaseScript script, string name = "")
        {
            this._Script = script;
            Name = name;
        }

        /// <summary>
        /// 显示元素
        /// </summary>
        /// <param name="show"></param>
        public void ShowElements(bool show)
        {
            _Script?.ShowUIElements(Elements, show);
        }

        /// <summary>
        /// 显示其他元素
        /// </summary>
        /// <param name="show"></param>
        public void ShowOuterElements(bool show)
        {
            _Script?.ShowUIElements(OuterElements, show);
        }

        /// <summary>
        /// 显示或隐藏
        /// </summary>
        /// <param name="show"></param>
        public void Show(bool show)
        {
            // 显示或隐藏子组
            foreach (var ui in ChildGroups)
            {
                ui.Show(show);
            }

            // 显示或隐藏外部子组
            foreach (var ui in OuterChildGroups)
            {
                ui.Show(show);
            }

            _Script?.ShowUIElement(this.ToggleBool, show);

            this.RefreshView();

            // 如果显示自己刷新视图
            if (show)
            {
                ShowOuterElements(true);
            }
            else // 否则隐藏所有元素
            {
                ShowElements(false);
                ShowOuterElements(false);
            }
        }

        /// <summary>
        /// 清理所有子元素
        /// </summary>
        public void Clear()
        {
            if (_Script == null)
                return;

            // TODO 清理子元素时，展开的子元素无法被正确清理，暂时没有找到原因
            //this.Show(false);

            var uiList = new List<object>() { this.ToggleBool };

            // 清理外部子组
            if (this.OuterChildGroups.Count > 0)
            {
                // 逐一移除子组中的元素
                foreach (var childGroup in this.OuterChildGroups)
                {
                    childGroup.Clear();
                }
            }

            // 清理内部子组
            if (this.ChildGroups.Count > 0)
            {
                // 逐一移除子组中的元素
                foreach (var childGroup in this.ChildGroups)
                {
                    childGroup.Clear();
                }
            }
            //uiList = uiList.Concat(Elements).Concat(OutElements).ToList();

            // 移除所有设定UI
            BaseScript.RemoveUIElements(_Script, this.Elements);

            // 移除其他UI
            BaseScript.RemoveUIElements(_Script, this.OuterElements);

            // 移除Toggle
            BaseScript.RemoveUIElements(_Script, uiList);

            this.ToggleBool = null;
            OuterChildGroups.Clear();
            ChildGroups.Clear();
            Elements.Clear();
            OuterElements.Clear();
        }

        /// <summary>
        /// 更新UI
        /// </summary>
        /// <param name="on"></param>
        public void RefreshView(bool? on = null)
        {
            //refresh view
            var isOn = true;

            if (on.HasValue)
            {
                isOn = on.Value;
            }
            else if (ToggleBool != null)
            {
                isOn = ToggleBool.val;
            }

            var open = isOn;

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

            _Script?.ShowUIElements(Elements, open);
        }
    }
}
