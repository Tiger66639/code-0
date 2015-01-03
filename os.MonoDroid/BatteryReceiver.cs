//-----------------------------------------------------------------------
// <copyright file="BatteryReceiver.cs">
//     Copyright (c) 2008-2012 Jan Bogaerts. All rights reserved.
// </copyright> 
// <authorJan Bogaerts</author>
// <email>Jan.Bogaerts@telenet.be</email>
// <date>29/04/2012</date>
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

namespace os.MonoDroid
{
   /// <summary>
   /// keeps trac of the battery status.
   /// </summary>
   public class BatteryReceiver : BroadcastReceiver
   {
      int fScale = -1;
      int fLevel = -1;
      double fVoltage = -1;
      double fTemp = -1;

      static BatteryReceiver fDefault = new BatteryReceiver();
      #region Default

      /// <summary>
      /// Gets the default object
      /// </summary>
      static public BatteryReceiver Default
      {
         get { return fDefault; }
      }

      #endregion

      #region Scale

      /// <summary>
      /// Gets the scale that should be applied to the level.
      /// </summary>
      public int Scale
      {
         get { return fScale; }
         internal set { fScale = value; }
      }

      #endregion
      
      #region Level

      /// <summary>
      /// Gets the level of the battery, using the scale specified in <see cref="BatteryReceiver.Scale"/>
      /// </summary>
      public int Level
      {
         get { return fLevel; }
         internal set { fLevel = value; }
      }

      #endregion
      
      #region Voltage

      /// <summary>
      /// Gets the voltage that the battery current has.
      /// </summary>
      public double Voltage
      {
         get { return fVoltage; }
         internal set { fVoltage = value; }
      }

      #endregion
      
      #region Temp

      /// <summary>
      /// Gets the temperature of the device/battery.
      /// </summary>
      public double Temp
      {
         get { return fTemp; }
         internal set { fTemp = value; }
      }

      #endregion

      private BatteryReceiver()
      {

      }

      /// <summary>
      /// The Intent filters used in <c><see cref="M:Android.Content.Context.RegisterReceiver(Android.Content.BroadcastReceiver, Android.Content.IntentFilter)"/></c>
      /// and in application manifests are <i>not</i> guaranteed to be exclusive.
      /// </summary>
      /// <param name="context">The Context in which the receiver is running.</param>
      /// <param name="intent">The Intent being received.</param>
      /// <since version="API Level 1"/>
      public override void OnReceive(Context context, Intent intent)
      {
         Level = intent.GetIntExtra(BatteryManager.ExtraLevel, -1);
         Scale = intent.GetIntExtra(BatteryManager.ExtraScale, -1);
         Temp = intent.GetIntExtra(BatteryManager.ExtraTemperature, -1) / 10.0;
         Voltage = intent.GetIntExtra(BatteryManager.ExtraVoltage, -1) / 10.0;
      }

      /// <summary>
      /// registers this receiver to the context so that 
      /// </summary>
      /// <param name="context"></param>
      public void Register(Context context)
      {
         IntentFilter iFilter = new IntentFilter(Intent.ActionBatteryChanged);
         context.RegisterReceiver(this, iFilter);
      }

      /// <summary>
      /// Stops monitoring battery status changes.
      /// </summary>
      /// <param name="context">The context.</param>
      public void Unregister(Context context)
      {
         context.UnregisterReceiver(this);
      }
   }
}