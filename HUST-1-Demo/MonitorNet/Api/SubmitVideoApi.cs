using System;

namespace MonitorNet
{
	public class SubmitVideoApi
	{
		private string url = "update_video";

		private byte[] m_btData;//（船号，参数）
		private Callback _callback;

		public SubmitVideoApi (byte[] btData, Callback _callback)//构造函数
		{
			this.m_btData = btData;
			this._callback = _callback;
		}

		public void Request()
		{
			try
			{
				string sJasonData = HttpHelper.HttpDataPost (Constant.BaseUrl + this.url, m_btData);
				_callback(null);
				Console.WriteLine (sJasonData);
			}
			catch
			{
				Console.WriteLine ("error: SubmitVideoApi");
			}
		}
	}
}