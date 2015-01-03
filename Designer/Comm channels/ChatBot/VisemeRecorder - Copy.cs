//-----------------------------------------------------------------------
// <copyright file="VisemeRecorder - Copy.cs">
//     Copyright (c) 2008-2012 Jan Bogaerts. All rights reserved.
// </copyright> 
// <authorJan Bogaerts</author>
// <email>Jan.Bogaerts@telenet.be</email>
// <date>08/08/2011</date>
//-----------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Speech.Synthesis;
using System.Windows.Threading;
using System.Diagnostics;

namespace JaStDev.HAB.Designer
{
   class VisemeRecorder : PlayerTrack
   {
      ChatBotChannel fChannel;
      AudioNote fRecordTo;                                                                //currently recording to this note.
      TimeSpan fPrevVisemePos;
      TimeSpan fPrevBookmarkPos;                                                          //keeps trakc of the position of the last bookmark event.
      VisemeNote fPrevNote;                                                               //to adjust for bad voices that return don't but the correct time between multiple visemes
      bool? fCanPlay = false;                                                             //used to indicate if we can start playing when data is rendered, or we need to wait for the idle animation to finish. it's a ? so we can have false (still rendering), null (rendered, but waiting), true (playing can begin)
      object fCanPlayLock = new object();                                                 //fCanPlay is accessed and modified by multiple threads, need to sync this.

      Stopwatch fTestTimer = new Stopwatch();


      public VisemeRecorder(ChatBotChannel channel)
      {
         fChannel = channel;
      }


      /// <summary>
      /// Called when a viseme has been reached for this track. Inheriters can implement this to play the viseme.
      /// </summary>
      /// <param name="value">The value.</param>
      protected override void OnVisemeReached(int value)
      {
         if (fChannel.SelectedCharacter != null)
            App.Current.Dispatcher.BeginInvoke(DispatcherPriority.Send, new Action<int>(fChannel.SelectedCharacter.SelectViseme), value);

         Debug.Print(fTestTimer.Elapsed.Ticks.ToString());
         fTestTimer.Restart();
      }

      /// <summary>
      /// Called when a bookmark has been reached.Inheriters can implement this to play the bookmark.
      /// </summary>
      /// <param name="value">The value.</param>
      protected override void OnBookmarkReached(string value)
      {
         if (fChannel.SelectedCharacter != null)
            App.Current.Dispatcher.BeginInvoke(DispatcherPriority.Send, new Action<string>(fChannel.SelectedCharacter.ActivateAnimation), value);
      }

      /// <summary>
      /// Starts playing this track.
      /// </summary>
      protected override void StartPlaying()
      {
         fTestTimer.Start();
         fChannel.IsSpeaking = true;                                                      //when speach really starts, we let the channel know.
         base.StartPlaying();
      }

      /// <summary>
      /// Called when this track stops playing (all the viseme events have passed, audio has stopped).
      /// </summary>
      protected override void StopPlaying()
      {
         base.StopPlaying();
         fChannel.IsSpeaking = false;
         fChannel.ResetIdleTimer();
         fTestTimer.Stop();
      }


      private void CloseRecording()
      {
         RegisterNote(fRecordTo);                                                   //this needs to be done outside of the fCanPlay lock, cause it also has a lock, which would potentially create problems.
         fRecordTo = null;
         lock (fCanPlayLock)
            fCanPlay = false;
      }

      /// <summary>
      /// Handles the SpeakStarted event of the fSpeaker control.
      /// Need to start/create the viseme player.
      /// </summary>
      /// <param name="sender">The source of the event.</param>
      /// <param name="e">The <see cref="System.Speech.Synthesis.SpeakStartedEventArgs"/> instance containing the event data.</param>
      internal void fSpeaker_SpeakStarted(object sender, SpeakStartedEventArgs e)
      {
         Debug.Assert(fRecordTo != null);                                              //can't start recording if there is no recording device specified (needs to be done in the channel).
         fPrevNote = null;
         fPrevBookmarkPos = new TimeSpan(0);
      }


      /// <summary>
      /// Handles the VisemeReached event of the fSpeaker control.
      /// </summary>
      /// <param name="sender">The source of the event.</param>
      /// <param name="e">The <see cref="System.Speech.Synthesis.VisemeReachedEventArgs"/> instance containing the event data.</param>
      internal void fSpeaker_VisemeReached(object sender, VisemeReachedEventArgs e)
      {
         if (fPrevNote != null || e.Viseme != 0)                                          //if the first note is the 0 viseme (init), skip it, cause this only causes a delay
         {
            VisemeNote iNew = new VisemeNote();
            iNew.Duration = e.Duration.Ticks;
            iNew.Viseme = e.Viseme;
            iNew.NextViseme = e.NextViseme;
            if (fPrevNote != null)
            {
               //if (e.AudioPosition == fPrevVisemePos)                                        //some voices give bad times for some viseme events, they have the same starting time for multiple viseme events, this is captured and we use the 'duration' in this case to estimate the length of the viseme and adjust the time accordingly.
              // {
               //   iNew.Ticks = fPrevNote.Duration;
               //   fPrevVisemePos.Add(new TimeSpan(fPrevNote.Duration));                      //we advance the previous time so it becomes accumlutive. We need to do this cause otherwise we get a time stretch (need to lower the start of the next item)
              // }
              // else
              // {
                  TimeSpan iDiv = e.AudioPosition - fPrevVisemePos;
                  iNew.Ticks = iDiv.Ticks;
                  fPrevVisemePos = e.AudioPosition;
               //}
            }
            else
            {
               iNew.Ticks = e.AudioPosition.Ticks;
               fPrevVisemePos = e.AudioPosition;
            }
            fPrevNote = iNew;
            fRecordTo.Visemes.Enqueue(iNew);
            Debug.Print("{0}, {1}, {2}, {3}, {4}, {5}", e.Viseme, e.AudioPosition.Ticks, e.Duration.Ticks, e.NextViseme, iNew.Ticks, iNew.Duration);
         }
      }


      /// <summary>
      /// Handles the SpeakCompleted event of the fSpeaker control.
      /// </summary>
      /// <param name="sender">The source of the event.</param>
      /// <param name="e">The <see cref="System.Speech.Synthesis.SpeakCompletedEventArgs"/> instance containing the event data.</param>
      internal void fSpeaker_SpeakCompleted(object sender, SpeakCompletedEventArgs e)
      {
         bool iVal = true;
         fChannel.SpeechCompleted(fRecordTo);
         lock (fCanPlayLock)
         {
            if (fCanPlay.HasValue == true && fCanPlay == false)
            {
               fCanPlay = null;
               iVal = fCanPlay.Value;
            }
         }
         if (iVal == true)
            CloseRecording();
      }


      /// <summary>
      /// Handles the BookmarkReached event of the fSpeaker control.
      /// </summary>
      /// <param name="sender">The source of the event.</param>
      /// <param name="e">The <see cref="System.Speech.Synthesis.BookmarkReachedEventArgs"/> instance containing the event data.</param>
      internal void fSpeaker_BookmarkReached(object sender, BookmarkReachedEventArgs e)
      {
         BookmarkEvent iNew = new BookmarkEvent();
         iNew.Value = e.Bookmark;
         TimeSpan iTimeDiv = e.AudioPosition - fPrevBookmarkPos;                    //get the time difference between the prev event and the new one (can be 0) 
         iNew.Ticks = iTimeDiv.Milliseconds;
         fRecordTo.Marks.Enqueue(iNew);
         fPrevBookmarkPos = e.AudioPosition;
      }

      /// <summary>
      /// Starts the recording of events. This 
      /// </summary>
      internal AudioNote StartRecording()
      {
         if (IsRecording == true)
            throw new InvalidOperationException("Still recording a previous dataset");
         fRecordTo = GetFreeNote();
         return fRecordTo;
      }

      /// <summary>
      /// Gets a value indicating whether this instance is recording.
      /// </summary>
      /// <value>
      /// 	<c>true</c> if this instance is recording; otherwise, <c>false</c>.
      /// </value>
      public bool IsRecording 
      {
         get { return fRecordTo != null; }
      }

      /// <summary>
      /// Determines whether this instance can play.
      /// </summary>
      internal void CanPlay()
      {
         bool iPlay = false;
         lock (fCanPlayLock)
         {
            if (fCanPlay.HasValue == true && fCanPlay.Value == false)
               fCanPlay = true;
            else if (fCanPlay.HasValue == false)                                    //recording has already finished.
               iPlay = true;
         }
         if (iPlay == true)
            CloseRecording();
      }
   }
}