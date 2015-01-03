//-----------------------------------------------------------------------
// <copyright file="IntEventArgs.cs">
//     Copyright (c) 2008-2012 Jan Bogaerts. All rights reserved.
// </copyright> 
// <authorJan Bogaerts</author>
// <email>Jan.Bogaerts@telenet.be</email>
// <date>03/06/2012</date>
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

namespace ColorPicker
{
   /// <summary>
   /// for passing along the color value back to the caller of the dialog.
   /// </summary>
   public class IntEventArgs : EventArgs
   {
      public int Value;
      public IntEventArgs(int i)
      {
         Value = i;
      }
   }
}