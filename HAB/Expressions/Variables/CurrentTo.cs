// --------------------------------------------------------------------------------------------------------------------
// <copyright file="CurrentTo.cs" company="">
//   
// </copyright>
// <summary>
//   A variable that returns the neuron in the to part of the link being
//   executed.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace JaStDev.HAB
{
    /// <summary>
    ///     A variable that returns the neuron in the to part of the link being
    ///     executed.
    /// </summary>
    [NeuronID((ulong)PredefinedNeurons.CurrentTo)]
    public class CurrentTo : SystemVariable
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="CurrentTo" /> class.
        /// </summary>
        internal CurrentTo()
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
                return (ulong)PredefinedNeurons.CurrentTo;
            }
        }

        /// <summary>Gets the value.</summary>
        /// <param name="proc">The proc.</param>
        internal override void GetValue(Processor proc)
        {
            if (proc != null && proc.CurrentTo != null)
            {
                // if it's null, don't add to the list, cause a null value causes lots of problems.
                proc.Mem.ArgumentStack.Peek().Add(proc.CurrentTo);
            }
        }

        /// <summary>gets the result of the variable without putting anything on the stack
        ///     but by simply returning the value.</summary>
        /// <param name="proc"></param>
        /// <returns>The <see cref="List"/>.</returns>
        protected internal override System.Collections.Generic.List<Neuron> ExtractValue(Processor proc)
        {
            var iRes = Factories.Default.NLists.GetBuffer();
            if (proc.CurrentTo != null)
            {
                iRes.Add(proc.CurrentTo);
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
            return "CurrentTo";
        }
    }
}