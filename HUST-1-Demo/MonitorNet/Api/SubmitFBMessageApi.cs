using System;

namespace MonitorNet
{
    class SubmitFBMessageApi
    {
        private string url = "update_fbmessage";

		private string m_sMessage;//状态信息

        public SubmitFBMessageApi(string sMessage)//构造函数
		{
            this.m_sMessage = sMessage;
		}

		public void Request()
		{
			try
			{
                string sJasonData = HttpHelper.HttpPost(Constant.BaseUrl + this.url, m_sMessage);
				Console.WriteLine (sJasonData);
			}
			catch
			{
				Console.WriteLine ("error: SubmitVideoApi");
			}
		}
    }
}
