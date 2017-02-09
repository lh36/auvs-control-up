using System.Collections;
using System.Collections.Generic;
using JsonFx.Json;

public class JsonHelper
{

    /// <summary>  
    /// 根据一个JSON，得到一个类  
    /// </summary>  
    static public T JsonToClass<T>(string json) where T : class  
    {  
        T t = JsonReader.Deserialize<T>(json);  
        return t;  
    }  

    /// <summary>  
    /// 将JSON转换为一个类数组  
    /// </summary>  
    static public T[] JsonToClasses<T>(string json) where T : class  
    {  
        //Debug.Log(json);  
        T[] list = JsonReader.Deserialize<T[]>(json);  
        return list;  
    }  

    /// <summary>  
    /// 将一个对象class转换成json字符串
    /// </summary>  
    static public string ClassToJson<T>(T t) where T : class
    {
        return JsonWriter.Serialize (t);
    }
}

