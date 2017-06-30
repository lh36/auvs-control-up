using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HUST_1_Demo.Model;

namespace HUST_1_Demo.Controller
{
    class VRController
    {
        #region 开环控制

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
        //直线跟踪
        public void FollowLine()
        {

        }
        //圆轨迹跟踪

        //编队
        #endregion
    }
}
