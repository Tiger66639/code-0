//-----------------------------------------------------------------------
// <copyright file="AssetInstaller.cs">
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
using System.Net;
using System.IO;

namespace AiciAndroid.Activities.helpers
{
   class AssetInstaller
   {
      /// <summary>
      /// contacts the server and downloads all the available
      /// </summary>
      /// <param name="context">a possible context that can be used to store a new default background image. can be null.</param>
      public static void InstallBackgroundImages(Context context = null)
      {
         string iGetFilesAddrs = "http://androidserver.bragisoft.com/List/Images";
         //string iFilesAddrs = "http://androidserver.bragisoft.com/files/Images";
         string iDestPath;
         if (Directory.Exists(Android.OS.Environment.DirectoryPictures) == true)
            iDestPath = iDestPath = Path.Combine(Android.OS.Environment.DirectoryPictures, "Aici");
         else
         {
            iDestPath = Path.Combine(Android.OS.Environment.ExternalStorageDirectory.AbsolutePath, "DCIM");    //dcim is the default images dir.
            if (Directory.Exists(iDestPath) == false)
               Directory.CreateDirectory(iDestPath);
				iDestPath = iDestPath = Path.Combine(iDestPath, "Aici");
         }
         if (Directory.Exists(iDestPath) == false)
            Directory.CreateDirectory(iDestPath);
         string iLast = null;
         using (WebClient client = new WebClient())
         {
            string iImages = client.DownloadString(new Uri(iGetFilesAddrs));                  //get the list of files available.
            string[] iFiles = iImages.Split(new char[] { '\n', '\r' },StringSplitOptions.RemoveEmptyEntries);
            foreach (string i in iFiles)
            {
               iLast = Path.Combine(iDestPath, Path.GetFileName(i));
               DownloadFile(i, iLast); //can't do async, WebClient wont allow concurrent downlaods (probably a missing setting)
               
            }
         }
         if (iLast != null && context != null)                                               //try to store a default value.
            AiciPrefs.Default.SetBackgroundImage(iLast, context);
      }

      /// <summary>
      /// installs all the images used for the animations.
      /// </summary>
      public static void InstallAnimationImages(Context context)
      {
         //context.ExternalCacheDir.Path

         string iGetCharsAddrs = string.Format("http://androidserver.bragisoft.com/list/Characters");
         string iFilesAddrs = "http://androidserver.bragisoft.com/files/Characters";

         string iDestPath = Path.Combine(Android.OS.Environment.DirectoryPictures, "Aici");
         if (Directory.Exists(Android.OS.Environment.DirectoryPictures) == false)
            Directory.CreateDirectory(Android.OS.Environment.DirectoryPictures);
         if (Directory.Exists(iDestPath) == false)
            Directory.CreateDirectory(iDestPath);
         iDestPath = Path.Combine(iDestPath, "Characters");
         if (Directory.Exists(iDestPath) == false)
            Directory.CreateDirectory(iDestPath);

         using (WebClient client = new WebClient())
         {
            string iLines = client.DownloadString(new Uri(iGetCharsAddrs));                  //get the list of available characters that can be downloaded.
            string[] iChrs = iLines.Split(new char[] { '\n', '\r' });
            foreach (string iChr in iChrs)
            {
               string iCharPath = Path.Combine(iDestPath, iChr);
               if (Directory.Exists(iCharPath) == false)
                  Directory.CreateDirectory(iCharPath);

               string iGetFilesAddrs = Path.Combine(iGetCharsAddrs, iChr);
               iLines = client.DownloadString(new Uri(iGetFilesAddrs));                  //get the list of files for the specified char.
               string[] iFiles = iLines.Split(new char[] { '\n', '\r' });
               foreach (string i in iFiles)
                  DownloadFile(Path.Combine(iFilesAddrs, iChr, i), Path.Combine(iCharPath, i)); //can't do async, WebClient wont allow concurrent downlaods (probably a missing setting)
            }
         }
      }

      /// <summary>
      /// Gets the list of available items from the server.
      /// </summary>
      /// <param name="type"></param>
      /// <returns></returns>
      public static string[] GetAvailable(string type)
      {
         string iGetCharsAddrs = string.Format("http://androidserver.bragisoft.com/list/{0}", type);

         using (WebClient client = new WebClient())
         {
            string iLines = client.DownloadString(new Uri(iGetCharsAddrs));                  //get the list of available characters that can be downloaded.
            return iLines.Split(new char[] { '\n', '\r' });
         }
      }

      /// <summary>
      /// Gets the list of available networks.
      /// </summary>
      /// <returns></returns>
      public static string[] GetAvailableNetworks()
      {
         return GetAvailable("Networks");
      }

      /// <summary>
      /// Installs the specified brain to the server.
      /// </summary>
      /// <param name="name">The name.</param>
      public static void InstallBrain(string name)
      {
         string iGetFilesAddrs = string.Format("http://androidserver.bragisoft.com/list/Networks/{0}", name);
         string iFilesAddrs = string.Format("http://androidserver.bragisoft.com/files/Networks/{0}", name);
         string iAiciPath = Path.Combine(Android.OS.Environment.ExternalStorageDirectory.AbsolutePath, AiciActivity.AICIDIR, name);
         if (Directory.Exists(iAiciPath) == false)
            Directory.CreateDirectory(iAiciPath);
         using (WebClient client = new WebClient())
         {
            string iImages = client.DownloadString(new Uri(iGetFilesAddrs));                  //get the list of files available.
            string[] iFiles = iImages.Split(new char[] { '\n', '\r' });
            foreach (string i in iFiles)
               DownloadFile(Path.Combine(iFilesAddrs, i), Path.Combine(iAiciPath, i));  //can't do async, WebClient wont allow concurrent downlaods (probably a missing setting)
         }
      }

      /// <summary>
      /// downloads the content of a file from the source (web address) to the destination.
      /// </summary>
      /// <param name="dest">The dest.</param>
      /// <param name="name">The name.</param>
      static private void DownloadFile(string source, string dest)
      {
         //Java.Net.URL u = new Java.Net.URL(source);
         //using (Java.Net.HttpURLConnection c = (Java.Net.HttpURLConnection)u.OpenConnection())
         //{
         //   c.RequestMethod = "GET";
         //   c.DoOutput = true;
         //   c.Connect();

         //   using (System.IO.Stream iStream = c.InputStream)
         //   {
         //      byte[] buffer = new byte[1024];
         //      int read;
         //      bool iOk = true;

         //      using (Stream iWriter = new FileStream(dest, FileMode.Create))
         //      {
         //         while (iOk == true)
         //         {
         //            read = iStream.Read(buffer, 0, buffer.Length);
         //            iWriter.Write(buffer, 0, read);
         //            iOk = read == buffer.Length;
         //         }
         //      }
         //   }
         //}


         Java.Net.URL u = new Java.Net.URL(source);
         using(Stream iStream = u.OpenStream())
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
                  iOk = read > 0;                                    //can't assume that the entire buffer got filled: the input Stream might also be using some form of buffering or so (need to do this, otherwise the file doesn't get completely downloaded).
               }
            }
		 }

      }
   }
}