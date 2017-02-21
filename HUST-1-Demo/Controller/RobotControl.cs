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
        

        public RobotControl() { } //默认构造函数
        public RobotControl(byte[] ship) //带参构造函数
        {
            this.command[0] = ship[0];
            this.command[1] = ship[1];
        }

        public byte[] command = new byte[6] { 0x00, 0x00, 0x06, 0x00, 0x00, 0xaa };

        public void Send_Command(SerialPort port)
        {
            port.Write(command, 0, 6);
        }

        public void Get_ShipData(SerialPort port)
        {
            command[3] = 0x47;
            Send_Command(port);
        }

        #region 开环控制区
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
        public byte Closed_Control_Point(ShipData boat, Point targetPoint)
        {
            double current_c = boat.Control_Phi;//实际航向

            double distance = Math.Sqrt((boat.X_mm - targetPoint.X) * (boat.X_mm - targetPoint.X) + (boat.Y_mm - targetPoint.Y) * (boat.Y_mm - targetPoint.Y));//毫米单位的距离

            if (distance <= 800.0d)
            {
              //  Stop_Robot(port);
                HUST_1_Demo.Form1.flag_ctrl = false;

                return 0x53;
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
                return (byte)R;
              //  Send_Command(port);
              //  Get_ShipData(port);//获取最新船状态信息
            }
        }

        /// <summary>
        /// 跟踪目标直线
        /// </summary>
        /// <param name="port"></param>
        /// <param name="boat"></param>
        /// <param name="line"></跟踪目标直线>
        public byte Closed_Control_Line(ShipData boat, double line)
        {
            double k = 3.5d;//制导角参数
            double Err_phi = 0.0d;
            double y_d = line;//目标直线，单位为米

            double Ye = boat.pos_Y - y_d;//实际坐标减参考坐标
            double Ref_phi = -Math.Atan(Ye / k) / Math.PI * 180;//制导角（角度制°）
            
            Err_phi = Ref_phi - boat.Control_Phi;
           

            if (Err_phi > 180)//偏差角大于180°时减去360°得到负值，表示航向左偏于制导角；偏差小于180°时表示航向右偏于制导角。
            {
                Err_phi = Err_phi - 360;
            }
            if (Math.Abs(Ye) < 0.2)
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

            return (byte)R;
           // Send_Command(port);
           // Get_ShipData(port);//获取最新船状态信息
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="port"></param>
        /// <param name="boat"></param>
        /// <param name="line"></param>
        public byte Closed_Control_Circle(ShipData boat, HUST_1_Demo.Form1.TargetCircle circle)
        {
            double Err_phi = 0.0d;
           // double ROBOTphi_r = 0.0d;//相对参考向的航向角或航迹角
            double k = 3.5d;

            
            double Radius = circle.Radius;//目标圆半径，将单位转为毫米
            double Center_X = circle.x;//圆心坐标
            double Center_Y = circle.y;

            double Ye = (Math.Sqrt((boat.pos_X - Center_X) * (boat.pos_X - Center_X) + (boat.pos_Y - Center_Y) * (boat.pos_Y - Center_Y)) - Radius);

            double Robot_xy = Math.Atan2(boat.pos_Y - Center_Y, boat.pos_X - Center_X) / Math.PI * 180;//航行器相对于原点的极坐标点
            double Dir_R = Robot_xy - 90;//圆切线角     得出航行器和制导角的参考0向，即极坐标的x轴，两者角度都是相对该轴的角度值

            if (Dir_R > 180) Dir_R = Dir_R - 360;
            else if (Dir_R < -180) Dir_R = Dir_R + 360;

            double errorRobot_Pos = boat.Control_Phi - Robot_xy;

            #region 获取当前制导角
            double Ref_phi = -Math.Atan(Ye / k) / Math.PI * 180;//制导角（角度制°）
            if ((errorRobot_Pos > 0 && errorRobot_Pos < 180) || (errorRobot_Pos < -180))//根据船向与顺逆边界的关系，选取制导角对称与否
            {
                if (Ref_phi < 0)
                {
                    Ref_phi = -180 - Ref_phi;
                }
                else
                {
                    Ref_phi = 180 - Ref_phi;
                }
            }
            #endregion

            Err_phi = Ref_phi-(boat.Control_Phi - Dir_R);//实际航向减去制导角的偏差
            if (Err_phi > 180)//偏差角大于180°时减去360°得到负值，表示航向左偏于制导角；偏差小于180°时表示航向右偏于制导角。
            {
                Err_phi = Err_phi - 360;
            }
            else if (Err_phi < -180)
            {
                Err_phi = Err_phi + 360;
            }

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

            return (byte)R;
          //  Send_Command(port);
          //  Get_ShipData(port);//获取最新船状态信息
        }
        #endregion

        #region 闭环控制区-速度控制
        public byte Closed_Control_Speed(ShipData boat, double leaderShip)
        {
            int U = (int)(boat.K1 * 100 + boat.K2 * (leaderShip - boat.pos_X - boat.dl_err));//设置领航者船2的速度档位10,为了与舵角区分，将命令加上100传输，范围
            if (U > 150)   //将速度档位范围保持在4~16范围内
            {
                U = 150;
            }
            else if (U < 50)
            {
                U = 50;
            }
            return (byte)U;
        }
        #endregion

        
    }
}
