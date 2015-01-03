//-----------------------------------------------------------------------
// <copyright file="CommunicationAndroid.cs">
//     Copyright (c) 2008-2012 Jan Bogaerts. All rights reserved.
// </copyright> 
// <authorJan Bogaerts</author>
// <email>Jan.Bogaerts@telenet.be</email>
// <date>16/05/2012</date>
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
using Java.IO;
using JaStDev.HAB;
using Android.Content.PM;

namespace os.MonoDroid
{
   class CommunicationAndroid
   {
      /// <summary>
      /// sends an sms to the specified address.
      /// </summary>
      /// <param name="message"></param>
      /// <param name="phoneNr"></param>
      internal static void SendSMS(string phoneNr, string message)
      {
         Intent smsIntent = new Intent(Intent.ActionView);
         smsIntent.SetType("vnd.android-dir/mms-sms");
         smsIntent.PutExtra("address", phoneNr);
         smsIntent.PutExtra("sms_body", message);
         smsIntent.SetFlags(ActivityFlags.NewTask);                                                              //need to do this, otherwise android complains that we are starting an activity outside of another activity (a service)
         ReflectionSin.Context.StartActivity(smsIntent);
      }

      /// <summary>
      /// Sends an email 
      /// </summary>
      /// <param name="address"></param>
      /// <param name="content"></param>
      /// <param name="attachements"></param>
      public static void SendEmail(string[] address, string subject, string content, string[] attachements = null, string[] cc = null)
      {
         Intent iIntent;
         if (attachements == null || attachements.Length <= 1)
         {
            iIntent = new Intent(Android.Content.Intent.ActionSend);
            if (attachements != null && attachements.Length == 1)
               iIntent.PutExtra(Android.Content.Intent.ExtraStream, Android.Net.Uri.FromFile(new File(attachements[0])));
         }
         else
         {
            iIntent = new Intent(Android.Content.Intent.ActionSendMultiple);                     //need to "send multiple" to get more than one attachment
            IList<IParcelable> uris = new List<IParcelable>();
            foreach (string i in attachements)                                                              //convert from paths to Android friendly Parcelable Uri's
            {
               File fileIn = new File(i);
               IParcelable u = Android.Net.Uri.FromFile(fileIn);
               uris.Add(u);
            }
            iIntent.PutParcelableArrayListExtra(Intent.ExtraStream, uris);

         }
         iIntent.SetType("plain/text");
         iIntent.PutExtra(Android.Content.Intent.ExtraEmail, address);
         iIntent.PutExtra(Android.Content.Intent.ExtraSubject, subject);
         if (cc != null)
            iIntent.PutExtra(Android.Content.Intent.ExtraCc, cc);
         iIntent.PutExtra(Android.Content.Intent.ExtraText, content);
         iIntent.SetFlags(ActivityFlags.NewTask);                                                              //need to do this, otherwise android complains that we are starting an activity outside of another activity (a service)
         iIntent = Intent.CreateChooser(iIntent, "Send mail...");
         iIntent.SetFlags(ActivityFlags.NewTask);                                                              //need to do this, otherwise android complains that we are starting an activity outside of another activity (a service)
         ReflectionSin.Context.StartActivity(iIntent);
      }


      /// <summary>
      /// initiates a phone call to the specified number. When the phone call is done, the system returns to the Aici screen.
      /// </summary>
      /// <param name="number"></param>
      public static void MakePhoneCall(string number)
      {
         IAndroidService iService = (IAndroidService)ReflectionSin.Context;
         if (iService.Activity != null)
         {
            //doing a phone call needs to be done from the UI thread, otherwise we can't create the EndCallListner. Hence this technique.
            iService.Activity.RunOnUiThread(() =>
            {
               EndCallListener iListener = new EndCallListener();
               TelephonyManager iPhone = (TelephonyManager)ReflectionSin.Context.GetSystemService(Context.TelephonyService);
               iPhone.Listen(iListener, Android.Telephony.PhoneStateListenerFlags.CallState);                              //we need to monitor phone changes, so that we can go back to our main window when done.

               Android.Net.Uri uri = Android.Net.Uri.Parse(string.Format("tel:{0}", number));
               Intent callIntent = new Intent(Intent.ActionCall, uri);
               callIntent.SetFlags(ActivityFlags.NewTask);                                                              //need to do this, otherwise android complains that we are starting an activity outside of another activity (a service)
               ReflectionSin.Context.StartActivity(callIntent);
            });
         }
      }

      public static void SendTweet(string message)
      {
         Intent iIntent = new Intent("com.disretrospect.twidgit.TWEET");
         iIntent.PutExtra("com.disretrospect.twidgit.extras.MESSAGE", message);
         iIntent.SetFlags(ActivityFlags.NewTask);                                                              //need to do this, otherwise android complains that we are starting an activity outside of another activity (a service)
         IAndroidService iService = ReflectionSin.Context as IAndroidService;
         if (iService != null)
         {
            try
            {
               iService.Activity.StartActivityForResult(iIntent, Communication.TWIDGIT_REQUEST_CODE);
            }
            catch (ActivityNotFoundException e)
            {
               Toast.MakeText(ReflectionSin.Context, "The 'Twidgit Lite' app needs to be installed for sending tweets.", ToastLength.Long).Show();
               Intent iMarketIntent = new Intent(Intent.ActionView);
               Android.Net.Uri iUri = Android.Net.Uri.Parse("market://details?id=com.distrospect.twidgit");
               iMarketIntent.SetData(iUri);
               iService.Activity.StartActivity(iMarketIntent);
            }
         }
      }

      /// <summary>
      /// Starts an email app
      /// </summary>
      internal static void StartEmail()
      {
         IAndroidService iService = ReflectionSin.Context as IAndroidService;
         Intent intent = new Intent(Android.Content.Intent.ActionSend);
         intent.SetType("text/plain");
         iService.Activity.StartActivity(intent);
      }

      /// <summary>
      /// Starts the gmail app.
      /// </summary>
      internal static void StartGmail()
      {
         IAndroidService iService = ReflectionSin.Context as IAndroidService;
         Intent intent = new Intent(Android.Content.Intent.ActionSend);
         intent.SetType("text/plain");
         PackageManager pm = iService.Activity.PackageManager;
         PackageInfoFlags iFlags = default(PackageInfoFlags );
         IList<ResolveInfo> matches = pm.QueryIntentActivities(intent, iFlags);
         ResolveInfo best = null;
         foreach (ResolveInfo info in matches)
            if (info.ActivityInfo.PackageName.EndsWith(".gm") || info.ActivityInfo.Name.ToLower().Contains("gmail"))
               best = info;
         if (best != null)
            intent.SetClassName(best.ActivityInfo.PackageName, best.ActivityInfo.Name);
         
          iService.Activity.StartActivity(intent);

      }
   }
}