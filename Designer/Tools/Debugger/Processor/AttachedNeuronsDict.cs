// --------------------------------------------------------------------------------------------------------------------
// <copyright file="AttachedNeuronsDict.cs" company="">
//   
// </copyright>
// <summary>
//   This class contains all the neurons that are attached to a processor.
//   This is used by the <see cref="ProcessorManager" /> to find neurons that
//   were changed in a processor other than the one it is attached to.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace JaStDev.HAB.Designer
{
    /// <summary>
    ///     This class contains all the neurons that are attached to a processor.
    ///     This is used by the <see cref="ProcessorManager" /> to find neurons that
    ///     were changed in a processor other than the one it is attached to.
    /// </summary>
    internal class AttachedNeuronsDict
    {
        /// <summary>Checks if a neuron with specified <paramref name="id"/> is registered
        ///     and if so, checks if the change is valid and lets the user know of any
        ///     invalid changes.</summary>
        /// <param name="id">The id.</param>
        /// <param name="proc">The proc.</param>
        /// <param name="message">The message to depict when there was an invalid change.</param>
        /// <returns>The <see cref="bool"/>.</returns>
        private bool ProcessId(ulong id, DebugProcessor proc, string message)
        {
            AttachedItem iFound;
            if (fAttachedItems.TryGetValue(id, out iFound))
            {
                if (iFound.Processor != proc)
                {
                    var iData = new InvalidChangeDebugData();
                    iData.Message = message;
                    iData.NeuronID = id;
                    iData.Watches = iFound.Watches;
                    iFound.Processor.OnAttachedNeuronChanged(iData);
                    proc.OnInvalidNeuronChange(iData);
                    if (iData.Watches != null)
                    {
                        foreach (var i in iData.Watches)
                        {
                            i.InvalidChangeData = iData;
                        }
                    }

                    System.Action<InvalidChangeDebugData> iDisplayError = DisplayChangeError;
                    System.Windows.Application.Current.Dispatcher.BeginInvoke(
                        System.Windows.Threading.DispatcherPriority.Background, 
                        iDisplayError, 
                        iData); // we use backgound here, so that the ProcItems have time to fill in there values.
                    return true;
                }
            }

            return false;
        }

        /// <summary>Displays the change error to the user so that he knows of the
        ///     situation.</summary>
        /// <param name="data">The data.</param>
        private void DisplayChangeError(InvalidChangeDebugData data)
        {
            var iMsg =
                string.Format(
                    "An attached neuron has been modified outside of it's assigned processor. Details:\nNeuron: {0} \nMessage: {1} \nOwning processor: {2} \nChanged in: {3}", 
                    BrainData.Current.NeuronInfo[data.NeuronID].DisplayTitle, 
                    data.Message, 
                    data.Owner.Name, 
                    data.Originator.Name);
            data.Message = iMsg; // we provide a more elaborate message for the UI.
            System.Windows.MessageBox.Show(
                iMsg, 
                "Neuron changed", 
                System.Windows.MessageBoxButton.OK, 
                System.Windows.MessageBoxImage.Warning);
        }

        /// <summary>Called when a processor is finished. It will remove all the neurons
        ///     for a specified processor.</summary>
        /// <param name="proc">The proc.</param>
        internal void ProcessorFinished(DebugProcessor proc)
        {
            System.Collections.Generic.List<Neuron> iFound;
            fLock.EnterWriteLock();
            try
            {
                if (fProcs.TryGetValue(proc, out iFound))
                {
                    foreach (var i in iFound)
                    {
                        fAttachedItems.Remove(i.ID);
                    }

                    fProcs.Remove(proc);
                    fHasAttachedItems = fAttachedItems.Count > 0;
                }
            }
            finally
            {
                fLock.ExitWriteLock();
            }
        }

        /// <summary>Adds the specified <paramref name="watch"/> to the list of variables
        ///     that need to be monitored for value assignement.</summary>
        /// <param name="watch">The watch.</param>
        internal void MonitorWatch(Watch watch)
        {
            fLock.EnterWriteLock();
            try
            {
                var iVar = (Variable)watch.Item;
                System.Diagnostics.Debug.Assert(iVar != null);
                if (iVar is SystemVariable)
                {
                    RegisterSystemVar(watch);
                }
                else
                {
                    WatchRegistration iFound;
                    if (fVariables.TryGetValue(iVar, out iFound) == false)
                    {
                        iFound = new WatchRegistration();
                        fVariables.Add(iVar, iFound);
                    }

                    iFound.Watches.Add(watch);
                }
            }
            finally
            {
                fLock.ExitWriteLock();
            }
        }

        /// <summary>Removes the <paramref name="watch"/> from the list of items to
        ///     monitor.</summary>
        /// <param name="watch">The watch.</param>
        internal void RemoveWatch(Watch watch)
        {
            fLock.EnterWriteLock();
            try
            {
                var iVar = (Variable)watch.Item;
                if (iVar is SystemVariable)
                {
                    RemoveSystemVar(watch);
                }
                else
                {
                    WatchRegistration iFound;
                    if (fVariables.TryGetValue(iVar, out iFound))
                    {
                        iFound.Watches.Remove(watch);
                        if (iFound.Watches.Count == 0)
                        {
                            if (iFound.Attachements != null)
                            {
                                foreach (var i in iFound.Attachements)
                                {
                                    DetachItem(i);
                                }
                            }

                            fVariables.Remove(iVar);
                        }
                    }
                }
            }
            finally
            {
                fLock.ExitWriteLock();
            }
        }

        /// <summary>checks which type it is and registers the system var for monitoring.</summary>
        /// <param name="watch">The watch.</param>
        private void RegisterSystemVar(Watch watch)
        {
            var iSys = (SystemVariable)watch.Item;
            if (iSys is CurrentTo)
            {
                fAttachedTo.Add(watch);
            }
            else if (iSys is CurrentFrom)
            {
                fAttachedFrom.Add(watch);
            }
            else if (iSys is CurrentInfo)
            {
                fAttachedInfo.Add(watch);
            }
            else if (iSys is CurrentMeaning)
            {
                fAttachedMeaning.Add(watch);
            }
            else if (iSys is CurrentSin)
            {
                fAttachedSin.Add(watch);
            }
            else
            {
                throw new System.InvalidOperationException("Unkown sytem variable: can't attach to a processor.");
            }
        }

        /// <summary>checks which type it is and Removes the system var from monitoring.</summary>
        /// <param name="watch">The watch.</param>
        private void RemoveSystemVar(Watch watch)
        {
            var iSysVar = (SystemVariable)watch.Item;
            if (iSysVar is CurrentTo)
            {
                fAttachedTo.Remove(watch);
            }
            else if (iSysVar is CurrentFrom)
            {
                fAttachedFrom.Remove(watch);
            }
            else if (iSysVar is CurrentInfo)
            {
                fAttachedInfo.Remove(watch);
            }
            else if (iSysVar is CurrentMeaning)
            {
                fAttachedMeaning.Remove(watch);
            }
            else if (iSysVar is CurrentSin)
            {
                fAttachedSin.Remove(watch);
            }
            else
            {
                throw new System.InvalidOperationException("Unkown sytem variable: can't attach to a processor.");
            }
        }

        /// <summary>The process sin for watch.</summary>
        /// <param name="value">The value.</param>
        /// <param name="proc">The proc.</param>
        internal void ProcessSinForWatch(Neuron value, DebugProcessor proc)
        {
            if (fAttachedSin != null && fAttachedSin.Count > 0)
            {
                fLock.EnterWriteLock();
                try
                {
                    AttachItem(value, proc, fAttachedSin);
                }
                finally
                {
                    fLock.ExitWriteLock();
                }
            }
        }

        /// <summary>The process meaning for watch.</summary>
        /// <param name="value">The value.</param>
        /// <param name="proc">The proc.</param>
        internal void ProcessMeaningForWatch(Neuron value, DebugProcessor proc)
        {
            if (fAttachedMeaning != null && fAttachedMeaning.Count > 0)
            {
                fLock.EnterWriteLock();
                try
                {
                    AttachItem(value, proc, fAttachedMeaning);
                }
                finally
                {
                    fLock.ExitWriteLock();
                }
            }
        }

        /// <summary>The process to for watch.</summary>
        /// <param name="value">The value.</param>
        /// <param name="proc">The proc.</param>
        internal void ProcessToForWatch(Neuron value, DebugProcessor proc)
        {
            if (fAttachedTo != null && fAttachedTo.Count > 0)
            {
                fLock.EnterWriteLock();
                try
                {
                    AttachItem(value, proc, fAttachedTo);
                }
                finally
                {
                    fLock.ExitWriteLock();
                }
            }
        }

        /// <summary>The process from for watch.</summary>
        /// <param name="value">The value.</param>
        /// <param name="proc">The proc.</param>
        internal void ProcessFromForWatch(Neuron value, DebugProcessor proc)
        {
            if (fAttachedFrom != null && fAttachedFrom.Count > 0)
            {
                fLock.EnterWriteLock();
                try
                {
                    AttachItem(value, proc, fAttachedFrom);
                }
                finally
                {
                    fLock.ExitWriteLock();
                }
            }
        }

        /// <summary>The process info for watch.</summary>
        /// <param name="values">The values.</param>
        /// <param name="proc">The proc.</param>
        internal void ProcessInfoForWatch(System.Collections.Generic.IEnumerable<Neuron> values, DebugProcessor proc)
        {
            if (fAttachedInfo != null && fAttachedInfo.Count > 0)
            {
                fLock.EnterWriteLock();
                try
                {
                    foreach (var i in values)
                    {
                        AttachItem(i, proc, fAttachedInfo);
                    }
                }
                finally
                {
                    fLock.ExitWriteLock();
                }
            }
        }

        /// <summary>Attaches every <paramref name="value"/> for the variable, if it was
        ///     registered to be monitored. If the neuron was already attached to
        ///     another variable, to original remains. Every watch that was registered
        ///     for the variable, is assigned to the attached item.</summary>
        /// <param name="var">The var.</param>
        /// <param name="value">The value.</param>
        /// <param name="proc">The proc.</param>
        internal void ProcessVarForWatch(
            Variable var, System.Collections.Generic.IEnumerable<Neuron> value, 
            DebugProcessor proc)
        {
            fLock.EnterWriteLock();
            try
            {
                WatchRegistration iFound;
                if (fVariables.TryGetValue(var, out iFound))
                {
                    iFound.Attachements = new System.Collections.Generic.List<Neuron>();
                    foreach (var i in value)
                    {
                        if (AttachItem(i, proc, iFound.Watches))
                        {
                            iFound.Attachements.Add(i);
                        }
                    }
                }
            }
            finally
            {
                fLock.ExitWriteLock();
            }
        }

        /// <summary>Processes the <paramref name="data"/> for all the processors that got
        ///     created during a split.</summary>
        /// <param name="data">The data.</param>
        public void ProcessSplit(ProcessorSplitter data)
        {
            foreach (DebugProcessor i in data.Processors)
            {
                if (i.NeuronToSolve != null)
                {
                    ProcessFromForWatch(i.NeuronToSolve, i);
                }

                if (i.CurrentTo != null)
                {
                    ProcessToForWatch(i.CurrentTo, i);
                }

                if (i.CurrentMeaning != null)
                {
                    ProcessMeaningForWatch(i.CurrentMeaning, i);
                }

                if (i.CurrentSin != null)
                {
                    ProcessSinForWatch(i.CurrentSin, i);
                }

                if (i.CurrentInfo != null)
                {
                    ProcessInfoForWatch(i.CurrentInfo, i);
                }
            }
        }

        #region Internal types

        /// <summary>
        ///     contains all the data that is required to manage registered watches.
        /// </summary>
        /// <remarks>
        ///     some notes: -A single variable can be mapped to multiple watches.
        ///     -when we remove a watch, which causes the variable to be removed
        ///     (because there are no more watches, we also need to remove all the
        ///     attached neurons that were caused by the variable/watches, to do this,
        ///     we need to know which variables caused a registration.
        /// </remarks>
        private class WatchRegistration
        {
            /// <summary>The f watches.</summary>
            private readonly System.Collections.Generic.List<Watch> fWatches =
                new System.Collections.Generic.List<Watch>();

            /// <summary>Gets the watches.</summary>
            public System.Collections.Generic.List<Watch> Watches
            {
                get
                {
                    return fWatches;
                }
            }

            /// <summary>
            ///     Gets or sets the list of neurons that were attached for these
            ///     watches.
            /// </summary>
            /// <value>
            ///     The attachements.
            /// </value>
            public System.Collections.Generic.List<Neuron> Attachements { get; set; }
        }

        /// <summary>
        ///     contains all the data for a single registered neuron.
        /// </summary>
        private class AttachedItem
        {
            /// <summary>
            ///     Gets or sets the processor that the item is attached to.
            /// </summary>
            /// <value>
            ///     The processor.
            /// </value>
            public DebugProcessor Processor { get; set; }

            /// <summary>
            ///     Gets or sets the watch that caused this neuron being attached to a
            ///     processor.
            /// </summary>
            /// <value>
            ///     The watch.
            /// </value>
            public System.Collections.Generic.List<Watch> Watches { get; set; }
        }

        #endregion

        #region Fields

        /// <summary>
        ///     Stores all the neurons that have been attached to a proc. Entry point
        ///     for the event side.
        /// </summary>
        private readonly System.Collections.Generic.Dictionary<ulong, AttachedItem> fAttachedItems =
            new System.Collections.Generic.Dictionary<ulong, AttachedItem>();

                                                                                    // we use a neuron so that we can also register temp items.

        /// <summary>The f has attached items.</summary>
        private volatile bool fHasAttachedItems;

                              // speed up: so we don't have to do a lock before checking if there are items.

        /// <summary>
        ///     Stores all the processors that have neurons attached + their list of
        ///     neurons. This is the entry point for the processors, when they have
        ///     finished.
        /// </summary>
        private readonly System.Collections.Generic.Dictionary<Processor, System.Collections.Generic.List<Neuron>> fProcs = new System.Collections.Generic.Dictionary<Processor, System.Collections.Generic.List<Neuron>>();

        /// <summary>The f lock.</summary>
        private readonly System.Threading.ReaderWriterLockSlim fLock = new System.Threading.ReaderWriterLockSlim();

        /// <summary>
        ///     stores all the variables that have been registered, with for each
        ///     variable, all the watches that were registered for it.
        /// </summary>
        private readonly System.Collections.Generic.Dictionary<Variable, WatchRegistration> fVariables =
            new System.Collections.Generic.Dictionary<Variable, WatchRegistration>();

        /// <summary>The f attached from.</summary>
        private readonly System.Collections.Generic.List<Watch> fAttachedFrom =
            new System.Collections.Generic.List<Watch>();

        /// <summary>The f attached meaning.</summary>
        private readonly System.Collections.Generic.List<Watch> fAttachedMeaning =
            new System.Collections.Generic.List<Watch>();

        /// <summary>The f attached to.</summary>
        private readonly System.Collections.Generic.List<Watch> fAttachedTo =
            new System.Collections.Generic.List<Watch>();

        /// <summary>The f attached sin.</summary>
        private readonly System.Collections.Generic.List<Watch> fAttachedSin =
            new System.Collections.Generic.List<Watch>();

        /// <summary>The f attached info.</summary>
        private readonly System.Collections.Generic.List<Watch> fAttachedInfo =
            new System.Collections.Generic.List<Watch>();

        #endregion

        #region functions

        /// <summary>Registers the specified to neuron for the specified processor so that
        ///     any change done in another processor is reported.</summary>
        /// <param name="toRegister">To register.</param>
        /// <param name="proc">The proc.</param>
        public void Attach(Neuron toRegister, DebugProcessor proc)
        {
            Attach(toRegister, proc, null);
        }

        /// <summary>Attaches the specified to neuron for the specified processor so that
        ///     any change done in another processor is reported.</summary>
        /// <remarks>Note: when the neuron is already attached to another processor, a
        ///     warning will be logged and the neuron will be attached to the new
        ///     processor. if you don't want this, try<see cref="AttachedNeuronsDict.TryAttach"/></remarks>
        /// <param name="toRegister">The nueron to register.</param>
        /// <param name="proc">The processor to which the neuron will be attached.</param>
        /// <param name="watch">The watch that caused the attach, will be warned when a change is
        ///     done. When not defined, only the offending processor will be warned,
        ///     otherwise, the watch can provide more info on where the registration
        ///     occured.</param>
        public void Attach(Neuron toRegister, DebugProcessor proc, System.Collections.Generic.List<Watch> watch)
        {
            fLock.EnterWriteLock();
            try
            {
                if (AttachItem(toRegister, proc, watch) == false)
                {
                    LogService.Log.LogWarning(
                        "AttachedNeuronsDict.Register", 
                        string.Format("{0} already attached to another processor, overwriting values!", toRegister.ID));
                    var iNew = fAttachedItems[toRegister.ID];
                    iNew.Processor = proc;
                    iNew.Watches = watch;
                }
            }
            finally
            {
                fLock.ExitWriteLock();
            }
        }

        /// <summary>Attaches the specified to neuron for the specified processor so that
        ///     any change done in another processor is reported if the neuron has not
        ///     yet been registered to another processor.</summary>
        /// <param name="toRegister">The nueron to register.</param>
        /// <param name="proc">The processor to which the neuron will be attached.</param>
        /// <param name="watch">The watch that caused the attach, will be warned when a change is
        ///     done. When not defined, only the offending processor will be warned,
        ///     otherwise, the watch can provide more info on where the registration
        ///     occured.</param>
        public void TryAttach(Neuron toRegister, DebugProcessor proc, System.Collections.Generic.List<Watch> watch)
        {
            fLock.EnterWriteLock();
            try
            {
                AttachItem(toRegister, proc, watch);
            }
            finally
            {
                fLock.ExitWriteLock();
            }
        }

        /// <summary>performs the actual Attaching, not thread safe. Returns<see langword="false"/> if the item was already attached.</summary>
        /// <param name="toRegister">To register.</param>
        /// <param name="proc">The proc.</param>
        /// <param name="watch">The watch.</param>
        /// <returns><c>true</c> if an attach was performed, otherwise false.</returns>
        private bool AttachItem(Neuron toRegister, DebugProcessor proc, System.Collections.Generic.List<Watch> watch)
        {
            AttachedItem iNew;
            if (fAttachedItems.TryGetValue(toRegister.ID, out iNew) == false)
            {
                iNew = new AttachedItem();
                fAttachedItems.Add(toRegister.ID, iNew);
                fHasAttachedItems = true;
                System.Collections.Generic.List<Neuron> iProcList;
                if (fProcs.TryGetValue(proc, out iProcList) == false)
                {
                    iProcList = new System.Collections.Generic.List<Neuron>();
                    fProcs.Add(proc, iProcList);
                }

                iProcList.Add(toRegister);
                iNew.Processor = proc;
                iNew.Watches = watch;
                return true;
            }

            return false;
        }

        /// <summary>Removes the specified neuron from the list of<paramref name="attached"/> items.</summary>
        /// <param name="attached">The attached.</param>
        public void Detach(Neuron attached)
        {
            fLock.EnterWriteLock();
            try
            {
                DetachItem(attached);
            }
            finally
            {
                fLock.ExitWriteLock();
            }
        }

        /// <summary>Thread unsafe version of <see cref="AttachedNeuronsDict.Detach"/> .</summary>
        /// <param name="attached">The attached.</param>
        private void DetachItem(Neuron attached)
        {
            AttachedItem iFound;
            if (fAttachedItems.TryGetValue(attached.ID, out iFound))
            {
                fAttachedItems.Remove(attached.ID);
                fHasAttachedItems = fAttachedItems.Count > 0;
                System.Collections.Generic.List<Neuron> iProcList;
                if (fProcs.TryGetValue(iFound.Processor, out iProcList))
                {
                    iProcList.Remove(attached);

                        // we don't have to check if the list is empty through this action (to remove it from the fProcs dict), since this is done when the proc is finished.
                }
                else
                {
                    LogService.Log.LogError(
                        "AttachedNeuronsDict.Detach", 
                        "internal error: Procslist and AttachedItems list are out of sync!!");
                }
            }
        }

        #endregion

        #region event handlers

        /// <summary>Called when any link was changed on a neuron.</summary>
        /// <param name="e">The <see cref="LinkChangedEventArgs"/> instance containing the event
        ///     data.</param>
        internal void LinkChanged(LinkChangedEventArgs e)
        {
            if (e.Processor != null && fHasAttachedItems)
            {
                // edits done in the editor don't need to be checked + speed up: only lock if really required.
                fLock.EnterReadLock();
                try
                {
                    if (ProcessId(e.NewFrom, (DebugProcessor)e.Processor, "New From part of link change") == false)
                    {
                        if (ProcessId(e.NewTo, (DebugProcessor)e.Processor, "New To part of link change") == false)
                        {
                            if (ProcessId(e.OldFrom, (DebugProcessor)e.Processor, "Old From part of link change")
                                == false)
                            {
                                ProcessId(e.OldTo, (DebugProcessor)e.Processor, "Old To part of link change");
                            }
                        }
                    }
                }
                finally
                {
                    fLock.ExitReadLock();
                }
            }
        }

        /// <summary>The link created.</summary>
        /// <param name="e">The e.</param>
        internal void LinkCreated(LinkChangedEventArgs e)
        {
            if (e.Processor != null && fHasAttachedItems)
            {
                // edits done in the editor don't need to be checked.
                fLock.EnterReadLock();
                try
                {
                    if (ProcessId(e.NewFrom, (DebugProcessor)e.Processor, "From part of link create") == false)
                    {
                        // process is faster than processid, the link should still be locked during this call, so still in existence.
                        ProcessId(e.NewTo, (DebugProcessor)e.Processor, "To part of link create");
                    }
                }
                finally
                {
                    fLock.ExitReadLock();
                }
            }
        }

        /// <summary>The link removed.</summary>
        /// <param name="e">The e.</param>
        internal void LinkRemoved(LinkChangedEventArgs e)
        {
            if (e.Processor != null && fHasAttachedItems)
            {
                // edits done in the editor don't need to be checked.
                fLock.EnterReadLock();
                try
                {
                    if (ProcessId(e.OldFrom, (DebugProcessor)e.Processor, "From part of link remove") == false)
                    {
                        ProcessId(e.OldTo, (DebugProcessor)e.Processor, "To part of link remove");
                    }
                }
                finally
                {
                    fLock.ExitReadLock();
                }
            }
        }

        /// <summary>Called when a neuron was changed (deleted, modified, created,...)</summary>
        /// <param name="e">The <see cref="NeuronChangedEventArgs"/> instance containing the
        ///     event data.</param>
        internal void NeuronChanged(NeuronChangedEventArgs e)
        {
            if (e.Processor != null && fHasAttachedItems)
            {
                // edits done in the editor don't need to be checked.
                fLock.EnterReadLock();
                try
                {
                    ProcessId(e.OriginalSourceID, (DebugProcessor)e.Processor, "Neuron changed");

                        // we can't use id here cause a meaning change or value change doesn't need a valid id.
                    if (e.Action == BrainAction.Removed)
                    {
                        // when a neuron gets removed from the network, we also make certain it is no longer attached to anything.
                        if (fAttachedItems.ContainsKey(e.OriginalSource.ID))
                        {
                            System.Action<Neuron> iDetach = Detach;

                                // note: we only detach, we don't clean up the list of watches (in case a var got removed), this is done by the WatchCollection, when it removes the watches.
                            iDetach.BeginInvoke(e.OriginalSource, null, null);
                        }
                    }
                }
                finally
                {
                    fLock.ExitReadLock();
                }
            }
        }

        /// <summary>Called when a list (clusters, children, info was changed.</summary>
        /// <param name="e">The <see cref="NeuronListChangedEventArgs"/> instance containing the
        ///     event data.</param>
        internal void ListChanged(NeuronListChangedEventArgs e)
        {
            if (e.Processor != null && fHasAttachedItems)
            {
                // edits done in the editor don't need to be checked.
                if (e.ListType != typeof(ClusterList))
                {
                    var iOwner = e.ListOwner;
                    if (iOwner != null)
                    {
                        fLock.EnterReadLock();
                        try
                        {
                            ProcessId(iOwner.ID, (DebugProcessor)e.Processor, "list changed");
                        }
                        finally
                        {
                            fLock.ExitReadLock();
                        }
                    }
                }
            }
        }

        /// <summary>
        ///     Called when the network was cleared.
        /// </summary>
        internal void NetworkCleared()
        {
            fLock.EnterWriteLock();
            try
            {
                fProcs.Clear();
                fAttachedItems.Clear();
                fVariables.Clear();
                fAttachedFrom.Clear();
                fHasAttachedItems = false;
                fAttachedInfo.Clear();
                fAttachedMeaning.Clear();
                fAttachedSin.Clear();
                fAttachedTo.Clear();
            }
            finally
            {
                fLock.ExitWriteLock();
            }
        }

        #endregion
    }
}