using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace 模拟MVC.Controllers
{
    public class Default1Controller : Controller
    {
        //
        // GET: /Default1/
        public ActionResult Test()
        {
            HttpRequest request = System.Web.HttpContext.Current.Request;
            HttpFileCollection FileCollect = request.Files;
            if (FileCollect.Count > 0)          //如果集合的数量大于0
            {
                foreach (string str in FileCollect)
                {
                    HttpPostedFile FileSave = FileCollect[str];  //用key获取单个文件对象HttpPostedFile
                    string imgName = DateTime.Now.ToString("yyyyMMddhhmmss");
                    string AbsolutePath = FileSave.FileName;
                    FileSave.SaveAs(AbsolutePath);              //将上传的东西保存
                }
            }
            return Content("键值对数目：" + FileCollect.Count);
        }

    }
}
