//-----------------------------------------------------------------------
// <copyright file="TextLogService.cs">
//     Copyright (c) 2008-2012 Jan Bogaerts. All rights reserved.
// </copyright> 
// <authorJan Bogaerts</author>
// <email>Jan.Bogaerts@telenet.be</email>
// <date>22/05/2012</date>
//-----------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using JaStDev.LogService;
using System.IO;

namespace Aici.Network
{
    public class TextLogService : ILogService
    {
        static TextLogService fDefault;
        FileInfo fFile;

        public TextLogService(string fileName, string ip)
        {
            fFile = new FileInfo(fileName);
            if (File.Exists(fileName) == false)
            {
                using (StreamWriter iStream = fFile.CreateText())
                {
                    if (string.IsNullOrEmpty(ip) == true)
                        iStream.WriteLine("General log");
                    else
                    {
                        iStream.WriteLine("Log for: {0}", ip);
                    }
                }
            }
        }

        public static TextLogService Default
        {
            get
            {
                if (fDefault == null)
                    fDefault = new TextLogService(Properties.Settings.Default.LogFile, null);
                return fDefault;
            }
        }


        public void WriteToLogAsync(LogItem aItem)
        {
            Action<LogItem> iWrite = new Action<LogItem>(WriteToLog);
            iWrite.BeginInvoke(aItem, null, null);
        }

        void WriteToLog(LogItem item)
        {
            if (fFile != null)
            {
                try
                {
                    using (StreamWriter iWriter = fFile.AppendText())
                        iWriter.WriteLine("{0} {1} ({2}): {3}", item.Level, item.Time, item.Source, item.Text);
                }
                catch (Exception e)
                {
                }
            }
        }

        public void WriteToLog(string header, string text)
        {
            if (fFile != null)
            {
                try
                {
                    using (StreamWriter iWriter = fFile.AppendText())
                        iWriter.WriteLine("{0}: {1}", header, text);
                }
                catch (Exception e)
                {
                }
            }
        }
    }
}