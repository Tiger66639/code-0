//-----------------------------------------------------------------------
// <copyright file="ApiController.cs">
//     Copyright (c) 2008-2012 Jan Bogaerts. All rights reserved.
// </copyright> 
// <authorJan Bogaerts</author>
// <email>Jan.Bogaerts@telenet.be</email>
// <date>07/06/2012</date>
//-----------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Aici.Network;
using System.Text.RegularExpressions;
using JaStDev.LogService;

namespace Aici.Controllers
{
    /// <summary>
    /// The api controller provides a web interface to Aici returning only xml formatted data and no html.
    /// </summary>
    public class ApiController : Controller
    {
        /// <summary>
        /// Returns the default view that lets the user know he is accessing the api incorrectly.
        /// GET: /Api/ 
        /// </summary>
        /// <returns></returns>
        public ActionResult Index()
        {
            return View();
        }

        /// <summary>
        /// Can be called to connect to the server so that the cooky can be retrieved. Will
        /// return any initial opening statement that was already created after initial session create.
        /// </summary>
        /// <returns></returns>
        public ActionResult Connect()
        {
            TextChannel iChannel = Session["Channel"] as TextChannel;
            if (iChannel != null && AiciServer.CheckAllowConnection(Request.ServerVariables["REMOTE_ADDR"]))
            {
                iChannel.LogFullText = false;
                string iRes = iChannel.GetOutputs();
                return Content(iRes);
            }
            return Content("Invalid session");
        }

        /// <summary>
        /// closes the current session.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        //[HttpPost]
        public ActionResult Disconnect()
        {
            Session.Abandon();                                                                          //stop the session, so that the channel associated with the session can be cleaned up.
            return Content("disconnected");
        }


        /// <summary>
        /// Sends the specified text for the specified id session to the engine and waits for any possible answer
        /// untill the timeout has expired.
        /// </summary>
        /// <param name="id"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        //[HttpPost]
        public ActionResult Send(string value)
        {
            if (ModelState.IsValid)
            {
                TextChannel iChannel = Session["Channel"] as TextChannel;
                if (iChannel != null && AiciServer.CheckAllowConnection(Request.ServerVariables["REMOTE_ADDR"]))
                {
                    iChannel.LogFullText = false;                                                       //make certain that this is off, don't need full logging, could be first call
                    iChannel.ProcessWait(value);
                    string iRes = iChannel.GetOutputs();
                    return Content(iRes);
                }
            }
            return Content("Invalid session");
        }


        /// <summary>
        /// Sends the text async to the engine (doesn't wait for a response).
        /// </summary>
        /// <param name="id"></param>
        /// <param name="value"></param>
        /// <returns>fixed value: 'ok' when succesfully sent message</returns>
        //[HttpPost]
        public ActionResult SendA(string value)
        {
            if (ModelState.IsValid)
            {
                TextChannel iChannel = Session["Channel"] as TextChannel;
                if (iChannel != null && AiciServer.CheckAllowConnection(Request.ServerVariables["REMOTE_ADDR"]))
                {
                    iChannel.LogFullText = false;                                                       //make certain that this is off, don't need full logging, could be first call
                    iChannel.Process(value);
                    return Content("ok");
                }
            }
            return Content("Invalid session");
            
        }

        /// <summary>
        /// Checks if there are any messages waiting to be sent for the specified id and returns them.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public ActionResult Poll()
        {
            TextChannel iChannel = Session["Channel"] as TextChannel;
            if (iChannel != null && AiciServer.CheckAllowConnection(Request.ServerVariables["REMOTE_ADDR"]))
            {
                iChannel.LogFullText = false;                                                       //make certain that this is off, don't need full logging, could be first call
                string iRes = iChannel.GetOutputs();
                return Content(iRes);
            }
            return Content("Invalid session");
        }
    }
}