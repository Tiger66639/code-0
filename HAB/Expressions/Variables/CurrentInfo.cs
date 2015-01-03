// --------------------------------------------------------------------------------------------------------------------
// <copyright file="CurrentInfo.cs" company="">
//   
// </copyright>
// <summary>
//   A variable implementation that returns the current info section of the
//   link that is being executed.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace JaStDev.HAB
{
    /// <summary>
    ///     A variable implementation that returns the current info section of the
    ///     link that is being executed.
    /// </summary>
    [NeuronID((ulong)PredefinedNeurons.CurrentInfo)]
    public class CurrentInfo : SystemVariable
    {
        #region ctor

        /// <summary>
        ///     Initializes a new instance of the <see cref="CurrentInfo" /> class.
        /// </summary>
        internal CurrentInfo()
        {
        }

        #endregion

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
                return (ulong)PredefinedNeurons.CurrentInfo;
            }
        }

        /// <summary>Gets the value.</summary>
        /// <param name="proc">The proc.</param>
        internal override void GetValue(Processor proc)
        {
            if (proc != null && proc.CurrentInfo != null)
            {
                proc.Mem.ArgumentStack.Peek().AddRange(proc.CurrentInfo);
            }
        }

        /// <summary>gets the result of the variable without putting anything on the stack
        ///     but by simply returning the value.</summary>
        /// <param name="proc"></param>
        /// <returns>The <see cref="List"/>.</returns>
        protected internal override System.Collections.Generic.List<Neuron> ExtractValue(Processor proc)
        {
            // List<Neuron> iRes = Factories.Default.NLists.GetBuffer();
            // if (proc.CurrentInfo != null)
            // iRes.AddRange(proc.CurrentInfo);
            return proc.CurrentInfo;

                // no need to make a copy of the list, can simply return the list itself cause it's a local copy that the processsor is using.
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
            return "CurrentInfo";
        }
    }
}