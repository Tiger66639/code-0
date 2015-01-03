//-----------------------------------------------------------------------
// <copyright file="CalendarItem.cs">
//     Copyright (c) 2008-2012 Jan Bogaerts. All rights reserved.
// </copyright> 
// <authorJan Bogaerts</author>
// <email>Jan.Bogaerts@telenet.be</email>
// <date>23/01/2012</date>
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
using DataCore;

namespace os.MonoDroid
{
   /// <summary>
   /// Contains all the data for a single calendar event. this is used so that the UI can sync with this object.
   /// </summary>
   public class CalendarItem : ObservableObject
   {
      public static String CALENDAR_UPDATE = "com.client.gaitlink.AiciServer.action.CALENDAR_UPDATE";

      string fTitle;
      bool fRecurring = false;
      DateTime fStartDate;
      string fDescription;
      string fLocation;

      #region Title

      /// <summary>
      /// Gets/sets the title of the calendar item
      /// </summary>
      public string Title
      {
         get
         {
            return fTitle;
         }
         set
         {
            fTitle = value;
            OnPropertyChanged("Title");
         }
      }

      #endregion

      #region Location

      /// <summary>
      /// Gets/sets the location of appointement
      /// </summary>
      public string Location
      {
         get
         {
            return fLocation;
         }
         set
         {
            fLocation = value;
            OnPropertyChanged("Location");
         }
      }

      #endregion
      
      #region Date

      /// <summary>
      /// Gets/sets the date of the calendar item
      /// </summary>
      public DateTime StartDate
      {
         get
         {
            return fStartDate;
         }
         set
         {
            fStartDate = value;
            OnPropertyChanged("StartDate");
         }
      }

      #endregion

      DateTime fEndDate;
      #region EndDate

      /// <summary>
      /// Gets/sets the end date
      /// </summary>
      public DateTime EndDate
      {
         get
         {
            return fEndDate;
         }
         set
         {
            fEndDate = value;
            OnPropertyChanged("EndDate");
         }
      }

      #endregion
      
      #region Description

      /// <summary>
      /// Gets/sets the description of the calendar item.
      /// </summary>
      public string Description
      {
         get
         {
            return fDescription;
         }
         set
         {
            fDescription = value;
            OnPropertyChanged("Description");
         }
      }

      #endregion

      #region Recurring

      /// <summary>
      /// Gets/sets the if this calendar item should be recurring or not
      /// </summary>
      public bool Recurring
      {
         get
         {
            return fRecurring;
         }
         set
         {
            fRecurring = value;
            OnPropertyChanged("Recurring");
         }
      }

      #endregion
   }
}