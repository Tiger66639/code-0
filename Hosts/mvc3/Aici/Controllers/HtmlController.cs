//-----------------------------------------------------------------------
// <copyright file="HtmlController.cs">
//     Copyright (c) 2008-2012 Jan Bogaerts. All rights reserved.
// </copyright> 
// <authorJan Bogaerts</author>
// <email>Jan.Bogaerts@telenet.be</email>
// <date>13/12/2011</date>
//-----------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Aici.Network;
using Aici.Models;

namespace Aici.Controllers
{
    /// <summary>
    /// Provides an html view to the chatbot.
    /// </summary>
    public class HtmlController : BaseWebController
    {
        //
        // GET: /Html/

        /// <summary>
        /// Shows the full webpage.
        /// </summary>
        /// <returns></returns>
        public ActionResult Index()
        {
            ChatInteraction iRes = new ChatInteraction();
            iRes.Conversation = FillViewBagWithOutput();
            if (Properties.Settings.Default.HtmlAsPartial == true)
                return PartialView("Send",iRes);
            else
                return View(iRes);
            //return View("test");
        }


        /// <summary>
        /// returns the current conversation log and input part
        /// </summary>
        /// <returns></returns>
        public ActionResult Send()
        {
            ChatInteraction iRes = new ChatInteraction();
            iRes.Conversation = FillViewBagWithOutput();
            if (Properties.Settings.Default.HtmlAsPartial == true)
                return PartialView(iRes);
            else
                return View("Index", iRes);
        }

        /// <summary>
        /// Sends the specified text to the textchannel, waits for a result (or times out) and renders the result
        /// on the screen.
        /// </summary>
        /// <param name="Input"></param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult Send(string Input)
        {
            ModelState.Clear();
            ChatInteraction iRes = new ChatInteraction();
            TextChannel iChannel = Session["Channel"] as TextChannel;
            if (iChannel != null)
            {
                iChannel.ProcessWait(Input);
                iRes.LastOutput = iChannel.PeekLastOutputs;
                iRes.Conversation = iChannel.GetOutputList();
            }
            if (Properties.Settings.Default.HtmlAsPartial == true)
                return PartialView(iRes);
            else
               return PartialView("Send", iRes);                                 //we do a partial refresh so that other parts of the screen remain the same
        }


        
    }
}