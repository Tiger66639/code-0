//-----------------------------------------------------------------------
// <copyright file="Calendar.cs">
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

namespace os.MonoDroid
{
   /// <summary>
   /// provides entry points to perform calendar operations.
   /// </summary>
   public class Calendar
   {
      /// <summary>
      /// Adds a new event to the calendar.
      /// </summary>
      /// <param name="title">The title of the event.</param>
      /// <param name="location">The location.</param>
      /// <param name="description">The description</param>
      /// <param name="date">when</param>
      /// <param name="recurring">is it recurring or not?</param>
      public static void AddEvent(string title, DateTime date, string location = null, string description = null, bool recurring = false)
      {
#if ANDROID
         CalendarAndroid.AddEvent(title, date, location, description, recurring);
#else
         throw new NotImplementedException();
#endif
      }


      /// <summary>
      /// Sets the title of the event we are currently building.
      /// </summary>
      /// <param name="title">The title.</param>
      public static void SetEventTitle(string title)
      {
#if ANDROID
         CalendarAndroid.SetEventTitle(title);
#else
         throw new NotImplementedException();
#endif
      }

      /// <summary>
      /// Sets the start date of the event we are currently building.
      /// </summary>
      /// <param name="date">The date.</param>
      public static void SetEventStartDate(DateTime date)
      {
#if ANDROID
         CalendarAndroid.SetEventStartDate(date);
#else
         throw new NotImplementedException();
#endif
      }

      /// <summary>
      /// Sets the end date of the event we are currently building.
      /// </summary>
      /// <param name="date">The date.</param>
      public static void SetEventEndDate(DateTime date)
      {
#if ANDROID
         CalendarAndroid.SetEventEndDate(date);
#else
         throw new NotImplementedException();
#endif
      }

      /// <summary>
      /// Sets the description of the event we are currently building.
      /// </summary>
      /// <param name="desc">The desc.</param>
      public static void SetEventDescription(string desc)
      {
#if ANDROID
         CalendarAndroid.SetEventDescription(desc);
#else
         throw new NotImplementedException();
#endif
      }

      /// <summary>
      /// Sets the location of the event we are currently building.
      /// </summary>
      /// <param name="loc">The loc.</param>
      public static void SetEventLocation(string loc)
      {
#if ANDROID
         CalendarAndroid.SetEventLocation(loc);
#else
         throw new NotImplementedException();
#endif
      }
   }
}