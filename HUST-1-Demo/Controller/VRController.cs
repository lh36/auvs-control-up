using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HUST_1_Demo.Model;
using System.Drawing;

namespace HUST_1_Demo.Controller
{
    class VRController
    {
        #region 开环控制

        //设置船速
        public void SetSpeed(ShipData boat, float speed)
        {
            boat.speed = speed;
        }

        //设置舵角
        public void SetRudder(ShipData boat, float rud)
        {
            boat.rud = rud;
        }

        //前进（加速）
        public void Speed_Up(ShipData boat)
        {
            if (boat.speed >= 0.8)
                return;
            else
                boat.speed += 0.10f;
        }

        //减速
        public void Speed_Down(ShipData boat)
        {
            if (boat.speed == 0)
                return;
            else
                boat.speed -= 0.10f;
        }

        //左转
        public void Turn_Left(ShipData boat)
        {
            if (boat.rud >= 25.0f)
                return;
            else
                boat.rud += 1.0f;
        }

        //右转
        public void Turn_Right(ShipData boat)
        {
            if (boat.rud <= -25.0f)
                return;
            else
                boat.rud -= 1.0f;
        }

        //停船
        public void Stop_Robot(ShipData boat)
        {
            boat.speed = 0;
            boat.rud = 0;
        }
        #endregion

        #region 闭环控制
        //点跟踪
        public void FollowPoint(ShipData boat, Point targetPoint)
        {
            double current_c = boat.Ctrl_Phi;//实际航向

            double distance = Math.Sqrt((boat.X_mm - targetPoint.X) * (boat.X_mm - targetPoint.X) + (boat.Y_mm - targetPoint.Y) * (boat.Y_mm - targetPoint.Y));//毫米单位的距离

            if (distance <= 1000.0d)
            {
                HUST_1_Demo.Form1.isRmtClsFlag = false;
                HUST_1_Demo.Form1.isFlagCtrl = false;
                Stop_Robot(boat);
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
                double R = boat.Kp * detc;

                if (R > 25)
                {
                    R = 25.0d;
                }
                else if (R < -25)
                {
                    R = -25.0d;
                }

                boat.rud = (float)R;
            }
        }
        
        //直线跟踪
        public void FollowLine(ShipData boat, double line)
        {
            double k = 3.5d;//制导角参数
            double Err_phi = 0.0d;
            double y_d = line;//目标直线，单位为米

            double Ye = boat.Fter_pos_Y - y_d;//实际坐标减参考坐标
            double Ref_phi = -Math.Atan(Ye / k) / Math.PI * 180;//制导角（角度制°）

            Err_phi = Ref_phi - boat.Ctrl_Phi;


            if (Err_phi > 180)//偏差角大于180°时减去360°得到负值，表示航向左偏于制导角；偏差小于180°时表示航向右偏于制导角。
            {
                Err_phi = Err_phi - 360;
            }
            if (Math.Abs(Ye) < 0.2)
            {
                boat.Err_phi_In += Err_phi;
            }

            double R = boat.Kp * Err_phi + boat.Ki * boat.Err_phi_In;

            if (R > 25)
            {
                R = 25.0d;
            }
            else if (R < -25)
            {
                R = -25.0d;
            }

            boat.rud = (float)R;
        }
        
        //圆轨迹跟踪
        public void FollowCircle(ShipData boat, HUST_1_Demo.Form1.TargetCircle circle)
        {
            double Err_phi = 0.0d;
            // double ROBOTphi_r = 0.0d;//相对参考向的航向角或航迹角
            double k = 3.5d;


            double Radius = circle.Radius;//目标圆半径，将单位转为毫米
            double Center_X = circle.x;//圆心坐标
            double Center_Y = circle.y;

            double Ye = (Math.Sqrt((boat.Fter_pos_X - Center_X) * (boat.Fter_pos_X - Center_X) + (boat.Fter_pos_Y - Center_Y) * (boat.Fter_pos_Y - Center_Y)) - Radius);

            float Robot_xy = (float)(Math.Atan2(boat.Fter_pos_Y - Center_Y, boat.Fter_pos_X - Center_X) / Math.PI * 180);//航行器相对于原点的极坐标点
            double Dir_R = Robot_xy - 90;//圆切线角     得出航行器和制导角的参考0向，即极坐标的x轴，两者角度都是相对该轴的角度值

            if (Dir_R > 180) Dir_R = Dir_R - 360;
            else if (Dir_R < -180) Dir_R = Dir_R + 360;

            double errorRobot_Pos = boat.Ctrl_Phi - Robot_xy;

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
                HUST_1_Demo.Form1.cirDir = 2;//逆时针
            }
            #endregion

            Err_phi = Ref_phi - (boat.Ctrl_Phi - Dir_R);//实际航向减去制导角的偏差
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

            double R = boat.Kp * Err_phi + boat.Ki * boat.Err_phi_In;

            if (R > 25)
            {
                R = 25.0d;
            }
            else if (R < -25)
            {
                R = -25.0d;
            }

            boat.rud = (float)R;
        }

        //编队
        #endregion
    }
}
