//-----------------------------------------------------------------------
// <copyright file="EndCallListener.cs">
//     Copyright (c) 2008-2012 Jan Bogaerts. All rights reserved.
// </copyright> 
// <authorJan Bogaerts</author>
// <email>Jan.Bogaerts@telenet.be</email>
// <date>20/01/2012</date>
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
using Android.Telephony;
using JaStDev.HAB;

namespace os.MonoDroid
{ 
   /// <summary>
   /// Monitors the phone for the call to end so that the sytem can return to the main window.
   /// </summary>
   public class EndCallListener : PhoneStateListener 
   {
      bool fInCall = false;
      /// <summary>
      /// Callback invoked when device call state changes.
      /// </summary>
      /// <param name="state">To be added.</param>
      /// <param name="incomingNumber">To be added.</param>
      /// <since version="API Level 1"/>
      public override void OnCallStateChanged(CallState state, string incomingNumber)
      {
         switch (state)
         {
            case CallState.Idle:
               if (fInCall == true)
               {
                  IAndroidService iService = ReflectionSin.Context as IAndroidService;
                  if (iService != null)
                  {
                     TelephonyManager iPhone = (TelephonyManager)ReflectionSin.Context.GetSystemService(Context.TelephonyService);
                     iPhone.Listen(this, Android.Telephony.PhoneStateListenerFlags.None);                              //no longer interested in the events, so unregister.
                     iService.ShowMainActivity();
                  }
                  fInCall = false;
               }
               break;
            case CallState.Offhook:
               fInCall = true;
               break;
            case CallState.Ringing:
               break;
            default:
               break;
         }   
      }
   }
}