//-----------------------------------------------------------------------
// <copyright file="TextChannel.cs">
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
using JaStDev.HAB;
using System.IO;
using System.Text;
using System.Threading;
using JaStDev.HAB.Characters;
using Aici.Models;
using JaStDev.LogService;

namespace Aici.Network
{
    /// <summary>
    /// Provides a wrapper for the textsins. So that we can log everything that goes on (for further analysis)
    /// </summary>
    public class TextChannel
    {
        const string SSMLSTRING = "<?xml version='1.0'?><speak xmlns='http://www.w3.org/2001/10/synthesis' version='1.0' xmlns:xsi='http://www.w3.org/2001/XMLSchema-instance' xsi:schemalocation='http://www.w3.org/2001/10/synthesis http://www.w3.org/TR/speech-synthesis/synthesis.xsd' xml:lang='en-US'>{0}</speak>";
        public const string BOT = "bot";
        const string USER = "You";


        TextSin fSin;
        string fIP;
        TextLogService fLogFile;
        List<string> fOutputs = new List<string>();
        ManualResetEvent fSignal = new ManualResetEvent(true);
        List<OutputValue> fFormattedOutputs = new List<OutputValue>();                                    //stores the outputs formatted for html output. When null: not needed to log this (for web api)

        public TextChannel(string ip)
        {
            try
            {
                fIP = ip;
                fLogFile = new TextLogService(Path.Combine(Properties.Settings.Default.ConversationsLogLoc, Path.GetFileName(Path.GetTempFileName())), ip);
                fSin = new TextSin();                                                                       //always create a new textsin for a channel, so a textsin remains session local.
                fSin.TextOut += fSin_TextOut;
                Brain.Current.Add(fSin);
            }
            catch (Exception e)
            {
                Log.LogError("Create channel", e.ToString());
                throw;
            }
        }

        /// <summary>
        /// gets the ip address associated with this channel.
        /// </summary>
        public string IP
        {
            get { return fIP; }
        }

        /// <summary>
        /// Gets the TextSin that this object provides access to.
        /// </summary>
        public TextSin Sin
        {
            get { return fSin; }
            internal set
            {
                if (fSin != null)
                    fSin.TextOut -= fSin_TextOut;
                fSin = value;
                    
            }
        }

        /// <summary>
        /// gets/sets if full logging should be done so that the controller show the entire conversation. true by default.
        /// </summary>
        public bool LogFullText
        {
            get { return fFormattedOutputs != null; }
            set
            {
                if (value != LogFullText)
                {
                    if (value == true)
                        fFormattedOutputs = new List<OutputValue>();
                    else
                        fFormattedOutputs = null;
                }
            }
        }

        /// <summary>
        /// returns a string that contains all the outputs  that are currently collected.
        /// </summary>
        /// <returns>the list of outputs that are waiting for this channel</returns>
        public string GetOutputs(bool asSSML = true)
        {
            lock (fOutputs)
            {
                string iRes = BuildOutputString(asSSML);
                fOutputs.Clear();
                return iRes;
            }
        }

        /// <summary>
        /// Gets the last output statements without removing them from the output list. This is used for hte speech system so we can both get the last statement
        /// and the list
        /// </summary>
        public string PeekLastOutputs
        {
            get
            {
                StringBuilder iRes = new StringBuilder();
                lock (fOutputs)
                {
                    if (fOutputs.Count > 0)
                    {
                        String iStr;
                        foreach (string i in fOutputs)
                        {
                            iStr = string.Format(SSMLSTRING, i);
                            iRes.Append(SSMLParser.ConvertSSMLToText(iStr));
                            iRes.Append(" ");                                               //for multiple statements, gets removed at the end, not very clean.
                        }
                    }
                }
                return iRes.ToString().Trim();                                  //trim the text so that the trailing enter is gone.
            }
        }

        /// <summary>
        /// gets  the entire conversation log for the current session, so it can be displayed.
        /// </summary>
        /// <returns></returns>
        public List<OutputValue> GetOutputList()
        {
            if (fFormattedOutputs != null)
            {
                lock (fOutputs)
                {
                    if (fOutputs.Count > 0)
                    {
                        String iStr;
                        foreach (string i in fOutputs)
                        {
                            iStr = string.Format(SSMLSTRING, i);
                            iStr = string.Format("{0}: {1}", BOT, SSMLParser.ConvertSSMLToText(iStr));
                            fFormattedOutputs.Add(new OutputValue() { Text = iStr, Source = BOT });
                        }
                        fOutputs.Clear();
                    }
                }
                return fFormattedOutputs.ToList();                                  //make a copy for tread savety.
            }
            return null;
        }

        private string BuildOutputString(bool asSSML = true)
        {
            if (fOutputs.Count > 0)
            {
                StringBuilder iStr = new StringBuilder();
                foreach (string i in fOutputs)
                    iStr.Append(i);
                if (asSSML == true)
                    return string.Format(SSMLSTRING, iStr.ToString());
                else
                    return iStr.ToString();
            }
            else
                return null;
        }

        /// <summary>
        /// Handles the TextOut event of the fSin control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="JaStDev.HAB.OutputEventArgs&lt;System.String&gt;"/> instance containing the event data.</param>
        void fSin_TextOut(object sender, OutputEventArgs<string> e)
        {
            lock (fOutputs)
            {
                fOutputs.Add(e.Value);
                fLogFile.WriteToLog("PC", e.Value);
                fSignal.Set();                                                          //the api does a blocked call, so when some data came back, let the other thread know we got something.
            }
        }

        internal void Process(string message)
        {
            fLogFile.WriteToLog("You", message);
            if (fFormattedOutputs != null)
                fFormattedOutputs.Add(new OutputValue() { Text = string.Format("{0}: {1}", USER, message), Source = USER });
            Processor iProc = ProcessorFactory.GetProcessor();
            Sin.Process(message, iProc, TextSinProcessMode.ClusterAndDict);
        }


        /// <summary>
        /// sends the message to the network and waits until some data comes back or until a timeout is reached.
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        internal void ProcessWait(string message)
        {
            fSignal.Reset();
 	        Process(message);
            fSignal.WaitOne(Properties.Settings.Default.SendTimeOutMs);                                 //we timeout so that we don't get stuck for ever.
        }

        /// <summary>
        /// Provides a way to close all the logs and unregister everything.
        /// </summary>
        internal void Close()
        {
            if (fSin != null)
            {
                fSin.TextOut -= fSin_TextOut;
                Brain.Current.Delete(fSin);                                                 //we created it for this track, so also delete it again.
            }
        }
    }
}