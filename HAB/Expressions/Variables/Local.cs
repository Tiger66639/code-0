// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Local.cs" company="">
//   
// </copyright>
// <summary>
//   An expression type that is able to store a list of neurons. The contents
//   of a local remain valid for as long as the current function is running.
//   (will be removed the fastest).
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace JaStDev.HAB
{
    /// <summary>
    ///     An expression type that is able to store a list of neurons. The contents
    ///     of a local remain valid for as long as the current function is running.
    ///     (will be removed the fastest).
    /// </summary>
    [NeuronID((ulong)PredefinedNeurons.Local, typeof(Neuron))]
    public class Local : Variable
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="Local" /> class.
        /// </summary>
        internal Local()
        {
        }

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
                return (ulong)PredefinedNeurons.Local;
            }
        }

        #endregion

        /// <summary>The to string.</summary>
        /// <returns>The <see cref="string"/>.</returns>
        public override string ToString()
        {
            return string.Format("local({0})", ID);
        }
    }
}