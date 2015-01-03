//-----------------------------------------------------------------------
// <copyright file="Global.asax.cs">
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
using System.Web.Routing;
using Aici.Network;
using JaStDev.LogService;

namespace Aici
{
    // Note: For instructions on enabling IIS6 or IIS7 classic mode, 
    // visit http://go.microsoft.com/?LinkId=9394801

    public class MvcApplication : System.Web.HttpApplication
    {
        public static void RegisterGlobalFilters(GlobalFilterCollection filters)
        {
            filters.Add(new HandleErrorAttribute());
        }

        public static void RegisterRoutes(RouteCollection routes)
        {
            routes.IgnoreRoute("{resource}.axd/{*pathInfo}");

            routes.MapRoute(
                "Default", // Route name
                "{controller}/{action}/{id}", // URL with parameters
                new { controller = "Home", action = "Index", id = UrlParameter.Optional } // Parameter defaults
            );

        }

        /// <summary>
        /// Called when the application starts.
        /// </summary>
        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();

            RegisterGlobalFilters(GlobalFilters.Filters);
            RegisterRoutes(RouteTable.Routes);
            InitAici();
        }

        
        /// <summary>
        /// Called when the application stops.
        /// </summary>
        protected void Application_End()
        {
            StopAici();
        }

        /// <summary>
        /// Called when a new session stats. Creates a new textChannel (and sin) for the session.
        /// </summary>
        public void Session_OnStart()
        {
            StartAici();
            Session["Channel"] = new TextChannel(Request.ServerVariables["REMOTE_ADDR"]);
        }

        /// <summary>
        /// At the end of the session, make certain that the channel is closed.
        /// </summary>
        public void Session_OnEnd()
        {
            TextChannel iChannel = Session["Channel"] as TextChannel;
            if (iChannel != null)
                iChannel.Close();
            TryStopAici();
        }

        /// <summary>
        /// Creates and fills all the application
        /// </summary>
        private void InitAici()
        {
            try
            {
                Application.Lock();
                try
                {
                    AiciServer iSrv = new AiciServer();
                    iSrv.IsOpen = true;                                     //open the server so that it's ready to run.
                    iSrv.CleanChannels();
                    Application["Aici"] = iSrv;
                    Application["SessionCount"] = 0;                        //so we have an init value
                }
                finally
                {
                    Application.UnLock();
                }
            }
            catch (Exception e)
            {
                Log.LogError("Init", e.ToString());
                throw;
            }
        }

        /// <summary>
        /// Makes certain that the Aici server is running and keeps track of the nr of active sessions.
        /// </summary>
        private void StartAici()
        {
            try
            {
                Application.Lock();
                try
                {
                    AiciServer iSrv = Application["Aici"] as AiciServer;
                    if (iSrv != null)
                        iSrv.IsOpen = true;
                    Application["SessionCount"] = (int)Application["SessionCount"] + 1;
                }
                finally
                {
                    Application.UnLock();
                }
            }
            catch (Exception e)
            {
                Log.LogError("Start", e.ToString());
                throw;
            }
        }

        /// <summary>
        /// checks if there any open sessions left, and if not, closes the aici server.
        /// </summary>
        private void TryStopAici()
        {
            try
            {
                Application.Lock();
                try
                {
                    int iVal = (int)Application["SessionCount"] - 1;
                    Application["SessionCount"] = iVal;
                    if (iVal == 0)
                    {
                        AiciServer iSrv = Application["Aici"] as AiciServer;
                        if (iSrv != null)
                            iSrv.IsOpen = false;
                    }
                }
                finally
                {
                    Application.UnLock();
                }
            }
            catch (Exception e)
            {
                Log.LogError("TryStop", e.ToString());
                throw;
            }
        }

        /// <summary>
        /// closes the aici server, no matter what.
        /// </summary>
        private void StopAici()
        {
            try
            {
                Application.Lock();
                try
                {
                    AiciServer iSrv = Application["Aici"] as AiciServer;
                    if (iSrv != null)
                        iSrv.IsOpen = false;
                }
                finally
                {
                    Application.UnLock();
                }
            }
            catch (Exception e)
            {
                Log.LogError("Stop", e.ToString());
                throw;
            }
        }
    }
}