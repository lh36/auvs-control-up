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

        
    }
}
