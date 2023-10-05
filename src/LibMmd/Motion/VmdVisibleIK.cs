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
}
