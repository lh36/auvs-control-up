using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HUST_1_Demo.Model
{
    public class ShipStatusData
    {
        public int ShipID { get; set; }//船号
        public double Lat { get; set; }//纬度
        public double Lon { get; set; }//经度
        public double pos_X { get; set; }//当前X坐标，此为测量坐标系下的坐标，北向为X，东向为Y
        public double pos_Y { get; set; }//Y坐标
        public double last_pos_X { get; set; }//上一坐标值
        public double last_pos_Y { get; set; }//Y坐标
        public double X_mm { get; set; }//图上X坐标
        public double Y_mm { get; set; }//图上Y坐标
        public double Control_Phi { get; set; }//当前用于控制采用的状态航向（航迹角或者航向角）
        public double GPS_Phi { get; set; }//GPS-Phi航迹角
        public float phi { get; set; }//航向角惯导
        public float Init_Phi { get; set; }//惯导原始数据，由于进行惯导0°初始化时需要获取当前惯导原始数据，所以这里需要保存惯导原始数据
        public float Phi_buchang { get; set; }//惯导补偿值
        public double rud { get; set; }//舵角
        public float speed { get; set; }//船速
        public string Time { get; set; }//时间
        public int gear { get; set; }//速度档
        public double Err_phi_In { get; set; }//积分控制中的积分量

        //控制参数
        public double Kp { get; set; }
        public double Ki { get; set; }
        public double Kd { get; set; }
        public double K1 { get; set; }
        public double K2 { get; set; }
        public double dl_err { get; set; }

    }
}
