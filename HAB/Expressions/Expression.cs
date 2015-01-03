// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Expression.cs" company="">
//   
// </copyright>
// <summary>
//   Base class for all code expressions.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace JaStDev.HAB
{
    /// <summary>
    ///     Base class for all code expressions.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         An expression differs from an instruction in that an instruction does
    ///         something while an expression can point to an instruction but can also be
    ///         used to control the execution loop.
    ///     </para>
    ///     <para>
    ///         This is also a neuron so that <see cref="Processor" /> s can easely edit,
    ///         create and filter them. This also allows us to use
    ///         <see cref="NeuronCluster" /> s to store them as functions.
    ///     </para>
    /// </remarks>
    public abstract class Expression : Neuron
    {
        /// <summary>
        ///     Gets a value indicating whether this expression causes the processor
        ///     to stop (an exit). This can be used by debuggers, to quickly check
        ///     this value.
        /// </summary>
        /// <value>
        ///     <c>true</c> if this instance is terminator; otherwise, <c>false</c> .
        /// </value>
        public virtual bool IsTerminator
        {
            get
            {
                return false;
            }
        }

        /// <summary>Inheriters should implement this function for performing the
        ///     expression.</summary>
        /// <remarks>Implementers don't have to increment the procoessor positions or
        ///     anything (except for conditional statements offcourse), this is done
        ///     by <see cref="Expression.Execute"/></remarks>
        /// <param name="handler">The processor to execute the statements on.</param>
        protected internal abstract void Execute(Processor handler);

        /// <summary>Asks the expression to load all the data that it requires for being
        ///     executed. This is used as an optimisation so that the code-data can be
        ///     pre-fetched. This is to solve a problem with slower platforms where
        ///     the first input can be handled slowly.</summary>
        /// <param name="alreadyProcessed">The list of neurons that have already been processed, but who's
        ///     WorkData hasn't been stored yet. This is used so we can avoid getting
        ///     stuck in an ethernal loop.</param>
        protected internal abstract void LoadCode(System.Collections.Generic.HashSet<Neuron> alreadyProcessed);

        /// <summary>small optimizer, checks if the code is loaded alrady or not. This is
        ///     used to see if a start point needs to be loaded or not, whithout
        ///     having to set up mem all the time.</summary>
        /// <returns>The <see cref="bool"/>.</returns>
        protected internal abstract bool IsCodeLoaded();
    }
}