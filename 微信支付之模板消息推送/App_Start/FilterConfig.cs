using System.Web;
using System.Web.Mvc;

namespace 微信支付之模板消息推送
{
    public class FilterConfig
    {
        public static void RegisterGlobalFilters(GlobalFilterCollection filters)
        {
            filters.Add(new HandleErrorAttribute());
        }
    }
}
