using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Diagnostics;

namespace JaStDev.HAB
{

   /// <summary>
   /// A manager class for all the locks on the neurons to make everything thread safe.
   /// </summary>
   internal class LockManager
   {

      #region Fields

      static LockManager fCurrent = new LockManager();
      Dictionary<Neuron, NeuronLock> fLinksIn = new Dictionary<Neuron, NeuronLock>();
      Dictionary<Neuron, NeuronLock> fLinksOut = new Dictionary<Neuron, NeuronLock>();
      Dictionary<Neuron, NeuronLock> fParents = new Dictionary<Neuron, NeuronLock>();
      Dictionary<Neuron, NeuronLock> fChildren = new Dictionary<Neuron, NeuronLock>();
      Dictionary<Neuron, NeuronLock> fValue = new Dictionary<Neuron, NeuronLock>();
      Dictionary<Neuron, NeuronLock> fProcessors = new Dictionary<Neuron, NeuronLock>();
      Dictionary<LinkInfoList, LinkInfoLock> fLinkInfos = new Dictionary<LinkInfoList, LinkInfoLock>();
      ReaderWriterLockSlim fLock = new ReaderWriterLockSlim();
      Queue<ManualResetEvent> fWaitHandles = new Queue<ManualResetEvent>();
      int fNrOfPreloadedHandles;
      int fNrOfUsedHandles;                                                            //stores the amount of handles that are currently being used. This allows us to create new handles as needed when the value for NrOfPreloadedhandles changes.
      bool fLocked;                                                                    //when true, the entire lockManager is locked, any requests for locks are illegal at this point. 
      #endregion

      #region ctor
      /// <summary>
      /// Initializes a new instance of the <see cref="LockManager"/> class. 
      /// Private so that it can' be created and everyone needs to use <see cref="LockManager.Current"/>.
      /// </summary>
      private LockManager()
      {

      } 
      #endregion

      #region Prop

      #region Current
      /// <summary>
      /// Gets the lockmanager object to use.
      /// </summary>
      /// <value>The current.</value>
      public static LockManager Current
      {
         get
         {
            return fCurrent;
         }
      } 
      #endregion

      #region NrOfPreloadedHandles

      /// <summary>
      /// Gets/sets the number of preloaded waithandles that the LockMananager creates that 
      /// the <see cref="neuronLock"/> can use.
      /// </summary>
      public int NrOfPreloadedHandles
      {
         get
         {
            return fNrOfPreloadedHandles;
         }
         set
         {
            if (value != fNrOfPreloadedHandles)
            {
               fNrOfPreloadedHandles = value;
               if (fNrOfPreloadedHandles < value)
               {
                  lock (fWaitHandles)
                  {
                     while (fWaitHandles.Count + fNrOfUsedHandles < fNrOfPreloadedHandles)
                        fWaitHandles.Enqueue(new ManualResetEvent(false));
                  }
               }
               else
               {
                  lock (fWaitHandles)
                  {
                     while (fWaitHandles.Count + fNrOfUsedHandles > fNrOfPreloadedHandles)
                        fWaitHandles.Dequeue();
                  }
               }
            }
         }
      }

      #endregion

      #endregion


      #region functions

      /// <summary>
      /// Gets a waithandle that is on stock, or a new one if there aren't any more.
      /// </summary>
      /// <returns></returns>
      internal ManualResetEvent GetWaitHandle()
      {
         lock (fWaitHandles)
         {
            fNrOfUsedHandles++;
            if (fWaitHandles.Count > 0)
            {
               ManualResetEvent iRes = fWaitHandles.Dequeue();
               iRes.Reset();                                                                       //need to make certain that the event is blocking
               return iRes;
            }
            else
               return new ManualResetEvent(false);
         }
      }

      internal void ReleaseWaitHandle(ManualResetEvent toRelease)
      {
         lock (fWaitHandles)
         {
            fNrOfUsedHandles--;
            fWaitHandles.Enqueue(toRelease);
         }
      }



      /// <summary>
      /// Requests a lock for the specified neuron, level and wether it needs to be writeable. 
      /// </summary>
      /// <remarks>
      /// Internally, it will create the lock, if it doesn't exist yet, prepare it for this processor
      /// </remarks>
      /// <param name="neuron">The neuron.</param>
      /// <param name="level">The level.</param>
      /// <returns></returns>
      public object RequestLock(Neuron neuron, LockLevel level, bool writeable)
      {
         if (fLocked == true)
            throw new InvalidOperationException("No lock can be acquired during a general lockdown.");
         NeuronLock iRes = null;
         switch (level)
         {
            case LockLevel.None:
               break;
            case LockLevel.LinksIn:
               iRes = RequestLockFor(neuron, fLinksIn, writeable);
               break;
            case LockLevel.LinksOut: 
               iRes = RequestLockFor(neuron, fLinksOut, writeable);
               break;
            case LockLevel.Children:
               iRes = RequestLockFor(neuron, fChildren, writeable);
               break;
            case LockLevel.Parents:
               iRes = RequestLockFor(neuron, fParents, writeable);
               break;
            case LockLevel.Value:
               iRes = RequestLockFor(neuron, fValue, writeable);
               break;
            case LockLevel.Processors:
               iRes = RequestLockFor(neuron, fProcessors, writeable);
               break;
            case LockLevel.All:
               return CreateCompleteLock(neuron, writeable);
            default:
               throw new InvalidOperationException("Unkown level requested");
         }  
         if (iRes != null && iRes.WaitHandle != null)                                  //we still need to wait untill the lock is ready to be used by the current proc because others were buzy with the neuron
         {
            iRes.WaitHandle.WaitOne();
            iRes.WaitHandle = null;                                                    //once the waithandle has served it's purpose, remove it so that it doesn't accidently block a next call.
         }
         return iRes;
      }

      /// <summary>
      /// Requests a lock for multiple neurons at the same time. This allows you to lock multiple neurons, used
      /// in a single operations, at once, in 1 thread safe call.
      /// </summary>
      /// <param name="requests">The request info: each record contains the info for 1 neuron. Upon returning, each
      /// <see cref="LockRequestInfo"/> item will contain the key that can later be used for unlocking or upgrading.</param>
      public void RequestLocks(IEnumerable<LockRequestInfo> requests)
      {
         if (fLocked == true)
            throw new InvalidOperationException("No lock can be acquired during a general lockdown.");
         List<NeuronLock> iRes = new List<NeuronLock>();
         fLock.EnterUpgradeableReadLock();
         try
         {
            foreach (LockRequestInfo i in requests)
            {
               if (i.Level == LockLevel.All)                                                          //we need to do this outside the switch, cause the switch is only for functions that return a single lock, when we lock all,we get multiple lock objects back.
               {
                  List<NeuronLock> iLocks = InternalCreateCompleteLock(i.Neuron, i.Writeable);
                  iRes.AddRange(from ii in iLocks where ii != null && ii.WaitHandle != null select ii);
                  i.Key = iLocks;
               }
               else
               {
                  NeuronLock iTemp = null;
                  switch (i.Level)
                  {
                     case LockLevel.None:
                        break;
                     case LockLevel.LinksIn:
                        iTemp = internalRequestLockFor(i.Neuron, fLinksIn, i.Writeable);
                        break;
                     case LockLevel.LinksOut:
                        iTemp = internalRequestLockFor(i.Neuron, fLinksOut, i.Writeable);
                        break;
                     case LockLevel.Children:
                        iTemp = internalRequestLockFor(i.Neuron, fChildren, i.Writeable);
                        break;
                     case LockLevel.Parents:
                        iTemp = internalRequestLockFor(i.Neuron, fParents, i.Writeable);
                        break;
                     case LockLevel.Processors:
                        iTemp = internalRequestLockFor(i.Neuron, fProcessors, i.Writeable);
                        break;
                     case LockLevel.Value:
                        iTemp = internalRequestLockFor(i.Neuron, fValue, i.Writeable);
                        break;
                     default:
                        throw new InvalidOperationException("Unkown level requested");
                  }
                  Debug.Assert(iTemp != null);
                  if (iTemp.WaitHandle != null)
                     iRes.Add(iTemp);
                  i.Key = iTemp;
               }
            }
         }
         finally
         {
            fLock.ExitUpgradeableReadLock();
         }
         if (iRes.Count > 0)                                                           //we still need to wait untill all locks are ready to be used by the current proc because others were buzy with the neurons
         {
            var iHandles = from i in iRes select i.WaitHandle;
            WaitHandle.WaitAll(iHandles.ToArray());
            foreach (NeuronLock i in iRes)
               i.WaitHandle = null;                                                    //once the waithandle has served it's purpose, remove it so that it doesn't accidently block a next call.
         }
      }

      /// <summary>
      /// Upgrades the lock for the current processor and neuron, so that it is possible to write to it.
      /// </summary>
      /// <param name="neuron">The neuron to lock</param>
      /// <param name="key">The current key for the neuron, required to make certain the proc already has
      /// a lock for the neuron.</param>
      /// <returns></returns>
      /// <remarks>
      /// We check if the lock is currently only used by 1 processor, if so, simply allow for writes. If
      /// there are multiple processors still reading the neuron, remove the current processor from the list
      /// and create a new key, which is inserted just after the curent one, so it becomes the next one to 
      /// let run, which will block the current processor untill all others are done reading. When this
      /// happens, a new key is returned after the lock has been acquired.
      /// </remarks>
      /// <exception cref="InvalidOperationException">If the key is not for the first in the list of requested 
      /// locks</exception>
      /// <exception cref="InvalidOperationException">No lock found for the specified neuron.</exception>
      public object UpgradeLockForWriting(Neuron neuron, LockLevel level, object key)
      {
         NeuronLock iNew = null;
         switch (level)
         {
            case LockLevel.None:
               return null;
            case LockLevel.LinksIn:
               iNew = UpgradeToWritingFor(neuron, key, fLinksIn);
               break;
            case LockLevel.LinksOut:
               iNew = UpgradeToWritingFor(neuron, key, fLinksOut);
               break;
            case LockLevel.Children:
               iNew = UpgradeToWritingFor(neuron, key, fChildren);
               break;
            case LockLevel.Parents:
               iNew = UpgradeToWritingFor(neuron, key, fParents);
               break;
            case LockLevel.Processors:
               iNew = UpgradeToWritingFor(neuron, key, fProcessors);
               break;
            case LockLevel.Value:
               iNew = UpgradeToWritingFor(neuron, key, fValue);
               break;
            case LockLevel.All:
               return UpgradeCompleteLock(neuron, key);
            default:
               throw new InvalidOperationException("Unkown level requested");
         }
         Debug.Assert(iNew != null);
         if (iNew != key && iNew.WaitHandle != null)                                            //when the write upgrade could not be done because there were still processors reading, we get a new lock, in which case we must wait untill it is ready to be used.
         {
            iNew.WaitHandle.WaitOne();
            return iNew;
         }
         return iNew;
      }


      /// <summary>
      /// Upgrades a list of locks for the current processor at the same time in a thread safe way, 
      /// so that it is possible to write to them.
      /// </summary>
      /// <remarks>
      /// For more info, see <see cref="LockManager.UpgradeLockForWriting"/>
      /// </remarks>
      /// <param name="requests">The requests.</param>
      public void UpgradeLocksForWriting(IEnumerable<LockRequestInfo> requests)
      {
         List<NeuronLock> iRes = new List<NeuronLock>();
         fLock.EnterReadLock();
         try
         {
            foreach (LockRequestInfo i in requests)
            {
               if (i.Level == LockLevel.All)                                                          //we need to do this outside the switch, cause the switch is only for functions that return a single lock, when we lock all,we get multiple lock objects back.
               {
                  List<NeuronLock> iLocks = InternalUpgradeCompleteLock(i.Neuron, (List<NeuronLock>)i.Key);
                  iRes.AddRange(from ii in iLocks where ii != null && ii.WaitHandle != null select ii);
                  i.Key = iLocks;
               }
               else
               {
                  NeuronLock iTemp = null;
                  switch (i.Level)
                  {
                     case LockLevel.None:
                        break;
                     case LockLevel.LinksIn:
                        iTemp = InternalUpgradeToWritingFor(i.Neuron, i.Key, fLinksIn);
                        break;
                     case LockLevel.LinksOut:
                        iTemp = internalRequestLockFor(i.Neuron, fLinksOut, i.Writeable);
                        break;
                     case LockLevel.Children:
                        iTemp = internalRequestLockFor(i.Neuron, fChildren, i.Writeable);
                        break;
                     case LockLevel.Parents:
                        iTemp = internalRequestLockFor(i.Neuron, fParents, i.Writeable);
                        break;
                     case LockLevel.Processors:
                        iTemp = internalRequestLockFor(i.Neuron, fProcessors, i.Writeable);
                        break;
                     case LockLevel.Value:
                        iTemp = internalRequestLockFor(i.Neuron, fValue, i.Writeable);
                        break;
                     default:
                        throw new InvalidOperationException("Unkown level requested");
                  }
                  Debug.Assert(iTemp != null);
                  if (iTemp.WaitHandle != null)
                     iRes.Add(iTemp);
                  i.Key = iTemp;
               }
            }
         }
         finally
         {
            fLock.ExitReadLock();
         }
         if (iRes.Count > 0)                                                           //we still need to wait untill all locks are ready to be used by the current proc because others were buzy with the neurons
         {
            var iHandles = from i in iRes select i.WaitHandle;
            WaitHandle.WaitAll(iHandles.ToArray());
            foreach (NeuronLock i in iRes)
               i.WaitHandle = null;                                                    //once the waithandle has served it's purpose, remove it so that it doesn't accidently block a next call.
         }
      }

      /// <summary>
      /// Releases the lock with the specified key for the specified neuron. If there are any other locks
      /// for the neuron, they are activated now.
      /// </summary>
      /// <param name="neuron">The neuron.</param>
      /// <param name="key">The key.</param>
      public void ReleaseLock(Neuron neuron, LockLevel level, object key)
      {
         switch (level)
         {
            case LockLevel.None:
               break;
            case LockLevel.LinksIn:
               ReleaseLockFor(neuron, key, fLinksIn);
               break;
            case LockLevel.LinksOut:
               ReleaseLockFor(neuron, key, fLinksOut);
               break;
            case LockLevel.Children:
               ReleaseLockFor(neuron, key, fChildren);
               break;
            case LockLevel.Parents:
               ReleaseLockFor(neuron, key, fParents);
               break;
            case LockLevel.Processors:
               ReleaseLockFor(neuron, key, fProcessors);
               break;
            case LockLevel.Value:
               ReleaseLockFor(neuron, key, fValue);
               break;
            case LockLevel.All:
               ReleaseCompleteLock(neuron, key);
               break;
            default:
               break;
         }
      }

      /// <summary>
      /// Releases all the locks, stored in the specified <see cref="LockRequestInfo"/> objects
      /// together in a single thread safe action.
      /// </summary>
      /// <param name="requests">The requests.</param>
      public void ReleaseLocks(IEnumerable<LockRequestInfo> requests)
      {
         fLock.EnterUpgradeableReadLock();
         try
         {
            foreach (LockRequestInfo i in requests)
            {
               switch (i.Level)
               {
                  case LockLevel.None:
                     break;
                  case LockLevel.LinksIn:
                     InternalReleaseLockFor(i.Neuron, i.Key, fLinksIn);
                     break;
                  case LockLevel.LinksOut:
                     InternalReleaseLockFor(i.Neuron, i.Key, fLinksOut);
                     break;
                  case LockLevel.Children:
                     InternalReleaseLockFor(i.Neuron, i.Key, fChildren);
                     break;
                  case LockLevel.Parents:
                     InternalReleaseLockFor(i.Neuron, i.Key, fParents);
                     break;
                  case LockLevel.Processors:
                     InternalReleaseLockFor(i.Neuron, i.Key, fProcessors);
                     break;
                  case LockLevel.Value:
                     InternalReleaseLockFor(i.Neuron, i.Key, fValue);
                     break;
                  case LockLevel.All:
                     InternalReleaseCompleteLock(i.Neuron, (List<NeuronLock>)i.Key);
                     break;
                  default:
                     break;
               }
            }
         }
         finally
         {
            fLock.ExitUpgradeableReadLock();
         }
      }

      #endregion

      #region Helpers


      #region Release lock

      /// <summary>
      /// Releases the lock on every part of the neuron.
      /// </summary>
      /// <param name="neuron">The neuron.</param>
      /// <param name="key">The key.</param>
      private void ReleaseCompleteLock(Neuron neuron, object key)
      {
         List<NeuronLock> iList = key as List<NeuronLock>;
         if (iList != null)
         {
            fLock.EnterUpgradeableReadLock();
            try
            {
               InternalReleaseCompleteLock(neuron, iList);
            }
            finally
            {
               fLock.ExitUpgradeableReadLock();
            }
         }
         else
            throw new InvalidOperationException("Invalid key.");
      }

      /// <summary>
      /// Releases the lock on every part of the neuron in a thread unsafe way.
      /// </summary>
      /// <param name="neuron">The neuron.</param>
      /// <param name="iList">The i list.</param>
      private void InternalReleaseCompleteLock(Neuron neuron, List<NeuronLock> iList)
      {
         Dictionary<Neuron, NeuronLock>[] iDicts = new Dictionary<Neuron, NeuronLock>[5] { fLinksIn, fLinksOut, fChildren, fParents, fValue };
         for (int i = 0; i < iList.Count; i++)
         {
            if (iList[i] != null)
               InternalReleaseLockFor(neuron, iList[i], iDicts[i]);
         }
      }

      /// <summary>
      /// Releases the lock for a specific list.
      /// </summary>
      /// <param name="neuron">The neuron.</param>
      /// <param name="key">The key.</param>
      /// <param name="dict">The f links in.</param>
      private void ReleaseLockFor(Neuron neuron, object key, Dictionary<Neuron, NeuronLock> dict)
      {
         fLock.EnterUpgradeableReadLock();
         try
         {
            InternalReleaseLockFor(neuron, key, dict);
         }
         finally
         {
            fLock.ExitUpgradeableReadLock();
         }
      }

      /// <summary>
      /// Releases the lock for a specific list in a thread unsafe way.
      /// </summary>
      /// <param name="neuron">The neuron.</param>
      /// <param name="key">The key.</param>
      /// <param name="dict">The dict.</param>
      private void InternalReleaseLockFor(Neuron neuron, object key, Dictionary<Neuron, NeuronLock> dict)
      {
         NeuronLock iFound;
         NeuronLock iNext;
         if (dict.TryGetValue(neuron, out iFound) == true)
         {
            if (iFound == key)
            {
               lock (iFound)                                                              //lock the entire object so no other thread can change it while performing the action.
                  iNext = (NeuronLock)iFound.Release();
               if (iNext == null)
                  dict.Remove(neuron);
               else if (iNext != iFound)
               {
                  fLock.EnterWriteLock();
                  try
                  {
                     dict[neuron] = iNext;                                               //this removes the first lock from linked list.
                  }
                  finally
                  {
                     fLock.ExitWriteLock();
                  }
                  Debug.Assert(iNext.WaitHandle != null);
                  iNext.WaitHandle.Set();
               }
            }
            else
               throw new InvalidOperationException("Invalid key.");
         }
         else
            throw new InvalidOperationException("No lock found for the specified neuron.");
      }

      #endregion


      #region RequestLock
      /// <summary>
      /// Creates a new ore reuses an already existing lock for a single list.
      /// </summary>
      /// <param name="neuron">The neuron.</param>
      /// <param name="dict">The dict.</param>
      /// <param name="writeable">if set to <c>true</c> [writeable].</param>
      /// <returns></returns>
      private NeuronLock RequestLockFor(Neuron neuron, Dictionary<Neuron, NeuronLock> dict, bool writeable)
      {
         fLock.EnterUpgradeableReadLock();
         try
         {
            return internalRequestLockFor(neuron, dict, writeable);
         }
         finally
         {
            fLock.ExitUpgradeableReadLock();
         }
      }

      /// <summary>
      /// Creates a new ore reuses an already existing lock for a single list in a semi thread unsafe way.
      /// </summary>
      /// <param name="neuron">The neuron.</param>
      /// <param name="dict">The dict.</param>
      /// <param name="writeable">if set to <c>true</c> [writeable].</param>
      /// <returns></returns>
      private NeuronLock internalRequestLockFor(Neuron neuron, Dictionary<Neuron, NeuronLock> dict, bool writeable)
      {
         NeuronLock iFound;
         NeuronLock iLock;
         if (dict.TryGetValue(neuron, out iFound) == false)
         {
            fLock.EnterWriteLock();
            try
            {
               iFound = new NeuronLock();                                     //don't init the writeable, if this is true, we get a false result cause the merge would fail
               dict.Add(neuron, iFound);
            }
            finally
            {
               fLock.ExitWriteLock();
            }
         }
         lock (iFound)                                                     //lock the entire object so no other thread can change it while performing the action.
            iLock = (NeuronLock)iFound.Merge(writeable);
         return iLock;
      }

      /// <summary>
      /// Creates a lock for the neuron on all of it's lists.  We wait untill each 
      /// event is signalled before returning.
      /// </summary>
      /// <param name="neuron">The neuron.</param>
      /// <param name="writeable">if set to <c>true</c> [writeable].</param>
      /// <returns>
      /// A list containing all the keys for each list.
      /// This list always contains 5 elements, for all the possible lists.
      /// </returns>
      private object CreateCompleteLock(Neuron neuron, bool writeable)
      {
         List<NeuronLock> iResList;
         fLock.EnterUpgradeableReadLock();
         try
         {
            iResList = InternalCreateCompleteLock(neuron, writeable);
         }
         finally
         {
            fLock.ExitUpgradeableReadLock();
         }
         ManualResetEvent[] iWaitFor = (from i in iResList where i != null && i.WaitHandle != null select i.WaitHandle).ToArray();
         if (iWaitFor.Length > 0)
         {
            WaitHandle.WaitAll(iWaitFor);
            foreach (NeuronLock i in iResList)                                               //still need to remove the waithandles, since these locks are now active.
               if (i != null)
                  i.WaitHandle = null;
         }
         return iResList;
      }

      private List<NeuronLock> InternalCreateCompleteLock(Neuron neuron, bool writeable)
      {
         NeuronLock iTemp;
         List<NeuronLock> iRes = new List<NeuronLock>(5);
         for (int i = 0; i < 5; i++)
            iRes.Add(null);
         iTemp = internalRequestLockFor(neuron, fParents, writeable);
         iRes[0] = iTemp;
         iTemp = internalRequestLockFor(neuron, fLinksIn, writeable);
         iRes[1] = iTemp;
         iTemp = internalRequestLockFor(neuron, fLinksOut, writeable);
         iRes[2] = iTemp;
         if (neuron is NeuronCluster)
         {
            iTemp = internalRequestLockFor(neuron, fChildren, writeable);
            iRes[3] = iTemp;
            iTemp = internalRequestLockFor(neuron, fValue, writeable);
            iRes[4] = iTemp;
         }
         else if (neuron is ValueNeuron)
         {
            iTemp = internalRequestLockFor(neuron, fValue, writeable);
            iRes[4] = iTemp;
         }
         return iRes;
      } 
      #endregion

      #region Upgrade
      /// <summary>
      /// Upgrades a lock that wraps the whole neuron.
      /// </summary>
      /// <param name="neuron">The neuron.</param>
      /// <param name="key">The key.</param>
      /// <returns></returns>
      private object UpgradeCompleteLock(Neuron neuron, object key)
      {
         List<NeuronLock> iList = key as List<NeuronLock>;
         if (iList != null)
         {
            List<NeuronLock> iToWaitFor;
            fLock.EnterReadLock();
            try
            {
               iToWaitFor = InternalUpgradeCompleteLock(neuron, iList);
            }
            finally
            {
               fLock.ExitReadLock();
            }
            if (iToWaitFor.Count > 0)
            {
               var iHandles = from i in iToWaitFor select i.WaitHandle;
               WaitHandle.WaitAll(iHandles.ToArray());
               foreach (NeuronLock i in iToWaitFor)                                                   //when all locks acquired, release the waithandles.
                  i.WaitHandle = null;
            }
         }
         else
            throw new InvalidOperationException("Invalid key.");
         return iList;
      }

      /// <summary>
      /// Upgrades a lock that wraps the whole neuron in a thread unsafe way.
      /// </summary>
      /// <param name="neuron">The neuron.</param>
      /// <param name="list">The list.</param>
      /// <returns></returns>
      private List<NeuronLock> InternalUpgradeCompleteLock(Neuron neuron, List<NeuronLock> list)
      {
         List<NeuronLock> iToWaitFor = new List<NeuronLock>();
         Dictionary<Neuron, NeuronLock>[] iDicts = new Dictionary<Neuron, NeuronLock>[5] { fLinksIn, fLinksOut, fChildren, fParents, fValue };
         for (int i = 0; i < list.Count; i++)
         {
            if (list[i] != null)
            {
               NeuronLock iNew = InternalUpgradeToWritingFor(neuron, list[i], iDicts[i]);
               if (list[i] != iNew && iNew.WaitHandle != null)
                  iToWaitFor.Add(iNew);
               list[i] = iNew;
            }
         }
         return iToWaitFor;
      }

      /// <summary>
      /// Upgrades a lock, found in the specified dictionary (so a specific part of the
      /// neuron is freed for writing).
      /// </summary>
      /// <param name="neuron">The neuron.</param>
      /// <param name="key">The key.</param>
      /// <param name="dict">The dict.</param>
      /// <returns></returns>
      private NeuronLock UpgradeToWritingFor(Neuron neuron, object key, Dictionary<Neuron, NeuronLock> dict)
      {
         fLock.EnterReadLock();
         try
         {
            return InternalUpgradeToWritingFor(neuron, key, dict);
         }
         finally
         {
            fLock.ExitReadLock();
         }
      }

      /// <summary>
      /// Upgrades a lock, found in the specified dictionary (so a specific part of the
      /// neuron is freed for writing) in a thread unsafe way.
      /// </summary>
      /// <param name="neuron">The neuron.</param>
      /// <param name="key">The key.</param>
      /// <param name="dict">The dict.</param>
      /// <returns></returns>
      private NeuronLock InternalUpgradeToWritingFor(Neuron neuron, object key, Dictionary<Neuron, NeuronLock> dict)
      {
         NeuronLock iFound = null;
         NeuronLock iNew = null;
         if (dict.TryGetValue(neuron, out iFound) == true)
         {
            if (iFound == key)
            {
               lock (iFound)                                                                          //lock the entire object so no other thread can change it while performing the action.
                  iNew = (NeuronLock)iFound.UpgradeForWriting();
            }
            else
               throw new InvalidOperationException("Invalid key.");
         }
         else
            throw new InvalidOperationException("No lock found for the specified neuron.");
         return iNew;
      } 
      #endregion

	   #endregion

      /// <summary>
      /// Locks the entire lockManager.  Any requests done when the lockManager is locked, result in
      /// an exception. This is to allow for 'Flush' write operations.
      /// </summary>
      internal void LockAll()
      {
         fLocked = true;
      }

      /// <summary>
      /// Releases the lock on the entire LockManager.
      /// </summary>
      internal void ReleaseLockAll()
      {
         fLocked = false;
      }

      /// <summary>
      /// Requests a lock for a <see cref="LinkInfoList"/>, which is not connected with a specific
      /// neuron.
      /// </summary>
      /// <param name="list">The list.</param>
      /// <param name="writeable">if set to <c>true</c> [writeable].</param>
      /// <returns></returns>
      internal object RequestLock(LinkInfoList list, bool writeable)
      {
         fLock.EnterUpgradeableReadLock();
         try
         {
            LinkInfoLock iFound;
            LinkInfoLock iLock;
            if (fLinkInfos.TryGetValue(list, out iFound) == false)
            {
               fLock.EnterWriteLock();
               try
               {
                  iFound = new LinkInfoLock();                                //don't init writeable, if this is true, the merge will also create a new record, which we don't want.
                  fLinkInfos.Add(list, iFound);
               }
               finally
               {
                  fLock.ExitWriteLock();
               }
            }
            lock (iFound)                                                     //lock the entire object so no other thread can change it while performing the action.
               iLock = (LinkInfoLock)iFound.Merge(writeable);
            return iLock;
         }
         finally
         {
            fLock.ExitUpgradeableReadLock();
         }
      }

      /// <summary>
      /// Releases the lock with the specified key for the specified neuron. If there are any other locks
      /// for the neuron, they are activated now.
      /// </summary>
      /// <param name="list">The list.</param>
      /// <param name="key">The key.</param>
      internal void ReleaseLock(LinkInfoList list, object key)
      {
         fLock.EnterUpgradeableReadLock();
         try
         {
            LinkInfoLock iFound;
            LinkInfoLock iNext;
            if (fLinkInfos.TryGetValue(list, out iFound) == true)
            {
               if (iFound == key)
               {
                  lock (iFound)                                                              //lock the entire object so no other thread can change it while performing the action.
                     iNext = (LinkInfoLock)iFound.Release();
                  if (iNext == null)
                     fLinkInfos.Remove(list);
                  else if (iNext != iFound)
                  {
                     fLock.EnterWriteLock();
                     try
                     {
                        fLinkInfos[list] = iNext;                                               //this removes the first lock from linked list.
                     }
                     finally
                     {
                        fLock.ExitWriteLock();
                     }
                     Debug.Assert(iNext.WaitHandle != null);
                     iNext.WaitHandle.Set();
                  }
               }
               else
                  throw new InvalidOperationException("Invalid key.");
            }
            else
               throw new InvalidOperationException("No lock found for the specified neuron.");
         }
         finally
         {
            fLock.ExitUpgradeableReadLock();
         }
      }
   }
}
