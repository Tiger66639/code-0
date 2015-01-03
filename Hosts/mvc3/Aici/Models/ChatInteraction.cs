//-----------------------------------------------------------------------
// <copyright file="ChatInteraction.cs">
//     Copyright (c) 2008-2012 Jan Bogaerts. All rights reserved.
// </copyright> 
// <authorJan Bogaerts</author>
// <email>Jan.Bogaerts@telenet.be</email>
// <date>10/12/2011</date>
//-----------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Aici.Models
{
    /// <summary>
    /// represents a single chat interaction object that is used to send data from the controller to the view.
    /// </summary>
    public class ChatInteraction
    {
        /// <summary>
        /// Gets/sets the input value that should be sent to the bot.
        /// </summary>
        public string Input { get; set; }

        /// <summary>
        /// Gets the list of previous conversation statements.
        /// </summary>
        public IEnumerable<OutputValue> Conversation{ get; set; }

        /// <summary>
        /// For the speech system, so that it knows which line to say.
        /// </summary>
        public string LastOutput { get; set; }

       /// <summary>
       /// this prop can be filled in to specify which extra function calls should be rendered. For instance, the CrazyTalk controller uses this
       /// to let the send view now it needs to render audio sync.
       /// </summary>
        public string SyncWith { get; set; }

        /// <summary>
        /// Gets if a custom web page should be used or not (controlled by the desktop app).
        /// When true, the html controller doesn't return input boxes, but will only render the chatlog in a div.
        /// </summary>
        public bool UseCustomPage
        {
            get
            {
                return Properties.Settings.Default.UseCustomPage;
            }
        }
    }
}