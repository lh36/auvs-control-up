using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace 模拟上传
{
    class Program
    {
        /** 
         * 如果要在客户端向服务器上传文件，我们就必须模拟一个POST multipart/form-data类型的请求 
         * Content-Type必须是multipart/form-data。 
         * 以multipart/form-data编码的POST请求格式与application/x-www-form-urlencoded完全不同 
         * multipart/form-data需要首先在HTTP请求头设置一个分隔符,例如7d4a6d158c9： 
         * 我们模拟的提交要设定 content-type不同于非含附件的post时候的content-type 
         * 这里需要： ("Content-Type", "multipart/form-data; boundary=ABCD"); 
         * 然后，将每个字段用“--7d4a6d158c9”分隔，最后一个“--7d4a6d158c9--”表示结束。 
         * 例如，要上传一个title字段"Today"和一个文件C:\1.txt，HTTP正文如下： 
         *  
         * --7d4a6d158c9 
         * Content-Disposition: form-data; name="title" 
         * \r\n\r\n 
         * Today 
         * --7d4a6d158c9 
         * Content-Disposition: form-data; name="1.txt"; filename="C:\1.txt" 
         * Content-Type: text/plain 
         * 如果是图片Content-Type: application/octet-stream 
         * \r\n\r\n 
         * <这里是1.txt文件的内容> 
         * --7d4a6d158c9 
         * \r\n 
         * 请注意，每一行都必须以\r\n结束value前面必须有2个\r\n，包括最后一行。 
        */
        static void Main(string[] args)
        {
            string boundary = "----------" + DateTime.Now.Ticks.ToString("x");//元素分割标记 
            StringBuilder sb = new StringBuilder();
            sb.Append("--" + boundary);
            sb.Append("\r\n");
            sb.Append("Content-Disposition: form-data; name=\"file1\"; filename=\"D:\\upload.xml" + "\"");
            sb.Append("\r\n");
            sb.Append("Content-Type: application/octet-stream");
            sb.Append("\r\n");
            sb.Append("\r\n");//value前面必须有2个换行  

            HttpWebRequest request = (HttpWebRequest)WebRequest.Create("http://localhost:3364/");
            request.ContentType = "multipart/form-data; boundary=" + boundary;//其他地方的boundary要比这里多--  
            request.Method = "POST";

            FileStream fileStream = new FileStream(@"D:\123.xml", FileMode.OpenOrCreate, FileAccess.Read);

            byte[] postHeaderBytes = Encoding.UTF8.GetBytes(sb.ToString());
            byte[] boundaryBytes = Encoding.ASCII.GetBytes("\r\n--" + boundary + "\r\n");
            //http请求总长度  
            request.ContentLength = postHeaderBytes.Length + fileStream.Length + boundaryBytes.Length;
            Stream requestStream = request.GetRequestStream();
            requestStream.Write(postHeaderBytes, 0, postHeaderBytes.Length);
            byte[] buffer = new Byte[checked((uint)Math.Min(4096, (int)fileStream.Length))];
            int bytesRead = 0;
            while ((bytesRead = fileStream.Read(buffer, 0, buffer.Length)) != 0)
            {
                requestStream.Write(buffer, 0, bytesRead);
            }
            fileStream.Dispose();
            requestStream.Write(boundaryBytes, 0, boundaryBytes.Length);
            WebResponse webResponse2 = request.GetResponse();
            Stream htmlStream = webResponse2.GetResponseStream();
            string HTML = GetHtml(htmlStream, "UTF-8");
            Console.WriteLine(HTML);
            htmlStream.Dispose();
        }

        private static string GetHtml(Stream htmlStream, string p)
        {
            throw new NotImplementedException();
        }
    }
    public class HTMLHelper
    {
        /// <summary>
        /// 获取CooKie
        /// </summary>
        /// <param name="loginUrl"></param>
        /// <param name="postdata"></param>
        /// <param name="header"></param>
        /// <returns></returns>
        public static CookieContainer GetCooKie(string loginUrl, string postdata, HttpHeader header)
        {
            HttpWebRequest request = null;
            HttpWebResponse response = null;
            try
            {
                CookieContainer cc = new CookieContainer();
                request = (HttpWebRequest)WebRequest.Create(loginUrl);
                request.Method = header.method;
                request.ContentType = header.contentType;
                byte[] postdatabyte = Encoding.UTF8.GetBytes(postdata);     //提交的请求主体的内容
                request.ContentLength = postdatabyte.Length;    //提交的请求主体的长度
                request.AllowAutoRedirect = false;
                request.CookieContainer = cc;
                request.KeepAlive = true;

                //提交请求
                Stream stream;
                stream = request.GetRequestStream();
                stream.Write(postdatabyte, 0, postdatabyte.Length);     //带上请求主体
                stream.Close();

                //接收响应
                response = (HttpWebResponse)request.GetResponse();      //正式发起请求
                response.Cookies = request.CookieContainer.GetCookies(request.RequestUri);

                CookieCollection cook = response.Cookies;
                //Cookie字符串格式
                string strcrook = request.CookieContainer.GetCookieHeader(request.RequestUri);

                return cc;
            }
            catch (Exception ex)
            {

                throw ex;
            }
        }

        /// <summary>
        /// 获取html
        /// </summary>
        /// <param name="getUrl"></param>
        /// <param name="cookieContainer"></param>
        /// <param name="header"></param>
        /// <returns></returns>
        public static string GetHtml(string getUrl, CookieContainer cookieContainer, HttpHeader header)
        {
            Thread.Sleep(1000);
            HttpWebRequest httpWebRequest = null;
            HttpWebResponse httpWebResponse = null;
            try
            {
                httpWebRequest = (HttpWebRequest)HttpWebRequest.Create(getUrl);
                httpWebRequest.CookieContainer = cookieContainer;
                httpWebRequest.ContentType = header.contentType;
                httpWebRequest.ServicePoint.ConnectionLimit = header.maxTry;
                httpWebRequest.Referer = getUrl;
                httpWebRequest.Accept = header.accept;
                httpWebRequest.UserAgent = header.userAgent;
                httpWebRequest.Method = "GET";
                httpWebResponse = (HttpWebResponse)httpWebRequest.GetResponse();
                Stream responseStream = httpWebResponse.GetResponseStream();
                StreamReader streamReader = new StreamReader(responseStream, Encoding.UTF8);
                string html = streamReader.ReadToEnd();
                streamReader.Close();
                responseStream.Close();
                httpWebRequest.Abort();
                httpWebResponse.Close();
                return html;
            }
            catch //(Exception e)
            {
                if (httpWebRequest != null) httpWebRequest.Abort();
                if (httpWebResponse != null) httpWebResponse.Close();
                return string.Empty;
            }
        }
    }
    
    public class HttpHeader
    {
        public string contentType { get; set; }

        public string accept { get; set; }

        public string userAgent { get; set; }

        public string method { get; set; }

        public int maxTry { get; set; }
    }
}
