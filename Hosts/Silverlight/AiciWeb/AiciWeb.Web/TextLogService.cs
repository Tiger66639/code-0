using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using JaStDev.LogService;
using System.IO;
using System.ServiceModel;
using System.ServiceModel.Channels;

namespace AiciWeb.Web
{
    public class TextLogService: ILogService
    {
        static TextLogService fDefault;
        FileInfo fFile;

        public TextLogService(string fileName)
        {
            fFile = new FileInfo(fileName);
            if (File.Exists(fileName) == false)
            {
                using (StreamWriter iStream = fFile.CreateText())
                {
                    if (fileName == Properties.Settings.Default.LogFile)
                        iStream.WriteLine("General log");
                    else
                    {
                        RemoteEndpointMessageProperty clientEndpoint = OperationContext.Current.IncomingMessageProperties[RemoteEndpointMessageProperty.Name] as RemoteEndpointMessageProperty;
                        iStream.WriteLine("Log for: {0}:{1}", clientEndpoint.Address, clientEndpoint.Port);
                    }
                }
            }
        }

        public static TextLogService Default
        {
            get
            {
                if(fDefault == null)
                    fDefault = new TextLogService(Properties.Settings.Default.LogFile);
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
                using (StreamWriter iWriter = fFile.AppendText())
                    iWriter.WriteLine("{0} {1} ({2}): {3}", item.Level, item.Time, item.Source, item.Text);
            }
        }

        public void WriteToLog(string header, string text)
        {
            if (fFile != null)
            {
                using (StreamWriter iWriter = fFile.AppendText())
                    iWriter.WriteLine("{0}: {1}", header, text);
            }
        }
    }
}