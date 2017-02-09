using System;
using System.Net;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Threading;
using Newtonsoft.Json;
using System.Collections;
using UnityEngine;

namespace 模拟登陆
{
    class Program
    {
        static void Main(string[] args)
        {
            string str1 = PostHttp("http://coolyiwen.cn:8001/ship/upper/updateParam?", "lat=3.0&lon=1.11&posX=1.1&posY=1.1&rud=1.1&speed=1.13&gear=2&time=1");
            string str2 = GetHttp("http://coolyiwen.cn:8001/ship/getparam", 50);

            LoginJson loginJson = JsonHelper.JsonToClass<LoginJson>(str2);
          //  Console.WriteLine(str1);//控制台打印
          //  Console.WriteLine(str2);//控制台打印
            Console.WriteLine(loginJson);//控制台打印
            Console.ReadKey();
        }

        /// <summary>
        /// 通过get方式请求页面，传递一个实例化的cookieContainer   Get方法获取页面方法一,可用
        /// </summary>
        /// <param name="postUrl"></param>
        /// <param name="cookie"></param>
        /// <returns></returns>
        public static ArrayList GetHtmlData(string postUrl, CookieContainer cookie)
        {
            HttpWebRequest request;
            HttpWebResponse response;
            ArrayList list = new ArrayList();
            request = WebRequest.Create(postUrl) as HttpWebRequest;
            request.Method = "GET";
            request.UserAgent = "Mozilla/4.0";
            request.CookieContainer = cookie;
            request.KeepAlive = true;

            request.CookieContainer = cookie;
            try
            {
                //获取服务器返回的资源
                using (response = (HttpWebResponse)request.GetResponse())
                {
                    using (StreamReader reader = new StreamReader(response.GetResponseStream(), Encoding.Default))
                    {
                        cookie.Add(response.Cookies);
                        //保存Cookies
                        list.Add(cookie);
                        list.Add(reader.ReadToEnd());
                        list.Add(Guid.NewGuid().ToString());//图片名
                    }
                }
            }
            catch (WebException ex)
            {
                list.Clear();
                list.Add("发生异常/n/r");
                WebResponse wr = ex.Response;
                using (Stream st = wr.GetResponseStream())
                {
                    using (StreamReader sr = new StreamReader(st, System.Text.Encoding.Default))
                    {
                        list.Add(sr.ReadToEnd());
                    }
                }
            }
            catch (Exception ex)
            {
                list.Clear();
                list.Add("5");
                list.Add("发生异常：" + ex.Message);
            }
            return list;
        }

        /// <summary>
        /// 发送相关数据至页面        post上传数据方法二，可用
        /// 进行登录操作
        /// 并保存cookie
        /// </summary>
        /// <param name="postData"></param>
        /// <param name="postUrl"></param>
        /// <param name="cookie"></param>
        /// <returns></returns>
        public static ArrayList PostData(string postData, string postUrl, CookieContainer cookie)
        {
            ArrayList list = new ArrayList();
            HttpWebRequest request;
            HttpWebResponse response;
            ASCIIEncoding encoding = new ASCIIEncoding();
            request = WebRequest.Create(postUrl) as HttpWebRequest;
            byte[] b = encoding.GetBytes(postData);
            request.UserAgent = "Mozilla/4.0";
            request.Method = "POST";
            request.CookieContainer = cookie;
            request.ContentLength = b.Length;
            using (Stream stream = request.GetRequestStream())
            {
                stream.Write(b, 0, b.Length);
            }

            try
            {
                //获取服务器返回的资源
                using (response = request.GetResponse() as HttpWebResponse)
                {
                    using (StreamReader reader = new StreamReader(response.GetResponseStream(), Encoding.UTF8))
                    {
                        if (response.Cookies.Count > 0)
                            cookie.Add(response.Cookies);
                        list.Add(cookie);
                        list.Add(reader.ReadToEnd());
                    }
                }
            }
            catch (WebException wex)
            {
                WebResponse wr = wex.Response;
                using (Stream st = wr.GetResponseStream())
                {
                    using (StreamReader sr = new StreamReader(st, System.Text.Encoding.Default))
                    {
                        list.Add(sr.ReadToEnd());
                    }
                }
            }
            catch (Exception ex)
            {
                list.Add("发生异常/n/r" + ex.Message);
            }
            return list;
        }
        
        /// <summary>
        /// 
        /// </summary>
        /// <param name="url">获取数据URL</param>
        /// <param name="time">超时时间</param>
        /// <returns>获取的数据</returns>
        public static string GetHttp(string url, int time)
        {
            HttpWebRequest httpWebRequest = null;
            HttpWebResponse httpWebResponse = null;
            try
            {
                httpWebRequest = (HttpWebRequest)WebRequest.Create(url);
                httpWebRequest.ContentType = "application/x-www-form-urlencoded";
                httpWebRequest.Method = "GET";
                httpWebRequest.Timeout = time;
                httpWebResponse = (HttpWebResponse)httpWebRequest.GetResponse();
                StreamReader streamReader = new StreamReader(httpWebResponse.GetResponseStream());
                string responseContent = streamReader.ReadToEnd();
                httpWebResponse.Close();
                streamReader.Close();
                httpWebRequest.Abort();
                httpWebResponse.Close();
                return responseContent; 
            }
            catch 
            {
                if (httpWebRequest != null) httpWebRequest.Abort();
                if (httpWebResponse != null) httpWebResponse.Close();
                return string.Empty;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="url">上传数据URL</param>
        /// <param name="body">拟上传的数据，格式为"lat=2.0&lon=1.11"</param>
        /// <returns>服务器响应</returns>
        public static string PostHttp(string url, string body) 
        { 
            HttpWebRequest httpWebRequest = (HttpWebRequest)WebRequest.Create(url);
            httpWebRequest.ContentType = "application/x-www-form-urlencoded"; 
            httpWebRequest.Method = "POST"; 
            httpWebRequest.Timeout = 20000; 
            byte[] btBodys = Encoding.UTF8.GetBytes(body); 
            httpWebRequest.ContentLength = btBodys.Length; 
            httpWebRequest.GetRequestStream().Write(btBodys, 0, btBodys.Length);
            HttpWebResponse httpWebResponse = (HttpWebResponse)httpWebRequest.GetResponse(); 
            StreamReader streamReader = new StreamReader(httpWebResponse.GetResponseStream()); 
            string responseContent = streamReader.ReadToEnd(); 
            httpWebResponse.Close(); 
            streamReader.Close(); 
            httpWebRequest.Abort(); 
            httpWebResponse.Close(); 
            return responseContent; 
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

    //**定义了JSON数据的接收类
    public class LoginJson
    {
        public bool status = false;
        public LoginResp resp = null;

        public LoginJson()
        { }
    }

    //**定义了JSON数据中Resp的数据类
    public class LoginResp
    {
        public double lat = 0.0d;
        public double speed = 0.0d;
        public double rud = 0.0d;
        public double posX = 0.0d;
        public string time = "";
        public double lon = 0.0d;
        public int gear = 0;
        public double posY = 0.0d;
        public LoginResp()
        { }
    }


}
