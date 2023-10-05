using System;
using System.Collections.Generic;
using System.Collections;
//using System.IO;
using System.Linq;
using LibMMD.Model;
using LibMMD.Motion;
using LibMMD.Reader;
using LibMMD.Util;
using UnityEngine;
using mmd2timeline;
using MVR.FileManagementSecure;

namespace LibMMD.Unity3D
{
    public class MmdGameObject : MonoBehaviour
    {
        public Action<MmdGameObject> OnUpdate = null;

        public bool AutoPhysicsStepLength = true;
        public bool Playing;
        public int PhysicsCacheFrameSize = 300;
        public int PhysicsMode = None;
        public float PhysicsFps = 120.0f;

        public string ModelName
        {
            get { return _model.Name; }
        }

        //public string MotionPath { get; private set; }
        public string BonePoseFilePath { get; private set; }

        public MmdGameObject()
        {
        }
        public GameObject[] _bones;
        //private const int DefaultMaxTextureSize = 1024;
        public MmdModel _model;
        public Poser _poser;
        public MmdMotion _motion;

        public float MotionLength
        {
            get
            {
                if (_motion != null)
                    return (float)_motion.Length / 30;
                return 0;
            }
        }

        public MotionPlayer _motionPlayer;
        public float _playTime;
        private List<List<int>> _partIndexes;
        private List<Vector3[]> _partMorphVertexCache;
        //private readonly ModelReadConfig _modelReadConfig = new ModelReadConfig { GlobalToonPath = "" };
        private UnityEngine.Material[] _materials;

        //private MmdUnityConfig _config = new MmdUnityConfig();
        private GameObject _boneRootGameObject;

        public static GameObject CreateGameObject(string name = "MMDGameObject")
        {
            var obj = new GameObject(name);
            obj.AddComponent<MeshFilter>();
            obj.AddComponent<MmdGameObject>();

            var skinnedMeshRenderer = obj.AddComponent<SkinnedMeshRenderer>();
            skinnedMeshRenderer.quality = SkinQuality.Bone4;

            return obj;
        }

        //public enum PhysicsModeEnum
        //{
        public const int None = 0;
        public const int Unity = 1;
        //}

        public bool LoadModel(string path = null)
        {
            //ModelPath = path;
            try
            {
                DoLoadModel(path);
            }
            catch (Exception e)
            {
                Debug.LogException(e);
                return false;
            }

            Utils.ClearAllTransformChild(transform);
            _bones = CreateBones(gameObject);

            if (_motion != null)
            {
                ResetMotionPlayer();
            }
            _playTime = 0.0f;
            //RestartBonePoseCalculation(0.0f, 1 / PhysicsFps);
            UpdateBones();
            return true;
        }

        //private void RestartBonePoseCalculation(double startTimePos, double stepLength)
        //{
        //StopBonePoseCalculation();
        //StartBonePoseCalculation(startTimePos, stepLength);
        //}

        //private void StartBonePoseCalculation(double startTimePos, double stepLength)
        //{
        //}
        //private void StopBonePoseCalculation()
        //{
        //}

        public void LoadMotion(string path)
        {
            if (_model == null)
            {
                throw new InvalidOperationException("model not loaded yet");
            }
            if (importedVmdPath.FindIndex(v => path == v) >= 0)
            {
                //已经加载过了
                return;
            }

            //MotionPath = path;
            var hasMotionData = LoadMotionKernal(path);
            _playTime = 0.0f;
            UpdateBones();
            //UpdateMesh(_playTime);
            if (hasMotionData)
            {
                //RestartBonePoseCalculation(_playTime, 1.0f / PhysicsFps);
            }
        }
        //public TimelineJson json;
        //public Dictionary<string, TimelineControlJson> timelineControlLookup;
        //public bool m_FrameStepMode = false;

        //public Dictionary<string, FloatParamsJson> m_RightFingerMotions = null;
        //public Dictionary<string, FloatParamsJson> m_LeftFingerMotions = null;


        //public void SetFingerKeyFrame(float time,string boneName,string key,float value)
        //{
        //    var dic = boneName.StartsWith("l") ? m_LeftFingerMotions : m_RightFingerMotions;
        //    TimelineFrameJson f = new TimelineFrameJson();
        //    f.t = time.ToString();
        //    f.v = ((int)value).ToString();
        //    f.c = "3";
        //    f.i = f.v;
        //    f.o = f.v;
        //    dic[key].Value.Add(f);
        //}
        //public Dictionary<string, HashSet<int>> fingerKeyFrames;

        //public void StepPlay(float beginTime, float endTime)
        //{
        //    json = new TimelineJson();
        //    json.Clips = new List<TimelineClipJson>();
        //    TimelineClipJson clipJson = new TimelineClipJson();
        //    json.Clips.Add(clipJson);

        //    //fingerKeyFrames = GetFingerKeyFrames();
        //    var list = GetMorphKeyFrames(beginTime, endTime);

        //    clipJson.FloatParams = list;
        //    m_RightFingerMotions = new Dictionary<string, FloatParamsJson>();
        //    m_LeftFingerMotions = new Dictionary<string, FloatParamsJson>();

        //    for (int i = 0; i < 2; i++)
        //    {
        //        var motions = i == 0 ? m_LeftFingerMotions : m_RightFingerMotions;
        //        foreach (var item in FingerMorph.setting)
        //        {
        //            var j = new FloatParamsJson();
        //            j.Storable = FingerMorph.StorableNames[i];
        //            j.Name = item;
        //            j.Min = "-100";
        //            j.Max = "100";
        //            motions.Add(item, j);
        //            clipJson.FloatParams.Add(j);
        //        }
        //    }


        //    clipJson.AnimationName = this._motion.Name;
        //    clipJson.AnimationLength = (endTime-beginTime).ToString();

        //    clipJson.Controllers = new List<TimelineControlJson>();
        //    timelineControlLookup = new Dictionary<string, TimelineControlJson>();
        //    //控制器动画
        //    foreach(var item in DazBoneMapping.vamControls)
        //    {
        //        if (Settings.EnableHeelAjust)
        //        {
        //            if (item == "rToeControl" || item == "lToeControl")
        //                continue;
        //        }
        //        var controlJson = new TimelineControlJson();
        //        controlJson.Controller = item;
        //        timelineControlLookup.Add(item, controlJson);
        //        clipJson.Controllers.Add(controlJson);
        //    }

        //    m_FrameStepMode = true;
        //    StartCoroutine(CoStepPlay(beginTime,endTime));
        //}
        //public float StepSpeed = 2;
        //public int CurFrame = 0;
        //public List<int> m_FrameSteps;
        //public bool IsSampling = false;
        //public bool IsPausing = false;

        //public float GetRelativeTime()
        //{
        //    int frame = CurFrame;
        //    float time = (float)frame / 30;
        //    float val = time - m_BeginTime;
        //    float min = 0;
        //    float max = m_EndTime - m_BeginTime;
        //    return Mathf.Clamp(val, min, max);
        //}
        //float m_BeginTime;
        //float m_EndTime;
        //IEnumerator CoStepPlay(float beginTime, float endTime)
        //{
        //    m_BeginTime = beginTime;
        //    m_EndTime = endTime;
        //    IsPausing = false;
        //    IsSampling = true;
        //    m_FrameSteps = GetMotionKeyFrames(beginTime, endTime);
        //    for (int i = 0; i < m_FrameSteps.Count; i++)
        //    {
        //        CurFrame = m_FrameSteps[i];
        //        _motionPlayer.SeekFrame(CurFrame);
        //        _poser.PrePhysicsPosing(false);
        //        _poser.PostPhysicsPosing();
        //        UpdateBones();
        //        if (OnUpdate != null)
        //            OnUpdate(this);

        //        if (i >= m_FrameSteps.Count - 1)
        //            break;

        //        int diff = m_FrameSteps[i + 1] - CurFrame;

        //        while (IsPausing)
        //            yield return new WaitForSeconds(0.5f);

        //        yield return new WaitForSeconds((1.0f / 30/ StepSpeed) * diff);
        //    }
        //    IsSampling = false;
        //    IsPausing = false;
        //    CurFrame = 0;
        //}
        //public void UpdateHeelAdjust()
        //{
        //    _motionPlayer.SeekFrame(CurFrame);
        //    _poser.PrePhysicsPosing(false);
        //    _poser.PostPhysicsPosing();
        //    UpdateBones();
        //    if (OnUpdate != null)
        //        OnUpdate(this);
        //}


        //private void Update()
        //{
        //    if (!Playing)
        //    {
        //        return;
        //    }
        //    if (m_FrameStepMode) return;

        //    var deltaTime = Time.deltaTime;
        //    _playTime += deltaTime;

        //    _motionPlayer.SeekTime(_playTime);
        //    _poser.PrePhysicsPosing(false);
        //    _poser.PostPhysicsPosing();

        //    UpdateBones();

        //    if (OnUpdate != null)
        //        OnUpdate(this);
        //}
        //表情
        public Dictionary<string, float> GetUpdatedMorph(float time)
        {
            Dictionary<string, float> faceMorphs = new Dictionary<string, float>();
            foreach (var item in FaceMorph.setting)
            {
                var morph = _motion.GetMorphPose(item.Key, time);
                float weight = morph.Weight;
                var settings = item.Value;
                foreach (var s in settings)
                {
                    float newValue = Mathf.Lerp(s.min, s.max, weight);
                    if (faceMorphs.ContainsKey(s.name))
                    {
                        if (faceMorphs[s.name] >= newValue)
                        {
                            continue;
                        }
                        faceMorphs[s.name] = newValue;
                    }
                    else
                    {
                        faceMorphs.Add(s.name, newValue);
                    }
                }
            }
            return faceMorphs;
        }
        public List<FloatParamsJson> GetMorphKeyFrames(float beginTime, float endTime)
        {
            List<FloatParamsJson> FloatParams = new List<FloatParamsJson>();

            Dictionary<string, FloatParamsJson> cache = new Dictionary<string, FloatParamsJson>();
            var motions = this._motion.MorphMotions;
            foreach (var item in motions)
            {
                if (FaceMorph.setting.ContainsKey(item.Key))
                {
                    Debug.Log("morph " + item.Key);
                    FaceMorph.MorphSetting[] morphs = FaceMorph.setting[item.Key];
                    foreach (var m in morphs)
                    {
                        if (!cache.ContainsKey(m.name))
                        {
                            var j = new FloatParamsJson();
                            j.Name = m.name;
                            cache.Add(m.name, j);
                        }

                        foreach (var frame in item.Value)
                        {
                            float time = (float)frame.Key / 30;
                            if (time >= beginTime && time <= endTime)
                            {
                                float v = Mathf.Lerp(m.min, m.max, frame.Value.Weight);
                                TimelineFrameJson x = new TimelineFrameJson(time - beginTime, v, "3");
                                //x.t = (time- beginTime).ToString();
                                x.frame = frame.Key;
                                x.value = v;
                                //x.v = x.value.ToString();
                                //x.c = "3";
                                //x.i = x.v;
                                //x.o = x.v;

                                if (cache[m.name].ValueLookup.ContainsKey(x.frame))
                                {
                                    var json = cache[m.name].ValueLookup[x.frame];
                                    if (json.value < x.value)
                                    {
                                        cache[m.name].Value.Remove(json);
                                        cache[m.name].ValueLookup.Remove(x.frame);

                                        cache[m.name].Value.Add(x);
                                        cache[m.name].ValueLookup.Add(x.frame, x);
                                    }
                                }
                                else
                                {
                                    cache[m.name].Value.Add(x);
                                    cache[m.name].ValueLookup.Add(x.frame, x);
                                }
                            }
                        }
                    }
                }
                else
                {
                    Debug.LogWarning("not process " + item.Key);
                }
            }

            foreach (var item in cache)
            {
                item.Value.Value.Sort((a, b) =>
                {
                    if (a.frame > b.frame)
                        return 1;
                    else if ((a.frame < b.frame))
                        return -1;
                    else
                        return 0;
                });

                FloatParams.Add(item.Value);
            }
            return FloatParams;
        }

        private void OnDestroy()
        {
            StopAllCoroutines();
            Release();
        }

        public Action<MmdModel> m_ChangeInitTransform;
        public void ChangeInitTransform()
        {
            if (m_ChangeInitTransform != null)
                m_ChangeInitTransform(this._model);
        }

        private void DoLoadModel(string path)
        {
            //Debug.LogFormat("start load model {0}", filePath);

            //每个骨骼的位置都定好
            _model = ModelReader2.LoadMmdModel(path);
            ChangeInitTransform();

            Release();

            _poser = new Poser(_model);

            Debug.LogFormat("load model finished");
        }
        List<string> importedVmdPath = new List<string>();
        public string GetImportedVmds()
        {
            List<string> names = new List<string>();
            foreach (var item in importedVmdPath)
            {
                string name = FileManagerSecure.GetFileName(item);
                if (name.EndsWith(".vmd"))
                {
                    name = name.Substring(0, name.Length - 4);
                }
                names.Add(name);
            }
            return string.Join(",", names.ToArray());
        }
        private bool LoadMotionKernal(string filePath)
        {
            if (_motion == null)
            {
                importedVmdPath.Add(filePath);
                _motion = new VmdReader2().Read(filePath);
            }
            else
            {
                importedVmdPath.Add(filePath);
                var tempMotion = new VmdReader2().Read(filePath);
                _motion.Length = Mathf.Max(_motion.Length, tempMotion.Length);
                _motion.CombineBoneMotions(tempMotion.BoneMotions);
                _motion.CombineMorphMotions(tempMotion.MorphMotions);

            }
            if (_motion.Length == 0)
            {
                _poser.ResetPosing();
                ResetMotionPlayer();
                return false;
            }
            ResetMotionPlayer();
            //_poser.Deform();
            return true;
        }

        private void ResetMotionPlayer()
        {
            _motionPlayer = new MotionPlayer(_motion, _poser);
            _motionPlayer.SeekFrame(0);
            _poser.PrePhysicsPosing();
            //_physicsReactor.Reset();
            _poser.PostPhysicsPosing();
        }

        public List<int> GetMotionKeyFrames(float beginTime, float endTime)
        {
            var motions = _motion.BoneMotions;
            HashSet<int> frames = new HashSet<int>();
            foreach (var item in motions)
            {
                foreach (var item2 in item.Value)
                {
                    frames.Add(item2.Key);
                }
            }
            List<int> ret = new List<int>();
            int startFrame = (int)(beginTime * 30);
            int endFrame = (int)(endTime * 30);
            foreach (var item in frames)
            {
                if (item >= startFrame && item <= endFrame)
                    ret.Add(item);
            }
            ret.Sort();
            return ret;
        }

        public void UpdateBones(bool skipPhysicsControlBones = false)
        {
            if (CanNotUpdateBone())
            {
                //Debug.LogError("illegal argument for UpdateBones");
                return;
            }
            for (var i = 0; i < _bones.Length; ++i)
            {
                UpdateBone(i);
            }
        }

        private bool CanNotUpdateBone()
        {
            return _bones == null || _poser == null || _model == null
                || _poser.BoneImages.Length != _bones.Length ||
                   _model.Bones.Length != _bones.Length;
        }

        private void UpdateBone(int i)
        {
            var rootTrans = _boneRootGameObject.transform;
            var transMatrix = _poser.BoneImages[i].SkinningMatrix;
            //从本地坐标变换到世界坐标
            _bones[i].transform.position =
                rootTrans.TransformPoint(transMatrix.MultiplyPoint3x4(_model.Bones[i].Position));
            _bones[i].transform.rotation = rootTrans.rotation * transMatrix.ExtractRotation();
            _bones[i].transform.localScale = Vector3.one;
        }
        private void Release()
        {
            //StopBonePoseCalculation();
        }

        public void ResetMotion()
        {
            if (_motionPlayer == null)
            {
                return;
            }
            //StopBonePoseCalculation();
            _playTime = 0.0f;
            _motionPlayer.SeekFrame(0);
            _poser.PrePhysicsPosing();
            _poser.PostPhysicsPosing();
            //StartBonePoseCalculation(0.0, 1.0f / PhysicsFps);
            UpdateBones();
        }

        public void SeekFrame(int frame)
        {
            _playTime = (float)frame / 30;
            _motionPlayer.SeekFrame(frame);
        }

        /// <summary>
        /// 刷新
        /// </summary>
        public void Refresh()
        {
            if (_motionPlayer == null)
            {
                return;
            }
            _motionPlayer.SeekTime(_playTime);
            _poser.PrePhysicsPosing();
            _poser.PostPhysicsPosing();
            UpdateBones();
            if (OnUpdate != null)
                OnUpdate(this);
        }
        internal void ClearMotion()
        {
            _motion = null;
            importedVmdPath.Clear();
        }

        public double GetMotionPos()
        {
            return _playTime;
        }

        public void SetMotionPos(float pos, bool update = true, float motionScale = 1f)
        {
            if (_motion == null) return;
            //StopBonePoseCalculation();
            _playTime = pos;
            //_restStepTime = 0.0f;
            _motionPlayer.SeekTime(_playTime, motionScale);
            _poser.PrePhysicsPosing();
            //_physicsReactor.Reset();
            _poser.PostPhysicsPosing();
            //StartBonePoseCalculation(0.0, 1.0f / PhysicsFps);
            UpdateBones();
            //_poser.Deform();
            if (update && OnUpdate != null)
                OnUpdate(this);
        }
        

        private GameObject[] CreateBones(GameObject rootGameObject)
        {
            if (_model == null)
            {
                return new GameObject[0];
            }
            var bones = EntryAttributeForBones();
            //先创建骨骼节点，然后全部串起来。
            AttachParentsForBone(rootGameObject, bones);
            return bones;
        }

        private GameObject[] EntryAttributeForBones()
        {
            return _model.Bones.Select(x =>
            {
                var gameObject = new GameObject(x.Name);
                //debug
                if (Settings.s_Debug)
                {
                    var go = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                    go.transform.parent = gameObject.transform;
                    go.transform.localPosition = Vector3.zero;
                    go.transform.localScale = new Vector3(0.2f, 0.2f, 0.2f);
                    go.GetComponent<MeshRenderer>().material.color = Color.red;
                    var col = go.GetComponent<Collider>();
                    Component.DestroyImmediate(col);
                }

                //gameObject.transform.position = x.Position;
                gameObject.transform.position = x.InitPosition;
                return gameObject;
            }).ToArray();
        }

        private void AttachParentsForBone(GameObject rootGameObject, GameObject[] bones)
        {
            var rootObj = new GameObject("Model");
            _boneRootGameObject = rootObj;
            var modelRootTransform = rootObj.transform;
            GetComponent<SkinnedMeshRenderer>().rootBone = modelRootTransform;
            modelRootTransform.parent = rootGameObject.transform;
            rootObj.transform.localPosition = Vector3.zero;
            rootObj.transform.localRotation = Quaternion.identity;
            rootObj.transform.localScale = Vector3.one;

            for (int i = 0, iMax = _model.Bones.Length; i < iMax; ++i)
            {
                var parentBoneIndex = _model.Bones[i].ParentIndex;
                bones[i].transform.parent = parentBoneIndex < bones.Length && parentBoneIndex >= 0
                    ? bones[parentBoneIndex].transform
                    : modelRootTransform;
            }
        }
        //private void OnDrawGizmos()
        //{
        //    Color col = Gizmos.color;
        //    if (_model != null)
        //    {
        //        Gizmos.color = Color.red;
        //        foreach (var item in _model.Bones)
        //        {
        //            Gizmos.DrawWireSphere(item.Position, 0.1f);
        //        }
        //    }
        //    if (_bones != null)
        //    {
        //        Gizmos.color = Color.green;
        //        foreach (var item in _bones)
        //        {
        //            Gizmos.DrawWireSphere(item.transform.position, 0.1f);
        //        }
        //    }
        //    Gizmos.color = col;
        //}

    }
}