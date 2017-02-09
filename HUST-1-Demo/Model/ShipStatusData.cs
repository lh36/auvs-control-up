using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HUST_1_Demo.Model
{
    class ShipStatusData
    {
        struct ship_state  //船舶状态信息
        {
            public int ShipID;//船号
            public double Lat;//纬度
            public double Lon;//经度
            public double pos_X;//当前X坐标，此为测量坐标系下的坐标，北向为X，东向为Y
            public double pos_Y;//Y坐标
            public double last_pos_X;//上一坐标值
            public double last_pos_Y;//Y坐标
            public double X_mm;//图上X坐标
            public double Y_mm;//图上Y坐标
            public double GPS_Phi;//GPS-Phi航迹角
            public float phi;//航向角惯导
            public float Init_Phi;//惯导原始数据，由于进行惯导0°初始化时需要获取当前惯导原始数据，所以这里需要保存惯导原始数据
            public float Phi_buchang;//惯导补偿值
            public double rud;//舵角
            public float speed;//船速
            public string Time;//时间
            public int gear;//速度档位
        }
    }
}
