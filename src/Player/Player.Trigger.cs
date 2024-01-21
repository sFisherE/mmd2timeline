using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace mmd2timeline
{
    internal partial class Player
    {
        TriggerEventHelper _triggerHelper = TriggerEventHelper.GetInstance();

        //const string TRIGGER_SCRIPT_LOADED = "Script Loaded Trigger";

        //const string TRIGGER_START_PLAYING = "Start Playing Trigger";
        //const string TRIGGER_PLAY_NEXT = "Play Next Trigger";
        //const string TRIGGER_IS_END = "Is End Trigger";

        //const string TRIGGER_FAVORITE = "Favorite Trigger";
        ////const string TRIGGER_UNFAVORITED = "Not Favorite Trigger";

        //const string TRIGGER_PLAYMODE_INIT = "In Init Mode Trigger";
        //const string TRIGGER_PLAYMODE_PLAY = "In Play Mode Trigger";
        //const string TRIGGER_PLAYMODE_EDIT = "In Edit Mode Trigger";
        //const string TRIGGER_PLAYMODE_LOAD = "In Load Mode Trigger";

        //const string TRIGGER_CAMERA_ACTIVATE = "Camera Activate Trigger";
        ////const string TRIGGER_CAMERA_DEACTIVATED = "Camera Deactivated Trigger";

        //void InitTriggers()
        //{
        //    _triggerHelper.InitTriggers(this);
        //    _triggerHelper.AddTrigger(TRIGGER_SCRIPT_LOADED, TriggerEventType.Action);
        //    _triggerHelper.AddTrigger(TRIGGER_FAVORITE, TriggerEventType.Bool);
        //    //_triggerHelper.AddTrigger(TRIGGER_UNFAVORITED);
        //    _triggerHelper.AddTrigger(TRIGGER_IS_END, TriggerEventType.Action);
        //    _triggerHelper.AddTrigger(TRIGGER_PLAY_NEXT, TriggerEventType.Action);
        //    _triggerHelper.AddTrigger(TRIGGER_START_PLAYING, TriggerEventType.Action);
        //    _triggerHelper.AddTrigger(TRIGGER_PLAYMODE_INIT, TriggerEventType.Action);
        //    _triggerHelper.AddTrigger(TRIGGER_PLAYMODE_PLAY, TriggerEventType.Action);
        //    _triggerHelper.AddTrigger(TRIGGER_PLAYMODE_EDIT, TriggerEventType.Action);
        //    _triggerHelper.AddTrigger(TRIGGER_PLAYMODE_LOAD, TriggerEventType.Action);
        //    _triggerHelper.AddTrigger(TRIGGER_CAMERA_ACTIVATE, TriggerEventType.Bool);
        //    //_triggerHelper.AddTrigger(TRIGGER_CAMERA_DEACTIVATED);
        //}
    }
}
