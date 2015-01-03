// --------------------------------------------------------------------------------------------------------------------
// <copyright file="MultiResultInstruction.cs" company="">
//   
// </copyright>
// <summary>
//   A result instruction that is able to return multiple results.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace JaStDev.HAB
{
    /// <summary>
    ///     A result instruction that is able to return multiple results.
    /// </summary>
    public abstract class MultiResultInstruction : ResultInstruction
    {
        /// <summary>Performs the tasks on the specified processor.</summary>
        /// <param name="processor">The processor on which the tasks need to be performed.</param>
        /// <param name="args">The arguments that the instruction requires to be properly executed.
        ///     These are also <see cref="Neuron"/> s.</param>
        public override void Execute(Processor processor, System.Collections.Generic.IList<Neuron> args)
        {
            var iRes = processor.Mem.ArgumentStack.Push();
            if (ArgCount > -1)
            {
                if (iRes.Capacity < ArgCount)
                {
                    iRes.Capacity = ArgCount;
                }
                else if (iRes.Capacity < 10)
                {
                    iRes.Capacity = 10;
                }
            }

            try
            {
                GetValues(processor, args);
            }
            finally
            {
                processor.Mem.ArgumentStack.Pop();
            }

            foreach (var i in iRes)
            {
                processor.Push(i);
            }
        }
    }
}