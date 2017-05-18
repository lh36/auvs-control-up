using System;

namespace MonitorNet
{
	public class GetControlApi
	{
		private string url = "get_control/";

        private bool m_bIsRunning = true;
        private int m_iInstanceID = 0;
        private Callback _callback;

		public GetControlApi (int iInstanceID, Callback _callback)
		{
            this.m_iInstanceID = iInstanceID;
            this._callback = _callback;
		}

		public void Request()
		{
			try
			{
				string sControlData = HttpHelper.HttpGet (Constant.BaseUrl + this.url + this.m_iInstanceID.ToString());
				Console.WriteLine (sControlData);

				_callback (sControlData);
			}
			catch
			{
				Console.WriteLine ("error");
			}
			finally
			{
                if(this.m_bIsRunning)
                {
                    Request ();
                }
			}
		}

        public void SetRunningStatus(bool bStatus)
        {
            this.m_bIsRunning = bStatus;
        }
	}
}

