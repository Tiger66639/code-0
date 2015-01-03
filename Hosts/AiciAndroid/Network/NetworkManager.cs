//-----------------------------------------------------------------------
// <copyright file="NetworkManager.cs">
//     Copyright (c) 2008-2012 Jan Bogaerts. All rights reserved.
// </copyright> 
// <authorJan Bogaerts</author>
// <email>Jan.Bogaerts@telenet.be</email>
// <date>10/05/2012</date>
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
using AiciAndroid.Network;
using System.Threading;
using AiciAndroid.Activities.helpers;

namespace AiciAndroid
{
   /// <summary>
   /// provides function for managing networks on sd-card.
   /// </summary>
   class NetworkManager
   {
      Action fCallWhenDone;
      Context fContext;
      ProgressDialog fDlg;

      /// <summary>
      /// gets the path to where all the networks are located.
      /// </summary>
      public static string AiciSdPath
      {
         get
         {
            return Path.Combine(Android.OS.Environment.ExternalStorageDirectory.AbsolutePath, AiciActivity.AICIDIR);
         }
      }

      /// <summary>
      /// Deletes a network from disk.
      /// </summary>
      /// <param name="network"></param>
      public static void Delete(string network)
      {
         string iPath = Path.Combine(AiciSdPath, network);
         if (Directory.Exists(iPath))
            Directory.Delete(iPath, true);
         iPath = Path.Combine(AiciSdPath, network + ".dpl");
         if (File.Exists(iPath) == true)
            File.Delete(iPath);
      }

      /// <summary>
      /// installs the default network (and images) without a user dialog.
      /// </summary>
      public static void InstallSilent()
      {
         NetworkManager.Delete(AiciActivity.AICIDEFAULTDIR);
         NetworkManager.InstallDefault();
         AssetInstaller.InstallBackgroundImages();
      }

      /// <summary>
      /// shows a progress dialog box while deleting a possible previous default network and installing the new one.
      /// </summary>
      /// <param name="context">The context.</param>
      /// <param name="callWhenDone">a possible action to be done when everything is installed.</param>
      public static void InstallDefaultWithDlg(Context context, Action callWhenDone)
      {
         NetworkManager iManager = new NetworkManager();
         iManager.fDlg = ProgressDialog.Show(context, "First run", "Installing default brain...", true, false);
         iManager.fContext = context;
         iManager.fCallWhenDone = callWhenDone;
         Action iAsync = new Action(iManager.InstallDefaultAndImagesAsync);
         iAsync.BeginInvoke(null, null);
      }

      /// <summary>
      /// called async from a ProgressDialog. Deletes the previous network, installs the new default.
      /// </summary>
      private void InstallDefaultAndImagesAsync()
      {
         //Thread.Sleep (1000);												//so that the dialog has time to show
         Delete(AiciActivity.AICIDEFAULTDIR);
         InstallDefault();
         AssetInstaller.InstallBackgroundImages(fContext);
         ((Activity) fContext).RunOnUiThread (new Action(fDlg.Dismiss) ) ;
         if (fCallWhenDone != null)                                    //when done, let caller know that it can continue.
         {
            ((Activity) fContext).RunOnUiThread (new Action(fCallWhenDone) ) ;
            fCallWhenDone = null;
         }
      }

      /// <summary>
      /// installs the default network.
      /// </summary>
      public static void InstallDefault()
      {
         string iAiciPath = AiciSdPath;
         string iDataPath = Path.Combine(iAiciPath, AiciActivity.AICIDEFAULTDIR);
         string iFile = Path.Combine(iDataPath, AiciServer.NETWORKFILE);

         if (Directory.Exists(iAiciPath) == false)
            Directory.CreateDirectory(iAiciPath);
         if (Directory.Exists(iDataPath) == false)
            Directory.CreateDirectory(iDataPath);
         CopyFile(iFile, "brain.xml");
         iFile = Path.Combine(iDataPath, "Data");
         if (Directory.Exists(iFile) == false)
            Directory.CreateDirectory(iFile);
         foreach (string i in Application.Context.Assets.List("Data"))
         {
            string iDest = Path.Combine(iFile, i);
            CopyFile(iDest, Path.Combine("Data", i));
         }

         iFile = Path.Combine(iDataPath, "Modules");
         if (Directory.Exists(iFile) == false)
            Directory.CreateDirectory(iFile);
         foreach (string i in Application.Context.Assets.List("Modules"))
         {
            string iDest = Path.Combine(iFile, i);
            CopyFile(iDest, Path.Combine("Modules", i));
         }
      }

      /// <summary>
      /// Copies the content of a file from the assets (compressed) to the external dir, so that it can be operated on.
      /// </summary>
      /// <param name="dest">The dest.</param>
      /// <param name="name">The name.</param>
      static private void CopyFile(string dest, string name)
      {
         using (Stream iStream = Application.Context.Assets.Open(name))
         {
            byte[] buffer = new byte[1024];
            int read;
            bool iOk = true;

            using (Stream iWriter = new FileStream(dest, FileMode.Create))
            {
               while (iOk == true)
               {
                  read = iStream.Read(buffer, 0, buffer.Length);
                  iWriter.Write(buffer, 0, read);
                  iOk = read == buffer.Length;
               }
            }
         }
      }
   }
}