using System;

namespace MonitorNet
{
	public class SubmitRefLineApi
	{
		private string url = "update_refline";

		private CSubmitData m_Data;//（船号，参数）

		public SubmitRefLineApi (CSubmitData oData)//构造函数
		{
			this.m_Data = oData;
		}

		public void Request()
		{
			RefLineData oData = this.m_Data.oParam as RefLineData;
			string sPostData = "flag=" + oData.flag.ToString () +
				"&posX=" + oData.posX.ToString () +
				"&posY=" + oData.posY.ToString () +
				"&radius=" + oData.radius.ToString () +
				"&instanceid=" + NetManager.Instance.GetInstanceID ().ToString () +
				"&shipid=" + this.m_Data.iShipID.ToString ();
			try
			{
				string sJasonData = HttpHelper.HttpPost (Constant.BaseUrl + this.url, sPostData);
				Console.WriteLine (sJasonData);
			}
			catch
			{
				Console.WriteLine ("error: SubmitRefLineApi");
			}
		}
	}
}

