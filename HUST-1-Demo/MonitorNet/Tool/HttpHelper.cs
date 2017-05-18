using System;
using System.Net;
using System.IO;
using System.Linq;
using System.Text;

namespace MonitorNet
{
	public class HttpHelper
	{
		/// <summary>  
		/// POST请求与获取结果  
		/// </summary>  
		public static string HttpPost(string Url, string postDataStr)  
		{  
			HttpWebRequest request = (HttpWebRequest)WebRequest.Create(Url);  
			request.Method = "POST";  
			request.ContentType = "application/x-www-form-urlencoded";  
			request.ContentLength = postDataStr.Length;  
			StreamWriter writer = new StreamWriter(request.GetRequestStream(),Encoding.ASCII);  
			writer.Write(postDataStr);  
			writer.Flush();  
			HttpWebResponse response = (HttpWebResponse)request.GetResponse();  
			string encoding = response.ContentEncoding;  
			if (encoding == null || encoding.Length < 1) {  
				encoding = "UTF-8"; //默认编码  
			}  
			StreamReader reader = new StreamReader(response.GetResponseStream(), Encoding.GetEncoding(encoding));  
			string retString = reader.ReadToEnd();  
			return retString;  
		}  

		/// <summary>  
		/// POST请求与获取结果  
		/// </summary>  
		public static string HttpGet(string Url)  
		{  
			HttpWebRequest request = (HttpWebRequest)WebRequest.Create(Url);  
			request.Method = "GET";  
			HttpWebResponse response = (HttpWebResponse)request.GetResponse();  
			string encoding = response.ContentEncoding;  
			if (encoding == null || encoding.Length < 1) {  
				encoding = "UTF-8"; //默认编码  
			}  
			StreamReader reader = new StreamReader(response.GetResponseStream(), Encoding.GetEncoding(encoding));  
			string retString = reader.ReadToEnd();  
			return retString;  
		}  
	}
}

