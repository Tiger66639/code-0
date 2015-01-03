// --------------------------------------------------------------------------------------------------------------------
// <copyright file="GUISin.cs" company="">
//   
// </copyright>
// <summary>
//   The gui sin.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace JaStDev.HAB
{
    /// <summary>The gui sin.</summary>
    [NeuronID((ulong)PredefinedNeurons.GuiSin, typeof(Neuron))]
    public class GUISin : Sin
    {
        #region TypeOfNeuron

        /// <summary>
        ///     Gets the type of this neuron expressed as a Neuron.
        /// </summary>
        /// <value>
        ///     <see cref="JaStDev.HAB.PredefinedNeurons.TextNeuron" /> .
        /// </value>
        public override ulong TypeOfNeuronID
        {
            get
            {
                return (ulong)PredefinedNeurons.GuiSin;
            }
        }

        #endregion

        /// <summary>Tries to translate the specified neuron to the output type of the<see cref="Sin"/> and send it to the outside world.</summary>
        /// <param name="toSend"></param>
        public override void Output(System.Collections.Generic.IList<Neuron> toSend)
        {
            throw new System.NotImplementedException();
        }

        /// <summary>
        ///     Called when the data needs to be saved.
        /// </summary>
        public override void Flush()
        {
            throw new System.NotImplementedException();
        }
    }
}