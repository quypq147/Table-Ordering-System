﻿using Microsoft.AspNetCore.Mvc;

namespace AdminWeb.Controllers
{
    public class TablesController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
