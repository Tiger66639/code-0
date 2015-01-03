// --------------------------------------------------------------------------------------------------------------------
// <copyright file="CurrentSin.cs" company="">
//   
// </copyright>
// <summary>
//   This system variable returns the <see cref="Sin" /> that triggered the
//   current processor or for which the processor is currently preparing an
//   output.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace JaStDev.HAB
{
    /// <summary>
    ///     This system variable returns the <see cref="Sin" /> that triggered the
    ///     current processor or for which the processor is currently preparing an
    ///     output.
    /// </summary>
    [NeuronID((ulong)PredefinedNeurons.CurrentSin)]
    public class CurrentSin : SystemVariable
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="CurrentSin" /> class.
        /// </summary>
        internal CurrentSin()
        {
        }

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
                return (ulong)PredefinedNeurons.CurrentSin;
            }
        }

        /// <summary>Gets the value.</summary>
        /// <param name="proc">The proc.</param>
        internal override void GetValue(Processor proc)
        {
            if (proc != null && proc.CurrentSin != null)
            {
                // if it's null, don't add to the list, cause a null value causes lots of problems.
                proc.Mem.ArgumentStack.Peek().Add(proc.CurrentSin);
            }
        }

        /// <summary>gets the result of the variable without putting anything on the stack
        ///     but by simply returning the value.</summary>
        /// <param name="proc"></param>
        /// <returns>The <see cref="List"/>.</returns>
        protected internal override System.Collections.Generic.List<Neuron> ExtractValue(Processor proc)
        {
            var iRes = Factories.Default.NLists.GetBuffer();
            if (proc.CurrentSin != null)
            {
                iRes.Add(proc.CurrentSin);
            }

            return iRes;
        }

        /// <summary>
        ///     Returns a <see cref="System.String" /> that represents the current
        ///     <see cref="System.Object" /> .
        /// </summary>
        /// <returns>
        ///     A <see cref="System.String" /> that represents the current
        ///     <see cref="System.Object" /> .
        /// </returns>
        public override string ToString()
        {
            return "CurrentSin";
        }
    }
}