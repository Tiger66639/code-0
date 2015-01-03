//-----------------------------------------------------------------------
// <copyright file="ColorPickerPreference.cs">
//     Copyright (c) Dev\Bragi. All rights reserved.
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
using Android.Preferences;
using Android.Util;
using ColorPicker;
using Android.Graphics;

namespace AiciAndroid.Activities
{
   public class ColorPickerPreference: Preference
   {
      const string NAME = "Color picker";
      const string COLORPICKERNS = "http://ColorPicker.MonoDroid.com";

      int fColor;
      int fDefault;

      #region ctor
      /// <summary>
      /// Initializes a new instance of the <see cref="ColorPickerPreference"/> class.
      /// </summary>
      /// <param name="c">The c.</param>
      public ColorPickerPreference(Context c)
         : base(c)
      {
      }

      /// <summary>
      /// Initializes a new instance of the <see cref="ColorPickerPreference"/> class.
      /// </summary>
      /// <param name="context">The context.</param>
      /// <param name="attrs">The attrs.</param>
      public ColorPickerPreference(Context context, IAttributeSet attrs)
         : base(context, attrs)
      {
         InitPreference(context, attrs);
      }

      /// <summary>
      /// Initializes a new instance of the <see cref="ColorPickerPreference"/> class.
      /// </summary>
      /// <param name="context">The context.</param>
      /// <param name="attrs">The attrs.</param>
      /// <param name="defStyle">The def style.</param>
      public ColorPickerPreference(Context context, IAttributeSet attrs, int defStyle)
         : base(context, attrs, defStyle)
      {
         InitPreference(context, attrs);
      }

      void InitPreference(Android.Content.Context context, IAttributeSet attrs)
      {
         fDefault = attrs.GetAttributeIntValue(COLORPICKERNS, "default", (int)ConsoleColor.Black);
      } 
      #endregion

      /// <summary>
      /// Make sure to call through to the superclass's implementation.
      /// </summary>
      /// <param name="parent">The parent that this View will eventually be attached to.</param>
      /// <returns>
      ///   <list type="bullet">
      ///   <item>
      ///   <term>The View that displays this Preference.</term>
      ///   </item>
      ///   </list>
      /// </returns>
      /// <since version="API Level 1"/>
      protected override View OnCreateView(ViewGroup parent)
      {
         RelativeLayout layout = null;
         try
         {
            LayoutInflater mInflater = (LayoutInflater)Context.GetSystemService(Context.LayoutInflaterService);
            layout = (RelativeLayout)mInflater.Inflate(Resource.Layout.ColorPickerPref, parent, false);
         }
         catch (Exception e)
         {
            Log.Error(NAME, "Error creating color-picker preference", e);
         }
         return layout;
      }

      /// <summary>
      /// This may not always be called.
      /// </summary>
      /// <param name="restorePersistedValue">True to restore the persisted value;
      /// false to use the given <format type="text/html"><var>defaultValue</var></format>.</param>
      /// <param name="defaultValue">The default value for this Preference. Only use this
      /// if <format type="text/html"><var>restorePersistedValue</var></format> is false.</param>
      /// <since version="API Level 1"/>
      protected override void OnSetInitialValue(bool restorePersistedValue, Java.Lang.Object defaultValue)
      {
         if (restorePersistedValue)
         {
            fColor = GetPersistedInt(fDefault);
         }
         else
         {
            try
            {
               fColor = (int)defaultValue;
            }
            catch 
            {
               Log.Error(NAME, "Invalid default value: " + defaultValue.ToString());
            }
            PersistInt(fColor);
         }
      }

      /// <summary>
      /// For example, if the value type is String, the body of the method would
      /// proxy to <c><see cref="M:Android.Content.Res.TypedArray.GetString(System.Int32)"/></c>.
      /// </summary>
      /// <param name="a">The set of attributes.</param>
      /// <param name="index">The index of the default value attribute.</param>
      /// <returns>
      ///   <list type="bullet">
      ///   <item>
      ///   <term>The default value of this preference type.
      ///   </term>
      ///   </item>
      ///   </list>
      /// </returns>
      /// <since version="API Level 1"/>
      protected override Java.Lang.Object OnGetDefaultValue(Android.Content.Res.TypedArray a, int index)
      {
         return a.GetInt(index, fDefault);
      }

      /// <summary>
      /// Make sure to call through to the superclass's implementation.
      /// </summary>
      /// <param name="view">The View that shows this Preference.</param>
      /// <since version="API Level 1"/>
      protected override void OnBindView(View view)
      {
         base.OnBindView(view);
         UpdateView(view);
      }

      /// <summary>
      /// Processes a click on the preference.
      /// </summary>
      /// <since version="API Level 1"/>
      protected override void OnClick()
      {
         base.OnClick();
         ColorPickerDialog iDlg = new ColorPickerDialog(this.Context);
         iDlg.SetTitle(Title);
         iDlg.InitialColor = fColor;
         iDlg.DefaultColor = fDefault;
         iDlg.ColorChanged += new EventHandler<IntEventArgs>(ColorChanged);
         iDlg.Show();
      }

      /// <summary>
      /// Called when the color was selected.
      /// </summary>
      /// <param name="sender">The sender.</param>
      /// <param name="e">The <see cref="ColorPicker.IntEventArgs"/> instance containing the event data.</param>
      void ColorChanged(object sender, IntEventArgs e)
      {
         UpdateValue(e.Value);
      }

      /// <summary>
      /// Updates the value.
      /// </summary>
      /// <param name="value">The value.</param>
      public void UpdateValue(int value)
      {
         fColor = value;
         PersistInt(value);
         NotifyChanged();
      }

      /// <summary>
      /// Makes certain that the image is displayed.
      /// </summary>
      /// <param name="view">The view.</param>
      private void UpdateView(View view)
      {
         try
         {
            RelativeLayout layout = (RelativeLayout)view;
            LinearLayout iImage = layout.FindViewById<LinearLayout>(Resource.Id.SelectedColor);
            iImage.SetBackgroundColor(new Color(fColor));
         }
         catch (Exception e)
         {
            Log.Error(NAME, "Error updating color picker preference", e);
         }
      }
   }
}