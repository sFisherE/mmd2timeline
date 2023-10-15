namespace mmd2timeline
{
    internal partial class Config
    {
        /// <summary>
        /// 获取主HUD是否为显示状态
        /// </summary>
        internal bool MainHUDVisible
        {
            get
            {
                //#if (VAM_GT_1_20)
                return (SuperController.singleton.mainHUD.gameObject.activeSelf);
                //#else
                //                return (config.DeactiveCameraWhenMainHUDOpened && SuperController.singleton.MainHUDVisible);
                //#endif
            }
        }

        /// <summary>
        /// 获取镜头是否可激活
        /// </summary>
        internal bool CameraActive
        {
            get
            {
                return !(MainHUDVisible);
            }
        }

        /// <summary>
        /// 是否显示Debug信息
        /// </summary>
        internal bool showDebugInfo;
    }
}
