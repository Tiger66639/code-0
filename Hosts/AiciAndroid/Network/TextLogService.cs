//-----------------------------------------------------------------------
// <copyright file="TextLogService.cs">
//     Copyright (c) 2008-2012 Jan Bogaerts. All rights reserved.
// </copyright> 
// <authorJan Bogaerts</author>
// <email>Jan.Bogaerts@telenet.be</email>
// <date>21/02/2012</date>
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
using JaStDev.LogService;

namespace AiciAndroid.Network
{
   class TextLogService: ILogService
   {
      static TextLogService fDefault;
      static bool fUseAndroidLog = true;
      FileInfo fFile;

      #region UseAndroidLog

      /// <summary>
      /// Gets/sets the value taht indicates if the android log is used or the internal version.
      /// True by default
      /// </summary>
      static public bool UseAndroidLog
      {
         get
         {
            return fUseAndroidLog;
         }
         set
         {
            if (value != fUseAndroidLog)
            {
               fUseAndroidLog = value;
               fDefault = null;                             //reset the log so we recreate the object.
            }
         }
      }

      #endregion

      /// <summary>
      /// Creaets a new text log service.
      /// </summary>
      /// <param name="fileName"></param>
      private TextLogService(string fileName)
      {
         string iPath = Path.Combine(Android.OS.Environment.ExternalStorageDirectory.AbsolutePath, AiciActivity.AICIDIR, fileName);
         fFile = new FileInfo(iPath);
         try
         {
            if (fFile.Exists == false)
            {
               string iAiciPath = Path.Combine(Android.OS.Environment.ExternalStorageDirectory.AbsolutePath, AiciActivity.AICIDIR);
               if (Directory.Exists(iAiciPath) == false)
                  Directory.CreateDirectory(iAiciPath);
               using (StreamWriter iStream = fFile.CreateText())
               {
                  iStream.WriteLine("Log created");
                  iStream.Flush();
               }
            }
         }
         catch
         {
            fFile = null;                                      //if something went wrong, make certain we don't try to log.
         }
      }

      /// <summary>
      /// Prevents a default instance of the <see cref="TextLogService"/> class from being created.
      /// used when we log to android.
      /// </summary>
      private TextLogService()
      {
      }

      /// <summary>
      /// Gets the default logging service.
      /// </summary>
      public static TextLogService Default
      {
         get
         {
            if (fDefault == null)
            {
               if (UseAndroidLog == true)
                  fDefault = new TextLogService();
               else
                  fDefault = new TextLogService("logfile.txt");
            }
            return fDefault;
         }
      }

      /// <summary>
      /// checks if the log has been properly initialized.
      /// </summary>
      public static bool IsInit
      {
         get
         {
            return fDefault != null && fDefault.fFile != null;
         }
      }

      public void WriteToLogAsync(LogItem aItem)
      {
         Action<LogItem> iWrite = new Action<LogItem>(WriteToLog);
         iWrite.BeginInvoke(aItem, null, null);
      }

      void WriteToLog(LogItem item)
      {

         try
         {
            if (UseAndroidLog == true)
            {
               switch (item.Level)
               {
                  case LogLevel.Error:
                     Android.Util.Log.Error(item.Source, item.Text);
                     break;
                  case LogLevel.Info:
                     Android.Util.Log.Info(item.Source, item.Text);
                     break;
                  case LogLevel.Warning:
                     Android.Util.Log.Warn(item.Source, item.Text);
                     break;
                  default:
                     break;
               }
            }
            else
            {
               if (fFile != null)
               {
                  using (StreamWriter iWriter = fFile.AppendText())
                  {
                     iWriter.WriteLine("{0} {1} ({2}): {3}", item.Level, item.Time, item.Source, item.Text);
                     iWriter.Flush();
                  }
               }
            }
         }
         catch
         {
         }
      }

      public void WriteToLog(string header, string text)
      {
         try
         {
            if (UseAndroidLog == true)
            {
               Android.Util.Log.Info(header, text);
            }
            else
            {
               if (fFile != null)
               {
                  using (StreamWriter iWriter = fFile.AppendText())
                  {
                     iWriter.WriteLine("{0}: {1}", header, text);
                     iWriter.Flush();
                  }
               }
            }
         }
         catch
         {
         }

      }
   }
}