//-----------------------------------------------------------------------
// <copyright file="orAnimatedImage.cs">
//     Copyright (c) 2008-2012 Jan Bogaerts. All rights reserved.
// </copyright> 
// <authorJan Bogaerts</author>
// <email>Jan.Bogaerts@telenet.be</email>
// <date>29/06/2011</date>
//-----------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Markup;
using System.Windows.Controls;
using System.Windows.Threading;
using System.Windows.Media.Imaging;
using System.Diagnostics;
using JaStDev.HAB.Characters;

namespace JaStDev.HAB.Designer.WPF.Controls
{
   /// <summary>
   /// An object that is able to play an animation through a series of images (bitmaps).
   /// Each bitmap is defined together with the amount of time it should remain visible.
   /// </summary>
   [ContentProperty("Frames")]
   public class AnimatedImage: Image
   {
      #region fields
      List<AnimatedImageFrame> fFrames = new List<AnimatedImageFrame>();
      DispatcherTimer fTimer;
      int fCurAnimPos = 0;
      bool fShowBackground;
      bool fAllowSpeech;
      bool fIsplayingReverse = false;                                                           //inidcates if we currently are playing in reverse or not. 
      bool fUseSoftStop = false;
      bool fIsStopping = false;                                                                 //indicates if we are in the stopping phase of the animation or not.
      double fTimeDistort = 1.0;                                                                //stores the quotient that needs to be applied on the time value during the stop animation period, so that the animation is sped up to 1 second max
      
      Random fRandom;                                                            //a randomizer, for if we are variable Jojo or need to vary the speed of the animation.
      AnimationLoopStyle fLoopStyle;
      AnimationEngineBase fEngine;

      #endregion

      /// <summary>
      /// Initializes a new instance of the <see cref="AnimatedImage"/> class.
      /// </summary>
      public AnimatedImage()
      {
         Loaded += new RoutedEventHandler(AnimatedImage_Loaded);
      }

      void AnimatedImage_Loaded(object sender, RoutedEventArgs e)
      {
         if (fFrames.Count > 0 && fTimer == null)                                                  //it could be that the user switched page, in which case we get a new load event, but we don't want to restart an already running timer, simply let that run.
         {
     
            fTimer = new DispatcherTimer(DispatcherPriority.Send);
            fEngine.Init(this);

            fTimer.Tick += new EventHandler(fTimer_Tick);
            fTimer.Start();
         }
      }

      /// <summary>
      /// Gets the randomizer that can be used by the engine.
      /// </summary>
      internal Random Randomizer
      {
         get
         {
            if(fRandom == null)
               fRandom = new Random();
            return fRandom;
         }
      }

      #region Frames

      /// <summary>
      /// Gets the list of frames that should be displayed for this  animated image.
      /// </summary>
      public List<AnimatedImageFrame> Frames
      {
         get { return fFrames; }
      }

      #endregion
      
      
      #region ShowBackground

      /// <summary>
      /// Gets/sets if the background, still, head is shown or not for this animation.
      /// </summary>
      public bool ShowBackground
      {
         get
         {
            return fShowBackground;
         }
         set
         {
            fShowBackground = value;
         }
      }

      #endregion

      #region UseSoftStop

      /// <summary>
      /// Gets/sets wether to use a hard stop (default) or a soft stop of the animation, in which it is fast forwarded to the 
      /// closest end (are we playing reverse or not).
      /// </summary>
      public bool UseSoftStop
      {
         get
         {
            return fUseSoftStop;
         }
         set
         {
            fUseSoftStop = value;
         }
      }

      #endregion

      #region AllowSpeech

      /// <summary>
      /// Gets/sets if the animation allows speech to occur or wether it should be terminated when speech occurs.
      /// </summary>
      public bool AllowSpeech
      {
         get
         {
            return fAllowSpeech;
         }
         set
         {
            fAllowSpeech = value;
         }
      }

      #endregion

      /// <summary>
      /// Gets or sets the max lower value that the speed can go down. This has to be a positive nr.
      /// </summary>
      /// <value>
      /// The max lower deviation.
      /// </value>
      public int MaxLowerDeviation { get; set; }

      /// <summary>
      /// Gets or sets the maximum value that the speed can go up. This has to be a positive nr.
      /// </summary>
      /// <value>
      /// The max upper deviation.
      /// </value>
      public int MaxUpperDeviation { get; set; }

      #region LoopStyle
      public AnimationLoopStyle LoopStyle
      {
         get
         {
            return fLoopStyle;
         }
         set
         {
            fLoopStyle = value;
            switch (value)
            {
               case AnimationLoopStyle.None:
                  fEngine = new SimpleAnimationEngine();
                  break;
               case AnimationLoopStyle.Jojo:
                  fEngine = new JojoAnimationEngine() { VariableSpeed = false };
                  break;
               case AnimationLoopStyle.VariableJojo:
                  fEngine = new JojoAnimationEngine() { VariableSpeed = true };
                  break;
               case AnimationLoopStyle.FrontToBack:
                  fEngine = new ForwardLoopAnimationEngine();
                  break;
               default:
                  break;
            }
         }
      } 
      #endregion

      /// <summary>
      /// Gets the timer used by the animation. This allows the engine to set the timer frequency.
      /// </summary>
      internal DispatcherTimer Timer
      {
         get { return fTimer; }
      }

      #region events
      /// <summary>
      /// Occurs when the animation has finished running and
      /// </summary>
      public event EventHandler AnimationFinished; 
      #endregion

      #region functions

      /// <summary>
      /// Stops the timer.
      /// </summary>
      public void Stop()
      {
         if (fTimer != null)
         {
            if (UseSoftStop == false)
               HardStop();
            else
            {
               fIsStopping = true;
               CalculateTotalTimeLeft();
            }
         }
      }

      /// <summary>
      /// Imidiatly stops the timer from playing.
      /// </summary>
      public void HardStop()
      {
         if (fTimer != null)                                                           //happens sometimes, not certain why, possibly out of sync events while reloading char.
         {
            fTimer.Stop();
            fTimer = null;
         }
         fTimeDistort = 1.0;
         fIsplayingReverse = false;
         fIsStopping = false;
         if (AnimationFinished != null)
            AnimationFinished(this, EventArgs.Empty);
      }

      /// <summary>
      /// Calculates the total time left to play in the animition to get to the start point. This is by the shortest
      /// route. It is used during the 'stop' part of the animation, so we can fast forward the animation
      /// Also stops and starts the timer again so that the animation can be sped up.
      /// </summary>
      private void CalculateTotalTimeLeft()
      {
         fTimer.Stop();                                                             //stop before calculating so taht this value can't change.
         long iTick = 0;
         for (int i = fCurAnimPos - 2; i >= 0; i--)                               //we do -2 cause current fCurAnimPos points to the next image to play, so -1 would give the current, but we want the prev.
            iTick += fFrames[i].Duration;
         if (iTick > 500)                                                          //if the current duration of the animation that's left takes longer then 0.8 second, speed it up.
            fTimeDistort = 500.0 / (double)iTick;
         else
            fTimeDistort = 1;
         fCurAnimPos -= 2;
         if (fCurAnimPos >= 0)
         {
            double iDuration = (double)fFrames[fCurAnimPos].Duration * fTimeDistort;      //calculate in doubles, most accuratie.
            fTimer.Interval = new TimeSpan(0, 0, 0, 0, (int)iDuration); //reset the timer, adjusted for the speed up
            fCurAnimPos--;
            fTimer.IsEnabled = true;
         }
         else
            HardStop();
      }

      void fTimer_Tick(object sender, EventArgs e)
      {
         fEngine.DoTimerTick(this);
         /*
         if (fIsplayingReverse == false && fIsStopping == false)
         {
            if (fCurAnimPos <= fLastFrame)
            {
               Source = fFrames[fCurAnimPos].Bitmap;
               AdvanceAnimation();
            }
            else if (LoopStyle == AnimationLoopStyle.Jojo || LoopStyle == AnimationLoopStyle.VariableJojo)
            {
               fIsplayingReverse = true;
               fCurAnimPos -= 2;                                                //go back 2, when last one was played, we went ahead 1, so remove that + the last one, cause we want to play the image in front of that.
               Source = fFrames[fCurAnimPos].Bitmap;
               ReverseAnimation();
            }
            else if (LoopStyle == AnimationLoopStyle.FrontToBack)
            {
               fCurAnimPos = 0;                                                  //we go back to the start of the animations.
               Source = fFrames[fCurAnimPos].Bitmap;
               AdvanceAnimation();
            }
            else
               HardStop();
         }
         else
         {
            if (fCurAnimPos >= 0)
            {
               Source = fFrames[fCurAnimPos].Bitmap;
               ReverseAnimation();

            }
            else if (fIsStopping == false)
            {
               if (LoopStyle == AnimationLoopStyle.Jojo)
               {
                  Source = fFrames[1].Bitmap;                                       //go back forward
                  AdvanceAnimation();
               }
               else if (LoopStyle == AnimationLoopStyle.VariableJojo)
               {
                  fLastFrame = Randomizer.Next(fFrames.Count);
                  Source = fFrames[1].Bitmap;                                       //go back forward
                  AdvanceAnimation();
               }
               else
                  HardStop();
            }
            else
               HardStop();
         }
          */ 
      }

      //private void ReverseAnimation()
      //{
      //   long iPassed = fStopWatch.ElapsedMilliseconds;
      //   fStopWatch.Restart();
      //   long iDiv = iPassed - fTimer.Interval.Milliseconds;
      //   double iDuration = (double)fFrames[fCurAnimPos].Duration * fTimeDistort;      //calculate in doubles, most accuratie. The distortion is for wehen we are stopping.
      //   while (fCurAnimPos >= 0 && iDuration < iDiv)                               //advance the animation for as long as the dif between actual and requested time is bigger then the time to the next frame. We do this to advance anim by skipping images so that we follow the correct time line.
      //   {
      //      iDiv -= (long)iDuration;
      //      fCurAnimPos--;
      //      if (fCurAnimPos >= 0)
      //         iDuration = (double)fFrames[fCurAnimPos].Duration * fTimeDistort;
      //   }

      //   if (fCurAnimPos >= 0)
      //   {
      //      fTimer.Interval = new TimeSpan(0, 0, 0, 0, (int)iDuration);                   //iduration is already calculated
      //      fCurAnimPos--;
      //   }
      //   else
      //      HardStop();
      //}

      ///// <summary>
      ///// Advances the animation 1 frame or more,depending on the exact time that passed + also sets the new time interval, so
      ///// we can keep track of left-over values.
      ///// </summary>
      //private void AdvanceAnimation()
      //{
      //   long iPassed = fStopWatch.ElapsedMilliseconds;
      //   fStopWatch.Restart();
      //   long iDiv = iPassed - fTimer.Interval.Milliseconds;

      //   while (fCurAnimPos < fFrames.Count && fFrames[fCurAnimPos].Duration < iDiv)                               //advance the animation for as long as the dif between actual and requested time is bigger then the time to the next frame. We do this to advance anim by skipping images so that we follow the correct time line.
      //   {
      //      iDiv -= fFrames[fCurAnimPos].Duration;
      //      fCurAnimPos++;
      //   }

      //   if (fCurAnimPos < fFrames.Count)
      //   {
      //      fTimer.Interval = new TimeSpan(0, 0, 0, 0, fFrames[fCurAnimPos].Duration - (int)iDiv);
      //      fCurAnimPos++;
      //   }
      //   else
      //      HardStop();
      //}


      #endregion

   }
}