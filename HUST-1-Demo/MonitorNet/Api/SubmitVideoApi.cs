using System;

namespace MonitorNet
{
	public class SubmitVideoApi
	{
		private string url = "update_video";

		private byte[] m_btData;//（船号，参数）

		public SubmitVideoApi (byte[] btData)//构造函数
		{
			this.m_btData = btData;
		}

		public void Request()
		{
			try
			{
				string sJasonData = HttpHelper.HttpDataPost (Constant.BaseUrl + this.url, m_btData);
				Console.WriteLine (sJasonData);
			}
			catch
			{
				Console.WriteLine ("error: SubmitVideoApi");
			}
		}
	}
}