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
   /// base class for controllers that render web-pages.
   /// </summary>
    public class BaseWebController : Controller
    {
       /// <summary>
       /// Fills the ViewBag with all the current output data so that a (partial)view can be built.
       /// </summary>
       protected List<OutputValue> FillViewBagWithOutput()
       {
          TextChannel iChannel = Session["Channel"] as TextChannel;
          if (iChannel != null)
             return iChannel.GetOutputList();
          return null;
       }
    }
}
