//-----------------------------------------------------------------------
// <copyright file="ApiTextController.cs">
//     Copyright (c) 2008-2012 Jan Bogaerts. All rights reserved.
// </copyright> 
// <authorJan Bogaerts</author>
// <email>Jan.Bogaerts@telenet.be</email>
// <date>26/03/2012</date>
//-----------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Aici.Network;

namespace Aici.Controllers
{
    public class ApiTextController : Controller
    {
        //
        // GET: /ApiText/

        public ActionResult Index()
        {
            return View();
        }

        /// <summary>
        /// Sends the specified text for the specified id session to the engine and waits for any possible answer
        /// untill the timeout has expired.
        /// </summary>
        /// <param name="id"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public ActionResult Send(string q)
        {
           if (ModelState.IsValid)
           {
              TextChannel iChannel = Session["Channel"] as TextChannel;
              if (iChannel != null && AiciServer.CheckAllowConnection(Request.ServerVariables["REMOTE_ADDR"]))
              {
                 iChannel.LogFullText = false;                                                       //make certain that this is off, don't need full logging, could be first call
                 iChannel.ProcessWait(q);
                 string iRes = iChannel.GetOutputs(false);
                 return Content(iRes);
              }
           }
           return Content("Invalid session");
        }
    }
}