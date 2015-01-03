//-----------------------------------------------------------------------
// <copyright file="CmdShell.cs">
//     Copyright (c) 2008-2012 Jan Bogaerts. All rights reserved.
// </copyright> 
// <authorJan Bogaerts</author>
// <email>Jan.Bogaerts@telenet.be</email>
// <date>12/05/2012</date>
//-----------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using JaStDev.HAB;
using os.MonoDroid;

namespace CmdShell
{
   public class CmdShell
   {
      /// <summary>
      /// Starts an intent.
      /// </summary>
      /// <param name="action">The action.</param>
      /// <param name="Uri">The URI.</param>
      public static void StartProcess(string processName, string fileName)
      {
         Intent iNew = new Intent(processName, Android.Net.Uri.Parse(fileName));
         IAndroidService iServ = (IAndroidService)ReflectionSin.Context;                     //can only start an activity from another activity, so
         iServ.Activity.StartActivity(iNew);
      }

      /// <summary>
      /// opens the browser intent and goes to the specified page.
      /// </summary>
      /// <param name="uri"></param>
      public static void OpenDocument(string fileName)
      {
         Intent browserIntent = new Intent(Intent.ActionView, Android.Net.Uri.Parse(fileName));
         IAndroidService iServ = (IAndroidService)ReflectionSin.Context;                     //can only start an activity from another activity, so
         iServ.Activity.StartActivity(browserIntent);
      }
   }
}