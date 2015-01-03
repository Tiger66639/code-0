// --------------------------------------------------------------------------------------------------------------------
// <copyright file="NewInstruction.cs" company="">
//   
// </copyright>
// <summary>
//   Creates new neurons.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace JaStDev.HAB
{
    /// <summary>
    ///     Creates new neurons.
    /// </summary>
    /// <remarks>
    ///     Expects 1 (or 2) parameters: the type of neuron that should be created.
    ///     And optionally, for <see langword="int" /> and DoubleNeurons an initial
    ///     value. The neuron that is returned is only registered as a temp neuron,
    ///     so it is not permanent, but if it is somehow stored permanetly, it's id
    ///     is updated. This allows for temporary values during code execution.
    /// </remarks>
    [NeuronID((ulong)PredefinedNeurons.NewInstruction)]
    public class NewInstruction : SingleResultInstruction
    {
        #region TypeOfNeuron

        /// <summary>
        ///     Gets the type of this neuron expressed as a Neuron.
        /// </summary>
        /// <value>
        ///     <see cref="JaStDev.HAB.PredefinedNeurons.NewInstruction" /> .
        /// </value>
        public override ulong TypeOfNeuronID
        {
            get
            {
                return (ulong)PredefinedNeurons.NewInstruction;
            }
        }

        #endregion

        /// <summary>
        ///     Returns the number of arguments that are required by this instruction.
        /// </summary>
        /// <remarks>
        ///     <para>
        ///         A value of -1 indicates that a list of neurons is allowed, without any
        ///         specific number of values.
        ///     </para>
        ///     <para>-1 is returned, but actually only 1 or 2 are allowed.</para>
        /// </remarks>
        /// <value>
        /// </value>
        public override int ArgCount
        {
            get
            {
                return -1;
            }
        }

        /// <summary>Creates a new neuron and returns this.</summary>
        /// <remarks>Expects 1 argument that defines which neuron to create. This should be
        ///     one of the predefined items:</remarks>
        /// <param name="processor">The processor.</param>
        /// <param name="list">Contains all the arguments that were passed along.</param>
        /// <returns>The <see cref="Neuron"/>.</returns>
        protected override Neuron InternalGetValue(Processor processor, System.Collections.Generic.IList<Neuron> list)
        {
            Neuron iRes = null;
            if (list != null && list.Count >= 1 && list[0] != null)
            {
                var iType = list[0];
                if (iType.ID == (ulong)PredefinedNeurons.Neuron)
                {
                    iRes = NeuronFactory.GetNeuron();
                }
                else if (iType.ID == (ulong)PredefinedNeurons.NeuronCluster)
                {
                    iRes = NeuronFactory.GetCluster();
                }
                else if (iType.ID == (ulong)PredefinedNeurons.DoubleNeuron)
                {
                    if (list.Count > 1 && list[1] is DoubleNeuron)
                    {
                        iRes = NeuronFactory.GetDouble(((DoubleNeuron)list[1]).Value);
                    }
                    else
                    {
                        iRes = NeuronFactory.GetDouble();
                    }
                }
                else if (iType.ID == (ulong)PredefinedNeurons.IntNeuron)
                {
                    if (list.Count > 1 && list[1] is IntNeuron)
                    {
                        iRes = NeuronFactory.GetInt(((IntNeuron)list[1]).Value);
                    }
                    else
                    {
                        iRes = NeuronFactory.GetInt();
                    }
                }
                else if (iType.ID == (ulong)PredefinedNeurons.TextNeuron)
                {
                    iRes = NeuronFactory.Get<TextNeuron>();
                }
                else if (iType.ID == (ulong)PredefinedNeurons.BoolExpression)
                {
                    iRes = NeuronFactory.Get<BoolExpression>();
                }
                else if (iType.ID == (ulong)PredefinedNeurons.ConditionalPart)
                {
                    iRes = NeuronFactory.Get<ConditionalExpression>();
                }
                else if (iType.ID == (ulong)PredefinedNeurons.ConditionalStatement)
                {
                    iRes = NeuronFactory.Get<ConditionalStatement>();
                }
                else if (iType.ID == (ulong)PredefinedNeurons.ResultStatement)
                {
                    iRes = NeuronFactory.Get<ResultStatement>();
                }
                else if (iType.ID == (ulong)PredefinedNeurons.LockExpression)
                {
                    iRes = NeuronFactory.Get<LockExpression>();
                }
                else if (iType.ID == (ulong)PredefinedNeurons.SearchExpression)
                {
                    iRes = NeuronFactory.Get<SearchExpression>();
                }
                else if (iType.ID == (ulong)PredefinedNeurons.Statement)
                {
                    iRes = NeuronFactory.Get<Statement>();
                }
                else if (iType.ID == (ulong)PredefinedNeurons.Variable)
                {
                    iRes = NeuronFactory.Get<Variable>();
                }
                else if (iType.ID == (ulong)PredefinedNeurons.Global)
                {
                    iRes = NeuronFactory.Get<Global>();
                }
                else if (iType.ID == (ulong)PredefinedNeurons.Local)
                {
                    iRes = NeuronFactory.Get<Local>();
                }
                else if (iType.ID == (ulong)PredefinedNeurons.Assignment)
                {
                    iRes = NeuronFactory.Get<Assignment>();
                }
                else if (iType.ID == (ulong)PredefinedNeurons.ExpressionsBlock)
                {
                    iRes = NeuronFactory.Get<ExpressionsBlock>();
                }
                else if (iType.ID == (ulong)PredefinedNeurons.ByRefExpression)
                {
                    iRes = NeuronFactory.Get<ByRefExpression>();
                }
                else
                {
                    LogService.Log.LogError(
                        "NewInstruction.InternalGetValue", 
                        string.Format(
                            "Unknown argument found: {0}.  Expected one of the predefined neurons that identifies a type.", 
                            list[0]));
                }
            }
            else
            {
                LogService.Log.LogError("NewInstruction.InternalGetValue", "No arguments specified");
            }

            if (iRes == null)
            {
                iRes = NeuronFactory.GetNeuron(); // we always return a value
            }

            Brain.Current.MakeTemp(iRes);
            return iRes;
        }
    }
}