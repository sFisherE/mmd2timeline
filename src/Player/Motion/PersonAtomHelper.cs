namespace mmd2timeline
{
    /// <summary>
    /// 人物原子数据快照
    /// </summary>
    internal class PersonAtomHelper
    {
        Atom person;

        internal bool autoExpressions;
        internal bool blinkEnabled;

        /// <summary>
        /// 快照
        /// </summary>
        /// <param name="person"></param>
        /// <returns></returns>
        public static PersonAtomHelper Snap(Atom person)
        {
            return new PersonAtomHelper
            {
                person = person,
                autoExpressions = person.GetStorableByID("AutoExpressions").GetBoolParamValue("enabled"),
                blinkEnabled = person.GetStorableByID("EyelidControl").GetBoolParamValue("blinkEnabled"),
            };
        }

        /// <summary>
        /// 停止表情
        /// </summary>
        public void StopExpressions()
        {
            person.GetStorableByID("AutoExpressions").SetBoolParamValue("enabled", false);
            person.GetStorableByID("EyelidControl").SetBoolParamValue("blinkEnabled", false);
        }

        /// <summary>
        /// 恢复
        /// </summary>
        public void Restore()
        {
            person.GetStorableByID("AutoExpressions").SetBoolParamValue("enabled", autoExpressions);
            person.GetStorableByID("EyelidControl").SetBoolParamValue("blinkEnabled", blinkEnabled);
        }
    }
}
