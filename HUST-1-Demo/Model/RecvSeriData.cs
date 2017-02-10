using HUST_1_Demo.Controller;
using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HUST_1_Demo.Model
{
    public static class RecvSeriData
    {
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

        public void ReceData(SerialPort serialPort1)
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
                    case 0x01: boat1 = RobotControl.UpdataStatusData(boat1, response_data); break;
                    case 0x02: boat2 = RobotControl.UpdataStatusData(boat2, response_data); break;
                    case 0x03: boat3 = RobotControl.UpdataStatusData(boat3, response_data); break;
                    default: break;
                }
                Array.Clear(response_data, 0, response_data.Length);
                Display();//有数据更新时才更新显示，否则不更新（即不是每次接收到数据才更新，只有接收到正确的数据才更新）
            }
            #endregion
        }
    }
}
