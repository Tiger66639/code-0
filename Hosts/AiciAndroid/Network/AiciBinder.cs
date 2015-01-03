//-----------------------------------------------------------------------
// <copyright file="AiciBinder.cs">
//     Copyright (c) 2008-2012 Jan Bogaerts. All rights reserved.
// </copyright> 
// <authorJan Bogaerts</author>
// <email>Jan.Bogaerts@telenet.be</email>
// <date>27/12/2011</date>
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

namespace AiciAndroid.Network
{
   /// <summary>
   /// Provides binding between the AiciServer and the AiciActivity.
   /// </summary>
   public class AiciBinder: Binder
   {
      /// <summary>
      /// Provides direct access to the server.
      /// </summary>
      public AiciServer Server { get; set; }
   }
}