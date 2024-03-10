using UnityEngine;

namespace LibMMD.Motion
{
    public class CameraKeyframe
    {
        public float Fov { get; set; }
        public float FocalLength { get; set; }

        public Vector3 Position { get; set; }
        public Vector3 Rotation { get; set; }

        public byte[] Interpolation { get; set; }

        public bool Orthographic { get; set; }

        #region 重写的镜头处理

        //public int Key;

        public Curve xCurve
        {
            get
            {
                return new Curve
                {
                    a = Interpolation[0] / 127.0,
                    b = Interpolation[2] / 127.0,
                    c = Interpolation[1] / 127.0,
                    d = Interpolation[3] / 127.0
                };
            }
        }
        public Curve yCurve
        {
            get
            {
                return new Curve
                {
                    a = Interpolation[4] / 127.0,
                    b = Interpolation[6] / 127.0,
                    c = Interpolation[5] / 127.0,
                    d = Interpolation[7] / 127.0
                };
            }
        }
        public Curve zCurve
        {
            get
            {
                return new Curve
                {
                    a = Interpolation[8] / 127.0,
                    b = Interpolation[10] / 127.0,
                    c = Interpolation[9] / 127.0,
                    d = Interpolation[11] / 127.0
                };
            }
        }
        public Curve rCurve
        {
            get
            {
                return new Curve
                {
                    a = Interpolation[12] / 127.0,
                    b = Interpolation[14] / 127.0,
                    c = Interpolation[13] / 127.0,
                    d = Interpolation[15] / 127.0
                };
            }
        }
        public Curve dCurve
        {
            get
            {
                return new Curve
                {
                    a = Interpolation[16] / 127.0,
                    b = Interpolation[18] / 127.0,
                    c = Interpolation[17] / 127.0,
                    d = Interpolation[19] / 127.0
                };
            }
        }
        public Curve vCurve
        {
            get
            {
                return new Curve
                {
                    a = Interpolation[20] / 127.0,
                    b = Interpolation[22] / 127.0,
                    c = Interpolation[21] / 127.0,
                    d = Interpolation[23] / 127.0
                };
            }
        }
        #endregion
    }

    public struct Curve
    {
        public double a;
        public double b;
        public double c;
        public double d;
    }

}