// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ReturnValue.cs" company="">
//   
// </copyright>
// <summary>
//   This system variable returns the value that was returned from the last
//   cluster call.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace JaStDev.HAB
{
    /// <summary>
    ///     This system variable returns the value that was returned from the last
    ///     cluster call.
    /// </summary>
    [NeuronID((ulong)PredefinedNeurons.ReturnValue)]
    public class ReturnValue : SystemVariable
    {
        /// <summary>Initializes a new instance of the <see cref="ReturnValue"/> class. 
        ///     Initializes a new instance of the <see cref="CurrentSin"/> class.</summary>
        internal ReturnValue()
        {
        }

        /// <summary>
        ///     Gets the type of this neuron expressed as a Neuron.
        /// </summary>
        /// <value>
        ///     <see cref="JaStDev.HAB.PredefinedNeurons.ReturnValue" /> .
        /// </value>
        public override ulong TypeOfNeuronID
        {
            get
            {
                return (ulong)PredefinedNeurons.ReturnValue;
            }
        }

        /// <summary>Gets the value.</summary>
        /// <param name="proc">The proc.</param>
        internal override void GetValue(Processor proc)
        {
            if (proc != null && proc.Mem.LastReturnValues.Count > 0)
            {
                var iItems = proc.Mem.LastReturnValues.Pop();
                if (proc.Mem.ArgumentStack.Count > 0)
                {
                    // a return could be called when there is no one waiting for the result (to remove the result)
                    proc.Mem.ArgumentStack.Peek().AddRange(iItems);
                }

                Factories.Default.NLists.Recycle(iItems, false);
            }
            else
            {
                LogService.Log.LogError("Return value", "no return values available");
            }
        }

        /// <summary>Gets the processor local value(s) for this variable, without
        ///     initializing it if there was no value yet. This is primarely for
        ///     display purposes, to make certain that vars aren't accedentilly
        ///     initialized.</summary>
        /// <param name="proc">The proc.</param>
        /// <returns>The same value as <see cref="Variable.GetValue"/> since this type of
        ///     var doesn't need initializing, it always has a value.</returns>
        public override System.Collections.Generic.IEnumerable<Neuron> GetValueWithoutInit(Processor proc)
        {
            var iRes = new System.Collections.Generic.List<Neuron>();
            if (proc != null && proc.Mem.LastReturnValues.Count > 0)
            {
                var iItems = proc.Mem.LastReturnValues.Peek();
                iRes.AddRange(iItems);
            }
            else
            {
                LogService.Log.LogError("Return value", "no return values available");
            }

            return iRes;
        }

        /// <summary>gets the result of the variable without putting anything on the stack
        ///     but by simply returning the value.</summary>
        /// <param name="proc"></param>
        /// <returns>The <see cref="List"/>.</returns>
        protected internal override System.Collections.Generic.List<Neuron> ExtractValue(Processor proc)
        {
            if (Processor.CurrentProcessor != null)
            {
                // if this thread is running a processor, we are returning the value for a processor, otherwise, we are returning the value for debugging. When for a processor, also remove the value from the stack, otherwise leaf it where it is.
                if (proc.Mem.LastReturnValues.Count > 0)
                {
                    return proc.Mem.LastReturnValues.Pop();
                }
            }
            else if (proc.Mem.LastReturnValues.Count > 0)
            {
                return proc.Mem.LastReturnValues.Peek();
            }

            LogService.Log.LogError("Return value", "no return values available");

                // when we get here, something went wrong.
            return null;
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
            return "ReturnValue";
        }
    }
}