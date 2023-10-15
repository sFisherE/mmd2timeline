using mmd2timeline.Store;
using System;
using System.Collections.Generic;
using System.Linq;

namespace mmd2timeline
{
    internal partial class Player
    {
        internal delegate void CurrentItemChangedHandle(MMDEntity entity, Action<MMDEntity, bool> confirm);

        internal event CurrentItemChangedHandle OnCurrentItemChanged;
        internal event CurrentItemChangedHandle OnCurrentItemChangedNoConfirm;

        /// <summary>
        /// 等待确认的委托清单
        /// </summary>
        Dictionary<CurrentItemChangedHandle, bool> waitingForConfirm = new Dictionary<CurrentItemChangedHandle, bool>();

        bool _WaitingForPlay = false;

        /// <summary>
        /// 发出MMD选定的通知
        /// </summary>
        /// <param name="entity"></param>
        void NotifyMMDSelected(MMDEntity entity)
        {
            _IsLoading = true;

            waitingForConfirm.Clear();

            var delegates = new Delegate[0];

            if (OnCurrentItemChanged != null)
            {
                delegates = OnCurrentItemChanged.GetInvocationList();
            }

            if (delegates.Length > 0)
            {
                // 如果是正在播放状态，停止播放
                if (this.IsPlaying)
                {
                    _WaitingForPlay = true;
                }

                // 将委托加载到等待队列
                foreach (CurrentItemChangedHandle delegator in delegates)
                {
                    waitingForConfirm[delegator] = false;
                }
            }

            OnMMDSelectedConfirm(entity);
        }

        /// <summary>
        /// 委托确认选中事件
        /// </summary>
        /// <param name="entity"></param>
        /// <param name="sender"></param>
        /// <param name="confirm"></param>
        void OnMMDSelectedConfirm(MMDEntity entity, CurrentItemChangedHandle sender = null, bool confirm = false)
        {
            LogUtil.Debug($"Player::OnMMDSelectedConfirm:{entity}");

            if (sender != null)
            {
                if (waitingForConfirm.ContainsKey(sender))
                {
                    waitingForConfirm[sender] = confirm;
                }
            }

            var nextWaitingConfirm = waitingForConfirm.Where(c => !c.Value).Select(c => c.Key).FirstOrDefault();

            if (nextWaitingConfirm != null)
            {
                nextWaitingConfirm.Invoke(entity, (e, b) => OnMMDSelectedConfirm(entity, nextWaitingConfirm, b));
            }
            else
            {
                // 没有需要确认的委托了，清空列表
                waitingForConfirm.Clear();

                if (entity.NeedSave)
                {
                    entity.Save();
                }

                // 播放entity
                StartCoroutine(PlayMMD(entity));
            }
        }
    }
}
