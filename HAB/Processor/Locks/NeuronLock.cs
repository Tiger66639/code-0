using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace JaStDev.HAB
{

   /// <summary>
   /// Enum that specifies the different levels of locking that can be requested for a neuron.
   /// </summary>
   public enum LockLevel
   {
      /// <summary>
      /// Nothing on the neuron is locked.
      /// </summary>
      None,
      /// <summary>
      /// The list of incomming links.
      /// </summary>
      LinksIn,
      /// <summary>
      /// The list of outgoing links.
      /// </summary>
      LinksOut,
      /// <summary>
      /// The children list of a neuron.
      /// </summary>
      Children,
      /// <summary>
      /// The ClusteredBy list of a neuron.
      /// </summary>
      Parents,
      /// <summary>
      /// for value neurons and the meaning of a cluster.
      /// </summary>
      Value,
      /// <summary>
      /// the set of processors to which the neuron is attached.
      /// </summary>
      Processors,
      /// <summary>
      /// when the entire neuron needs to be/is locked.
      /// </summary>
      All
   }

   /// <summary>
   /// contains all the information for a lock on a single neuron, as used by the
   /// <see cref="LockManager"/>.
   /// Note: all operations performed on this object should be wrapped with a lock on
   /// the object itself, like the <see cref="LockManager"/> does.
   /// </summary>
   class NeuronLock: LockObject
   {
      #region Fields
      NeuronLock fNext;
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
            fNext = (NeuronLock)value;
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
         NeuronLock iNew = new NeuronLock();
         iNew.IsWritable = writeable;
         iNew.Processors.Add(Thread.CurrentThread, 1);
         iNew.WaitHandle = LockManager.Current.GetWaitHandle();                                   //any locks added to the end of this list need to wait before they are released when it's prev one is done.
         NeuronLock iLast = (NeuronLock)Last;
         if (iLast != null)
            iLast.Next = iNew;
         else
            Next = iNew;
         return iNew;
      }

      #endregion
   }
}
