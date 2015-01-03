// --------------------------------------------------------------------------------------------------------------------
// <copyright file="IsInitializedInstruction.cs" company="">
//   
// </copyright>
// <summary>
//   Returns <see langword="true" /> if all the variables in the argument list
//   have a value assigned in the current processor, <see langword="false" />
//   when this is not the case (e.a: they haven't been used yet). Arguments: a
//   list of variables (passed along through a ByRef expression or as the
//   content of another variable).
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace JaStDev.HAB
{
    /// <summary>
    ///     Returns <see langword="true" /> if all the variables in the argument list
    ///     have a value assigned in the current processor, <see langword="false" />
    ///     when this is not the case (e.a: they haven't been used yet). Arguments: a
    ///     list of variables (passed along through a ByRef expression or as the
    ///     content of another variable).
    /// </summary>
    [NeuronID((ulong)PredefinedNeurons.IsInitializedInstruction)]
    public class IsInitializedInstruction : SingleResultInstruction, ICalculateBool
    {
        #region TypeOfNeuron

        /// <summary>
        ///     Gets the type of this neuron expressed as a Neuron.
        /// </summary>
        /// <value>
        ///     <see cref="JaStDev.HAB.PredefinedNeurons.IsInitializedInstruction" />
        ///     .
        /// </value>
        public override ulong TypeOfNeuronID
        {
            get
            {
                return (ulong)PredefinedNeurons.IsInitializedInstruction;
            }
        }

        #endregion

        /// <summary>
        ///     Returns the number of arguments that are required by this
        ///     instruction.
        /// </summary>
        /// <remarks>
        ///     A value of -1 indicates that a list of neurons is allowed, without
        ///     any specific number of values.
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

        #region ICalculateBool Members

        /// <summary>Calculate the <see langword="int"/> value and return it.</summary>
        /// <param name="processor"></param>
        /// <param name="list"></param>
        /// <returns>The <see cref="bool"/>.</returns>
        public bool CalculateBool(Processor processor, System.Collections.Generic.IList<Neuron> list)
        {
            foreach (var i in list)
            {
                var iGlob = i as Global;
                if (iGlob != null)
                {
                    // globals are in a different dict.
                    if (processor.GlobalValues.ContainsKey(iGlob) == false)
                    {
                        return false;
                    }
                }
                else
                {
                    var iSys = i as SystemVariable;
                    if (iSys == null)
                    {
                        // system vars are always initialized, so don't need to check for them.
                        var iVar = i as Variable;
                        if (iVar != null)
                        {
                            if (processor.Mem.VariableValues.Count > 0)
                            {
                                var iDict = processor.Mem.VariableValues.Peek();
                                if (iDict.ContainsKey(iVar) == false)
                                {
                                    return false;
                                }
                            }

                            return false;
                        }

                        LogService.Log.LogError(
                            "IsInitializedInstruction.InternalGetValue", 
                            "Can only determine if varaibles are initialized, not other types of neurons.");
                        return false;
                    }
                }
            }

            return true;
        }

        #endregion

        /// <summary>Gets the actual value.</summary>
        /// <param name="processor">The processor to use.</param>
        /// <param name="list">the list to get the nr of items from.</param>
        /// <returns>The result of the instruction.</returns>
        protected override Neuron InternalGetValue(Processor processor, System.Collections.Generic.IList<Neuron> list)
        {
            if (CalculateBool(processor, list))
            {
                return Brain.Current.TrueNeuron;
            }

            return Brain.Current.FalseNeuron;
        }
    }
}