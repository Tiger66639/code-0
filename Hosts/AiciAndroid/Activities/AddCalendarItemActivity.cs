//-----------------------------------------------------------------------
// <copyright file="AddCalendarItemActivity.cs">
//     Copyright (c) 2008-2012 Jan Bogaerts. All rights reserved.
// </copyright> 
// <authorJan Bogaerts</author>
// <email>Jan.Bogaerts@telenet.be</email>
// <date>04/03/2012</date>
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
using os.MonoDroid;
using Android.Speech;

namespace AiciAndroid
{
   /// <summary>
   /// This activity manages the input screen for calendar items (events).
   /// </summary>
   [Activity(Label = "Add event")]
   public class AddCalendarItemActivity : Activity
   {
      CalendarItem fValues;
      STT fSTT;
      /// <summary>
      /// Called when created.
      /// </summary>
      /// <param name="bundle">The bundle.</param>
      protected override void OnCreate(Bundle bundle)
      {
         base.OnCreate(bundle);

         fValues = CalendarAndroid.CurrentItem;
         fSTT = new STT(this);
         if (fValues != null)
         {
            fValues.PropertyChanged += new DataCore.PropertyChangedHandler(fValues_PropertyChanged);
            AssignTitle();
            AssignLocation();
            AssignDescription();
            AssignEndDate();
            AssignEndDate();

            //CheckBox iCheck = FindViewById<CheckBox>(Resource.Id.ChkIsRecurring);
            //if (iCheck != null)
            //   iCheck.Checked = iExtras.GetBoolean("Recurring");
         }
         Button iOk = FindViewById<Button>(Resource.Id.BtnAddCalOk);
         iOk.Click += new EventHandler(Ok_Click);

         Button iSpeak = FindViewById<Button>(Resource.Id.BtnAddCalSpeak);
         iSpeak.Click += new EventHandler(Speak_Click);
         iSpeak.Enabled = fSTT.HasSTT;
      }

      protected override void OnActivityResult(int requestCode, Result resultCode, Intent data)
      {
         if (requestCode == STT.HANDLESTTINPUT)        //got some input text from the STT
         {
            if (resultCode == Result.Ok && AiciActivity.Server != null)
            {
               fSTT.CurrentInputs = data.GetStringArrayListExtra(RecognizerIntent.ExtraResults);             //get the results of the STT and send them to the network.
               AiciActivity.Server.Process(fSTT.CurrentInputs);
            }
         }
      }

      void fValues_PropertyChanged(object sender, DataCore.PropertyChangedEventArgs e)
      {
         if (e.PropertyName == "Title")
            RunOnUiThread(new Action(AssignTitle));

         if (e.PropertyName == "Location")
            RunOnUiThread(new Action(AssignLocation));

         if (e.PropertyName == "Description")
            RunOnUiThread(new Action(AssignDescription));

         if (e.PropertyName == "EndDate")
            RunOnUiThread(new Action(AssignEndDate));

         if (e.PropertyName == "StartDate")
            RunOnUiThread(new Action(AssignStartDate));
      }

      void AssignLocation()
      {
         TextView iView = FindViewById<TextView>(Resource.Id.LblLocationData);
         if (iView != null)
            iView.Text = fValues.Location;
      }

      void AssignDescription()
      {
         TextView iView = FindViewById<TextView>(Resource.Id.LblDescriptionData);
         if (iView != null)
            iView.Text = fValues.Description;
      }

      void AssignEndDate()
      {
         TextView iView = FindViewById<TextView>(Resource.Id.LblEndDate);
         if (iView != null)
            iView.Text = fValues.EndDate.ToString();
      }

      void AssignStartDate()
      {
         TextView iView = FindViewById<TextView>(Resource.Id.LblDateData);
         if (iView != null)
            iView.Text = fValues.StartDate.ToString();
      }

      void AssignTitle()
      {
         TextView iView = FindViewById<TextView>(Resource.Id.LblTitleData);
         if (iView != null)
            iView.Text = fValues.Title;
      }

      /// <summary>
      /// Handles the Click event of the Cancel control.
      /// </summary>
      /// <param name="sender">The source of the event.</param>
      /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
      void Speak_Click(object sender, EventArgs e)
      {
         fSTT.TryStartSTT(this);
      }

      /// <summary>
      /// Handles the Click event of the Ok control.
      /// </summary>
      /// <param name="sender">The source of the event.</param>
      /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
      void Ok_Click(object sender, EventArgs e)
      {
         Intent intent = new Intent(Intent.ActionEdit);
         intent.SetType("vnd.android.cursor.item/event");
         if (string.IsNullOrEmpty(fValues.Title) == false)
            intent.PutExtra("title", fValues.Title);
         if (string.IsNullOrEmpty(fValues.Description) == false)
            intent.PutExtra("description", fValues.Description);
         if (string.IsNullOrEmpty(fValues.Location) == false)
            intent.PutExtra("eventLocation", fValues.Location);
         intent.PutExtra("beginTime", fValues.StartDate.Ticks);
         intent.PutExtra("endTime", fValues.EndDate.Ticks);
         StartActivity(intent);
         CalendarAndroid.ResetCurrentItem();
         Finish();
      }
   }
}