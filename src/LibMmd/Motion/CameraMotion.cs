using mmd2timeline;
using Oculus.Platform;
using System.Collections.Generic;
using UnityEngine;

namespace LibMMD.Motion
{
    public class CameraMotion
    {
        List<KeyValuePair<int, CameraKeyframe>> _keyFrames;
        public List<KeyValuePair<int, CameraKeyframe>> KeyFrames
        {
            get
            {
                return _keyFrames;
            }
            set
            {
                _keyFrames = value;
                prevIndex = 0;
            }
        }

        private CameraPose GetCameraPoseByFrame(float frame)
        {
            if (KeyFrames.Count == 0)
            {
                return null;
            }
            if (KeyFrames[0].Key >= frame)
            {
                var value = KeyFrames[0].Value;
                return CameraKeyFrameToCameraPose(value);
            }

            if (KeyFrames[KeyFrames.Count - 1].Key <= frame)
            {
                var value = KeyFrames[KeyFrames.Count - 1].Value;
                return CameraKeyFrameToCameraPose(value);
            }

            var toSearch = new KeyValuePair<int, CameraKeyframe>((int)frame, null);

            var rightBoundIndex = KeyFrames.BinarySearch(toSearch, CameraKeyframeSearchComparator.Instance);

            if (rightBoundIndex < 0)
            {
                rightBoundIndex = ~rightBoundIndex;
            }
            int leftBoundIndex;
            if (rightBoundIndex == 0)
            {
                leftBoundIndex = 0;
            }
            else if (rightBoundIndex >= KeyFrames.Count)
            {
                rightBoundIndex = leftBoundIndex = KeyFrames.Count - 1;
            }
            else
            {
                leftBoundIndex = rightBoundIndex - 1;
            }
            var rightBound = KeyFrames[rightBoundIndex];
            var rightFrame = rightBound.Key;
            var rightKey = rightBound.Value;
            var leftBound = KeyFrames[leftBoundIndex];
            var leftFrame = leftBound.Key;
            var leftKey = leftBound.Value;
            if (leftFrame == rightFrame)
            {
                return CameraKeyFrameToCameraPose(leftKey);
            }
            var t = (frame - leftFrame) / (rightFrame - leftFrame);
            var points = new float[6];//MoveX,MoveY,MoveZ,Rotate,Distance,Angle
            bool isLinear = false;
            for (var i = 0; i < 6; i++)
            {
                var p1 = new Vector3(leftKey.Interpolation[i * 4], leftKey.Interpolation[i * 4 + 2]);
                var p2 = new Vector3(leftKey.Interpolation[i * 4 + 1], leftKey.Interpolation[i * 4 + 3]);
                //线性插值
                if (p1.x == p1.y && p2.x == p2.y)
                {
                    points[i] = t;
                    isLinear = true;
                }
                else
                    points[i] = CalculBezierPointByTwo(t, p1, p2);
            }

            if (isLinear && leftFrame == rightFrame - 1)
            {
                return CameraKeyFrameToCameraPose(leftKey);
            }
            //如果只差一帧的话，强制用线性插值。
            if (leftFrame == rightFrame - 1)
            {
                for (var i = 0; i < 6; i++)
                {
                    points[i] = t;
                }
            }

            var x = leftKey.Position.x + points[0] * (rightKey.Position.x - leftKey.Position.x);
            //x = (1-points[0])*leftKey.Position.x+points[0]*rightKey.Position.x

            var y = leftKey.Position.y + points[1] * (rightKey.Position.y - leftKey.Position.y);
            var z = leftKey.Position.z + points[2] * (rightKey.Position.z - leftKey.Position.z);

            var rx = leftKey.Rotation.x + points[3] * (rightKey.Rotation.x - leftKey.Rotation.x);
            var ry = leftKey.Rotation.y + points[3] * (rightKey.Rotation.y - leftKey.Rotation.y);
            var rz = leftKey.Rotation.z + points[3] * (rightKey.Rotation.z - leftKey.Rotation.z);
            var focalLength = leftKey.FocalLength + points[4] * (rightKey.FocalLength - leftKey.FocalLength);
            var fov = leftKey.Fov + (rightKey.Fov - leftKey.Fov) * points[5];
            return new CameraPose
            {
                FocalLength = focalLength,
                Fov = fov,
                Orthographic = leftKey.Orthographic,
                Position = new Vector3(x, y, z),
                Rotation = new Vector3(rx, ry, rz)
            };
        }

        private static CameraPose CameraKeyFrameToCameraPose(CameraKeyframe value)
        {
            return new CameraPose
            {
                FocalLength = value.FocalLength,
                Fov = value.Fov,
                Orthographic = value.Orthographic,
                Position = value.Position,
                Rotation = value.Rotation
            };
        }

        public CameraPose GetCameraPose(double time, bool evaluation = false, bool newInterpolate = false)
        {
            var frame = (float)(time * 30.0);
            if (evaluation)
            {
                return GetFrame(frame, newInterpolate);
            }
            return GetCameraPoseByFrame(frame);
        }

        private class CameraKeyframeSearchComparator : IComparer<KeyValuePair<int, CameraKeyframe>>
        {
            public static readonly CameraKeyframeSearchComparator Instance = new CameraKeyframeSearchComparator();

            private CameraKeyframeSearchComparator()
            {
            }

            public int Compare(KeyValuePair<int, CameraKeyframe> x, KeyValuePair<int, CameraKeyframe> y)
            {
                return x.Key.CompareTo(y.Key);
            }
        }


        //https://github.com/lzh1590/MMDCameraPath/blob/master/scripts/VMDCameraWork.cs
        private static float CalculBezierPointByTwo(float t, Vector3 p1, Vector3 p2)
        {
            var p = CalculateBezierPoint(t, Vector3.zero, p1, p2, new Vector3(127, 127, 0));
            var a = p.y / 127.0f;
            return a;
        }

        //https://github.com/lzh1590/MMDCameraPath/blob/master/scripts/VMDCameraWork.cs
        private static Vector3 CalculateBezierPoint(float t, Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3)
        {
            var u = 1 - t;
            var tt = t * t;
            var uu = u * u;
            var uuu = uu * u;
            var ttt = tt * t;
            var p = uuu * p0; //first term
            p += 3 * uu * t * p1; //second term
            p += 3 * u * tt * p2; //third term
            p += ttt * p3; //fourth term
            return p;
        }

        #region 重写的镜头数据获取方法

        int prevIndex = 0;

        public CameraPose GetFrame(float frame, bool newInterpolate = false)
        {
            if (this.KeyFrames.Count == 0)
                return null;

            // 如果目标帧小于等于第一帧，返回第一帧
            if (frame <= this.KeyFrames[0].Key)
            {
                var value = KeyFrames[0].Value;
                return CameraKeyFrameToCameraPose(value);
            }
            // 如果目标帧大于等于最后一帧，返回最后一帧
            else if (frame >= KeyFrames[KeyFrames.Count - 1].Key)
            {
                var value = KeyFrames[KeyFrames.Count - 1].Value;
                return CameraKeyFrameToCameraPose(value);
            }
            // 如果目标帧在上一帧和下一帧之间，取两帧之间的插值
            else if (frame > KeyFrames[this.prevIndex].Key && frame < KeyFrames[this.prevIndex + 1].Key)
            {
                if (newInterpolate)
                {
                    return NewInterpolateCameraFrame(frame, KeyFrames[this.prevIndex], KeyFrames[this.prevIndex + 1]);
                }
                return InterpolateCameraFrame(frame, KeyFrames[this.prevIndex], KeyFrames[this.prevIndex + 1]);
            }
            else
            {
                // 循环所有帧
                for (var i = 0; i < KeyFrames.Count; i++)
                {
                    // 如果目标帧与当前帧一致
                    if (frame == KeyFrames[i].Key)
                    {
                        // 记录当前帧
                        this.prevIndex = i;
                        // 返回当前帧
                        return CameraKeyFrameToCameraPose(KeyFrames[i].Value);
                    }
                    // 如果目标帧小于下一帧，则判定取当前帧与下一帧的插值
                    else if (frame < KeyFrames[i + 1].Key)
                    {
                        // 记录当前帧
                        this.prevIndex = i;
                        // 返回上一帧与下一帧的插值
                        if (newInterpolate)
                        {
                            return NewInterpolateCameraFrame(frame, KeyFrames[i], KeyFrames[i + 1]);
                        }
                        return InterpolateCameraFrame(frame, KeyFrames[i], KeyFrames[i + 1]);
                    }
                }
            }
            return null;
        }

        CameraPose NewInterpolateCameraFrame(float targetFrameNo, KeyValuePair<int, CameraKeyframe> prevFrameData, KeyValuePair<int, CameraKeyframe> nextFrameData)
        {
            var prevFrame = prevFrameData.Value;
            var nextFrame = nextFrameData.Value;
            if (prevFrameData.Key + 1 == nextFrameData.Key)
                return CameraKeyFrameToCameraPose(prevFrame);
            var tframe = (float)(targetFrameNo - prevFrameData.Key) / (float)(nextFrameData.Key - prevFrameData.Key);

            float tx, ty, tz, tr, td, tv;

            if (IsLinear(nextFrame.xCurve))
            {
                tx = tframe;
            }
            else
            {
                tx = InterpolateFrame(nextFrame.xCurve, tframe);
            }
            if (IsLinear(nextFrame.yCurve))
                ty = tframe;
            else
                ty = InterpolateFrame(nextFrame.yCurve, tframe);
            if (IsLinear(nextFrame.zCurve))
                tz = tframe;
            else
                tz = InterpolateFrame(nextFrame.zCurve, tframe);
            if (IsLinear(nextFrame.rCurve))
                tr = tframe;
            else
                tr = InterpolateFrame(nextFrame.rCurve, tframe);
            if (IsLinear(nextFrame.dCurve))
                td = tframe;
            else
                td = InterpolateFrame(nextFrame.dCurve, tframe);
            if (IsLinear(nextFrame.vCurve))
                tv = tframe;
            else
                tv = InterpolateFrame(nextFrame.vCurve, tframe);

            var nf = new CameraPose
            {
                FocalLength = prevFrame.FocalLength + (nextFrame.FocalLength - prevFrame.FocalLength) * td,
                Position = new Vector3(
                   prevFrame.Position.x + (nextFrame.Position.x - prevFrame.Position.x) * tx,
                   prevFrame.Position.y + (nextFrame.Position.y - prevFrame.Position.y) * ty,
                   prevFrame.Position.z + (nextFrame.Position.z - prevFrame.Position.z) * tz),
                Rotation = new Vector3(
                   prevFrame.Rotation.x + (nextFrame.Rotation.x - prevFrame.Rotation.x) * tr,
                   prevFrame.Rotation.y + (nextFrame.Rotation.y - prevFrame.Rotation.y) * tr,
                   prevFrame.Rotation.z + (nextFrame.Rotation.z - prevFrame.Rotation.z) * tr),
                Fov = prevFrame.Fov + (nextFrame.Fov - prevFrame.Fov) * tv
            };

            return nf;
        }

        CameraPose InterpolateCameraFrame(float targetFrameNo, KeyValuePair<int, CameraKeyframe> prevFrameData, KeyValuePair<int, CameraKeyframe> nextFrameData)
        {
            var prevFrame = prevFrameData.Value;
            var nextFrame = nextFrameData.Value;
            if (prevFrameData.Key + 1 == nextFrameData.Key)
                return CameraKeyFrameToCameraPose(prevFrame);
            var tframe = (float)(targetFrameNo - prevFrameData.Key) / (float)(nextFrameData.Key - prevFrameData.Key);

            var points = new float[6];//MoveX,MoveY,MoveZ,Rotate,Distance,Angle
            bool isLinear = false;
            for (var i = 0; i < 6; i++)
            {
                var p1 = new Vector3(prevFrame.Interpolation[i * 4], prevFrame.Interpolation[i * 4 + 2]);
                var p2 = new Vector3(prevFrame.Interpolation[i * 4 + 1], prevFrame.Interpolation[i * 4 + 3]);
                //线性插值
                if (p1.x == p1.y && p2.x == p2.y)
                {
                    points[i] = tframe;
                    isLinear = true;
                }
                else
                    points[i] = CalculBezierPointByTwo(tframe, p1, p2);
            }

            //如果只差一帧的话，强制用线性插值。
            if (prevFrameData.Key == nextFrameData.Key - 1)
            {
                for (var i = 0; i < 6; i++)
                {
                    points[i] = tframe;
                }
            }

            var x = prevFrame.Position.x + points[0] * (nextFrame.Position.x - prevFrame.Position.x);
            //x = (1-points[0])*prevFrame.Position.x+points[0]*nextFrame.Position.x

            var y = prevFrame.Position.y + points[1] * (nextFrame.Position.y - prevFrame.Position.y);
            var z = prevFrame.Position.z + points[2] * (nextFrame.Position.z - prevFrame.Position.z);

            var rx = prevFrame.Rotation.x + points[3] * (nextFrame.Rotation.x - prevFrame.Rotation.x);
            var ry = prevFrame.Rotation.y + points[3] * (nextFrame.Rotation.y - prevFrame.Rotation.y);
            var rz = prevFrame.Rotation.z + points[3] * (nextFrame.Rotation.z - prevFrame.Rotation.z);
            var focalLength = prevFrame.FocalLength + points[4] * (nextFrame.FocalLength - prevFrame.FocalLength);
            var fov = prevFrame.Fov + (nextFrame.Fov - prevFrame.Fov) * points[5];
            var pose = new CameraPose
            {
                FocalLength = focalLength,
                Fov = fov,
                Orthographic = prevFrame.Orthographic,
                Position = new Vector3(x, y, z),
                Rotation = new Vector3(rx, ry, rz)
            };

            //float tx, ty, tz, tr, td, tv;

            //if (IsLinear(nextFrame.xCurve))
            //{
            //    tx = tframe;
            //}
            //else
            //{
            //    tx = InterpolateFrame(nextFrame.xCurve, tframe);
            //}
            //if (IsLinear(nextFrame.yCurve))
            //    ty = tframe;
            //else
            //    ty = InterpolateFrame(nextFrame.yCurve, tframe);
            //if (IsLinear(nextFrame.zCurve))
            //    tz = tframe;
            //else
            //    tz = InterpolateFrame(nextFrame.zCurve, tframe);
            //if (IsLinear(nextFrame.rCurve))
            //    tr = tframe;
            //else
            //    tr = InterpolateFrame(nextFrame.rCurve, tframe);
            //if (IsLinear(nextFrame.dCurve))
            //    td = tframe;
            //else
            //    td = InterpolateFrame(nextFrame.dCurve, tframe);
            //if (IsLinear(nextFrame.vCurve))
            //    tv = tframe;
            //else
            //    tv = InterpolateFrame(nextFrame.vCurve, tframe);

            //var nf = new CameraPose();

            //nf.FocalLength = prevFrame.FocalLength + (nextFrame.FocalLength - prevFrame.FocalLength) * td;
            //nf.Position = new Vector3(
            //   prevFrame.Position.x + (nextFrame.Position.x - prevFrame.Position.x) * tx,
            //   prevFrame.Position.y + (nextFrame.Position.y - prevFrame.Position.y) * ty,
            //   prevFrame.Position.z + (nextFrame.Position.z - prevFrame.Position.z) * tz);
            //nf.Rotation = new Vector3(
            //   prevFrame.Rotation.x + (nextFrame.Rotation.x - prevFrame.Rotation.x) * tr,
            //   prevFrame.Rotation.y + (nextFrame.Rotation.y - prevFrame.Rotation.y) * tr,
            //   prevFrame.Rotation.z + (nextFrame.Rotation.z - prevFrame.Rotation.z) * tr);
            //nf.Fov = prevFrame.Fov + (nextFrame.Fov - prevFrame.Fov) * tv;

            //var dirty = false;
            //if (pose.Fov != nf.Fov)
            //{
            //    dirty = true;
            //    LogUtil.Log($"Fov:{pose.Fov}!={nf.Fov}");
            //}
            //if (pose.FocalLength != nf.FocalLength)
            //{
            //    dirty = true;

            //    LogUtil.Log($"FocalLength:{pose.FocalLength}!={nf.FocalLength}");
            //}
            //if (pose.Position != nf.Position)
            //{
            //    dirty = true;

            //    LogUtil.Log($"Position:{pose.Position}!={nf.Position}");
            //}
            //if (pose.Rotation != nf.Rotation)
            //{
            //    dirty = true;

            //    LogUtil.Log($"Rotation:{pose.Rotation}!={nf.Rotation}");
            //}
            //if (pose.Orthographic != nf.Orthographic)
            //{
            //    dirty = true;

            //    LogUtil.Log($"Orthographic:{pose.Orthographic}!={nf.Orthographic}");
            //}

            //if (dirty)
            //{
            //    LogUtil.Log($"-------------------------------------------------");
            //}
            //else
            //{
            //    count++;
            //    LogUtil.Log($"OK:{count}");
            //}

            return pose;
        }

        int count = 0;

        Vector2 SampleBezier(Vector2 handle1, Vector2 handle2, float t)
        {
            var zero = Vector2.zero;
            var full = new Vector2(1.0f, 1.0f);
            var v1 = Vector2.Lerp(zero, handle1, t);
            var v2 = Vector2.Lerp(handle1, handle2, t);
            var v3 = Vector2.Lerp(handle2, full, t);
            var v4 = Vector2.Lerp(v1, v2, t);
            var v5 = Vector2.Lerp(v2, v3, t);
            var v6 = Vector2.Lerp(v4, v5, t);
            return v6;
        }

        float Interpolate(double p1x, double p2x, double p1y, double p2y, float x)
        {
            var handle1 = new Vector2((float)p1x, (float)p1y);
            var handle2 = new Vector2((float)p2x, (float)p2y);
            var xtgt = x;
            var x0 = 0.0f;
            var x1 = 1.0f;
            var c = 0;
            Vector2 v;

            while (true)
            {
                v = SampleBezier(handle1, handle2, x);
                c += 1;
                if (c >= 10)
                    break;
                if (Mathf.Abs(xtgt - v.x) < 0.001f)
                    break;
                if (v.x > xtgt)
                {
                    x1 = x;
                    x = (x1 + x0) / 2;
                }
                else
                {
                    x0 = x;
                    x = (x1 + x0) / 2;
                }
            }
            return v.y;
        }
        float InterpolateFrame(Curve curve, float x)
        {
            return Interpolate(curve.a, curve.c, curve.b, curve.d, x);
        }
        bool IsLinear(Curve cuv)
        {
            return cuv.a == cuv.b && cuv.c == cuv.d;
        }
        #endregion
    }
}