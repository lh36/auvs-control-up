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
        static byte[] response_data = new byte[26];//下位机回复报文
        static byte[] rxbuffer = new byte[200];//这里定义的是临时局部变量，所以每次进来都会重新更新，所以不用清空
        static int rx_counter = 0;
        static byte head1 = 0xA5;
        static byte head2 = 0x5A;
        static byte tail = 0xAA;
        static int head_pos = 0;//报头位置
        static int tail_pos = 0;//报尾位置

        public static byte[] ReceData(SerialPort serialPort1)
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
                            rx_counter = 0;
                            Array.Clear(rxdata, 0, rxdata.Length);//清除二级缓存
                        }
                        else
                        {
                            Array.Clear(rxdata, 0, rxdata.Length);//没有找到包尾，直接清除二级缓存
                            rx_counter = 0;
                        }
                    }
                }
                #endregion 26字节处理完毕

                #region 接收到一组正确的数据，则进行处理和显示
                if ((response_data[1] == head2) && (response_data[25] == tail))//第二位是0X5A时处理数据，否则丢弃数据 
                {
                    return response_data;
                }
                #endregion

                else
                {
                    return null;
                }
            }

            else
            {
                return null;
            }
            
        }
    }
}
