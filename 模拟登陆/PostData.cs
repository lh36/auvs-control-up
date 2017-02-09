using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Threading;
using System.Net;
using System.IO;
using System.IO.Compression;
using System.Text.RegularExpressions;
using System.Runtime.InteropServices;
using System.Web;


namespace 模拟登陆
{
    class Test
    {
        /// </summary>
        /// RequestUrl: request some url, return the response.
        /// 适合方式:POST/GET
        /// </summary>
        /// <param name="strUrl">the url</param>
        /// <param name="strPostDatas">the post data</param>
        /// <param name="method">发送方式："POST"或"GET"</param>
        /// <param name="objencoding">编码方式("utf-8")</param>
        /// <param name="objCookieContainer">cookie's(session's) container</param>
        /// <param name="rescookie">返回string类型的cookie</param>
        /// <param name="flag">flag = false 直接发送 不分割 （默认）</param>
        /// <returns>返回HTML页面</returns>
        public static string RequestUrl(string strUrl, string strPostDatas, string method, string objencoding, ref CookieContainer objCookieContainer, bool flag, ref string rescookie)
        {
            HttpWebResponse res = null;
            string strResponse = "";
            //string strcookie = "";
            try
            {
                HttpWebRequest req = (HttpWebRequest)WebRequest.Create(strUrl);
                req.Method = method;
                req.KeepAlive = true;
                req.ContentType = "application/x-www-form-urlencoded";
                //req.Timeout = 20;

                if (objCookieContainer == null)
                    objCookieContainer = new CookieContainer();
                req.CookieContainer = objCookieContainer;

                StringBuilder objEncodedPostDatas = new StringBuilder();
                byte[] postDatas = null;

                req.ContentLength = 0;
                #region SendData
                if (strPostDatas != null && strPostDatas.Length > 0)
                {
                    if (flag == true)
                    {
                        string[] datas = strPostDatas.TrimStart('?').Split(new char[] { '&' });
                        for (int i = 0; i < datas.Length; i++)
                        {
                            string[] keyValue = datas[i].Split(new char[] { '=' });
                            if (keyValue.Length >= 2)
                            {
                                objEncodedPostDatas.Append(HttpUtility.UrlEncode(keyValue[0]));
                                objEncodedPostDatas.Append("=");
                                objEncodedPostDatas.Append(HttpUtility.UrlEncode(keyValue[1]));
                                if (i < datas.Length - 1)
                                {
                                    objEncodedPostDatas.Append("&");
                                }
                            }
                        }
                        //postDatas = Encoding.UTF8.GetBytes(objEncodedPostDatas.ToString());
                        postDatas = Encoding.GetEncoding(objencoding.Trim()).GetBytes(objEncodedPostDatas.ToString());
                    }
                    else
                    {
                        //postDatas = Encoding.UTF8.GetBytes(strPostDatas);
                        postDatas = Encoding.GetEncoding(objencoding.Trim()).GetBytes(strPostDatas);
                    }

                    req.ContentLength = postDatas.Length;
                    using (Stream reqStream = req.GetRequestStream())
                    {
                        reqStream.Write(postDatas, 0, postDatas.Length);
                    }
                }
                #endregion
                res = (HttpWebResponse)req.GetResponse();
                objCookieContainer = req.CookieContainer;
                CookieCollection mycookie = objCookieContainer.GetCookies(req.RequestUri);
                foreach (Cookie cook in mycookie)
                {
                    rescookie = cook.Name + "=" + cook.Value;
                }
                using (Stream resStream = res.GetResponseStream())
                {
                    using (StreamReader sr = new StreamReader(resStream, System.Text.Encoding.GetEncoding(objencoding.Trim())))
                    {
                        strResponse = sr.ReadToEnd();
                    }
                }
            }
            //catch (Exception ex)
            //{
            //    strResponse = ex.ToString();
            //}  
            finally
            {
                if (res != null)
                {
                    res.Close();
                }
            }
            return strResponse;
        }
    }
}
