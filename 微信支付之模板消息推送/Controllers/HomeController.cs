using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Web;
using System.Web.Mvc;
using System.Web.Script.Serialization;
using WxPayAPI;
using 微信支付之模板消息推送.Models;

namespace 微信支付之模板消息推送.Controllers
{
    public class HomeController : Controller
    {
        JsApiPay jsApiPay = new JsApiPay();

        JavaScriptSerializer JsonHelper = new JavaScriptSerializer();
        /// <summary>
        /// 最后更新Access_token的时间
        /// </summary>
        public static DateTime dtAccess_token;
        /// <summary>
        /// Access_token的值
        /// </summary>
        public static string strAccess_token;
        // GET: Home
        public ActionResult Index()
        {
            if (Session["openid"] == null)
            {
                try
                {
                    //调用【网页授权获取用户信息】接口获取用户的openid和access_token
                    GetOpenidAndAccessToken();

                }
                catch (Exception ex)
                {
                    //Response.Write(ex.ToString());
                    //throw;
                }
            }
            return View();
        }

        /**
        * 
        * 网页授权获取用户基本信息的全部过程
        * 详情请参看网页授权获取用户基本信息：http://mp.weixin.qq.com/wiki/17/c0f37d5704f0b64713d5d2c37b468d75.html
        * 第一步：利用url跳转获取code
        * 第二步：利用code去获取openid和access_token
        * 
        */
        public void GetOpenidAndAccessToken()
        {
            if (Session["code"] != null)
            {
                //获取code码，以获取openid和access_token
                string code = Session["code"].ToString();
                Log.Debug(this.GetType().ToString(), "Get code : " + code);
                jsApiPay.GetOpenidAndAccessTokenFromCode(code);
            }
            else
            {
                //构造网页授权获取code的URL
                string host = Request.Url.Host;
                string path = Request.Path;
                string redirect_uri = HttpUtility.UrlEncode("http://" + host + path);
                WxPayData data = new WxPayData();
                data.SetValue("appid", WxPayConfig.APPID);
                data.SetValue("redirect_uri", redirect_uri);
                data.SetValue("response_type", "code");
                data.SetValue("scope", "snsapi_base");
                data.SetValue("state", "STATE" + "#wechat_redirect");
                string url = "https://open.weixin.qq.com/connect/oauth2/authorize?" + data.ToUrl();
                Log.Debug(this.GetType().ToString(), "Will Redirect to URL : " + url);
                Session["url"] = url;
            }
        }


        /// <summary>
        /// 获取code
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public ActionResult getCode()
        {
            object objResult = "";
            if (Session["url"] != null)
            {
                objResult = Session["url"].ToString();
            }
            else
            {
                objResult = "url为空。";
            }
            return Json(objResult);
        }

        /// <summary>
        /// 通过code换取网页授权access_token和openid的返回数据
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public ActionResult getWxInfo()
        {
            object objResult = "";
            string strCode = Request.Form["code"];
            string strAccess_Token = "";
            string strOpenid = "";
            if (Session["access_token"] == null || Session["openid"] == null)
            {
                jsApiPay.GetOpenidAndAccessTokenFromCode(strCode);
                strAccess_Token = Session["access_token"].ToString();
                strOpenid = Session["openid"].ToString();
            }
            else
            {
                strAccess_Token = Session["access_token"].ToString();
                strOpenid = Session["openid"].ToString();
            }
            objResult = new { openid = strOpenid, access_token = strAccess_Token };
            return Json(objResult);
        }

        /// <summary>
        /// 推送
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public ActionResult Push()
        {
            object objResult = "";
            string strFirst = Request.Form["first"];
            string strMsg = "";
            bool bResult = false;
            //这个是推送消息的类
            PushMessage aPushMessage = new PushMessage()
            {
                template_id = "lypG1jYyOEfYsrAfNFk9SvjOK7-LhEwpPeVNHHxemSI",//模板ID
                                                                            //data=,//暂时不赋值
                topcolor = "#FF0000",//头部颜色
                touser = Session["openid"].ToString(),//用户的Openid
                url = "http://www.baidu.com"//用途是当用户点击推送消息的时候，会进入这个页面，具体用途，自己拓展
            };

            //构造要推送的内容
            CashModel aCachData = new CashModel()
            {
                adCharge = new DataFontStyle()
                {
                    color = "#589E63",
                    value = "对应变动金额"
                },
                cashBalance = new DataFontStyle()
                {
                    color = "#589E63",
                    value = "对应帐户余额"
                },
                date = new DataFontStyle()
                {
                    color = "#589E63",
                    value = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")
                },
                first = new DataFontStyle()
                {
                    color = "#589E63",
                    value = strFirst
                },
                remark = new DataFontStyle()
                {
                    color = "#589E63",
                    value = "对应：点击“查看详情“立即查阅您的帐户财务记录。"
                },
                type = new DataFontStyle()
                {
                    color = "#589E63",
                    value = "对应“现金”"
                }
            };
            //这时候，把要推送的内容，赋值给push,这样，我们要推送的内容就完成了。
            aPushMessage.data = aCachData;

            string strUrl = "https://api.weixin.qq.com/cgi-bin/message/template/send?access_token=" + GetLatestAccess_token();
            
            string strJsonData = JsonHelper.Serialize(aPushMessage);
            string strResult = HttpPost(strUrl, strJsonData);
            WxResult aResult = JsonHelper.Deserialize<WxResult>(strResult);
            if (aResult != null)
            {
                if (aResult.errcode == 0)
                {
                    bResult = true;
                }
                else
                {
                    bResult = false;
                    strMsg = aResult.errmsg;
                }
            }
            else
            {
                bResult = false;
                strMsg = "通讯失败，请重试。";
            }

            objResult = new { result = bResult, msg = strMsg };
            return Json(objResult);
        }



        /// <summary>
        /// HttpPost
        /// </summary>
        /// <param name="Url"></param>
        /// <param name="postDataStr"></param>
        /// <returns></returns>
        public static string HttpPost(string Url, string postDataStr)
        {
            byte[] postData = Encoding.UTF8.GetBytes(postDataStr);//编码，尤其是汉字，事先要看下抓取网页的编码方式   
            WebClient webClient = new WebClient();
            webClient.Headers.Add("Content-Type", "application/x-www-form-urlencoded");//采取POST方式必须加的header，如果改为GET方式的话就去掉这句话即可  
            byte[] responseData = webClient.UploadData(Url, "POST", postData);//得到返回字符流             
            string srcString = Encoding.UTF8.GetString(responseData);//解码 
            return srcString;
           
        }


        /// <summary>
        /// 返回最新的Access_token
        /// </summary>
        /// <returns></returns>
        public string GetLatestAccess_token()
        {
            if (dtAccess_token == null || dtAccess_token <= DateTime.Now.AddHours(-1) || string.IsNullOrWhiteSpace(strAccess_token))
            {
                string strUrl = string.Format("https://api.weixin.qq.com/cgi-bin/token?grant_type=client_credential&appid={0}&secret={1}", WxPayConfig.APPID, WxPayConfig.APPSECRET);
                string strAccess_tokenData = HttpGet(strUrl, "");
                ModelForAccess_token aToken = JsonHelper.Deserialize<ModelForAccess_token>(strAccess_tokenData);
                dtAccess_token = DateTime.Now;
                strAccess_token = aToken.access_token;
                return strAccess_token;
            }
            else
            {
                return strAccess_token;
            }
        }

        /// <summary>
        /// WebGet
        /// </summary>
        /// <param name="Url"></param>
        /// <param name="postDataStr"></param>
        /// <returns></returns>
        public static string HttpGet(string Url, string postDataStr)
        {
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(Url + (postDataStr == "" ? "" : "?") + postDataStr);
            request.Method = "GET";
            request.ContentType = "text/html;charset=UTF-8";

            HttpWebResponse response = (HttpWebResponse)request.GetResponse();
            Stream myResponseStream = response.GetResponseStream();
            StreamReader myStreamReader = new StreamReader(myResponseStream, Encoding.GetEncoding("utf-8"));
            string retString = myStreamReader.ReadToEnd();
            myStreamReader.Close();
            myResponseStream.Close();

            return retString;
        }

        public class ModelForAccess_token
        {
            public string access_token { get; set; }
            public int? expires_in { get; set; }
        }
    }
}