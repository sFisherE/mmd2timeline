using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using SimpleJSON;
using UnityEngine.UI;
using LibMMD.Unity3D;
using MVR.FileManagementSecure;
using LibMMD.Motion;


namespace mmd2timeline
{
    class Mmd2TimelineCameraAtomPlugin : MVRScript
    {
        void Start()
        {
        }

        void CreateHeader(string v, bool rightSide, Color color)
        {
            var header = CreateSpacer(rightSide);
            if (header != null)
            {
                header.height = 40;
                var text = header.gameObject.AddComponent<Text>();
                text.text = v;
                text.font = (Font)Resources.GetBuiltinResource(typeof(Font), "Arial.ttf");
                text.fontSize = 30;
                text.fontStyle = FontStyle.Bold;
                text.color = color;
            }
        }

        CameraControl m_CameraControl;
        public override void Init()
        {
            if(containingAtom.type!= "WindowCamera")
            {
                CreateHeader("Only used on WindowCamera", false, Color.red);
                return;
            }

            try
            {
                m_CameraControl = (CameraControl)containingAtom.GetStorableByID("CameraControl");
                m_CameraControl.SetBoolParamValue("cameraOn", true);
                m_CameraControl.SetBoolParamValue("showHUDView", true);

                CreateHeader("Step 1:", false, Color.black);
                var btn = CreateButton("Import Camera Vmd");
                if (btn != null)
                {
                    btn.button.onClick.AddListener(ImportCameraVmd);
                }

                playProgress = new JSONStorableFloat("Preview", 0f, SetProgress, 0, 1f, true);
                CreateSlider(playProgress);//.quickButtonsEnabled = false;

                CreateHeader("Edit Before Sample:", true, Color.black);
                sampleRateChooser = new JSONStorableStringChooser("Sample Mode",
                    new List<string>() { "EveryFrame", "KeyFrame", }, "EveryFrame", "Sample Mode");
                CreateScrollablePopup(sampleRateChooser, true);

                startTime = new JSONStorableFloat("Start Time", 0, SetStartTime, 0, 10, true, true);
                var slider = CreateSlider(startTime, true);
                if(slider!=null)
                    slider.quickButtonsEnabled = false;

                endTime = new JSONStorableFloat("End Time", 0, SetEndTime, 0, 10, true, true);
                slider = CreateSlider(endTime, true);
                if (slider != null)
                    slider.quickButtonsEnabled = false;

                CreateHeader("Step 2:", false, Color.black);
                btn = CreateButton("Export For WindowCamera");
                if (btn != null)
                    btn.button.onClick.AddListener(SampleForWindowCamera);
                //btn = CreateButton("Export For Empty");
                //if (btn != null)
                //    btn.button.onClick.AddListener(SampleForEmpty);
            }
            catch (Exception e)
            {
                SuperController.LogError("[mmd2timeline]:Exception caught: " + e);
            }





        }
        public JSONStorableStringChooser sampleRateChooser;

        public JSONStorableFloat playProgress;
        public JSONStorableFloat startTime;
        public JSONStorableFloat endTime;
        void SetProgress(float val)
        {
            m_MmdCamera.SetPlayPos(val);

        }

        void ImportCameraVmd()
        {
            try
            {
                SuperController.singleton.GetMediaPathDialog(path =>
                {
                    if (string.IsNullOrEmpty(path)) return;

                    ImportVmd(path);

                }, "vmd", "MMD", false);
            }
            catch (Exception exc)
            {
                SuperController.LogError($"[mmd2timeline]: Failed to open file dialog: {exc}");
            }
        }
        void SetStartTime(float val)
        {
            m_MmdCamera.SetPlayPos(val);
            float max = endTime.max;
            endTime.min = val;
            endTime.max = max;
        }
        void SetEndTime(float val)
        {
            endTime.val = val;
        }
        MmdCameraObject m_MmdCamera;
        public void ImportVmd(string path)
        {
            GameObject root = new GameObject("mmd2timeline camera root");
            root.transform.localPosition = Vector3.zero;
            root.transform.localScale = new Vector3(0.1f, 0.1f, 0.1f);
            root.transform.localEulerAngles = new Vector3(0, 180, 0);

            m_MmdCamera = MmdCameraObject.CreateGameObject();
            m_MmdCamera.transform.SetParent(root.transform);
            m_MmdCamera.transform.localPosition = Vector3.zero;
            m_MmdCamera.transform.localScale = Vector3.one;
            m_MmdCamera.transform.localRotation = Quaternion.identity;

            m_MmdCamera.m_CameraControl = m_CameraControl;
            m_MmdCamera.m_Control = containingAtom.mainController;
            m_MmdCamera.LoadCameraMotion(path);
            //m_MmdCamera.Playing = true;

            int lastFrame = m_MmdCamera._cameraMotion.KeyFrames[m_MmdCamera._cameraMotion.KeyFrames.Count - 1].Key;

            Debug.Log("lastFrame " + lastFrame);
            endTime.max= (float)((double)lastFrame / 30);
            endTime.val = endTime.max;
            startTime.max = endTime.max;

            playProgress.max = endTime.max;
        }
        Quaternion quat = new Quaternion(0, 1, 0, 0);
        TimelineJson json;

        void SampleForWindowCamera()
        {
            Sample("WindowCamera");
        }
        void SampleForEmpty()
        {
            Sample("Empty");
        }
        public void Sample(string atomType)
        {
            json = new TimelineJson();
            json.AtomType = atomType;
            json.Clips = new List<TimelineClipJson>();
            TimelineClipJson clipJson = new TimelineClipJson();
            json.Clips.Add(clipJson);

            clipJson.AnimationName = "camera motion";
            clipJson.AnimationLength = (endTime.val - startTime.val).ToString();

            clipJson.Controllers = new List<TimelineControlJson>();
            var controlJson = new TimelineControlJson();
            controlJson.Controller = "control";
            clipJson.Controllers.Add(controlJson);

            clipJson.FloatParams = new List<FloatParamsJson>();
            var fovJson = new FloatParamsJson();
            fovJson.Storable = "CameraControl";
            fovJson.Name = "FOV";
            fovJson.Min = "10";
            fovJson.Max = "100";
            if (atomType == "Empty")
                fovJson.Atom = "WindowCamera";
            clipJson.FloatParams.Add(fovJson);

            var list = m_MmdCamera._cameraMotion.KeyFrames;
            float beginT = startTime.val;
            float endT = endTime.val;
            int startFrame = 0;
            int maxFrame = list[list.Count - 1].Key;
            int endFrame = maxFrame;
            for (int i = 0; i < list.Count; i++)
            {
                int frame = list[i].Key;
                float time = (float)frame / 30;
                if (time >= beginT)
                {
                    startFrame = frame;
                    break;
                }
            }
            for(int i = list.Count - 1; i >= 0; i--)
            {
                int frame = list[i].Key;
                float time = (float)frame / 30;
                if (time <= endT)
                {
                    endFrame = frame;
                    break;
                }
            }
            Debug.Log("maxFrame " + maxFrame);
            Debug.Log("startFrame " + startFrame);
            Debug.Log("endFrame " + endFrame);

            HashSet<int> keyFrames = new HashSet<int>();
            foreach(var item in m_MmdCamera._cameraMotion.KeyFrames)
            {
                if (item.Key >= startFrame && item.Key <= endFrame)
                {
                    keyFrames.Add(item.Key);
                }
            }


            for (int i = startFrame; i <= endFrame; i++)
            {
                if(sampleRateChooser.val!= "EveryFrame")
                {
                    if (!keyFrames.Contains(i)) continue;
                }

                float time = (float)i / 30;
                var cameraPose = m_MmdCamera._cameraMotion.GetCameraPose(time);
                float relativeTime = time - beginT;
                if (cameraPose != null)
                {
                    //fov
                    TimelineFrameJson fovRecord = new TimelineFrameJson(relativeTime,cameraPose.Fov, "3");
                    fovJson.Value.Add(fovRecord);

                    m_MmdCamera.transform.localPosition = cameraPose.Position;// / 10;
                    m_MmdCamera.transform.localRotation = Quaternion.Euler(-180 / Mathf.PI * cameraPose.Rotation);// * quat;
                    m_MmdCamera.m_CameraTf.transform.localPosition = new Vector3(0.0f, 0.0f, cameraPose.FocalLength);// / 10);

                    m_MmdCamera.m_Control.transform.SetPositionAndRotation(m_MmdCamera.m_CameraTf.position, m_MmdCamera.m_CameraTf.rotation);

                    string type = "3";
                    if (i < maxFrame)
                    {
                        if (sampleRateChooser.val != "EveryFrame")
                        {
                            int nextKeyFrame = maxFrame;
                            type = GuessType(list, i, ref nextKeyFrame);
                            fovRecord.c = type;

                            RecordController(relativeTime, m_MmdCamera.m_Control, controlJson, type);

                            if (type != "3")
                            {
                                for (int j = i; j < nextKeyFrame; j += 5)
                                {
                                    time = (float)j / 30;
                                    cameraPose = m_MmdCamera._cameraMotion.GetCameraPose(time);
                                    relativeTime = time - beginT;

                                    m_MmdCamera.transform.localPosition = cameraPose.Position;/// 10;
                                    m_MmdCamera.transform.localRotation = Quaternion.Euler(-180 / Mathf.PI * cameraPose.Rotation);// * quat;
                                    m_MmdCamera.m_CameraTf.transform.localPosition = new Vector3(0.0f, 0.0f, cameraPose.FocalLength);// / 10);
                                    m_MmdCamera.m_Control.transform.SetPositionAndRotation(m_MmdCamera.m_CameraTf.position, m_MmdCamera.m_CameraTf.rotation);

                                    RecordController(relativeTime, m_MmdCamera.m_Control, controlJson, "3");
                                }
                            }
                        }
                        else
                        {
                            type = "2";//Linear
                            RecordController(relativeTime, m_MmdCamera.m_Control, controlJson, type);
                        }
                    }
                    else
                    {
                        type = "2";//Linear
                        RecordController(relativeTime, m_MmdCamera.m_Control, controlJson, type);
                    }
                }
            }
            var fileBrowserUI = SuperController.singleton.fileBrowserUI;
            fileBrowserUI.SetTitle("Export animation");
            fileBrowserUI.fileRemovePrefix = null;
            fileBrowserUI.hideExtension = false;
            fileBrowserUI.keepOpen = false;
            fileBrowserUI.fileFormat = _saveExt;
            fileBrowserUI.defaultPath = _saveFolder;
            fileBrowserUI.showDirs = true;
            fileBrowserUI.shortCuts = null;
            fileBrowserUI.browseVarFilesAsDirectories = false;
            fileBrowserUI.SetTextEntry(true);
            fileBrowserUI.Show(ExportFileSelected);
            fileBrowserUI.ActivateFileNameField();
        }

        string GuessType(List<KeyValuePair<int, CameraKeyframe>> list,int currentFrame,ref int nextFrame)
        {
            int maxFrame = list[list.Count - 1].Key;
            string type = "3";

            nextFrame = maxFrame;
            int lastFrame = 0;
            for (int k = list.Count - 1; k >= 0; k--)
            {
                if (list[k].Key < lastFrame)
                {
                    lastFrame = list[k].Key;
                    break;
                }
            }
            //找到下一个关键帧
            for (int k = 0; k < list.Count; k++)
            {
                if (list[k].Key > currentFrame)
                {
                    nextFrame = list[k].Key;
                    break;
                }
            }
            if (nextFrame == currentFrame + 1)
            {
                //突然变化 Constant
                type = "8";
            }
            else if (lastFrame == currentFrame - 1 && nextFrame > currentFrame + 1)
            {
                //SmoothGlobal
                type = "10";
            }
            return type;
        }
        private void ExportFileSelected(string path)
        {
            if (string.IsNullOrEmpty(path)) return;
            if (!path.ToLower().EndsWith($".{_saveExt}")) path += $".{_saveExt}";

            SuperController.singleton.SaveJSON(json.GetJSONClass(), path);

        }
        private const string _saveExt = "json";
        private const string _saveFolder = "Saves";
        void RecordController(float time, FreeControllerV3 freeController, TimelineControlJson json,string type)
        {
            Transform target = freeController.transform;

            TimelineFrameJson x = new TimelineFrameJson(time, target.localPosition.x, type);
            json.X.Add(x);

            TimelineFrameJson y = new TimelineFrameJson(time, target.localPosition.y,type);
            json.Y.Add(y);

            TimelineFrameJson z = new TimelineFrameJson(time, target.localPosition.z,type);
            json.Z.Add(z);

            TimelineFrameJson rx = new TimelineFrameJson(time, target.localRotation.x,type);
            json.RotX.Add(rx);

            TimelineFrameJson ry = new TimelineFrameJson(time, target.localRotation.y, type);
            json.RotY.Add(ry);

            TimelineFrameJson rz = new TimelineFrameJson(time, target.localRotation.z,type);
            json.RotZ.Add(rz);

            TimelineFrameJson rw = new TimelineFrameJson(time, target.localRotation.w,type);
            json.RotW.Add(rw);
        }

    }
}