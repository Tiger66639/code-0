//-----------------------------------------------------------------------
// <copyright file="CalendarAndroid.cs">
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
using JaStDev.HAB;

namespace os.MonoDroid
{
   /// <summary>
   /// Provides calendar support.
   /// </summary>
   public class CalendarAndroid
   {
      /// <summary>
      /// The calendar item currently being edited.
      /// </summary>
      static CalendarItem fCurrentItem;

      /// <summary>
      /// Gets the calendar item currently being worked on, so that the engine can add things to it or save it.
      /// </summary>
      public static CalendarItem CurrentItem
      {
         get { return fCurrentItem; }
      }

      /// <summary>
      /// When the item has been stored, it should be reset so that the info doesn't get used again.
      /// </summary>
      static public void ResetCurrentItem()
      {
         fCurrentItem = null;
      }

      /// <summary>
      /// Adds a new event to the calendar.
      /// </summary>
      /// <param name="title">The title of the event.</param>
      /// <param name="location">The location.</param>
      /// <param name="description">The description</param>
      /// <param name="date">when</param>
      /// <param name="recurring">is it recurring or not?</param>
      internal static void AddEvent(string title, DateTime date, string location = null, string description = null, bool recurring = false)
      {
         IAndroidService iService = ReflectionSin.Context as IAndroidService;
         fCurrentItem = new CalendarItem()
         {
            StartDate = date,
            Title = title,
            Location = location,
            Description = description,
            Recurring = recurring
         };
         iService.ShowAddCalendarItem(fCurrentItem);
      }


      /// <summary>
      /// Sets the title of the event we are currently building.
      /// </summary>
      /// <param name="title">The title.</param>
      internal static void SetEventTitle(string title)
      {
         if (fCurrentItem == null)
            fCurrentItem = new CalendarItem();
         fCurrentItem.Title = title;
      }

      /// <summary>
      /// Sets the start date of the event we are currently building.
      /// </summary>
      /// <param name="date">The date.</param>
      internal static void SetEventStartDate(DateTime date)
      {
         if (fCurrentItem == null)
            fCurrentItem = new CalendarItem();
         fCurrentItem.StartDate = date;
      }

      /// <summary>
      /// Sets the end date of the event we are currently building.
      /// </summary>
      /// <param name="date">The date.</param>
      internal static void SetEventEndDate(DateTime date)
      {
         if (fCurrentItem == null)
            fCurrentItem = new CalendarItem();
         fCurrentItem.EndDate = date;
      }


      /// <summary>
      /// Sets the description of the event we are currently building.
      /// </summary>
      /// <param name="desc">The desc.</param>
      internal static void SetEventDescription(string desc)
      {
         if (fCurrentItem == null)
            fCurrentItem = new CalendarItem();
         fCurrentItem.Description = desc;
      }


      /// <summary>
      /// Sets the location of the event we are currently building.
      /// </summary>
      /// <param name="loc">The loc.</param>
      internal static void SetEventLocation(string loc)
      {
         if (fCurrentItem == null)
            fCurrentItem = new CalendarItem();
         fCurrentItem.Location= loc;
      }
   }
}