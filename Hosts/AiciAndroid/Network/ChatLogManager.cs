//-----------------------------------------------------------------------
// <copyright file="ChatLogManager.cs">
//     Copyright (c) 2008-2012 Jan Bogaerts. All rights reserved.
// </copyright> 
// <authorJan Bogaerts</author>
// <email>Jan.Bogaerts@telenet.be</email>
// <date>23/01/2012</date>
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
using System.IO;
using os.MonoDroid;

namespace AiciAndroid.Network
{
   /// <summary>
   /// manages the chatlog when the application stops and starts: stores the previous log and when started, it asks if this can
   /// be sent to the developer for further analysis.
   /// </summary>
   class ChatLogManager
   {
      string fPath;                                                                             //so we can pass this along to the callback
      /// <summary>
      /// Saves the log to disk.
      /// </summary>
      /// <param name="items"></param>
      public static void SaveLog(LogAdapter items)
      {
         if (Android.OS.Environment.ExternalStorageState == (Android.OS.Environment.MediaMounted) == true)              //don't try this if there is no sd card
         {
            string iAiciPath = Path.Combine(Android.OS.Environment.ExternalStorageDirectory.AbsolutePath, AiciActivity.AICIDIR, "prevchatlog.txt");
            using (TextWriter iWriter = File.CreateText(iAiciPath))
            {
               for (int i = 0; i < items.Count; i++)
               {
                  UILogItem iItem = items.GetItem(i);
                  if (iItem != null)
                     iWriter.WriteLine("{0} : {1}", iItem.Source.ToString(), iItem.Text);
               }
            }
         }
      }

      /// <summary>
      /// checks if there is a previous log on disk and asks if this can be sent out.
      /// </summary>
      public static void SendPreviousLog(Context context)
      {
         string iAiciPath = Path.Combine(Android.OS.Environment.ExternalStorageDirectory.AbsolutePath, AiciActivity.AICIDIR, "prevchatlog.txt");
         if (File.Exists(iAiciPath) == true)
         {
            AlertDialog.Builder alert = new AlertDialog.Builder(context);
            alert.SetTitle("Help us");
            alert.SetMessage("Can I sent the previous chatlog to the developer so it can be used to improve the appliation?");

            // Set an EditText view to get user input 
            CheckBox input = new CheckBox(context);
            alert.SetView(input);

            ChatLogManager iNew = new ChatLogManager();
            iNew.fPath = iAiciPath;
            alert.SetPositiveButton("Ok", new EventHandler<DialogClickEventArgs>(iNew.OnOk));
            alert.Show();

         }
      }

      public void OnOk(Object sender, DialogClickEventArgs e)
      {
         //Communication.SendEmail(new string[]{"bragi@jastdev.com"},
         //String value = input.getText();
         // Do something with value!
      }


      
   }
}