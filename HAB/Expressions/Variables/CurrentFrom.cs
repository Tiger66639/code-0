// --------------------------------------------------------------------------------------------------------------------
// <copyright file="CurrentFrom.cs" company="">
//   
// </copyright>
// <summary>
//   A variable that returns the neuron in the from part of the link being
//   executed.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace JaStDev.HAB
{
    /// <summary>
    ///     A variable that returns the neuron in the from part of the link being
    ///     executed.
    /// </summary>
    [NeuronID((ulong)PredefinedNeurons.CurrentFrom)]
    public class CurrentFrom : SystemVariable
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="CurrentFrom" /> class.
        /// </summary>
        internal CurrentFrom()
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
                return (ulong)PredefinedNeurons.CurrentFrom;
            }
        }

        /// <summary>Gets the value.</summary>
        /// <param name="proc">The proc.</param>
        internal override void GetValue(Processor proc)
        {
            if (proc != null && proc.NeuronToSolve != null)
            {
                // if it's null, don't add to the list, cause a null value causes lots of problems.
                proc.Mem.ArgumentStack.Peek().Add(proc.NeuronToSolve);
            }
        }

        /// <summary>gets the result of the variable without putting anything on the stack
        ///     but by simply returning the value.</summary>
        /// <param name="proc"></param>
        /// <returns>The <see cref="List"/>.</returns>
        protected internal override System.Collections.Generic.List<Neuron> ExtractValue(Processor proc)
        {
            var iRes = Factories.Default.NLists.GetBuffer();
            if (proc.NeuronToSolve != null)
            {
                iRes.Add(proc.NeuronToSolve);
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
            return "CurrentFrom";
        }
    }
}