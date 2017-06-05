using System;

namespace MonitorNet
{
	public class CreateInstanceApi
	{
		private string url = "start_instance";

        private InstanceData m_InstanceData;
        private Callback _callback;

		public CreateInstanceApi (InstanceData oData, Callback _callback)
		{
            this.m_InstanceData = oData;
            this._callback = _callback;
		}

        public void Request()
        {
            string sPostData = "name=" + this.m_InstanceData.Name +
                "&desp=" + this.m_InstanceData.Desp +
                "&amount=" + this.m_InstanceData.Amount.ToString () +
                "&shape=" + this.m_InstanceData.Shape +
                "&time=" + this.m_InstanceData.Time.ToString ();
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
				Console.WriteLine ("error: CreateInstanceApi");
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

