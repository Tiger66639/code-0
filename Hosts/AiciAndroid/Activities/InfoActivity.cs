//-----------------------------------------------------------------------
// <copyright file="InfoActivity.cs">
//     Copyright (c) 2008-2012 Jan Bogaerts. All rights reserved.
// </copyright> 
// <authorJan Bogaerts</author>
// <email>Jan.Bogaerts@telenet.be</email>
// <date>07/04/2012</date>
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
using Android.Text.Util;

namespace AiciAndroid.Acitivities
{
   [Activity(Label = "Info")]
   public class InfoActivity : Activity
   {
      protected override void OnCreate(Bundle bundle)
      {
         base.OnCreate(bundle);
         SetContentView(Resource.Layout.Info);                                            // Set our view from the "main" layout resource
         // Create your application here
         TextView iLink = FindViewById<TextView>(Resource.Id.homepage);
         Linkify.AddLinks(iLink, MatchOptions.WebUrls);
      }
   }
}