using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using JaStDev.HAB;
using System.ServiceModel;
using System.Diagnostics;
using System.IO;

namespace AiciWeb.Web
{
    /// <summary>
    /// Provides a wrapper for the textsins. So that we can log everything that goes on (for further analysis)
    /// </summary>
    public class TextChannel
    {
        TextSin fSin;
        string fId;
        TextLogService fLogFile;
        IAiciCallback fCallBack;

        public TextChannel()
        {
            fLogFile = new TextLogService(Path.Combine(Properties.Settings.Default.ConversationsLogLoc, Path.GetFileName(Path.GetTempFileName())));
            fCallBack = OperationContext.Current.GetCallbackChannel<IAiciCallback>();                           //get now: when we need to write to the log, we have lost the context of the caller, but during creation, we know the requestor.
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
                if (fSin != null)
                    fSin.TextOut += fSin_TextOut;
            }
        }


        #region Id

        /// <summary>
        /// Gets/sets the id of this channel as seen by the outside world.
        /// </summary>
        public string Id
        {
            get
            {
                return fId;
            }
            set
            {
                fId = value;
            }
        }

        #endregion

        /// <summary>
        /// Handles the TextOut event of the fSin control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="JaStDev.HAB.OutputEventArgs&lt;System.String&gt;"/> instance containing the event data.</param>
        void fSin_TextOut(object sender, OutputEventArgs<string> e)
        {
            if (fCallBack != null)
            {
                fCallBack.Output(e.Value);
                fLogFile.WriteToLog("PC", e.Value);
            }

        }

        internal void Process(string message)
        {
            fLogFile.WriteToLog("You", message);
            Processor iProc = new Processor();
            Sin.Process(message, iProc, TextSinProcessMode.LetterStream);
        }

        /// <summary>
        /// Provides a way to close all the logs and unregister everything.
        /// </summary>
        internal void Close()
        {
            if (fSin != null)
                fSin.TextOut -= fSin_TextOut;
        }
    }
}