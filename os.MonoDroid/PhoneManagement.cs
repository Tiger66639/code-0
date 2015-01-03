//-----------------------------------------------------------------------
// <copyright file="PhoneManagement.cs">
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
using JaStDev.LogService;



namespace os.MonoDroid
{
   /// <summary>
   /// provides static functions for managing features about the android.
   /// </summary>
   public class PhoneManagement
   {
      public const int STRINGRESULT = 10001;

      /// <summary>
      /// Makes the phone silent.
      /// </summary>
      public static void SetPhoneToSilent()
      {
#if ANDROID
         PhoneManagementAndroid.SetPhoneToSilent();
#else
         throw new NotImplementedException();
#endif
      }

      /// <summary>
      /// turns the phone into vibration mode.
      /// </summary>
      public static void SetPhoneToVibrate()
      {
#if ANDROID
         PhoneManagementAndroid.SetPhoneToVibrate();
#else
         throw new NotImplementedException();
#endif
      }

      /// <summary>
      /// turns the phone into normal ring mode.
      /// </summary>
      public static void SetPhoneToNormal()
      {
#if ANDROID
         PhoneManagementAndroid.SetPhoneToNormal();
#else
         throw new NotImplementedException();
#endif
      }


      /// <summary>
      /// gets the % power that remains in the battery.
      /// </summary>
      public static double GetBatteryLevel()
      {
#if ANDROID
         return PhoneManagementAndroid.GetBatteryLevel();
#else
         throw new NotImplementedException();
#endif
      }

      /// <summary>
      /// returnst he temperature of the battery/device.
      /// </summary>
      public static double GetBatteryTemp()
      {
#if ANDROID
         return PhoneManagementAndroid.GetBatteryTemp();
#else
         throw new NotImplementedException();
#endif
      }

      /// <summary>
      /// gest the voltage that the battery has.
      /// </summary>
      public static double GetBatteryVoltage()
      {
#if ANDROID
         return PhoneManagementAndroid.GetBatteryVoltage();
         Log.LogInfo("PhoneManagement","Battery voltage can only be retrieved on android.");
#else
         throw new NotImplementedException();
#endif
      }

      /// <summary>
      /// selects an image
      /// </summary>
      /// <returns></returns>
      public static string SelectImage()
      {
#if ANDROID
         return PhoneManagementAndroid.SelectImage();
#else
         throw new NotImplementedException();
#endif
      }

      /// <summary>
      /// shows the help screen.
      /// </summary>
      public static void ShowHelp()
      {
#if ANDROID
         PhoneManagementAndroid.ShowHelp();
#else
         Log.LogInfo("PhoneManagement","ShowHelp can only be done on android.");
#endif
      }

   }
}