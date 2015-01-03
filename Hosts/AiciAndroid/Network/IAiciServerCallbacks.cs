//-----------------------------------------------------------------------
// <copyright file="IAiciServerCallbacks.cs">
//     Copyright (c) 2008-2012 Jan Bogaerts. All rights reserved.
// </copyright> 
// <authorJan Bogaerts</author>
// <email>Jan.Bogaerts@telenet.be</email>
// <date>04/03/2012</date>
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

namespace AiciAndroid.Network
{
   /// <summary>
   /// This interface should be implemented by everybody that can talk to the <see cref="AiciServer"/> so that it know to whom it needs
   /// to send data back.
   /// </summary>
   public interface IAiciServerCallbacks
   {
      /// <summary>
      /// Called when there is output text that needs to be processed. This is not called from the UI thread.
      /// </summary>
      /// <param name="item"></param>
      void TextOut(UILogItem item);

      /// <summary>
      /// called when the server has some buffered incomming text that still needs to be synced with the UI. This happens when there was voice input
      /// from a subwindow.
      /// </summary>
      /// <param name="item"></param>
      void TextIn(UILogItem item);

      /// <summary>
      /// Called when the actity needs to ask the user to select 1 item from the list. When the user has made his selection,
      /// the index of the selected item is returned as a child of the list, all the other values should be removed.
      /// </summary>
      /// <param name="list">The list.</param>
      void FilterInput(List<int> list);

      /// <summary>
      /// called when a list of possible inputs was sent to the engine and 1 final result got found. This can be used to update
      /// the ui and or train the STT.
      /// </summary>
      /// <param name="index"></param>
      void SelectInput(int index);


      /// <summary>
      /// called when a list of possible inputs was sent to the engine and 1 final result got found. This can be used to update
      /// the ui and or train the STT.
      /// For calls from threads other than the ui.
      /// </summary>
      /// <param name="index"></param>
      void SelectInputAsync(int index);

      /// <summary>
      /// Sets the network activity.
      /// </summary>
      /// <param name="NetworkActive">if set to <c>true</c> [network active].</param>
      void SetNetworkActivity(bool NetworkActive);
   }
}