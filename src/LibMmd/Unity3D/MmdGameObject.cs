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
        public float PhysicsFps = 120.0f;

        public string ModelName
        {
            get { return _model.Name; }
        }

        public string BonePoseFilePath { get; private set; }

        public MmdGameObject()
        {
        }
        public GameObject[] _bones;
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
        private float _motionScale;

        public MotionPlayer _motionPlayer;
        public float _playTime;
        bool _forceDisableIK = false;
        public bool ForceDisableIK
        {
            set
            {
                _forceDisableIK = value;
                if (_poser != null)
                {
                    _poser.ForceDisableIK = value;
                }
            }
        }


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

        public bool LoadModel(string path = null)
        {
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
            MatchBone();

            if (_motion != null)
            {
                ResetMotionPlayer();
            }
            _playTime = 0.0f;
            UpdateBones();
            return true;
        }

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

        public Action<MmdModel> m_MatchBone;
        public void MatchBone()
        {
            if (m_MatchBone != null)
                m_MatchBone(this._model);
        }

        private void DoLoadModel(string path)
        {
            //Debug.LogFormat("start load model {0}", filePath);

            //每个骨骼的位置都定好
            _model = ModelReader2.LoadMmdModel(path);
            ChangeInitTransform();

            Release();

            _poser = new Poser(_model);
            _poser.ForceDisableIK = _forceDisableIK;

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
            if (_motionPlayer != null)
            {
                _motionPlayer.Clear();
                _motionPlayer = null;
            }

            _motionPlayer = new MotionPlayer(_motion, _poser);
            _motionPlayer.SeekFrame(0, _motionScale);
            _poser.PrePhysicsPosing();
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

            _poser?.Clear();
        }

        public void ResetMotion()
        {
            if (_motionPlayer == null)
            {
                return;
            }
            //StopBonePoseCalculation();
            _playTime = 0.0f;
            _motionPlayer.SeekFrame(0, _motionScale);
            _poser.PrePhysicsPosing();
            _poser.PostPhysicsPosing();
            //StartBonePoseCalculation(0.0, 1.0f / PhysicsFps);
            UpdateBones();
        }

        public void SeekFrame(int frame)
        {
            _playTime = (float)frame / 30;
            _motionPlayer.SeekFrame(frame, _motionScale);
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
            _motionPlayer.SeekTime(_playTime, _motionScale);
            _poser.PrePhysicsPosing();
            _poser.PostPhysicsPosing();
            UpdateBones();
            if (OnUpdate != null)
                OnUpdate(this);
        }
        internal void ClearMotion()
        {
            if (_motion != null)
            {
                _motion.Clear();
                _motion = null;
            }
            importedVmdPath.Clear();
        }

        public double GetMotionPos()
        {
            return _playTime;
        }

        public void SetMotionPos(float pos, bool update = true, float motionScale = 1f)
        {
            if (_motion == null) return;
            _motionScale = motionScale;
            _playTime = pos;
            _motionPlayer.SeekTime(_playTime, motionScale);
            _poser.PrePhysicsPosing();
            _poser.PostPhysicsPosing();
            UpdateBones();
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
                if (Config.s_Debug)
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
    }
}