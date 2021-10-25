using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;

namespace WebStore.Controllers
{
    public class HomeController : Controller
    {
       

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Exception(string Message) => throw new InvalidOperationException(Message ?? "Ошибка в контроллере!");

        public IActionResult Status(string id)
        {
            if (id is null)
                throw new ArgumentNullException(nameof(id));

            switch (id)
            {
                default: return Content($"Status --- {id}");
                case "404": return View("NotFound");
            }
        }

        public IActionResult ContactUs() => View();
    }
}
