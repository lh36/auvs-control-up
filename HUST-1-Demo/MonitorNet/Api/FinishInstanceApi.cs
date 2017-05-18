using System;

namespace MonitorNet
{
	public class FinishInstanceApi
	{
		private string url = "finish_instance";

		public FinishInstanceApi (int iInstanceID, long lTime, Callback _callback)
		{
			string sPostData = "id=" + iInstanceID.ToString() +
				"&time=" + lTime.ToString ();
			try
			{
				string sJasonData = HttpHelper.HttpPost (Constant.BaseUrl + this.url, sPostData);
				Console.WriteLine (sJasonData);
				FinishInstanceJson json = JsonTool.JsonToClass<FinishInstanceJson> (sJasonData);

				_callback(json.status);
			}
			catch
			{
				Console.WriteLine ("error");
			}
		}
	}

	//JSON解析类
	public class FinishInstanceJson
	{
		public bool status = false;
		public object resp = null;
	}
}

