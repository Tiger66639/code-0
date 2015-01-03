using Aici.Models;
using Aici.Network;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Aici.Controllers
{
   /// <summary>
   /// a controller for working with crazy talk characters.
   /// </summary>
   public class CTController : BaseWebController
   {
        //
        // GET: /CT/
       public ActionResult Index()
       {
           ChatInteraction iRes = new ChatInteraction();
           iRes.Conversation = FillViewBagWithOutput();
           return View("Index", iRes);
       }

        /// <summary>
        /// returns the current conversation log and input part
        /// </summary>
        /// <returns></returns>
        public ActionResult Send()
        {
           ChatInteraction iRes = new ChatInteraction();
           iRes.Conversation = FillViewBagWithOutput();
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
           ChatInteraction iRes = new ChatInteraction() { SyncWith = "CT" };
           TextChannel iChannel = Session["Channel"] as TextChannel;
           if (iChannel != null)
           {
              iChannel.ProcessWait(Input);
              iRes.LastOutput = iChannel.PeekLastOutputs;
              iRes.Conversation = iChannel.GetOutputList();
           }
           return PartialView("Send", iRes);
        }

    }
}
