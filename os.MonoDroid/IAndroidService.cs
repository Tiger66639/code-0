//-----------------------------------------------------------------------
// <copyright file="IAndroidService.cs">
//     Copyright (c) 2008-2012 Jan Bogaerts. All rights reserved.
// </copyright> 
// <authorJan Bogaerts</author>
// <email>Jan.Bogaerts@telenet.be</email>
// <date>20/05/2012</date>
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

namespace os.MonoDroid
{
   /// <summary>
   /// an interface that should be implemented by the service that manages the network so that there is a callback mechanisme.
   /// The service registers itself at <see cref="ReflectionSin.Context"/> so that the entire system can access this value.
   /// </summary>
   public interface IAndroidService
   {
      /// <summary>
      /// Gets the activity that is currently associtiated with the Android service. This allows us to call sertain functions
      /// from the context of an activity (like StartActivityWithREsult).
      /// </summary>
      Activity Activity { get; }

      /// <summary>
      /// Instructs the service to display the main activity again.
      /// </summary>
      void ShowMainActivity();

      void ShowAddCalendarItem(CalendarItem item);


      void ShowHelp();
   }
}