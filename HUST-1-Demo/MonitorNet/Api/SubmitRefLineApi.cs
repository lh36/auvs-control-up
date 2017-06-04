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
			string sPostData = "&instanceid=" + NetManager.Instance.GetInstanceID ().ToString () +
			                   "&shipid=" + this.m_Data.iShipID.ToString () +
			                   "flag=" + oData.flag.ToString () +
			                   "&posX=" + oData.posX.ToString () +
			                   "&posY=" + oData.posY.ToString () +
			                   "&radius=" + oData.radius.ToString ();
			var pointList = oData.points;
			sPostData += "&count=" + pointList.Count.ToString();
			if(pointList.Count != 0)
			{
				sPostData += "&points=";
				for(int i=1; i<pointList.Count; i++)
				{
					if(i != 1)
					{
						sPostData += ",";
					}

					sPostData += pointList [i].ToString ();
				}
			}

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

