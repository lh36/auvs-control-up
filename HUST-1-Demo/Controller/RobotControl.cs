using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO.Ports;
using HUST_1_Demo.Model;
using System.Drawing;

namespace HUST_1_Demo.Controller
{
    public class RobotControl
    {
        public static double a = 6378137.0;//定义地球长半轴长度  
        public static double earth_e = 0.003352810664; //定义椭球的第一偏心律
        public static double lat_start = 30.51584003;//定义原点位置
        public static double lon_start = 114.42665029;

        public RobotControl() { } //默认构造函数
        public RobotControl(byte[] ship) //带参构造函数
        {
            this.command[0] = ship[0];
            this.command[1] = ship[1];
        }

        public byte[] command = new byte[6] { 0x00, 0x00, 0x06, 0x00, 0x00, 0xaa };

        #region 开环控制区
        public void Send_Command(SerialPort port)
        {
            port.Write(command, 0, 6);
        }

        public void Speed_Up(SerialPort port)
        {
            command[3] = 0x49;
            Send_Command(port);
        }

        public void Speed_Down(SerialPort port)
        {
            command[3] = 0x44;
            Send_Command(port);
        }

        public void Turn_Left(SerialPort port)
        {
            command[3] = 0x51;
            Send_Command(port);
        }

        public void Turn_Right(SerialPort port)
        {
            command[3] = 0x52;
            Send_Command(port);
        }

        public void Stop_Robot(SerialPort port)
        {
            command[3] = 0x53;
            Send_Command(port);
        }
        #endregion

        #region 闭环控制区-航向控制
        /// <summary>
        /// 跟踪目标点
        /// </summary>
        /// <param name="port"></发送命令端口>
        /// <param name="boat"></当前船状态>
        /// <param name="targetPoint"></目标点>
        public void Closed_Control_Point(SerialPort port, ShipStatusData boat, Point targetPoint)
        {
            double current_c = boat.Control_Phi;//实际航向

            double distance = Math.Sqrt((boat.X_mm - targetPoint.X) * (boat.X_mm - targetPoint.X) + (boat.Y_mm - targetPoint.Y) * (boat.Y_mm - targetPoint.Y));//毫米单位的距离

            if (distance <= 800.0d)
            {
                Stop_Robot(port);
            }

            else//距离目标点很远 需要控制
            {
                double target_c = Math.Atan2(targetPoint.Y - boat.Y_mm, targetPoint.X - boat.X_mm) / Math.PI * 180;

                double detc = target_c - current_c;	//detc

                if (target_c * current_c < -90 * 90)//处理正负180度附近的偏差值，如期望角和当前角分别是170和-170，则偏差为360-|170|-|-170|=20，而不用170+170=340，   -90*90是阈值
                {
                    detc = 360 - Math.Abs(target_c) - Math.Abs(current_c);
                    if (target_c > 0)
                        detc = -detc;  //若期望角为正，而实际角为负，则此时偏差值要取反
                }
                if (Math.Abs(detc) > 180)
                {
                    if (detc > 0)
                        detc -= 360;
                    else
                        detc += 360;
                }


                int R = (int)(boat.Kp * detc);
                if (R > 32)
                {
                    R = 32;
                }
                else if (R < -32)
                {
                    R = -32;
                }
                R = R + 32;

                command[3] = (byte)R;
                Send_Command(port);
            }
        }

        /// <summary>
        /// 跟踪目标直线
        /// </summary>
        /// <param name="port"></param>
        /// <param name="boat"></param>
        /// <param name="line"></跟踪目标直线>
        public void Closed_Control_Line(SerialPort port, ShipStatusData boat, double line)
        {
            double k = 3.5d;//制导角参数
            double Err_phi = 0.0d;
            double y_d = line;

            double Ye = ((boat.Y_mm - y_d) / 1000);//实际坐标减参考坐标
            double Ref_phi = -Math.Atan(Ye / k) / Math.PI * 180;//制导角（角度制°）
            
            Err_phi = Ref_phi - boat.Control_Phi;
           

            if (Err_phi > 180)//偏差角大于180°时减去360°得到负值，表示航向左偏于制导角；偏差小于180°时表示航向右偏于制导角。
            {
                Err_phi = Err_phi - 360;
            }
            if (Math.Abs(Ye) < 0.8)
            {
                boat.Err_phi_In += Err_phi;
            }

            int R = (int)(boat.Kp * Err_phi + boat.Ki * boat.Err_phi_In);

            if (R > 32)
            {
                R = 32;
            }
            else if (R < -32)
            {
                R = -32;
            }
            R = R + 32;

            command[3] = (byte)R;
            Send_Command(port);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="port"></param>
        /// <param name="boat"></param>
        /// <param name="line"></param>
        public void Closed_Control_Circle(SerialPort port, ShipStatusData boat, HUST_1_Demo.Form1.TargetCircle circle)
        {
            double Err_phi = 0.0d;
            double ROBOTphi_r = 0.0d;//相对参考向的航向角或航迹角
            double k = 3.5d;

            
            double Radius = circle.Radius;//目标圆半径
            double Center_X = circle.x;//圆心坐标
            double Center_Y = circle.y;

            double Robot_xy = 0.0d;
            int Dir_flag = 0;//正逆时针标志

            double Ye = (Math.Sqrt((boat.X_mm - Center_X) * (boat.X_mm - Center_X) + (boat.Y_mm - Center_Y) * (boat.Y_mm - Center_Y)) - Radius) / 1000;

            Robot_xy = Math.Atan2(boat.Y_mm - Center_Y, boat.X_mm - Center_X) / Math.PI * 180;//之前每次跑偏的原因是这里将Xrectification写成了Yrectification

            double limit_ang = 0.0d;//边界，通过圆心与航行器坐标点的直径的另一半角度

            #region 跟随方向的判断和 跟随标志的确定
            if (Robot_xy > 0)
            {
                limit_ang = Robot_xy - 180;//航行器坐标点角度大于零，则判断界限角小于0，故-180，在航行器方向在界限（坐标角通过圆心的直线）右侧顺时针跟随，反之逆时针
                if ((boat.phi < limit_ang) || (boat.phi > Robot_xy))
                    Dir_flag = 1;
            }
            else if (Robot_xy < 0)
            {
                limit_ang = Robot_xy + 180;//航行器坐标点小于零，则判断界限大于零，故+180，在界限左侧则逆时针跟随，反之顺时针
                if ((boat.phi > Robot_xy) && (boat.phi < limit_ang))
                    Dir_flag = 1;
            }
            else
            {
                if (boat.phi > 0)
                    Dir_flag = 1;
            }
            Dir_flag = 1;
            #endregion

            #region 获取当前制导角
            double Ref_phi = -Math.Atan(Ye / k) / Math.PI * 180;//制导角（角度制°）
            if (Dir_flag == 1)  //若标志为1，则要处理关于Y轴对称，确定是顺时针旋转还是逆时针旋转
            {
                if (Ref_phi > 0)
                    Ref_phi = 180 - Ref_phi;
                else if (Ref_phi < 0)
                    Ref_phi = -180 - Ref_phi;
                else
                    Ref_phi = 180;
            }
            #endregion

            #region 计算参考向，以及航行器方向相对参考向的角度，以得到控制error
            double Dir_R = Robot_xy - 90;//得出航行器和制导角的参考0向，即极坐标的x轴，两者角度都是相对该轴的角度值
            
            ROBOTphi_r = boat.Control_Phi - Dir_R;
           

            if (ROBOTphi_r > 180)
                ROBOTphi_r = ROBOTphi_r - 360;//使得航行器相对于参考方向角度范围总在正负180之间
            if (ROBOTphi_r < -180)
                ROBOTphi_r = ROBOTphi_r + 360;

            if (Ref_phi * ROBOTphi_r < -90 * 90)//处理正负180度附近的偏差值，如期望角和当前角分别是170和-170，则偏差为360-|170|-|-170|=20，而不用170+170=340，   -90*90是阈值
            {
                Err_phi = 360 - Math.Abs(Ref_phi) - Math.Abs(ROBOTphi_r);
                if (Ref_phi > 0)
                    Err_phi = -Err_phi;  //若期望角为正，而实际角为负，则此时偏差值要取反
            }
            else
            {
                Err_phi = Ref_phi - ROBOTphi_r;  //阈值内取正常偏差，当Y偏差为零时，参考角度REFphi始终为零，但是ROBOTphi_r不为零，故可以一直绕圆走。
            }

            #endregion
            if (Math.Abs(Ye) < 0.8)
            {
                boat.Err_phi_In += Err_phi;
            }

            int R = (int)(boat.Kp * Err_phi + boat.Ki * boat.Err_phi_In);

            //   R = (int)(boat.Kp * Err_phi);

            if (R > 32)
            {
                R = 32;
            }
            else if (R < -32)
            {
                R = -32;
            }
            R = R + 32;
        }
        #endregion

        #region 闭环控制区-速度控制

        #endregion

        #region 更新船舶状态信息
        public static ShipStatusData UpdataStatusData(ShipStatusData boat, byte[] response_data)
        {
            ShipStatusData ship = new ShipStatusData();
           // ship.pos_X = response_data[0];
            boat.Lat = ((response_data[4] << 24) + (response_data[5] << 16) + (response_data[6] << 8) + response_data[7]) / Math.Pow(10, 8) + 30;
            boat.Lon = ((response_data[8] << 24) + (response_data[9] << 16) + (response_data[10] << 8) + response_data[11]) / Math.Pow(10, 8) + 114;

            boat.pos_X = (boat.Lat - lat_start) * a * (1 - Math.Pow(earth_e, 2)) * 3.1415926 / (180 * Math.Sqrt(Math.Pow((1 - Math.Pow(earth_e * Math.Sin(boat.Lat / 180 * 3.1415926), 2)), 3)));
            boat.pos_Y = -((boat.Lon - lon_start) * a * Math.Cos(boat.Lat / 180 * 3.1415926) * 3.1415926 / (180 * Math.Sqrt(1 - Math.Pow(earth_e * Math.Sin(boat.Lat / 180 * 3.1415926), 2))));//Y坐标正向朝西
            boat.GPS_Phi = Math.Atan2(boat.pos_Y - boat.last_pos_Y, boat.pos_X - boat.last_pos_X) / Math.PI * 180;//航迹角
            boat.last_pos_X = boat.pos_X;//更新上一次坐标信息
            boat.last_pos_Y = boat.pos_Y;

            boat.X_mm = boat.pos_X * 1000;
            boat.Y_mm = boat.pos_Y * 1000;

            boat.Time = response_data[12].ToString() + ":" + response_data[13].ToString() + ":" + response_data[14].ToString();
            boat.speed = ((response_data[15] << 8) + (response_data[16])) / 1000.0f;

            byte[] Euler_Z = new byte[4];
            Euler_Z[0] = response_data[17];
            Euler_Z[1] = response_data[18];
            Euler_Z[2] = response_data[19];
            Euler_Z[3] = response_data[20];
            boat.Init_Phi = BitConverter.ToSingle(Euler_Z, 0);

            boat.phi = boat.Init_Phi + boat.Phi_buchang;
            if (boat.phi > 180) boat.phi = boat.phi - 360;
            if (boat.phi < -180) boat.phi = boat.phi + 360;
            boat.rud = response_data[21];
            boat.gear = response_data[22];
            return ship;
        }
        #endregion


    }
}
