using System.Web;
using System.Web.Mvc;

namespace Immanuel.Ffmpeg
{
    public class FilterConfig
    {
        public static void RegisterGlobalFilters(GlobalFilterCollection filters)
        {
            filters.Add(new HandleErrorAttribute());
        }
    }
}
