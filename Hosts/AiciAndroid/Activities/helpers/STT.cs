//-----------------------------------------------------------------------
// <copyright file="STT.cs">
//     Copyright (c) 2008-2012 Jan Bogaerts. All rights reserved.
// </copyright> 
// <authorJan Bogaerts</author>
// <email>Jan.Bogaerts@telenet.be</email>
// <date>05/03/2012</date>
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
using Android.Content.PM;
using Android.Speech;
using Java.Util;
using AiciAndroid.Network;

namespace AiciAndroid
{
   public class STT
   {
      public const int HANDLESTTINPUT = 2;
      string fLang;                                                        //stores the language id to use for the speech to text.
      bool fHasSTT = false;
      bool fPrefFilter = false;                                            //determines if we ask the user to select the correct input before accessing the network or if we want the network to solve it.
      int fMaxNrVariations = 8;
      IList<string> fCurrentInputs;                                        //so we can keep track of all the current inputs from the mic so that we can ask the user to make a selection if the system couldn't figure it all out.
      Activity fContext;
      List<int> fIndexesToSelect;                                          //keeps track of the indexes of the currentInput that still need to be filtered further (after the network already filtered out as much as it could).

      /// <summary>
      /// Initializes a new instance of the <see cref="STT"/> class.
      /// </summary>
      /// <param name="context">The context.</param>
      public STT(Activity context)
      {
         // Check to see if a recognition activity is present
         fContext = context;
         var activities = context.PackageManager.QueryIntentActivities(new Intent(RecognizerIntent.ActionRecognizeSpeech), 0);
         fHasSTT = activities.Count != 0;
         ISharedPreferences iSettings = context.GetSharedPreferences(AiciActivity.AICIPREF, FileCreationMode.Private);
         fMaxNrVariations = iSettings.GetInt(context.Resources.GetString(Resource.String.InputVarCountPrefKey), 8);
         fLang = iSettings.GetString(context.Resources.GetString(Resource.String.LanguagePrefKey), "en-US");
         fPrefFilter = iSettings.GetBoolean("PreFilterInput", true);
      }
      
      /// <summary>
      /// Gets a value indicating whether this instance has STT.
      /// </summary>
      /// <value>
      ///   <c>true</c> if this instance has STT; otherwise, <c>false</c>.
      /// </value>
      public bool HasSTT
      {
         get { return fHasSTT; }
      }

      /// <summary>
      /// Gets the list of inptuts currently being processed.
      /// </summary>
      public IList<string> CurrentInputs
      {
         get { return fCurrentInputs; }
         set 
         {
            if (value != fCurrentInputs)
            {
               if (value != null)
               {
                  if (string.IsNullOrEmpty(InputContext) == false)
                  {
                     for (int i = 0; i < fCurrentInputs.Count; i++)
                        fCurrentInputs[i] = string.Format("{0} {1}", InputContext, fCurrentInputs[i]);      //add the inputcontext in front of all the inputs, to make certain that it is always used.
                  }
               }
               else
                  InputContext = null;
               fCurrentInputs = value;
            }
         }
      }



      /// <summary>
      /// tries to start stt engine for voice input
      /// </summary>
      /// <param name="caller"></param>
      public void TryStartSTT(Activity caller)
      {
         if (fHasSTT)
         {
            //speakButton.setOnClickListener(this);

            Intent intent = new Intent(RecognizerIntent.ActionRecognizeSpeech);
            intent.PutExtra(RecognizerIntent.ExtraCallingPackage, caller.CallingPackage);                       // Specify the calling package to identify your application
            intent.PutExtra(RecognizerIntent.ExtraPrompt, "Talk");                        // Display an hint to the user about what he should say.
            intent.PutExtra(RecognizerIntent.ExtraLanguageModel, RecognizerIntent.LanguageModelFreeForm); // Given an hint to the recognizer about what the user is going to say
            intent.PutExtra(RecognizerIntent.ExtraMaxResults, fMaxNrVariations);                                        // Specify how many results you want to receive. The results will be sorted where the first result is the one with higher confidence.
            intent.PutExtra(RecognizerIntent.ExtraLanguage, fLang);

            Locale iPrefered = GetPreferedLanguage(caller);
            if (iPrefered != null)
               intent.PutExtra(RecognizerIntent.ExtraLanguage, iPrefered.Language);

            caller.StartActivityForResult(intent, HANDLESTTINPUT);
         }
      }

      /// <summary>
      /// Gets the prefered language and if not yet defined, stores the language currently selected by the system as the default language.
      /// </summary>
      /// <param name="?">The ?.</param>
      /// <returns></returns>
      internal Locale GetPreferedLanguage(Activity caller)
      {
         Locale iRes = null;
         ISharedPreferences iSettings = caller.GetSharedPreferences(AiciActivity.AICIPREF, FileCreationMode.Private);
         if (iSettings.Contains(caller.Resources.GetString(Resource.String.LanguagePrefKey)))
            iRes = new Locale(iSettings.GetString(caller.Resources.GetString(Resource.String.LanguagePrefKey), "eng-USA"));
         return iRes;
      }

      /// <summary>
      /// Gets/sets the text that should be inserted in front of all the STT inputs in order to create a proper context.
      /// for instance: 'call', 'email',...
      /// </summary>
      public string InputContext { get; set; }



      /// <summary>
      /// Processes the input.
      /// </summary>
      /// <param name="iList">The i list.</param>
      /// <param name="fServer">The f server.</param>
      internal void ProcessInput(IList<string> iList)
      {
         CurrentInputs = iList;
         if (fPrefFilter == false)
            AiciActivity.Server.Process(iList);
         else
            ShowSelectionDialog(iList);
      }

      internal void SelectInput(int index)
      {
         AiciActivity.Server.LogTextIn(CurrentInputs[index]);                                                                     //also need to add it to the log.
         if (fPrefFilter == false)
            AiciActivity.Server.PutFilteredInputResult(index);
         else
            AiciActivity.Server.Process(CurrentInputs[index]);
         CurrentInputs = null;                                                                 //selection has been made, clear this so that it is empty even if there is no output at all.
      }

      void ShowSelectionDialog(IList<string> list)
      {
         AlertDialog.Builder iBuilder = new AlertDialog.Builder(fContext);
         iBuilder.SetTitle("Filter input");
         iBuilder.SetItems(list.ToArray(), new EventHandler<DialogClickEventArgs>(OnSelectInputItem));
         AlertDialog iDlg = iBuilder.Create();
         iDlg.Show();
      }

      void OnSelectInputItem(object sender, DialogClickEventArgs e)
      {
         if (fIndexesToSelect != null)                                                 //check if we need to get an index nr out of the list that still needs filtering, or if we still need to start the pattern matching process.
         {
            SelectInput(fIndexesToSelect[e.Which]);
            fIndexesToSelect = null;
         }
         else
            SelectInput(e.Which);
      }

      /// <summary>
      /// Ask the user to further filter the list of inputs down to 1 item. The network has already done some work, but couldn't filter them all out.
      /// </summary>
      /// <param name="values">The values.</param>
      /// <param name="list">The list of indexes into the 'currentInputs' list that still needs work.</param>
      internal void FilterInput(List<string> values, List<int> list)
      {
         fIndexesToSelect = list;
         ShowSelectionDialog(values);
      }
   }
}