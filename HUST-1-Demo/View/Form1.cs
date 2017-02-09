using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
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
        static double a = 6378137.0;//定义地球长半轴长度  
        static double earth_e = 0.003352810664; //定义椭球的第一偏心律
        static double lat_start = 30.51584003;//定义原点位置
        static double lon_start = 114.42665029;

      //  static double lat_start = 0;//定义原点位置
      //  static double lon_start = 0;

        struct ship_state  //船舶状态信息
        {

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

        ship_state boat1 = new ship_state();//保存三艘船的状态信息
        ship_state boat2 = new ship_state();
        ship_state boat3 = new ship_state();

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
                    ComOpen1.Text = "关闭串口";
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
                ComOpen1.Text = "打开串口";
                ComPortNum1.Enabled = true;
                BaudRate1.Enabled = true;
            }
        }
        /*前进*/
        private void Advance_Click(object sender, EventArgs e)
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
                command[3] = 0x49;
                serialPort1.Write(command, 0, 6);
            }


        }
        /*后退*/
        private void Back_Click(object sender, EventArgs e)
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
                command[3] = 0x44;
                serialPort1.Write(command, 0, 6);
                //  serialPort1.Write("D");
                //      MethodInvoker invoker1 = () => boat1_speed.Text = (speed_boat / 1000.0).ToString();
                //      phi_B1.BeginInvoke(invoker1);
            }

        }


        private void Stop_Click(object sender, EventArgs e)
        {
            if (!serialPort1.IsOpen)//由于画图需要打开串口，因此先判断串口状态，若没打开则先打开
            {
                MessageBox.Show("请先打开串口！\r\n");
            }
            {
                command[0] = 0xa1;
                command[1] = 0x1a;
                command[3] = 0x53;
                serialPort1.Write(command, 0, 6);
                Thread.Sleep(40);
                command[0] = 0xa2;
                command[1] = 0x2a;
                command[3] = 0x53;
                serialPort1.Write(command, 0, 6);
                Thread.Sleep(40);
                command[0] = 0xa3;
                command[1] = 0x3a;
                command[3] = 0x53;
                serialPort1.Write(command, 0, 6);
            }

        }


        byte[] Euler_Z = new byte[4];//Yaw角度
        byte[] rxdata = new byte[200];//数据接收二级缓存，用来存储寻找包含包头包尾的数据
        static byte[] response_data = new byte[26];//下位机回复报文
        byte[] rxbuffer = new byte[200];//这里定义的是临时局部变量，所以每次进来都会重新更新，所以不用清空
        int rx_counter = 0;
        byte head1 = 0xA5;
        byte head2 = 0x5A;
        byte tail = 0xAA;
        static int head_pos = 0;//报头位置
        static int tail_pos = 0;//报尾位置

        private void serialPort1_DataReceived(object sender, System.IO.Ports.SerialDataReceivedEventArgs e)
        {
            int rxdatalen = serialPort1.BytesToRead;
            serialPort1.Read(rxbuffer, 0, rxdatalen);
            if (rxdatalen != 0)//只要有数据过来就进行接收并存储
            {
                for (int i = 0; i < rxdatalen; i++)
                {
                    rxdata[rx_counter] = rxbuffer[i];//接收数据二级缓存，用来进行包头包尾寻找
                    rx_counter++;
                }
                Array.Clear(rxbuffer, 0, rxbuffer.Length);
                #region 接收数据大于26字节进入处理
                if (rx_counter >= 26)
                {
                    // rx_counter = 0;//二级缓存清空，防止数据溢出  这里如果采用清零的方式，则可能出现数据从中间开始，则一直找不到一组完整的数据，或者错误的数据。
                    head_pos = Array.IndexOf(rxdata, head1);
                    if (head_pos != -1)
                    {
                        tail_pos = Array.IndexOf(rxdata, tail, head_pos);//从包头处开始寻找包尾
                        if (tail_pos != -1)//找到了一组完整的数据
                        {
                            for (int i = 0; i < tail_pos - head_pos + 1; i++)
                            {
                                response_data[i] = rxdata[head_pos + i];
                            }

                            int last_tail_pos = Array.LastIndexOf(rxdata, tail);//找到数组中最后一个包尾位置，之前数据全部除去
                            rx_counter = rx_counter - (last_tail_pos + 1);//除去最后一个包尾后剩余的数据的个数

                            for (int i = 0; i < rx_counter; i++)//保留缓存区包尾后的数据
                            {
                                rxdata[i] = rxdata[last_tail_pos + i + 1];
                            }
                            Array.Clear(rxdata, rx_counter, 100);//清除遗留的尾数
                        }
                        else
                        {
                            rx_counter = rx_counter - head_pos;
                            for (int i = 0; i < rx_counter; i++)
                            {
                                rxdata[i] = rxdata[head_pos + i];
                            }
                        }

                    }
                }
                #endregion 26字节处理完毕
            }
            #region 接收到一组正确的数据，则进行处理和显示
            if ((response_data[1] == head2) && (response_data[25] == tail))//第二位是0X5A时处理数据，否则丢弃数据 
            {
                byte ID_Temp = response_data[3];
                switch (ID_Temp)
                {
                    case 0x01: boat1 = Handle_response_data(boat1); break;
                    case 0x02: boat2 = Handle_response_data(boat2); break;
                    case 0x03: boat3 = Handle_response_data(boat3); break;
                    default: break;
                }
                Array.Clear(response_data, 0, response_data.Length);
                Display();//有数据更新时才更新显示，否则不更新（即不是每次接收到数据才更新，只有接收到正确的数据才更新）
            }
            #endregion
        }

        
        private ship_state Handle_response_data(ship_state boat)
        {
            boat.Lat = ((response_data[4] << 24) + (response_data[5] << 16) + (response_data[6] << 8) + response_data[7]) / Math.Pow(10, 8) + 30;
            boat.Lon = ((response_data[8] << 24) + (response_data[9] << 16) + (response_data[10] << 8) + response_data[11]) / Math.Pow(10, 8) + 114;

            boat.pos_X = (boat.Lat - lat_start) * a * (1 - Math.Pow(earth_e, 2)) * 3.1415926 / (180 * Math.Sqrt(Math.Pow((1 - Math.Pow(earth_e * Math.Sin(boat.Lat / 180 * 3.1415926), 2)), 3)));
            boat.pos_Y = -((boat.Lon - lon_start) * a * Math.Cos(boat.Lat / 180 * 3.1415926) * 3.1415926 / (180 * Math.Sqrt(1 - Math.Pow(earth_e * Math.Sin(boat.Lat / 180 * 3.1415926), 2))));//Y坐标正向朝西
            boat.GPS_Phi =  Math.Atan2(boat.pos_Y - boat.last_pos_Y, boat.pos_X - boat.last_pos_X) / Math.PI * 180;//航迹角
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

            //GPS数据和惯导数据保存
            using (FileStream fs = new FileStream(@"D:\" + name + ".txt", FileMode.Append))
            {
                //string strX = str_lat + ",";
                //string strY = str_lon + ",";
                //string strA = boat.phi.ToString() + ",";//保存航向角
                string str_data = response_data[3].ToString() + "," + boat.Lat.ToString("0.00000000") + "," + boat.Lon.ToString("0.00000000") + "," + boat.pos_X.ToString() + "," + boat.pos_Y.ToString() + "," + boat.phi.ToString() + ","
                          + boat.GPS_Phi.ToString() + "," + boat.speed.ToString("0.000") + "," + boat.gear.ToString() + "," + boat.Time.ToString();//将数据转换为字符串

                byte[] data = System.Text.Encoding.Default.GetBytes(str_data);
                byte[] data3 = new byte[2];
                data3[0] = 0x0d; data3[1] = 0x0a;
                //开始写入
                fs.Write(data, 0, data.Length);

                fs.Write(data3, 0, data3.Length);

                //清空缓冲区、关闭流
                fs.Flush();
                fs.Close();
            }
            return boat;
        }
        //参数显示函数
        private void Display()
        {
            Boat1_X.Text = boat1.pos_X.ToString("0.00");
            Boat1_Y.Text = boat1.pos_Y.ToString("0.00");
            Boat1_phi.Text = boat1.phi.ToString("0.00");
            Boat1_Ru.Text = boat1.rud.ToString("0.0");
            Boat1_speed.Text = boat1.speed.ToString("0.000");
            Boat1_grade.Text = boat1.gear.ToString();
            Boat1_time.Text = boat1.Time;

            Boat2_X.Text = boat2.pos_X.ToString("0.00");
            Boat2_Y.Text = boat2.pos_Y.ToString("0.00");
            Boat2_phi.Text = boat2.phi.ToString("0.00");
            Boat2_Ru.Text = boat2.rud.ToString("0.0");
            Boat2_speed.Text = boat2.speed.ToString("0.000");
            Boat2_grade.Text = boat2.gear.ToString();
            Boat2_time.Text = boat2.Time;

            Boat3_X.Text = boat3.pos_X.ToString("0.00");
            Boat3_Y.Text = boat3.pos_Y.ToString("0.00");
            Boat3_phi.Text = boat3.phi.ToString("0.00");
            Boat3_Ru.Text = boat3.rud.ToString("0.0");
            Boat3_speed.Text = boat3.speed.ToString("0.000");
            Boat3_grade.Text = boat3.gear.ToString();
            Boat3_time.Text = boat3.Time;
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
                command[3] = 0x51;
                serialPort1.Write(command, 0, 6);
                //serialPort1.Write("Q");
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
                command[3] = 0x52;
                serialPort1.Write(command, 0, 6);
                //  serialPort1.Write("R");
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
            while (flag_draw)
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

                #region 画目标直线和圆
                if (flag_ctrl)
                {
                    if (path_mode.Text == "直线")
                    {
                        int x = Widthmap - (int)(Convert.ToInt32(this.line_Y.Text) * 1000 * paint_scale);
                        g.DrawLine(new Pen(Color.Blue, 2), x, 0, x, PathMap.Height);
                    }
                    else if (path_mode.Text == "圆轨迹")
                    {
                        int x = Convert.ToInt32(this.circle_X.Text);
                        int y = Convert.ToInt32(this.circle_Y.Text);
                        int r = Convert.ToInt32(this.circle_R.Text);

                        int x1 = Widthmap - (int)((y + r) * 1000 * paint_scale);
                        int y1 = Heightmap - (int)((x + r) * 1000 * paint_scale);

                        g.DrawEllipse(new Pen(Color.Cyan, 2), x1, y1, (int)(r * 1000 * paint_scale) * 2, (int)(r * 1000 * paint_scale) * 2);

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
                if (path_mode.Text == "目标点")//绘制目标点
                {
                    g.DrawRectangle(new Pen(Color.Red, 2), target_pt.X - 4, target_pt.Y - 4, 4, 4);
                }
                Thread.Sleep(200);
            }

        }

        string name = DateTime.Now.ToString("yyyyMMddHHMMss");

        private void timer1_Tick(object sender, EventArgs e)
        {
            command[0] = 0xa1;
            command[1] = 0x1a;
            command[3] = 0x47;
            serialPort1.Write(command, 0, 6);
            Thread.Sleep(40);
            command[0] = 0xa2;
            command[1] = 0x2a;
            command[3] = 0x47;
            serialPort1.Write(command, 0, 6);
            Thread.Sleep(40);
            command[0] = 0xa3;
            command[1] = 0x3a;
            command[3] = 0x47;
            serialPort1.Write(command, 0, 6);
        }

        private void Reset_Click(object sender, EventArgs e)
        {
            listPoint_Boat1.Clear();
            listPoint_Boat2.Clear();
            listPoint_Boat3.Clear();
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
        static bool flag_ctrl = false;//绘画线程标志
        static bool flag_draw = false;//控制线程标志

        private void Start_Click(object sender, EventArgs e)
        {
            if (!serialPort1.IsOpen)//由于画图需要打开串口，因此先判断串口状态，若没打开则先打开
            {
                MessageBox.Show("请先打开串口！\r\n");
            }
            else
            {
                if (this.Start.Text == "开始")
                {
                    flag_draw = true;
                    Thread threadDraw = new Thread(DrawMap);
                    threadDraw.IsBackground = true;
                    threadDraw.Start();

                    timer1.Enabled = true;//默认是开环控制，则启动获取三船位姿线程
                    this.Start.Text = "停止";

                }
                else if (this.Start.Text == "停止")
                {
                    flag_draw = false;
                    flag_ctrl = false;//控制线程标志
                    timer1.Enabled = false;//坐标跟新
                    this.Start.Text = "开始";
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
                this.Switch.Text = "遥控";
            }
            else
            {
                command[0] = 0xa1;
                command[1] = 0x1a;
                command[3] = 0x5A;
                serialPort1.Write(command, 0, 6);
                // serialPort1.Write("Z");//引脚拉低
                swich_flag = false;
                this.Switch.Text = "自主";
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
            if (button1.Text == "开始跟随")
            {
                timer1.Enabled = false;//首先关闭开环定时器获取当前状态信息的定时器
                flag_ctrl = true;
                Thread threadControl = new Thread(Control_PF);
                threadControl.IsBackground = true;
                threadControl.Start();
                button1.Text = "停止跟随";

            }
            else
            {
                timer1.Enabled = true;//关闭闭环控制后，重新开启开环获取船位姿状态信息
                flag_ctrl = false;
                command[0] = 0xa1;
                command[1] = 0x1a;
                command[3] = 0x53;
                serialPort1.Write(command, 0, 6);
                Thread.Sleep(40);
                command[0] = 0xa2;
                command[1] = 0x2a;
                command[3] = 0x53;
                serialPort1.Write(command, 0, 6);
                Thread.Sleep(40);
                command[0] = 0xa3;
                command[1] = 0x3a;
                command[3] = 0x53;
                command[4] = 0;
                serialPort1.Write(command, 0, 6);
                button1.Text = "开始跟随";
            }

        }
        /// <summary>
        /// 控制线程
        /// </summary>

        double target_x;//定义目标点位姿信息
        double target_y;

        private void Control_PF()
        {
            while (flag_ctrl)
            {
                command[0] = 0xa1;
                command[1] = 0x1a;
                Control_fun(boat1);

                command[0] = 0xa2;
                command[1] = 0x2a; ;//2号小船控制
                Control_fun(boat2);

                command[0] = 0xa3;
                command[1] = 0x3a; ;//3号小船控制
                Control_fun(boat3);
                Thread.Sleep(200);
            }
        }
        private void Control_fun(ship_state boat)
        {
            double Kp = double.Parse(Kp_value.Text);//获取控制参数
            double Ki = double.Parse(Ki_value.Text);
            double Kd = double.Parse(Kd_value.Text);
            #region 跟踪目标点
            if (path_mode.Text == "目标点")
            {

                double current_c = boat.GPS_Phi;//实际航向

                double distance = Math.Sqrt((boat.X_mm - target_x) * (boat.X_mm - target_x) + (boat.Y_mm - target_y) * (boat.Y_mm - target_y));//毫米单位的距离

                if (distance <= 800.0d)
                {
                    command[3] = 0x53;
                    serialPort1.Write(command, 0, 6);

                    flag_ctrl = false;//控制结束，关闭控制标志，退出循环
                    button1.Text = "开始跟随";
                }

                else//距离目标点很远 需要控制
                {
                    double target_c = Math.Atan2(target_y - boat.Y_mm, target_x - boat.X_mm) / Math.PI * 180;

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


                    int R = (int)(Kp * detc);
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
                    
                    //速度控制
                    command[4] = 150;
                    if (serialPort1.IsOpen)//由于画图需要打开串口，因此先判断串口状态，若没打开则先打开
                    {
                        serialPort1.Write(command, 0, 6);
                    }

                    command[3] = 0x47;
                    if (serialPort1.IsOpen)//由于画图需要打开串口，因此先判断串口状态，若没打开则先打开
                    {
                        serialPort1.Write(command, 0, 6);
                    }
                }
            }
            #endregion

            #region 跟随直线
            if (path_mode.Text == "直线")
            {
                double k = 3.5d;//制导角参数
                double Err_phi = 0.0d;

                string y_str = line_Y.Text;
                double y_d = double.Parse(y_str) * 1000;

                double Ye = ((boat.Y_mm - y_d) / 1000);//实际坐标减参考坐标
                double Ref_phi = -Math.Atan(Ye / k) / Math.PI * 180;//制导角（角度制°）
                if (Phi_mode.Text == "航向角")
                {
                    Err_phi = Ref_phi - boat.phi;
                }
                else
                {
                    Err_phi = Ref_phi - boat.GPS_Phi;
                }
                if (Err_phi > 180)//偏差角大于180°时减去360°得到负值，表示航向左偏于制导角；偏差小于180°时表示航向右偏于制导角。
                {
                    Err_phi = Err_phi - 360;
                }
                int R = (int)(Kp * Err_phi);

                if (R > 32)
                {
                    R = 32;
                }
                else if (R < -32)
                {
                    R = -32;
                }
                R = R + 32;

                byte[] shi = BitConverter.GetBytes(R);
                command[3] = shi[0];
                if (serialPort1.IsOpen)//由于画图需要打开串口，因此先判断串口状态，若没打开则先打开
                {
                    serialPort1.Write(command, 0, 6);
                }
                command[3] = 0x47;
                command[4] = 150;
                if (serialPort1.IsOpen)//由于画图需要打开串口，因此先判断串口状态，若没打开则先打开
                {
                    serialPort1.Write(command, 0, 6);
                }
                
            }
            #endregion

            #region 跟随圆轨迹
            else if (path_mode.Text == "圆轨迹")
            {
                double Err_phi = 0.0d;
                double ROBOTphi_r = 0.0d;//相对参考向的航向角或航迹角
                double k = 3.5d;

                string str_R = circle_R.Text;//获取界面目标圆信息
                string str_X = circle_X.Text;
                string str_Y = circle_Y.Text;
                double Radius = double.Parse(str_R) * 1000;//目标圆半径
                double Center_X = double.Parse(str_X) * 1000;//圆心坐标
                double Center_Y = double.Parse(str_Y) * 1000;

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
                if (Phi_mode.Text == "航向角")
                {
                    ROBOTphi_r = boat.phi - Dir_R;
                }
                else
                {
                    ROBOTphi_r = boat.GPS_Phi - Dir_R;
                }
                
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

                int R = (int)(Kp * Err_phi);

                if (R > 32)
                {
                    R = 32;
                }
                else if (R < -32)
                {
                    R = -32;
                }
                R = R + 32;

                byte[] shi = BitConverter.GetBytes(R);
                command[3] = shi[0];
                command[4] = 150;
                if (serialPort1.IsOpen)//由于画图需要打开串口，因此先判断串口状态，若没打开则先打开
                {
                    serialPort1.Write(command, 0, 6);
                }
                command[3] = 0x47;
                if (serialPort1.IsOpen)//由于画图需要打开串口，因此先判断串口状态，若没打开则先打开
                {
                    serialPort1.Write(command, 0, 6);
                }
            }
            #endregion
        }
        private void Form1_Load(object sender, EventArgs e)
        {
            Control.CheckForIllegalCrossThreadCalls = false;
        }
        Point target_pt = new Point();
        private void PathMap_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                target_pt = new Point(e.X, e.Y);
                //   Graphics g = this.PathMap.CreateGraphics();
                //地图像素大小
                int Widthmap = PathMap.Width / 2;
                int Heightmap = PathMap.Height / 2;

                //实际大小
                int Heigh_mm = halfHeight_mm;
                int Width_mm = Heigh_mm / Heightmap * Widthmap;

                //比例尺和反比例尺
                double scale = Heigh_mm / Heightmap;//单位像素代表的实际长度，单位：mm
                double paint_scale = 1 / scale;//每毫米在图上画多少像素，单位：像素

                target_x = (Heightmap - target_pt.Y) * scale;
                target_y = (Widthmap - target_pt.X) * scale;

                this.tar_Point_X.Text = (target_x / 1000).ToString();
                this.tar_Point_Y.Text = (target_y / 1000).ToString();
                flag_ctrl = true;
            }
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
    }
}
