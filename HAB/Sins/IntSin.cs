// --------------------------------------------------------------------------------------------------------------------
// <copyright file="IntSin.cs" company="">
//   
// </copyright>
// <summary>
//   A sensory interface that provides 'facial' information.
//   At the moment, it only excepts int neurons being send for output. The int is raised in an event.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace JaStDev.HAB
{
    using System.Linq;

    /// <summary>
    ///     A sensory interface that provides 'facial' information.
    ///     At the moment, it only excepts int neurons being send for output. The int is raised in an event.
    /// </summary>
    [NeuronID((ulong)PredefinedNeurons.IntSin, typeof(Neuron))]
    public class IntSin : Sin
    {
        #region Fields

        /// <summary>The f buffer.</summary>
        private System.Collections.Generic.List<Neuron> fBuffer;

        #endregion

        /// <summary>
        ///     Need to provide an implemntation, does nothing, check <see cref="TextSin.SaveDict" /> for more info.
        /// </summary>
        public override void Flush()
        {
        }

        #region Events

        /// <summary>
        ///     Raised when the FaceSin has got some output.
        /// </summary>
        public event OutputEventHandler<int> IntOut;

        /// <summary>
        ///     Raised when there is a list of ints to send out. this can be used to request a selection from a list of items.
        /// </summary>
        public event OutputEventHandler<System.Collections.Generic.List<int>> IntsOut;

        #endregion

        #region Functions

        #region TypeOfNeuron

        /// <summary>
        ///     Gets the type of this neuron expressed as a Neuron.
        /// </summary>
        /// <value><see cref="PredefinedNeurons.TextNeuron" />.</value>
        public override ulong TypeOfNeuronID
        {
            get
            {
                return (ulong)PredefinedNeurons.IntSin;
            }
        }

        #endregion

        /// <summary>Tries to translate the specified neuron to the output type of the Sin and send it to the outside world.</summary>
        /// <param name="toSend">The to Send.</param>
        /// <remarks><para>will only perform an output if there are objects monitoring the <see cref="TextSin.TextOut"/> event.</para>
        /// <para>This method is called by the <see cref="Brain"/> itself during/after processing of input.</para>
        /// <para>Allowed output values are:
        ///         - Any value neuron, like TextNeuron (-&gt; text is sent), DoubleNeuron or IntNeuron (-&gt; value is sent).
        ///         - An object cluster, in that case, the 'TextNeuron' is extracted</para>
        /// </remarks>
        public override void Output(System.Collections.Generic.IList<Neuron> toSend)
        {
            foreach (var i in toSend)
            {
                if (i.ID == (ulong)PredefinedNeurons.BeginTextBlock)
                {
                    // need to start a new block, check if we need to close a previous one first.
                    OutputInts();
                    fBuffer = new System.Collections.Generic.List<Neuron>();
                }
                else if (i.ID == (ulong)PredefinedNeurons.EndTextBlock)
                {
                    // need to close the current block: send the event + close the block.
                    OutputInts();
                    fBuffer = null;
                }
                else
                {
                    var iVal = toSend as IntNeuron;
                    if (iVal != null)
                    {
                        if (fBuffer != null)
                        {
                            fBuffer.Add(iVal);
                        }
                        else
                        {
                            OutputInt(iVal.Value);
                        }
                    }
                }
            }
        }

        /// <summary>
        ///     Outputs the ints.
        /// </summary>
        private void OutputInts()
        {
            if (fBuffer != null && IntsOut != null)
            {
                var iValues = from i in fBuffer select ((IntNeuron)i).Value;
                IntsOut(
                    this, 
                    new OutputEventArgs<System.Collections.Generic.List<int>>
                        {
                            Data = fBuffer, 
                            Value =
                                new System.Collections.Generic.List
                                <int>(iValues)
                        });
            }
        }

        /// <summary>Outputs the int.</summary>
        /// <param name="value">The value.</param>
        private void OutputInt(int value)
        {
            if (IntOut != null)
            {
                IntOut(this, new OutputEventArgs<int> { Value = value });
            }
        }

        #endregion
    }
}