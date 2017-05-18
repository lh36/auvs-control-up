using System;

namespace MonitorNet
{
	public class CreateInstanceApi
	{
		private string url = "start_instance";

		public CreateInstanceApi (InstanceData oData, Callback _callback)
		{
			string sPostData = "name=" + oData.Name +
				"&desp=" + oData.Desp +
				"&amount=" + oData.Amount.ToString () +
				"&shape=" + oData.Shape +
				"&time=" + oData.Time.ToString ();
			try
			{
				string sJasonData = HttpHelper.HttpPost (Constant.BaseUrl + this.url, sPostData);
				Console.WriteLine (sJasonData);
				NewInstanceJson paramJson = JsonTool.JsonToClass<NewInstanceJson> (sJasonData);

				if(paramJson.status == true)
				{
					NewInstanceResp resp = paramJson.resp;

					_callback (resp.id);
				}  
			}
			catch
			{
				Console.WriteLine ("error");
			}
		}
	}

	//JSON解析类
	public class NewInstanceJson
	{
		public bool status = false;
		public NewInstanceResp resp = null;
	}

	public class NewInstanceResp
	{
		public int id = 0;
	}
}

