//-----------------------------------------------------------------------
// <copyright file="AiciPrefs.cs">
//     Copyright (c) 2008-2012 Jan Bogaerts. All rights reserved.
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
using Java.Util;
using Android.Graphics;

namespace AiciAndroid
{
   class AiciPrefs
   {
      const string INSTALLEDVERSION = "InstalledVersion";

      static AiciPrefs fDefault;
      Locale fPreferedLanguage;
      bool fPreferShowTextInput;
      string fAppId;
      bool fLogToServer;
      bool fTTSActivated;
      string fBotFont;
      string fUserFont;
      int fBotTextColor;
      int fUserTextColor;
      int fBotFontSize;
      int fUserFontSize;
      int fBackgroundColor;
      string fBackgroundImage;
      bool fIsFirstRun = false;

      /// <summary>
      /// Initializes a new instance of the <see cref="AiciPrefs"/> class.
      /// </summary>
      /// <param name="owner">The owner.</param>
      public AiciPrefs(Context owner)
      {
         ISharedPreferences iSettings = owner.GetSharedPreferences(AiciActivity.AICIPREF, FileCreationMode.Private);

         fPreferedLanguage = new Locale(iSettings.GetString(owner.Resources.GetString(Resource.String.LanguagePrefKey), "eng-USA"));
         fPreferShowTextInput = iSettings.GetBoolean("AlwaysShowTextInput", true);
         fAppId = iSettings.GetString("AppId", null);
         if (fAppId == null)
            BuildAppId(iSettings);
         fLogToServer = iSettings.GetBoolean("LogToServer", true);
         fTTSActivated = iSettings.GetBoolean("TTSActivated", true);
         fBotFont = iSettings.GetString("BotFont", "Default");
         fUserFont = iSettings.GetString("UserFont", "Default");
         fBotTextColor = iSettings.GetInt("BotTextColor", Color.LightGreen);
         fUserTextColor = iSettings.GetInt("UserTextColor", Color.LightBlue);
         fBackgroundColor = iSettings.GetInt("BackgroundColor", Color.Black);
         fBackgroundImage = iSettings.GetString("BackgroundImage", "");
         fBotFontSize = iSettings.GetInt("BotFontSize", 14);
         fUserFontSize = iSettings.GetInt("UserFontSize", 14);



         int iInstalledVer = iSettings.GetInt(INSTALLEDVERSION, -1);
         int iNewVer = owner.PackageManager.GetPackageInfo(owner.ApplicationInfo.PackageName, 0).VersionCode;
         fIsFirstRun = iInstalledVer < iNewVer;
      }

      private void BuildAppId(ISharedPreferences iSettings)
      {
         fAppId = Guid.NewGuid().ToString();

         ISharedPreferencesEditor iEditor = iSettings.Edit();
         iEditor.PutString("AppId", fAppId);
         iEditor.Commit();           // Commit the edits!
      }

      #region Default
      /// <summary>
      /// Gets or sets the default preferences object.
      /// </summary>
      /// <value>
      /// The default.
      /// </value>
      static public AiciPrefs Default
      {
         get
         {
            return fDefault;
         }
         set
         {
            fDefault = value;
         }
      } 
      #endregion


      #region IsFirstRun
      /// <summary>
      /// Gets if this is the first time that the network was started or not.
      /// </summary>
      public bool IsFirstRun
      {
         get { return fIsFirstRun; }
      } 
      
      public void ResetFirstRun(Context owner)
      {
         int iNewVer = owner.PackageManager.GetPackageInfo(owner.ApplicationInfo.PackageName, 0).VersionCode;
         ISharedPreferences iSettings = owner.GetSharedPreferences(AiciActivity.AICIPREF, FileCreationMode.Private);
      	 ISharedPreferencesEditor iEditor = iSettings.Edit();
         iEditor.PutInt(INSTALLEDVERSION, iNewVer);                                             //also store the version of the newly installed package.
         iEditor.Commit();
      }
      
      #endregion

      #region PreferedLanguage

      /// <summary>
      /// Gets/sets the prefered language.
      /// </summary>
      public Locale PreferedLanguage
      {
         get
         {
            return fPreferedLanguage;
         }
         set
         {
            fPreferedLanguage = value;
         }
      }

      #endregion

      #region PreferShowTextInput

      /// <summary>
      /// Gets/sets the value that indicates if the textinput box needs to be shown by default or not.
      /// </summary>
      public bool PreferShowTextInput
      {
         get
         {
            return fPreferShowTextInput;
         }
         set
         {
            fPreferShowTextInput = value;
         }
      }

      #endregion

      #region TTSActivated

      /// <summary>
      /// Gets the value that indicates if TTS should be used or not.
      /// </summary>
      public bool TTSActivated
      {
         get { return fTTSActivated; }
      }


      public void SetTTSActivated(bool value, Context owner)
      {
         if (fTTSActivated != value)
         {
            fTTSActivated = value;
            ISharedPreferences iSettings = owner.GetSharedPreferences(AiciActivity.AICIPREF, FileCreationMode.Private);
            ISharedPreferencesEditor iEditor = iSettings.Edit();
            iEditor.PutBoolean("TTSActivated", fTTSActivated);
            iEditor.Commit();           // Commit the edits!
         }
      }

      #endregion

      #region AppId

      /// <summary>
      /// Gets/sets the id of hte application for server logging.
      /// </summary>
      public string AppId
      {
         get
         {
            return fAppId;
         }
         set
         {
            fAppId = value;
         }
      }

      #endregion

      #region LogToServer

      /// <summary>
      /// Gets/sets the value that indicates if chatlogs can be logged to the server (for future improvements).
      /// </summary>
      public bool LogToServer
      {
         get
         {
            return fLogToServer;
         }
         set
         {
            fLogToServer = value;
         }
      }

      #endregion


      
      #region BackgroundColor

      /// <summary>
      /// Gets the background color of the log view.
      /// </summary>
      public int BackgroundColor
      {
         get
         {
            return fBackgroundColor;
         }
      }

      public void SetBackgroundColor(int value, Context owner)
      {
         if (fBackgroundColor != value)
         {
            fBackgroundColor = value;
            ISharedPreferences iSettings = owner.GetSharedPreferences(AiciActivity.AICIPREF, FileCreationMode.Private);
            ISharedPreferencesEditor iEditor = iSettings.Edit();
            iEditor.PutInt("BackgroundColor", fBackgroundColor);
            iEditor.Commit();           // Commit the edits!
         }
      }

      #endregion

      #region BackgroundImage

      /// <summary>
      /// Gets the location of the background image to use.
      /// </summary>
      public string BackgroundImage
      {
         get { return fBackgroundImage; }
      }

      /// <summary>
      /// sets the default background image 
      /// </summary>
      /// <param name="value"></param>
      /// <param name="owner"></param>
      public void SetBackgroundImage(string value, Context owner)
      {
         if (fBackgroundImage != value)
         {
            fBackgroundImage = value;
            ISharedPreferences iSettings = owner.GetSharedPreferences(AiciActivity.AICIPREF, FileCreationMode.Private);
            ISharedPreferencesEditor iEditor = iSettings.Edit();
            iEditor.PutString("BackgroundImage", fBackgroundImage);
            iEditor.Commit();           // Commit the edits!
         }
      }

      #endregion

      #region BotFont

      /// <summary>
      /// Gets the name of the font to use for the bot(this is just to pick one of the standard fonts).
      /// </summary>
      public string BotFont
      {
         get { return fBotFont; }
         internal set { fBotFont = value; }
      }

      #endregion

      
      #region UserFont

      /// <summary>
      /// Gets/sets the font to use for the user's text.
      /// </summary>
      public string UserFont
      {
         get
         {
            return fUserFont;
         }
         private set
         {
            fUserFont = value;
         }
      }

      #endregion

      
      #region BotTextColor

      /// <summary>
      /// Gets the color of the text for the bot.
      /// </summary>
      public int BotTextColor
      {
         get { return fBotTextColor; }
      }
      public void SetBotTextColor(int value, Context owner)
      {
         if (fBotTextColor != value)
         {
            fBotTextColor = value;
            ISharedPreferences iSettings = owner.GetSharedPreferences(AiciActivity.AICIPREF, FileCreationMode.Private);
            ISharedPreferencesEditor iEditor = iSettings.Edit();
            iEditor.PutInt("BotTextColor", fBotTextColor);
            iEditor.Commit();           // Commit the edits!
         }
      }


      #endregion     

      #region UserTextColor

      /// <summary>
      /// Gets the color of the text for the user.
      /// </summary>
      public int UserTextColor
      {
         get { return fUserTextColor; }
      }

      public void SetUserTextColor(int value, Context owner)
      {
         if (fUserTextColor != value)
         {
            fUserTextColor = value;
            ISharedPreferences iSettings = owner.GetSharedPreferences(AiciActivity.AICIPREF, FileCreationMode.Private);
            ISharedPreferencesEditor iEditor = iSettings.Edit();
            iEditor.PutInt("UserTextColor", fUserTextColor);
            iEditor.Commit();           // Commit the edits!
         }
      }


      #endregion

      
      #region BotFontSize

      /// <summary>
      /// Gets the size of the font for all the textviews (default is 14 sp). Always expressed in sp
      /// </summary>
      public int BotFontSize
      {
         get { return fBotFontSize; }
         internal set { fBotFontSize = value; }
      }

      #endregion

      
      #region UserFontSize

      /// <summary>
      /// Gets/sets the size of the font for the user.
      /// </summary>
      public int UserFontSize
      {
         get
         {
            return fUserFontSize;
         }
         internal set
         {
            fUserFontSize = value;
         }
      }

      #endregion
   }
}