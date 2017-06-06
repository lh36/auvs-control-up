using HUST_1_Demo.Controller;
using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HUST_1_Demo.Model
{
    public class RecvSeriData
    {
        byte[] Euler_Z = new byte[4];//Yaw角度
        static byte[] rxdata = new byte[200];//数据接收二级缓存，用来存储寻找包含包头包尾的数据
        public static byte[] response_data = new byte[26];//下位机回复报文
        public static bool bDataGet = false;//获取一组正确的数据标志
        static byte[] recv = new byte[200];//这里定义的是临时局部变量，所以每次进来都会重新更新，所以不用清空
        static int rx_counter = 0;
        static byte head1 = 0xA5;
        static byte head2 = 0x5A;
        static byte tail = 0xAA;
        static int head_pos = 0;//报头位置
        static int tail_pos = 0;//报尾位置

        public static void ReceData(SerialPort serialPort1)
        {
            int len = serialPort1.BytesToRead;
            if(len<200)
            {
                serialPort1.Read(recv, 0, len);
            }
            
            if (len != 0)
            {
                try
                {
                    for (int i = 0; i < len; i++)
                    {
                        rxdata[rx_counter] = recv[i];//接收数   据二级缓存，用来进行包头包尾寻找
                        rx_counter++;
                    }
                    Array.Clear(recv, 0, recv.Length);
                }
                catch
                {
                    rx_counter = 0;
                    Array.Clear(rxdata, 0, rxdata.Length);
                    Array.Clear(recv, 0, recv.Length);
                }

                #region 数据接收程序
                if (rx_counter >= 26)
                {
                    head_pos = Array.IndexOf(rxdata, head1);

                    if ((head_pos != -1) && (rxdata[head_pos + 1] == head2))
                    {
                        tail_pos = Array.IndexOf(rxdata, tail, head_pos);//从包头处开始寻找包尾

                        #region 找到包头和包尾
                        if (tail_pos != -1)//找到了一组完整的数据
                        {
                            for (int i = 0; i < rxdata[head_pos + 2]; i++)
                            {
                                response_data[i] = rxdata[head_pos + i];
                            }
                            bDataGet = true;

                            if (response_data[25] == tail)
                            {
                                rx_counter = rx_counter - (head_pos + 26);//除去最后一个包尾后剩余的数据的个数

                                for (int i = 0; i < rx_counter; i++)//保留缓存区包尾后的数据
                                {
                                    rxdata[i] = rxdata[head_pos + i + 26];
                                }
                                Array.Clear(rxdata, rx_counter, 200 - rx_counter);//清除遗留的尾数
                            }
                            else if ((rxdata[25] != 0) || (rx_counter > 26))
                            {
                                rx_counter = 0;
                                Array.Clear(rxdata, 0, rxdata.Length);//清除遗留的尾数
                            }
                        }
                        #endregion

                        #region 找到包头未找到包尾
                        else //未找到报尾则清除报头之前的数据
                        {
                            rx_counter = rx_counter - head_pos;
                            for (int i = 0; i < rx_counter; i++)
                            {
                                rxdata[i] = rxdata[head_pos + i];
                            }
                        }
                        #endregion
                    }
                }
                #endregion
            }

        }
    }
}