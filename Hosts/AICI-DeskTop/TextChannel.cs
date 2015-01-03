//-----------------------------------------------------------------------
// <copyright file="TextChannel.cs">
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
using JaStDev.Data;
using System.Collections.ObjectModel;
using JaStDev.HAB;

namespace AICI_DeskTop
{
   public class DialogItem: ObservableObject
   {
      /// <summary>
      /// Gets or sets who caused the dialog item to be added to the list, the user or the network.
      /// </summary>
      /// <value>The sender.</value>
      public string Originator { get; set; }

      /// <summary>
      /// Gets or sets the text that was typed/returned.
      /// </summary>
      /// <value>The text.</value>
      public string Text { get; set; }

      bool fNeedsScrollInView;
      #region NeedsScrollInView

      /// <summary>
      /// Gets/sets the wether this item needs to be scrolled into view.
      /// </summary>
      public bool NeedsScrollInView
      {
         get
         {
            return fNeedsScrollInView;
         }
         set
         {
            fNeedsScrollInView = value;
            OnPropertyChanged("NeedsScrollInView");
         }
      }

      #endregion
   }

   /// <summary>
   /// Provides access to a single text sin.
   /// </summary>
   public class TextChannel: ObservableObject
   {
      #region fields
      ObservableCollection<DialogItem> fChatLog = new ObservableCollection<DialogItem>();
      string fInputText = "Type here";
      TextSin fSin;
      string fDisplayTitle;

      public const string PCNAME = "PC";
      public const string YOUNAME = "You";

      #endregion

      public TextChannel(TextSin sin)
      {
         Sin = sin;
      }

      #region Prop
      #region ChatLog

      /// <summary>
      /// Gets the list of previous dialog statements.
      /// </summary>
      public ObservableCollection<DialogItem> ChatLog
      {
         get { return fChatLog; }
         internal set { fChatLog = value; }
      }

      #endregion


      #region InputText

      /// <summary>
      /// Gets/sets the text that the user has already entered, but has not yet sent to the network. This is stored
      /// here so that the window doesn't loose the data when it needs to repaint.
      /// </summary>
      public string InputText
      {
         get
         {
            return fInputText;
         }
         set
         {
            fInputText = value;
            OnPropertyChanged("InputText");
         }
      }

      #endregion  

      
      #region DisplayTitle

      /// <summary>
      /// Gets/sets the title to display in the tab header.
      /// </summary>
      public string DisplayTitle
      {
         get
         {
            return fDisplayTitle;
         }
         set
         {
            fDisplayTitle = value;
            OnPropertyChanged("DisplayTitle");
         }
      }

      #endregion
      #region Sin

      /// <summary>
      /// Gets the TextSin that this object provides access to.
      /// </summary>
      public TextSin Sin
      {
         get { return fSin; }
         private set 
         {
            if (fSin != null)
               fSin.TextOut -= fSin_TextOut;
            fSin = value;
            if (fSin != null)
               fSin.TextOut += fSin_TextOut;
         }
      }

      /// <summary>
      /// Handles the TextOut event of the fSin control. It simply displays the output value.
      /// </summary>
      /// <param name="sender">The source of the event.</param>
      /// <param name="e">The <see cref="JaStDev.HAB.OutputEventArgs&lt;System.String&gt;"/> instance containing the event data.</param>
      void fSin_TextOut(object sender, OutputEventArgs<string> e)
      {
         DialogItem iNew = new DialogItem();
         iNew.Originator = PCNAME;
         iNew.Text = e.Value;
         App.Current.Dispatcher.BeginInvoke(new Action<DialogItem>(AddDialogItem), iNew);
      }

      void AddDialogItem(DialogItem item)
      {
         ChatLog.Add(item);
         item.NeedsScrollInView = true;
      }

      #endregion
      #endregion

      /// <summary>
      /// Sends the input text to the sin + logs the text.
      /// </summary>
      public void SendInputText()
      {
         Brain.Current.LoadCodeAt((ulong)PredefinedNeurons.ContainsWord);                                //a little speed optimisation: we preload the code so that the first salvo also runs fast.
         Brain.Current.LoadCodeAt(1365);                                                                 //1365 = patternmatch finished, so we have some more code preloaded.
         Processor iProc = ProcessorFactory.GetProcessor();                   //don't just create a processor, that will fail. it needs to be set up, so use GetProcessor.
         Sin.Process(InputText, iProc, TextSinProcessMode.ClusterAndDict);
         DialogItem iNew = new DialogItem();
         iNew.Originator = YOUNAME;
         iNew.Text = InputText;
         ChatLog.Add(iNew);
         InputText = null;
      }
   }
}