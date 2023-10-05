using UnityEngine;

namespace LibMMD.Motion
{
    public class BonePose
    {
        public Vector3 Translation { get; set; }
        public Quaternion Rotation { get; set; }

        public bool SkipForIk = false;
    }
}