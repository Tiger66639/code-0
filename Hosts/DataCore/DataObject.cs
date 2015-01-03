//-----------------------------------------------------------------------
// <copyright file="DataObject.cs">
//     Copyright (c) 2008-2012 Jan Bogaerts. All rights reserved.
// </copyright> 
// <authorJan Bogaerts</author>
// <email>Jan.Bogaerts@telenet.be</email>
// <date>21/01/2012</date>
//-----------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DataCore
{
   #region PropertyChanged event types

   /// <summary>
   /// Event arguments for the PropertyChanged event.
   /// </summary>
   public class PropertyChangedEventArgs : EventArgs
   {
      public string PropertyName { get; set; }
   }

   /// <summary>
   /// delegate used for propertychanged events.
   /// </summary>
   /// <param name="sender"></param>
   /// <param name="e"></param>
   public delegate void PropertyChangedHandler(object sender, PropertyChangedEventArgs e);

   #endregion

   /// <summary>
   /// base class for android data objects.
   /// </summary>
   public class ObservableObject
   {

      /// <summary>
      /// Raised when the TextSin has got some text it wants to output.
      /// </summary>
      public event PropertyChangedHandler PropertyChanged;

      /// <summary>
      /// Called when a property was changed. raises the appropriate events.
      /// </summary>
      /// <param name="name">The name.</param>
      protected virtual void OnPropertyChanged(string name)
      {
         if (PropertyChanged != null)
            PropertyChanged(this, new PropertyChangedEventArgs() { PropertyName = name });
      }
   }
}