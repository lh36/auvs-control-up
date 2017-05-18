using System;
using System.Collections.Generic;
using System.Threading;

namespace MonitorNet
{
	public class NetManager : Singleton<NetManager>
	{
		private int m_iInstanceID = 0;

		private int m_iSubmitLimit = 0;

		public NetManager ()
		{
		}

        /// <summary>
        /// 请求生成一个新的实例
        /// </summary>
        /// <param name="oData">实例信息</param>
		public void NetCreateNewInstance(InstanceData oData)
		{
			var oApi = new CreateInstanceApi (oData, this.SetInstanceID);
		}

		private void SetInstanceID(object oSender)
		{
			this.m_iInstanceID = (int)oSender;
		}

        /// <summary>
        /// 结束本实例
        /// </summary>
        /// <param name="lTime">Unix时间戳.</param>
        /// <param name="_callback">回调处理函数.</param>
		public void NetFinishInstance(long lTime, Callback _callback)
		{
			var oApi = new FinishInstanceApi (this.m_iInstanceID, lTime, _callback);
		}

        /// <summary>
        /// 发送实验数据
        /// </summary>
        /// <param name="iShipID">船舶id.</param>
        /// <param name="oParam">数据</param>
		public void NetSubmitParam(int iShipID, SShipParam oParam)
		{
			if (this.m_iSubmitLimit >= 10)
			{
				return;
			}
			this.m_iSubmitLimit += 1;
			CSubmitData oData = new CSubmitData ();
			oData.iShipID = iShipID;
			oData.oParam = oParam;
			Thread oThread = new Thread (SubmitThread);
			oThread.Start (oData);
		}

		private void SubmitThread(object oData)
		{
			CSubmitData oApiData = oData as CSubmitData;
			SubmitParamApi oApi = new SubmitParamApi (oApiData, this.ApiDone);
		}

		private void ApiDone(object oSender)
		{
			this.m_iSubmitLimit -= 1;
		}

        /// <summary>
        /// 获取控制信息
        /// </summary>
        /// <param name="_callback">回调处理函数</param>
		public void NetGetControlData(Callback _callback)
		{
			Thread oThread = new Thread (GetControlThread);
			oThread.Start (_callback);
		}

		private void GetControlThread(object _callback)
		{
			GetControlApi oApi = new GetControlApi (this.m_iInstanceID, _callback as Callback);
		}

		/// <summary>
        /// 获取实例id
        /// </summary>
		public int GetInstanceID()
		{
			return this.m_iInstanceID;
		}
	}

	public class CSubmitData
	{
		public int iShipID;
		public SShipParam oParam;
	}

	public delegate void Callback(object oSender);
}

