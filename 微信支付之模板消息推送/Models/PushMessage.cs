using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace 微信支付之模板消息推送.Models
{
    public class PushMessage
    {

        public string touser { get; set; }
        public string template_id { get; set; }
        public string url { get; set; }
        public string topcolor { get; set; }
        /// <summary>
        /// 注意，这里是CashModel,你如果要通用，有多个推送模板要用，那你就用object
        /// </summary>
        public CashModel data { get; set; }
   
    }
}