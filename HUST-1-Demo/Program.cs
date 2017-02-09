using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace HUST_1_Demo
{
    static class Program
    {
        /// <summary>
        /// 应用程序的主入口点。
        /// </summary>
        [STAThread]
        static void Main()
        {
            string str1 = Model.HttpWebModel.PostHttp("http://coolyiwen.cn:8001/ship/upper/updateParam?", "lat=3.0&lon=1.11&posX=1.1&posY=1.1&rud=1.1&speed=1.13&gear=2&time=1");
            string str2 = Model.HttpWebModel.GetHttp("http://coolyiwen.cn:8001/ship/getparam", 50);
            Model.LoginJson loginJson = Model.JsonHelper.JsonToClass<Model.LoginJson>(str2);
            
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new Form1());
        }
    }
}
