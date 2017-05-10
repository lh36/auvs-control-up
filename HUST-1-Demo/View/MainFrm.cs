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


namespace HUST_1_Demo
{
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
        public static bool isCirPath = false;//跟随路径选择标志=false：直线，=true：圆
        public static bool isFlagDir = false;//圆轨迹跟随方向选择标志=false：顺时针，=true：逆时针
        public static bool isStartPt = false;//直线跟踪起始点
        public static bool isTarLineSet = false;//是否已设置目标直线
        public static bool isOvalSet = false;//是否已设置目标椭圆成功
        public static int SetOvalPtFlag = 0;//获取椭圆点的标志，0为Pt1，1为Pt2，2为Pt3
        public static int SetOvalPathID = 0;//椭圆跟随边（两条直线/两个椭圆）切换标志
        public static bool isMulLineEnd = false;//多段直线设定结束
        

        string name = "";//保存数据txt
        DataTable dataRec = new DataTable();

        Point target_pt = new Point();//捕获左键鼠标按下去的点，以得到跟踪目标点
        List<double[]> PtPoolGPSBd = new List<double[]>();//泳池边界点经纬度
        List<double[]> PtPoolXYBd = new List<double[]>();//泳池边界点经纬度
        
        public Point tarPoint;  //目标点
        public TargetLine tarLineGe;//一般直线
        public static List<Point> tarMultiLine = new List<Point>();//多段直线
        static List<Point> tarMultiLineDraw = new List<Point>();//绘图使用
        public static int followLineID = 0;//跟随多段直线分段标志
        public TargetCircle tarCircle; //目标圆
        public TargetOval tarOval;//椭圆
        public float tarLineSp;  //平行于X轴的特殊直线


        ShipData boat1 = new ShipData();
        ShipData boat2 = new ShipData();
        ShipData boat3 = new ShipData();
        

        static byte[] ship1 = new byte[2] { 0xa1, 0x1a };
        static byte[] ship2 = new byte[2] { 0xa2, 0x2a };
        static byte[] ship3 = new byte[2] { 0xa3, 0x3a };

        RobotControl ship1Control = new RobotControl(ship1);
        RobotControl ship2Control = new RobotControl(ship2);
        RobotControl ship3Control = new RobotControl(ship3);

        static byte[] command = new byte[6] { 0xa1, 0x1a, 0x06, 0x00, 0x00, 0xaa };
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
                    ship1Control.Speed_Up(serialPort1);
                }
                else if (asv2.Checked)
                {
                    ship2Control.Speed_Up(serialPort1);
                }
                else
                {
                    ship3Control.Speed_Up(serialPort1);
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
                    ship1Control.Speed_Down(serialPort1);
                }
                else if (asv2.Checked)
                {
                    ship2Control.Speed_Down(serialPort1);
                }
                else
                {
                    ship3Control.Speed_Down(serialPort1);
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
                ship1Control.Stop_Robot(serialPort1);
                ship2Control.Stop_Robot(serialPort1);
                ship3Control.Stop_Robot(serialPort1);
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
                    case 0x01: boat1.UpdataStatusData(response_data); if (isFlagCtrl == true) boat1.StoreShipData(name, dataRec); break;//闭环时的数据才进行存储
                    case 0x02: boat2.UpdataStatusData(response_data); if (isFlagCtrl == true) boat2.StoreShipData(name, dataRec); break;
                    case 0x03: boat3.UpdataStatusData(response_data); if (isFlagCtrl == true) boat3.StoreShipData(name, dataRec); break;
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
            Boat1_time.Text = boat1.Time;
            Boat1_MotorSpd.Text = boat1.MotorSpd.ToString();

            Boat2_X.Text = boat2.pos_X.ToString("0.00");
            Boat2_Y.Text = boat2.pos_Y.ToString("0.00");
            Boat2_phi.Text = boat2.phi.ToString("0.00");
            Boat2_Ru.Text = boat2.rud.ToString("0.0");
            Boat2_speed.Text = boat2.speed.ToString("0.000");
            Boat2_grade.Text = boat2.gear.ToString();
            Boat2_time.Text = boat2.Time;
            Boat2_MotorSpd.Text = boat2.MotorSpd.ToString();

            Boat3_X.Text = boat3.pos_X.ToString("0.00");
            Boat3_Y.Text = boat3.pos_Y.ToString("0.00");
            Boat3_phi.Text = boat3.phi.ToString("0.00");
            Boat3_Ru.Text = boat3.rud.ToString("0.0");
            Boat3_speed.Text = boat3.speed.ToString("0.000");
            Boat3_grade.Text = boat3.gear.ToString();
            Boat3_time.Text = boat3.Time;
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
                    ship1Control.Turn_Left(serialPort1);
                }
                else if (asv2.Checked)
                {
                    ship2Control.Turn_Left(serialPort1);
                }
                else
                {
                    ship3Control.Turn_Left(serialPort1);
                }
            }

        }

        private void leftdown_Click(object sender, EventArgs e)
        {
            if (!serialPort1.IsOpen)//由于画图需要打开串口，因此先判断串口状态，若没打开则先打开
            {
                MessageBox.Show("请先打开串口！\r\n");
            }
            else
            {
                if (asv1.Checked)
                {
                    command[0] = 0xa1;
                    command[1] = 0x1a;
                }
                else if (asv2.Checked)
                {
                    command[0] = 0xa2;
                    command[1] = 0x2a;
                }
                else
                {
                    command[0] = 0xa3;
                    command[1] = 0x3a;
                }
                command[3] = 0x4C;
                serialPort1.Write(command, 0, 6);
                //   serialPort1.Write("L");
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
                    ship1Control.Turn_Right(serialPort1);
                }
                else if (asv2.Checked)
                {
                    ship2Control.Turn_Right(serialPort1);
                }
                else
                {
                    ship3Control.Turn_Right(serialPort1);
                }
            }

        }

        private void rightdown_Click(object sender, EventArgs e)
        {
            if (!serialPort1.IsOpen)//由于画图需要打开串口，因此先判断串口状态，若没打开则先打开
            {
                MessageBox.Show("请先打开串口！\r\n");
            }
            else
            {
                if (asv1.Checked)
                {
                    command[0] = 0xa1;
                    command[1] = 0x1a;
                }
                else if (asv2.Checked)
                {
                    command[0] = 0xa2;
                    command[1] = 0x2a;
                }
                else
                {
                    command[0] = 0xa3;
                    command[1] = 0x3a;
                }
                command[3] = 0x56;
                serialPort1.Write(command, 0, 6);
                //  serialPort1.Write("V");
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
                double[] Pt4 = new double[2] { 30.51626484, 114.4268432 };
                
                PtPoolGPSBd.Add(Pt1);
                PtPoolGPSBd.Add(Pt2);
                PtPoolGPSBd.Add(Pt3);
                PtPoolGPSBd.Add(Pt4);

                for (int i = 0; i < 4;i++ )//将经纬度转为平面坐标xy
                {
                    double[] tepXY = HUST_1_Demo.Model.ShipData.GPS2XY(PtPoolGPSBd.ElementAt(i));
                    PtPoolXYBd.Add(tepXY);
                }

                for (int i = 0; i < 4;i++ )//将xy坐标转为绘图坐标
                {
                    Point tepPaint = new Point();
                    tepPaint.X = Widthmap - (int)(PtPoolXYBd.ElementAt(i).ElementAt(1)*1000 * paint_scale);//转换为图上的坐标
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
                    g.DrawRectangle(new Pen(Color.Red, 2), target_pt.X - 4, target_pt.Y - 4, 4, 4);
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
                    int x = Widthmap - (int)(Convert.ToInt32(this.line_Y2.Text) * 1000 * paint_scale);
                    g.DrawLine(new Pen(Color.Blue, 2), x, 0, x, PathMap.Height);
                }
                if (path_mode.Text == "Multi line")
                {
                    if (tarMultiLineDraw.Count >= 2)
                    {
                        for (int i = 0; i < tarMultiLineDraw.Count-1; i++)
                        {
                            g.DrawLine(new Pen(Color.Blue, 1),tarMultiLineDraw.ElementAt(i),tarMultiLineDraw.ElementAt(i+1));
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
            ship1Control.Get_ShipData(serialPort1);
            Thread.Sleep(40);
            ship2Control.Get_ShipData(serialPort1);
            Thread.Sleep(40);
            ship3Control.Get_ShipData(serialPort1);
        }

        private void Reset_Click(object sender, EventArgs e)
        {
            //三船状态数据清除
            listPoint_Boat1.Clear();
            listPoint_Boat2.Clear();
            listPoint_Boat3.Clear();
            
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
                command[0] = 0xa1;
                command[1] = 0x1a;
                command[3] = 0x53;
                serialPort1.Write(command, 0, 5);//复位先停船
                command[0] = 0xa2;
                command[1] = 0x2a;
                command[3] = 0x53;
                serialPort1.Write(command, 0, 5);//复位先停船
                command[0] = 0xa3;
                command[1] = 0x3a;
                command[3] = 0x53;
                serialPort1.Write(command, 0, 5);//复位先停船

                boat1.Err_phi_In = 0;
                boat2.Err_phi_In = 0;
                boat3.Err_phi_In = 0;
            }

            /*   MethodInvoker invoker1 = () => Boat1_speed.Text = "0";//面板显示速度置0
               Boat1_speed.BeginInvoke(invoker1);
               MethodInvoker invoker2 = () => Boat1_X.Text = "0";//坐标置0
               Boat1_X.BeginInvoke(invoker2);
               MethodInvoker invoker3 = () => Boat1_Y.Text = "0";
               Boat1_Y.BeginInvoke(invoker3);

               boat1.lat_start = boat1.Lat;//将当前船的位置点设为坐标坐标原点
               boat1.lon_start = boat1.Lon;
               boat2.lat_start = boat2.Lat;
               boat2.lon_start = boat2.Lon;
               boat3.lat_start = boat3.Lat;
               boat3.lon_start = boat3.Lon;*/
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
                    this.Start.Text = "Stop";

                }
                else if (this.Start.Text == "Stop")
                {
                    isFlagDraw = false;
                    isFlagCtrl = false;//控制线程标志
                    timer1.Enabled = false;//坐标跟新
                    this.Start.Text = "Start";
                }

            }
        }
        static bool swich_flag = false;
        private void Switch_Click(object sender, EventArgs e)
        {
            if (swich_flag == false)
            {
                command[0] = 0xa1;
                command[1] = 0x1a;
                command[3] = 0x59;
                serialPort1.Write(command, 0, 6);//引脚拉高
                swich_flag = true;
                this.Switch.Text = "Manual";
            }
            else
            {
                command[0] = 0xa1;
                command[1] = 0x1a;
                command[3] = 0x5A;
                serialPort1.Write(command, 0, 6);
                // serialPort1.Write("Z");//引脚拉低
                swich_flag = false;
                this.Switch.Text = "Auto";
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
                command[0] = 0xa1;
                command[1] = 0x1a;
                command[3] = 0x42;
                serialPort1.Write(command, 0, 6);
                //  serialPort1.Write("B");
            }

        }



        private void button1_Click(object sender, EventArgs e)
        {
            if (button1.Text == "Start following")
            {
                name = DateTime.Now.ToString("yyyyMMddHHmmss");//保存数据txt
                timer1.Enabled = false;//首先关闭开环定时器获取当前状态信息的定时器
                isFlagCtrl = true;
                Thread threadControl = new Thread(Control_PF);
                threadControl.IsBackground = true;
                threadControl.Start();
                button1.Text = "Stop following";

            }
            else
            {
                timer1.Enabled = true;//关闭闭环控制后，重新开启开环获取船位姿状态信息
                isFlagCtrl = false;
                ship1Control.Stop_Robot(serialPort1);
                Thread.Sleep(40);
                ship2Control.Stop_Robot(serialPort1);
                Thread.Sleep(40);
                ship3Control.Stop_Robot(serialPort1);
                button1.Text = "Start following";
            }

        }

        private void UpdateCtrlPhi()
        {
            if (Phi_mode.Text == "Heading angle")
            {
                boat1.Control_Phi = boat1.phi;
                boat2.Control_Phi = boat2.phi;
                boat3.Control_Phi = boat3.phi;
            }
            else
            {
                if(TckAngFter.Checked)//是否滤波
                {
                    boat1.Control_Phi = boat1.Fter_GPS_Phi;
                    boat2.Control_Phi = boat2.Fter_GPS_Phi;
                    boat3.Control_Phi = boat3.Fter_GPS_Phi;
                }
                else
                {
                    boat1.Control_Phi = boat1.GPS_Phi;
                    boat2.Control_Phi = boat2.GPS_Phi;
                    boat3.Control_Phi = boat3.GPS_Phi;
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
        }
        
        private void UpdateCtrlOutput()
        {
            tarLineSp = float.Parse(line_Y1.Text);//1号船目标线和圆
            tarCircle.Radius = float.Parse(circle_R1.Text);
            tarCircle.x = float.Parse(circle_X.Text);
            tarCircle.y = float.Parse(circle_Y.Text);

            ship1Control.command[3] = Control_fun(ship1Control, boat1);//1号小船控制
            if (AutoSpeed.Checked)
                ship1Control.command[4] = ship1Control.Closed_Control_LineSpeed(boat1, boat2, isCirPath, isFlagDir);
            else 
                ship1Control.command[4] = (byte)(int.Parse(Manualspeedset.Text)); 
 
            boat1.CtrlRudOut = ship1Control.command[3];//舵角控制输出量
            boat1.CtrlSpeedOut = ship1Control.command[4];//速度控制输出量
            boat1.XError = boat2.pos_X - boat1.pos_X;
            ship1Control.Send_Command(serialPort1);
            ship1Control.Get_ShipData(serialPort1);

            tarLineSp = float.Parse(line_Y2.Text);//2号船目标线和圆
            tarCircle.Radius = float.Parse(circle_R2.Text);
            ship2Control.command[3] = Control_fun(ship2Control, boat2);//2号小船控制，2号小船为leader，无需控制速度
            if (AutoSpeed.Checked)
                ship2Control.command[4] = ship2Control.Closed_Control_LineSpeed(boat2, boat2, isCirPath, isFlagDir);
            else 
                ship2Control.command[4] = (byte)(int.Parse(Manualspeedset.Text));  
         //   ship2Control.command[4] = 100;
            boat2.CtrlRudOut = ship2Control.command[3];//舵角控制输出量
            boat2.CtrlSpeedOut = ship2Control.command[4];//速度控制输出量
            boat2.XError = boat1.pos_X - boat3.pos_X;
            ship2Control.Send_Command(serialPort1);
            ship2Control.Get_ShipData(serialPort1);

            tarLineSp = float.Parse(line_Y3.Text);//2号船目标线和圆
            tarCircle.Radius = float.Parse(circle_R3.Text);
            ship3Control.command[3] = Control_fun(ship3Control, boat3);//3号小船控制
            if (AutoSpeed.Checked)
                ship3Control.command[4] = ship3Control.Closed_Control_LineSpeed(boat3, boat2, isCirPath, isFlagDir);
            else 
                ship3Control.command[4] = (byte)(int.Parse(Manualspeedset.Text));
           // ship3Control.command[4] = 110;
            boat3.CtrlRudOut = ship3Control.command[3];//舵角控制输出量
            boat3.CtrlSpeedOut = ship3Control.command[4];//速度控制输出量
            boat3.XError = boat2.pos_X - boat3.pos_X;
            ship3Control.Send_Command(serialPort1);
            ship3Control.Get_ShipData(serialPort1);

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
                Thread.Sleep(195);//控制周期
            }
        }
        
        private byte Control_fun(RobotControl shipControl, ShipData shipData)
        {
            byte rudder = 0;
            #region 跟踪目标点
            if (path_mode.Text == "Point")
            {
               rudder =  shipControl.FollowPoint(shipData, tarPoint);
            }
            #endregion

            #region 跟随一般直线
            if (path_mode.Text == "General line")
            {
                rudder = shipControl.FollowLine(shipData, tarLineGe);
                isCirPath = false;//直线
            }
            #endregion

            #region 跟随特殊直线
            if (path_mode.Text == "Special line")
            {
                rudder = shipControl.FollowLine(shipData, tarLineSp);
                isCirPath = false;
            }
            #endregion

            #region 跟随多段直线
            if (path_mode.Text == "Multi line")
            {
                rudder = shipControl.FollowMulLine(shipData);
                isCirPath = false;
            }
            #endregion

            #region 跟随圆轨迹
            if (path_mode.Text == "Circular path")
            {
                rudder = shipControl.FollowCircle(shipData, tarCircle);
                isCirPath = true;
            }
            #endregion

            #region 跟随椭圆
            if (path_mode.Text == "Oval path")
            {
                rudder = shipControl.FollowOval(shipData, tarOval);
                isCirPath = false;
            }
            #endregion

            return rudder;
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
        private void Form1_Load(object sender, EventArgs e)
        {
            InitRecTable();
            Control.CheckForIllegalCrossThreadCalls = false;
            
        }
        
        private void PathMap_MouseDown(object sender, MouseEventArgs e)
        {
            target_pt = new Point(e.X, e.Y);
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
                                tarOval.B1 = (tarOval.Pt1.Y- tarOval.K1 * tarOval.Pt1.X);//截距 毫米单位
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
                        for (int i = 0; i < tarMultiLine.Count;i++ )
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

    }
}
