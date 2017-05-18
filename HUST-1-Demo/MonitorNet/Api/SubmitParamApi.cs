using System;

namespace MonitorNet
{
	public class SubmitParamApi
	{
		private string url = "update_param";

        private CSubmitData m_Data;
        private Callback _callback;

		public SubmitParamApi (CSubmitData oData, Callback _callback)
		{
            this.m_Data = oData;
            this._callback = _callback;
		}

        public void Request()
        {
            SShipParam oParam = this.m_Data.oParam;
            string sPostData = "lat=" + oParam.lat.ToString () +
                "&lon=" + oParam.lon.ToString () +
                "&posX=" + oParam.posX.ToString () +
                "&posY=" + oParam.posY.ToString () +
                "&rud=" + oParam.rud.ToString () +
                "&phi=" + oParam.phi.ToString () +
                "&gps_phi=" + oParam.GPS_Phi.ToString () +
                "&speed=" + oParam.speed.ToString () +
                "&gear=" + oParam.gear.ToString () +
                "&time=" + oParam.time.ToString () +
                "&kp=" + oParam.Kp.ToString () +
                "&ki=" + oParam.Ki.ToString () +
                "&kd=" + oParam.Kd.ToString () +
                "&k1=" + oParam.K1.ToString () +
                "&k2=" + oParam.K2.ToString () +
                "&instanceid=" + NetManager.Instance.GetInstanceID ().ToString () +
                "&shipid=" + this.m_Data.iShipID.ToString ();
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

