//-----------------------------------------------------------------------
// <copyright file="LogAdapter.cs">
//     Copyright (c) 2008-2012 Jan Bogaerts. All rights reserved.
// </copyright> 
// <authorJan Bogaerts</author>
// <email>Jan.Bogaerts@telenet.be</email>
// <date>15/05/2012</date>
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
using Android.Graphics;

namespace AiciAndroid
{
   /// <summary>
   /// provides custom view management for UILogItem values.
   /// </summary>
   class LogAdapter: ArrayAdapter<UILogItem>
   {
      public LogAdapter(Context context, int textViewResourceId ): base(context, textViewResourceId)
      {
      }

      public LogAdapter(Context context, int textViewResourceId, IList<UILogItem> objects)
         : base(context, textViewResourceId, objects)
      {
      }

      /// <summary>
      ///   <format type="text/html">
      ///   <a href="http://developer.android.com/reference/android/widget/ArrayAdapter.html#getView(int, android.view.View, android.view.ViewGroup)">[Android Documentation]</a>
      ///   </format>
      /// </summary>
      /// <param name="position">To be added.</param>
      /// <param name="convertView">To be added.</param>
      /// <param name="parent">To be added.</param>
      /// <returns>
      /// To be added.
      /// </returns>
      /// <since version="API Level 1"/>
      public override View GetView(int position, View convertView, ViewGroup parent)
      {
         View iView = convertView;
         if (iView == null)
         {
            LayoutInflater iInflater = (LayoutInflater)Context.GetSystemService(Context.LayoutInflaterService);
            iView = iInflater.Inflate(Resource.Layout.Log_List_Item, null);
         }
         UILogItem iItem = GetItem(position);
         if (iItem != null)
         {
            TextView tt = (TextView)iView.FindViewById(Resource.Id.Log_List_Item);
            if (tt != null)
            {
               if (iItem.Source == SourceType.Bot)
               {
                  tt.SetTextColor(new Color(AiciPrefs.Default.BotTextColor));
                  SetFont(tt, AiciPrefs.Default.BotFont);
                  tt.SetTextSize(Android.Util.ComplexUnitType.Sp, AiciPrefs.Default.BotFontSize);
               }
               else
               {
                  tt.SetTextColor(new Color(AiciPrefs.Default.UserTextColor));
                  SetFont(tt, AiciPrefs.Default.UserFont);
                  tt.SetTextSize(Android.Util.ComplexUnitType.Sp, AiciPrefs.Default.UserFontSize);
               }
               tt.Text = iItem.Text;
            }
         }
         return iView;
      }

      private void SetFont(TextView view, string font)
      {
         if (AiciPrefs.Default.BotFont == "Default")
            view.Typeface = Typeface.Default;
         else if (AiciPrefs.Default.BotFont == "Monospace")
            view.Typeface = Typeface.Monospace;
         else if (AiciPrefs.Default.BotFont == "Sans Serif")
            view.Typeface = Typeface.SansSerif;
         else if (AiciPrefs.Default.BotFont == "Serif")
            view.Typeface = Typeface.Serif;
      }

      /// <summary>
      /// Adds the specified value to the list.
      /// </summary>
      /// <param name="value">The value.</param>
      public void AddLogItem(UILogItem value)
      {
         Add(value);
         NotifyDataSetChanged();
      }
   }
}