using MacGruber;
using UnityEngine;

namespace mmd2timeline
{
    internal partial class MotionHelper
    {
        #region 骨骼旋转微调处理函数
        JSONStorableStringChooser _BoneRotationAdjust;
        JSONStorableFloat _BoneAdjustX;
        JSONStorableFloat _BoneAdjustY;
        JSONStorableFloat _BoneAdjustZ;

        /// <summary>
        /// 更新骨骼调整值
        /// </summary>
        /// <param name="name"></param>
        void UpdateBoneAdjustValues(string name)
        {
            var values = GetVectorByBoneName(name);
            _BoneAdjustX.valNoCallback = values.x;
            _BoneAdjustY.valNoCallback = values.y;
            _BoneAdjustZ.valNoCallback = values.z;
        }

        void SetBoneAdjustX(float v)
        {
            var boneName = Lang.From(_BoneRotationAdjust.val);

            SetBoneAdjustX(boneName, v);
        }
        void SetBoneAdjustY(float v)
        {
            var boneName = Lang.From(_BoneRotationAdjust.val);

            SetBoneAdjustY(boneName, v);
        }
        void SetBoneAdjustZ(float v)
        {
            var boneName = Lang.From(_BoneRotationAdjust.val);

            SetBoneAdjustY(boneName, v);
        }

        /// <summary>
        /// 清除所有骨骼角度微调数值
        /// </summary>
        /// <param name="controller"></param>
        void ClearAllBoneRotationAdjust()
        {
            _BoneAdjustX.val = 0;
            _BoneAdjustY.val = 0;
            _BoneAdjustZ.val = 0;

            ClearBoneRotationAdjust();
        }

        #endregion

        /// <summary>
        /// 保存骨骼旋转微调数据
        /// </summary>
        /// <param name="motion"></param>
        internal void UpdateBoneRotationAdjustValuesToSettings()
        {
            var boneAdjusts = GetBoneAdjustValues();

            foreach (var boneAdjust in boneAdjusts)
            {
                if (boneAdjust.Value.x != 0f)
                    _MotionSetting[$"BoneAdjust-{boneAdjust.Key}-X"].AsFloat = boneAdjust.Value.x;
                if (boneAdjust.Value.y != 0f)
                    _MotionSetting[$"BoneAdjust-{boneAdjust.Key}-Y"].AsFloat = boneAdjust.Value.y;
                if (boneAdjust.Value.z != 0f)
                    _MotionSetting[$"BoneAdjust-{boneAdjust.Key}-Z"].AsFloat = boneAdjust.Value.z;
            }
        }

        /// <summary>
        /// 加载骨骼旋转微调数据
        /// </summary>
        /// <param name="motion"></param>
        /// <param name="personMotionController"></param>
        internal void LoadBoneRotationAdjustValues()
        {
            var currentBoneName = Lang.From(_BoneRotationAdjust.val);

            foreach (var name in AdjustBones.Names)
            {
                var key = $"BoneAdjust-{name}-";

                var val = Vector3.zero;

                if (_MotionSetting.HasKey($"{key}X"))
                {
                    val.x = _MotionSetting[$"{key}X"].AsFloat;
                }

                if (_MotionSetting.HasKey($"{key}Y"))
                {
                    val.y = _MotionSetting[$"{key}Y"].AsFloat;
                }

                if (_MotionSetting.HasKey($"{key}Z"))
                {
                    val.z = _MotionSetting[$"{key}Z"].AsFloat;
                }

                SetBoneAdjust(name, val);
            }

            UpdateBoneAdjustValues(currentBoneName);
        }

        /// <summary>
        /// 创建骨骼旋转微调UI
        /// </summary>
        /// <param name="controller"></param>
        /// <param name="personId"></param>
        private void CreateBoneRotationAdjustUI(BaseScript self, bool rightSide = false)
        {
            _BoneRotationAdjust = self.SetupStaticEnumsChooser<AdjustBones>("Bone Rotation Adjust", AdjustBones.Names, AdjustBones.GetName(AdjustBones.Arm), rightSide, v => UpdateBoneAdjustValues(v));

            _BoneAdjustX = Utils.SetupSliderFloat(self, Lang.Get("Adjust Bone Rotation X"), 0f, -30f, 30f, v => SetBoneAdjustX(v), rightSide);
            _BoneAdjustY = Utils.SetupSliderFloat(self, Lang.Get("Adjust Bone Rotation Y"), 0f, -30f, 30f, v => SetBoneAdjustY(v), rightSide);
            _BoneAdjustZ = Utils.SetupSliderFloat(self, Lang.Get("Adjust Bone Rotation Z"), 0f, -30f, 30f, v => SetBoneAdjustZ(v), rightSide);

            _MotionSettingsUI.Elements.Add(Utils.SetupButton(self, Lang.Get("Clear Bone Rotation Adjust"), () => ClearAllBoneRotationAdjust(), rightSide));

            _MotionSettingsUI.Elements.Add(_BoneRotationAdjust);
            _MotionSettingsUI.Elements.Add(_BoneAdjustX);
            _MotionSettingsUI.Elements.Add(_BoneAdjustY);
            _MotionSettingsUI.Elements.Add(_BoneAdjustZ);
        }
    }
}
