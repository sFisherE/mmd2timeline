using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
namespace mmd2timeline
{
    partial class Mmd2TimelinePersonAtomPlugin
    {
        //public float StepSpeed = 2;
        public int CurFrame = 0;
        float m_BeginTime;
        float m_EndTime;
        public float GetRelativeTime()
        {
            int frame = CurFrame;
            float time = (float)frame / 30;
            float val = time - m_BeginTime;
            float min = 0;
            float max = m_EndTime - m_BeginTime;
            return Mathf.Clamp(val, min, max);
        }
        
        public bool IsSampling = false;

        public TimelineJson m_PersonAniJson;


        public Dictionary<string, TimelineControlJson> timelineControlLookup;

        public Dictionary<string, FloatParamsJson> m_RightFingerMotions = null;
        public Dictionary<string, FloatParamsJson> m_LeftFingerMotions = null;
        public void SetFingerKeyFrame(float time, string boneName, string key, float value)
        {
            var dic = boneName.StartsWith("l") ? m_LeftFingerMotions : m_RightFingerMotions;
            TimelineFrameJson f = new TimelineFrameJson(time, (int)value,"3");
            dic[key].Value.Add(f);
        }


        private string liuFolder = MVR.FileManagementSecure.FileManagerSecure.GetFullPath("Saves");//liu修改 返回指定路径字符串的绝对路径
        void ImportVmd()
        {
            if (m_MmdPersonGameObject == null)
            {
                SuperController.LogError("[mmd2timeline]: need InitAtom");
                return;
            }
            try
            {
                SuperController.singleton.GetMediaPathDialog(path =>
                {
                    if (string.IsNullOrEmpty(path)) return;
                    this.liuFolder = MVR.FileManagementSecure.FileManagerSecure.GetDirectoryName(path, false);//返回指定路径字符串的目录信息
                    ImportVmd(path);

                    //更新动画长度
                    playProgress.max = m_MmdPersonGameObject.MotionLength;

                    startTime.max = m_MmdPersonGameObject.MotionLength;
                    endTime.max = m_MmdPersonGameObject.MotionLength;
                    endTime.val = endTime.max;

                    importedVmdLabel.text = "Imported Vmd:" + m_MmdPersonGameObject.GetImportedVmds();

                }, "vmd", this.liuFolder, false);
            }
            catch (Exception exc)
            {
                SuperController.LogError($"[mmd2timeline]: Failed to open file dialog: {exc}");
            }
        }
        void Pause()
        {
            IsPausing = true;
            IsSteping = false;
        }
        void Continue()
        {
            IsPausing = false;
            IsSteping = false;
        }
        void NextKeyFrame()
        {
            IsSteping = true;
            IsPausing = false;
        }


        public void StepPlay(float beginTime, float endTime)
        {
            m_BeginTime = beginTime;
            m_EndTime = endTime;

            m_PersonAniJson = new TimelineJson();
            m_PersonAniJson.Clips = new List<TimelineClipJson>();
            TimelineClipJson clipJson = new TimelineClipJson();
            m_PersonAniJson.Clips.Add(clipJson);

            //fingerKeyFrames = GetFingerKeyFrames();
            var list = m_MmdPersonGameObject.GetMorphKeyFrames(beginTime, endTime);

            clipJson.FloatParams = list;
            m_RightFingerMotions = new Dictionary<string, FloatParamsJson>();
            m_LeftFingerMotions = new Dictionary<string, FloatParamsJson>();

            //手指动画
            for (int i = 0; i < 2; i++)
            {
                var motions = i == 0 ? m_LeftFingerMotions : m_RightFingerMotions;
                foreach (var item in FingerMorph.setting)
                {
                    var j = new FloatParamsJson();
                    j.Storable = FingerMorph.StorableNames[i];
                    j.Name = item;
                    j.Min = "-100";
                    j.Max = "100";
                    motions.Add(item, j);
                    clipJson.FloatParams.Add(j);
                }
            }


            clipJson.AnimationName = m_MmdPersonGameObject._motion.Name;
            clipJson.AnimationLength = (endTime - beginTime).ToString();

            clipJson.Controllers = new List<TimelineControlJson>();
            timelineControlLookup = new Dictionary<string, TimelineControlJson>();
            //控制器动画
            foreach (var item in DazBoneMapping.vamControls)
            {
                if (enableHeel.val)
                {
                    if (item == "rToeControl" || item == "lToeControl")
                        continue;
                }
                //没有控制关节，不导出帧
                if (controllerNameLookup[item].currentPositionState== FreeControllerV3.PositionState.Off)
                {
                    continue;
                }
                var controlJson = new TimelineControlJson();
                controlJson.Controller = item;
                timelineControlLookup.Add(item, controlJson);
                clipJson.Controllers.Add(controlJson);
            }
            if(m_SampleCo!=null)
            {
                StopCoroutine(m_SampleCo);
                m_SampleCo = null;
            }
            m_SampleCo= StartCoroutine(CoStepPlay(beginTime, endTime));
        }
        Coroutine m_SampleCo = null;
        bool IsPausing
        {
            get;set;
        }
        bool IsSteping
        {
            get; set;
        }
        IEnumerator CoStepPlay(float beginTime, float endTime)
        {
            m_BeginTime = beginTime;
            m_EndTime = endTime;
            IsPausing = false;
            IsSampling = true;
            //所有的运动帧
            var personFrameSteps = m_MmdPersonGameObject.GetMotionKeyFrames(beginTime, endTime);
            HashSet<int> assetFrameSet = null;
            //if (sampleRateChooser.val == "EveryFrame")//逐帧模式
            {
                List<int> frames = new List<int>();

                if (personFrameSteps.Count > 1)
                {
                    int start = personFrameSteps[0];
                    int end = personFrameSteps[personFrameSteps.Count - 1];
                    for(int i=start; i <= end; i++)
                    {
                        frames.Add(i);
                    }
                }
                personFrameSteps = frames;
            }
            

            sampleTimeLabel.text = GetTimeText();
            for (int i = 0; i < personFrameSteps.Count; i++)
            {
                CurFrame = personFrameSteps[i];
                sampleTimeLabel.text = GetTimeText();
                m_MmdPersonGameObject.SeekFrame(CurFrame);
                m_MmdPersonGameObject._poser.PrePhysicsPosing(false);
                m_MmdPersonGameObject._poser.PostPhysicsPosing();
                m_MmdPersonGameObject.UpdateBones();
                if (m_MmdPersonGameObject.OnUpdate != null)
                    m_MmdPersonGameObject.OnUpdate(m_MmdPersonGameObject);

                //需要多等一会
                if (WithAsset())
                {
                    Physics.autoSimulation = false;
                    for (int k = 0; k < 20; k++)
                    {
                        Physics.Simulate(Time.fixedDeltaTime);
                        yield return new WaitForEndOfFrame();
                    }
                    Physics.autoSimulation = true;
                }

                int diff = 1;
                //最后一帧可能有问题
                if (i < personFrameSteps.Count - 1)
                    diff = personFrameSteps[i + 1] - CurFrame;
                sampleSpeed.val = Math.Max(0.05f, sampleSpeed.val);
                yield return new WaitForSeconds((1.0f / 30 / sampleSpeed.val) * diff);

                if (i >= personFrameSteps.Count - 1)
                    break;

                if (IsSteping)
                    IsPausing = true;

                while (IsPausing)
                    yield return new WaitForSeconds(0.5f);

            }
            IsSampling = false;
            IsPausing = false;
            CurFrame = 0;

            m_SampleCo = null;
        }

    }
}
