//-----------------------------------------------------------------------
// <copyright file="IIntentResultService.cs">
//     Copyright (c) 2008-2012 Jan Bogaerts. All rights reserved.
// </copyright> 
// <authorJan Bogaerts</author>
// <email>Jan.Bogaerts@telenet.be</email>
// <date>07/05/2012</date>
//-----------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace os.MonoDroid
{
   /// <summary>
   /// provides support for intent results so that the backend can receive them.
   /// </summary>
   public interface IIntentResultService
   {

      /// <summary>
      /// the event to signal when there is a result found (or canceled). This allows the calling thread to block untill the user is done.
      /// </summary>
      AutoResetEvent ResultFlag { get; }

      /// <summary>
      /// gets the last string result (if not canceled.
      /// </summary>
      /// <param name="value"></param>
      /// <returns>true if the user selected the value, false if the user canceled the action.</returns>
      bool GetStringResult(out string value);
   }
}