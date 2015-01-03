//-----------------------------------------------------------------------
// <copyright file="IntEditTextPreference.cs">
//     Copyright (c) Dev\Bragi. All rights reserved.
// </copyright> 
// <authorJan Bogaerts</author>
// <email>Jan.Bogaerts@telenet.be</email>
// <date>20/12/2011</date>
//-----------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Android.Preferences;
using Android.Content;
using Android.Util;


namespace Common
{
   /// <summary>
   /// extends the EditTextPreference so that is saves to an int.
   /// </summary>
   public class IntEditTextPreference : EditTextPreference 
   {
      #region ctor
      /// <summary>
      /// Initializes a new instance of the <see cref="IntEditTextPreference"/> class.
      /// </summary>
      /// <param name="context">The context.</param>
      public IntEditTextPreference(Context context)
         : base(context)
      {
      }

      /// <summary>
      /// Initializes a new instance of the <see cref="IntEditTextPreference"/> class.
      /// </summary>
      /// <param name="context">The context.</param>
      /// <param name="attrs">The attrs.</param>
      public IntEditTextPreference(Context context, IAttributeSet attrs)
         : base(context, attrs)
      {
      }

      /// <summary>
      /// Initializes a new instance of the <see cref="IntEditTextPreference"/> class.
      /// </summary>
      /// <param name="context">The context.</param>
      /// <param name="attrs">The attrs.</param>
      /// <param name="defStyle">The def style.</param>
      public IntEditTextPreference(Context context, IAttributeSet attrs, int defStyle)
         : base(context, attrs, defStyle)
      {

      } 
      #endregion


      /// <summary>
      /// Gets the persisted string.
      /// </summary>
      /// <param name="defaultReturnValue">The default return value.</param>
      /// <returns></returns>
      protected override String GetPersistedString(string defaultReturnValue)
      {
         return GetPersistedInt(-1).ToString();
      }

      /// <summary>
      /// Persists the string.
      /// </summary>
      /// <param name="value">The value.</param>
      /// <returns></returns>
      protected override bool PersistString(string value)
      {
         return PersistInt(int.Parse(value));
      }

   }
}