using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace JaStDev.HAB
{
   class LinkInfoLock: LockObject
   {
      #region Fields
      LinkInfoLock fNext;
      #endregion

      #region Prop



      #region Next

      /// <summary>
      /// Gets or sets the next lock that is requested for this neuron, but which can't be
      /// merged with this one (writable locks are always for a single processor.
      /// </summary>
      public override LockObject Next
      {
         get { return fNext; }
         protected internal set
         {
            fNext = (LinkInfoLock)value;
         }
      }

      #endregion

      #endregion


      #region helpers

      /// <summary>
      /// Creates a new NeuronLock, adds it to the back of the list and returns a ref to it.
      /// </summary>
      /// <param name="level">The level.</param>
      /// <param name="writeable">if set to <c>true</c> [writeable].</param>
      /// <returns></returns>
      protected override LockObject AddNewLock(bool writeable)
      {
         LinkInfoLock iNew = new LinkInfoLock();
         iNew.IsWritable = writeable;
         iNew.Processors.Add(Thread.CurrentThread, 1);
         iNew.WaitHandle = LockManager.Current.GetWaitHandle();                                   //any locks added to the end of this list need to wait before they are released when it's prev one is done.
         LinkInfoLock iLast = (LinkInfoLock)Last;
         if (iLast != null)
            iLast.Next = iNew;
         else
            Next = iNew;
         return iNew;
      }

      #endregion
   }
}
