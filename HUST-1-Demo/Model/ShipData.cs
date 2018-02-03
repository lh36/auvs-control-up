using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Drawing;
namespace HUST_1_Demo.Model
{
    public class ShipData
    {
        public byte ShipID { get; set; }//船号
        public double Lat { get; set; }//纬度
        public double Lon { get; set; }//经度
        public double LLat { get; set; }//纬度
        public double LLon { get; set; }//经度
        public double pos_X { get; set; }//当前X坐标，此为测量坐标系下的坐标，北向为X，东向为Y
        public double Fter_pos_X { get; set; }
        public double pos_Y { get; set; }//Y坐标
        public double Fter_pos_Y { get; set; }
        public double XError { get; set; }//编队误差，与leader的跟随误差
        public double fError { get; set; }//轨迹跟随误差
        public double last_pos_X { get; set; }//上一坐标值
        public double last_pos_Y { get; set; }//Y坐标
        public double X_mm { get; set; }//图上X坐标
        public double Y_mm { get; set; }//图上Y坐标
        public double Ctrl_Phi { get; set; }//当前用于控制采用的状态航向（航迹角或者航向角）
        public double L_Ctrl_Phi { get; set; }//上一次航迹角/航向角
        public double GPS_Phi { get; set; }//原始GPS-Phi航迹角
        public double Fter_GPS_Phi { get; set; }//滤波后的航迹角
        public float phi { get; set; }//航向角惯导
        public float Init_Phi { get; set; }//惯导原始数据，由于进行惯导0°初始化时需要获取当前惯导原始数据，所以这里需要保存惯导原始数据
        public float Phi_buchang { get; set; }//惯导补偿值
        public float Pos_Phi { get; set; }//位置坐标角（极坐标，范围[-180, 180])
        public float rud { get; set; }//舵角
        public float speed { get; set; }//船速
        public long lTime { get; set; }//long类型时间戳
        public string sTime { get; set; }//string类型时间戳
        public int gear { get; set; }//速度档
        public int CtrlRudOut { get; set; }//舵角控制输出量
        public int CtrlSpeedOut { get; set; }//速度控制输出量
        public double e1 { get; set; }//Rise 控制中间量
        public double e2 { get; set; }//Rise 控制中间量
        public double e20 { get; set; }//Rise 控制中间量
        public double Vf { get; set; }//Rise 控制积分项
        public double F2 { get; set; }
        public double Err_phi_In { get; set; }//积分控制中的积分量

        //传感器值
        public int MotorSpd1 { get; set; }//左电机转速
        public int MotorSpd2 { get; set; }//右电机转速
        public double Water_Tem { get; set; }//水温度
        public double Water_Tur { get; set; }//水浊度
        public double Water_PH { get; set; }//水PH值
        public int Water_Con { get; set; }//水电导率
        public double Water_Oxy { get; set; }//水溶解氧
        public int Dzhangai { get; set; }//障碍物49-51


        ////3个障碍物扇区，数组中每行表示一个扇区，分别表示扇区低度边沿和高度边沿、扇形半径。
        //public int[,] obstacle = new int[3, 3] { { 0, 0, 0 }, { 0, 0, 0 }, { 0, 0, 0 } };


        //控制参数
        public float Kp { get; set; }
        public float Ki { get; set; }
        public float Kd { get; set; }
        public float K1 { get; set; }
        public float K2 { get; set; }
        public float dx_err { get; set; }
        public float dy_err { get; set; }
        public bool m_bIsClsCtrStopped { get; set; }// 点跟踪和直线跟踪是否停止标志
        public int m_iMulLineNum { get; set; }//跟随多段直线当前编号


        public static double a = 6378137.0;//定义地球长半轴长度  
        public static double earth_e = 0.003352810664; //定义椭球的第一偏心律

  //      public static double lat_start = 31.402835;//定义原点位置
   //     public static double lon_start = 121.348840;

        //public static double lat_start = 30.51582550;//定义原点位置
        //public static double lon_start = 114.426780000;

        public static double lat_start = 30.513891333;//定义原点位置
        public static double lon_start = 114.409441333;


     //   public double[] FilterLat = new double[10] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };
    //    public double[] FilterLon = new double[10] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };
        public double[] tempFterGPSPhi = new double[5] { 0, 0, 0, 0, 0 };
        public double[] tempFterPosX = new double[5] { 0, 0, 0, 0, 0 };
        public double[] tempFterPosY = new double[5] { 0, 0, 0, 0, 0 };
        public double Filter(double[] Lvbobuf)
        {
            double average, max, min, sum = 0.0d;
            max = Lvbobuf[0];
            min = Lvbobuf[0];
            for (int i = 0; i < Lvbobuf.Length; i++)
            {
                if (max < Lvbobuf[i])
                    max = Lvbobuf[i];
                if (min > Lvbobuf[i])
                    min = Lvbobuf[i];
                sum = sum + Lvbobuf[i];
            }
            average = (sum - max - min) / (Lvbobuf.Length - 2);
            return average;
        }

        public static double[] GPS2XY(double[] GPSData)
        {
            double[] Pt = new double[2] { 0.0d, 0.0d };
            Pt[0] = ((GPSData[0] - lat_start) * a * (1 - Math.Pow(earth_e, 2)) * 3.1415926 / (180 * Math.Sqrt(Math.Pow((1 - Math.Pow(earth_e * Math.Sin(GPSData[0] / 180 * 3.1415926), 2)), 3))));
            Pt[1] = -((GPSData[1] - lon_start) * a * Math.Cos(GPSData[0] / 180 * 3.1415926) * 3.1415926 / (180 * Math.Sqrt(1 - Math.Pow(earth_e * Math.Sin(GPSData[0] / 180 * 3.1415926), 2))));//Y坐标正向朝西
            return Pt;
        }

        /// <summary>
        /// 更新船舶状态信息
        /// </summary>
        /// <param name="response_data"></param>
        public void UpdataStatusData(byte[] response_data)
        {
            ShipID = response_data[3];
            Lat = ((response_data[4] << 24) + (response_data[5] << 16) + (response_data[6] << 8) + response_data[7]) / Math.Pow(10, 8) + 30;
            Lon = ((response_data[8] << 24) + (response_data[9] << 16) + (response_data[10] << 8) + response_data[11]) / Math.Pow(10, 8) + 114;
           
            double[] tempGPS = new double[2] { Lat, Lon };
            double[] tempXY = GPS2XY(tempGPS);
            
            pos_X = tempXY[0];
            pos_Y = tempXY[1];
            GPS_Phi = Math.Atan2(pos_Y - last_pos_Y, pos_X - last_pos_X) / Math.PI * 180;//航迹角

            //将临时存储区向前移位
            for (int i = 0; i < 4; i++)
            {
                tempFterGPSPhi[i] = tempFterGPSPhi[i + 1];
                tempFterPosX[i] = tempFterPosX[i + 1];
                tempFterPosY[i] = tempFterPosY[i + 1];
            }
            tempFterGPSPhi[4] = GPS_Phi;//存入最新值  
            tempFterPosX[4] = pos_X;
            tempFterPosY[4] = pos_Y;

            Fter_GPS_Phi = Filter(tempFterGPSPhi);//对航迹角滤波
            double x = Filter(tempFterPosX);
            double y = Filter(tempFterPosY);

            if (Math.Abs(tempFterPosX[4] - tempFterPosX[3]) < 1)//0.2s内不可能走这么多，正常的数据才去更新，否则不更新
            {
                Fter_pos_X = x;//对x坐标滤波
            }

            if (Math.Abs(tempFterPosY[4] - tempFterPosY[3]) < 1)
            {
                Fter_pos_Y = y;//对y坐标滤波
            }
           
            last_pos_X = pos_X;//更新上一次坐标信息
            last_pos_Y = pos_Y;

            X_mm = Fter_pos_X * 1000;
            Y_mm = Fter_pos_Y * 1000;

            this.lTime = HUST_1_Demo.Form1.GetTimeStamp();
            
            DateTime dtStart = TimeZone.CurrentTimeZone.ToLocalTime(new DateTime(1970, 1, 1));
            long lTime = long.Parse(this.lTime.ToString() + "0000000");
            TimeSpan toNow = new TimeSpan(lTime);
            this.sTime = dtStart.Add(toNow).ToString();


            speed = ((response_data[15] << 8) + (response_data[16])) / 1000.0f;

            byte[] Euler_Z = new byte[4];
            Euler_Z[0] = response_data[17];
            Euler_Z[1] = response_data[18];
            Euler_Z[2] = response_data[19];
            Euler_Z[3] = response_data[20];
            Init_Phi = BitConverter.ToSingle(Euler_Z, 0);
            phi = Init_Phi + Phi_buchang;
            if (phi > 180) phi = phi - 360;
            if (phi < -180) phi = phi + 360;
            rud = (float)((response_data[21] - 32) * 0.9);//小船舵角信息
            if (response_data[22] == 0) gear = response_data[22];
            else gear = response_data[22];

            Dzhangai = response_data[27];//障碍物信息49-51

            Water_Tem = ((response_data[28] << 8) + response_data[29]) * 0.0625;//水温度
            Water_PH=((response_data[30] << 8) + response_data[31]) / 1000.00;//水PH
            Water_Oxy = ((response_data[32] << 8) + response_data[33]);//水溶解氧
            Water_Tur = ((response_data[34] << 8) + response_data[35]) / 1000.00;//水浊度
            Water_Con = ((response_data[36] << 8) + response_data[37]);//电导率
        }
        
        public void SubmitParamToServer()
        {
            HUST_1_Demo.Form1.sendcnt++;
            var oShipParam = new MonitorNet.SShipParam();
            
            oShipParam.lat = this.Lat;
            oShipParam.lon = this.Lon;
            oShipParam.posX = this.Fter_pos_X;
            oShipParam.posY = this.Fter_pos_Y;

            oShipParam.fError = this.fError;

            oShipParam.phi = this.Ctrl_Phi;
            oShipParam.GPS_Phi = this.GPS_Phi;

            oShipParam.rud = this.rud;
            oShipParam.speed = this.speed;
            oShipParam.gear = this.gear;
            oShipParam.time = this.lTime;

            oShipParam.Kp = this.Kp;
            oShipParam.Ki = this.Ki;
            oShipParam.Kd = this.Kd;
            oShipParam.K1 = this.K1;
            oShipParam.K2 = this.K2;

            oShipParam.tem = this.Water_Tem;
            oShipParam.ph = this.Water_PH;
            oShipParam.diso = this.Water_Oxy;
            oShipParam.tur = this.Water_Tur;
            oShipParam.con = this.Water_Con;
            oShipParam.check_value = oShipParam.lat + oShipParam.lon + oShipParam.posX + oShipParam.posY + oShipParam.phi +
                                             oShipParam.GPS_Phi + oShipParam.rud + oShipParam.speed + oShipParam.Kp + oShipParam.Ki +
                                             oShipParam.Kd + oShipParam.K1 + oShipParam.K2 + oShipParam.tem + oShipParam.ph +
                                             oShipParam.diso + oShipParam.tur + oShipParam.con;

            MonitorNet.NetManager.Instance.NetSubmitParam((int)this.ShipID, oShipParam);
        }

        /// <summary>
        /// 存储船舶状态信息
        /// </summary>
        /// <param name="fileName"></param>
        public void StoreShipData(string fileName, DataTable dataRec)
        {
            HUST_1_Demo.Form1.storecnt++;
            /*using (FileStream fs = new FileStream(@"D:\" + fileName + ".txt", FileMode.Append))
            {
                //数据保存信息量为：
                //船号，纬度，经度，X坐标(m)，Y坐标，和领队误差，航向角，航迹角，速度，速度等级，时间
                //在速度等级后面增加舵角信息，舵角控制输出量信息和速度控制输出量信息
                //共13个存储量
                string str_data = ShipID.ToString() + "," + Lat.ToString("0.00000000") + "," + Lon.ToString("0.00000000") + ","
                                + pos_X.ToString("0.000") + "," + pos_Y.ToString("0.000") + ","
                                + XError.ToString("0.000") + ","
                                + phi.ToString("0.0") + "," + GPS_Phi.ToString("0.0") + ","
                                + speed.ToString("0.00") + "," + gear.ToString() + "," + rud.ToString("0.0") + ','
                                + CtrlRudOut.ToString() + ',' + CtrlSpeedOut.ToString() + ','
                                + Time.ToString();//将数据转换为字符串

                byte[] data = System.Text.Encoding.Default.GetBytes(str_data);
                byte[] data3 = new byte[2];
                data3[0] = 0x0d; data3[1] = 0x0a;
                //开始写入
                fs.Write(data, 0, data.Length);

                fs.Write(data3, 0, data3.Length);

                //清空缓冲区、关闭流
                fs.Flush();
                fs.Close();
            }*/
            /*gridView.Rows.Add(ShipID.ToString(), Lat.ToString("0.00000000"), Lon.ToString("0.00000000"),
                pos_X.ToString("0.000"), pos_Y.ToString("0.000"), XError.ToString("0.000"),
                phi.ToString("0.0"), GPS_Phi.ToString("0.0"),
                speed.ToString("0.00"), gear.ToString(),
                rud.ToString("0.0"), CtrlRudOut.ToString(), CtrlSpeedOut.ToString(),
                Time.ToString());*/
            if (HUST_1_Demo.Form1.isRealMode)
            {
                dataRec.Rows.Add(new object[] { ShipID.ToString(), Lat.ToString("0.00000000"), Lon.ToString("0.00000000"),
                Fter_pos_X.ToString("0.0000"), Fter_pos_Y.ToString("0.0000"), XError.ToString("0.000"),
                phi.ToString("0.0"), GPS_Phi.ToString("0.0"),Fter_GPS_Phi.ToString("0.0"),
                speed.ToString("0.000"), gear.ToString(),
                rud.ToString("0.0"), CtrlRudOut.ToString(), CtrlSpeedOut.ToString(),
                e1.ToString(),e2.ToString(),Vf.ToString(),F2.ToString(),
                MotorSpd1.ToString(),
                this.m_iMulLineNum.ToString(),//多段直线ID戳
                sTime.ToString() });
            }
            else
            {
                dataRec.Rows.Add(new object[] { ShipID.ToString(), Fter_pos_X.ToString("0.0000"), Fter_pos_Y.ToString("0.0000"), this.Ctrl_Phi.ToString("0.0") });
            }
        }

    }
}
