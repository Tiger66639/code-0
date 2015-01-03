//-----------------------------------------------------------------------
// <copyright file="MaintenanceController.cs">
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
using System.IO;

namespace Aici.Controllers
{
    /// <summary>
    /// provides maintenance access for desktop client.
    /// </summary>
    public class MaintenanceController : Controller
    {
        //
        // GET: /Maintenance/

        public ActionResult Index()
        {
            return Content("Invalid access");
        }

        /// <summary>
        /// Stops the engine in a clean fashion (disallowing any new posts + disconnecting all session) so that the
        /// database or system can be updated.
        /// When the call returns, the engine is stopped and closed so that it can be maintained.
        /// </summary>
        /// <param name="key">The key used to install the db with, so that no invalid access can happen</param>
        /// <returns>'stopped' when done</returns>
        [HttpPost]
        public ActionResult Stop(string key)
        {
            return Content("stopped");
        }

        /// <summary>
        /// compresses and returns the entire db.
        /// </summary>
        /// <returns></returns>
        public ActionResult GetDB()
        {
            Stream iZip = null;
            return File(iZip, "application/zip", "db.zip");
        }


        /// <summary>
        /// used to upload the db to the system and unpack it again. When done, 'ok' is returned.
        /// </summary>
        /// <param name="file"></param>
        /// <returns>'ok' upon completion</returns>
        [HttpPost]
        public ActionResult LoadDB(HttpPostedFileBase file)
        {
            return Content("ok");
        }
    }
}