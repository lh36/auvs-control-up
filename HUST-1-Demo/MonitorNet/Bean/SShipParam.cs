using System;

namespace MonitorNet
{
    public class SShipParam
    {
        public double lat;//经度
        public double lon;//纬度

        public double posX;//X坐标
        public double posY;//Y坐标

        public double fError;//跟随误差

        public double phi;//航向角
        public double GPS_Phi;//未滤波航迹角

        public float rud;//舵角
        public float speed;//船速
        public int gear;//船速等级
        public long time;//运行时间

        public float Kp;//PID控制参数
        public float Ki;
        public float Kd;
        public float K1;//速度控制参数
        public float K2;

        public double tem;
        public double ph;
        public double diso;
        public double tur;
        public double con;

        public SShipParam() //默认构造函数
        {
            this.lat = 0;
            this.lon = 0;
            this.posX = 0;
            this.posY = 0;

            this.fError = 0;

            this.phi = 0;
            this.GPS_Phi = 0;

            this.rud = 0;
            this.speed = 0;
            this.gear = 0;
            this.time = 0;

            this.Kp = 0;
            this.Ki = 0;
            this.Kd = 0;
            this.K1 = 0;
            this.K2 = 0;

            this.tem = 0;
            this.ph = 0;
            this.diso = 0;
            this.tur = 0;
            this.con = 0;
        }

        public SShipParam(double lat, double lon, double posX, double posY,
            double phi, double gps_phi, float rudAng, float speed, int gear, long time,
            float kp, float ki, float kd, float k1, float k2, double tem, 
            double ph, double diso, double tur, double con) //带参构造函数
        {
            this.lat = lat;
            this.lon = lon;
            this.posX = posX;
            this.posY = posY;

            this.phi = phi;
            this.GPS_Phi = gps_phi;

            this.rud = rudAng;
            this.speed = speed;
            this.gear = gear;
            this.time = time;

            this.Kp = kp;
            this.Ki = ki;
            this.Kd = kd;
            this.K1 = k1;
            this.K2 = k2;

            this.tem = tem;
            this.ph = ph;
            this.diso = diso;
            this.tur = tur;
            this.con = con;
        }
    }
}