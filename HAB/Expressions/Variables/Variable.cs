// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Variable.cs" company="">
//   
// </copyright>
// <summary>
//   A <see cref="Variable" /> result expression always returns a specific
//   neuron (list). It's initial value is specified in
//   <see cref="JaStDev.HAB.Variable.Value" /> .
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace JaStDev.HAB
{
    /// <summary>
    ///     A <see cref="Variable" /> result expression always returns a specific
    ///     neuron (list). It's initial value is specified in
    ///     <see cref="JaStDev.HAB.Variable.Value" /> .
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         This type of expression can be used to reference somethng and who's
    ///         reference can change during execution.
    ///     </para>
    ///     <para>
    ///         You can also use a variable expression to encapsulate other expressions
    ///         if you want to work on the expression object itself instead of executing
    ///         it. For instance, if you want to do a search in the actions lists of a
    ///         result expression, it will be executed and it's result will be searched.
    ///         To prevent this, incapsulate it in this type of expression.
    ///     </para>
    /// </remarks>
    [NeuronID((ulong)PredefinedNeurons.Variable, typeof(Neuron))]
    [NeuronID((ulong)PredefinedNeurons.Value, typeof(Neuron))]
    [NeuronID((ulong)PredefinedNeurons.SplitReaction, typeof(Neuron))]
    [NeuronID((ulong)PredefinedNeurons.Duplicate, typeof(Neuron))]
    [NeuronID((ulong)PredefinedNeurons.Copy, typeof(Neuron))]
    [NeuronID((ulong)PredefinedNeurons.shared, typeof(Neuron))]
    public class Variable : SimpleResultExpression
    {
        #region fields

        /// <summary>The f work data.</summary>
        private VarWorkData fWorkData;

        #endregion

        #region TypeOfNeuron

        /// <summary>
        ///     Gets the type of this neuron expressed as a Neuron.
        /// </summary>
        /// <value>
        ///     <see cref="JaStDev.HAB.PredefinedNeurons.Variable" /> .
        /// </value>
        public override ulong TypeOfNeuronID
        {
            get
            {
                return (ulong)PredefinedNeurons.Variable;
            }
        }

        #endregion

        /// <summary>
        ///     Gets/sets the initial value assigned to the variable if there is no
        ///     value assigned when it is used. This can be an expression but doesn't
        ///     have to be.
        /// </summary>
        public Neuron Value
        {
            get
            {
                return FindFirstOut((ulong)PredefinedNeurons.Value);
            }

            set
            {
                SetFirstOutgoingLinkTo((ulong)PredefinedNeurons.Value, value);
            }
        }

        /// <summary>
        ///     gets a value that indicates if the content of this variable should be
        ///     interpreted as a single int.
        /// </summary>
        public bool AsInt
        {
            get
            {
                if (fWorkData == null)
                {
                    LoadWorkData();
                }

                return fWorkData.fType == (ulong)PredefinedNeurons.IntNeuron;
            }
        }

        /// <summary>
        ///     gets a value that indicates if the content of this variable should be
        ///     interpreted as a single double.
        /// </summary>
        public bool AsDouble
        {
            get
            {
                if (fWorkData == null)
                {
                    LoadWorkData();
                }

                return fWorkData.fType == (ulong)PredefinedNeurons.DoubleNeuron;
            }
        }

        /// <summary>Gets a value indicating whether as bool.</summary>
        public bool AsBool
        {
            get
            {
                if (fWorkData == null)
                {
                    LoadWorkData();
                }

                return fWorkData.fType == (ulong)PredefinedNeurons.True;
            }
        }

        /// <summary>
        ///     gets the type of the value that this variable contains, if it is
        ///     assigned. This can be double, <see langword="int" /> or
        ///     <see langword="bool" />
        /// </summary>
        public System.Type TypeOfValue
        {
            get
            {
                if (AsBool)
                {
                    return typeof(bool);
                }

                if (AsDouble)
                {
                    return typeof(double);
                }

                if (AsInt)
                {
                    return typeof(int);
                }

                return null;
            }
        }

        #region SplitReaction

        /// <summary>
        ///     Gets/sets the neuron that identifies how this var should be handled
        ///     during a processor split.
        /// </summary>
        /// <remarks>
        ///     <para>
        ///         This can be <see cref="JaStDev.HAB.PredefinedNeurons.Copy" /> ,
        ///         <see cref="JaStDev.HAB.PredefinedNeurons.Duplicate" /> or
        ///         <see cref="JaStDev.HAB.PredefinedNeurons.Empty" /> .
        ///     </para>
        ///     <para>The default is 'copy'.</para>
        /// </remarks>
        public Neuron SplitReaction
        {
            get
            {
                return FindFirstOut((ulong)PredefinedNeurons.SplitReaction);
            }

            set
            {
                SetFirstOutgoingLinkTo((ulong)PredefinedNeurons.SplitReaction, value);
            }
        }

        #endregion

        /// <summary>The get value.</summary>
        /// <param name="proc">The proc.</param>
        internal override void GetValue(Processor proc)
        {
            var iRes = ExtractValue(proc);
            if (iRes != null)
            {
                proc.Mem.ArgumentStack.Peek().AddRange(iRes);
            }
        }

        /// <summary>gets the result of the variable without putting anything on the stack
        ///     but by simply returning the value.</summary>
        /// <param name="proc"></param>
        /// <returns>The <see cref="List"/>.</returns>
        protected internal virtual System.Collections.Generic.List<Neuron> ExtractValue(Processor proc)
        {
            VarValuesList iCopyTo = null;
            if (proc.Mem.VariableValues.Count > 0)
            {
                var iDict = proc.Mem.VariableValues.Peek();
                System.Diagnostics.Debug.Assert(iDict != null);
                if (iDict.TryGetValue(this, out iCopyTo) == false)
                {
                    var iValue = Value;
                    if (iValue != null)
                    {
                        var iExp = iValue as ResultExpression;
                        if (iExp != null)
                        {
                            var iRes = SolveResultExp(iExp, proc);
                            iCopyTo = iDict.Set(this, iRes);
                        }
                        else
                        {
                            iCopyTo = iDict.Set(this, iValue);
                        }
                    }
                    else
                    {
                        iCopyTo = iDict.Add(this);
                    }
                }
            }
            else
            {
                LogService.Log.LogError("Internal error", "Variables list out of sync");
            }

            return iCopyTo.Data;
        }

        /// <summary>gets the value without performing an init. This is used to prepare
        ///     locals. This is why there is also a <see cref="VarValuesList"/>
        ///     returned and not the actual data. This allows for faster setting of
        ///     the new value.</summary>
        /// <param name="proc"></param>
        /// <returns>The <see cref="VarValuesList"/>.</returns>
        internal VarValuesList ExtractValueNoInit(Processor proc)
        {
            VarValuesList iCopyTo = null;
            if (proc.Mem.VariableValues.Count > 0)
            {
                var iDict = proc.Mem.VariableValues.Peek();
                System.Diagnostics.Debug.Assert(iDict != null);
                if (iDict.TryGetValue(this, out iCopyTo))
                {
                    return iCopyTo;
                }
            }

            return null;
        }

        /// <summary>returns if this object can return an int.</summary>
        /// <returns>The <see cref="bool"/>.</returns>
        public override bool CanGetInt()
        {
            return AsInt;
        }

        /// <summary>Gets the <see langword="int"/> value.</summary>
        /// <param name="processor"></param>
        /// <returns>The <see cref="int"/>.</returns>
        public override int GetInt(Processor processor)
        {
            var iRes = ExtractValue(processor);
            if (iRes != null && iRes.Count > 0)
            {
                var iFirst = iRes[0] as IntNeuron;
                if (iFirst != null)
                {
                    return iFirst.Value;
                }

                LogService.Log.LogError(
                    "Variable", 
                    string.Format("variable {0} is marked as an integer: invalid content detected.", ID));
            }
            else
            {
                LogService.Log.LogError(
                    "Variable", 
                    string.Format("variable {0} is marked as an integer: invalid content detected.", ID));
            }

            return 0;
        }

        /// <summary>returns if this object can return an double.</summary>
        /// <returns>The <see cref="bool"/>.</returns>
        public override bool CanGetDouble()
        {
            return AsDouble;
        }

        /// <summary>Gets the <see langword="double"/> value.</summary>
        /// <param name="processor"></param>
        /// <returns>The <see cref="double"/>.</returns>
        public override double GetDouble(Processor processor)
        {
            var iRes = ExtractValue(processor);
            if (iRes.Count > 0)
            {
                var iFirst = iRes[0] as DoubleNeuron;
                if (iFirst != null)
                {
                    return iFirst.Value;
                }

                LogService.Log.LogError("Variable", "variable is marked as a double: invalid content detected.");
            }
            else
            {
                LogService.Log.LogError("Variable", "variable is marked as a double: no content detected.");
            }

            return 0;
        }

        /// <summary>returns if this object can return an bool.</summary>
        /// <returns>The <see cref="bool"/>.</returns>
        public override bool CanGetBool()
        {
            return AsBool;
        }

        /// <summary>gets the <see langword="bool"/> value.</summary>
        /// <param name="processor"></param>
        /// <returns>The <see cref="bool"/>.</returns>
        public override bool GetBool(Processor processor)
        {
            var iRes = ExtractValue(processor);
            if (iRes.Count > 0)
            {
                return iRes[0] == Brain.Current.TrueNeuron;
            }

            return false;
        }

        /// <summary>Gets the processor local value(s) for this variable, without
        ///     initializing it if there was no value yet. This is primarely for
        ///     cloning globals so that an empty global remains empty. It is also for
        ///     display purposes, to make certain that vars aren't accedentilly
        ///     initialized.</summary>
        /// <param name="proc">The proc.</param>
        /// <returns>The <see cref="IEnumerable"/>.</returns>
        public virtual System.Collections.Generic.IEnumerable<Neuron> GetValueWithoutInit(Processor proc)
        {
            VarValuesList iRes = null;
            if (proc != null && proc.Mem.VariableValues.Count > 0)
            {
                var iDict = proc.Mem.VariableValues.Peek();
                System.Diagnostics.Debug.Assert(iDict != null);
                iDict.TryGetValue(this, out iRes);
            }

            if (iRes == null)
            {
                return new System.Collections.Generic.List<Neuron>();
            }

            return iRes.Data;
        }

        /// <summary>The store value.</summary>
        /// <param name="neuron">The neuron.</param>
        /// <param name="processor">The processor.</param>
        public void StoreValue(Neuron neuron, Processor processor)
        {
            var iVal = Factories.Default.NLists.GetBuffer();

                // we get from the current processor and pass along to the next, cause we don't want to change to much in the mem of the new proc in the current thread, it could sync badly?
            iVal.Add(neuron);
            StoreValue(iVal, processor);
        }

        /// <summary>Stores the value.</summary>
        /// <param name="value">The value.</param>
        /// <param name="proc">The proc.</param>
        protected internal virtual void StoreValue(System.Collections.Generic.List<Neuron> value, Processor proc)
        {
            proc.StoreVariableValue(this, value);
        }

        /// <summary>
        ///     Returns a <see cref="string" /> that represents the current
        ///     <see cref="object" /> .
        /// </summary>
        /// <returns>
        ///     A <see cref="string" /> that represents the current
        ///     <see cref="object" /> .
        /// </returns>
        public override string ToString()
        {
            return string.Format("var({0})", ID);
        }

        /// <summary>clears any temp buffers that no longer are valid cause the neuron
        ///     changed.</summary>
        /// <param name="fromGC">The from GC.</param>
        protected override void ClearBuffers(bool fromGC = false)
        {
            base.ClearBuffers(fromGC);
            fWorkData = null;
        }

        /// <summary>Asks the expression to load all the data that it requires for being
        ///     executed. This is used as an optimisation so that the code-data can be
        ///     pre-fetched. This is to solve a problem with slower platforms where
        ///     the first input can be handled slowly.</summary>
        /// <param name="alreadyProcessed">The already Processed.</param>
        protected internal override void LoadCode(System.Collections.Generic.HashSet<Neuron> alreadyProcessed)
        {
            if (Brain.Current.IsInitialized && alreadyProcessed.Contains(this) == false)
            {
                alreadyProcessed.Add(this);
                if (fWorkData == null)
                {
                    LoadWorkData();
                }
            }
        }

        #region internal types

        /// <summary>The var work data.</summary>
        private class VarWorkData
        {
            /// <summary>The f split reaction id.</summary>
            public ulong fSplitReactionID;

            /// <summary>The f type.</summary>
            public ulong fType; // small speed up so we don't need to load it again and again.
        }

        #endregion

        #region ctor

        /// <summary>Initializes a new instance of the <see cref="Variable"/> class. A constructor that initializes the object with a reference to the
        ///     specified <see cref="Neuron"/> .</summary>
        /// <param name="value"></param>
        internal Variable(Neuron value)
        {
            var iNew = new Link(value, this, (ulong)PredefinedNeurons.Value);
        }

        /// <summary>Initializes a new instance of the <see cref="Variable"/> class. 
        ///     default constructor.</summary>
        internal Variable()
        {
        }

        #endregion

        #region SplitReactionID

        /// <summary>
        ///     Gets the ID of the split-reaction neuron. This is a fast, buffered
        ///     (reset when changed) accessor using during processing.
        /// </summary>
        public ulong SplitReactionID
        {
            get
            {
                if (fWorkData == null)
                {
                    LoadWorkData();
                }

                return fWorkData.fSplitReactionID;
            }
        }

        /// <summary>The load work data.</summary>
        private void LoadWorkData()
        {
            var iData = new VarWorkData();
            if (LinksOutIdentifier != null)
            {
                using (var iLinks = LinksOut)
                    foreach (var i in iLinks)
                    {
                        if (i.MeaningID == (ulong)PredefinedNeurons.SplitReaction)
                        {
                            iData.fSplitReactionID = i.ToID;
                        }
                        else if (i.MeaningID == (ulong)PredefinedNeurons.IntNeuron)
                        {
                            iData.fType = (ulong)PredefinedNeurons.IntNeuron;
                        }
                        else if (i.MeaningID == (ulong)PredefinedNeurons.DoubleNeuron)
                        {
                            iData.fType = (ulong)PredefinedNeurons.DoubleNeuron;
                        }
                        else if (i.MeaningID == (ulong)PredefinedNeurons.True)
                        {
                            iData.fType = (ulong)PredefinedNeurons.True;
                        }
                    }
            }

            fWorkData = iData;
        }

        /// <summary>small optimizer, checks if the code is loaded alrady or not. This is
        ///     used to see if a start point needs to be loaded or not, whithout
        ///     having to set up mem all the time.</summary>
        /// <returns>The <see cref="bool"/>.</returns>
        protected internal override bool IsCodeLoaded()
        {
            return fWorkData != null;
        }

        #endregion
    }
}