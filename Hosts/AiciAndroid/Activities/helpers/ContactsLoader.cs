//-----------------------------------------------------------------------
// <copyright file="ContactsLoader.cs">
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
using os.MonoDroid;
using JaStDev.HAB;

namespace AiciAndroid
{
   class ContactsLoader: IProcessorFactory
   {

      ProgressDialog fProgressDlg;
      IProcessorFactory fPrevFactory;                                //so we can restore this value after the contacts were loaded. need to overwrite it cause we need to supply our own factory for progress reporting.
	  static ContactsLoader fLoader;

      #region inner types

      /// <summary>
      /// we overwrite the processor class so that we can update the progress based on the stack content.
      /// </summary>
      class ReportingProcessor: Processor
      {
         ProgressDialog fProgressDlg;
         Activity fContext;

         /// <summary>
         /// Initializes a new instance of the <see cref="ReportingProcessor"/> class.
         /// </summary>
         /// <param name="fProgressDlg">The f progress DLG.</param>
         public ReportingProcessor(ProgressDialog progressDlg, Activity context)
         {
            fProgressDlg = progressDlg;
            fContext = context;
         }


         /// <summary>
         /// Tries to solve all the <see cref="Neuron"/>s currently on the stack untill the stack is empty, untill there is only 1 neuron
         /// left that can't be solved anymore or untill <see cref="Process.Exit"/> is called.
         /// </summary>
         public override void Solve()
         {
			fContext.RunOnUiThread(() =>
			{
            	fProgressDlg.Max = this.NeuronStack.Count;
			});
            base.Solve();
         }

         protected override void OnNeuronProcessed(Neuron toSolve, NeuronCluster cluster)
         {
            fContext.RunOnUiThread(() =>
            {
               fProgressDlg.Progress += 1;
            });
            base.OnNeuronProcessed(toSolve, cluster);
         }
      }

      #endregion

      private Activity fContext;
      /// <summary>
      /// Loads all the contacts in the network, using dialog boxes.
      /// </summary>
      /// <param name="context"></param>
      internal static void Load (Activity context, string title = null)
	  {
		 if (fLoader == null) 													//could be that the loader is already running.
		 {
			fLoader = new ContactsLoader ();
			fLoader.fContext = context;
		 	fLoader.AskUserPermission (title);
		 }
	  }

      private void AskUserPermission(string title)
      {
         AlertDialog iDlg = null;
         AlertDialog.Builder iAlert = new AlertDialog.Builder(fContext);
         if (string.IsNullOrEmpty(title) == true)
            iAlert.SetTitle("First run");
         else
            iAlert.SetTitle(title);
         iAlert.SetMessage("Would you like to import your contact names?");
         iAlert.SetPositiveButton("Ok", delegate
         {
            LoadContacts(); 
            iDlg.Dismiss();
           });
         iAlert.SetNegativeButton("Cancel", delegate
         {
            fLoader = null;
            iDlg.Dismiss();
         });
         iDlg = iAlert.Create ();
         iDlg.Show();
      }

      private void LoadContacts()
      {
         fProgressDlg = new ProgressDialog(fContext);

         fProgressDlg.SetTitle("In progress");
         fProgressDlg.SetMessage("Importing contacts...");
	      fProgressDlg.SetProgressStyle(ProgressDialogStyle.Horizontal);
         fProgressDlg.Show();
         fPrevFactory = ProcessorFactory.Factory;
         ProcessorFactory.Factory = this;
         ThreadManager.Default.ActivityStopped += Network_ActivityStopped;                   //the starting processer doesn't need to run untill finish, need to trap that in a different way.
         ReportingProcessor iProc = new ReportingProcessor(fProgressDlg, fContext);
		   iProc.Mem = new MemoryFactory();													//need to make certain that there is memory avalabile for the proc
         AiciContacts.LoadContactsInfo(iProc);
      }



      /// <summary>
      /// Handles the ActivityStopped event of the Network control.
      /// </summary>
      /// <param name="sender">The source of the event.</param>
      /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
      void Network_ActivityStopped(object sender, EventArgs e)
      {
         fContext.RunOnUiThread(() =>
         {
            fProgressDlg.Progress = fProgressDlg.Max;
            fProgressDlg.Dismiss();
			fLoader = null;
         });
         ThreadManager.Default.ActivityStopped -= Network_ActivityStopped;
         ProcessorFactory.Factory = fPrevFactory;
      }

      #region IProcessorFactory Members

      /// <summary>
      /// Gets the processor. 
      /// We need to provide every processor with a handlet to the progress dialog otherwise we can't report the entire progess.
      /// </summary>
      /// <returns></returns>
      public Processor CreateProcessor()
      {
         return new ReportingProcessor(fProgressDlg, fContext);
      }

      public void ActivateProc(Processor proc)
      {
      }

      #endregion

   }
}