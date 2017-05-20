using System;

namespace MonitorNet
{
	public class FinishInstanceApi
	{
		private string url = "finish_instance";

        private int m_InstanceID = 0;
        private long m_Time = 0;
        private Callback _callback;

		public FinishInstanceApi (int iInstanceID, long lTime, Callback _callback)
		{
            this.m_InstanceID = iInstanceID;
            this.m_Time = lTime;
            this._callback = _callback;
		}

        public void Request()
        {
            string sPostData = "id=" + this.m_InstanceID.ToString() +
                "&time=" + this.m_Time.ToString ();
            try
            {
                string sJasonData = HttpHelper.HttpPost (Constant.BaseUrl + this.url, sPostData);
                Console.WriteLine (sJasonData);
                FinishInstanceJson json = JsonTool.JsonToClass<FinishInstanceJson> (sJasonData);

                _callback(json.status);
            }
            catch
            {
				Console.WriteLine ("error: FinishInstanceApi");
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

