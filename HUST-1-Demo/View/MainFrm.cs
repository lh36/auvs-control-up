using HUST_1_Demo.Controller;
using HUST_1_Demo.Model;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Security.Permissions;
using MonitorNet;


namespace HUST_1_Demo
{
    [PermissionSet(SecurityAction.Demand, Name = "FullTrust")]
    [System.Runtime.InteropServices.ComVisible(true)]
    //  [ComVisible(true)]
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        /// <summary>
        /// 定义全局变量
        /// </summary>
        /// 
        public struct TargetLine  //目标直线，一般直线
        {
            public Point Start;
            public Point End;
            public double LineK;
            public double LineB;
            public bool isReverse;
        }
        public struct TargetCircle  //目标圆参数
        {
            public double Radius;
            public double x;
            public double y;
        }
        public struct TargetOval   //椭圆
        {
            public Point Pt1;
            public Point Pt2;
            public Point Pt3;
            public Point Pt4;

            public Point OriPt1;//上半圆圆心1
            public Point OriPt2;//下半圆圆心2
            public double R;//半圆半径

            public double K1;//L1、L3斜率
            public double K2;//L2、L4斜率
            public double B1;//L1截距
            public double B2;//L2截距
            public double B3;//L3截距
            public double B4;//L4截距
        }


        public static bool isFlagCtrl = false;//绘画线程标志
        public static bool isFlagDraw = false;//控制线程标志
        public static int pathType = 1;//跟随路径选择标志=1：直线，=2：圆
        public static int cirDir = 1;//圆轨迹跟随方向选择标志=1：顺时针，=2：逆时针
        public static bool isStartPt = false;//直线跟踪起始点
        public static bool isTarLineSet = false;//是否已设置目标直线
        public static bool isOvalSet = false;//是否已设置目标椭圆成功
        public static int SetOvalPtFlag = 0;//获取椭圆点的标志，0为Pt1，1为Pt2，2为Pt3
        public static int SetOvalPathID = 0;//椭圆跟随边（两条直线/两个椭圆）切换标志
        public static bool isMulLineEnd = false;//多段直线设定结束
        public static bool isRmtCtrl = false;//是否接受远程控制
        public static bool isRmtClsFlag = false;//可直接循环执行当跟随目标轨迹
        public static bool bRecdData = false;//是否保存数据


        string name = "";//保存数据txt
        DataTable dataRec = new DataTable();

     //   Point target_pt = new Point();//捕获左键鼠标按下去的点，以得到跟踪目标点
        List<double[]> PtPoolGPSBd = new List<double[]>();//泳池边界点经纬度
        List<double[]> PtPoolXYBd = new List<double[]>();//泳池边界点经纬度

        public Point tarPoint;  //目标点
        public Point tarPointDraw;//绘图使用
        public TargetLine tarLineGe;//一般直线
        public static List<Point> tarMultiLine = new List<Point>();//多段直线
        static List<Point> tarMultiLineDraw = new List<Point>();//绘图使用
        public static int followLineID = 0;//跟随多段直线分段标志
        public TargetCircle tarCircle; //目标圆
        public TargetOval tarOval;//椭圆
        public double tarLineSp;  //平行于X轴的特殊直线

        ShipData boat1 = new ShipData();//状态参数对象
        ShipData boat2 = new ShipData();
        ShipData boat3 = new ShipData();

        RobotControl ship1Control = new RobotControl(0xa1, 0x1a);//控制参数对象
        RobotControl ship2Control = new RobotControl(0xa2, 0x2a);
        RobotControl ship3Control = new RobotControl(0xa3, 0x3a);

        /// <summary>
        /// 打开串口
        /// </summary>
        private void ComOpen1_Click(object sender, EventArgs e)
        {
            string CommNum = this.ComPortNum1.Text;
            int IntBaudRate = Convert.ToInt32(this.BaudRate1.Text);      //转换方法1
            if (!serialPort1.IsOpen)
            {
                serialPort1.PortName = CommNum;
                serialPort1.BaudRate = IntBaudRate;
                try   //try:尝试下面的代码，如果错误就跳出来执行catch里面的代码
                {
                    serialPort1.Open();
                    ComOpen1.Text = "Close port";
                    ComPortNum1.Enabled = false;
                    BaudRate1.Enabled = false;
                }
                catch
                {
                    MessageBox.Show("串口打开失败！\r\n");
                }
            }
            else
            {
                timer1.Enabled = false;
                serialPort1.Close();
                ComOpen1.Text = "Open port";
                ComPortNum1.Enabled = true;
                BaudRate1.Enabled = true;
            }
        }

        private void Advance_Click(object sender, EventArgs e)/*前进*/
        {
            if (!serialPort1.IsOpen)//由于画图需要打开串口，因此先判断串口状态，若没打开则先打开
            {
                MessageBox.Show("请先打开串口！\r\n");
            }
            else
            {
                if (asv1.Checked)
                {
                    ship1Control.Speed_Up();
                }
                else if (asv2.Checked)
                {
                    ship2Control.Speed_Up();
                }
                else
                {
                    ship3Control.Speed_Up();
                }
            }
        }

        private void Back_Click(object sender, EventArgs e)/*后退*/
        {
            if (!serialPort1.IsOpen)//由于画图需要打开串口，因此先判断串口状态，若没打开则先打开
            {
                MessageBox.Show("请先打开串口！\r\n");
            }
            else
            {
                if (asv1.Checked)
                {
                    ship1Control.Speed_Down();
                }
                else if (asv2.Checked)
                {
                    ship2Control.Speed_Down();
                }
                else
                {
                    ship3Control.Speed_Down();
                }
            }

        }

        private void Stop_Click(object sender, EventArgs e)
        {
            if (!serialPort1.IsOpen)//由于画图需要打开串口，因此先判断串口状态，若没打开则先打开
            {
                MessageBox.Show("请先打开串口！\r\n");
            }
            {
                isFlagCtrl = false;
                ship1Control.Stop_Robot();
                ship2Control.Stop_Robot();
                ship3Control.Stop_Robot();
            }

        }

        private void serialPort1_DataReceived(object sender, System.IO.Ports.SerialDataReceivedEventArgs e)
        {
            byte[] response_data = RecvSeriData.ReceData(serialPort1);

            #region 接收到一组正确的数据，则进行处理和显示
            if (response_data != null)
            {
                byte ID_Temp = response_data[3];
                switch (ID_Temp)
                {
                    case 0x01: boat1.UpdataStatusData(response_data); boat1.SubmitParamToServer(); if (bRecdData == true) boat1.StoreShipData(name, dataRec); break;//闭环时的数据才进行存储
                    case 0x02: boat2.UpdataStatusData(response_data); boat2.SubmitParamToServer(); if (bRecdData == true) boat2.StoreShipData(name, dataRec); break;
                    case 0x03: boat3.UpdataStatusData(response_data); boat3.SubmitParamToServer(); if (bRecdData == true) boat3.StoreShipData(name, dataRec); break;
                    default: break;
                }
                Array.Clear(response_data, 0, response_data.Length);
                Display();//有数据更新时才更新显示，否则不更新（即不是每次接收到数据才更新，只有接收到正确的数据才更新）
            }
            #endregion
        }

        private void Display()//参数显示函数
        {
            Boat1_X.Text = boat1.pos_X.ToString("0.00");
            Boat1_Y.Text = boat1.pos_Y.ToString("0.00");
            Boat1_phi.Text = boat1.phi.ToString("0.00");
            Boat1_Ru.Text = boat1.rud.ToString("0.0");
            Boat1_speed.Text = boat1.speed.ToString("0.000");
            Boat1_grade.Text = boat1.gear.ToString();
            Boat1_time.Text = boat1.sTime;
            Boat1_MotorSpd.Text = boat1.MotorSpd.ToString();

            Boat2_X.Text = boat2.pos_X.ToString("0.00");
            Boat2_Y.Text = boat2.pos_Y.ToString("0.00");
            Boat2_phi.Text = boat2.phi.ToString("0.00");
            Boat2_Ru.Text = boat2.rud.ToString("0.0");
            Boat2_speed.Text = boat2.speed.ToString("0.000");
            Boat2_grade.Text = boat2.gear.ToString();
            Boat2_time.Text = boat2.sTime;
            Boat2_MotorSpd.Text = boat2.MotorSpd.ToString();

            Boat3_X.Text = boat3.pos_X.ToString("0.00");
            Boat3_Y.Text = boat3.pos_Y.ToString("0.00");
            Boat3_phi.Text = boat3.phi.ToString("0.00");
            Boat3_Ru.Text = boat3.rud.ToString("0.0");
            Boat3_speed.Text = boat3.speed.ToString("0.000");
            Boat3_grade.Text = boat3.gear.ToString();
            Boat3_time.Text = boat3.lTime.ToString();
            Boat3_MotorSpd.Text = boat3.MotorSpd.ToString();
        }

        private void leftup_Click(object sender, EventArgs e)
        {

            if (!serialPort1.IsOpen)//由于画图需要打开串口，因此先判断串口状态，若没打开则先打开
            {
                MessageBox.Show("请先打开串口！\r\n");
            }
            else
            {
                if (asv1.Checked)
                {
                    ship1Control.Turn_Left();
                }
                else if (asv2.Checked)
                {
                    ship2Control.Turn_Left();
                }
                else
                {
                    ship3Control.Turn_Left();
                }
            }

        }
        private void rightup_Click(object sender, EventArgs e)
        {
            if (!serialPort1.IsOpen)//由于画图需要打开串口，因此先判断串口状态，若没打开则先打开
            {
                MessageBox.Show("请先打开串口！\r\n");
            }
            else
            {
                if (asv1.Checked)
                {
                    ship1Control.Turn_Right();
                }
                else if (asv2.Checked)
                {
                    ship2Control.Turn_Right();
                }
                else
                {
                    ship3Control.Turn_Right();
                }
            }

        }



        static int halfHeight_mm = 55000;//地图一半长55米
        static List<Point> listPoint_Boat1 = new List<Point>();
        static List<Point> listPoint_Boat2 = new List<Point>();
        static List<Point> listPoint_Boat3 = new List<Point>();

        private void DrawMap()
        {
            while (isFlagDraw)
            {
                Graphics g = this.PathMap.CreateGraphics();
                g.Clear(Color.White);
                Pen p = new Pen(Color.Black, 2);//定义了一个蓝色,宽度为的画笔
                g.DrawLine(p, 0, PathMap.Height / 2, PathMap.Width, PathMap.Height / 2);//在画板上画直线,起始坐标为(10,10),终点坐标为(100,100)
                g.DrawLine(p, PathMap.Width / 2, 0, PathMap.Width / 2, PathMap.Height);
                //地图像素大小
                int Widthmap = PathMap.Width / 2;
                int Heightmap = PathMap.Height / 2;

                //实际大小
                int Heigh_mm = halfHeight_mm;
                int Width_mm = Heigh_mm / Heightmap * Widthmap;

                //比例尺和反比例尺
                double scale = Heigh_mm / Heightmap;//单位像素代表的实际长度，单位：mm
                double paint_scale = 1 / scale;//每毫米在图上画多少像素，单位：像素

                int paint_x1 = Widthmap - (int)(boat1.Y_mm * paint_scale);//转换为图上的坐标
                int paint_y1 = Heightmap - (int)(boat1.X_mm * paint_scale);

                int paint_x2 = Widthmap - (int)(boat2.Y_mm * paint_scale);//转换为图上的坐标
                int paint_y2 = Heightmap - (int)(boat2.X_mm * paint_scale);

                int paint_x3 = Widthmap - (int)(boat3.Y_mm * paint_scale);//转换为图上的坐标
                int paint_y3 = Heightmap - (int)(boat3.X_mm * paint_scale);

                #region 绘制泳池边界
                List<Point> PtPaint = new List<Point>();//绘图
                double[] Pt1 = new double[2] { 30.51582463, 114.426777 };//定义边界点
                double[] Pt2 = new double[2] { 30.51584359, 114.4265784 };
                double[] Pt3 = new double[2] { 30.5162782, 114.42661763 };
                double[] Pt4 = new double[2] { 30.51626484, 114.426825 };

                PtPoolGPSBd.Add(Pt1);
                PtPoolGPSBd.Add(Pt2);
                PtPoolGPSBd.Add(Pt3);
                PtPoolGPSBd.Add(Pt4);

                for (int i = 0; i < 4; i++)//将经纬度转为平面坐标xy
                {
                    double[] tepXY = HUST_1_Demo.Model.ShipData.GPS2XY(PtPoolGPSBd.ElementAt(i));
                    PtPoolXYBd.Add(tepXY);
                }

                for (int i = 0; i < 4; i++)//将xy坐标转为绘图坐标
                {
                    Point tepPaint = new Point();
                    tepPaint.X = Widthmap - (int)(PtPoolXYBd.ElementAt(i).ElementAt(1) * 1000 * paint_scale);//转换为图上的坐标
                    tepPaint.Y = Heightmap - (int)(PtPoolXYBd.ElementAt(i).ElementAt(0) * 1000 * paint_scale);//转换为图上的坐标
                    PtPaint.Add(tepPaint);
                }


                for (int i = 0; i < PtPaint.Count - 1; i++)//绘制泳池边界
                {
                    g.DrawLine(new Pen(Color.Blue, 1), PtPaint.ElementAt(i), PtPaint.ElementAt(i + 1));
                }
                g.DrawLine(new Pen(Color.Blue, 1), PtPaint.ElementAt(3), PtPaint.ElementAt(0));

                #endregion

                #region 画目标直线和圆
                if (path_mode.Text == "Point")//绘制目标点
                {
                    g.DrawRectangle(new Pen(Color.Red, 2), tarPointDraw.X - 4, tarPointDraw.Y - 4, 4, 4);
                }

                if (path_mode.Text == "General line")
                {
                    if (isTarLineSet)
                    {
                        int x1 = Widthmap - (int)(tarLineGe.Start.Y * paint_scale);
                        int y1 = Heightmap - (int)(tarLineGe.Start.X * paint_scale);
                        int x2 = Widthmap - (int)(tarLineGe.End.Y * paint_scale);
                        int y2 = Heightmap - (int)(tarLineGe.End.X * paint_scale);

                        g.DrawLine(new Pen(Color.Blue, 1), x1, y1, x2, y2);
                    }
                }
                if (path_mode.Text == "Special line")
                {
                    int x = Widthmap - (int)(Convert.ToInt32(this.line_Y1.Text) * 1000 * paint_scale);
                    g.DrawLine(new Pen(Color.DarkGreen, 1), x, 0, x, PathMap.Height);

                    x = Widthmap - (int)(Convert.ToInt32(this.line_Y2.Text) * 1000 * paint_scale);
                    g.DrawLine(new Pen(Color.Red, 1), x, 0, x, PathMap.Height);

                    x = Widthmap - (int)(Convert.ToInt32(this.line_Y3.Text) * 1000 * paint_scale);
                    g.DrawLine(new Pen(Color.Blue, 1), x, 0, x, PathMap.Height);
                }
                if (path_mode.Text == "Multi line")
                {
                    if (tarMultiLineDraw.Count >= 2)
                    {
                        for (int i = 0; i < tarMultiLineDraw.Count - 1; i++)
                        {
                            g.DrawLine(new Pen(Color.Blue, 1), tarMultiLineDraw.ElementAt(i), tarMultiLineDraw.ElementAt(i + 1));
                        }
                    }
                }
                if (path_mode.Text == "Circular path")
                {
                    int x = Convert.ToInt32(this.circle_X.Text);
                    int y = Convert.ToInt32(this.circle_Y.Text);
                    int r = Convert.ToInt32(this.circle_R1.Text);

                    int x1 = Widthmap - (int)((y + r) * 1000 * paint_scale);
                    int y1 = Heightmap - (int)((x + r) * 1000 * paint_scale);

                    g.DrawEllipse(new Pen(Color.Cyan, 2), x1, y1, (int)(r * 1000 * paint_scale) * 2, (int)(r * 1000 * paint_scale) * 2);

                }
                if (path_mode.Text == "Oval path")
                {
                    if (isOvalSet == true)
                    {
                        Pen penOval = new Pen(Color.Blue, 1);//定义了一个蓝色,宽度为的画笔
                        int x1 = Widthmap - (int)(tarOval.Pt1.Y * paint_scale);
                        int y1 = Heightmap - (int)(tarOval.Pt1.X * paint_scale);
                        int x2 = Widthmap - (int)(tarOval.Pt2.Y * paint_scale);
                        int y2 = Heightmap - (int)(tarOval.Pt2.X * paint_scale);
                        int x3 = Widthmap - (int)(tarOval.Pt3.Y * paint_scale);
                        int y3 = Heightmap - (int)(tarOval.Pt3.X * paint_scale);
                        int x4 = Widthmap - (int)(tarOval.Pt4.Y * paint_scale);
                        int y4 = Heightmap - (int)(tarOval.Pt4.X * paint_scale);

                        g.DrawLine(penOval, x1, y1, x2, y2);
                        g.DrawLine(penOval, x2, y2, x3, y3);
                        g.DrawLine(penOval, x3, y3, x4, y4);
                        g.DrawLine(penOval, x4, y4, x1, y1);

                        //    g.DrawArc(penOval, x2, y2, 100, 200, 30, 330);
                    }
                }

                #endregion


                listPoint_Boat1.Add(new Point(paint_x1, paint_y1));
                listPoint_Boat2.Add(new Point(paint_x2, paint_y2));
                listPoint_Boat3.Add(new Point(paint_x3, paint_y3));
                if (listPoint_Boat1.Count >= 2)
                {
                    g.DrawCurve(new Pen(Color.Red, 2), listPoint_Boat1.ToArray());
                }
                if (listPoint_Boat2.Count >= 2)
                {
                    g.DrawCurve(new Pen(Color.Green, 2), listPoint_Boat2.ToArray());
                }
                if (listPoint_Boat3.Count >= 2)
                {
                    g.DrawCurve(new Pen(Color.Gold, 2), listPoint_Boat3.ToArray());
                }

                Thread.Sleep(200);
            }

        }


        private void timer1_Tick(object sender, EventArgs e)
        {
            ship1Control.Send_Command(serialPort1);
            Thread.Sleep(40);
            ship2Control.Send_Command(serialPort1);
            Thread.Sleep(40);
            ship3Control.Send_Command(serialPort1);
        }

        private void Reset_Click(object sender, EventArgs e)
        {
            //三船状态数据清除
            listPoint_Boat1.Clear();
            listPoint_Boat2.Clear();
            listPoint_Boat3.Clear();
            webBrowser1.Document.InvokeScript("clear_line");
            //多段直线数据清除
            tarMultiLine.Clear();
            tarMultiLineDraw.Clear();//画图数据清除
            isMulLineEnd = false;//多段直线设定完成置位
            followLineID = 0;//多段直线分段标志置位
            dataRec.Clear();
            if (!serialPort1.IsOpen)//由于画图需要打开串口，因此先判断串口状态，若没打开则先打开
            {
                MessageBox.Show("请先打开串口！\r\n");
            }
            else
            {
                ship1Control.Stop_Robot();
                ship2Control.Stop_Robot();
                ship3Control.Stop_Robot();

                boat1.Err_phi_In = 0;
                boat2.Err_phi_In = 0;
                boat3.Err_phi_In = 0;
            }
        }

        private void Start_Click(object sender, EventArgs e)
        {
            if (!serialPort1.IsOpen)//由于画图需要打开串口，因此先判断串口状态，若没打开则先打开
            {
                MessageBox.Show("请先打开串口！\r\n");
            }
            else
            {
                if (this.Start.Text == "Start")
                {
                    isFlagDraw = true;
                    Thread threadDraw = new Thread(DrawMap);
                    threadDraw.IsBackground = true;
                    threadDraw.Start();

                    timer1.Enabled = true;//默认是开环控制，则启动获取三船位姿线程
                    timer2.Enabled = true;
                    this.Start.Text = "Stop";

                }
                else if (this.Start.Text == "Stop")
                {
                    isFlagDraw = false;
                    isFlagCtrl = false;//控制线程标志
                    timer1.Enabled = false;//坐标跟新
                    timer2.Enabled = false;
                    this.Start.Text = "Start";
                }

            }
        }

        private void Backoff_Click(object sender, EventArgs e)
        {
            if (!serialPort1.IsOpen)//由于画图需要打开串口，因此先判断串口状态，若没打开则先打开
            {
                MessageBox.Show("请先打开串口！\r\n");
            }
            else
            {
                ship1Control.command[3] = 0x42;
            }

        }



        private void button1_Click(object sender, EventArgs e)
        {
            if (button1.Text == "Start following")
            {
                name = DateTime.Now.ToString("yyyyMMddHHmmss");//保存数据txt
                timer1.Enabled = false;//首先关闭开环定时器获取当前状态信息的定时器
                isFlagCtrl = true;
                bRecdData = true;//开始记录数据
                Thread threadControl = new Thread(Control_PF);
                threadControl.IsBackground = true;
                threadControl.Start();
                button1.Text = "Stop following";

            }
            else
            {
                timer1.Enabled = true;//关闭闭环控制后，重新开启开环获取船位姿状态信息
                isFlagCtrl = false;
                ship1Control.Stop_Robot();
                Thread.Sleep(40);
                ship2Control.Stop_Robot();
                Thread.Sleep(40);
                ship3Control.Stop_Robot();
                button1.Text = "Start following";
            }

        }

        private void UpdateCtrlPhi()
        {
            if (Phi_mode.Text == "Heading angle")
            {
                boat1.Ctrl_Phi = boat1.phi;
                boat2.Ctrl_Phi = boat2.phi;
                boat3.Ctrl_Phi = boat3.phi;
            }
            else
            {
                if (TckAngFter.Checked)//是否滤波
                {
                    boat1.Ctrl_Phi = boat1.Fter_GPS_Phi;
                    boat2.Ctrl_Phi = boat2.Fter_GPS_Phi;
                    boat3.Ctrl_Phi = boat3.Fter_GPS_Phi;
                }
                else
                {
                    boat1.Ctrl_Phi = boat1.GPS_Phi;
                    boat2.Ctrl_Phi = boat2.GPS_Phi;
                    boat3.Ctrl_Phi = boat3.GPS_Phi;
                }
            }
        }

        private void UpdateCtrlPara()
        {
            boat1.Kp = float.Parse(Boat1_Kp.Text);//获取舵机控制参数
            boat1.Ki = float.Parse(Boat1_Ki.Text);
            boat1.Kd = float.Parse(Boat1_Kd.Text);
            boat1.K1 = float.Parse(Boat1_K1.Text);//获取螺旋桨控制参数
            boat1.K2 = float.Parse(Boat1_K2.Text);
            boat1.dl_err = float.Parse(Boat1_DL.Text);

            boat2.Kp = float.Parse(Boat2_Kp.Text);//获取舵机控制参数
            boat2.Ki = float.Parse(Boat2_Ki.Text);
            boat2.Kd = float.Parse(Boat2_Kd.Text);
            boat2.K1 = float.Parse(Boat2_K1.Text);//获取螺旋桨控制参数
            boat2.K2 = float.Parse(Boat2_K2.Text);
            boat2.dl_err = float.Parse(Boat2_DL.Text);

            boat3.Kp = float.Parse(Boat3_Kp.Text);//获取舵机控制参数
            boat3.Ki = float.Parse(Boat3_Ki.Text);
            boat3.Kd = float.Parse(Boat3_Kd.Text);
            boat3.K1 = float.Parse(Boat3_K1.Text);//获取螺旋桨控制参数
            boat3.K2 = float.Parse(Boat3_K2.Text);
            boat3.dl_err = float.Parse(Boat3_DL.Text);

            HUST_1_Demo.Controller.RobotControl.Alf1 = float.Parse(Alfa1.Text);
            HUST_1_Demo.Controller.RobotControl.Alf2 = float.Parse(Alfa1.Text);
            HUST_1_Demo.Controller.RobotControl.Ks = float.Parse(RiseKs.Text);
            HUST_1_Demo.Controller.RobotControl.Bet = float.Parse(Beta.Text);

            HUST_1_Demo.Controller.RobotControl.Kp = float.Parse(NSFC_Kp.Text);
            HUST_1_Demo.Controller.RobotControl.Kd = float.Parse(NSFC_Kd.Text);
        }

        private void UpdateCtrlOutput()
        {
            tarLineSp = float.Parse(line_Y1.Text);//1号船目标线和圆
            tarCircle.Radius = float.Parse(circle_R1.Text);
            tarCircle.x = float.Parse(circle_X.Text);
            tarCircle.y = float.Parse(circle_Y.Text);

            Control_fun(ship1Control, boat1);//1号小船控制
            if (AutoSpeed.Checked)
                ship1Control.Closed_Control_LineSpeed(boat1, boat2, pathType, cirDir);
            else
                ship1Control.command[4] = (byte)(int.Parse(Manualspeedset.Text));

            boat1.CtrlRudOut = ship1Control.command[3];//舵角控制输出量
            boat1.CtrlSpeedOut = ship1Control.command[4];//速度控制输出量
            boat1.XError = boat2.pos_X - boat1.pos_X;

            tarLineSp = float.Parse(line_Y2.Text);//2号船目标线和圆
            tarCircle.Radius = float.Parse(circle_R2.Text);
            Control_fun(ship2Control, boat2);//2号小船控制，2号小船为leader，无需控制速度
            if (AutoSpeed.Checked)
                ship2Control.Closed_Control_LineSpeed(boat2, boat2, pathType, cirDir);
            else
                ship2Control.command[4] = (byte)(int.Parse(Manualspeedset.Text));
            //   ship2Control.command[4] = 100;
            boat2.CtrlRudOut = ship2Control.command[3];//舵角控制输出量
            boat2.CtrlSpeedOut = ship2Control.command[4];//速度控制输出量
            boat2.XError = boat1.pos_X - boat3.pos_X;

            tarLineSp = float.Parse(line_Y3.Text);//3号船目标线和圆
            tarCircle.Radius = float.Parse(circle_R3.Text);
            Control_fun(ship3Control, boat3);//3号小船控制
            if (AutoSpeed.Checked)
                ship3Control.Closed_Control_LineSpeed(boat3, boat2, pathType, cirDir);
            else
                ship3Control.command[4] = (byte)(int.Parse(Manualspeedset.Text));
            // ship3Control.command[4] = 110;
            boat3.CtrlRudOut = ship3Control.command[3];//舵角控制输出量
            boat3.CtrlSpeedOut = ship3Control.command[4];//速度控制输出量
            boat3.XError = boat2.pos_X - boat3.pos_X;

            xError1.Text = boat1.XError.ToString("0.000");//领队减1号
            xError2.Text = boat2.XError.ToString("0.000");//1号减2号
            xError3.Text = boat3.XError.ToString("0.000");//领队减3号
        }

        /// <summary>
        /// 控制线程
        /// </summary>
        private void Control_PF()
        {
            while (isFlagCtrl)
            {
                UpdateCtrlPhi();//更新控制方式为航向角或航迹角
                UpdateCtrlPara();//更新PID控制参数和速度控制参数
                UpdateCtrlOutput();//更新航向和速度控制输出
                HUST_1_Demo.Controller.RobotControl.timeTickCnt++;
                Thread.Sleep(195);//控制周期
            }
        }

        private void Control_fun(RobotControl shipControl, ShipData shipData)
        {
            #region 跟踪目标点
            if (path_mode.Text == "Point")
            {
                shipControl.FollowPoint(shipData, tarPoint);
            }
            #endregion

            #region 跟随一般直线
            if (path_mode.Text == "General line")
            {
                shipControl.FollowLine(shipData, tarLineGe);
            }
            #endregion

            #region 跟随特殊直线
            if (path_mode.Text == "Special line")
            {
                shipControl.FollowLine(shipData, tarLineSp);
                pathType = 1;
            }
            #endregion

            #region 跟随多段直线
            if (path_mode.Text == "Multi line")
            {
                shipControl.FollowMulLine(shipData);
            }
            #endregion

            #region 跟随圆轨迹
            if (path_mode.Text == "Circular path")
            {
                shipControl.FollowCircle(shipData, tarCircle);
                pathType = 2;
            }
            #endregion

            #region 跟随椭圆
            if (path_mode.Text == "Oval path")
            {
                shipControl.FollowOval(shipData, tarOval);
            }
            #endregion

            #region RISE Test
            if (path_mode.Text == "RISE test")
            {
                shipControl.RISE_Test(shipData);
            }
            #endregion

            #region RISE Test
            if (path_mode.Text == "NSFC test")
            {
                shipControl.NSFC_Test(shipData);
            }
            #endregion
        }

        //初始化表格表头
        private void InitRecTable()
        {
            dataRec.Columns.Add("ShipID", Type.GetType("System.String"));
            dataRec.Columns.Add("Lat", Type.GetType("System.String"));
            dataRec.Columns.Add("Lon", Type.GetType("System.String"));
            dataRec.Columns.Add("X", Type.GetType("System.String"));
            dataRec.Columns.Add("Y", Type.GetType("System.String"));
            dataRec.Columns.Add("X Error", Type.GetType("System.String"));
            dataRec.Columns.Add("Phi", Type.GetType("System.String"));
            dataRec.Columns.Add("GPSPhi", Type.GetType("System.String"));
            dataRec.Columns.Add("FlterGPSPhi", Type.GetType("System.String"));
            dataRec.Columns.Add("Speed", Type.GetType("System.String"));
            dataRec.Columns.Add("Gear", Type.GetType("System.String"));
            dataRec.Columns.Add("Rud", Type.GetType("System.String"));
            dataRec.Columns.Add("CtrlRudOut", Type.GetType("System.String"));
            dataRec.Columns.Add("CtrlSpeedOut", Type.GetType("System.String"));
            dataRec.Columns.Add("e1", Type.GetType("System.String"));
            dataRec.Columns.Add("e2", Type.GetType("System.String"));
            dataRec.Columns.Add("Vf", Type.GetType("System.String"));
            dataRec.Columns.Add("F2", Type.GetType("System.String"));
            dataRec.Columns.Add("MotorSpd", Type.GetType("System.String"));
            dataRec.Columns.Add("LineID", Type.GetType("System.String"));
            dataRec.Columns.Add("Time", Type.GetType("System.String"));
        }
        private void Init_Map()
        {
            string url = Application.StartupPath + "/map_type.html";
            webBrowser1.Url = new Uri(url);//指定url  
            //  webBrowser1.ObjectForScripting = this;
            // webBrowser1.Navigate(new Uri("E:\\LAB\\ASV\\02 源代码\\auvs-control-up\\HUST-1-Demo\\bin\\Debug\\map_type.html"));
        }
        private void Form1_Load(object sender, EventArgs e)
        {
            InitRecTable();
            Init_Map();
            Control.CheckForIllegalCrossThreadCalls = false;

        }

        private object[] GetRdPt(ShipData boat)
        {
            object[] boatPt = { boat.LLat, boat.LLon, boat.Lat, boat.Lon };
            boat.LLat = boat.Lat;
            boat.LLon = boat.Lon;
            return boatPt;
        }
        private void Draw_Map_Road(int boatID, object[] obj)
        {
            switch (boatID)
            {
                case 1: webBrowser1.Document.InvokeScript("poly_line_route1", obj); break;
                case 2: webBrowser1.Document.InvokeScript("poly_line_route2", obj); break;
                case 3: webBrowser1.Document.InvokeScript("poly_line_route3", obj); break;
            }
        }
        private void PathMap_MouseDown(object sender, MouseEventArgs e)
        {
            Point target_pt = new Point(e.X, e.Y);
            //地图像素大小
            int Widthmap = PathMap.Width / 2;
            int Heightmap = PathMap.Height / 2;

            //实际大小
            int Heigh_mm = halfHeight_mm;
            int Width_mm = Heigh_mm / Heightmap * Widthmap;

            //比例尺和反比例尺
            double scale = Heigh_mm / Heightmap;//单位像素代表的实际长度，单位：mm
            double paint_scale = 1 / scale;//每毫米在图上画多少像素，单位：像素

            #region 目标点跟随设置
            if (path_mode.Text == "Point")
            {
                if (e.Button == MouseButtons.Left)//如果是鼠标左键，则为设定目标点
                {
                    tarPoint.X = (int)((Heightmap - target_pt.Y) * scale);//得到以毫米为单位的目标X值
                    tarPoint.Y = (int)((Widthmap - target_pt.X) * scale);//得到以毫米为单位的目标点Y值

                    tarPointDraw.X = target_pt.X;//绘制目标点使用
                    tarPointDraw.Y = target_pt.Y;

                    tar_Point_X.Text = (tarPoint.X / 1000).ToString();
                    tar_Point_Y.Text = (tarPoint.Y / 1000).ToString();
                }
            }
            #endregion

            #region 一般直线属性点设置
            if (path_mode.Text == "General line")
            {
                if (e.Button == MouseButtons.Right)//如果是鼠标右键，则为设定直线
                {
                    if (isStartPt == false)//说明该点是第一个点，即起始点
                    {
                        tarLineGe.Start.X = (int)((Heightmap - target_pt.Y) * scale);//得到以毫米为单位的目标X值
                        tarLineGe.Start.Y = (int)((Widthmap - target_pt.X) * scale);//得到以毫米为单位的目标点Y值
                        isStartPt = true;
                        isTarLineSet = false;
                    }
                    else  //说明该点是第二个点，即终止点
                    {
                        tarLineGe.End.X = (int)((Heightmap - target_pt.Y) * scale);//得到以毫米为单位的目标X值
                        tarLineGe.End.Y = (int)((Widthmap - target_pt.X) * scale);//得到以毫米为单位的目标点Y值

                        tarLineGe.LineK = (double)(tarLineGe.Start.Y - tarLineGe.End.Y) / (double)(tarLineGe.Start.X - tarLineGe.End.X);
                        tarLineGe.LineB = (tarLineGe.Start.Y - tarLineGe.LineK * tarLineGe.Start.X) / 1000.0f;

                        if (tarLineGe.End.X < tarLineGe.Start.X)//判断是否为逆向直线
                            tarLineGe.isReverse = true;
                        else
                            tarLineGe.isReverse = false;
                        isStartPt = false; //将起始点置位，即又可以重新开始设置起始点
                        isTarLineSet = true;
                    }
                }
            }
            #endregion

            #region 椭圆属性点设置
            if (path_mode.Text == "Oval path")
            {
                if (e.Button == MouseButtons.Left)//左键设置
                {
                    switch (SetOvalPtFlag)
                    {
                        case 0:
                            {
                                tarOval.Pt1.X = (int)((Heightmap - target_pt.Y) * scale);
                                tarOval.Pt1.Y = (int)((Widthmap - target_pt.X) * scale);
                                SetOvalPtFlag++;
                                isOvalSet = false;
                                break;
                            }
                        case 1:
                            {
                                tarOval.Pt2.X = (int)((Heightmap - target_pt.Y) * scale);
                                tarOval.Pt2.Y = (int)((Widthmap - target_pt.X) * scale);
                                SetOvalPtFlag++;
                                break;
                            }
                        case 2:
                            {
                                tarOval.Pt3.X = (int)((Heightmap - target_pt.Y) * scale);
                                tarOval.Pt3.Y = (int)((Widthmap - target_pt.X) * scale);

                                //计算椭圆直线斜率
                                tarOval.K1 = (double)(tarOval.Pt2.Y - tarOval.Pt1.Y) / (double)(tarOval.Pt2.X - tarOval.Pt1.X);
                                tarOval.K2 = -1 / tarOval.K1;//椭圆第二条线斜率，半圆直径方向

                                //计算椭圆直线截距
                                tarOval.B1 = (tarOval.Pt1.Y - tarOval.K1 * tarOval.Pt1.X);//截距 毫米单位
                                tarOval.B2 = (tarOval.Pt2.Y - tarOval.K2 * tarOval.Pt2.X);//截距 毫米单位
                                double R = ((tarOval.K1 * tarOval.Pt3.X - tarOval.Pt3.Y + tarOval.B1) / Math.Sqrt(Math.Pow(tarOval.K1, 2) + 1)) / 2;//半径，毫米单位,带符号
                                tarOval.B3 = tarOval.B1 - 2 * R * Math.Sqrt(Math.Pow(tarOval.K1, 2) + 1);
                                tarOval.B4 = (tarOval.Pt1.Y - tarOval.K2 * tarOval.Pt1.X);

                                //重新计算椭圆的Pt3
                                tarOval.Pt3.X = (int)((tarOval.B2 - tarOval.B3) / (tarOval.K1 - tarOval.K2));//
                                tarOval.Pt3.Y = (int)((tarOval.K2 * tarOval.Pt3.X + tarOval.B2));

                                //计算上半圆圆心Ori1
                                tarOval.OriPt1.X = (tarOval.Pt2.X + tarOval.Pt3.X) / 2;
                                tarOval.OriPt1.Y = (tarOval.Pt2.Y + tarOval.Pt3.Y) / 2;

                                //计算Pt4
                                tarOval.Pt4.X = (int)((tarOval.B3 - tarOval.B4) / (tarOval.K2 - tarOval.K1));//mm单位
                                tarOval.Pt4.Y = (int)((tarOval.K2 * tarOval.Pt4.X + tarOval.B4));

                                //计算下半圆圆心Ori2
                                tarOval.OriPt2.X = (tarOval.Pt1.X + tarOval.Pt4.X) / 2;
                                tarOval.OriPt2.Y = (tarOval.Pt1.Y + tarOval.Pt4.Y) / 2;
                                tarOval.R = Math.Abs(R);

                                isOvalSet = true;
                                SetOvalPtFlag = 0;
                                break;
                            }
                    }

                }
            }
            #endregion

            #region 多段直线
            if (path_mode.Text == "Multi line")
            {
                if (isMulLineEnd == false)
                {
                    int x = (int)((Heightmap - target_pt.Y) * scale);
                    int y = (int)((Widthmap - target_pt.X) * scale);
                    tarMultiLine.Add(new Point(x, y));//毫米单位坐标点
                    tarMultiLineDraw.Add(target_pt);//绘图坐标点
                }

                if (e.Button == MouseButtons.Right)
                {
                    isMulLineEnd = true;
                }
            }
            #endregion


        }

        private void boat1_init_Phi_Click(object sender, EventArgs e)
        {
            boat1.Phi_buchang = -boat1.Init_Phi;
        }

        private void boat2_init_Phi_Click(object sender, EventArgs e)
        {
            boat2.Phi_buchang = -boat2.Init_Phi;
        }

        private void boat3_init_Phi_Click(object sender, EventArgs e)
        {
            boat3.Phi_buchang = -boat3.Init_Phi;
        }

        private void BTNStoreData_Click(object sender, EventArgs e)
        {
            Microsoft.Office.Interop.Excel.Application excel = new Microsoft.Office.Interop.Excel.Application();
            excel.Application.Workbooks.Add(true);
            //填充数据
            for (int i = 0; i < dataRec.Rows.Count; i++)
            {
                for (int j = 0; j < dataRec.Columns.Count; j++)
                {
                    excel.Cells[i + 1, j + 1] = dataRec.Rows[i][j];
                }

            }
            string PFID = path_mode.Text;
            switch (PFID)
            {
                case "General line":
                    {
                        excel.Cells[dataRec.Rows.Count + 1, 1] = tarLineGe.Start.X.ToString();//记录一般直线,起始点x坐标
                        excel.Cells[dataRec.Rows.Count + 1, 2] = tarLineGe.Start.Y.ToString();//起始点y坐标
                        excel.Cells[dataRec.Rows.Count + 2, 1] = tarLineGe.End.X.ToString();
                        excel.Cells[dataRec.Rows.Count + 2, 2] = tarLineGe.End.Y.ToString();
                        break;
                    }
                case "Multi line":
                    {
                        for (int i = 0; i < tarMultiLine.Count; i++)
                        {
                            excel.Cells[dataRec.Rows.Count + 1 + i, 1] = tarMultiLine.ElementAt(i).X.ToString();
                            excel.Cells[dataRec.Rows.Count + 1 + i, 2] = tarMultiLine.ElementAt(i).Y.ToString();
                        }
                        break;
                    }
                case "Oval path":
                    {
                        excel.Cells[dataRec.Rows.Count + 1, 1] = tarOval.Pt1.X.ToString();//记录椭圆
                        excel.Cells[dataRec.Rows.Count + 2, 1] = tarOval.Pt2.X.ToString();//起始点x坐标
                        excel.Cells[dataRec.Rows.Count + 3, 1] = tarOval.Pt3.X.ToString();
                        excel.Cells[dataRec.Rows.Count + 4, 1] = tarOval.Pt4.X.ToString();

                        excel.Cells[dataRec.Rows.Count + 1, 2] = tarOval.Pt1.Y.ToString();//起始点y坐标
                        excel.Cells[dataRec.Rows.Count + 2, 2] = tarOval.Pt2.Y.ToString();
                        excel.Cells[dataRec.Rows.Count + 3, 2] = tarOval.Pt3.Y.ToString();
                        excel.Cells[dataRec.Rows.Count + 4, 2] = tarOval.Pt4.Y.ToString();
                        break;
                    }
            }



            excel.Visible = true;
        }


        static byte[] commandOpen = new byte[6] { 0xa1, 0x1a, 0x06, 32, 90, 0xaa };
        private void OpnTstFun()
        {
            serialPort1.Write(commandOpen, 0, 6);
            Thread.Sleep(10000);
            commandOpen[3] = 64;
            serialPort1.Write(commandOpen, 0, 6);
        }
        private void OpnCtrTst_Click(object sender, EventArgs e)
        {
            Thread OpenCtrTst = new Thread(OpnTstFun);
            OpenCtrTst.IsBackground = true;
            OpenCtrTst.Start();
            isFlagCtrl = true;
        }

        private void timer2_Tick(object sender, EventArgs e)
        {
            Draw_Map_Road(1, GetRdPt(boat1));
            Draw_Map_Road(2, GetRdPt(boat2));
            Draw_Map_Road(3, GetRdPt(boat3));
        }

        public static long GetTimeStamp()
        {
            TimeSpan ts = DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0, 0);
            return Convert.ToInt64(ts.TotalSeconds);
        }

        public static long GetMillTimeStamp()
        {
            TimeSpan ts = DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0, 0);
            return Convert.ToInt64(ts.TotalMilliseconds);
        }

        private void bSerInit_Click(object sender, EventArgs e)
        {
            if (bSerInit.Text == "Start Server")
            {
                var oInstanceData = new MonitorNet.InstanceData();
                oInstanceData.Name = "Local test";                              //  实验名称
                oInstanceData.Desp = "LH";                                      //  实验描述
                oInstanceData.Amount = 1;                                   //  此次实验参与船的数量
                oInstanceData.Shape = "A";                                     //  船的类型
                oInstanceData.Time = GetTimeStamp();

                NetManager.Instance.NetCreateNewInstance(oInstanceData);        //  创建上传数据实例

                // 如何关闭获取控制连接
                NetManager.Instance.NetGetControlData(this.ControlFromServer);  //  创建监听远程控制命令实例
                bSerInit.Text = "Close Server";
            }
            else
            {
                NetManager.Instance.FinishControlRequest();
                NetManager.Instance.NetFinishInstance(GetTimeStamp(), null);
                bSerInit.Text = "Start Server";

                isRmtCtrl = false;//关闭远程模式-切换到本地模式
                isRmtClsFlag = false;//关闭闭环循环模式
                RmtCtrl.Text = "Local control mode";
            }

        }

        private void ControlFromServer(object oControlDara)
        {
            var sControlData = oControlDara.ToString();//这里是json格式数据，需要通过json解析

            if (sControlData == "startlink")//开始远程控制请求
            {
                if (MessageBox.Show("Allow remote control mode?", "Confirm Message",
                    MessageBoxButtons.OKCancel, MessageBoxIcon.Question) == DialogResult.OK)
                {
                    isRmtCtrl = true;

                    RmtCtrl.Text = "Remote control mode";
                }
            }
            else if (sControlData == "closelink")//结束远程控制请求
            {
                if (MessageBox.Show("Remote control closed!", "Confirm Message",
                MessageBoxButtons.OKCancel) == DialogResult.OK)
                {
                    isRmtCtrl = false;
                    isRmtClsFlag = false;
                    RmtCtrl.Text = "Local control mode";
                }
            }
            else
            {
                if (isRmtCtrl == true)//远程模式在本地允许后才进行命令解析，否则不解析
                {
                    string[] sArr = sControlData.Split('-');

                    if (sArr[0] == "o")     //  开环控制命令解析
                    {
                        OpnCmd(sArr);
                    }
                    if (sArr[0] == "c")                   //  闭环控制命令解析
                    {
                        //接到闭环命令，立即向云端服务器反馈接收到的跟随目标
                        //本地闭环控制时，跟随目标更新后，也应该立即反馈给云端服务器反馈
                        /**********************
                         * 1. 设定任务模式（点/特殊直线/一般直线/多段直线/圆轨迹等）
                         *    即更改path_mode属性的值
                         * 2. 设置目标轨迹参数
                         * 3. 开始跟随—开启新线程跟随
                         * *******************/
                        UpdtPathMode(sArr[2]);
                        UpdtRefPath(sArr);

                        UpdtRmtRefTask(sArr);//更新远程闭环控制目标（直线/圆）
                        isRmtClsFlag = true;
                        bRecdData = true;//开始记录数据
                        MessageBox.Show("闭环控制启动！");
                        Thread t = new Thread(this.RmtClsCtrl);
                        t.IsBackground = true;
                        t.Start(sArr);
                    }
                    if (sArr[0] == "s")
                    {
                        isRmtClsFlag = false;
                        bRecdData = false;//停止记录数据
                        ship1Control.Stop_Robot();
                        ship2Control.Stop_Robot();
                        ship3Control.Stop_Robot();
                        MessageBox.Show("停船！");
                    }
                }
            }
        }

        private void UpdtPathMode(string sArr)
        {
            switch (sArr)
            {
                case "p":
                    {
                        //点跟踪
                        path_mode.Text = "Point";
                        break;
                    }
                case "g":
                    {
                        //一般直线跟踪
                        path_mode.Text = "General line";
                        break;
                    }
                case "l":
                    {
                        //特殊直线跟踪
                        path_mode.Text = "Special line";
                        break;
                    }
                case "m":
                    {
                        //多段直线跟踪
                        path_mode.Text = "Multi line";
                        break;
                    }
                case "r":
                    {
                        //圆轨迹跟踪
                        path_mode.Text = "Circular path";
                        break;
                    }
                case "f":
                    {
                        //编队航行
                        path_mode.Text = "Special line";
                        break;
                    }
            }
        }

        private void UpdtRefPath(string[] sArr)
        {
            int Widthmap = PathMap.Width / 2;
            int Heightmap = PathMap.Height / 2;

            //实际大小
            int Heigh_mm = halfHeight_mm;
            int Width_mm = Heigh_mm / Heightmap * Widthmap;

            //比例尺和反比例尺
            double scale = Heigh_mm / Heightmap;//单位像素代表的实际长度，单位：mm
            double paint_scale = 1 / scale;//每毫米在图上画多少像素，单位：像素

            switch (sArr[2])
            {
                case "p":
                    {
                        //点跟踪参考轨迹参数设定
                        tar_Point_X.Text = sArr[4];
                        tar_Point_Y.Text = sArr[5];
                        tarPoint.X = int.Parse(sArr[4]) * 1000;
                        tarPoint.Y = int.Parse(sArr[5]) * 1000;

                        tarPointDraw.X = Widthmap - (int)(tarPoint.Y * paint_scale);
                        tarPointDraw.Y = Heightmap - (int)(tarPoint.X * paint_scale);
                        break;
                    }
                case "g":
                    {
                        //一般直线跟踪
                        tarLineGe.Start.X = int.Parse(sArr[4]) * 1000;
                        tarLineGe.Start.Y = int.Parse(sArr[5]) * 1000;

                        tarLineGe.End.X = int.Parse(sArr[6]) * 1000;
                        tarLineGe.End.Y = int.Parse(sArr[7]) * 1000;

                        tarLineGe.LineK = (double)(tarLineGe.Start.Y - tarLineGe.End.Y) / (double)(tarLineGe.Start.X - tarLineGe.End.X);
                        tarLineGe.LineB = (tarLineGe.Start.Y - tarLineGe.LineK * tarLineGe.Start.X) / 1000.0f;

                        if (tarLineGe.End.X < tarLineGe.Start.X)//判断是否为逆向直线
                            tarLineGe.isReverse = true;
                        else
                            tarLineGe.isReverse = false;

                        isTarLineSet = true;
                        break;
                    }
                case "l":
                    {
                        //特殊直线跟踪
                        if (sArr[1] == "1")
                        {
                            line_Y1.Text = sArr[3];
                        }

                        else if (sArr[1] == "2")
                        {
                            line_Y2.Text = sArr[3];
                        }

                        else
                        {
                            line_Y3.Text = sArr[3];
                        }
                        tarLineSp = int.Parse(sArr[3]);
                        break;
                    }
                case "m":
                    {
                        //多段直线跟踪
                        int numPt = int.Parse(sArr[3]);
                        int i = 0;
                        while (tarMultiLineDraw.Count<numPt)
                        {
                            //需要将实际坐标点转换为绘图坐标点
                            int x = int.Parse(sArr[4 + i]) * 1000;
                            int y = int.Parse(sArr[5 + i]) * 1000;
                            tarMultiLine.Add(new Point(x, y));//毫米单位坐标点

                            int x1 = Widthmap - (int)(y * paint_scale);
                            int y1 = Heightmap - (int)(x * paint_scale);
                            tarMultiLineDraw.Add(new Point(x1, y1));//绘图坐标点
                            i = i + 2;
                        }
                        break;
                    }
                case "r":
                    {
                        //圆轨迹跟踪
                        if (sArr[1] == "1")
                        {
                            circle_R1.Text = sArr[3];
                        }

                        else if (sArr[1] == "2")
                        {
                            circle_R2.Text = sArr[3];
                        }

                        else
                        {
                            circle_R3.Text = sArr[3];
                        }
                        tarCircle.Radius = double.Parse(sArr[3]);
                        tarCircle.x = 15;
                        tarCircle.y = 10;
                        break;
                    }
            }
        }
       
        private void UpdtRmtCtrlOt1(string sArr)
        {
            switch(sArr)
            {
                case "1":
                    {
                        Control_fun(ship1Control, boat1);
                        ship1Control.command[4] = 100;
                        boat1.CtrlRudOut = ship1Control.command[3];//舵角控制输出量
                        boat1.CtrlSpeedOut = ship1Control.command[4];//速度控制输出量
                        boat1.XError = boat2.pos_X - boat1.pos_X;
                        break;
                    }
                case "2":
                    {
                        Control_fun(ship2Control, boat2); ship2Control.command[4] = 100; 
                        boat2.CtrlRudOut = ship2Control.command[3];//舵角控制输出量
                        boat2.CtrlSpeedOut = ship2Control.command[4];//速度控制输出量
                        boat2.XError = boat1.pos_X - boat3.pos_X;
                        break;
                    }
                case "3":
                    {
                        Control_fun(ship3Control, boat3);
                        ship3Control.command[4] = 100;
                        boat3.CtrlRudOut = ship3Control.command[3];//舵角控制输出量
                        boat3.CtrlSpeedOut = ship3Control.command[4];//速度控制输出量
                        boat3.XError = boat2.pos_X - boat3.pos_X;
                        break;
                    }
                case "4"://编队，三条船一起控横向距离2米，纵向距离2米
                    {
                        line_Y1.Text = "7";
                        tarLineSp = 7;
                        Control_fun(ship1Control, boat1);
                        ship1Control.Closed_Control_LineSpeed(boat1, boat2, pathType, cirDir);
                        boat1.CtrlRudOut = ship1Control.command[3];//舵角控制输出量
                        boat1.CtrlSpeedOut = ship1Control.command[4];//速度控制输出量
                        boat1.XError = boat2.pos_X - boat1.pos_X;

                        line_Y2.Text = "10";
                        tarLineSp = 10;
                        Control_fun(ship2Control, boat2);
                        ship2Control.Closed_Control_LineSpeed(boat2, boat2, pathType, cirDir);
                        boat2.CtrlRudOut = ship2Control.command[3];//舵角控制输出量
                        boat2.CtrlSpeedOut = ship2Control.command[4];//速度控制输出量
                        boat2.XError = boat1.pos_X - boat3.pos_X;

                        line_Y3.Text = "13";
                        tarLineSp = 13;
                        Control_fun(ship3Control, boat3);
                        ship3Control.Closed_Control_LineSpeed(boat3, boat2, pathType, cirDir);
                        boat3.CtrlRudOut = ship3Control.command[3];//舵角控制输出量
                        boat3.CtrlSpeedOut = ship3Control.command[4];//速度控制输出量
                        boat3.XError = boat2.pos_X - boat3.pos_X;
                        break;
                    }
            }
        }
        private void RmtClsCtrl(object s)
        {
            string[] sArr = (string[])s;
            while (isRmtClsFlag)//此处应该开启新线程执行，否则会在此处一直循环，导致其他无法执行
            {
                UpdateCtrlPhi();          //航迹角/航向角选择
                UpdateCtrlPara();         //控制参数由本地确定
                UpdtRmtCtrlOt1(sArr[1]);      //更新控制输出
                Thread.Sleep(195);//控制周期
            }
        }

        private void UpdtRmtRefTask(string[] sArr)
        {
            var oRefParam = new RefLineData();//参看轨迹参数

            if (sArr[2] == "l")
            {
                oRefParam.flag = 1;
                oRefParam.posY = tarLineSp;
            }
            if (sArr[2] == "r")
            {
                oRefParam.flag = 2;
                oRefParam.posX = 15;
                oRefParam.posY = 10;
                oRefParam.radius = tarCircle.Radius;
            }
            NetManager.Instance.NetSubmitRefLine(int.Parse(sArr[1]), oRefParam);//向云端服务器发送参考轨迹
        }

        private bool OpnCmd(string[] sArr)    //  开环命令解析
        {
            switch (sArr[2])    //  命令解析
            {
                case "w":
                    {
                        if (sArr[1] == "1")
                            ship1Control.Speed_Up();
                        else if (sArr[1] == "2")
                            ship2Control.Speed_Up();
                        else
                            ship3Control.Speed_Up();
                        break;
                    }
                case "a":
                    {
                        if (sArr[1] == "1")
                            ship1Control.Turn_Left();
                        else if (sArr[1] == "2")
                            ship2Control.Turn_Left();
                        else
                            ship3Control.Turn_Left();
                        break;
                    }
                case "d":
                    {
                        if (sArr[1] == "1")
                            ship1Control.Turn_Right();
                        else if (sArr[1] == "2")
                            ship2Control.Turn_Right();
                        else
                            ship3Control.Turn_Right();
                        break;
                    }
                case "s":
                    {
                        if (sArr[1] == "1")
                            ship1Control.Stop_Robot();
                        else if (sArr[1] == "2")
                            ship2Control.Stop_Robot();
                        else
                            ship3Control.Stop_Robot();
                        break;
                    }
            }
            return true;
        }

        private void RmtCtrl_Click(object sender, EventArgs e)//权限切换（本地具有最高权限，可随时切换）
        {
            if (RmtCtrl.Text == "Local control mode")
            {
                isRmtCtrl = true;//开启远程模式
                isRmtClsFlag = true;//开启闭环循环模式
                RmtCtrl.Text = "Remote control mode";
            }
            else
            {
                isRmtCtrl = false;//关闭远程模式-切换到本地模式
                isRmtClsFlag = false;//关闭闭环循环模式
                RmtCtrl.Text = "Local control mode";
            }
        }
    }
}
