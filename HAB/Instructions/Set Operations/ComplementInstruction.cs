// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ComplementInstruction.cs" company="">
//   
// </copyright>
// <summary>
//   Performs a complement on the content of the 2 variables that were passed
//   along as arguments.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace JaStDev.HAB
{
    using Enumerable = System.Linq.Enumerable;

    /// <summary>
    ///     Performs a complement on the content of the 2 variables that were passed
    ///     along as arguments.
    /// </summary>
    /// <remarks>
    ///     <para>arguments: 2</para>
    ///     <list type="bullet">
    ///         <item>
    ///             <description>
    ///                 var 1 = the universe (the complete set of items)
    ///             </description>
    ///         </item>
    ///         <item>
    ///             <description>
    ///                 var 2 = the complemented set result: all the items in var 1 that aren't
    ///                 in var 2
    ///             </description>
    ///         </item>
    ///     </list>
    /// </remarks>
    [NeuronID((ulong)PredefinedNeurons.ComplementInstruction)]
    public class ComplementInstruction : MultiResultInstruction, IExecResultStatement
    {
        #region TypeOfNeuron

        /// <summary>
        ///     Gets the type of this neuron expressed as a Neuron.
        /// </summary>
        /// <value>
        ///     <see cref="JaStDev.HAB.PredefinedNeurons.ComplementInstruction" /> .
        /// </value>
        public override ulong TypeOfNeuronID
        {
            get
            {
                return (ulong)PredefinedNeurons.ComplementInstruction;
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
        ///     -1
        /// </value>
        public override int ArgCount
        {
            get
            {
                return -1;
            }
        }

        /// <summary>The get value.</summary>
        /// <param name="processor">The processor.</param>
        /// <param name="list">The list.</param>
        /// <returns>The <see cref="bool"/>.</returns>
        public bool GetValue(Processor processor, System.Collections.Generic.IList<Neuron> list)
        {
            if (list != null && list.Count >= 2)
            {
                var a = list[0] as Variable;
                if (a == null)
                {
                    a = SolveSingleResultExp(list[0], processor) as Variable;
                }

                if (a != null)
                {
                    var b = list[1] as Variable;
                    if (b == null)
                    {
                        b = SolveSingleResultExp(list[1], processor) as Variable;
                    }

                    if (b != null)
                    {
                        var iRes = processor.Mem.ArgumentStack.Peek();
                        var iUniverse = a.ExtractValue(processor); // Neuron.SolveResultExpNoStackChange(a, processor);
                        System.Collections.Generic.IEnumerable<Neuron> iComplement = b.ExtractValue(processor);

                            // Neuron.SolveResultExpNoStackChange(b, processor);
                        iRes.AddRange(Enumerable.Except(iUniverse, iComplement));
                    }
                    else
                    {
                        LogService.Log.LogError(
                            "ComplementInstruction.GetValues", 
                            "All arguments should be variables (usually passed through a 'ByRef' expression)!");
                    }
                }
                else
                {
                    LogService.Log.LogError(
                        "ComplementInstruction.GetValues", 
                        "All arguments should be variables (usually passed through a 'ByRef' expression)!");
                }

                return true;
            }

            return false;
        }

        /// <summary>checks if the value can be returned as a bool.</summary>
        /// <param name="args"></param>
        /// <returns>The <see cref="bool"/>.</returns>
        public bool CanGetBool(System.Collections.Generic.IList<Neuron> args)
        {
            return false;
        }

        /// <summary>gets the result as a <see langword="bool"/> value (argumnets still
        ///     need to be calculated).</summary>
        /// <param name="handler"></param>
        /// <param name="args"></param>
        /// <exception cref="System.NotImplementedException"></exception>
        /// <returns>The <see cref="bool"/>.</returns>
        public bool GetBool(Processor handler, System.Collections.Generic.IList<Neuron> args)
        {
            throw new System.NotImplementedException();
        }

        /// <summary>checks if the result should be an int.</summary>
        /// <param name="args">The args, not calculated.</param>
        /// <returns><c>true</c> if this instance [can get int] the specified args;
        ///     otherwise, <c>false</c> .</returns>
        public bool CanGetInt(System.Collections.Generic.IList<Neuron> args)
        {
            return false;
        }

        /// <summary>The get int.</summary>
        /// <param name="handler">The handler.</param>
        /// <param name="args">The args.</param>
        /// <returns>The <see cref="int"/>.</returns>
        /// <exception cref="NotImplementedException"></exception>
        public int GetInt(Processor handler, System.Collections.Generic.IList<Neuron> args)
        {
            throw new System.NotImplementedException();
        }

        /// <summary>checks if the result is a double.</summary>
        /// <param name="args"></param>
        /// <returns>The <see cref="bool"/>.</returns>
        public bool CanGetDouble(System.Collections.Generic.IList<Neuron> args)
        {
            return false;
        }

        /// <summary>The get double.</summary>
        /// <param name="handler">The handler.</param>
        /// <param name="args">The args.</param>
        /// <returns>The <see cref="double"/>.</returns>
        /// <exception cref="NotImplementedException"></exception>
        public double GetDouble(Processor handler, System.Collections.Generic.IList<Neuron> args)
        {
            throw new System.NotImplementedException();
        }

        /// <summary>performs the task and returns it's result.</summary>
        /// <remarks>Note: when When a result instruction is executed, ( so<see cref="Instruction.Execute"/> is called instead of<see cref="ResultInstruction.GetValues"/> , the result value(s) are
        ///     pushed on the execution stack.</remarks>
        /// <param name="processor"></param>
        /// <param name="list">The list of arguments</param>
        public override void GetValues(Processor processor, System.Collections.Generic.IList<Neuron> list)
        {
            if (list != null && list.Count >= 2)
            {
                var a = list[0] as Variable;
                if (a != null)
                {
                    var b = list[1] as Variable;
                    if (b != null)
                    {
                        var iRes = processor.Mem.ArgumentStack.Peek();
                        var iUniverse = SolveResultExpNoStackChange(a, processor);

                        // using 2 try blocks, to make certain that the argument-stack can't get corrupted because some exception in the second Solve.
                        try
                        {
                            System.Collections.Generic.IEnumerable<Neuron> iComplement = SolveResultExpNoStackChange(
                                b, 
                                processor);
                            try
                            {
                                iRes.AddRange(Enumerable.Except(iUniverse, iComplement));
                            }
                            finally
                            {
                                processor.Mem.ArgumentStack.Pop();
                            }
                        }
                        finally
                        {
                            processor.Mem.ArgumentStack.Pop();
                        }
                    }
                    else
                    {
                        LogService.Log.LogError(
                            "ComplementInstruction.GetValues", 
                            "All arguments should be variables (usually passed through a 'ByRef' expression)!");
                    }
                }
                else
                {
                    LogService.Log.LogError(
                        "ComplementInstruction.GetValues", 
                        "All arguments should be variables (usually passed through a 'ByRef' expression)!");
                }
            }
            else
            {
                LogService.Log.LogError("ComplementInstruction.GetValues", "No arguments specified");
            }
        }
    }
}