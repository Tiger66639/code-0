// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ProcessorSplitter.cs" company="">
//   
// </copyright>
// <summary>
//   The processor splitter.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace JaStDev.HAB
{
    using Enumerable = System.Linq.Enumerable;

    /// <summary>The processor splitter.</summary>
    public class ProcessorSplitter
    {
        /// <summary>The split.</summary>
        /// <param name="source">The source.</param>
        /// <param name="res">The res.</param>
        public void Split(Processor source, Processor[] res)
        {
            fSourceStack = Factories.Default.NLists.GetBuffer();
            fSourceStack.AddRange(Enumerable.Reverse(source.NeuronStack));

                // we need a temp list cause all copy functions of stack, do a copy last in, first out, so similar as pop order.
            Frames = new System.Collections.Generic.List<CallFrame>(Enumerable.Reverse(source.CallFrameStack));

                // same as NeuronStack.
            Source = source;
            Processors = res;
            CloneNeurons();
        }

        /// <summary>
        ///     <see cref="Clones" /> all the neurons that need to be cloned from the
        ///     source and stores them temp here in the splitter object.
        /// </summary>
        private void CloneNeurons()
        {
            CloneCurrentPos();
            CloneNeuronStack();
            CloneGlobals();
            CloneVariables();
        }

        /// <summary>
        ///     all the variables that are registered as 'duplicate', should also make
        ///     certain that their content is duplicated. There is no need to store
        ///     the results in a custom place, other the the list of Clones, cause
        ///     while processing the split results, variables values are always
        ///     recomposed, based on the content of the 'clones' dictionary.
        /// </summary>
        /// <remarks>
        /// </remarks>
        private void CloneVariables()
        {
            for (var counter = 0; counter < SourceVariableValues.Count; counter++)
            {
                foreach (var i in SourceVariableValues[counter])
                {
                    // we need to make certain that the content of the variables is updated so that they contain the clones of the stack content.
                    if (i.Key.SplitReactionID == (ulong)PredefinedNeurons.Duplicate)
                    {
                        foreach (var iNeuron in i.Value.Data)
                        {
                            DuplicateNeuron(iNeuron);
                        }
                    }
                }
            }

            foreach (var iFrame in Source.CallFrameStack)
            {
                // make certain that the content of the callframe stack is also duplicated (for the locals). 
                foreach (var i in iFrame.LocalsBuffer)
                {
                    if (i.Key.SplitReactionID == (ulong)PredefinedNeurons.Duplicate && i.Value != null)
                    {
                        // i.value can be null when the var just got initialized.
                        foreach (var iNeuron in i.Value)
                        {
                            DuplicateNeuron(iNeuron);
                        }
                    }
                }
            }
        }

        /// <summary>The duplicate neuron.</summary>
        /// <param name="iNeuron">The i neuron.</param>
        /// <returns>The <see cref="List"/>.</returns>
        private System.Collections.Generic.List<Neuron> DuplicateNeuron(Neuron iNeuron)
        {
            System.Collections.Generic.List<Neuron> iClone;
            if (Clones.TryGetValue(iNeuron, out iClone) == false)
            {
                if (iNeuron.ID != Neuron.TempId)
                {
                    if (Processors.Length < 2)
                    {
                        iClone = Factories.Default.NLists.GetBuffer();
                        iClone.Add(iNeuron.Duplicate());
                    }
                    else
                    {
                        var iDuplicator = new MultiDuplicator();
                        iClone = iDuplicator.Duplicate(iNeuron, Processors.Length);
                    }
                }
                else
                {
                    var iType = iNeuron.GetType();
                    iClone = Factories.Default.NLists.GetBuffer();
                    for (var u = 0; u < Processors.Length; u++)
                    {
                        var iToAdd = NeuronFactory.Get(iType);
                        iNeuron.CopyTo(iToAdd);
                        Brain.Current.MakeTemp(iToAdd);
                        iClone.Add(iToAdd);
                    }
                }

                Clones.Add(iNeuron, iClone); // we need to store all the copies as well.
            }

            return iClone;
        }

        /// <summary>The clone neuron stack.</summary>
        private void CloneNeuronStack()
        {
            foreach (var iNeuron in fSourceStack)
            {
                var iClone = DuplicateNeuron(iNeuron);
                fTargetStacks.Add(iClone);
            }
        }

        /// <summary>The clone current pos.</summary>
        private void CloneCurrentPos()
        {
            if (Source.NeuronToSolve != null && Source.NeuronToSolve.ID != Neuron.EmptyId)
            {
                if (Clones.TryGetValue(Source.NeuronToSolve, out fTargetNeuronToSolve) == false)
                {
                    // could already have been duplicated.
                    if (Processors.Length > 1)
                    {
                        var iDup = new MultiDuplicator();

                            // only use the multidpupliator when it's faster, otherwise use the single duplicator.
                        fTargetNeuronToSolve = iDup.Duplicate(Source.NeuronToSolve, Processors.Length);
                    }
                    else
                    {
                        var iRes = Factories.Default.NLists.GetBuffer();
                        iRes.Add(Source.NeuronToSolve.Duplicate());
                        fTargetNeuronToSolve = iRes;
                    }

                    Clones[Source.NeuronToSolve] = fTargetNeuronToSolve;
                }
            }
            else
            {
                fTargetNeuronToSolve = Factories.Default.NLists.GetBuffer();

                    // when there is no target to duplicate, we need to provide enough nulls to assign.
                if (fTargetNeuronToSolve.Capacity < Processors.Length)
                {
                    fTargetNeuronToSolve.Capacity = Processors.Length;
                }

                for (var i = 0; i < Processors.Length; i++)
                {
                    fTargetNeuronToSolve.Add(null);
                }
            }
        }

        /// <summary>
        ///     <see cref="Clones" /> the global values to this dictionary, taking into
        ///     account the type of cloning specified in the global.
        /// </summary>
        /// <param name="source">The source.</param>
        private void CloneGlobals()
        {
            foreach (var i in Source.GlobalValues)
            {
                var iGlobal = i.Key;
                if (iGlobal != null)
                {
                    var iSplitReaction = iGlobal.SplitReactionID;
                    if (iSplitReaction == (ulong)PredefinedNeurons.Copy)
                    {
                        if (TargetCopiedGlobals == null)
                        {
                            TargetCopiedGlobals =
                                new System.Collections.Generic.List
                                    <
                                        System.Collections.Generic.KeyValuePair
                                            <Global, System.Collections.Generic.List<Neuron>>>();
                        }

                        TargetCopiedGlobals.Add(
                            new System.Collections.Generic.KeyValuePair<Global, System.Collections.Generic.List<Neuron>>
                                (iGlobal, iGlobal.ExtractValue(Source)));

                            // we simply get the value, don't make a copy of the list, cause when the data is copied into the processors, each processor has to get a unique list anyway, so it does it's own copying.
                    }

                    if (iSplitReaction == (ulong)PredefinedNeurons.shared)
                    {
                        if (TargetSharedGlobals == null)
                        {
                            TargetSharedGlobals =
                                new System.Collections.Generic.List
                                    <
                                        System.Collections.Generic.KeyValuePair
                                            <Global, System.Collections.Generic.List<Neuron>>>();
                        }

                        TargetSharedGlobals.Add(
                            new System.Collections.Generic.KeyValuePair<Global, System.Collections.Generic.List<Neuron>>
                                (iGlobal, iGlobal.ExtractValue(Source)));

                            // we simply get the value, don't make a copy of the list, cause when the data is copied into the processors, each processor has to get a unique list anyway, so it does it's own copying.
                    }
                    else if (iSplitReaction == (ulong)PredefinedNeurons.Duplicate)
                    {
                        if (TargetClonedGlobals == null)
                        {
                            TargetClonedGlobals =
                                new System.Collections.Generic.List
                                    <
                                        System.Collections.Generic.KeyValuePair
                                            <Global, 
                                                System.Collections.Generic.List
                                                    <System.Collections.Generic.IList<Neuron>>>>();
                        }

                        DuplicateGlobal(iGlobal);
                    }

                    // in all other cases, we leave the global empty so that a new value is created.
                }
            }
        }

        /// <summary>Duplicates the content of the global and stores it in the global
        ///     values dict.</summary>
        /// <param name="value">The value.</param>
        protected virtual void DuplicateGlobal(Global value)
        {
            var iList = value.GetValueWithoutInit(Source);
            if (iList != null)
            {
                var iMemFac = Factories.Default;
                var iNewVals = new System.Collections.Generic.List<System.Collections.Generic.IList<Neuron>>();
                foreach (var i in iList)
                {
                    if (i.ID != Neuron.EmptyId)
                    {
                        System.Collections.Generic.List<Neuron> iClone;
                        if (Clones.TryGetValue(i, out iClone) == false)
                        {
                            // if the item has already been cloned, don't need to do it again.
                            if (i.ID != Neuron.TempId)
                            {
                                if (Processors.Length < 2)
                                {
                                    iClone = iMemFac.NLists.GetBuffer();
                                    iClone.Add(i.Duplicate());
                                }
                                else
                                {
                                    var iDuplicator = new MultiDuplicator();
                                    iClone = iDuplicator.Duplicate(i, Processors.Length);
                                }
                            }
                            else
                            {
                                var iType = i.GetType();
                                iClone = iMemFac.NLists.GetBuffer();
                                if (iClone.Capacity < Processors.Length)
                                {
                                    iClone.Capacity = Processors.Length;
                                }

                                for (var count = 0; count < Processors.Length; count++)
                                {
                                    var iToAdd = NeuronFactory.Get(iType);
                                    i.CopyTo(iToAdd);
                                    Brain.Current.MakeTemp(iToAdd);
                                    iClone.Add(iToAdd);
                                }
                            }

                            Clones[i] = iClone; // we need to store all the copies as well.
                        }

                        iNewVals.Add(iClone);
                    }
                }

                TargetClonedGlobals.Add(
                    new System.Collections.Generic.KeyValuePair
                        <Global, System.Collections.Generic.List<System.Collections.Generic.IList<Neuron>>>(
                        value, 
                        iNewVals));
            }
        }

        /// <summary>
        ///     should be called when the split process is done: releases all the
        ///     memory so that it can be reused.
        /// </summary>
        internal void Recycle()
        {
            var iMemFac = Factories.Default;
            foreach (var i in Clones)
            {
                iMemFac.NLists.Recycle(i.Value);
            }

            fClones = null;
            iMemFac.NLists.Recycle(fSourceStack);
            fSourceStack = null;

            // if (Source.NeuronToSolve == null ||Source.NeuronToSolve.ID == Neuron.EmptyId)
            // Source.Mem.NListFactory.Release(fTargetNeuronToSolve);
            fTargetNeuronToSolve = null;
        }

        #region Fields

        /// <summary>The f source stack.</summary>
        private System.Collections.Generic.List<Neuron> fSourceStack;

        /// <summary>The frames.</summary>
        public System.Collections.Generic.List<CallFrame> Frames;

        /// <summary>The f clones.</summary>
        private System.Collections.Generic.Dictionary<Neuron, System.Collections.Generic.List<Neuron>> fClones =
            new System.Collections.Generic.Dictionary<Neuron, System.Collections.Generic.List<Neuron>>();

        /// <summary>The f target stacks.</summary>
        private readonly System.Collections.Generic.List<System.Collections.Generic.List<Neuron>> fTargetStacks =
            new System.Collections.Generic.List<System.Collections.Generic.List<Neuron>>();

        /// <summary>The f target neuron to solve.</summary>
        private System.Collections.Generic.List<Neuron> fTargetNeuronToSolve;

        #endregion

        #region Prop

        #region TargetStacks

        /// <summary>
        ///     Gets the stack for each target.
        /// </summary>
        /// <value>
        ///     The target stacks.
        /// </value>
        public System.Collections.Generic.List<System.Collections.Generic.List<Neuron>> TargetStacks
        {
            get
            {
                return fTargetStacks;
            }
        }

        #endregion

        #region TargetNeuronToSolve

        /// <summary>
        ///     Gets the NeuronToSolve for each processor to clone.
        /// </summary>
        public System.Collections.Generic.List<Neuron> TargetNeuronToSolve
        {
            get
            {
                return fTargetNeuronToSolve;
            }

            internal set
            {
                fTargetNeuronToSolve = value;
            }
        }

        #endregion

        #region TargetClonedGlobals

        /// <summary>
        ///     Gets the list of globals that were cloned
        /// </summary>
        public System.Collections.Generic.List<System.Collections.Generic.KeyValuePair<Global, System.Collections.Generic.List<System.Collections.Generic.IList<Neuron>>>> TargetClonedGlobals { get; internal set; }

        #endregion

        #region TargetCopiedGlobals

        /// <summary>
        ///     Gets the list of globals that were copied
        /// </summary>
        public System.Collections.Generic.List<System.Collections.Generic.KeyValuePair<Global, System.Collections.Generic.List<Neuron>>> TargetCopiedGlobals { get; internal set; }

        #endregion

        #region TargetSharedGlobals

        /// <summary>
        ///     Gets the list of globals that were copied
        /// </summary>
        public System.Collections.Generic.List<System.Collections.Generic.KeyValuePair<Global, System.Collections.Generic.List<Neuron>>> TargetSharedGlobals { get; internal set; }

        #endregion

        #region Source

        /// <summary>
        ///     Gets the source processor that triggered the split.
        /// </summary>
        /// <value>
        ///     The source.
        /// </value>
        public Processor Source { get; private set; }

        #region Clones

        /// <summary>
        ///     A dictionary containing all the clones that were made during the split
        ///     operation.
        /// </summary>
        /// <remarks>
        ///     This is a dictionary since it is mostly used for looking up neurons,
        ///     to see if they are cloned. It maps from object to lis of objects so
        ///     that we can also find objects again for each processor.
        /// </remarks>
        public System.Collections.Generic.Dictionary<Neuron, System.Collections.Generic.List<Neuron>> Clones
        {
            get
            {
                return fClones;
            }
        }

        #endregion

        #endregion

        #region SourceVariableValues

        /// <summary>
        ///     Gets/sets a copy of the variable values found in the source. This is
        ///     used to copy them over to the others.
        /// </summary>
        public System.Collections.Generic.List<VarDict> SourceVariableValues { get; set; }

        #endregion

        #region Processors

        /// <summary>
        ///     Gets the processors that were created during the duplication process.
        /// </summary>
        /// <value>
        ///     The processors.
        /// </value>
        public Processor[] Processors { get; private set; }

        #endregion

        #endregion
    }
}