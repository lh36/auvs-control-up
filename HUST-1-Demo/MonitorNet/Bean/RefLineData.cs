using System;
using System.Collections.Generic;

namespace MonitorNet
{
	public class RefLineData
	{
		public int flag = 0;
		public double posX = 0;
		public double posY = 0;
		public double radius = 0;
		public List<double> points = new List<double>();

		/// <summary>
		/// 开环参考线数据
		/// </summary>
		public RefLineData(){}

		/// <summary>
		/// 闭环直线参考线数据
		/// </summary>
		/// <param name="flag">Flag=1.</param>
		/// <param name="posY">Position y.</param>
		public RefLineData(int flag, double posY)
		{
			this.flag = flag;
			this.posY = posY;
		}

		/// <summary>
		/// 闭环圆参考线数据
		/// </summary>
		/// <param name="flag">Flag=2.</param>
		/// <param name="posX">Position x.</param>
		/// <param name="posY">Position y.</param>
		/// <param name="radius">Radius.</param>
		public RefLineData(int flag, double posX, double posY, double radius)
		{
			this.flag = flag;
			this.posX = posX;
			this.posY = posY;
			this.radius = radius;
		}

		/// <summary>
		/// 直线参考线数据
		/// </summary>
		/// <param name="flag">3：简单直线，4：多段直线</param>
		/// <param name="points">Points.</param>
		public RefLineData(int flag, List<double> points)
		{
			this.flag = flag;
			this.points = points;
		}

		/// <summary>
		/// 目标点参考线数据
		/// </summary>
		/// <param name="flag">Flag=5.</param>
		/// <param name="posX">Position x.</param>
		/// <param name="posY">Position y.</param>
		/// <param name="radius">Radius.</param>
		public RefLineData(int flag, double posX, double posY, double radius)
		{
			this.flag = flag;
			this.posX = posX;
			this.posY = posY;
		}
	}
}

