//-----------------------------------------------------------------------
// <copyright file="HomeController.cs">
//     Copyright (c) 2008-2012 Jan Bogaerts. All rights reserved.
// </copyright> 
// <authorJan Bogaerts</author>
// <email>Jan.Bogaerts@telenet.be</email>
// <date>03/12/2011</date>
//-----------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Aici.Controllers
{
   /// <summary>
   /// the default controller, entry point to the website.
   /// </summary>
    public class HomeController : Controller
    {
       /// <summary>
       /// Gets the default page for this controller: found through a setting. If non defined, the web is default.
       /// </summary>
       /// <returns></returns>
        public ActionResult Index()
        {
            if (string.IsNullOrEmpty(Properties.Settings.Default.DefaultController) == false)
                return RedirectToAction("Index", Properties.Settings.Default.DefaultController);
            else
                return RedirectToAction("Index", "Web");
        }

        /// <summary>
        /// returns an about page for aici.
        /// </summary>
        /// <returns></returns>
        public ActionResult About()
        {
            return View();
        }
    }
}