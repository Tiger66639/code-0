//-----------------------------------------------------------------------
// <copyright file="ImagePickerPreference.cs">
//     Copyright (c) Dev\Bragi. All rights reserved.
// </copyright> 
// <authorJan Bogaerts</author>
// <email>Jan.Bogaerts@telenet.be</email>
// <date>10/05/2012</date>
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
using Android.Graphics;
using System.IO;


namespace AiciAndroid.Activities
{
   /// <summary>
   /// an activity to pick images.
   /// </summary>
   public class ImagePickerPreference: Preference
   {
      string fImage;
      const int THUMBNAIL_SIZE = 30;

      #region ctor
      /// <summary>
      /// Initializes a new instance of the <see cref="ImagePickerPreference"/> class.
      /// </summary>
      /// <param name="c">The c.</param>
      public ImagePickerPreference(Context c)
         : base(c)
      {

      }

      /// <summary>
      /// Initializes a new instance of the <see cref="ImagePickerPreference"/> class.
      /// </summary>
      /// <param name="context">The context.</param>
      /// <param name="attrs">The attrs.</param>
      public ImagePickerPreference(Context context, IAttributeSet attrs)
         : base(context, attrs)
      {

      }

      /// <summary>
      /// Initializes a new instance of the <see cref="ImagePickerPreference"/> class.
      /// </summary>
      /// <param name="context">The context.</param>
      /// <param name="attrs">The attrs.</param>
      /// <param name="defStyle">The def style.</param>
      public ImagePickerPreference(Context context, IAttributeSet attrs, int defStyle)
         : base(context, attrs, defStyle)
      {

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
            layout = (RelativeLayout)mInflater.Inflate(Resource.Layout.ImagePickerPref, parent, false);
         }
         catch (Exception e)
         {
            Log.Error("Image picker", "Error creating seek-Image-picker preference", e);
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
            fImage = GetPersistedString("");
         }
         else
         {
            try
            {
               fImage = ((Java.Lang.String)defaultValue).ToString();
            }
            catch (Exception ex)
            {
               Log.Error("Image picker", string.Format("Invalid default value: {0}. Error: {1}", defaultValue.ToString(), ex.ToString()));
            }
            PersistString(fImage);
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
         string iVal = a.GetString(index);
         return ValidateValue(iVal);
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

      public void UpdateValue(string value)
      {
         fImage = value;
         PersistString(value);
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
            ImageView iImage = layout.FindViewById<ImageView>(Resource.Id.SelectedImage);
            iImage.SetImageBitmap(GetPreview());
         }
         catch (Exception e)
         {
            Log.Error("Image picker", "Error updating image picker preference", e);
         }
      }

      Bitmap GetPreview()
      {
         BitmapFactory.Options bounds = new BitmapFactory.Options();
         bounds.InJustDecodeBounds = true;
         BitmapFactory.DecodeFile(fImage, bounds);
         if ((bounds.OutWidth == -1) || (bounds.OutHeight == -1))
            return null;

         int originalSize = (bounds.OutHeight > bounds.OutWidth) ? bounds.OutHeight : bounds.OutWidth;

         BitmapFactory.Options opts = new BitmapFactory.Options();
         opts.InSampleSize = originalSize / THUMBNAIL_SIZE;
         return BitmapFactory.DecodeFile(fImage, opts);
      }


      /// <summary>
      /// cheks of the image is valid.
      /// </summary>
      /// <param name="iVal">The i val.</param>
      /// <returns></returns>
      private Java.Lang.Object ValidateValue(string iVal)
      {
         if (File.Exists(iVal) == true)
            return new Java.Lang.String(iVal);
         else
            return new Java.Lang.String("");
      }
   }
}