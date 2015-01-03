//-----------------------------------------------------------------------
// <copyright file="AiciPreferencesActivity.cs">
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
using Android.Preferences;
using Java.Util;
using System.IO;
using ColorPicker;
using Android.Provider;
using Android.Database;
using AiciAndroid.Activities;

namespace AiciAndroid
{
   /// <summary>
   /// the activity that manages the user preferences.
   /// </summary>
   [Activity(Label = "Preferences")]
   public class AiciPreferencesActivity : PreferenceActivity
   {
      const int SELECTPICTURE = 1;
      ImagePickerPreference fImagePick;

      protected override void OnCreate(Bundle bundle)
      {
         try
         {
            base.OnCreate(bundle);

            PreferenceManager.SharedPreferencesName = AiciActivity.AICIPREF;
            AddPreferencesFromResource(Resource.Layout.prefs);

            //Preference button = (Preference)FindPreference("BackgroundColor");
            //button.PreferenceClick += new EventHandler<Preference.PreferenceClickEventArgs>(BackgroundColor_PreferenceClick);
            fImagePick = (ImagePickerPreference)FindPreference("BackgroundImage");
            fImagePick.PreferenceClick += new EventHandler<Preference.PreferenceClickEventArgs>(BackgroundImage_PreferenceClick);


            //button = (Preference)FindPreference("BotTextColor");
            //button.PreferenceClick += new EventHandler<Preference.PreferenceClickEventArgs>(BotTextColor_PreferenceClick);
            //button = (Preference)FindPreference("UserTextColor");
            //button.PreferenceClick += new EventHandler<Preference.PreferenceClickEventArgs>(UserTextColor_PreferenceClick);

            LoadFontValues();
            LoadBrainValues();
            LoadLangValues();
         }
         catch (Exception e)
         {
            Toast iToast = Toast.MakeText(this, "Something went serriously wrong while trying to show the preferences screen. " + e.Message, ToastLength.Short);
            iToast.Show();
         }
      }

      protected override void OnActivityResult(int requestCode, Result resultCode, Intent data)
      {
         if (resultCode == Result.Ok)
         {
            if (requestCode == SELECTPICTURE)
            {
               Android.Net.Uri selectedImageUri = data.Data;
               fImagePick.UpdateValue(getPath(selectedImageUri));
            }
         }

      }

      String getPath(Android.Net.Uri uri)
      {
         String[] projection = { "_data" };
         ICursor cursor = ManagedQuery(uri, projection, null, null, null);
         int column_index = cursor.GetColumnIndexOrThrow("_data");
         cursor.MoveToFirst();
         return cursor.GetString(column_index);
      }

      

      void BackgroundImage_PreferenceClick(object sender, Preference.PreferenceClickEventArgs e)
      {
         Intent i = new Intent(Intent.ActionPick, Android.Provider.MediaStore.Images.Media.ExternalContentUri);
         StartActivityForResult(i, SELECTPICTURE);
      }

      

      //void UserTextColor_PreferenceClick(object sender, Preference.PreferenceClickEventArgs e)
      //{
      //   ColorPickerDialog iDlg = GetColorDlg(AiciPrefs.Default.UserTextColor, e);
      //   iDlg.ColorChanged += new EventHandler<IntEventArgs>(UserText_ColorChanged);
      //   iDlg.Show();
      //}

      //void BotTextColor_PreferenceClick(object sender, Preference.PreferenceClickEventArgs e)
      //{
      //   ColorPickerDialog iDlg = GetColorDlg(AiciPrefs.Default.BotTextColor, e);
      //   iDlg.ColorChanged += new EventHandler<IntEventArgs>(BotText_ColorChanged);
      //   iDlg.Show();
      //}

      //void BackgroundColor_PreferenceClick(object sender, Preference.PreferenceClickEventArgs e)
      //{
      //   ColorPickerDialog iDlg = GetColorDlg(AiciPrefs.Default.BackgroundColor, e);
      //   iDlg.ColorChanged += new EventHandler<IntEventArgs>(Background_ColorChanged);
      //   iDlg.Show();
      //}

      //ColorPickerDialog GetColorDlg(int value, Preference.PreferenceClickEventArgs e)
      //{
      //   ColorPickerDialog iDlg = new ColorPickerDialog(this);
      //   iDlg.SetTitle(e.Preference.Title);
      //   iDlg.InitialColor = value;
      //   iDlg.DefaultColor = value;
      //   return iDlg;
      //}


      //void Background_ColorChanged(object sender, IntEventArgs e)
      //{
      //   AiciPrefs.Default.SetBackgroundColor(e.Value, this);                               //if we don't do this, the color doesn't appear saved while editing.
      //}

      //void UserText_ColorChanged(object sender, IntEventArgs e)
      //{
      //   AiciPrefs.Default.SetUserTextColor(e.Value, this);                               //if we don't do this, the color doesn't appear saved while editing.
      //}

      //void BotText_ColorChanged(object sender, IntEventArgs e)
      //{
      //   AiciPrefs.Default.SetBotTextColor(e.Value, this);                               //if we don't do this, the color doesn't appear saved while editing.
      //}


      /// <summary>
      /// loads all the languages and assigns it to the correct listpreference.
      /// </summary>
      private void LoadLangValues()
      {
         ListPreference iList = FindPreference(Resources.GetString(Resource.String.LanguagePrefKey)) as ListPreference;
         if (iList != null)
         {
            Bundle iBundle = Intent.Extras;
            iList.SetEntries(iBundle.GetStringArray("Lang_text"));
            iList.SetEntryValues(iBundle.GetStringArray("Lang_value"));
         }
      }

      /// <summary>
      /// loads all the available brains that were found in the system.
      /// </summary>
      private void LoadBrainValues()
      {
         ListPreference iList = FindPreference(Resources.GetString(Resource.String.BrainPrefKey)) as ListPreference;
         if (iList != null)
         {
            List<string> iText = new List<string>();
            List<string> iValues = new List<string>();
            iText.Add("Default");
            iValues.Add("");

            foreach (string i in Directory.GetFiles(Path.Combine(Android.OS.Environment.ExternalStorageDirectory.AbsolutePath, AiciActivity.AICIDIR), "*.dpl"))
            {
               iText.Add(Path.GetFileNameWithoutExtension(i));
               iValues.Add(Path.Combine(Path.GetDirectoryName(i), Path.GetFileNameWithoutExtension(i)));
            }
            iList.SetEntries(iText.ToArray());
            iList.SetEntryValues(iValues.ToArray());
         }
      }

      private void LoadFontValues()
      {
         ListPreference iList = FindPreference("BotFont") as ListPreference;
         ListPreference iList2 = FindPreference("UserFont") as ListPreference;
         if (iList != null && iList2 != null)
         {
            List<string> iText = new List<string>();
            iText.Add("Default");
            iText.Add("Monospace");
            iText.Add("Sans Serif");
            iText.Add("Serif");

            iList.SetEntries(iText.ToArray());
            iList2.SetEntries(iText.ToArray());
            iList.SetEntryValues(iText.ToArray());
            iList2.SetEntryValues(iText.ToArray());
         }
      }

      public override void OnBackPressed()
      {
         AiciPrefs.Default = new AiciPrefs(this);                             //the prefs have changed, we need to reload
         base.OnBackPressed();
      }

      
   }
}