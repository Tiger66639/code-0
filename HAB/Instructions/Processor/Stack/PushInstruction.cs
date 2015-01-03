﻿// --------------------------------------------------------------------------------------------------------------------
// <copyright file="PushInstruction.cs" company="">
//   
// </copyright>
// <summary>
//   This instruction puts the Neurons specified in the argument on the top of
//   the <see cref="Processor" /> 's processing stack.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace JaStDev.HAB
{
    /// <summary>
    ///     This instruction puts the Neurons specified in the argument on the top of
    ///     the <see cref="Processor" /> 's processing stack.
    /// </summary>
    [NeuronID((ulong)PredefinedNeurons.PushInstruction)]
    public class PushInstruction : Instruction
    {
        #region TypeOfNeuron

        /// <summary>
        ///     Gets the type of this neuron expressed as a Neuron.
        /// </summary>
        /// <value>
        ///     <see cref="JaStDev.HAB.PredefinedNeurons.PushInstruction" /> .
        /// </value>
        public override ulong TypeOfNeuronID
        {
            get
            {
                return (ulong)PredefinedNeurons.PushInstruction;
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
                return -1;
            }
        }

        /// <summary>Performs the tasks on the specified processor.</summary>
        /// <remarks>Instructions should never work directly on the data other than for
        ///     searching. Instead, they should go through the methods of the<see cref="Processor"/> that is passed along as an argument. This is
        ///     important cause when the instruction is executed for a sub processor,
        ///     the changes might need to be discarded.</remarks>
        /// <param name="processor">The processor on which the tasks need to be performed.</param>
        /// <param name="args">The arguments that the instruction requires to be properly executed.
        ///     These are also <see cref="Neuron"/> s.</param>
        public override void Execute(Processor processor, System.Collections.Generic.IList<Neuron> args)
        {
            System.Diagnostics.Debug.Assert(processor != null);
            System.Diagnostics.Debug.Assert(args != null);
            foreach (var i in args)
            {
                processor.Push(i);
            }
        }
    }
}