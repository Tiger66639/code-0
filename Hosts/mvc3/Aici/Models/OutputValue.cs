//-----------------------------------------------------------------------
// <copyright file="OutputValue.cs">
//     Copyright (c) 2008-2012 Jan Bogaerts. All rights reserved.
// </copyright> 
// <authorJan Bogaerts</author>
// <email>Jan.Bogaerts@telenet.be</email>
// <date>04/12/2011</date>
//-----------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Aici.Models
{
    /// <summary>
    /// stores a single conversation line that a <see cref="TextChannel"/> can use.
    /// </summary>
    public class OutputValue
    {
        /// <summary>
        /// Gets/sets the text to display.
        /// </summary>
        public string Text { get; set; }

        /// <summary>
        /// Gets/sets the source of the person.
        /// </summary>
        public string Source { get; set; }
    }
}