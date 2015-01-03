//-----------------------------------------------------------------------
// <copyright file="ColorPickerDialog.cs">
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
using Android.OS;
using Android.Views;
using Android.Content;
using Android.Graphics;

namespace ColorPicker
{
   /// <summary>
   /// a dialog to select the color.
   /// </summary>
   public class ColorPickerDialog: Dialog
   {
      #region internal types

      class ColorPickerView: View
      {
         ColorPickerDialog fDlg;
         private Paint mPaint;
		   float mCurrentHue = 0;
		   int mCurrentX = 0, mCurrentY = 0;
		   int mCurrentColor;
		   int[] mHueBarColors = new int[258];
   		int[] mMainColors = new int[65536];

         public ColorPickerView(Context c, ColorPickerDialog dlg): base(c)
         {
            fDlg = dlg;

            // Get the current hue from the current color and update the main color field
            float[] hsv = new float[3];
            ColorToHSV(fDlg.InitialColor, hsv);
            mCurrentHue = hsv[0];
            updateMainColors();

            mCurrentColor = fDlg.InitialColor;

            // Initialize the colors of the hue slider bar
            int index = 0;
            for (float i = 0; i < 256; i += 256 / 42) // Red (#f00) to pink (#f0f)
            {
               mHueBarColors[index] = Color.Rgb(255, 0, (int)i);
               index++;
            }
            for (float i = 0; i < 256; i += 256 / 42) // Pink (#f0f) to blue (#00f)
            {
               mHueBarColors[index] = Color.Rgb(255 - (int)i, 0, 255);
               index++;
            }
            for (float i = 0; i < 256; i += 256 / 42) // Blue (#00f) to light blue (#0ff)
            {
               mHueBarColors[index] = Color.Rgb(0, (int)i, 255);
               index++;
            }
            for (float i = 0; i < 256; i += 256 / 42) // Light blue (#0ff) to green (#0f0)
            {
               mHueBarColors[index] = Color.Rgb(0, 255, 255 - (int)i);
               index++;
            }
            for (float i = 0; i < 256; i += 256 / 42) // Green (#0f0) to yellow (#ff0)
            {
               mHueBarColors[index] = Color.Rgb((int)i, 255, 0);
               index++;
            }
            for (float i = 0; i < 256; i += 256 / 42) // Yellow (#ff0) to red (#f00)
            {
               mHueBarColors[index] = Color.Rgb(255, 255 - (int)i, 0);
               index++;
            }

            // Initializes the Paint that will draw the View
            mPaint = new Paint(PaintFlags.AntiAlias);
            mPaint.TextAlign = Paint.Align.Center;
            mPaint.TextSize = 12;
         }


         // Update the main field colors depending on the current selected hue
         private void updateMainColors()
         {
            int mainColor = GetCurrentMainColor();
            int index = 0;
            int[] topColors = new int[256];
            for (int y = 0; y < 256; y++)
            {
               for (int x = 0; x < 256; x++)
               {
                  if (y == 0)
                  {
                     mMainColors[index] = Color.Rgb(255 - (255 - Color.GetRedComponent(mainColor)) * x / 255, 255 - (255 - Color.GetGreenComponent(mainColor)) * x / 255, 255 - (255 - Color.GetBlueComponent(mainColor)) * x / 255);
                     topColors[x] = mMainColors[index];
                  }
                  else
                     mMainColors[index] = Color.Rgb((255 - y) * Color.GetRedComponent(topColors[x]) / 255, (255 - y) * Color.GetGreenComponent(topColors[x]) / 255, (255 - y) * Color.GetBlueComponent(topColors[x]) / 255);
                  index++;
               }
            }
         }

         // Get the current selected color from the hue bar
         private int GetCurrentMainColor()
         {
            int translatedHue = 255 - (int)(mCurrentHue * 255 / 360);
            int index = 0;
            for (float i = 0; i < 256; i += 256 / 42)
            {
               if (index == translatedHue)
                  return Color.Rgb(255, 0, (int)i);
               index++;
            }
            for (float i = 0; i < 256; i += 256 / 42)
            {
               if (index == translatedHue)
                  return Color.Rgb(255 - (int)i, 0, 255);
               index++;
            }
            for (float i = 0; i < 256; i += 256 / 42)
            {
               if (index == translatedHue)
                  return Color.Rgb(0, (int)i, 255);
               index++;
            }
            for (float i = 0; i < 256; i += 256 / 42)
            {
               if (index == translatedHue)
                  return Color.Rgb(0, 255, 255 - (int)i);
               index++;
            }
            for (float i = 0; i < 256; i += 256 / 42)
            {
               if (index == translatedHue)
                  return Color.Rgb((int)i, 255, 0);
               index++;
            }
            for (float i = 0; i < 256; i += 256 / 42)
            {
               if (index == translatedHue)
                  return Color.Rgb(255, 255 - (int)i, 0);
               index++;
            }
            return Color.Red;
         }

         static void ColorToHSV(int color, float[] res)
         {
            int[] iColors = new int[3];
            iColors[0] = Color.GetGreenComponent(color);
            iColors[1] = Color.GetBlueComponent(color);
            iColors[2] = Color.GetRedComponent(color);

            float iMin = iColors.Min();
            float iMax = iColors.Max();
            float iDelta = iMax - iMin;

            res[2] = iMax;
            if (iMax != 0)
               res[1] = iDelta / iMax;
            else
            {
               res[0] = 0;
               res[1] = -1;
               return;
            }
            if (iColors[2] == iMax)
               res[0] = (iColors[0] - iColors[1]) / iDelta;
            else if (iColors[0] == iMax)
               res[0] = 2 + (iColors[1] - iColors[2]) / iDelta;
            else
               res[0] = 4 + (iColors[2] - iColors[0]) / iDelta;

            res[0] *= 60;
            if (res[0] < 0)
               res[0] += 360;
         }

         /// <summary>
         /// Measure the view and its content to determine the measured width and the
         /// measured height.
         /// </summary>
         /// <param name="widthMeasureSpec">horizontal space requirements as imposed by the parent.
         /// The requirements are encoded with
         /// <c><see cref="T:Android.Views.View+MeasureSpec"/></c>.</param>
         /// <param name="heightMeasureSpec">vertical space requirements as imposed by the parent.
         /// The requirements are encoded with
         /// <c><see cref="T:Android.Views.View+MeasureSpec"/></c>.</param>
         /// <since version="API Level 1"/>
         protected override void OnMeasure(int widthMeasureSpec, int heightMeasureSpec)
         {
            SetMeasuredDimension(276, 366);
         }

         /// <summary>
         /// Implement this to do your drawing.
         /// </summary>
         /// <param name="canvas">the canvas on which the background will be drawn</param>
         /// <since version="API Level 1"/>
         protected override void OnDraw(Canvas canvas)
         {
            int translatedHue = 255-(int)(mCurrentHue*255/360);
			   // Display all the colors of the hue bar with lines
			   for (int x=0; x<256; x++)
			   {
				   // If this is not the current selected hue, display the actual color
				   if (translatedHue != x)
				   {
                  mPaint.Color = new Color(mHueBarColors[x]);
					   mPaint.StrokeWidth =1;
				   }
				   else // else display a slightly larger black line
				   {
					   mPaint.Color = Color.Black;
					   mPaint.StrokeWidth = 3;
				   }
				   canvas.DrawLine(x+10, 0, x+10, 40, mPaint);
			   }

			   // Display the main field colors using LinearGradient
			   for (int x=0; x<256; x++)
			   {
				   int[] colors = new int[2];
				   colors[0] = mMainColors[x];
				   colors[1] = Color.Black;
				   Shader shader = new LinearGradient(0, 50, 0, 306, colors, null, Shader.TileMode.Repeat);
				   mPaint.SetShader(shader);
				   canvas.DrawLine(x+10, 50, x+10, 306, mPaint);
			   }
			   mPaint.SetShader(null);

			   // Display the circle around the currently selected color in the main field
			   if (mCurrentX != 0 && mCurrentY != 0)
			   {
				   mPaint.SetStyle(Paint.Style.Stroke);
				   mPaint.Color = Color.Black;
				   canvas.DrawCircle(mCurrentX, mCurrentY, 10, mPaint);
			   }

			   // Draw a 'button' with the currently selected color
			   mPaint.SetStyle(Paint.Style.Fill);
            mPaint.Color = new Color(mCurrentColor);
			   canvas.DrawRect(10, 316, 138, 356, mPaint);

			   // Set the text color according to the brightness of the color
			   if (Color.GetRedComponent(mCurrentColor)+Color.GetGreenComponent(mCurrentColor)+Color.GetBlueComponent(mCurrentColor) < 384)
				   mPaint.Color = Color.White;
			   else
				   mPaint.Color = Color.Black;
			   canvas.DrawText("Tap here to confirm", 74, 340, mPaint);

			   // Draw a 'button' with the default color
			   mPaint.SetStyle(Paint.Style.Fill);
            mPaint.Color = new Color(fDlg.DefaultColor);
			   canvas.DrawRect(138, 316, 266, 356, mPaint);

			   // Set the text color according to the brightness of the color
            if (Color.GetRedComponent(fDlg.DefaultColor) + Color.GetGreenComponent(fDlg.DefaultColor) + Color.GetBlueComponent(fDlg.DefaultColor) < 384)
				   mPaint.Color = Color.White;
			   else
				   mPaint.Color = Color.Black;
			   canvas.DrawText("Default color", 202, 340, mPaint);
            }


         /// <summary>
         /// Implement this method to handle touch screen motion events.
         /// </summary>
         /// <param name="e">The motion event.</param>
         /// <returns>
         ///   <list type="bullet">
         ///   <item>
         ///   <term>True if the event was handled, false otherwise.
         ///   </term>
         ///   </item>
         ///   </list>
         /// </returns>
         /// <since version="API Level 1"/>
         public override bool OnTouchEvent(MotionEvent e)
         {
            if (e.Action != MotionEventActions.Down  ) return true;
            float x = e.GetX();
            float y = e.GetY();

			   // If the touch event is located in the hue bar
			   if (x > 10 && x < 266 && y > 0 && y < 40)
			   {
				   // Update the main field colors
				   mCurrentHue = (255-x)*360/255;
				   updateMainColors();

				   // Update the current selected color
				   int transX = mCurrentX-10;
				   int transY = mCurrentY-60;
				   int index = 256*(transY-1)+transX;
				   if (index > 0 && index < mMainColors.Length)
					   mCurrentColor = mMainColors[256*(transY-1)+transX];

				   // Force the redraw of the dialog
               Invalidate();
			   }

			   // If the touch event is located in the main field
			   if (x > 10 && x < 266 && y > 50 && y < 306)
			   {
				   mCurrentX = (int) x;
				   mCurrentY = (int) y;
				   int transX = mCurrentX-10;
				   int transY = mCurrentY-60;
				   int index = 256*(transY-1)+transX;
				   if (index > 0 && index < mMainColors.Length)
				   {
					   // Update the current color
					   mCurrentColor = mMainColors[index];
					   // Force the redraw of the dialog
					   Invalidate();
				   }
			   }

			// If the touch event is located in the left button, notify the listener with the current color
			if (x > 10 && x < 138 && y > 316 && y < 356)
				fDlg.OnColorChanged(mCurrentColor);

			// If the touch event is located in the right button, notify the listener with the default color
			if (x > 138 && x < 266 && y > 316 && y < 356)
            fDlg.OnColorChanged(fDlg.DefaultColor);

			return true;
         }
      }

      #endregion

      int fInitialColor;
      int fDefaultColor;

      public ColorPickerDialog(Context c): base(c)
      {
            
      }

      public event EventHandler<IntEventArgs> ColorChanged;

      #region InitialColor

      /// <summary>
      /// Gets/sets the value for the initial color
      /// </summary>
      public int InitialColor
      {
         get
         {
            return fInitialColor;
         }
         set
         {
            fInitialColor = value;
         }
      }

      #endregion

      
      #region DefaultColor

      /// <summary>
      /// Gets/sets the value for the default color.
      /// </summary>
      public int DefaultColor
      {
         get
         {
            return fDefaultColor;
         }
         set
         {
            fDefaultColor = value;
         }
      }

      #endregion

     

      /// <summary>
      /// Similar to <c><see cref="M:Android.App.Activity.OnCreate(Android.OS.Bundle)"/></c>, you should initialize your dialog
      /// in this method, including calling <c><see cref="M:Android.App.Dialog.SetContentView(Android.Views.View)"/></c>.
      /// </summary>
      /// <param name="savedInstanceState">If this dialog is being reinitalized after a
      /// the hosting activity was previously shut down, holds the result from
      /// the most recent call to <c><see cref="M:Android.App.Dialog.OnSaveInstanceState"/></c>, or null if this
      /// is the first time.</param>
      /// <since version="API Level 1"/>
      protected override void OnCreate(Bundle savedInstanceState)
      {
         base.OnCreate(savedInstanceState);
         SetContentView(new ColorPickerView(Context, this));
      }

      internal void OnColorChanged(int value)
      {
         if (ColorChanged != null)
            ColorChanged(this, new IntEventArgs(value));
         Dismiss();
      }
   }
}