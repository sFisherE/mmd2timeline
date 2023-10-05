using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using SimpleJSON;
using LibMMD.Unity3D;
using UnityEngine.UI;
using MVR.FileManagementSecure;
using MacGruber;

namespace mmd2timeline
{
   partial class Mmd2TimelinePersonAtomPlugin : MVRScript
    {
        public virtual bool WithAsset()
        {
            return false;
        }
        void Start()
        {
            EnableHighHeel(enableHeel.val);
            //sfishere.mmd2timeline.10:/Custom/Scripts/mmd2timeline
            Settings.varPmxPath = MacGruber.Utils.GetPluginPath(this) + "/g2f.pmx";
            LogUtil.Log("path:"+ Settings.varPmxPath);
        }
        public Text sampleTimeLabel;
        public Text importedVmdLabel;
        public string GetTimeText()
        {
          return  string.Format("Time:{0:F2}/{1:F2} Frame:{2}", 
              (float)CurFrame / 30,endTime.val-startTime.val, CurFrame);
            
        }
        public JSONStorableFloat playProgress;
        public JSONStorableFloat motionScale;
        void SetMotionScale(float val)
        {
            Settings.s_MotionScale = val;
            if(m_MmdPersonGameObject!=null)
                m_MmdPersonGameObject.SetMotionPos(m_MmdPersonGameObject._playTime, true);
        }
        public JSONStorableFloat posY;
        public JSONStorableFloat startTime;
        public JSONStorableFloat endTime;

        public JSONStorableFloat sampleSpeed;


        JSONStorableStringChooser m_BoneChooser;
        public JSONStorableFloat m_BoneAdjustX;
        public JSONStorableFloat m_BoneAdjustY;
        public JSONStorableFloat m_BoneAdjustZ;

        void SetPosY(float y)
        {
            var pos = containingAtom.mainController.transform.localPosition;
            containingAtom.mainController.transform.localPosition = new Vector3(pos.x, y, pos.z);
            SetTransform();
        }

        public override void Init()
        {
            Debug.Log("Init");
            if (containingAtom.type != "Person")
            {
                CreateHeader("Only used on Person", false, Color.red);
                return;
            }
            //高跟鞋
            enableHeel = new JSONStorableBool("Enable High Heel", false, EnableHighHeel);
            holdRotationMaxForceAdjust = new JSONStorableFloat("Foot Hold Rotation Max Force", 0, 0, 1000);
            footJointDriveXTargetAdjust = new JSONStorableFloat("Foot Joint Drive X Angle", -45f, SetJointDriveXAngle, -65f, 40, true);
            toeJointDriveXTargetAdjust = new JSONStorableFloat("Toe Joint Drive X Angle", 35f, SetJointDriveXAngle, -40f, 75, true);

            posY = new JSONStorableFloat("PosY", 0, SetPosY, 0, 1, true, true);
            startTime = new JSONStorableFloat("Start Time", 0, SetStartTime, 0, 10, true, true);
            endTime = new JSONStorableFloat("End Time", 0, SetEndTime, 0, 10, true, true);

            motionScale = new JSONStorableFloat("Motion Scale", 1f, SetMotionScale, 0.1f, 2, true);
            sampleSpeed = new JSONStorableFloat("Sample Speed", 2, SetSampleSpeed, 0.1f, 3);
            //sampleRateChooser = new JSONStorableStringChooser("Sample Mode", new List<string>() { "EveryFrame","KeyFrame",  }, "EveryFrame", "Sample Mode");
            playProgress = new JSONStorableFloat("Preview", 0f, SetProgress, 0, 10f, true);


            m_BoneAdjustX= new JSONStorableFloat("Adjust Bone Rotation X", 0f, SetBoneAdjustX, -30, 30f, true);
            m_BoneAdjustY = new JSONStorableFloat("Adjust Bone Rotation Y", 0f, SetBoneAdjustY, -30, 30f, true);
            m_BoneAdjustZ = new JSONStorableFloat("Adjust Bone Rotation Z", 0f, SetBoneAdjustZ, -30, 30f, true);


            // 是否激活高跟鞋的调整，高跟鞋激活状态，toeControl就不输出了
            CreateHeader("Pre Setting:", false, Color.black);
            InitHeelUI();

            LogUtil.Log("InitHeelUI");
            CreateHeader("Step 1:", false, Color.black);
            var btn1 = CreateButton("InitAtom");
            if (btn1 != null)
            {
                btn1.button.onClick.AddListener(InitAtom);
            }

            CreateHeader("Step 2:", false, Color.black);
            {
                var btn = CreateButton("Import Vmd");
                if(btn!=null)
                    btn.button.onClick.AddListener(ImportVmd);
            }
            importedVmdLabel = CreateLabel("Imported Vmd:None", false, Color.green, false);

            if (!WithAsset())
            {
                CreateHeader("Edit Before Sample:", true, Color.black);
                //采样模式
                //CreateScrollablePopup(sampleRateChooser, true);

                //CreateLabel("adjust person position/rotation", true, Color.yellow);
                //var btn = CreateButton("Confirm Position/Rotation", true);//
                //if (btn != null)
                //{
                //    btn.button.onClick.AddListener(() =>
                //    {
                //        SetTransform();
                //    });
                //}
            }

            if (!WithAsset())
            {
                var slider = CreateSlider(startTime, true);
                if (slider != null)
                    slider.quickButtonsEnabled = false;
            }

            if (!WithAsset())
            {
                var slider = CreateSlider(endTime, true);
                if (slider != null)
                    slider.quickButtonsEnabled = false;
            }

            CreateHeader("Edit At Any Time:", true, Color.black);
            CreateSlider(posY, true);
            {
                var slider = CreateSlider(motionScale, true);
                if (slider != null)
                    slider.quickButtonsEnabled = false;
            }
            {
            //采样速度
                var slider = CreateSlider(sampleSpeed, true);
                if (slider != null)
                    slider.quickButtonsEnabled = false;
            }
            {
                var slider = CreateSlider(playProgress);
                if (slider != null)
                    slider.quickButtonsEnabled = false;
            }
            //骨骼微调
            {
                m_BoneChooser = new JSONStorableStringChooser("AdjustBoneRotation", new List<string>() { "Shoulder", "Arm", "Elbow","Hand","Foot" }, "Arm", "Bone Rotation Adjust",SetBone);
                CreateFilterablePopup(m_BoneChooser, true);

                CreateSlider(m_BoneAdjustX, true);
                CreateSlider(m_BoneAdjustY, true);
                CreateSlider(m_BoneAdjustZ, true);

                var btn = CreateButton("Clear Bone Rotation Adjust",true);
                if (btn != null)
                {
                    btn.button.onClick.AddListener(() =>
                    {
                        if (m_MmdPersonGameObject == null) return;
                        if (m_MmdPersonGameObject._model == null) return;
                        m_BoneAdjustX.val = 0;
                        m_BoneAdjustY.val = 0;
                        m_BoneAdjustZ.val = 0;
                        m_MmdPersonGameObject._model.m_BoneAdjust.Clear();
                        m_MmdPersonGameObject.Refresh();
                    });
                }
            }

            //if (!WithAsset())
            //{
            //    CreateLabel("adjust person position/rotation", true, Color.yellow);
            //    var btn = CreateButton("Confirm Position/Rotation", true);
            //    if (btn != null)
            //    {
            //        btn.button.onClick.AddListener(SetTransform);
            //    }
            //}


            CreateHeader("Step 3:", false, Color.black);
            {
                var btn = CreateButton("Start Sample");
                if (btn != null)
                {
                    btn.button.onClick.AddListener(() =>
                    {
                        if (m_MmdPersonGameObject == null) return;
                        if (m_MmdPersonGameObject._model == null) return;
                        m_MmdPersonGameObject._playTime = 0;
                        fingerKeyFrames = GetFingerKeyFrames(startTime.val, endTime.val);
                        bodyKeyFrames = GetBodyKeyFrames(startTime.val, endTime.val);

                        StepPlay(startTime.val, endTime.val);
                    });
                }
            }

            {
                var btn = CreateButton("Pause", false);
                if (btn != null)
                    btn.button.onClick.AddListener(Pause);
            }
            {
                var btn = CreateButton("Continue", false);
                if (btn != null)
                    btn.button.onClick.AddListener(Continue);
            }
            {
                var btn = CreateButton("Next KeyFrame", false);
                if (btn != null)
                    btn.button.onClick.AddListener(NextKeyFrame);
            }

            sampleTimeLabel = CreateLabel(GetTimeText(), false, Color.black, false);

            CreateHeader("Step 4:", false, Color.black);
            {
                var btn = CreateButton("Export Person Animation");
                if (btn != null)
                {
                    btn.button.onClick.AddListener(ExportPersonAnimation);
                }
            }
        }
        public Dictionary<string, HashSet<int>> fingerKeyFrames;
        public HashSet<int> bodyKeyFrames;

        void SetBone(JSONStorableStringChooser js)
        {
            if (m_MmdPersonGameObject == null) return;
            string boneName = js.val;
            if (m_MmdPersonGameObject._model.m_BoneAdjust.ContainsKey(boneName))
            {
                Vector3 v = m_MmdPersonGameObject._model.m_BoneAdjust[boneName];
                m_BoneAdjustX.val = v.x;
                m_BoneAdjustY.val = v.y;
                m_BoneAdjustZ.val = v.z;
            }
            else
            {
                m_BoneAdjustX.val = 0;
                m_BoneAdjustY.val = 0;
                m_BoneAdjustZ.val = 0;
            }
            m_MmdPersonGameObject.Refresh();
        }
        void SetBoneAdjustX(float val)
        {
            if (m_MmdPersonGameObject == null) return;
            string boneName = m_BoneChooser.val;
            if (!m_MmdPersonGameObject._model.m_BoneAdjust.ContainsKey(boneName))
            {
                m_MmdPersonGameObject._model.m_BoneAdjust.Add(boneName, new Vector3());
            }
            var v = m_MmdPersonGameObject._model.m_BoneAdjust[boneName];
            m_MmdPersonGameObject._model.m_BoneAdjust[boneName] = new Vector3(val, v.y, v.z);
            m_MmdPersonGameObject.Refresh();
        }
        void SetBoneAdjustY(float val)
        {
            if (m_MmdPersonGameObject == null) return;
            string boneName = m_BoneChooser.val;
            if (!m_MmdPersonGameObject._model.m_BoneAdjust.ContainsKey(boneName))
            {
                m_MmdPersonGameObject._model.m_BoneAdjust.Add(boneName, new Vector3());
            }
            var v = m_MmdPersonGameObject._model.m_BoneAdjust[boneName];
            m_MmdPersonGameObject._model.m_BoneAdjust[boneName] = new Vector3(v.x, val, v.z);
            m_MmdPersonGameObject.Refresh();
        }
        void SetBoneAdjustZ(float val)
        {
            if (m_MmdPersonGameObject == null) return;
            string boneName = m_BoneChooser.val;
            if (!m_MmdPersonGameObject._model.m_BoneAdjust.ContainsKey(boneName))
            {
                m_MmdPersonGameObject._model.m_BoneAdjust.Add(boneName, new Vector3());
            }
            var v = m_MmdPersonGameObject._model.m_BoneAdjust[boneName];
            m_MmdPersonGameObject._model.m_BoneAdjust[boneName] = new Vector3(v.x, v.y, val);
            m_MmdPersonGameObject.Refresh();
        }
        public HashSet<int> GetBodyKeyFrames(float beginTime, float endTime)
        {
            int startFrame = (int)(beginTime * 30);
            int endFrame = (int)(endTime * 30);

            var motions =m_MmdPersonGameObject._motion.BoneMotions;
            HashSet<int> frames = new HashSet<int>();
            foreach (var item in motions)
            {
                bool include = true;
                    if (DazBoneMapping.fingerBoneNames.Contains(item.Key))
                        include = false;
                if (include)
                {
                    foreach (var item2 in item.Value)
                    {
                        int frame = item2.Key;
                        if (frame >= startFrame && frame <= endFrame)
                            frames.Add(frame);
                    }
                }
            }
            return frames;
        }
        Dictionary<string, HashSet<int>> GetFingerKeyFrames(float beginTime, float endTime)
        {
            int startFrame = (int)(beginTime * 30);
            int endFrame = (int)(endTime * 30);

            var motions = m_MmdPersonGameObject._motion.BoneMotions;
            Dictionary<string, HashSet<int>> ret = new Dictionary<string, HashSet<int>>();
            foreach (var item in motions)
            {
                if (DazBoneMapping.fingerBoneNames.Contains(item.Key))
                {
                    HashSet<int> list = new HashSet<int>();
                    foreach (var item2 in item.Value)
                    {
                        int frame = item2.Key;
                        if (frame >= startFrame && frame <= endFrame)
                            list.Add(frame);
                    }
                    ret.Add(item.Key, list);
                }
            }

            return ret;
        }

        void SetSampleSpeed(float val)
        {
            if (m_MmdPersonGameObject == null) return;
            //StepSpeed = val;
        }
        void SetProgress(float val)
        {
            if (m_MmdPersonGameObject == null) return;
            m_MmdPersonGameObject.SetMotionPos(val);

        }
        void SetStartTime(float val)
        {
            if (m_MmdPersonGameObject == null) return;
            m_MmdPersonGameObject.SetMotionPos(val);
            float max = endTime.max;
            endTime.min= val;
            endTime.max = max;
        }
        void SetEndTime(float val)
        {
            if (m_MmdPersonGameObject == null) return;
            //endTime.max = plugin.mmdGameObject.MotionLength;
            endTime.val = val;
        }

        void ImportVmd(string path)
        {
            m_MmdPersonGameObject.LoadMotion(path);
            m_MmdPersonGameObject.SetMotionPos(0);
        }
        void SetTransform()
        {
            Transform tf = containingAtom.mainController.transform;
            rootHandler.transform.SetPositionAndRotation(tf.position, tf.rotation);
        }

        public MmdGameObject m_MmdPersonGameObject;
        Coroutine m_ChoosePerson = null;
        public void InitAtom()
        {
            if (m_MmdPersonGameObject != null)
            {
                Destroy(m_MmdPersonGameObject);
                m_MmdPersonGameObject = null;
            }

            if (m_ChoosePerson != null)
            {
                StopCoroutine(m_ChoosePerson);
                m_ChoosePerson = null;
            }
            if (m_SampleCo != null)
            {
                StopCoroutine(m_SampleCo);
                m_SampleCo = null;
            }

            StopAllCoroutines();
            IsSampling = false;
            CurFrame = 0;
            m_BeginTime = 0;
            m_EndTime = 0;

            importedVmdLabel.text = "Imported Vmd:None";

            m_BoneAdjustX.val = 0;
            m_BoneAdjustY.val = 0;
            m_BoneAdjustZ.val = 0;
            m_ChoosePerson = StartCoroutine(CoChoosePerson());
        }
        IEnumerator CoChoosePerson()
        {
            //需要摆成A-pose
            containingAtom.tempFreezePhysics = true;
            containingAtom.ResetPhysics(true, true);
            //等15帧，等reset结束
            for (int i = 0; i < 20; i++)
            {
                yield return null;
            }

            for (int i = 0; i < 5; i++)
            {
                yield return null;
            }
            CoLoad();

            //var pos = containingAtom.mainController.transform.localPosition;
            //containingAtom.mainController.transform.localPosition = new Vector3(pos.x, posY.val, pos.z);
            //Transform tf = containingAtom.mainController.transform;
            //rootHandler.transform.SetPositionAndRotation(tf.position, tf.rotation);
            containingAtom.tempFreezePhysics = false;
            m_ChoosePerson = null;
        }
        public Dictionary<string, DAZMorph> m_FaceMorphs = new Dictionary<string, DAZMorph>();
        Dictionary<Transform, FreeControllerV3> controllerLookup;
        Dictionary<string, FreeControllerV3> controllerNameLookup;
        public Transform rootHandler;
        void Prepare()
        {
            //先记一下
            //设置handControl模式
            var leftHandControl = containingAtom.GetStorableByID("LeftHandControl");
            leftHandControl.SetStringChooserParamValue("fingerControlMode", "JSONParams");
            var rightHandControl = containingAtom.GetStorableByID("RightHandControl");
            rightHandControl.SetStringChooserParamValue("fingerControlMode", "JSONParams");

            Utility.ResetHandControl(leftHandControl as HandControl);
            Utility.ResetHandControl(rightHandControl as HandControl);

            //拿到所有的面部变形
            DAZCharacterSelector cs = containingAtom.GetStorableByID("geometry") as DAZCharacterSelector;
            var morphControl = cs.morphsControlUI;
            m_FaceMorphs.Clear();

            //脸部变形
            foreach (var item in morphControl.GetMorphs())
            {
                if (item.region == "Face" || item.region == "Head" || item.region == "Eyes")
                    m_FaceMorphs.Add(item.uid, item);
            }

            controllerLookup = new Dictionary<Transform, FreeControllerV3>();
            controllerNameLookup = new Dictionary<string, FreeControllerV3>();

            foreach (var item in containingAtom.freeControllers)
            {
                //男性生殖器的控制点不处理
                if (item.name == "testesControl"
                    || item.name == "penisBaseControl"
                    || item.name == "penisMidControl"
                    || item.name == "penisTipControl") continue;

                controllerNameLookup.Add(item.name, item);
                //胸部跟着动就行
                if (item.name == "rNippleControl" || item.name == "lNippleControl")
                    continue;

                if (enableHeel.val)
                {
                    //高跟状态，不打开了
                    if (item.name == "lToeControl" || item.name == "rToeControl")
                    {
                        item.currentRotationState = FreeControllerV3.RotationState.Off;
                        item.currentPositionState = FreeControllerV3.PositionState.Off;

                        var p2 = item.GetFloatJSONParam("jointDriveXTarget");
                        p2.val = toeJointDriveXTargetAdjust.val;
                        continue;
                    }
                }
                if (item.name == "lToeControl" || item.name == "rToeControl")
                {
                    var p2 = item.GetFloatJSONParam("jointDriveXTarget");
                    p2.val = p2.defaultVal;
                }

                if (item.name == "lFootControl" || item.name == "rFootControl")
                {
                    var p1 = item.GetFloatJSONParam("holdRotationMaxForce");
                    var p2 = item.GetFloatJSONParam("jointDriveXTarget");
                    if (enableHeel.val)
                    {
                        p1.val = holdRotationMaxForceAdjust.val;
                        p2.val = footJointDriveXTargetAdjust.val;
                    }
                    else
                    {
                        p1.val = p1.defaultVal;
                        p2.val = p2.defaultVal;
                    }
                }
                item.currentRotationState = FreeControllerV3.RotationState.On;
                item.currentPositionState = FreeControllerV3.PositionState.On;

                if (item.followWhenOff != null)
                {
                    controllerLookup.Add(item.followWhenOff, item);
                }
            }
        }

        void CoLoad()
        {
            Prepare();
            var mmdObj = MmdGameObject.CreateGameObject("MmdGameObject");
            mmdObj.transform.position = containingAtom.transform.position;
            mmdObj.transform.rotation = containingAtom.transform.rotation;
            mmdObj.transform.localScale = new Vector3(0.1f, 0.1f, 0.1f);
            m_MmdPersonGameObject = mmdObj.GetComponent<MmdGameObject>();

            GameObject newRoot = new GameObject("MmdRoot");
            newRoot.transform.position = containingAtom.mainController.transform.position;
            newRoot.transform.rotation = containingAtom.mainController.transform.rotation;

            //Debug.LogWarning("rotation:" + containingAtom.transform.rotation + " " + containingAtom.mainController.transform.rotation);
            //Debug.LogWarning("position:" + containingAtom.transform.position + " " + containingAtom.mainController.transform.position);
            mmdObj.transform.parent = newRoot.transform;
            rootHandler = newRoot.transform;


            GameObject temp = new GameObject();
            temp.transform.position = containingAtom.transform.position;
            //temp.transform.rotation = mmdObj.transform.rotation;// Quaternion.identity;
            //temp.transform.Rotate(new Vector3(0, 180, 0),Space.Self);
            temp.transform.localEulerAngles = new Vector3(0, 180, 0);
            //转180°
            //temp.transform.rotation =  mmdObj.transform.rotation* Quaternion.Euler(0, 180, 0);

            Transform parent2 = containingAtom.transform.Find("rescale2/PhysicsModel");
            if (parent2 == null)
            {
                parent2 = containingAtom.transform.Find("rescale2/MoveWhenInactive/PhysicsModel");
            }

            //创建一副假骨架，然后转成A-pose，然后取他的坐标点
            GameObject root = new GameObject("Root");
            Dictionary<string, Transform> check = DazBoneMapping.CreateFakeBones(root.transform,parent2);
            m_MmdPersonGameObject.m_ChangeInitTransform = model =>
            {
                for (int i = 0; i < model.Bones.Length; i++)
                {
                    var bone = model.Bones[i];
                    Vector3 pos = DazBoneMapping.GetPosition(parent2.gameObject, bone, bone.Name, check);
                    //mmd里面的尺寸大了10倍
                    bone.Position = 10 * (temp.transform.TransformPoint(pos) - temp.transform.position);
                }
            };

            //mmdGameObject.LoadModel("BepInEx/plugins/g2f.pmx");
            m_MmdPersonGameObject.LoadModel();
            GameObject.DestroyImmediate(root);
            //GameObject.DestroyImmediate(temp);
            //targetAtom.mainController.transform.position = atomInitPosition;

            //加载模型之后再转向
            m_MmdPersonGameObject.transform.localEulerAngles = new Vector3(0, 180, 0);
            //var rootPos = m_TargetAtom.mainController.transform.position;

            m_MmdPersonGameObject.OnUpdate = mmd =>
            {
                List<GameObject> fingerBones = new List<GameObject>();
                if (!Settings.s_OnlyFace)
                {
                    var bones2 = mmd._bones;
                    List<GameObject> thumbBones = new List<GameObject>();
                    foreach (var item in bones2)
                    {
                        var rotation = item.transform.rotation;
                        if (DazBoneMapping.fingerBoneNames.Contains(item.name))
                        {
                            fingerBones.Add(item);
                            continue;
                        }
                        string pmxBoneName = item.name;
                        if (DazBoneMapping.ignoreUpdateBoneNames.Contains(pmxBoneName))
                            continue;

                        if (DazBoneMapping.cachedBoneLookup.ContainsKey(pmxBoneName))
                        {
                            Transform boneTf = DazBoneMapping.cachedBoneLookup[item.name];
                            if (boneTf != null)
                            {
                                if (enableHeel.val)
                                {
                                    if (boneTf.name == "lToe" || boneTf.name == "rToe")
                                    {
                                        continue;
                                    }
                                }


                                Vector3 pos = item.transform.position;// + rootPosition;// + rootPos;
                                if (controllerLookup.ContainsKey(boneTf))
                                {
                                    if (DazBoneMapping.armBones.Contains(pmxBoneName))
                                    {
                                        if (pmxBoneName.StartsWith("r") || pmxBoneName.StartsWith("右"))
                                        {
                                            controllerLookup[boneTf].transform.SetPositionAndRotation(pos, 
                                                rotation * Quaternion.Euler(new Vector3(0, 0, 36)) * Utility.quat);
                                        }
                                        else
                                        {
                                            controllerLookup[boneTf].transform.SetPositionAndRotation(pos, 
                                                rotation * Quaternion.Euler(new Vector3(0, 0, -36)) * Utility.quat);
                                        }
                                    }
                                    else
                                    {
                                       //if (boneTf.name == "lShin"|| boneTf.name == "rShin")
                                       // {
                                       //     //直腿模式
                                       //     //Settings.s_StraightLeg
                                       //     controllerLookup[boneTf].transform.SetPositionAndRotation(pos, rotation * Utility.quat);
                                       // }
                                       // else
                                       // {
                                            controllerLookup[boneTf].transform.SetPositionAndRotation(pos, rotation * Utility.quat);
                                        //}
                                    }
                                    //controllerLookup[boneTf].gameObject.SendMessage("FixedUpdate");

                                    var c = controllerLookup[boneTf];
                                    if (c.followWhenOff != null)
                                    {
                                        //c.followWhenOff.transform.SetPositionAndRotation(c.transform.position, c.transform.rotation);

                                    }
                                }
                                else
                                {
                                    //这里设置会引发跳变
                                    //boneTf.SetPositionAndRotation(pos, rotation * quat);
                                }
                            }

                        }
                        else
                        {
                            //Debug.Log(item.name);
                        }
                    }
                    //最后再处理大拇指，这样才能达到握拳的目的
                    //foreach (var item in thumbBones)
                    //{
                    //    UpdateFinger(item);
                    //}
                    foreach(var item in fingerBones)
                    {
                        UpdateFinger(item);
                    }
                }

                //表情
                float time = GetRelativeTime();
                var morphs = mmd.GetUpdatedMorph(time);
                foreach (var item in morphs)
                {
                    if (m_FaceMorphs.ContainsKey(item.Key))
                    {
                        m_FaceMorphs[item.Key].morphValue = item.Value;
                    }
                }
                //Physics.autoSimulation = false;
                //Physics.Simulate(Time.fixedDeltaTime * 3);
                //Physics.autoSimulation = true;

                if (IsSampling)
                {
                    //录手指
                    foreach(var item in fingerBones)
                    {
                        Transform boneTf = DazBoneMapping.cachedBoneLookup[item.name];
                        int frame = CurFrame;

                        //只生成手指的关键帧的数据
                        //if (sampleRateChooser.val == "EveryFrame")//逐帧模式
                        {
                            var fingerOutput = boneTf.GetComponent<MeshVR.Hands.FingerOutput>();
                            RecordFinger(time, boneTf.name, fingerOutput);
                        }
                        //else
                        //{
                        //    if (fingerKeyFrames.ContainsKey(item.name) && fingerKeyFrames[item.name].Contains(frame))
                        //    {
                        //        var fingerOutput = boneTf.GetComponent<MeshVR.Hands.FingerOutput>();
                        //        RecordFinger(time, boneTf.name, fingerOutput);
                        //    }
                        //}
                    }
                    //控制点录制
                    foreach (var item in timelineControlLookup)
                    {
                        if (enableHeel.val)
                        {
                            if (item.Key == "lToeControl" || item.Key == "rToeControl")
                            {
                                continue;
                            }
                        }

                        var freeController = controllerNameLookup[item.Key];
                        TimelineControlJson json = item.Value;
                        int frame = CurFrame;
                        //if (sampleRateChooser.val == "EveryFrame")//逐帧模式
                        {
                            Utility.RecordController(time, freeController, json);
                        }
                        //else
                        //{
                        //    if (bodyKeyFrames.Contains(frame))
                        //    {
                        //        Utility.RecordController(time, freeController, json);
                        //    }
                        //}
                    }
                }
            };

        }
        void UpdateFinger(GameObject item)
        {
            string pmxBoneName = item.name;
            var rotation = item.transform.rotation;

            Transform boneTf = DazBoneMapping.cachedBoneLookup[item.name];

            //算出世界坐标系坐标
            var worldRotation = item.transform.rotation * Utility.quat;

            var parentRotation = item.transform.parent.rotation;
            var parentWorldRotation = parentRotation;
            if (pmxBoneName.StartsWith("r") || pmxBoneName.StartsWith("右"))
            {
                worldRotation = rotation * Quaternion.Euler(new Vector3(0, 0, 36)) * Utility.quat;
                parentWorldRotation = parentRotation * Quaternion.Euler(new Vector3(0, 0, 36)) * Utility.quat;
            }
            else
            {
                worldRotation = rotation * Quaternion.Euler(new Vector3(0, 0, -36)) * Utility.quat;
                parentWorldRotation = parentRotation * Quaternion.Euler(new Vector3(0, 0, -36)) * Utility.quat;
            }
            //算出本地rotation
            var localRotation = Quaternion.Inverse(parentWorldRotation) * worldRotation;

            //if (boneTf.name.Contains("Thumb"))
            //{
            //    LogUtil.Log("0." + boneTf.name + " " + item.transform.localEulerAngles);
            //    LogUtil.Log("1." + boneTf.name + " " + localRotation.eulerAngles);
            //    //localRotation = item.transform.localRotation;
            //}

            //boneTf.localRotation = localRotation;

            var dazBone = boneTf.GetComponent<DAZBone>();
            //需要同时设置joint的属性
            var joint = boneTf.GetComponent<ConfigurableJoint>();
            //根据本地的rotation算出joint的targetRotation
            //joint.SetTargetRotationLocal(localRotation, dazBone.startingLocalRotation);
            //初始的角度好像都是(0,0,0)
            joint.SetTargetRotationLocal(localRotation, Quaternion.identity);
            //var rot = ConfigurableJointExtensions2.GetJointRotationInJointAxisSpace(joint, dazBone.startingLocalRotation);
            //joint.targetRotation = rot;

            var fingerOutput = dazBone.GetComponent<MeshVR.Hands.FingerOutput>();
            fingerOutput.ConvertRotation(dazBone, joint);
            //刷新
            fingerOutput.UpdateOutput();

            //if (boneTf.name.Contains("Thumb"))
            //{
            //    LogUtil.Log("1." + boneTf.name + " " + dazBone.startingLocalRotation.eulerAngles);
            //    LogUtil.Log("2."+ boneTf.name + " " + boneTf.localRotation.eulerAngles);
            //    //localRotation = item.transform.localRotation;
            //    //LogUtil.Log("3."+ fingerOutput.currentBend+" "+ fingerOutput.currentSpread+" "+ fingerOutput.currentTwist);
            //}

            //if (IsSampling)
            //{
            //    int frame = CurFrame;
            //    float time = GetRelativeTime();

            //    //只生成手指的关键帧的数据
            //    if (fingerKeyFrames.ContainsKey(item.name) && fingerKeyFrames[item.name].Contains(frame))
            //    {
            //        RecordFinger(time, dazBone.name, fingerOutput);
            //    }
            //}
        }
        void RecordFinger(float time, string boneName, MeshVR.Hands.FingerOutput fingerOutput)
        {
            if (boneName == "lThumb1" || boneName == "rThumb1")
            {
                SetFingerKeyFrame(time, boneName, "thumbProximalBend", fingerOutput.currentBend);
                SetFingerKeyFrame(time, boneName, "thumbProximalSpread", fingerOutput.currentSpread);
                SetFingerKeyFrame(time, boneName, "thumbProximalTwist", fingerOutput.currentTwist);
            }
            else if (boneName == "lThumb2" || boneName == "rThumb2")
            {
                SetFingerKeyFrame(time, boneName, "thumbMiddleBend", fingerOutput.currentBend);
            }
            else if (boneName == "lThumb3" || boneName == "rThumb3")
            {
                SetFingerKeyFrame(time, boneName, "thumbDistalBend", fingerOutput.currentBend);
            }
            else if (boneName == "lIndex1" || boneName == "rIndex1")
            {
                SetFingerKeyFrame(time, boneName, "indexProximalBend", fingerOutput.currentBend);
                SetFingerKeyFrame(time, boneName, "indexProximalSpread", fingerOutput.currentSpread);
                SetFingerKeyFrame(time, boneName, "indexProximalTwist", fingerOutput.currentTwist);
            }
            else if (boneName == "lIndex2" || boneName == "rIndex2")
            {
                SetFingerKeyFrame(time, boneName, "indexMiddleBend", fingerOutput.currentBend);
            }
            else if (boneName == "lIndex3" || boneName == "rIndex3")
            {
                SetFingerKeyFrame(time, boneName, "indexDistalBend", fingerOutput.currentBend);
            }
            else if (boneName == "lMid1" || boneName == "rMid1")
            {
                SetFingerKeyFrame(time, boneName, "middleProximalBend", fingerOutput.currentBend);
                SetFingerKeyFrame(time, boneName, "middleProximalSpread", fingerOutput.currentSpread);
                SetFingerKeyFrame(time, boneName, "middleProximalTwist", fingerOutput.currentTwist);
            }
            else if (boneName == "lMid2" || boneName == "rMid2")
            {
                SetFingerKeyFrame(time, boneName, "middleMiddleBend", fingerOutput.currentBend);
            }
            else if (boneName == "lMid3" || boneName == "rMid3")
            {
                SetFingerKeyFrame(time, boneName, "middleDistalBend", fingerOutput.currentBend);
            }
            else if (boneName == "lRing1" || boneName == "rRing1")
            {
                SetFingerKeyFrame(time, boneName, "ringProximalBend", fingerOutput.currentBend);
                SetFingerKeyFrame(time, boneName, "ringProximalSpread", fingerOutput.currentSpread);
                SetFingerKeyFrame(time, boneName, "ringProximalTwist", fingerOutput.currentTwist);
            }
            else if (boneName == "lRing2" || boneName == "rRing2")
            {
                SetFingerKeyFrame(time, boneName, "ringMiddleBend", fingerOutput.currentBend);
            }
            else if (boneName == "lRing3" || boneName == "rRing3")
            {
                SetFingerKeyFrame(time, boneName, "ringDistalBend", fingerOutput.currentBend);
            }
            else if (boneName == "lPinky1" || boneName == "rPinky1")
            {
                SetFingerKeyFrame(time, boneName, "pinkyProximalBend", fingerOutput.currentBend);
                SetFingerKeyFrame(time, boneName, "pinkyProximalSpread", fingerOutput.currentSpread);
                SetFingerKeyFrame(time, boneName, "pinkyProximalTwist", fingerOutput.currentTwist);
            }
            else if (boneName == "lPinky2" || boneName == "rPinky2")
            {
                SetFingerKeyFrame(time, boneName, "pinkyMiddleBend", fingerOutput.currentBend);
            }
            else if (boneName == "lPinky3" || boneName == "rPinky3")
            {
                SetFingerKeyFrame(time, boneName, "pinkyDistalBend", fingerOutput.currentBend);
            }
        }


        public void ExportPersonAnimation()
        {
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
            fileBrowserUI.Show(path=>
            {
                if (string.IsNullOrEmpty(path)) return;
                if (!path.ToLower().EndsWith($".{_saveExt}")) path += $".{_saveExt}";

                //var output = Valve.Newtonsoft.Json.JsonConvert.SerializeObject(m_PersonAniJson);
                //FileManagerSecure.WriteAllText(path, output);

                SuperController.singleton.SaveJSON(m_PersonAniJson.GetJSONClass(), path);
            });
            fileBrowserUI.ActivateFileNameField();
        }
        private const string _saveExt = "json";
        private const string _saveFolder = "Saves";
    }
}