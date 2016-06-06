using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace 微信支付之模板消息推送.Models
{
    /// <summary>
    /// 帐户资金变动提醒Model
    /// </summary>
    public class CashModel
    {
        public DataFontStyle first { get; set; }
        public DataFontStyle date { get; set; }
        public DataFontStyle adCharge { get; set; }
        public DataFontStyle type { get; set; }
        public DataFontStyle cashBalance { get; set; }
        public DataFontStyle remark { get; set; }
    }
}