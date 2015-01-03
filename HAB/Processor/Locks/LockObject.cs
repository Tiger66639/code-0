using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace JaStDev.HAB
{
   /// <summary>
   /// Base class for lock objects like the <see cref="NeuronLock"/> and the <see cref="LinkInfoLock"/>
   /// </summary>
   abstract class LockObject
   {

      #region Fields

      Dictionary<Thread, int> fProcessors = new Dictionary<Thread, int>();
      ManualResetEvent fWaitHandle;

      #endregion


      #region Prop

      #region IsWritable
      /// <summary>
      /// Gets or sets a value indicating whether the lock allows for writes or not. This is
      /// important to determin if a lock can be shared by multiple threads or not.
      /// </summary>
      /// <value>
      /// 	<c>true</c> if this instance allows writes to the neuron; otherwise, <c>false</c>.
      /// </value>
      public bool IsWritable { get; set; }
      #endregion

      #region Processors

      /// <summary>
      /// Gets the list of all the processors that share this lock + which parts they have locked.  This allows
      /// us to calculate the entire lock, even after processors were removed and added again.
      /// </summary>
      public Dictionary<Thread, int> Processors
      {
         get { return fProcessors; }
      }

      #endregion

      #region WaitHandle

      /// <summary>
      /// Gets the object to use for waiting untill this thread can be used.
      /// </summary>
      /// <remarks>
      /// By default, there isn't any, only when a merge failed do we need to create a waitHandle.
      /// </remarks>
      internal ManualResetEvent WaitHandle
      {
         get { return fWaitHandle; }
         set
         {
            if (value != fWaitHandle)
            {
               if (value == null)
                  LockManager.Current.ReleaseWaitHandle(fWaitHandle);                  //we release the lock again once we are done.
               fWaitHandle = value;
            }
         }
      }

      #endregion 

      #region Next
      /// <summary>
      /// Gets or sets the next lock that is requested for this object, but which can't be
      /// merged with this one (writable locks are always for a single processor.
      /// </summary>
      public abstract LockObject Next { get; protected internal set; } 
      #endregion


      #region Last
      /// <summary>
      /// Gets or sets the last item in the list of requested locks. This is not thread safe.
      /// </summary>
      /// <value>The last.</value>
      public LockObject Last
      {
         get
         {
            LockObject iLast = Next;
            if (iLast != null)
            {
               while (iLast.Next != null)
                  iLast = iLast.Next;
            }
            return iLast;
         }
      } 
      #endregion

      #endregion

      #region functions

      protected abstract LockObject AddNewLock(bool writeable);

      /// <summary>
      /// Will try to add the current processor with the specified lock level and write mode, to 
      /// this lock. If this is not possible, a new lock object is created, which can only be 
      /// used once the 
      /// </summary>
      /// <param name="level">The level.</param>
      /// <param name="writeable">if set to <c>true</c> the lock should be writable..</param>
      /// <returns>Either this lock, or the newly created one.</returns>
      internal LockObject Merge(bool writeable)
      {
         if (IsWritable == false)
         {
            if (writeable == false)
            {
               int iFound;
               if (Processors.TryGetValue(Thread.CurrentThread, out iFound) == true)
                  Processors[Thread.CurrentThread] += 1;
               else
                  Processors.Add(Thread.CurrentThread, 1);
               return this;
            }
            else
            {
               if (Processors.Count > 0)
                  return AddNewLock(writeable);
               else
               {
                  IsWritable = true;
                  Processors.Add(Thread.CurrentThread, 1);
                  return this;
               }
            }
         }
         else
         {
            if (Processors.Count > 0)                                                  //check taht the lock is actually used, if not, we can still use this lock.
               return AddNewLock(writeable);
            else
            {
               IsWritable = writeable;
               Processors.Add(Thread.CurrentThread, 1);
               return this;
            }
         }
      }

      /// <summary>
      /// Changes the current lock to writeable. This is possible if there are no other processors
      /// monitoring
      /// </summary>
      /// <returns></returns>
      internal LockObject UpgradeForWriting()
      {
         if (IsWritable == false)
         {
            if (Processors.Count == 1)
            {
               IsWritable = true;
               return this;
            }
            else
            {
               Processors.Remove(Thread.CurrentThread);                                  //we remove the lock that needs to be upgraded from this list and create a new lock, specifically for that processor.
               return AddNewLock(true);
            }
         }
         else
            return this;
      }

      /// <summary>
      /// Releases the lock for the current processor.
      /// </summary>
      /// <returns>
      /// -This lock, if there are still other processors. 
      /// - if ther are no more processors, the next lock is returned.
      /// - if there is no next lock, null is returned.
      /// </returns>
      internal LockObject Release()
      {
         int iFound;
         if (Processors.TryGetValue(Thread.CurrentThread, out iFound) == true)
         {
            if (iFound == 1)
            {
               Processors.Remove(Thread.CurrentThread);
               return Next;
            }
            else
            {
               Processors[Thread.CurrentThread] = iFound - 1;
               return this;
            }
         }
         else
            throw new InvalidOperationException("Processor doesn't appear to own this lock-key.");
      }
      #endregion
   }
}
