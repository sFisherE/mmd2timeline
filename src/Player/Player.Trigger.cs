using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace mmd2timeline
{
    internal partial class Player
    {
        TriggerHelper _triggerHelper = TriggerHelper.GetInstance();

        const string TRIGGER_SCRIPT_LOADED = "Script Loaded Trigger";

        const string TRIGGER_START_PLAYING = "Start Playing Trigger";
        const string TRIGGER_PLAY_NEXT = "Play Next Trigger";
        const string TRIGGER_IS_END = "Is End Trigger";

        const string TRIGGER_FAVORITED = "Favorited Trigger";
        const string TRIGGER_UNFAVORITED = "Not Favorite Trigger";

        const string TRIGGER_PLAYMODE_INIT = "In Init Mode Trigger";
        const string TRIGGER_PLAYMODE_PLAY = "In Play Mode Trigger";
        const string TRIGGER_PLAYMODE_EDIT = "In Edit Mode Trigger";
        const string TRIGGER_PLAYMODE_LOAD = "In Load Mode Trigger";

        const string TRIGGER_CAMERA_ACTIVATED = "Camera Activated Trigger";
        const string TRIGGER_CAMERA_DEACTIVATED = "Camera Deactivated Trigger";

        void InitTriggers()
        {
            _triggerHelper.InitTriggers(this);
            _triggerHelper.AddTrigger(TRIGGER_SCRIPT_LOADED);
            _triggerHelper.AddTrigger(TRIGGER_FAVORITED);
            _triggerHelper.AddTrigger(TRIGGER_UNFAVORITED);
            _triggerHelper.AddTrigger(TRIGGER_IS_END);
            _triggerHelper.AddTrigger(TRIGGER_PLAY_NEXT);
            _triggerHelper.AddTrigger(TRIGGER_START_PLAYING);
            _triggerHelper.AddTrigger(TRIGGER_PLAYMODE_INIT);
            _triggerHelper.AddTrigger(TRIGGER_PLAYMODE_PLAY);
            _triggerHelper.AddTrigger(TRIGGER_PLAYMODE_EDIT);
            _triggerHelper.AddTrigger(TRIGGER_PLAYMODE_LOAD);
            _triggerHelper.AddTrigger(TRIGGER_CAMERA_ACTIVATED);
            _triggerHelper.AddTrigger(TRIGGER_CAMERA_DEACTIVATED);
        }
    }
}
