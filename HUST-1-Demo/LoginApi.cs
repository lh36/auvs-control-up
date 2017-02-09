using UnityEngine;
using System.Collections;

//这里用的不是HttpWebRequest类来进行网络请求，只需看下如何把JSON数据转换成对象里的数据

public class LoginApi : SingletonUnity<LoginApi>
{
    private string uri = "login";

    public bool IsDone = false;
    public bool IsLoginSucceess = false;
    public bool IsOffInternet = false;

    public LoginResp result = null;

    /// <summary>
    /// 根据token进行登录验证
    /// </summary>
    public IEnumerator LoginPost(string token)
    {
        WWWForm form = new WWWForm ();
        form.AddField ("token", token);

        WWW www = new WWW (Constant.BaseUrl + uri, form);
        yield return www;

        if(www.error != null)
        {
            Debug.Log (www.error);
			IsOffInternet = true;
            yield return null;
        }
        else
        {
            LoginJson loginJson = JsonHelper.JsonToClass<LoginJson> (www.text);//**这里把接收的Json数据转换为对象

            if(loginJson == null)
            {
                IsOffInternet = true;
            }
            else
            {
                Debug.Log (www.text);

                if(loginJson.status == Constant.Status_OK)
                {
                    result = loginJson.resp;
                    IsLoginSucceess = true;
                }

            }
        }

		IsDone = true;

    }

}

//**定义了JSON数据的接收类
public class LoginJson
{
    public string status = "";
    public LoginResp resp = null;

    public LoginJson()
    {}
}

//**定义了JSON数据中Resp的数据类
public class LoginResp
{
    public string name = "";
    public string identity = "";

    public LoginResp()
    {}
}