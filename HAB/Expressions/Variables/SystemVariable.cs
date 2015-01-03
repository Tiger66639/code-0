// --------------------------------------------------------------------------------------------------------------------
// <copyright file="SystemVariable.cs" company="">
//   
// </copyright>
// <summary>
//   A system variable is a special kind of variable that is prefilled by the
//   brain, which can't be changed and describes something about the system.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace JaStDev.HAB
{
    /// <summary>
    ///     A system variable is a special kind of variable that is prefilled by the
    ///     brain, which can't be changed and describes something about the system.
    /// </summary>
    public class SystemVariable : Variable
    {
        /// <summary>The store value.</summary>
        /// <remarks>A system variable doesn't allow setting.</remarks>
        /// <param name="value">The value.</param>
        /// <param name="proc">The proc.</param>
        protected internal override void StoreValue(System.Collections.Generic.List<Neuron> value, Processor proc)
        {
            LogService.Log.LogError("SystemVariable.Store", "Can't save a value in a system variable.");
        }

        /// <summary>Gets the processor local value(s) for this variable, without
        ///     initializing it if there was no value yet. This is primarely for
        ///     display purposes, to make certain that vars aren't accedentilly
        ///     initialized.</summary>
        /// <param name="proc">The proc.</param>
        /// <returns>The same value as <see cref="Variable.GetValue"/> since this type of
        ///     var doesn't need initializing, it always has a value.</returns>
        public override System.Collections.Generic.IEnumerable<Neuron> GetValueWithoutInit(Processor proc)
        {
            return SolveResultExp(this, proc);
        }
    }
}