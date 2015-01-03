// --------------------------------------------------------------------------------------------------------------------
// <copyright file="SingleResultInstruction.cs" company="">
//   
// </copyright>
// <summary>
//   A resultInstruction that only has 1 result.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace JaStDev.HAB
{
    /// <summary>
    ///     A resultInstruction that only has 1 result.
    /// </summary>
    public abstract class SingleResultInstruction : ResultInstruction
    {
        /// <summary>performs the task and returns it's result on the topmost ArgumentList
        ///     of the processor.</summary>
        /// <remarks><para>Note: when When a result instruction is executed, ( so<see cref="Instruction.Execute"/> is called instead of<see cref="ResultInstruction.GetValues"/> , the result value(s) are
        ///         pushed on the execution stack.</para>
        /// <para>This method has a default implementation that uses<see cref="ResultInstruction.InternalGetValue"/></para>
        /// <para>to get the actual result.</para>
        /// </remarks>
        /// <param name="processor">The processor.</param>
        /// <param name="iArgs"></param>
        public override void GetValues(Processor processor, System.Collections.Generic.IList<Neuron> iArgs)
        {
            var iRes = InternalGetValue(processor, iArgs);
            if (iRes != null)
            {
                // don't add null to the result list.
                processor.Mem.ArgumentStack.Peek().Add(iRes);
            }
        }

        /// <summary>The execute.</summary>
        /// <param name="processor">The processor.</param>
        /// <param name="args">The args.</param>
        public override void Execute(Processor processor, System.Collections.Generic.IList<Neuron> args)
        {
            var iRes = InternalGetValue(processor, args);
            if (iRes != null)
            {
                // don't add null to the result list.
                processor.Push(iRes);
            }
        }

        /// <summary>Gets the actual value.</summary>
        /// <param name="processor">The processor to use.</param>
        /// <param name="list">the list to get the nr of items from.</param>
        /// <returns>The result of the instruction.</returns>
        protected abstract Neuron InternalGetValue(Processor processor, System.Collections.Generic.IList<Neuron> list);
    }
}