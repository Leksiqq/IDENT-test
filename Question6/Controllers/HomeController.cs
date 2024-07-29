using Microsoft.AspNetCore.Mvc;
using System;

namespace Question6.Controllers
{
    public class HomeController
    {
        public string Test([ModelBinder(BinderType = typeof(TestFilterModelBinder))] TestFilter filter)
        {
            return filter?.YearMonth?.Display ?? "unknown";
        }
    }
}
