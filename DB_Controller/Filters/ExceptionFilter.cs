using Microsoft.AspNetCore.Mvc.Filters;

namespace DB_Controller.Filters
{
    public class ExceptionFilter : ActionFilterAttribute, IExceptionFilter
    {
        public void OnException(ExceptionContext context)
        {
            //Log.Error("Error", context.Exception);
        }
    }
}
