//-----------------------------------------------------------------------
// <copyright file="PhoneManagementAndroid.cs">
//     Copyright (c) 2008-2012 Jan Bogaerts. All rights reserved.
// </copyright> 
// <authorJan Bogaerts</author>
// <email>Jan.Bogaerts@telenet.be</email>
// <date>20/05/2012</date>
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
using Android.Media;
using JaStDev.HAB;

namespace os.MonoDroid
{
   class PhoneManagementAndroid
   {
      /// <summary>
      /// Makes the phone silent.
      /// </summary>
      static public void SetPhoneToSilent()
      {
         AudioManager am = (AudioManager)ReflectionSin.Context.GetSystemService(Context.AudioService);
         am.RingerMode = RingerMode.Silent;
      }

      /// <summary>
      /// turns the phone into vibration mode.
      /// </summary>
      static public void SetPhoneToVibrate()
      {
         AudioManager am = (AudioManager)ReflectionSin.Context.GetSystemService(Context.AudioService);
         am.RingerMode = RingerMode.Vibrate;
      }

      /// <summary>
      /// turns the phone into normal ring mode.
      /// </summary>
      static public void SetPhoneToNormal()
      {
         AudioManager am = (AudioManager)ReflectionSin.Context.GetSystemService(Context.AudioService);
         am.RingerMode = RingerMode.Normal;
      }

      /// <summary>
      /// Gets the battery level. expressed in %
      /// </summary>
      /// <returns></returns>
      internal static double GetBatteryLevel()
      {
         return Math.Round(((double)BatteryReceiver.Default.Level / (double)BatteryReceiver.Default.Scale) * 100, 1);
      }

      /// <summary>
      /// Gets the battery voltage.
      /// </summary>
      /// <returns></returns>
      internal static double GetBatteryVoltage()
      {
         return BatteryReceiver.Default.Voltage;
      }

      /// <summary>
      /// Gets the battery temperature.
      /// </summary>
      /// <returns></returns>
      internal static double GetBatteryTemp()
      {
         return BatteryReceiver.Default.Temp;
      }


      /// <summary>
      /// Selects the image.
      /// </summary>
      /// <returns></returns>
      internal static string SelectImage()
      {
         Intent i = new Intent(Intent.ActionPick, Android.Provider.MediaStore.Images.Media.ExternalContentUri);
         IAndroidService iService = (IAndroidService)ReflectionSin.Context;
         IIntentResultService iIntentHandler = iService.Activity as IIntentResultService;
         if (iIntentHandler != null)
         {
            iService.Activity.StartActivityForResult(i, PhoneManagement.STRINGRESULT);
            iIntentHandler.ResultFlag.WaitOne();                                                               //wait for the result.
            string iRes;
            if (iIntentHandler.GetStringResult(out iRes) == true)
               return iRes;
            else
               return null;
         }
         else
            throw new InvalidOperationException("No intent handler found");
      }

      /// <summary>
      /// shows the help screen.
      /// </summary>
      internal static void ShowHelp()
      {
         IAndroidService iService = (IAndroidService)ReflectionSin.Context;
         iService.ShowHelp();
      }
   }
}