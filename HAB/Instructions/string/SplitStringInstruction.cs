// --------------------------------------------------------------------------------------------------------------------
// <copyright file="SplitStringInstruction.cs" company="">
//   
// </copyright>
// <summary>
//   Returns the content of the argument (a textneuron) split into all the
//   words, numbers, signs that are found in the text.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace JaStDev.HAB
{
    /// <summary>
    ///     Returns the content of the argument (a textneuron) split into all the
    ///     words, numbers, signs that are found in the text.
    /// </summary>
    /// <remarks>
    ///     <para>Arguments:</para>
    ///     <list type="bullet">
    ///         <item>
    ///             <description>
    ///                 the textneuron from which to return the content as single tokens.
    ///             </description>
    ///         </item>
    ///     </list>
    /// </remarks>
    [NeuronID((ulong)PredefinedNeurons.SplitStringInstruction)]
    public class SplitStringInstruction : MultiResultInstruction
    {
        #region TypeOfNeuron

        /// <summary>
        ///     Gets the type of this neuron expressed as a Neuron.
        /// </summary>
        /// <value>
        ///     <see cref="JaStDev.HAB.PredefinedNeurons.SplitStringInstruction" /> .
        /// </value>
        public override ulong TypeOfNeuronID
        {
            get
            {
                return (ulong)PredefinedNeurons.SplitStringInstruction;
            }
        }

        #endregion

        /// <summary>
        ///     Returns the number of arguments that are required by this instruction.
        /// </summary>
        /// <remarks>
        ///     A value of -1 indicates that a list of neurons is allowed, without any
        ///     specific number of values.
        /// </remarks>
        /// <value>
        /// </value>
        public override int ArgCount
        {
            get
            {
                return 1;
            }
        }

        /// <summary>performs the task and returns it's result.</summary>
        /// <remarks>Note: when When a result instruction is executed, ( so<see cref="Instruction.Execute"/> is called instead of<see cref="ResultInstruction.GetValues"/> , the result value(s) are
        ///     pushed on the execution stack.</remarks>
        /// <param name="processor"></param>
        /// <param name="list">The list of arguments</param>
        public override void GetValues(Processor processor, System.Collections.Generic.IList<Neuron> list)
        {
            if (list != null && list.Count >= 1)
            {
                var iText = list[0] as TextNeuron;
                if (iText != null)
                {
                    var iSplitter = new Parsers.Tokenizer(iText.Text);
                    iSplitter.AllowEscaping = false;
                    var iRes = processor.Mem.ArgumentStack.Peek();
                    while (iSplitter.CurrentToken != Parsers.Token.End)
                    {
                        Neuron iToAdd = null;
                        switch (iSplitter.WordType)
                        {
                            case Parsers.WordTokenType.Word:
                                iToAdd = TextNeuron.GetFor(iSplitter.CurrentValue);
                                break;
                            case Parsers.WordTokenType.Integer:
                                iToAdd = NeuronFactory.GetInt(int.Parse(iSplitter.CurrentValue));
                                break;
                            case Parsers.WordTokenType.Decimal:
                                iToAdd = NeuronFactory.GetDouble(double.Parse(iSplitter.CurrentValue));
                                break;
                            default:
                                break;
                        }

                        if (iToAdd != null)
                        {
                            iRes.Add(iToAdd);
                        }

                        iSplitter.GetNext();
                    }
                }
                else
                {
                    LogService.Log.LogError("GetChildren.GetValues", "First argument should be a TextNeuron!");
                }
            }
            else
            {
                LogService.Log.LogError("GetChildren.GetValues", "No arguments specified");
            }
        }
    }
}