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
        //点跟随
        public byte FollowPoint(ShipData boat, Point targetPoint)
        {
            double current_c = boat.Control_Phi;//实际航向

            double distance = Math.Sqrt((boat.X_mm - targetPoint.X) * (boat.X_mm - targetPoint.X) + (boat.Y_mm - targetPoint.Y) * (boat.Y_mm - targetPoint.Y));//毫米单位的距离

            if (distance <= 800.0d)
            {
              //  Stop_Robot(port);
                HUST_1_Demo.Form1.isFlagCtrl = false;

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
        
        //一般直线跟随
        public byte FollowLine(ShipData boat, HUST_1_Demo.Form1.TargetLine line)
        {
            float k = 3.5f;//制导角参数
            double Err_phi = 0.0f;

            double targetY = line.LineK * boat.pos_X + line.LineB;//航行器X坐标对应目标直线的Y坐标
            double refDir = Math.Atan(line.LineK)/Math.PI*180; //参考方向，与目标直线平行
            double deltaY = boat.pos_Y - targetY;//实际坐标减参考坐标,基于参考坐标点坐标系的建立的误差
            double Ye=deltaY*Math.Cos(refDir/180*Math.PI);//航行器到目标线的垂向距离
            float Ref_phi = (float)(-Math.Atan(Ye / k) / Math.PI * 180);//制导角（角度制°）
           
            if (line.isReverse == true)//如果为逆向直线，则需要对制导角进行Y轴对称变换
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
              
            Err_phi = Ref_phi - (boat.Control_Phi - refDir);

            if (Err_phi > 180)//偏差角大于180°时减去360°得到负值，表示航向左偏于制导角；偏差小于180°时表示航向右偏于制导角。
            {
                Err_phi = Err_phi - 360;
            }
            if (Err_phi < -180)
            {
                Err_phi = Err_phi + 360;
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
        }
        
        //特殊直线跟随
        public byte FollowLine(ShipData boat, double line)
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

        //多段直线跟踪
        public byte FollowMulLine(ShipData boat)
        {
            byte R = 0;
            HUST_1_Demo.Form1.TargetLine line = new Form1.TargetLine();
            line.Start = HUST_1_Demo.Form1.tarMultiLine.ElementAt(HUST_1_Demo.Form1.followLineID);//线段起始点
            line.End = HUST_1_Demo.Form1.tarMultiLine.ElementAt(HUST_1_Demo.Form1.followLineID+1);//线端终点
            line.LineK = (double)(line.Start.Y - line.End.Y) / (double)(line.Start.X - line.End.X);
            line.LineB = (line.Start.Y - line.LineK * line.Start.X) / 1000.0f;
            if (line.End.X < line.Start.X)//判断是否为逆向直线
                line.isReverse = true;

            R = FollowLine(boat, line);
            if (Math.Sqrt((boat.X_mm - line.End.X) * (boat.X_mm - line.End.X) + (boat.Y_mm - line.End.Y) * (boat.Y_mm - line.End.Y)) < 4000)
                HUST_1_Demo.Form1.followLineID++;
            if (HUST_1_Demo.Form1.followLineID == HUST_1_Demo.Form1.tarMultiLine.Count - 1)
            {
                HUST_1_Demo.Form1.followLineID = 0;
            }
                
            return R;
        }
        
        //圆轨迹跟随
        public byte FollowCircle(ShipData boat, HUST_1_Demo.Form1.TargetCircle circle)
        {
            float Err_phi = 0.0f;
           // double ROBOTphi_r = 0.0d;//相对参考向的航向角或航迹角
            double k = 3.5d;

            
            double Radius = circle.Radius;//目标圆半径，将单位转为毫米
            double Center_X = circle.x;//圆心坐标
            double Center_Y = circle.y;

            double Ye = (Math.Sqrt((boat.pos_X - Center_X) * (boat.pos_X - Center_X) + (boat.pos_Y - Center_Y) * (boat.pos_Y - Center_Y)) - Radius);

            float Robot_xy = (float)(Math.Atan2(boat.pos_Y - Center_Y, boat.pos_X - Center_X) / Math.PI * 180);//航行器相对于原点的极坐标点
            float Dir_R = Robot_xy - 90;//圆切线角     得出航行器和制导角的参考0向，即极坐标的x轴，两者角度都是相对该轴的角度值

            if (Dir_R > 180) Dir_R = Dir_R - 360;
            else if (Dir_R < -180) Dir_R = Dir_R + 360;

            double errorRobot_Pos = boat.Control_Phi - Robot_xy;

            #region 获取当前制导角
            float Ref_phi = (float)(-Math.Atan(Ye / k) / Math.PI * 180);//制导角（角度制°）
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
                HUST_1_Demo.Form1.isFlagDir = true;
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

        //椭圆轨迹跟随
        public byte FollowOval(ShipData boat, HUST_1_Demo.Form1.TargetOval oval)
        {
            byte R = 0 ;
            switch (HUST_1_Demo.Form1.SetOvalPathID)
            {
                case 0:
                    {
                        HUST_1_Demo.Form1.TargetLine line = new Form1.TargetLine();
                        line.Start = oval.Pt1;
                        line.End = oval.Pt2;
                        line.LineK = oval.K1;
                        line.LineB = oval.B1/1000d;
                        R = FollowLine(boat, line);

                        if (Math.Sqrt((boat.X_mm - oval.Pt2.X) * (boat.X_mm - oval.Pt2.X) + (boat.Y_mm - oval.Pt2.Y) * (boat.Y_mm - oval.Pt2.Y)) < 2000)
                            HUST_1_Demo.Form1.SetOvalPathID = 1;
                        break;
                    }
                case 1:
                    {
                        HUST_1_Demo.Form1.TargetCircle circle = new Form1.TargetCircle();
                        circle.x = oval.OriPt1.X / 1000d;
                        circle.y = oval.OriPt1.Y / 1000d;
                        circle.Radius = oval.R / 1000d;

                        R = FollowCircle(boat, circle);
                        if (Math.Sqrt((boat.X_mm - oval.Pt3.X) * (boat.X_mm - oval.Pt3.X) + (boat.Y_mm - oval.Pt3.Y) * (boat.Y_mm - oval.Pt3.Y)) < 2000)
                            HUST_1_Demo.Form1.SetOvalPathID = 2;
                        break;
                    }
                case 2:
                    {
                        HUST_1_Demo.Form1.TargetLine line = new Form1.TargetLine();
                        line.Start = oval.Pt3;
                        line.End = oval.Pt4;
                        line.LineK = oval.K1;
                        line.LineB = oval.B3 / 1000d;
                        line.isReverse = true;
                        R = FollowLine(boat, line);

                        if (Math.Sqrt((boat.X_mm - oval.Pt4.X) * (boat.X_mm - oval.Pt4.X) + (boat.Y_mm - oval.Pt4.Y) * (boat.Y_mm - oval.Pt4.Y)) < 2000)
                            HUST_1_Demo.Form1.SetOvalPathID = 3;
                        break;
                    }
                case 3:
                    {
                        HUST_1_Demo.Form1.TargetCircle circle = new Form1.TargetCircle();
                        circle.x = oval.OriPt2.X / 1000d;
                        circle.y = oval.OriPt2.Y / 1000d;
                        circle.Radius = oval.R / 1000d;

                        R = FollowCircle(boat, circle);
                        if (Math.Sqrt((boat.X_mm - oval.Pt1.X) * (boat.X_mm - oval.Pt1.X) + (boat.Y_mm - oval.Pt1.Y) * (boat.Y_mm - oval.Pt1.Y)) < 2000)
                            HUST_1_Demo.Form1.SetOvalPathID = 0;
                        break;
                    }
            }

            return R;
        }
        #endregion

        #region 闭环控制区-速度控制
        public byte Closed_Control_LineSpeed(ShipData boat, ShipData leaderboat, bool flagPathSelect, bool flagFollowDir)
        {
            double tempLeader = 0.0d;
            double tempFollow = 0.0d;//跟随者变量
            double deltaError = 0.0d;//距离误差
            if (flagPathSelect == false)//判断为跟随直线控制还是跟随圆轨迹控制
            {
                tempLeader = leaderboat.pos_X;
                tempFollow = boat.pos_X;
            }
            else
            {
                tempLeader = leaderboat.Pos_Phi;
                tempFollow = boat.Pos_Phi;
            }

            if (flagFollowDir == false)
            {
                if ((tempFollow < 0) && (tempLeader > 0)) //处理正负180°附近的情况
                    tempFollow = tempFollow + 360;
                deltaError = tempFollow - tempLeader;
            }
            else
            {
                if ((tempLeader < 0) && (tempFollow > 0)) //处理正负180°附近的情况
                    tempLeader = tempLeader + 360;
                deltaError = tempLeader - tempFollow;
            }

            
            int U = (int)(boat.K1 * 100 + boat.K2 * (deltaError - boat.dl_err));//速度控制
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
