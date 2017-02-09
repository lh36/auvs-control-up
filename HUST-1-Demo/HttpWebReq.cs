using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace HUST_1_Demo
{
    class HttpWebReq
    {
        /// <summary>
        /// Post Http请求
        /// </summary>
        /// <param name="url">请求地址</param>
        /// <param name="postData">传输数据</param>
        /// <param name="timeout">超时时间</param>
        /// <param name="contentType">媒体格式</param>
        /// <param name="encode">编码</param>
        /// <returns>泛型集合</returns>
        public static List<T> PostAndRespList<T>(string url, string postData, int timeout = 5000, string contentType = "application/json;", string encode = "UTF-8")
        {
            if (!string.IsNullOrEmpty(url) && !string.IsNullOrEmpty(encode) && !string.IsNullOrEmpty(contentType) && postData != null)
            {
                // webRequest.Headers.Add("Authorization", "Bearer " + "SportApiAuthData");
                HttpWebResponse webResponse = null;
                Stream responseStream = null;
                Stream requestStream = null;
                StreamReader streamReader = null;
                try
                {
                    string respstr = GetStreamReader(url, responseStream, requestStream, streamReader, webResponse, timeout, encode, postData, contentType);
                    return JsonHelper.JsonDeserialize<List<T>>(respstr);

                }
                catch (Exception ex)
                {

                }
                finally
                {
                    if (responseStream != null) responseStream.Dispose();
                    if (webResponse != null) webResponse.Dispose();
                    if (requestStream != null) requestStream.Dispose();
                    if (streamReader != null) streamReader.Dispose();
                }
            }
            return null;
        }

        /// <summary>
        /// Post Http请求
        /// </summary>
        /// <param name="url">请求地址</param>
        /// <param name="postData">传输数据</param>
        /// <param name="timeout">超时时间</param>
        /// <param name="contentType">媒体格式</param>
        /// <param name="encode">编码</param>
        /// <returns>泛型集合</returns>
        public static T PostAndRespSignle<T>(string url, int timeout = 5000, string postData = "", string contentType = "application/json;", string encode = "UTF-8")
        {
            if (!string.IsNullOrEmpty(url) && !string.IsNullOrEmpty(encode) && !string.IsNullOrEmpty(contentType) && postData != null)
            {
                // webRequest.Headers.Add("Authorization", "Bearer " + "SportApiAuthData");
                HttpWebResponse webResponse = null;
                Stream responseStream = null;
                Stream requestStream = null;
                StreamReader streamReader = null;
                try
                {
                    string respstr = GetStreamReader(url, responseStream, requestStream, streamReader, webResponse, timeout, encode, postData, contentType);
                    return JsonHelper.JsonDeserialize<T>(respstr);
                }
                catch (Exception ex)
                {

                }
                finally
                {
                    if (responseStream != null) responseStream.Dispose();
                    if (webResponse != null) webResponse.Dispose();
                    if (requestStream != null) requestStream.Dispose();
                    if (streamReader != null) streamReader.Dispose();
                }
            }
            return default(T);
        }

        /// <summary>
        /// Post Http请求
        /// </summary>
        /// <param name="url"></param>
        /// <param name="postData"></param>
        /// <param name="timeout"></param>
        /// <param name="contentType"></param>
        /// <param name="encode"></param>
        /// <returns>响应流字符串</returns>
        public static string PostAndRespStr(string url, int timeout = 5000, string postData = "", string contentType = "application/json;", string encode = "UTF-8")
        {
            if (!string.IsNullOrEmpty(url) && !string.IsNullOrEmpty(encode) && !string.IsNullOrEmpty(contentType) && postData != null)
            {
                HttpWebResponse webResponse = null;
                Stream responseStream = null;
                Stream requestStream = null;
                StreamReader streamReader = null;
                try
                {

                    return GetStreamReader(url, responseStream, requestStream, streamReader, webResponse, timeout, encode, postData, contentType);
                }
                catch (Exception ex)
                {

                }
                finally
                {
                    if (responseStream != null) responseStream.Dispose();
                    if (webResponse != null) webResponse.Dispose();
                    if (requestStream != null) requestStream.Dispose();
                    if (streamReader != null) streamReader.Dispose();
                }
            }
            return null;
        }

        private static string GetStreamReader(string url, Stream responseStream, Stream requestStream, StreamReader streamReader, WebResponse webResponse, int timeout, string encode, string postData, string contentType)
        {
            byte[] data = Encoding.GetEncoding(encode).GetBytes(postData);
            HttpWebRequest webRequest = (HttpWebRequest)WebRequest.Create(url);
            SetAuth(webRequest);
            webRequest.Method = "POST";
            webRequest.ContentType = contentType + ";" + encode;
            webRequest.ContentLength = data.Length;
            webRequest.Timeout = timeout;
            requestStream = webRequest.GetRequestStream();
            requestStream.Write(data, 0, data.Length);
            webResponse = (HttpWebResponse)webRequest.GetResponse();
            responseStream = webResponse.GetResponseStream();
            if (responseStream == null) { return ""; }
            streamReader = new StreamReader(responseStream, Encoding.GetEncoding(encode));
            return streamReader.ReadToEnd();
        }

        /// <summary>
        /// Post文件流给指定Url
        /// </summary>
        /// <param name="url">url</param>
        /// <param name="filePath">文件路径</param>
        /// <returns>响应流字符串</returns>
        public static string PostFile(string url, string filePath, string contentType = "application/octet-stream", string encode = "UTF-8")
        {
            if (!string.IsNullOrEmpty(url) && !string.IsNullOrEmpty(encode) && !string.IsNullOrEmpty(contentType) && !string.IsNullOrEmpty(filePath))
            {

                Stream requestStream = null;
                Stream responseStream = null;
                StreamReader streamReader = null;
                FileStream fileStream = null;
                try
                {
                    // 设置参数
                    HttpWebRequest webRequest = WebRequest.Create(url) as HttpWebRequest;
                    SetAuth(webRequest);
                    webRequest.AllowAutoRedirect = true;
                    webRequest.Method = "POST";
                    string boundary = DateTime.Now.Ticks.ToString("X"); // 随机分隔线
                    webRequest.ContentType = "multipart/form-data;charset=" + encode + ";boundary=" + boundary;
                    byte[] itemBoundaryBytes = Encoding.UTF8.GetBytes("\r\n--" + boundary + "\r\n");//消息开始
                    byte[] endBoundaryBytes = Encoding.UTF8.GetBytes("\r\n--" + boundary + "--\r\n");//消息结尾
                    var fileName = filePath.Substring(filePath.LastIndexOf("/") + 1);
                    //请求头部信息
                    string postHeader = string.Format("Content-Disposition:form-data;name=\"media\";filename=\"{0}\"\r\nContent-Type:{1}\r\n\r\n", fileName, contentType);
                    byte[] postHeaderBytes = Encoding.UTF8.GetBytes(postHeader);
                    fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read);
                    byte[] fileByteArr = new byte[fileStream.Length];
                    fileStream.Read(fileByteArr, 0, fileByteArr.Length);
                    fileStream.Close();
                    requestStream = webRequest.GetRequestStream();
                    requestStream.Write(itemBoundaryBytes, 0, itemBoundaryBytes.Length);
                    requestStream.Write(postHeaderBytes, 0, postHeaderBytes.Length);
                    requestStream.Write(fileByteArr, 0, fileByteArr.Length);
                    requestStream.Write(endBoundaryBytes, 0, endBoundaryBytes.Length);
                    requestStream.Close();
                    responseStream = webRequest.GetResponse().GetResponseStream();//发送请求，得到响应流
                    if (responseStream == null) return string.Empty;
                    streamReader = new StreamReader(responseStream, Encoding.UTF8);
                    return streamReader.ReadToEnd();
                }
                catch (Exception ex)
                {

                }
                finally
                {

                }
            }
            return null;

        }

        /// <summary>
        /// Get http请求
        /// </summary>
        /// <param name="url">请求地址</param>
        /// <param name="timeout">超时时间</param>
        /// <param name="encode">编码</param>
        /// <returns>返回单个实体</returns>
        public static T GetSingle<T>(string url, int timeout = 5000, string encode = "UTF-8")
        {
            //HttpWebRequest对象
            //HttpClient->dudu 调用预热处理
            //Stream—>Model

            if (!string.IsNullOrEmpty(url) && !string.IsNullOrEmpty(encode))
            {
                Stream responseStream = null;
                StreamReader streamReader = null;
                WebResponse webResponse = null;
                try
                {
                    string respStr = GetRespStr(url, responseStream, streamReader, webResponse, timeout, encode);
                    return JsonHelper.JsonDeserialize<T>(respStr);
                }
                catch (Exception ex)
                {

                }
                finally
                {
                    if (responseStream != null) responseStream.Dispose();
                    if (streamReader != null) streamReader.Dispose();
                    if (webResponse != null) webResponse.Dispose();
                }
            }
            return default(T);
        }

        /// <summary>
        ///  Get http请求
        /// </summary>
        /// <param name="url"></param>
        /// <param name="timeout"></param>
        /// <param name="encode"></param>
        /// <returns>响应流字符串</returns>
        public static string GetResponseString(string url, int timeout = 5000, string encode = "UTF-8")
        {
            if (!string.IsNullOrEmpty(url) && !string.IsNullOrEmpty(encode))
            {
                Stream responseStream = null;
                StreamReader streamReader = null;
                WebResponse webResponse = null;
                try
                {
                    return GetRespStr(url, responseStream, streamReader, webResponse, timeout, encode);
                }
                catch (Exception ex)
                {

                }
                finally
                {
                    if (responseStream != null) responseStream.Dispose();
                    if (streamReader != null) streamReader.Dispose();
                    if (webResponse != null) webResponse.Dispose();
                }
            }
            return null;
        }

        private static string GetRespStr(string url, Stream responseStream, StreamReader streamReader, WebResponse webResponse, int timeout, string encode)
        {
            HttpWebRequest webRequest = (HttpWebRequest)WebRequest.Create(url);
            webRequest.Method = "GET";
            webRequest.Timeout = timeout;
            webResponse = webRequest.GetResponse();
            responseStream = webResponse.GetResponseStream();
            if (responseStream == null) { return ""; }
            streamReader = new StreamReader(responseStream, Encoding.GetEncoding(encode));
            return streamReader.ReadToEnd();
        }
    }
}
