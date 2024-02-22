using mmd2timeline.Store;
using SimpleJSON;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static UnityEngine.EventSystems.EventTrigger;

namespace mmd2timeline
{
    internal partial class Player : BaseScript
    {
        // 指示是否在播放中
        internal bool IsPlaying
        {
            get
            {
                return _ProgressHelper.IsPlaying;
            }
        }

        /// <summary>
        /// 指示是否播放到末尾
        /// </summary>
        /// <remarks>此属性用于单次播放模式，播放完毕后用于判定再次播放时是否重置进度条到开头</remarks>
        bool isEnd = false;

        /// <summary>
        /// 是否是编辑模式
        /// </summary>
        bool isEditing = false;

        #region 工具类实例
        /// <summary>
        /// 进度条助手
        /// </summary>
        ProgressHelper _ProgressHelper;
        /// <summary>
        /// 音频播放助手
        /// </summary>
        AudioPlayHelper _AudioPlayHelper;
        /// <summary>
        /// 人物动作助手组
        /// </summary>
        MotionHelperGroup _MotionHelperGroup;
        /// <summary>
        /// 镜头动作助手
        /// </summary>
        CameraHelper _CameraHelper;
        /// <summary>
        /// MMD文件夹助手
        /// </summary>
        MMDFolderHelper _MMDFolderHelper;
        #endregion

        /// <summary>
        /// 状态检查时间。
        /// </summary>
        float _CheckTime = 0f;

        /// <summary>
        /// 检查动画暂停时间
        /// </summary>
        float _CheckFreezeTime = 0f;

        /// <summary>
        /// 最大状态检查时间。超过此时间没有响应，则跳到下一个
        /// </summary>
        const float MAX_CHECK_TIME = 1f;

        /// <summary>
        /// 指示动画是否正在加载中
        /// </summary>
        bool _IsLoading = false;

        /// <summary>
        /// 创建人物原子列表变量
        /// </summary>
        List<Atom> _PersonAtoms;
        /// <summary>
        /// 获取人物原子列表
        /// </summary>
        public List<Atom> PersonAtoms
        {
            get
            {
                if (_PersonAtoms == null)
                {
                    _PersonAtoms = new List<Atom>();
                }

                return _PersonAtoms;
            }
        }

        /// <summary>
        /// 当前播放的MMD动画
        /// </summary>
        MMDEntity _CurrentItem = null;
        /// <summary>
        /// 获取或设置当前播放项目
        /// </summary>
        internal MMDEntity CurrentItem
        {
            get { return _CurrentItem; }
            set
            {
                if (_CurrentItem != value)
                {
                    _CurrentItem = value;
                }
            }
        }

        /// <summary>
        /// 指示播放器是否已经锁定
        /// </summary>
        internal bool IsLocked = false;

        public override bool ShouldIgnore()
        {
            return false;
        }

        /// <summary>
        /// 设置对外开放的方法
        /// </summary>
        void SetOpenActions()
        {
            RegisterAction(new JSONStorableAction("Play/Pause", () => this.TogglePlaying()));
            RegisterAction(new JSONStorableAction("Play", () => this.StartPlaying()));
            RegisterAction(new JSONStorableAction("Stop", () => this.StopPlaying()));
            RegisterAction(new JSONStorableAction("Prev", () => this.Prev()));
            RegisterAction(new JSONStorableAction("Next", () => this.Next()));
            RegisterAction(new JSONStorableAction("Import From Folder", () => this._MMDFolderHelper.ImportFromFolder()));
            RegisterAction(new JSONStorableAction("Load From Folder", () => this._MMDFolderHelper.LoadFolder()));
            RegisterAction(new JSONStorableAction("Load File", () => this._MMDFolderHelper.LoadFile()));

            RegisterAction(new JSONStorableAction("Clear All", () => StopPlayAndRun(() => this.StartCoroutine(this.Playlist.ClearList()))));
            RegisterAction(new JSONStorableAction("Remove Current", () => this.Playlist.RemovePlayItem()));

            RegisterAction(new JSONStorableAction("Import From File", () => this.Playlist.BeginImport()));
            RegisterAction(new JSONStorableAction("Export Playlist", () => this.Playlist.BeginExport()));

            RegisterAction(new JSONStorableAction("Load All", () => this.LoadAll()));
            RegisterAction(new JSONStorableAction("Load Favorite", () => this.LoadFavorite()));

            //RegisterAction(new JSONStorableAction("Favorite", () => this.CurrentItem?.Favorite()));
            RegisterAction(new JSONStorableAction("Toggle Favorite", () => this.ToggleFavorite()));

            RegisterAction(new JSONStorableAction("Toggle Play Mode", () => this.TogglePlayMode()));

            RegisterAction(new JSONStorableAction("Reset Person Motion", () => StartCoroutine(ResetAllPersonMotion())));

            _loadPlaylistFile = new JSONStorableString($"Load Playlist File", null);
            _loadPlaylistFile.setCallbackFunction = v =>
            {
                this.Playlist.LoadPlayListPreset(v);
            };

            RegisterString(_loadPlaylistFile);
        }

        JSONStorableString _loadPlaylistFile;

        #region 各种事件处理函数

        /// <summary>
        /// 有Atom被添加
        /// </summary>
        bool hasAtomAdded = false;

        /// <summary>
        /// 原子添加后的事件
        /// </summary>
        /// <param name="atom"></param>
        void OnAtomAdded(Atom atom)
        {
            CheckAtomAdded(atom);

            // 刷新聚焦原子列表
            RefreshFocusAtomList();
        }

        /// <summary>
        /// 原子被添加的接收方法
        /// </summary>
        /// <param name="atom"></param>
        void CheckAtomAdded(Atom atom)
        {
            if (atom.type == "Person")
            {
                // 正在加载中，不进行处理
                if (SuperController.singleton.isLoading)
                {
                    hasAtomAdded = true;
                    return;
                }

                // 如果pmx路径为空则为其赋值
                if (string.IsNullOrEmpty(Config.varPmxPath))
                {
                    Config.varPmxPath = PluginPath + "/g2f.pmx";
                }

                var index = PersonAtoms.IndexOf(atom);
                if (index < 0)
                {
                    PersonAtoms.Add(atom);
                }
                if (CurrentItem != null)
                {
                    StartCoroutine(InitPersonAtomMotionHelper(atom, CurrentItem.GetFileData()));
                }
                else
                {
                    StartCoroutine(InitPersonAtomMotionHelper(atom));
                }
            }
        }
        /// <summary>
        /// 原子被移除的接收方法
        /// </summary>
        /// <param name="atom"></param>
        void OnAtomRemoved(Atom atom)
        {
            // 移除人物
            RemoveMotionHelper(atom);

            // 刷新聚焦原子列表
            RefreshFocusAtomList();
        }

        /// <summary>
        /// 刷新聚焦原子列表
        /// </summary>
        void RefreshFocusAtomList()
        {
            // 刷新镜头聚焦原子列表
            _CameraHelper.RefreshFocusAtomList();

            Atom targetPersonAtom;

            if (containingAtom.type == "Person")
            {
                targetPersonAtom = containingAtom;
            }
            else
            {
                targetPersonAtom = SuperController.singleton.GetAtoms().FirstOrDefault(a => a.type == "Person");
            }

            if (targetPersonAtom != null)
            {
                _CameraHelper.SetFocusTarget(targetPersonAtom.uid, "neckControl");
            }
        }

        /// <summary>
        /// 同步进度条
        /// </summary>
        /// <param name="progress"></param>
        /// <param name="hardUpdate">强制更新音频进度</param>
        private void SyncProgress(float progress, bool hardUpdate)
        {
            _triggerHelper.Trigger(TriggerEventHelper.TRIGGER_PROGRESS_CHANGE, progress, 0f, _ProgressHelper.MaxTime);

            // 设置镜头播放进度
            SetMMDCameraProgress(progress);

            // 设置音频播放进度
            if (config.SyncMode != ProgressSyncMode.SyncWithAudio // 同步进度不依据音频
                || config.PlaySpeed != 1f // 速度不为1的情况下要同步音频进度
                || hardUpdate // 明确设定硬更新时同步音频进度
                || _AudioPlayHelper.IsDelay // 启用了音频延迟时需要同步进度
                || (_AudioPlayHelper.HasAudio && _ProgressHelper.IsPlaying && !_AudioPlayHelper.IsPlaying))// 有音频 但是没有在播放时
            {
                _AudioPlayHelper.SetProgress(progress, _ProgressHelper.IsPlaying);
            }

            // 同步人物的动作播放进度
            _MotionHelperGroup.SyncMotionProgress(progress);
        }

        /// <summary>
        /// 播放状态更改的事件处理方法
        /// </summary>
        /// <param name="progress"></param>
        /// <param name="playing"></param>
        void OnPlayStatusChanged(float progress, bool playing)
        {
            if (playing)
            {
                // 有镜头数据时，才会隐藏主UI
                if (_CameraHelper.HasMotion)
                {
                    // 隐藏主HUD
                    SuperController.singleton.HideMainHUD();
                }
                else
                {
                    //_triggerHelper.Trigger(TRIGGER_CAMERA_DEACTIVATED);
                }

                _AudioPlayHelper.SetProgress(progress, true);

                //// 播放时重发一下镜头状态
                //if (_CameraHelper.IsActive)
                //{
                //    _triggerHelper.Trigger(TRIGGER_CAMERA_ACTIVATED);
                //}
                //else
                //{
                //    _triggerHelper.Trigger(TRIGGER_CAMERA_DEACTIVATED);
                //}
            }
            else
            {
                _AudioPlayHelper.Stop(1);
                //_triggerHelper.Trigger(TRIGGER_CAMERA_DEACTIVATED);
            }
            SetPlayButton();
        }

        /// <summary>
        /// 进度条结束的事件处理方法
        /// </summary>
        /// <param name="isEnd"></param>
        void OnProgressEnded(bool isEnd)
        {
            //_triggerHelper.Trigger(TriggerEventHelper.TRIGGER_IS_END);

            if (Playlist.PlayMode == MMDPlayMode.Once)
            {
                this.isEnd = true;
                this.StopPlaying();
            }
            else
            {
                // 等待3秒后下一个
                //StartCoroutine(WaitForSecondsRealtime(3f, Next));

                Next();
            }
        }

        /// <summary>
        /// 单个MMD选中的处理
        /// </summary>
        /// <param name="entity"></param>
        void OnMMDSelected(MMDEntity entity)
        {
            if (entity != null)
            {
                InEditMode();

                LoadMMD(entity, true);
            }
            else
            {
                LoadMMD(this.Playlist.CurrentMMD);
            }
        }

        /// <summary>
        /// 多个MMD导入时执行的方法
        /// </summary>
        /// <param name="path"></param>
        /// <param name="step"></param>
        /// <param name="total"></param>
        void OnMMDImported(MMDEntity entity, string info, int step, int total)
        {
            if (entity != null)
            {
                // 添加播放列表但不触发事件通知
                Playlist.AddPlayItem(entity, notify: false);
            }

            // 显示导入进度
            ShowImportProgress(info, step, total);

            if (step == total)
            {
                // 导入完成后，清理播放列表
                Playlist.ClearPlayList();

                OnPlaylistUpdated(Playlist, this.Playlist.GetPlayList());
            }
        }
        #endregion

        /// <summary>
        /// 初始化设置
        /// </summary>
        protected void InitSettings()
        {
            // 初始化HUD UI
            InitHUDUI();

            // 设置开放方法
            SetOpenActions();

            // 清空人物列表
            PersonAtoms.Clear();

            // 初始化动作助手组实例
            _MotionHelperGroup = MotionHelperGroup.GetInstance();

            // 初始化进度条助手
            _ProgressHelper = ProgressHelper.GetInstance();
            _ProgressHelper.OnProgressChanged += SyncProgress;
            _ProgressHelper.OnProgressEnded += OnProgressEnded;
            _ProgressHelper.OnPlayStatusChanged += OnPlayStatusChanged;

            // 初始化镜头助手
            _CameraHelper = CameraHelper.GetInstance();
            _CameraHelper.OnCameraMotionLoaded += OnCameraLoaded;
            _CameraHelper.OnCameraActivateStatusChanged += OnCameraActivateStatusChanged;

            //--音频播放助手--------------------------------------------------------------
            _AudioPlayHelper = AudioPlayHelper.GetInstance();
            _AudioPlayHelper.OnAudioLoaded += OnAudioLoaded;
            _AudioPlayHelper.SetVolume(config.Volume);

            //--MMD文件管理器--------------------------------------------------------------
            _MMDFolderHelper = MMDFolderHelper.GetInstance();
            _MMDFolderHelper.OnMMDSelected += OnMMDSelected;
            _MMDFolderHelper.OnMMDImported += OnMMDImported;

            //// 增加原子添加移除的事件处理
            //SuperController.singleton.onAtomAddedHandlers += CheckAtomAdded;
            //SuperController.singleton.onAtomRemovedHandlers += CheckAtomRemoved;

            // 创建UI
            CreateUI();

            //RefreshPersonAtoms();
        }

        /// <summary>
        /// 镜头激活状态更改事件处理函数
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="activate"></param>
        private void OnCameraActivateStatusChanged(CameraHelper sender, bool activate)
        {
            _cameraActiveJSON.val = activate;
        }

        #region Save/Load
        /// <summary>
        /// 获取保存的JSON数据
        /// </summary>
        /// <param name="includePhysical"></param>
        /// <param name="includeAppearance"></param>
        /// <param name="forceStore"></param>
        /// <returns></returns>
        public override JSONClass GetJSON(bool includePhysical = true, bool includeAppearance = true, bool forceStore = false)
        {
            var json = base.GetJSON(includePhysical, includeAppearance, forceStore);
            try
            {
                if (includePhysical || forceStore)
                {
                    needsStore = true;

                    json = _triggerHelper.GetJSON(json);

                    //// 生成所有触发器的JSON数据
                    //var allTriggers = _triggerHelper.GetAllTriggers();
                    //foreach (var et in allTriggers)
                    //{
                    //    json[et.Name] = et.GetJSON(base.subScenePrefix);
                    //}
                }
            }
            catch (Exception exc)
            {
                LogUtil.LogError(exc);
            }
            return json;
        }

        /// <summary>
        /// 从JSON数据恢复
        /// </summary>
        /// <param name="jc"></param>
        /// <param name="restorePhysical"></param>
        /// <param name="restoreAppearance"></param>
        /// <param name="presetAtoms"></param>
        /// <param name="setMissingToDefault"></param>
        public override void RestoreFromJSON(JSONClass jc, bool restorePhysical = true, bool restoreAppearance = true, JSONArray presetAtoms = null, bool setMissingToDefault = true)
        {
            base.RestoreFromJSON(jc, restorePhysical, restoreAppearance, presetAtoms, setMissingToDefault);

            try
            {
                _triggerHelper.RestoreFromJSON(jc);
                //if (!base.physicalLocked && restorePhysical && !IsCustomPhysicalParamLocked("trigger"))
                //{
                //    //// 恢复所有触发器的数据
                //    //var allTriggers = _triggerHelper.GetAllTriggers();
                //    //foreach (var et in allTriggers)
                //    //{
                //    //    et.RestoreFromJSON(jc, base.subScenePrefix, base.mergeRestore, setMissingToDefault);
                //    //}
                //}
            }
            catch (Exception exc)
            {
                LogUtil.LogError(exc);
            }
        }

        public override void PostRestore(bool restorePhysical, bool restoreAppearance)
        {
            base.PostRestore(restorePhysical, restoreAppearance);
        }

        #endregion

        #region 音频、镜头、动作加载完成的事件处理函数
        /// <summary>
        /// 音频加载完毕调用的函数
        /// </summary>
        /// <param name="length"></param>
        void OnAudioLoaded(float length)
        {
            _ProgressHelper.MaxTime = length;
        }

        /// <summary>
        /// 镜头加载完毕调用的函数
        /// </summary>
        /// <param name="length"></param>
        void OnCameraLoaded(float length)
        {
            _ProgressHelper.MaxTime = length;
        }

        /// <summary>
        /// 动作加载完毕调用的函数
        /// </summary>
        /// <param name="maxtime"></param>
        void OnMotionLoaded(MotionHelper sender, float length)
        {
            if (sender.PersonAtom && !sender.PersonAtom.collisionEnabled)
            {
                sender.PersonAtom.collisionEnabled = true;
            }

            _ProgressHelper.MaxTime = length;
        }
        #endregion

        #region 配置相关的方法

        /// <summary>
        /// 当前项目数据内容改变的事件
        /// </summary>
        /// <param name="obj"></param>
        /// <exception cref="NotImplementedException"></exception>
        private void UpdateFavoriteLabel(MMDEntity obj)
        {
            if (obj.InFavorite)
            {
                _triggerHelper.Trigger(TriggerEventHelper.TRIGGER_FAVORITED);
            }
            else
            {
                _triggerHelper.Trigger(TriggerEventHelper.TRIGGER_UNFAVORITED);
            }
            _FavoriteLabel.text = obj.InFavorite ? Lang.Get("UnFavorite") : Lang.Get("Favorite");
        }

        /// <summary>
        /// 切换收藏或取消收藏
        /// </summary>
        private void ToggleFavorite()
        {
            try
            {
                if (CurrentItem.InFavorite)
                {
                    CurrentItem.UnFavorite();
                }
                else
                {
                    CurrentItem.Favorite();
                }

                UpdateFavoriteLabel(CurrentItem);
            }
            catch (Exception ex)
            {
                LogUtil.LogError(ex, "Player:FavoriteOrUnFavorite::");
            }
        }

        /// <summary>
        /// 保存设置
        /// </summary>
        /// <param name="item"></param>
        internal IEnumerator SaveSettings(MMDEntity item)
        {
            if (item != null)
            {
                // 更新动作设定数据
                _MotionHelperGroup.UpdateValuesToSettings();

                // 克隆一下
                item = item.Clone();

                Playlist.AddPlayItem(item);

                item.Save();

                yield return null;

                if (isEditing)
                {
                    OutEditMode();
                }
                //LoadMMD(item);
            }
            yield break;
        }

        /// <summary>
        /// 清空播放列表
        /// </summary>
        void ClearPlayList()
        {
            try
            {
                // 停止播放
                StopPlaying();
                CurrentItem = null;

                ClearMotions();

                _PlaylistChooser.displayChoices = noneStrings;
                _PlaylistChooser.choices = noneStrings;
                _PlaylistChooser.valNoCallback = noneString;

                ShowInitLoadUI();
            }
            catch (Exception ex)
            {
                LogUtil.Debug(ex, $"ClearList Error");
            }
        }

        /// <summary>
        /// 清空选中的元素
        /// </summary>
        private void ClearMotions()
        {
            _AudioPlayHelper.Clear();
            _CameraHelper.Clear();

            _MotionHelperGroup.ResetMotions();
            _ProgressHelper.Clear();
        }

        #endregion

        #region 播放相关的方法

        /// <summary>
        /// 是否已经完成初始化
        /// </summary>
        bool isInited = false;

        public override void Init()
        {
            base.Init();

            //InitTriggers();
            InitStatusParams();
        }

        public void Start()
        {
            // 初始化模型目录
            //Config.varPmxPath = MacGruber.Utils.GetPluginPath(this) + "/g2f.pmx";

            base.InitScript();

            InitSettings();

            ShowInitLoadUI();

            HideHUDMessage();

            // 从默认播放列表加载内容
            StartCoroutine(StartDeferred());
        }

        /// <summary>
        /// 延迟执行
        /// </summary>
        /// <returns></returns>
        IEnumerator StartDeferred()
        {
            yield return this.Playlist.LoadFromDefalut();
            yield return null;

            // 设定有原子添加，等待处理
            hasAtomAdded = true;
            isInited = true;
            yield return null;
            //_triggerHelper.Trigger(TriggerEventHelper.TRIGGER_SCRIPT_LOADED);
        }

        #region 处理人物位置同步
        /// <summary>
        /// 指示动作是否正在重置
        /// </summary>
        bool isMotionResetting = false;
        float transformCheckTime = 0f;
        const float TRANSFORM_MAX_CHECK_TIME = 1f;
        public void FixedUpdate()
        {
            if (isMotionResetting) return;

            transformCheckTime += Time.fixedDeltaTime;
            if (transformCheckTime > TRANSFORM_MAX_CHECK_TIME)
            {
                transformCheckTime = 0;
                _MotionHelperGroup.UpdateTransform();
            }
        }
        #endregion

        int waitFrames = 0;
        const int MAX_WAIT_FRAMES = 60;

        public void Update()
        {
            // 如果场景正在加载中，则不执行后边的代码
            if (SuperController.singleton.isLoading) return;

            if (IsLocked || !isInited) return;

            #region 如果有人物原子被添加，则进行让人物刷新的处理（只有新加载场景时才会出现这种情况）
            if (hasAtomAdded)
            {
                if (waitFrames > MAX_WAIT_FRAMES)
                {
                    waitFrames = 0;

                    RefreshPersonAtoms();

                    hasAtomAdded = false;
                }
                else
                {
                    waitFrames++;
                }
                return;
            }
            #endregion

            if (_scriptLoadedJSON?.val == false)
            {
                _scriptLoadedJSON.val = true;
            }

            #region 快捷键控制

            // 只有在播放和编辑模式才会检查快捷键
            if (_currentUIMode == PlayerUIMode.Play || _currentUIMode == PlayerUIMode.Edit)
            {
                // 如果按了SPACE键则切换播放状态
                if (Input.GetKeyDown(KeyCode.Space) && config.EnableSpacePlay)
                {
                    TogglePlaying();
                    return;
                }
                // 左箭头后退1s
                if (Input.GetKeyUp(KeyCode.LeftArrow) && config.EnableLeftArrow)
                {
                    _ProgressHelper.Back();
                    return;
                }
                // 右箭头前进1s
                if (Input.GetKeyUp(KeyCode.RightArrow) && config.EnableRightArrow)
                {
                    _ProgressHelper.Forward();
                    return;
                }
                // 下箭头 下一个
                if (Input.GetKeyUp(KeyCode.UpArrow) && config.EnableUpArrow)
                {
                    this.Prev();
                    return;
                }
                // 下箭头 下一个
                if (Input.GetKeyUp(KeyCode.DownArrow) && config.EnableDownArrow)
                {
                    this.Next();
                    return;
                }
            }
            #endregion

            ShowDebugInfos();

            // 如果选中显示Debug信息
            if (_ShowDebugInfoJSON.val)
            {
                var playInfo = DateTime.Now.ToString();

                playInfo += $"\r\n" +
                    $"IsPlaying:{IsPlaying}" +
                    $"\r\n" +
                    $"_IsLoading:{_IsLoading}" +
                    $"\r\n" +
                    $"Playing:{_AudioPlayHelper.IsPlaying}" +
                    $"\r\n" +
                    $"Loading:{_AudioPlayHelper.IsLoading}" +
                    $"\r\n" +
                    //$"Waiting:{_AudioPlayHelper.IsWaitingPlay}" +
                    //$"\r\n" +
                    $"IsEnd:{_ProgressHelper.IsEnd}" +
                    $"\r\n" +
                    //$"IsPaused:{_AudioPlayHelper.IsPaused.val}" +
                    //$"\r\n" +
                    $"HasAudio:{_AudioPlayHelper.HasAudio}" +
                    $"\r\n" +
                    $"Clip:{_AudioPlayHelper.PlayingAudio?.displayName}" +
                    $"\r\n" +
                    $"CheckTime:{_CheckTime}" +
                    $"\r\n" +
                    $"MaxTime:{_AudioPlayHelper.MaxTime}" +
                    $"\r\n" +
                    $"Progress:{_ProgressHelper.Progress}" +
                    $"\r\n" +
                    $"AMaxTime:{_AudioPlayHelper.MaxTime}" +
                    $"\r\n" +
                    $"CMaxTime:{_CameraHelper.MaxTime}" +
                    $"\r\n";

                _DebugInfo.SetVal(playInfo);
            }

            // 如果冻结动画或不是活动和启用状态、动作重置中、播放速度为0，进行播放进度冻结后，直接返回
            if (SuperController.singleton.freezeAnimation || !SuperController.singleton.isActiveAndEnabled || isMotionResetting || !_MotionHelperGroup.AllHasAtomInited() || config.PlaySpeed <= 0f)
            {
                if (IsPlaying)
                {
                    _ProgressHelper.Freeze();
                }
                _CheckFreezeTime += Time.deltaTime;
                return;
            }

            // 如果有冻结时间，则说明已经暂停，此时取消暂停
            if (_CheckFreezeTime > 0f)
            {
                _ProgressHelper.FreezeRestore();
                _CheckFreezeTime = 0f;
                return;
            }

            // 如果音频还在加载中
            if (_AudioPlayHelper.IsLoading)
            {
                _CheckTime += Time.deltaTime;
                if (_CheckTime > MAX_CHECK_TIME)
                {
                    _CheckTime = 0f;
                    _AudioPlayHelper.CheckLoad();
                }
                return;
            }

            // 检查是否可以播放
            if (!IsPlaying || _ProgressHelper.IsEnd || _AudioPlayHelper.IsLoading || _IsLoading) return;

            try
            {
                // 如果有音频、在播放中，播放速度是1，按照音频进度更新
                if (config.SyncMode == ProgressSyncMode.SyncWithAudio &&
                !_AudioPlayHelper.IsDelay && _AudioPlayHelper.HasAudio && _AudioPlayHelper.IsPlaying && config.PlaySpeed == 1f)
                {
                    _ProgressHelper.SetProgress(_AudioPlayHelper.GetAudioTime(), false);
                }
                else
                {
                    _ProgressHelper.Update(config.PlaySpeed);
                    //if (!_AudioPlayHelper.IsDelay && _AudioPlayHelper.HasAudio && _AudioPlayHelper.IsPlaying)
                    //    _AudioPlayHelper.SetAudioTime(_ProgressHelper.Progress, false);
                }
            }
            catch (Exception ex)
            {
                LogUtil.Debug(ex, "Update:");
                this.StopPlaying();
            }
        }

        /// <summary>
        /// 显示调试信息
        /// </summary>
        /// <param name="msg"></param>
        void ShowDebugInfos(string msg = "")
        {
            if (config.showDebugInfo)
            {
                ShowHUDMessage($"Speed:{config.PlaySpeed}" +
                    $"\n" +
                    $"HasAudio:{_AudioPlayHelper.HasAudio}" +
                    $"\n" +
                    $"P.IsPlaying:{_ProgressHelper.IsPlaying}" +
                    $"\n" +
                    $"A.IsPlaying:{_AudioPlayHelper.IsPlaying}" +
                    $"\n" +
                    $"A.Progress:{_AudioPlayHelper.GetAudioTime().ToString("0.000")}" +
                    $"\n" +
                    $"lowestControlName:{_MotionHelperGroup.Helpers.FirstOrDefault()?.lowestControlName}" +
                    //$"\n" +
                    //$"AudioSource:{_AudioPlayHelper._AudioSource}" +
                    msg);
            }
        }

        /// <summary>
        /// 上一个
        /// </summary>
        void Prev()
        {
            if (!_MotionHelperGroup.AllIsReady())
                return;

            this.Playlist.Prev();
        }

        /// <summary>
        /// 下一个
        /// </summary>
        void Next()
        {
            if (!_MotionHelperGroup.AllIsReady())
                return;

            //_cameraActiveJSON.val = false;

            _triggerHelper.Trigger(TriggerEventHelper.TRIGGER_PLAY_NEXT);

            if (isEditing)
            {
                StartCoroutine(Repeat());
            }
            else
            {
                this.Playlist.Next();
            }
        }

        /// <summary>
        /// 设置镜头动作进度
        /// </summary>
        /// <param name="value"></param>
        private void SetMMDCameraProgress(float value)
        {
            try
            {
                if (config.EnableCamera && config.CameraActive)
                {
                    if (_CameraHelper.HasMotion)
                    {
                        _CameraHelper.SetProgress(value);
                        // 启用镜头跟随
                        AlignHeadToCamera();
                        return;
                    }
                    //else
                    //{
                    //    if (config.UseWindowCamera)
                    //    {
                    //        // 启用镜头跟随
                    //        AlignHeadToCamera();

                    //        return;
                    //    }
                    //}
                }

                _CameraHelper.IsActive = false;
            }
            catch (Exception ex)
            {
                LogUtil.Debug(ex);
            }
        }

        /// <summary>
        /// 重新播放
        /// </summary>
        private IEnumerator Repeat()
        {
            _ProgressHelper.Stop(6);
            yield return null;

            SetAllPersonOff();
            yield return null;

            _ProgressHelper.SetProgress(0.0f, true);
            //StopAndStartOver();
            yield return new WaitForSeconds(1);

            SetAllPersonOn();
            yield return null;

            _ProgressHelper.Play();

            _triggerHelper.Trigger(TriggerEventHelper.TRIGGER_START_PLAYING, CurrentItem.Title);

            yield break;
        }

        /// <summary>
        /// 重置人物动作
        /// </summary>
        /// <param name="atom"></param>
        private void ResetPose(Atom atom)
        {
            atom.ResetPhysics(true, true);

            AtomUI componentInChildren = atom.GetComponentInChildren<AtomUI>();
            if (componentInChildren != null)
            {
                componentInChildren?.resetButton?.onClick?.Invoke();
            }
        }

        /// <summary>
        /// 切换播放和暂停状态
        /// </summary>
        public bool TogglePlaying()
        {
            // 如果正在播放
            if (IsPlaying)
            {
                _ProgressHelper.Stop(3);
            }
            else // 如果未在播放
            {
                // 开始播放
                StartPlaying();
            }

            // 返回是否在播放状态
            bool onPlay = SetPlayButton();

            return onPlay;
        }

        /// <summary>
        /// 设置播放按钮
        /// </summary>
        /// <returns></returns>
        private bool SetPlayButton()
        {
            var onPlaying = IsPlaying;

            // 如果是播放状态，切换播放按钮的文字
            if (onPlaying)
            {
                var sPause = Lang.Get("Pause");

                if (_UIPlayButton.buttonText.text != sPause)
                {
                    _UIPlayButton.buttonText.text = sPause;

                    _playStatusJSON.val = true;

                    if (!config.CameraActive)
                    {
                        _CameraHelper.DisableNavigation(false);
                    }
                    else
                    {
                        _CameraHelper.DisableNavigation();
                    }
                }
            }
            else
            {
                var sPlay = Lang.Get("Play");
                if (_UIPlayButton.buttonText.text != sPlay)
                {
                    _playStatusJSON.val = false;

                    _UIPlayButton.buttonText.text = sPlay;
                    _CameraHelper.DisableNavigation(false);
                }
            }
            return onPlaying;
        }

        /// <summary>
        /// 停止播放
        /// </summary>
        public void StopPlaying()
        {
            _AudioPlayHelper.Stop(4);
            _ProgressHelper.Stop(2);
        }

        /// <summary>
        /// 停止播放并定位到开头
        /// </summary>
        public void StopAndStartOver()
        {
            StopPlaying();

            // 重置进度到开头
            _ProgressHelper.SetProgress(0f, true);
        }

        /// <summary>
        /// 开始播放
        /// </summary>
        public void StartPlaying()
        {
            // 如果正在播放 直接返回
            if (IsPlaying)
            {
                return;
            }

            // 如果到了末尾
            if (isEnd)
            {
                isEnd = false;

                // 只播放一次并且到了末尾，再次播放时更新进度条到开头
                if (Playlist.PlayMode == MMDPlayMode.Once)
                {
                    // 重置进度到开头
                    _ProgressHelper.SetProgress(0f, true);
                }
            }

            _ProgressHelper.Play();
        }

        /// <summary>
        /// 加载MMD
        /// </summary>
        /// <param name="mmdItem"></param>
        void LoadMMD(MMDEntity mmdItem)
        {
            if (mmdItem == null)
            {
                this.ClearPlayList();
            }
            else
            {
                this.LoadMMD(mmdItem, false);
            }
        }
        /// <summary>
        /// 加载MMD数据
        /// </summary>
        /// <param name="entity"></param>
        internal void LoadMMD(MMDEntity entity, bool test)
        {
            try
            {
                ClearMotions();

                if (entity != null)
                {
                    NotifyMMDSelected(entity);

                    if (!test)
                    {
                        this.ShowPlayUI();
                    }
                    else
                    {
                        this.ShowEditUI();
                    }
                }
            }
            catch (Exception e)
            {
                LogUtil.LogError(e, "Player::LoadMMD");
            }
        }

        /// <summary>
        /// 播放MMD数据
        /// </summary>
        /// <param name="entity"></param>
        internal IEnumerator PlayMMD(MMDEntity entity)
        {
            if (entity == null)
            {
                yield break;
            }

            OnCurrentItemChangedNoConfirm?.Invoke(entity, null);

            if (CurrentItem != entity)
            {
                CurrentItem = entity;

                UpdateFavoriteLabel(entity);
            }

            var fileData = entity.GetFileData();

            // 如果没有内容，停止播放
            if (fileData.DefaultCamera == noneString
                && fileData.DefaultAudio == noneString
                && fileData.DefaultMotion == noneString)
            {
                this.StopPlaying();
            }

            _ProgressHelper.InitSettings(entity.CurrentSetting);
            // 加载音频设置
            LoadAudioSettings(entity.AudioSetting, fileData.AudioPaths, fileData.AudioNames);

            // 加载镜头动作
            LoadCameraSettings(entity.CameraSetting, fileData.MotionPaths, fileData.MotionNames);

            //try
            //{
            foreach (var atom in PersonAtoms)
            {
                yield return InitPersonAtomMotionHelper(atom, fileData);

                SetPersonOff(atom);
            }
            //}
            //catch (Exception e)
            //{
            //    LogUtil.LogError(e, $"InitPersonAtomMotionHelper");
            //}

            // 播放前设置一下人物关节参数
            _MotionHelperGroup.SetPersonAllJoints();
            yield return null;//new WaitForSeconds(1);
            _ProgressHelper.Forward(0.001f);
            yield return new WaitForSeconds(1);

            foreach (var atom in PersonAtoms)
            {
                SetPersonOn(atom);
            }

            yield return null;//new WaitForSeconds(1);

            _IsLoading = false;

            _triggerHelper.Trigger(TriggerEventHelper.TRIGGER_START_PLAYING, CurrentItem.Title);
            if (_WaitingForPlay)
            {
                _WaitingForPlay = false;

                this.StartPlaying();
            }
            //yield return null;//new WaitForSeconds(1);
            //_cameraActiveJSON.val = false;
        }

        /// <summary>
        /// 初始化动作原子助手
        /// </summary>
        /// <param name="atom"></param>
        /// <returns></returns>
        IEnumerator InitPersonAtomMotionHelper(Atom atom)
        {
            atom.collisionEnabled = false;

            // 实例化MMD人物
            var motionHelper = InitMotionHelper(atom);

            if (!motionHelper.HasAtomInited)
            {
                yield return motionHelper.CoInitAtom();
            }

            yield return null;

            atom.collisionEnabled = true;
        }

        /// <summary>
        /// 初始化人物原子动作助手
        /// </summary>
        /// <param name="atom"></param>
        /// <param name="fileData"></param>
        IEnumerator InitPersonAtomMotionHelper(Atom atom, MMDEntity.FilesData fileData)
        {
            atom.collisionEnabled = false;

            // 实例化MMD人物
            var motionHelper = InitMotionHelper(atom);

            if (!motionHelper.HasAtomInited)
            {
                yield return motionHelper.CoInitAtom();
            }

            if (CurrentItem != null)
            {
                var index = PersonAtoms.IndexOf(atom);

                PersonMotion motion;

                // 如果没有足够的动作数据，则为后边的人物设定新的动作数据对象
                if (CurrentItem.Motions.Count <= index)
                {
                    motion = new PersonMotion();
                    CurrentItem.Motions.Add(motion);
                    motion.InitMotion(fileData);
                }
                else
                {
                    motion = CurrentItem.Motions[index];
                }

                // 设置人物动作
                motionHelper?.InitSettings(fileData.MotionPaths, fileData.MotionNames, motion);
            }

            yield return null;
            atom.collisionEnabled = true;
        }

        /// <summary>
        /// 加载音频设置
        /// </summary>
        /// <param name="settings"></param>
        /// <param name="choices"></param>
        /// <param name="displayChoices"></param>
        private void LoadAudioSettings(AudioSetting settings, List<string> choices, List<string> displayChoices)
        {
            _AudioPlayHelper.InitPlay(settings, choices, displayChoices);
        }

        /// <summary>
        /// 加载镜头设置
        /// </summary>
        /// <param name="settings"></param>
        /// <param name="choices"></param>
        /// <param name="displayChoices"></param>
        private void LoadCameraSettings(CameraSetting settings, List<string> choices, List<string> displayChoices)
        {
            var choice = string.IsNullOrEmpty(settings.CameraPath) ? noneString : settings.CameraPath;

            _CameraHelper.InitPlay(choices, displayChoices, choice, settings);
        }

        /// <summary>
        /// 初始化人物动作管理器
        /// </summary>
        /// <param name="atom"></param>
        /// <param name="atomid"></param>
        /// <returns></returns>
        private MotionHelper InitMotionHelper(Atom atom)
        {
            MotionHelper helper;

            if (!MotionHelperGroup.GetInstance().TryGetInitedMotionHelper(atom, out helper))
            {
                // 如果是新的对象，进行初始化
                helper.CreatePersonMotionUI(this, LeftSide);
                helper.OnMotionLoaded += OnMotionLoaded;
                helper.OnMotionInited += OnMotionInited;

                RefreshUIByCurrentUIMode();
            }

            return helper;
        }

        /// <summary>
        /// 动作初始化完成的事件
        /// </summary>
        /// <param name="sender"></param>
        private void OnMotionInited(MotionHelper sender)
        {
            if (sender.PersonAtom && !sender.PersonAtom.collisionEnabled)
            {
                sender.PersonAtom.collisionEnabled = true;
            }
        }

        /// <summary>
        /// 移除人物动作助手
        /// </summary>
        /// <param name="atom"></param>
        private void RemoveMotionHelper(Atom atom)
        {
            if (atom?.type == "Person")
            {
                var index = PersonAtoms.IndexOf(atom);

                if (index > -1)
                {
                    PersonAtoms.RemoveAt(index);

                    // 移除对应的动作数据
                    CurrentItem?.Motions.RemoveAt(index);
                }
                try
                {
                    _MotionHelperGroup.RemoveMotionHelper(atom);
                }
                catch (Exception ex)
                {
#if DEBUG
                    LogUtil.LogError(ex, $"Player::RemoveMotionHelper");
#endif
                }
            }
        }

        /// <summary>
        /// 刷新人物原子
        /// </summary>
        void RefreshPersonAtoms()
        {
            if (PersonAtoms.Count > 0)
            {
                foreach (var item in PersonAtoms)
                {
                    _MotionHelperGroup.RemoveMotionHelper(item);
                }

                PersonAtoms.Clear();
            }

            try
            {
                // 轮询和检查原子
                foreach (var atom in SuperController.singleton.GetAtoms())
                {
                    CheckAtomAdded(atom);
                }
            }
            catch (Exception ex)
            {
                LogUtil.LogError(ex, "CheckAtomAdded::");
            }

            RefreshFocusAtomList();
        }

        /// <summary>
        /// 打开人物
        /// </summary>
        /// <param name="person"></param>
        private void SetPersonOn(Atom person)
        {
            // 碰撞开启
            person.collisionEnabled = true;

            // 允许初始动作修正时调用
            if (config.EnableInitialMotionAdjustment)
            {
                MotionHelper helper = _MotionHelperGroup.GetMotionHelper(person);

                if (helper == null)
                {
                    LogUtil.LogWarning($"The MotionHelper for {person.name} is Not Init.");
                    return;
                }

                StartCoroutine(helper.Ready());

                //_MotionHelperGroup.Ready(person);
            }
        }

        /// <summary>
        /// 关闭人物
        /// </summary>
        /// <param name="person"></param>
        private void SetPersonOff(Atom person)
        {
            //if (config.ResetPhysicalWhenLoadMotion)
            //{
            //    // 重置物理
            //    person.ResetRigidbodies();
            //    person.ResetPhysical();
            //}

            // 关闭碰撞
            person.collisionEnabled = false;
            // 允许初始动作修正时调用
            if (config.EnableInitialMotionAdjustment)
            {
                MotionHelper helper = _MotionHelperGroup.GetMotionHelper(person);

                if (helper == null)
                {
                    LogUtil.LogWarning($"The MotionHelper for {person.name} is Not Init.");
                    return;
                }

                StartCoroutine(helper.MakeReady());
            }
        }

        /// <summary>
        /// 重置所有人物动作
        /// </summary>
        /// <returns></returns>
        IEnumerator ResetAllPersonMotion()
        {
            isMotionResetting = true;
            // 跳一帧
            yield return null;
            foreach (var item in _MotionHelperGroup.Helpers)
            {
                //var pre = item.PersonAtom.mainController.transform.position;

                //item.PersonAtom.tempFreezePhysics = true;
                //ResetPose(item.PersonAtom);
                //for (int i = 0; i < 30; i++)
                //    yield return null;
                //item.PersonAtom.mainController.SetPositionNoForce(pre);
                //item.PersonAtom.tempFreezePhysics = false;
                //for (int i = 0; i < 30; i++)
                //    yield return null;
                //item.UpdateTransform();
                //yield return null;

                yield return item.ReloadMotions(3, init: true);

                for (int i = 0; i < 30; i++)
                    yield return null;

                SetPersonOff(item.PersonAtom);

                for (int i = 0; i < 30; i++)
                    yield return null;

                SetPersonOn(item.PersonAtom);
            }
            yield return null;
            isMotionResetting = false;
            yield return null;
        }

        /// <summary>
        /// 设置所有人物关闭
        /// </summary>
        private void SetAllPersonOff()
        {
            foreach (var atom in PersonAtoms)
            {
                SetPersonOff(atom);
            }
        }

        /// <summary>
        /// 设置所有人物开启
        /// </summary>
        private void SetAllPersonOn()
        {
            foreach (var atom in PersonAtoms)
            {
                SetPersonOn(atom);
            }
        }

        #endregion

        #region 镜头播放的代码
        public void AlignHeadToCamera(bool fixedview = false)
        {
            try
            {
                // 如果主HUD激活，则停止跟踪（测试时不启用）
                if (!config.CameraActive)
                {
                    _CameraHelper.IsActive = false;
                }
                else
                {
                    _CameraHelper.IsActive = true;
                }
            }
            catch (Exception e)
            {
                LogUtil.Debug(e, "Failed to update: ");
            }
        }

        #endregion

        /// <summary>
        /// 当启用时执行的方法
        /// </summary>
        public override void OnEnable()
        {
            SuperController.singleton.onAtomAddedHandlers += OnAtomAdded;
            SuperController.singleton.onAtomRemovedHandlers += OnAtomRemoved;

            base.OnEnable();

            _AudioPlayHelper?.OnEnable();
            _CameraHelper?.OnEnable();
            _MMDFolderHelper?.OnEnable();
            _MotionHelperGroup?.OnEnable();
            _ProgressHelper?.OnEnable();
        }

        /// <summary>
        /// 当禁用时执行的方法
        /// </summary>
        public override void OnDisable()
        {
            SuperController.singleton.onAtomAddedHandlers -= OnAtomAdded;
            SuperController.singleton.onAtomRemovedHandlers -= OnAtomRemoved;

            base.OnDisable();

            _ProgressHelper?.OnDisable();
            _AudioPlayHelper?.OnDisable();
            _CameraHelper?.OnDisable();
            _MotionHelperGroup?.OnDisable();
            _MMDFolderHelper?.OnDisable();
        }

        /// <summary>
        /// 当销毁时执行的方法
        /// </summary>
        public override void OnDestroy()
        {
            SuperController.singleton.onAtomAddedHandlers -= OnAtomAdded;
            SuperController.singleton.onAtomRemovedHandlers -= OnAtomRemoved;

            // 未初始化，不执行后边的代码
            if (!isInited)
                return;

            DestroyHUDUI();

            RemovePlaylistEvent();

            _ProgressHelper.OnPlayStatusChanged -= OnPlayStatusChanged;
            _ProgressHelper.OnProgressChanged -= SyncProgress;
            _ProgressHelper.OnDestroy();
            _ProgressHelper = null;

            _AudioPlayHelper.OnAudioLoaded -= OnAudioLoaded;
            _AudioPlayHelper.OnDestroy();
            _AudioPlayHelper = null;

            _CameraHelper.OnCameraMotionLoaded -= OnCameraLoaded;
            _CameraHelper.OnCameraActivateStatusChanged -= OnCameraActivateStatusChanged;
            _CameraHelper.OnDestroy();
            _CameraHelper = null;

            foreach (var m in _MotionHelperGroup.Helpers)
            {
                m.OnMotionLoaded -= OnMotionLoaded;
                m.OnMotionInited -= OnMotionInited;
            }
            _MotionHelperGroup.OnDestroy();
            _MotionHelperGroup = null;

            _MMDFolderHelper.OnMMDImported -= OnMMDImported;
            _MMDFolderHelper.OnMMDSelected -= OnMMDSelected;
            _MMDFolderHelper.OnDestroy();
            _MMDFolderHelper = null;

            this.CurrentItem = null;

            this._PersonAtoms.Clear();
            this._PersonAtoms = null;

            this._PlayUIs.Clear();
            this._PlayUIs = null;

            this._UIPlayButton = null;

            SuperController.singleton.BroadcastMessage("OnActionsProviderDestroyed", this, SendMessageOptions.DontRequireReceiver);

            base.OnDestroy();
        }
    }
}
