using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace HUST_1_Demo.Model
{
    public class ShipData
    {
        public byte ShipID { get; set; }//船号
        public double Lat { get; set; }//纬度
        public double Lon { get; set; }//经度
        public float pos_X { get; set; }//当前X坐标，此为测量坐标系下的坐标，北向为X，东向为Y
        public float pos_Y { get; set; }//Y坐标
        public float XError { get; set; }//编队误差，与leader的跟随误差
        public float last_pos_X { get; set; }//上一坐标值
        public float last_pos_Y { get; set; }//Y坐标
        public float X_mm { get; set; }//图上X坐标
        public float Y_mm { get; set; }//图上Y坐标
        public float Control_Phi { get; set; }//当前用于控制采用的状态航向（航迹角或者航向角）
        public float GPS_Phi { get; set; }//GPS-Phi航迹角
        public float phi { get; set; }//航向角惯导
        public float Init_Phi { get; set; }//惯导原始数据，由于进行惯导0°初始化时需要获取当前惯导原始数据，所以这里需要保存惯导原始数据
        public float Phi_buchang { get; set; }//惯导补偿值
        public float Pos_Phi { get; set; }//位置坐标角（极坐标，范围[-180, 180])
        public float rud { get; set; }//舵角
        public float speed { get; set; }//船速
        public string Time { get; set; }//时间
        public int gear { get; set; }//速度档
        public int CtrlRudOut { get; set; }//舵角控制输出量
        public int CtrlSpeedOut { get; set; }//速度控制输出量
        public double Err_phi_In { get; set; }//积分控制中的积分量

        //控制参数
        public float Kp { get; set; }
        public float Ki { get; set; }
        public float Kd { get; set; }
        public float K1 { get; set; }
        public float K2 { get; set; }
        public float dl_err { get; set; }


        public static double a = 6378137.0;//定义地球长半轴长度  
        public static double earth_e = 0.003352810664; //定义椭球的第一偏心律
     //   public static double lat_start = 30.51584003;//定义原点位置
     //   public static double lon_start = 114.42665029;

        public static double lat_start = 30.51582550;//定义原点位置
        public static double lon_start = 114.426780000;

        public double[] FilterLat = new double[10] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };
        public double[] FilterLon = new double[10] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };

        public double Filter(double[] Lvbobuf)
        {
	        double average,max,min,sum=0.0d;
	        max=Lvbobuf[0];
	        min=Lvbobuf[0];
	        for(int i=0;i<10;i++)
	        {
		        if(max<Lvbobuf[i])
			        max=Lvbobuf[i];
		        if(min>Lvbobuf[i])
			        min=Lvbobuf[i];
		        sum=sum+Lvbobuf[i];
	        }
	        average=(sum-max-min)/8;
	        return average;
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
            /*for (int i = 0; i < 9; i++)
            {
                FilterLat[i] = FilterLat[i + 1];
                FilterLon[i] = FilterLon[i + 1];
            }
            FilterLat[9] = Lat;
            FilterLon[9] = Lon;

            Lat = Filter(FilterLat);
            Lon = Filter(FilterLon);*/

            pos_X = (float)((Lat - lat_start) * a * (1 - Math.Pow(earth_e, 2)) * 3.1415926 / (180 * Math.Sqrt(Math.Pow((1 - Math.Pow(earth_e * Math.Sin(Lat / 180 * 3.1415926), 2)), 3))));
            pos_Y = (float)(-((Lon - lon_start) * a * Math.Cos(Lat / 180 * 3.1415926) * 3.1415926 / (180 * Math.Sqrt(1 - Math.Pow(earth_e * Math.Sin(Lat / 180 * 3.1415926), 2)))));//Y坐标正向朝西
            
            GPS_Phi = (float)(Math.Atan2(pos_Y - last_pos_Y, pos_X - last_pos_X) / Math.PI * 180);//航迹角
            last_pos_X = pos_X;//更新上一次坐标信息
            last_pos_Y = pos_Y;

            X_mm = pos_X * 1000;
            Y_mm = pos_Y * 1000;

            Time = response_data[12].ToString() + ":" + response_data[13].ToString() + ":" + response_data[14].ToString();
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
          //  rud = response_data[21];//大船没有舵角信息
            rud = (response_data[21] - 30) * 1.8f;//小船舵角信息
            if (response_data[22] == 0) gear = response_data[22];
            else gear = response_data[22]-12;
        }

        /// <summary>
        /// 存储船舶状态信息
        /// </summary>
        /// <param name="fileName"></param>
        public void StoreShipData(string fileName, DataTable dataRec)
        {
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
            dataRec.Rows.Add(new object[] { ShipID.ToString(), Lat.ToString("0.00000000"), Lon.ToString("0.00000000"),
                pos_X.ToString("0.000"), pos_Y.ToString("0.000"), XError.ToString("0.000"),
                phi.ToString("0.0"), GPS_Phi.ToString("0.0"),
                speed.ToString("0.00"), gear.ToString(),
                rud.ToString("0.0"), CtrlRudOut.ToString(), CtrlSpeedOut.ToString(),
                Time.ToString() });
        }

    }
}
