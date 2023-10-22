using System.Collections;
using UnityEngine;

namespace mmd2timeline
{
    internal partial class MotionHelper
    {
        /// <summary>
        /// 准备中
        /// </summary>
        bool prepareToReady = false;

        /// <summary>
        /// 准备时悬空高度
        /// </summary>
        const float READY_HEIGHT = 0.1f;

        /// <summary>
        /// 当前准备动作位置Y
        /// </summary>
        float readyPositionY = 0.0f;

        /// <summary>
        /// 获得人物动作是否准备完毕
        /// </summary>
        internal bool IsReady
        {
            get
            {
                return !prepareToReady;
            }
        }

        /// <summary>
        /// 指示动作准备完毕
        /// </summary>
        internal IEnumerator Ready()
        {
            if (!prepareToReady)
            {
                yield break;
            }

            this.Prepare();

            this.UpdateEnableHighHeel();

            var perStep = READY_HEIGHT / 5f;

            yield return new WaitWhile(() =>
            {
                readyPositionY -= perStep;
                //this._PositionY.val -= perStep;

                return readyPositionY >= perStep;
            });
            yield return null;
            readyPositionY = 0.0f;
            prepareToReady = false;
        }

        /// <summary>
        /// 预备动作
        /// </summary>
        internal IEnumerator MakeReady()
        {
            if (prepareToReady)
            {
                yield break;
            }
            prepareToReady = true;

            if (readyPositionY <= 0.0f)
            {
                yield return null;
                readyPositionY = READY_HEIGHT;
                //this._PositionY.val += readyPositionY;
            }
        }
    }
}
