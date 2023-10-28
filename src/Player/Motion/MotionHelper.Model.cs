using LibMMD.Motion;
using LibMMD.Unity3D;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace mmd2timeline
{
    internal partial class MotionHelper
    {
        bool hasAtomInited = false;

        /// <summary>
        /// 获取原子是否完成初始化
        /// </summary>
        internal bool HasAtomInited
        {
            get { return hasAtomInited; }
        }

        /// <summary>
        /// 重置原子
        /// </summary>
        public void ResetAtom()
        {
            hasAtomInited = false;

            if (_MmdPersonGameObject != null)
            {
                UnityEngine.Object.Destroy(_MmdPersonGameObject);
                _MmdPersonGameObject = null;
            }

            if (_ChoosePerson != null)
            {
                SuperController.singleton.StopCoroutine(_ChoosePerson);
                _ChoosePerson = null;
            }
            //if (m_SampleCo != null)
            //{
            //    StopCoroutine(m_SampleCo);
            //    m_SampleCo = null;
            //}

            SuperController.singleton.StopAllCoroutines();
            //IsSampling = false;
            //CurFrame = 0;
            //m_BeginTime = 0;
            //m_EndTime = 0;

            //importedVmdLabel.text = "Imported Vmd:None";

            //m_BoneAdjustX.val = 0;
            //m_BoneAdjustY.val = 0;
            //m_BoneAdjustZ.val = 0;
            //motionScale.val = 1;
            //_ChoosePerson = SuperController.singleton.StartCoroutine(CoChoosePerson());
        }

        /// <summary>
        /// 重置人物动作
        /// </summary>
        /// <param name="atom"></param>
        private void ResetPose()
        {
            _PersonAtom.ResetPhysics(true, true);

            AtomUI componentInChildren = _PersonAtom.GetComponentInChildren<AtomUI>();
            if (componentInChildren != null)
            {
                componentInChildren?.resetButton?.onClick?.Invoke();
            }
        }

        internal IEnumerator CoInitAtom()
        {
            var prePosition = _PersonAtom.mainController.transform.position;
            var preRotation = _PersonAtom.mainController.transform.rotation.eulerAngles;

            //需要摆成A-pose
            _PersonAtom.tempFreezePhysics = true;
            ResetPose();
            for (int i = 0; i < 30; i++)
                yield return null;
            _PersonAtom.mainController.SetPositionNoForce(prePosition);
            _PersonAtom.mainController.SetRotationNoForce(preRotation);
            _PersonAtom.tempFreezePhysics = false;
            for (int i = 0; i < 30; i++)
                yield return null;
            UpdateTransform();
            yield return null;
            //foreach (var item in containingAtom.freeControllers)
            //{
            //    if(item!= containingAtom.mainController)
            //    {
            //        item.ResetControl();
            //    }
            //}
            //for (int i = 0; i < 30; i++)
            //    yield return null;

            CoLoad();

            //var pos = containingAtom.mainController.transform.localPosition;
            //containingAtom.mainController.transform.localPosition = new Vector3(pos.x, posY.val, pos.z);
            //Transform tf = containingAtom.mainController.transform;
            //rootHandler.transform.SetPositionAndRotation(tf.position, tf.rotation);
            //_PersonAtom.tempFreezePhysics = false;

            //for (int i = 0; i < 30; i++)
            //    yield return null;
            //UpdateTransform();
            //yield return null;
            _ChoosePerson = null;

            yield return null;
            hasAtomInited = true;
        }

        //void Prepare2()
        //{
        //    //先记一下
        //    //设置handControl模式
        //    var leftHandControl = _PersonAtom.GetStorableByID("LeftHandControl");
        //    leftHandControl.SetStringChooserParamValue("fingerControlMode", "JSONParams");
        //    var rightHandControl = _PersonAtom.GetStorableByID("RightHandControl");
        //    rightHandControl.SetStringChooserParamValue("fingerControlMode", "JSONParams");

        //    Utility.ResetHandControl(leftHandControl as HandControl);
        //    Utility.ResetHandControl(rightHandControl as HandControl);

        //    //拿到所有的面部变形
        //    #region 准备面部变形的处理
        //    // 获取变形控制器UI对象
        //    var morphsControlUI = (this._PersonAtom.GetStorableByID("geometry") as DAZCharacterSelector).morphsControlUI;
        //    // 清理面部变形列表
        //    this._FaceMorphs.Clear();

        //    // 循环获取面部变形
        //    foreach (var dazmorph in morphsControlUI.GetMorphs())
        //    {
        //        // 如果是面部区域
        //        if (IsFace(dazmorph.region))
        //        {
        //            // 将变形id和变形添加到面部变形缓存字典
        //            this._FaceMorphs.Add(dazmorph.uid, dazmorph);
        //        }
        //    }
        //    #endregion

        //    controllerLookup = new Dictionary<Transform, FreeControllerV3>();
        //    controllerNameLookup = new Dictionary<string, FreeControllerV3>();

        //    foreach (var item in _PersonAtom.freeControllers)
        //    {
        //        //男性生殖器的控制点不处理
        //        if (item.name == "testesControl"
        //            || item.name == "penisBaseControl"
        //            || item.name == "penisMidControl"
        //            || item.name == "penisTipControl") continue;

        //        controllerNameLookup.Add(item.name, item);
        //        //胸部跟着动就行
        //        if (item.name == "rNippleControl" || item.name == "lNippleControl")
        //            continue;

        //        if (EnableHeel)
        //        {
        //            //高跟状态，不打开了
        //            if (item.name == "lToeControl" || item.name == "rToeControl")
        //            {
        //                item.currentRotationState = FreeControllerV3.RotationState.Off;
        //                item.currentPositionState = FreeControllerV3.PositionState.Off;

        //                var p2 = item.GetFloatJSONParam("jointDriveXTarget");
        //                p2.val = _ToeJointDriveXTargetAdjust.val;
        //                continue;
        //            }
        //        }
        //        if (item.name == "lToeControl" || item.name == "rToeControl")
        //        {
        //            var p2 = item.GetFloatJSONParam("jointDriveXTarget");
        //            p2.val = p2.defaultVal;
        //        }

        //        if (item.name == "lFootControl" || item.name == "rFootControl")
        //        {
        //            var p1 = item.GetFloatJSONParam("holdRotationMaxForce");
        //            var p2 = item.GetFloatJSONParam("jointDriveXTarget");
        //            if (EnableHeel)
        //            {
        //                p1.val = _HoldRotationMaxForceAdjust.val;
        //                p2.val = _FootJointDriveXTargetAdjust.val;
        //            }
        //            else
        //            {
        //                p1.val = p1.defaultVal;
        //                p2.val = p2.defaultVal;
        //            }
        //        }
        //        item.currentRotationState = FreeControllerV3.RotationState.On;
        //        item.currentPositionState = FreeControllerV3.PositionState.On;

        //        if (item.followWhenOff != null)
        //        {
        //            controllerLookup.Add(item.followWhenOff, item);
        //        }
        //    }
        //}

        //void CoLoad2()
        //{
        //    Prepare2();

        //    var mmdObj = MmdGameObject.CreateGameObject(Utility.GameObjectHead + "MmdGameObject");
        //    mmdObj.transform.localScale = new Vector3(0.1f, 0.1f, 0.1f);
        //    _MmdPersonGameObject = mmdObj.GetComponent<MmdGameObject>();

        //    GameObject newRoot = new GameObject(Utility.GameObjectHead + "MmdRoot");
        //    //if (Config.s_Debug)
        //    {
        //        var go = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        //        go.transform.parent = newRoot.transform;
        //        go.transform.localPosition = Vector3.zero;
        //        go.transform.localScale = new Vector3(0.2f, 0.2f, 0.2f);
        //        go.GetComponent<MeshRenderer>().material.color = Color.yellow;
        //        var col = go.GetComponent<Collider>();
        //        Component.DestroyImmediate(col);
        //    }

        //    //Debug.LogWarning("rotation:" + containingAtom.transform.rotation + " " + containingAtom.mainController.transform.rotation);
        //    //Debug.LogWarning("position:" + containingAtom.transform.position + " " + containingAtom.mainController.transform.position);
        //    mmdObj.transform.parent = newRoot.transform;
        //    rootHandler = newRoot.transform;

        //    newRoot.transform.position = _PersonAtom.mainController.transform.position;
        //    newRoot.transform.rotation = _PersonAtom.mainController.transform.rotation;


        //    GameObject temp = new GameObject(Utility.GameObjectHead + "temp");
        //    temp.transform.position = _PersonAtom.transform.position;
        //    //转180°
        //    //temp.transform.localEulerAngles = new Vector3(0, 180, 0);
        //    temp.transform.rotation = _PersonAtom.transform.rotation * Quaternion.Euler(0, 180, 0);
        //    Debug.LogError(temp.transform.localEulerAngles);

        //    Transform parent2 = _PersonAtom.transform.Find("rescale2/PhysicsModel");
        //    if (parent2 == null)
        //    {
        //        parent2 = _PersonAtom.transform.Find("rescale2/MoveWhenInactive/PhysicsModel");
        //    }

        //    _MmdPersonGameObject.m_MatchBone = model =>
        //    {
        //        DazBoneMapping.MatchTarget(_PersonAtom, _MmdPersonGameObject, parent2);
        //    };
        //    _MmdPersonGameObject.LoadModel();
        //    //GameObject.DestroyImmediate(root);
        //    //GameObject.DestroyImmediate(temp);
        //    //targetAtom.mainController.transform.position = atomInitPosition;

        //    //加载模型之后再转向
        //    _MmdPersonGameObject.transform.localEulerAngles = new Vector3(0, 180, 0);
        //    //var rootPos = m_TargetAtom.mainController.transform.position;

        //    cachedBoneLookup = new Dictionary<string, Transform>();
        //    foreach (var item in _MmdPersonGameObject._model.Bones)
        //    {
        //        string name = item.Name;
        //        string boneName = name;
        //        if (DazBoneMapping.boneNames.ContainsKey(name))
        //            boneName = DazBoneMapping.boneNames[name];
        //        if (!boneName.Contains("|"))
        //        {
        //            var tf = DazBoneMapping.SearchObjName(parent2, boneName);
        //            if (cachedBoneLookup == null)
        //                cachedBoneLookup = new Dictionary<string, Transform>();
        //            cachedBoneLookup[name] = tf;
        //        }
        //    }

        //    #region 挑出有效骨骼
        //    listFingerGameObject.Clear();
        //    validBoneNames.Clear();
        //    // 轮询骨骼清单挑出有效的骨骼
        //    foreach (var bone in _MmdPersonGameObject._bones)
        //    {
        //        if (DazBoneMapping.fingerBoneNames.Contains(bone.name))
        //        {
        //            listFingerGameObject.Add(bone);

        //            if (!validBoneNames.ContainsKey(bone.name))
        //            {
        //                validBoneNames.Add(bone.name, "");
        //            }
        //        }
        //        else
        //        {
        //            var boneName = bone.name;
        //            if (!DazBoneMapping.ignoreUpdateBoneNames.Contains(boneName) && cachedBoneLookup.ContainsKey(boneName))
        //            {
        //                Transform boneTransform = cachedBoneLookup[boneName];
        //                if (boneTransform != null)
        //                {
        //                    if (this.controllerLookup.ContainsKey(boneTransform))
        //                    {
        //                        var controller = this.controllerLookup[boneTransform];

        //                        if (!validBoneNames.ContainsKey(bone.name))
        //                        {
        //                            validBoneNames.Add(bone.name, controller.name);
        //                        }
        //                    }
        //                }
        //            }
        //        }
        //    }
        //    #endregion
        //    _MmdPersonGameObject.OnUpdate = UpdateMotion;

        //    //_MmdPersonGameObject.OnUpdate = mmd =>
        //    //{
        //    //    List<GameObject> fingerBones = new List<GameObject>();
        //    //    if (!Config.s_OnlyFace)
        //    //    {
        //    //        //var bones2 = mmd._bones;
        //    //        List<GameObject> thumbBones = new List<GameObject>();
        //    //        foreach (var item in mmd._bones)
        //    //        {
        //    //            var rotation = item.transform.rotation;
        //    //            if (DazBoneMapping.fingerBoneNames.Contains(item.name))
        //    //            {
        //    //                fingerBones.Add(item);
        //    //                continue;
        //    //            }
        //    //            string pmxBoneName = item.name;
        //    //            if (DazBoneMapping.ignoreUpdateBoneNames.Contains(pmxBoneName))
        //    //                continue;

        //    //            if (cachedBoneLookup.ContainsKey(pmxBoneName))
        //    //            {
        //    //                Transform boneTf = cachedBoneLookup[item.name];
        //    //                if (boneTf != null)
        //    //                {
        //    //                    if (EnableHeel)
        //    //                    {
        //    //                        if (boneTf.name == "lToe" || boneTf.name == "rToe")
        //    //                        {
        //    //                            continue;
        //    //                        }
        //    //                    }


        //    //                    Vector3 pos = item.transform.position;// + rootPosition;// + rootPos;
        //    //                    if (controllerLookup.ContainsKey(boneTf))
        //    //                    {
        //    //                        if (DazBoneMapping.armBones.Contains(pmxBoneName))
        //    //                        {
        //    //                            if (pmxBoneName.StartsWith("r") || pmxBoneName.StartsWith("右"))
        //    //                            {
        //    //                                controllerLookup[boneTf].transform.SetPositionAndRotation(pos,
        //    //                                    rotation * Quaternion.Euler(new Vector3(0, 0, 36)) * Utility.quat);
        //    //                            }
        //    //                            else
        //    //                            {
        //    //                                controllerLookup[boneTf].transform.SetPositionAndRotation(pos,
        //    //                                    rotation * Quaternion.Euler(new Vector3(0, 0, -36)) * Utility.quat);
        //    //                            }
        //    //                        }
        //    //                        else
        //    //                        {
        //    //                            controllerLookup[boneTf].transform.SetPositionAndRotation(pos, rotation * Utility.quat);
        //    //                        }

        //    //                        var c = controllerLookup[boneTf];
        //    //                        if (c.followWhenOff != null)
        //    //                        {
        //    //                            //c.followWhenOff.transform.SetPositionAndRotation(c.transform.position, c.transform.rotation);

        //    //                        }
        //    //                    }
        //    //                    else
        //    //                    {
        //    //                        //这里设置会引发跳变
        //    //                        //boneTf.SetPositionAndRotation(pos, rotation * quat);
        //    //                    }
        //    //                }

        //    //            }
        //    //            else
        //    //            {
        //    //                //Debug.Log(item.name);
        //    //            }
        //    //        }
        //    //        //最后再处理大拇指，这样才能达到握拳的目的
        //    //        //foreach (var item in thumbBones)
        //    //        //{
        //    //        //    UpdateFinger(item);
        //    //        //}
        //    //        foreach (var item in fingerBones)
        //    //        {
        //    //            UpdateFinger(item);
        //    //        }
        //    //    }

        //    //    //表情
        //    //    float time = GetRelativeTime();
        //    //    var morphs = mmd.GetUpdatedMorph(time);
        //    //    foreach (var item in morphs)
        //    //    {
        //    //        if (_FaceMorphs.ContainsKey(item.Key))
        //    //        {
        //    //            _FaceMorphs[item.Key].morphValue = item.Value;
        //    //        }
        //    //    }
        //    //    //Physics.autoSimulation = false;
        //    //    //Physics.Simulate(Time.fixedDeltaTime * 3);
        //    //    //Physics.autoSimulation = true;

        //    //    //if (IsSampling)
        //    //    //{
        //    //    //    //录手指
        //    //    //    foreach(var item in fingerBones)
        //    //    //    {
        //    //    //        Transform boneTf = cachedBoneLookup[item.name];
        //    //    //        int frame = CurFrame;

        //    //    //        //只生成手指的关键帧的数据
        //    //    //        //if (sampleRateChooser.val == "EveryFrame")//逐帧模式
        //    //    //        {
        //    //    //            var fingerOutput = boneTf.GetComponent<MeshVR.Hands.FingerOutput>();
        //    //    //            RecordFinger(time, boneTf.name, fingerOutput);
        //    //    //        }
        //    //    //        //else
        //    //    //        //{
        //    //    //        //    if (fingerKeyFrames.ContainsKey(item.name) && fingerKeyFrames[item.name].Contains(frame))
        //    //    //        //    {
        //    //    //        //        var fingerOutput = boneTf.GetComponent<MeshVR.Hands.FingerOutput>();
        //    //    //        //        RecordFinger(time, boneTf.name, fingerOutput);
        //    //    //        //    }
        //    //    //        //}
        //    //    //    }
        //    //    //    //控制点录制
        //    //    //    foreach (var item in timelineControlLookup)
        //    //    //    {
        //    //    //        if (enableHeel.val)
        //    //    //        {
        //    //    //            if (item.Key == "lToeControl" || item.Key == "rToeControl")
        //    //    //            {
        //    //    //                continue;
        //    //    //            }
        //    //    //        }

        //    //    //        var freeController = controllerNameLookup[item.Key];
        //    //    //        TimelineControlJson json = item.Value;
        //    //    //        int frame = CurFrame;
        //    //    //        //if (sampleRateChooser.val == "EveryFrame")//逐帧模式
        //    //    //        {
        //    //    //            Utility.RecordController(time, freeController, json);
        //    //    //        }
        //    //    //        //else
        //    //    //        //{
        //    //    //        //    if (bodyKeyFrames.Contains(frame))
        //    //    //        //    {
        //    //    //        //        Utility.RecordController(time, freeController, json);
        //    //    //        //    }
        //    //    //        //}
        //    //    //    }
        //    //    //}
        //    //};

        //}
        ///// <summary>
        ///// 根据骨骼部位情况获得高跟修正高度
        ///// </summary>
        ///// <param name="bones"></param>
        ///// <param name="floorHeight"></param>
        ///// <param name="horizon"></param>
        ///// <returns></returns>
        //private float GetHeelFixHeight(GameObject[] bones, float floorHeight, float horizon)
        //{
        //    var reviseY = 0f;

        //    if (EnableHeel)
        //    {
        //        try
        //        {
        //            // 查找高度最低的骨骼
        //            var lowestBones = bones.Where(b =>
        //            {
        //                var name = validBoneNames[b.name];

        //                // 忽略手部、膝盖、脚趾关节
        //                if (string.IsNullOrEmpty(name) || name.EndsWith("HandControl")/* || name.EndsWith("ToeControl")*/)
        //                {
        //                    return false;
        //                }

        //                return true;
        //            }).OrderBy(b => b.transform.position.y).ToList();

        //            var fix = 0f;
        //            var lowestBoneName = "";
        //            var lowestY = 0f;
        //            kneeFixed = false;
        //            heelFixed = false;
        //            lKneeFixed = false;
        //            rKneeFixed = false;

        //            foreach (var lowestBone in lowestBones)
        //            {
        //                // Y轴最小的骨骼名称
        //                lowestBoneName = lowestBone.name;
        //                // Y轴最小位置
        //                lowestY = lowestBone.transform.position.y;

        //                if (validBoneNames.ContainsKey(lowestBoneName))
        //                {
        //                    // 最低控制器名称
        //                    lowestControlName = validBoneNames[lowestBoneName];

        //                    // 当启用高跟并且最低的控制器是脚趾，进行高跟高度修正的计算
        //                    if (EnableHeel && lowestControlName.EndsWith("ToeControl"))
        //                    {
        //                        if (!kneeFixed && !heelFixed)
        //                        {
        //                            fix += _HeelHeightAdjustJSON.val;
        //                            heelFixed = true;
        //                        }

        //                        continue;
        //                    }
        //                    else if (lowestControlName.EndsWith("KneeControl"))// 膝盖修正
        //                    {
        //                        // 获得膝盖触地的修正值
        //                        if (!kneeFixed)
        //                        {
        //                            if (controlFixValues.ContainsKey(lowestControlName))
        //                            {
        //                                var fixValue = controlFixValues[lowestControlName];
        //                                fix += fixValue;
        //                            }
        //                            kneeFixed = true;

        //                            // 如果跪地修正时
        //                            if (heelFixed)
        //                            {
        //                                fix -= _HeelHeightAdjustJSON.val;
        //                                heelFixed = false;
        //                            }
        //                        }

        //                        // 判定是左膝还是右膝
        //                        if (lowestControlName.StartsWith("l"))
        //                        {
        //                            lKneeFixed = true;
        //                        }
        //                        else
        //                        {
        //                            rKneeFixed = true;
        //                        }

        //                        // 进行下一个关节的检查
        //                        continue;
        //                    }
        //                    else if ((EnableHeel) && controlFixValues.ContainsKey(lowestControlName))
        //                    {
        //                        var fixValue = controlFixValues[lowestControlName];

        //                        fix += fixValue;
        //                        break;
        //                    }
        //                }
        //            }

        //            //if (config.ShowDebugInfo)
        //            //{
        //            //    showDebug = true;
        //            //    player.ShowStatusMessage($"Bone:{lowestBoneName}" +
        //            //        $"\n" +
        //            //        $"Control:{validBoneNames[lowestBoneName]}" +
        //            //        $"\n" +
        //            //        $"_MinY:{lowestY}" +
        //            //        $"\n" +
        //            //        $"kneeFixed:{kneeFixed}" +
        //            //        $"\n" +
        //            //        $"heelFixed:{heelFixed}");
        //            //}
        //            //else if (showDebug)
        //            //{
        //            //    showDebug = false;
        //            //    player.HideStatusMessage();
        //            //}

        //            reviseY = 0f;

        //            if (lowestY < horizon)
        //            {
        //                reviseY = floorHeight - lowestY;
        //            }

        //            reviseY += fix;
        //        }
        //        catch (Exception e)
        //        {
        //            LogUtil.LogError(e, $"FIX:::");
        //        }
        //    }
        //    else
        //    {
        //        reviseY = 0f;
        //    }

        //    return reviseY;
        //}
    }
}
