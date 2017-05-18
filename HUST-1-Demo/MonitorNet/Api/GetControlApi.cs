using System;

namespace MonitorNet
{
	public class GetControlApi
	{
		private string url = "get_control/";

		public GetControlApi (int iInstanceID, Callback _callback)
		{
			Request (iInstanceID, _callback);
		}

		private void Request(int iInstanceID, Callback _callback)
		{
			try
			{
				string sControlData = HttpHelper.HttpGet (Constant.BaseUrl + this.url + iInstanceID.ToString());
				Console.WriteLine (sControlData);

				_callback (sControlData);
			}
			catch
			{
				Console.WriteLine ("error");
			}
			finally
			{
				Request (iInstanceID, _callback);
			}
		}
	}
}

