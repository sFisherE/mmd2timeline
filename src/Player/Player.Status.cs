namespace mmd2timeline
{
    internal partial class Player
    {
        /// <summary>
        /// 镜头状态
        /// </summary>
        JSONStorableBool _cameraActiveJSON;

        /// <summary>
        /// 播放状态
        /// </summary>
        JSONStorableBool _playStatusJSON;

        /// <summary>
        /// 脚本加载完毕
        /// </summary>
        JSONStorableBool _scriptLoadedJSON;

        void InitStatusParams()
        {
            _cameraActiveJSON = new JSONStorableBool($"Camera Active Status", false);
            _cameraActiveJSON.setCallbackFunction = v =>
            {
                if (v)
                {
                    _triggerHelper.Trigger(TriggerEventHelper.TRIGGER_CAMERA_ACTIVATED);
                }
                else
                {
                    _triggerHelper.Trigger(TriggerEventHelper.TRIGGER_CAMERA_DEACTIVATED);
                }
            };

            _playStatusJSON = new JSONStorableBool($"Playing Status", false);
            _playStatusJSON.setCallbackFunction = (v) =>
            {
                _triggerHelper.Trigger(TriggerEventHelper.TRIGGER_PLAYING, v);
            };

            _scriptLoadedJSON = new JSONStorableBool($"Script Loaded", false);
            _scriptLoadedJSON.setCallbackFunction = (v) =>
            {
                if (v)
                {
                    _triggerHelper.Trigger(TriggerEventHelper.TRIGGER_SCRIPT_LOADED, v);
                }
            };
        }
    }
}
