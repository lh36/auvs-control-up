﻿using System;
using System.Collections.Generic;
using System.Threading;

namespace MonitorNet
{
	public class NetManager : Singleton<NetManager>
	{
		private int m_iInstanceID = 0;

		private int m_iSubmitLimit = 0;

		private bool m_bIsVideoIdle = true;

        private GetControlApi m_GetControlApi;

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
            oApi.Request ();
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
            oApi.Request ();
		}

        /// <summary>
        /// 发送实验数据
        /// </summary>
        /// <param name="iShipID">船舶id.</param>
        /// <param name="oParam">数据</param>
		public void NetSubmitParam(int iShipID, SShipParam oParam)//上传状态参数
		{
			if (this.m_iSubmitLimit >= 10)
			{
				return;
			}
			this.m_iSubmitLimit += 1;
			CSubmitData oData = new CSubmitData ();
			oData.iShipID = iShipID;
			oData.oParam = oParam;
			Thread oThread = new Thread (SubmitParamThread);
			oThread.Start (oData);
		}

		private void SubmitParamThread(object oData)
		{
			CSubmitData oApiData = oData as CSubmitData;
			SubmitParamApi oApi = new SubmitParamApi (oApiData, this.ApiDone);
            oApi.Request ();
		}

		private void ApiDone(object oSender)
		{
			this.m_iSubmitLimit -= 1;
		}

		/// <summary>
		/// 发送参考线数据
		/// </summary>
		/// <param name="iShipID">船舶id.</param>
		/// <param name="oParam">数据</param>
		public void NetSubmitRefLine(int iShipID, RefLineData oParam)
		{
			CSubmitData oData = new CSubmitData ();
			oData.iShipID = iShipID;
			oData.oParam = oParam;
			Thread oThread = new Thread (SubmitRefLineThread);
			oThread.Start (oData);
		}

		private void SubmitRefLineThread(object oData)
		{
			CSubmitData oApiData = oData as CSubmitData;
			SubmitRefLineApi oApi = new SubmitRefLineApi (oApiData);
			oApi.Request ();
		}

		/// <summary>
		/// 发送视频数据
		/// </summary>
		/// <param name="oParam">数据</param>
		public void NetSubmitVideo(byte[] btData)
		{
			if(! this.m_bIsVideoIdle)
			{
				return;
			}
			this.m_bIsVideoIdle = false;
			Thread oThread = new Thread (SubmitVideoThread);
			oThread.Start (btData);
		}

		private void SubmitVideoThread(object oData)
		{
			SubmitVideoApi oApi = new SubmitVideoApi ((byte[])oData, this.VideoApiDone);
			oApi.Request ();
		}

		private void VideoApiDone(object oSender)
		{
			this.m_bIsVideoIdle = true;
		}

        /// <summary>
        /// 获取控制信息
        /// </summary>
        /// <param name="_callback">回调处理函数</param>
		public void NetGetControlData(Callback _callback)
		{
			Thread oThread = new Thread (GetControlThread);
            oThread.IsBackground = true;
			oThread.Start (_callback);
		}

		private void GetControlThread(object _callback)
		{
			this.m_GetControlApi = new GetControlApi (this.m_iInstanceID, _callback as Callback);
            this.m_GetControlApi.Request ();
		}

        public void FinishControlRequest()
        {
            this.m_GetControlApi.SetRunningStatus (false);
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
		public object oParam;
	}


	public delegate void Callback(object oSender);
}

