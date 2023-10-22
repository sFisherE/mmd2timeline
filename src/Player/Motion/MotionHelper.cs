using LibMMD.Model;
using LibMMD.Unity3D;
using MeshVR.Hands;
using mmd2timeline.Store;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace mmd2timeline
{
    /// <summary>
    /// 人物动作控制器
    /// </summary>
    internal partial class MotionHelper
    {
        /// <summary>
        /// 配置数据
        /// </summary>
        protected static readonly Config config = Config.GetInstance();

        /// <summary>
        /// 获取或设置人物原子
        /// </summary>
        public Atom PersonAtom
        {
            get { return _PersonAtom; }
            set { _PersonAtom = value; }
        }

        /// <summary>
        /// 人物原子
        /// </summary>
        private Atom _PersonAtom;

        /// <summary>
        /// 人物游戏对象
        /// </summary>
        public MmdGameObject _MmdPersonGameObject;

        /// <summary>
        /// 选择人物
        /// </summary>
        private Coroutine _ChoosePerson;

        /// <summary>
        /// 面部变形字典
        /// </summary>
        public Dictionary<string, DAZMorph> _FaceMorphs = new Dictionary<string, DAZMorph>();

        /// <summary>
        /// 控制器字典
        /// </summary>
        private Dictionary<Transform, FreeControllerV3> controllerLookup;

        /// <summary>
        /// 控制器名称字典
        /// </summary>
        private Dictionary<string, FreeControllerV3> controllerNameLookup;

        /// <summary>
        /// 根处理器
        /// </summary>
        public Transform rootHandler;

        /// <summary>
        /// 当前人物的动作数据
        /// </summary>
        PersonMotion _MotionSetting;

        /// <summary>
        /// 滞空时间
        /// </summary>
        const float MAX_HANGTIME = 0.427f;

        /// <summary>
        /// 最大可选择动作文件数量
        /// </summary>
        const int MAX_MOTION_COUNT = 4;

        /// <summary>
        /// 获取动作缩放率
        /// </summary>
        private float motionScaleRate
        {
            get
            {
                if (this._MotionScaleJSON != null)
                {
                    return this._MotionScaleJSON.val;
                }
                return 1f;
            }
        }

        /// <summary>
        /// 物理网格字典
        /// </summary>
        Dictionary<string, DAZPhysicsMesh> PhysicsMeshs = new Dictionary<string, DAZPhysicsMesh>();

        /// <summary>
        /// 设置物理网格的开关状态
        /// </summary>
        /// <param name="mesh"></param>
        /// <param name="on"></param>
        void SetPhysicsMesh(DAZPhysicsMesh mesh, bool on)
        {
            if (mesh != null)
            {
                mesh.on = on;
            }
        }

        /// <summary>
        /// 设置物理网格的开关状态
        /// </summary>
        /// <param name="key"></param>
        /// <param name="on"></param>
        public void SetPhysicsMesh(string key, bool on)
        {
            if (PhysicsMeshs.ContainsKey(key))
            {
                var mesh = PhysicsMeshs[key];

                SetPhysicsMesh(mesh, on);
            }
        }

        /// <summary>
        /// 实例化人物动作管理员
        /// </summary>
        /// <param name="personAtom"></param>
        public MotionHelper(Atom personAtom)
        {
            _PersonAtom = personAtom;

            InitEyeBehavior();

            #region 因舞蹈动作比较大时，嘴部会变形，找到嘴部的物理网格，并将其禁用
            DAZCharacterRun characterRun = _PersonAtom.GetComponentInChildren<DAZCharacterRun>();

            foreach (var mesh in characterRun.physicsMeshes)
            {
                var on = true;

                switch (mesh.name)
                {
                    case "MouthPhysicsMesh":
                        on = config.MouthPhysicsMesh;
                        break;
                    case "LowerPhysicsMesh":
                        on = config.LowerPhysicsMesh;
                        break;
                    case "BreastPhysicsMesh":
                        on = config.BreastPhysicsMesh;
                        break;
                }

                PhysicsMeshs.Add(mesh.name, mesh);

                SetPhysicsMesh(mesh, on);
            }
            #endregion

            Init();
        }
        Dictionary<string, Transform> cachedBoneLookup = new Dictionary<string, Transform>();
        // 手指关节游戏对象
        List<GameObject> listFingerGameObject = new List<GameObject>();
        /// <summary>
        /// 初始化
        /// </summary>
        private void Init()
        {
            // 初始化原子及模型
            InitAtom();

            // 轮询骨骼清单挑出有效的骨骼
            foreach (var bone in _MmdPersonGameObject._bones)
            {
                if (DazBoneMapping.fingerBoneNames.Contains(bone.name))
                {
                    if (!validBoneNames.ContainsKey(bone.name))
                    {
                        validBoneNames.Add(bone.name, "");
                    }
                }
                else
                {
                    var boneName = bone.name;
                    if (!DazBoneMapping.ignoreUpdateBoneNames.Contains(boneName) && cachedBoneLookup.ContainsKey(boneName))
                    {
                        Transform boneTransform = cachedBoneLookup[boneName];
                        if (boneTransform != null)
                        {
                            if (this.controllerLookup.ContainsKey(boneTransform))
                            {
                                var controller = this.controllerLookup[boneTransform];

                                if (!validBoneNames.ContainsKey(bone.name))
                                {
                                    validBoneNames.Add(bone.name, controller.name);
                                }
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// 初始化动作设置
        /// </summary>
        /// <param name="choices"></param>
        /// <param name="displayChoices"></param>
        /// <param name="choice"></param>
        /// <param name="motion"></param>
        internal void InitSettings(List<string> choices, List<string> displayChoices, PersonMotion settings)
        {
            _MotionSetting = settings;

            _delay = _MotionSetting?.TimeDelay ?? 0f;

            if (_delay != 0f)
            {
                // 先设定范围
                SetDelayRange(Mathf.Abs(_delay));

                SetTimeDelay(_delay);
            }
            else
            {
                // 重置延迟范围
                SetDelayRange(0f);
            }

            SetChoosers(displayChoices, choices, settings?.Files);

            InitSettingValues();

            LoadBoneRotationAdjustValues();
        }

        /// <summary>
        /// 重新加载动作数据
        /// </summary>
        public void ReloadMotions(int test, bool init = false)
        {
            if (init || config.ResetPhysicalWhenLoadMotion)
            {
                this.InitAtom();
            }
            else
            {
                _MmdPersonGameObject.ClearMotion();
            }

            _MotionSetting.Files.Clear();

            foreach (var motionChooser in _MotionChoosers)
            {
                if (motionChooser.val != noneString)
                {
                    _MotionSetting.Files.Add(motionChooser.val);
                    this.ImportVmd(motionChooser.val);
                }
            }
        }

        /// <summary>
        /// 保存动作设定到Settings
        /// </summary>
        internal void UpdateMotionsToSettings()
        {
            _MotionSetting.Files.Clear();

            foreach (var motionChooser in _MotionChoosers)
            {
                if (motionChooser.val != noneString)
                {
                    _MotionSetting.Files.Add(motionChooser.val);
                }
            }
        }

        /// <summary>
        /// 重置数据
        /// </summary>
        public void Reset()
        {
            MaxTime = 0f;

            ResetChoosers();
        }

        /// <summary>
        /// 更新动作
        /// </summary>
        private void ReUpdateMotion()
        {
            if (this._MmdPersonGameObject != null)
            {
                this._MmdPersonGameObject.SetMotionPos(this._MmdPersonGameObject._playTime, true, motionScale: motionScaleRate);
            }
        }

        /// <summary>
        /// 更新位置和角度
        /// </summary>
        private void UpdatePositionAndRotation()
        {
            Vector3 pos = Vector3.zero;
            Quaternion rat = Quaternion.identity;
            if (UICreated)
            {
                pos = new Vector3(_PositionX.val, _PositionY.val, _PositionZ.val);
                rat = new Quaternion(_RotationX.val, _RotationY.val, _RotationZ.val, 1);
            }

            Vector3 localPosition = _PersonAtom.mainController.transform.localPosition;
            _PersonAtom.mainController.transform.localPosition = pos;
            _PersonAtom.mainController.transform.localRotation = rat;
            this.UpdateTransform();
        }

        /// <summary>
        /// 导入VMD
        /// </summary>
        /// <param name="path"></param>
        private void ImportVmd(string path)
        {
            try
            {
                if (path == noneString || string.IsNullOrEmpty(path))
                {
                    return;
                }

                _MmdPersonGameObject.LoadMotion(path);
                _MmdPersonGameObject.SetMotionPos(0f, true, motionScale: motionScaleRate);

                #region 新的动作处理模式需要进行的关节筛选
                //var bones = _MmdPersonGameObject._poser.BoneImages;
                //if (bones != null)
                //{
                //    foreach (var item in bones)
                //    {
                //        if (!item.HasValue && DazBoneMapping.vamControlLookup.ContainsKey(item.Name))
                //        {
                //            LogUtil.Debug($"ImportVmd:::item.Name:{item.Name}");

                //            ignoreBoneNames.Add(DazBoneMapping.vamControlLookup[item.Name]);
                //        }
                //    }

                //}
                #endregion

                MaxTime = _MmdPersonGameObject.MotionLength;

                OnMotionLoaded?.Invoke(this, MaxTime);
            }
            catch (Exception ex)
            {
                LogUtil.LogError(ex, $"ImportVmd:{path}");
            }
        }
        /// <summary>
        /// 更新骨骼的Transform
        /// </summary>
        public void UpdateTransform()
        {
            Transform transform = this._PersonAtom.mainController.transform;
            this.rootHandler.transform.SetPositionAndRotation(transform.position, transform.rotation);
        }

        /// <summary>
        /// 初始化原子
        /// </summary>
        public void InitAtom()
        {
            try
            {
                if (this._MmdPersonGameObject != null)
                {
                    UnityEngine.Object.Destroy(this._MmdPersonGameObject);
                    this._MmdPersonGameObject = null;
                }
                if (this._ChoosePerson != null)
                {
                    SuperController.singleton.StopCoroutine(this._ChoosePerson);
                    this._ChoosePerson = null;
                }

                this._PersonAtom.tempFreezePhysics = true;
                //this._PersonAtom.ResetPhysics(true, true);
                foreach (var item in _PersonAtom.freeControllers)
                {
                    if (item != _PersonAtom.mainController)
                    {
                        item.ResetControl();
                    }
                }
                this.CoLoad();

                this._PersonAtom.tempFreezePhysics = false;
                this._ChoosePerson = null;

                //if (!config.LockPersonPosition && UICreated)
                //{
                //    this._PositionX.SetValToDefault();
                //    this._PositionY.SetValToDefault();
                //    this._PositionZ.SetValToDefault();

                //    this._RotationX.SetValToDefault();
                //    this._RotationY.SetValToDefault();
                //    this._RotationZ.SetValToDefault();
                //}
                //else
                //{
                // 更新位置
                //UpdatePositionAndRotation();
                //}

                SetPersonAllJoints();

                var AutoExpressions = this._PersonAtom.GetStorableByID("AutoExpressions");
                AutoExpressions.SetBoolParamValue("enabled", false);
            }
            catch (Exception ex)
            {
                LogUtil.LogError(ex);
            }
        }

        /// <summary>
        /// 根据配置设置人物的所有关节
        /// </summary>
        public void SetPersonAllJoints()
        {
            // 设置左右关节控制数据
            var allJointsController = this._PersonAtom.GetComponentInChildren<AllJointsController>();

            SetPersonAllJointsSpringPercent(allJointsController);
            SetPersonAllJointsDamperPercent(allJointsController);
            SetPersonAllJointsMaxVelocity(allJointsController);
        }

        /// <summary>
        /// 设置人物的所有关节弹簧比例
        /// </summary>
        /// <param name="allJointsController"></param>
        public void SetPersonAllJointsSpringPercent(AllJointsController allJointsController = null)
        {
            if (!UICreated)
                return;

            var v = config.AllJointsSpringPercent;
            if (this._UseAllJointsSettingsJSON.val)
            {
                v = this._AllJointsSpringPercentJSON.val;
            }
            SetPersonAllJointsSpringPercent(v, allJointsController);
        }

        /// <summary>
        /// 设置人物的所有关节弹簧比例
        /// </summary>
        /// <param name="v"></param>
        /// <param name="allJointsController"></param>
        public void SetPersonAllJointsSpringPercent(float v, AllJointsController allJointsController = null)
        {
            if (allJointsController == null)
            {
                // 设置左右关节控制数据
                allJointsController = this._PersonAtom.GetComponentInChildren<AllJointsController>();
            }
            var springPercentJSON = allJointsController.GetFloatJSONParam("springPercent");
            springPercentJSON.val = v;
            allJointsController.SetAllJointsPercentHoldSpring();
        }

        /// <summary>
        /// 设置人物所有关节阻尼比例
        /// </summary>
        /// <param name="allJointsController"></param>
        public void SetPersonAllJointsDamperPercent(AllJointsController allJointsController = null)
        {
            if (!UICreated)
                return;

            var v = config.AllJointsDamperPercent;
            if (this._UseAllJointsSettingsJSON.val)
            {
                v = this._AllJointsDamperPercentJSON.val;
            }
            SetPersonAllJointsDamperPercent(v, allJointsController);
        }

        /// <summary>
        /// 设置人物所有关节阻尼比例
        /// </summary>
        /// <param name="allJointsController"></param>
        public void SetPersonAllJointsDamperPercent(float v, AllJointsController allJointsController = null)
        {
            if (allJointsController == null)
            {
                // 设置左右关节控制数据
                allJointsController = this._PersonAtom.GetComponentInChildren<AllJointsController>();
            }
            var damperPercentJSON = allJointsController.GetFloatJSONParam("damperPercent");
            damperPercentJSON.val = v;
            allJointsController.SetAllJointsPercentHoldDamper();
        }

        /// <summary>
        /// 设置人物所有关节最大速度
        /// </summary>
        /// <param name="allJointsController"></param>
        public void SetPersonAllJointsMaxVelocity(AllJointsController allJointsController = null)
        {
            if (!UICreated)
                return;

            var v = config.AllJointsMaxVelocity;
            if (this._UseAllJointsSettingsJSON.val)
            {
                v = this._AllJointsMaxVelocityJSON.val;
            }
            SetPersonAllJointsMaxVelocity(v, allJointsController);
        }

        /// <summary>
        /// 设置人物所有关节最大速度
        /// </summary>
        /// <param name="allJointsController"></param>
        public void SetPersonAllJointsMaxVelocity(float v, AllJointsController allJointsController = null)
        {
            if (allJointsController == null)
            {
                // 设置左右关节控制数据
                allJointsController = this._PersonAtom.GetComponentInChildren<AllJointsController>();
            }
            var maxVelocityJSON = allJointsController.GetFloatJSONParam("maxVeloctiy");

            if (maxVelocityJSON == null)
            {
                maxVelocityJSON = allJointsController.GetFloatJSONParam("maxVelocity");
            }

            maxVelocityJSON.val = v;
            allJointsController.SetAllJointsMaxVelocity();
        }

        /// <summary>
        /// 检查是否是脸部
        /// </summary>
        /// <param name="region"></param>
        /// <returns></returns>
        private bool IsFace(string region)
        {
            var regionLower = region.ToLower();

            return regionLower.Contains("face") || regionLower.Contains("head") || regionLower.Contains("eye");
        }

        /// <summary>
        /// 准备加载
        /// </summary>
        internal void Prepare()
        {
            #region 准备手指控制处理
            // 获取左手控制器，并设置手指控制模式
            var leftHandControl = this._PersonAtom.GetStorableByID("LeftHandControl");
            leftHandControl.SetStringChooserParamValue("fingerControlMode", "JSONParams");
            // 获取右手控制器，并设置手指控制模式
            var rightHandControl = this._PersonAtom.GetStorableByID("RightHandControl");
            rightHandControl.SetStringChooserParamValue("fingerControlMode", "JSONParams");
            // 重置左手和右手控制器
            Utility.ResetHandControl(leftHandControl as HandControl);
            Utility.ResetHandControl(rightHandControl as HandControl);
            #endregion

            #region 准备面部变形的处理
            // 获取变形控制器UI对象
            var morphsControlUI = (this._PersonAtom.GetStorableByID("geometry") as DAZCharacterSelector).morphsControlUI;
            // 清理面部变形列表
            this._FaceMorphs.Clear();

            // 循环获取面部变形
            foreach (var dazmorph in morphsControlUI.GetMorphs())
            {
                // 如果是面部区域
                if (IsFace(dazmorph.region))
                {
                    // 将变形id和变形添加到面部变形缓存字典
                    this._FaceMorphs.Add(dazmorph.uid, dazmorph);
                }
            }
            #endregion

            // 初始化控制器和控制器名称字典
            this.controllerLookup = new Dictionary<Transform, FreeControllerV3>();
            this.controllerNameLookup = new Dictionary<string, FreeControllerV3>();

            // 轮询人物的控制器
            foreach (var controller in this._PersonAtom.freeControllers)
            {
                //男性生殖器的控制点不处理
                if (controller.name == "testesControl"
                    || controller.name == "penisBaseControl"
                    || controller.name == "penisMidControl"
                    || controller.name == "penisTipControl") continue;

                // 将控制器添加到控制器名称字典
                this.controllerNameLookup.Add(controller.name, controller);

                //胸部跟着动就行
                if (controller.name == "rNippleControl" || controller.name == "lNippleControl")
                    continue;

                // 如果允许高跟并且当前控制器是脚趾控制器
                if (EnableHeel && (controller.name == "lToeControl" || controller.name == "rToeControl"))
                {
                    // 关闭脚趾控制器的位置和角度状态
                    controller.currentRotationState = FreeControllerV3.RotationState.Off;
                    controller.currentPositionState = FreeControllerV3.PositionState.Off;
                    // 设置脚趾关节驱动X目标设定
                    controller.GetFloatJSONParam("jointDriveXTarget").val = this._ToeJointDriveXTargetAdjust.val;

                    continue;
                }

                // 如果是脚趾控制器
                if (controller.name == "lToeControl" || controller.name == "rToeControl")
                {
                    // 将脚趾的关节控制X目标值设置为默认值
                    var floatJSONParam = controller.GetFloatJSONParam("jointDriveXTarget");
                    floatJSONParam.val = floatJSONParam.defaultVal;
                }

                // 如果是足部控制器
                if (controller.name == "lFootControl" || controller.name == "rFootControl")
                {
                    // 获取保持角度最大力的参数
                    var holdRotationMaxForce = controller.GetFloatJSONParam("holdRotationMaxForce");
                    // 获取关节驱动X目标的参数
                    var jointDriveXTarget = controller.GetFloatJSONParam("jointDriveXTarget");

                    // 如果开启高跟
                    if (EnableHeel)
                    {
                        // 设定参数
                        holdRotationMaxForce.val = this._HoldRotationMaxForceAdjust.val;
                        jointDriveXTarget.val = this._FootJointDriveXTargetAdjust.val;
                    }
                    else
                    {
                        // 设置为默认值
                        holdRotationMaxForce.val = holdRotationMaxForce.defaultVal;
                        jointDriveXTarget.val = jointDriveXTarget.defaultVal;
                    }
                }
                // 设定角度和位置状态为开启
                controller.currentRotationState = config.MotionRotationState;
                controller.currentPositionState = config.MotionPositionState;
                if (controller.followWhenOff != null)
                {
                    this.controllerLookup.Add(controller.followWhenOff, controller);
                }
            }
        }

        /// <summary>
        /// 重置控制器状态
        /// </summary>
        internal void ResetControlState()
        {
            // 如果关闭下半身
            if (this._CloseLowerBonesJSON.val)
            {
                foreach (var item in this.controllerLookup)
                {
                    var controller = item.Value;

                    if (!lowerControls.Any(c => controller.name.EndsWith($"{c}Control")))
                    {
                        SetControlState(controller);
                    }
                }
            }
            else
            {
                foreach (var item in this.controllerLookup)
                {
                    var controller = item.Value;
                    SetControlState(controller);
                }
            }
        }

        /// <summary>
        /// 设置控制器状态
        /// </summary>
        /// <param name="controller"></param>
        private void SetControlState(FreeControllerV3 controller)
        {
            // 如果允许高跟并且当前控制器是脚趾控制器
            if (EnableHeel && (controller.name == "lToeControl" || controller.name == "rToeControl"))
            {
                // 关闭脚趾控制器的位置和角度状态
                controller.currentRotationState = FreeControllerV3.RotationState.Off;
                controller.currentPositionState = FreeControllerV3.PositionState.Off;
            }
            else
            {
                // 设定角度和位置状态为开启
                controller.currentRotationState = config.MotionRotationState;
                controller.currentPositionState = config.MotionPositionState;
            }
        }

        /// <summary>
        /// 初始加载
        /// </summary>
        private void CoLoad()
        {
            this.Prepare();
            GameObject gameObject = MmdGameObject.CreateGameObject(Utility.GameObjectHead + "MmdGameObject");
            gameObject.transform.localScale = new Vector3(0.1f, 0.1f, 0.1f);
            this._MmdPersonGameObject = gameObject.GetComponent<MmdGameObject>();
            GameObject newRoot = new GameObject(Utility.GameObjectHead + "MmdRoot");
            gameObject.transform.parent = newRoot.transform;
            this.rootHandler = newRoot.transform;

            newRoot.transform.position = this._PersonAtom.mainController.transform.position;
            newRoot.transform.rotation = this._PersonAtom.mainController.transform.rotation;

            GameObject temp = new GameObject(Utility.GameObjectHead + "temp");
            temp.transform.position = this._PersonAtom.transform.position;
            //temp.transform.localEulerAngles = new Vector3(0f, 180f, 0f);
            temp.transform.rotation = this._PersonAtom.transform.rotation * Quaternion.Euler(0, 180, 0);

            Transform parent2 = this._PersonAtom.transform.Find("rescale2/PhysicsModel");
            if (parent2 == null)
            {
                parent2 = this._PersonAtom.transform.Find("rescale2/MoveWhenInactive/PhysicsModel");
            }
            
            _MmdPersonGameObject.m_MatchBone = model =>
            {
                DazBoneMapping.MatchTarget(_PersonAtom, _MmdPersonGameObject, parent2);
            };
            this._MmdPersonGameObject.LoadModel();
            this._MmdPersonGameObject.transform.localEulerAngles = new Vector3(0f, 180f, 0f);

            cachedBoneLookup.Clear();
            foreach (var item in _MmdPersonGameObject._model.Bones)
            {
                string name = item.Name;
                string boneName = name;
                if (DazBoneMapping.boneNames.ContainsKey(name))
                    boneName = DazBoneMapping.boneNames[name];
                if (!boneName.Contains("|"))
                {
                    var tf = DazBoneMapping.SearchObjName(parent2, boneName);
                    if (cachedBoneLookup == null)
                        cachedBoneLookup = new Dictionary<string, Transform>();
                    cachedBoneLookup[name] = tf;
                }
            }
            listFingerGameObject.Clear();
            foreach (var mmdbone in _MmdPersonGameObject._bones)
            {
                if (DazBoneMapping.fingerBoneNames.Contains(mmdbone.name))
                {
                    listFingerGameObject.Add(mmdbone);
                    continue;
                }
            }

            this._MmdPersonGameObject.OnUpdate = UpdateMotion;
        }

        /// <summary>
        /// 设置指定控制器的最大保持力
        /// </summary>
        /// <param name="controller"></param>
        /// <param name="force"></param>
        void SetHoldMaxForce(FreeControllerV3 controller, float? force)
        {
            var position = controller.GetFloatJSONParam("holdPositionMaxForce");
            var rotation = controller.GetFloatJSONParam("holdRotationMaxForce");

            if (force.HasValue)
            {
                position.SetVal(force.Value);
                rotation.SetVal(force.Value);
            }
            else
            {
                position.SetValToDefault();
                rotation.SetValToDefault();
            }
        }

        int outTimes = 0;

        bool showDebug = false;

        /// <summary>
        /// 有效骨骼字典
        /// </summary>
        Dictionary<string, string> validBoneNames = new Dictionary<string, string>();

        /// <summary>
        /// 更新动作
        /// </summary>
        /// <param name="mmd"></param>
        private void UpdateMotion(MmdGameObject mmd)
        {
            try
            {
                if (mmd._motion == null)
                {
                    return;
                }

                SetLookMode();

                // 骨骼数组
                GameObject[] bones = mmd._bones.Where(b => validBoneNames.ContainsKey(b.name)).ToArray();

                // 计算地板高度
                var floorHeight = config.AutoCorrectFloorHeight + _PositionY.val;

                // 修正高度
                var horizon = Math.Max(config.AutoCorrectFixHeight, floorHeight);

                #region 修正骨骼位置，如果骨骼位置高度小于0，则对其进行修正
                var reviseY = GetFixHeight(bones, floorHeight, horizon);
                #endregion

                foreach (GameObject mmdbone in bones)
                {
                    string bonename = mmdbone.name;

                    // 如果选中关闭下半身，并且骨骼在下半身字典中，则不更新骨骼动作
                    if (this._CloseLowerBonesJSON.val && lowerBones.ContainsKey(bonename))
                    {
                        continue;
                    }

                    Transform boneTransform = cachedBoneLookup[bonename];
                    if (boneTransform != null)
                    {
                        if (this.controllerLookup.ContainsKey(boneTransform))
                        {
                            Quaternion rotation = mmdbone.transform.rotation;
                            Vector3 position = mmdbone.transform.position;
                            var freeControllerV = this.controllerLookup[boneTransform];

                            // 如果开启了高跟，跳过脚趾的更新
                            if (EnableHeel && freeControllerV.name.EndsWith("ToeControl"))
                            {
                                continue;
                            }

                            // 只有高度修正模式不为None时，才会启用修正功能
                            if (config.AutoFixHeightMode != AutoCorrectHeightMode.None || config.EnableKneeingCorrections || EnableHeel)
                            {
                                // 跪姿优化
                                if (kneeFixed && (freeControllerV.name.EndsWith("FootControl") || freeControllerV.name.EndsWith("ToeControl")))
                                {
                                    // 左膝只处理左侧的脚和脚趾，右膝只处理右侧的脚和脚趾
                                    if ((lKneeFixed && freeControllerV.name.StartsWith("l"))
                                        || (rKneeFixed && freeControllerV.name.StartsWith("r")))
                                    {
                                        if (freeControllerV.name.EndsWith("FootControl"))
                                        {
                                            freeControllerV.GetFloatJSONParam("jointDriveXTarget").val = -45;
                                        }

                                        //freeControllerV.transform.SetPositionAndRotation(position, rotation * Utility.quat);
                                        freeControllerV.transform.rotation = rotation * Utility.quat;
                                        freeControllerV.currentPositionState = FreeControllerV3.PositionState.Off;
                                        freeControllerV.currentRotationState = FreeControllerV3.RotationState.Off;

                                        continue;
                                    }
                                }

                                if (config.AutoFixHeightMode == AutoCorrectHeightMode.WholeBody || EnableHeel)
                                {
                                    // 如果是手部，则修正高度未地板高度+0.02，其他部位按照整体计算值进行修正
                                    if ((freeControllerV.name.EndsWith("HandControl") || freeControllerV.name.EndsWith("KneeControl"))
                                        && position.y < floorHeight)
                                    {
                                        position.y = floorHeight + 0.02f;
                                    }
                                    else
                                    {
                                        #region 修正位置的y参数
                                        if (reviseY != 0)
                                        {
                                            position.y += reviseY;
                                        }
                                        #endregion
                                    }
                                }
                                else if (config.AutoFixHeightMode == AutoCorrectHeightMode.PartOnly)
                                {
                                    if (position.y < horizon)
                                    {
                                        position.y = floorHeight;
                                    }
                                }

                                // 处理脚部控制
                                if (config.EnableFootFree)
                                {
                                    #region 处理某些脚部被陷入地板的情况
                                    if (freeControllerV.transform.position.y < 0)
                                    {
                                        if (freeControllerV.name.EndsWith("FootControl") || freeControllerV.name.EndsWith("ToeControl"))
                                        {
                                            outTimes++;

                                            if (outTimes > 120)
                                            {
                                                outTimes = 0;
                                            }
                                        }
                                    }
                                    #endregion
                                }

                                if (!EnableHeel)
                                {
                                    // 如果脚趾关闭了，跳过位置和角度更新
                                    if (FreeToe(freeControllerV, position, right: false) || FreeToe(freeControllerV, position, right: true))
                                        continue;
                                }

                                // 如果脚部关闭了，跳过位置和角度更新
                                if (FreeFoot(freeControllerV, position, right: false) || FreeFoot(freeControllerV, position, right: true))
                                    continue;
                            }

                            if (DazBoneMapping.armBones.Contains(bonename))
                            {
                                if (bonename.StartsWith("r") || bonename.StartsWith("右"))
                                {
                                    freeControllerV.transform.SetPositionAndRotation(position, rotation * Quaternion.Euler(new Vector3(0f, 0f, 36f)) * Utility.quat);
                                }
                                else
                                {
                                    freeControllerV.transform.SetPositionAndRotation(position, rotation * Quaternion.Euler(new Vector3(0f, 0f, -36f)) * Utility.quat);
                                }
                            }
                            else
                            {
                                freeControllerV.transform.SetPositionAndRotation(position, rotation * Utility.quat);
                            }
                        }

                    }

                    foreach (GameObject item in listFingerGameObject)
                    {
                        this.UpdateFinger(item);
                    }

                    float time = GetRelativeTime();
                    if (this._EnableFaceJSON.val)
                    {
                        var morphs = mmd.GetUpdatedMorph(time);
                        foreach (var item in morphs)
                        {
                            if (this._FaceMorphs.ContainsKey(item.Key))
                            {
                                this._FaceMorphs[item.Key].morphValue = item.Value;
                            }
                        }
                    }
                }
            }
            catch (Exception e)
            {
                LogUtil.LogError(e, $"MotionHelper::UpdateMotion:{_MotionSetting.ToString()}");
            }
        }
        /// <summary>
        /// 更新手指
        /// </summary>
        /// <param name="item"></param>
        private void UpdateFinger(GameObject item)
        {
            string pmxBoneName = item.name;
            if (!cachedBoneLookup.ContainsKey(pmxBoneName))
            {
                return;
            }

            var rotation = item.transform.rotation;
            Transform transform = cachedBoneLookup[pmxBoneName];
            Quaternion worldRotation = item.transform.rotation * Utility.quat;
            Quaternion parentRotation = item.transform.parent.rotation;
            var parentWorldRotation = parentRotation;
            if (pmxBoneName.StartsWith("r") || pmxBoneName.StartsWith("右"))
            {
                worldRotation = rotation * Quaternion.Euler(new Vector3(0f, 0f, 36f)) * Utility.quat;
                parentWorldRotation = parentRotation * Quaternion.Euler(new Vector3(0f, 0f, 36f)) * Utility.quat;
            }
            else
            {
                worldRotation = rotation * Quaternion.Euler(new Vector3(0f, 0f, -36f)) * Utility.quat;
                parentWorldRotation = parentRotation * Quaternion.Euler(new Vector3(0f, 0f, -36f)) * Utility.quat;
            }
            var localRotation = Quaternion.Inverse(parentWorldRotation) * worldRotation;
            var dazBone = transform.GetComponent<DAZBone>();
            var joint = transform.GetComponent<ConfigurableJoint>();
            joint.SetTargetRotationLocal(localRotation, Quaternion.identity);
            var fingerOutput = dazBone.GetComponent<FingerOutput>();
            fingerOutput.ConvertRotation(dazBone, joint);
            fingerOutput.UpdateOutput();
        }

        /// <summary>
        /// 更新高跟设置
        /// </summary>
        internal void UpdateEnableHighHeel()
        {
            foreach (FreeControllerV3 freeControllerV in this._PersonAtom.freeControllers)
            {
                if (freeControllerV.name == "lToeControl" || freeControllerV.name == "rToeControl")
                {
                    var toeJointDriveXTarget = freeControllerV.GetFloatJSONParam("jointDriveXTarget");
                    if (EnableHeel)
                    {
                        freeControllerV.currentRotationState = FreeControllerV3.RotationState.Off;
                        freeControllerV.currentPositionState = FreeControllerV3.PositionState.Off;
                        toeJointDriveXTarget.val = this._ToeJointDriveXTargetAdjust.val;
                    }
                    else
                    {
                        freeControllerV.currentRotationState = FreeControllerV3.RotationState.On;
                        freeControllerV.currentPositionState = FreeControllerV3.PositionState.On;
                        freeControllerV.transform.localPosition = freeControllerV.startingLocalPosition;
                        freeControllerV.transform.localRotation = freeControllerV.startingLocalRotation;
                        toeJointDriveXTarget.val = toeJointDriveXTarget.defaultVal;
                    }
                }
                else if (freeControllerV.name == "lFootControl" || freeControllerV.name == "rFootControl")
                {
                    var footHoldRotationMaxForce = freeControllerV.GetFloatJSONParam("holdRotationMaxForce");
                    var footJointDriveXTarget = freeControllerV.GetFloatJSONParam("jointDriveXTarget");
                    if (EnableHeel)
                    {
                        footHoldRotationMaxForce.val = this._HoldRotationMaxForceAdjust.val;
                        footJointDriveXTarget.val = this._FootJointDriveXTargetAdjust.val;
                    }
                    else
                    {
                        footHoldRotationMaxForce.val = footHoldRotationMaxForce.defaultVal;
                        footJointDriveXTarget.val = footJointDriveXTarget.defaultVal;
                    }
                }
            }
        }

        /// <summary>
        /// 更新高跟关节驱动X角度
        /// </summary>
        private void UpateHeelJointDriveXAngle()
        {
            if (!EnableHeel)
            {
                return;
            }

            foreach (FreeControllerV3 freeControllerV in this._PersonAtom.freeControllers)
            {
                if (freeControllerV.name == "lFootControl" || freeControllerV.name == "rFootControl")
                {
                    var footHoldRotationMaxForce = freeControllerV.GetFloatJSONParam("holdRotationMaxForce");
                    var footJointDriveXTarget = freeControllerV.GetFloatJSONParam("jointDriveXTarget");
                    footHoldRotationMaxForce.val = this._HoldRotationMaxForceAdjust.val;
                    footJointDriveXTarget.val = this._FootJointDriveXTargetAdjust.val;
                }
                else if (freeControllerV.name == "lToeControl" || freeControllerV.name == "rToeControl")
                {
                    var toeJointDriveXTarget = freeControllerV.GetFloatJSONParam("jointDriveXTarget");
                    toeJointDriveXTarget.val = this._ToeJointDriveXTargetAdjust.val;
                }
            }
        }

        /// <summary>
        /// 获取相对时间
        /// </summary>
        /// <returns></returns>
        public float GetRelativeTime()
        {
            float value = _ProgressJSON.val;//this.Progress - this.timeDelay.val;
            float min = 0f;
            float max = this.MaxTime;
            return Mathf.Clamp(value, min, max);
        }

        public void OnDisable()
        {
            RestoreEyeBehavior();
        }

        public void OnEnable()
        {

        }

        public void OnDestroy()
        {
            Reset();

            RestoreEyeBehavior();

            if (PhysicsMeshs != null)
            {
                foreach (var item in PhysicsMeshs)
                {
                    var mesh = item.Value;

                    SetPhysicsMesh(mesh, true);
                }

                this.PhysicsMeshs?.Clear();
                this.PhysicsMeshs = null;
            }

            this.controllerLookup?.Clear();
            this.controllerLookup = null;
            this.controllerNameLookup?.Clear();
            this.controllerNameLookup = null;

            this._FaceMorphs?.Clear();
            this._FaceMorphs = null;

            //this._MmdAssetGameObject = null;
            this._MmdPersonGameObject = null;
            this._PersonAtom = null;
            this._MotionSetting = null;
        }
    }
}
