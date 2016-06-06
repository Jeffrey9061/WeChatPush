using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace 微信支付之模板消息推送.Models
{
    public class WxResult
    {
        public int? errcode { get; set; }
        public string errmsg { get; set; }
        public int? msgid { get; set; }
    }
}