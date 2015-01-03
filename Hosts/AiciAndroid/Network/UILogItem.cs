//-----------------------------------------------------------------------
// <copyright file="UILogItem.cs">
//     Copyright (c) 2008-2012 Jan Bogaerts. All rights reserved.
// </copyright> 
// <authorJan Bogaerts</author>
// <email>Jan.Bogaerts@telenet.be</email>
// <date>28/12/2011</date>
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

namespace AiciAndroid
{
   public enum SourceType
   {
      User,
      Bot
   }

   /// <summary>
   /// contains the data for a single log item (input or output).
   /// </summary>
   public class UILogItem
   {
      /// <summary>
      /// Gets/sets the text to display.
      /// </summary>
      public string Text { get; set; }

      /// <summary>
      /// Gets/sets the source of the person.
      /// </summary>
      public SourceType Source { get; set; }
   }
}