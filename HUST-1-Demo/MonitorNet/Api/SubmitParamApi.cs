using System;

namespace MonitorNet
{
	public class SubmitParamApi
	{
		private string url = "update_param";

		public SubmitParamApi (CSubmitData oData, Callback _callback)
		{
			SShipParam oParam = oData.oParam;
			string sPostData = "lat=" + oParam.lat.ToString() +
			                   "&lon=" + oParam.lon.ToString () +
			                   "&posX=" + oParam.posX.ToString () +
			                   "&posY=" + oParam.posY.ToString () +
			                   "&rud=" + oParam.rud.ToString () +
			                   "&phi=" + oParam.phi.ToString () +
			                   "&speed=" + oParam.speed.ToString () +
			                   "&gear=" + oParam.gear.ToString () +
			                   "&time=" + oParam.time.ToString () +
							   "&instanceid=" + NetManager.Instance.GetInstanceID().ToString () +
			                   "&shipid=" + oData.iShipID.ToString ();
			try
			{
				string sJasonData = HttpHelper.HttpPost (Constant.BaseUrl + this.url, sPostData);
				Console.WriteLine (sJasonData);
			}
			catch
			{
				Console.WriteLine ("error");
			}
			_callback (null);
		}


	}
}

