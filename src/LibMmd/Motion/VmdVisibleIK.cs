using System;
using System.Collections.Generic;

namespace LibMMD.Motion
{
	public class VmdVisibleIK
	{
		public class IK
		{
			public bool Enable;
			public string IKName { get; set; }
			public IK()
			{
				IKName = "";
				Enable = true;
			}
		}

		public int FrameIndex;
		public bool Visible;

        public List<IK> IKList { get; set; }
		public VmdVisibleIK()
		{
			Visible = true;
			IKList = new List<IK>();
		}

	}

	public class VmdVisibleIKKey
	{
		public int FrameIndex;
		public int PreFrameIndex = -1;
		public class IK
		{
			public int IKBoneIndex;
			public bool Enable;
		}

		public bool Visible;

		public IK[] IKEnable;

		public VmdVisibleIKKey()
		{
			Visible = true;
			IKEnable = new IK[0];
		}
        public static VmdVisibleIKKey FromVmdVisibleIK(VmdVisibleIK vik, Dictionary<string, int> boneTable)
        {
            VmdVisibleIKKey vmdVisibleIKKey = new VmdVisibleIKKey();
            vmdVisibleIKKey.Visible = vik.Visible;
            List<IK> list = new List<IK>();
            for (int i = 0; i < vik.IKList.Count; i++)
            {
                string iKName = vik.IKList[i].IKName;
                if (boneTable.ContainsKey(iKName))
                {
                    list.Add(new IK
                    {
                        Enable = vik.IKList[i].Enable,
                        IKBoneIndex = boneTable[iKName]
                    });
                }
            }
            vmdVisibleIKKey.IKEnable = list.ToArray();
            vmdVisibleIKKey.FrameIndex = vik.FrameIndex;
            return vmdVisibleIKKey;
        }
    }

	public class VmdVisibleIKKeyList
	{
		public VmdVisibleIKKeyList()
        {
			m_keyList = new List<VmdVisibleIKKey>();
        }

		protected List<VmdVisibleIKKey> m_keyList;
		public int FindFrameIndex(int frameIndex)
		{
			var item = new VmdVisibleIKKey
			{
				FrameIndex = frameIndex
			};
			return  m_keyList.BinarySearch(item, new Find());
		}
		private class Find : IComparer<VmdVisibleIKKey>
		{
			public int Compare(VmdVisibleIKKey x, VmdVisibleIKKey y)
			{
				return x.FrameIndex - y.FrameIndex;
			}
		}
		public void Add(VmdVisibleIKKey key)
		{
			m_keyList.Add(key);
			int preFrameIndex = m_keyList.Count - 1;
			SetPreFrameIndex(preFrameIndex);
		}
		protected void SetPreFrameIndex(int ix)
		{
			int num = ix - 1;
			if (num >= 0)
			{
				m_keyList[ix].PreFrameIndex = m_keyList[num].FrameIndex;
			}
			else
			{
				m_keyList[ix].PreFrameIndex = -1;
			}
		}
		public void RemoveAt(int ix)
		{
			m_keyList.RemoveAt(ix);
			if (m_keyList.Count > 0 && m_keyList.Count > ix)
			{
				SetPreFrameIndex(ix);
			}
		}

		//取最近的那个帧数据
		public VmdVisibleIKKey GetState(int frameIndex)
		{
			int count = m_keyList.Count;
			int num = FindFrameIndex(frameIndex);
			if (num >= 0)
			{
				return m_keyList[num];
			}
			int num2 = ~num;
			if (num2 >= count)
			{
				return m_keyList[count - 1];
			}
			if (m_keyList[num2].PreFrameIndex == -1 || num2 <= 0)
			{
				return m_keyList[num2];
			}
			return m_keyList[num2 - 1];
		}
	}
}
