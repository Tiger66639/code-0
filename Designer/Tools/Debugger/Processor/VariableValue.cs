// --------------------------------------------------------------------------------------------------------------------
// <copyright file="VariableValue.cs" company="">
//   
// </copyright>
// <summary>
//   Provides a wrapper object to display all the variables and global values
//   of a processor.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace JaStDev.HAB.Designer
{
    /// <summary>
    ///     Provides a wrapper object to display all the variables and global values
    ///     of a processor.
    /// </summary>
    public class VariableValue
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="VariableValue" /> class.
        /// </summary>
        public VariableValue()
        {
        }

        /// <summary>Initializes a new instance of the <see cref="VariableValue"/> class.</summary>
        /// <param name="values">The values.</param>
        public VariableValue(System.Collections.Generic.KeyValuePair<Global, System.Collections.Generic.List<Neuron>> values)
        {
            Variable = new DebugNeuron(values.Key, false);
            Values = new System.Collections.Generic.List<DebugNeuron>();
            foreach (var i in values.Value)
            {
                Values.Add(new DebugNeuron(i, false));
            }
        }

        /// <summary>Initializes a new instance of the <see cref="VariableValue"/> class.</summary>
        /// <param name="values">The values.</param>
        public VariableValue(System.Collections.Generic.KeyValuePair<ulong, System.Collections.Generic.List<Neuron>> values)
        {
            Variable = new DebugNeuron(Brain.Current[values.Key], false);
            Values = new System.Collections.Generic.List<DebugNeuron>();
            foreach (var i in values.Value)
            {
                Values.Add(new DebugNeuron(i, false));
            }
        }

        /// <summary>Initializes a new instance of the <see cref="VariableValue"/> class.</summary>
        /// <param name="values">The values.</param>
        public VariableValue(System.Collections.Generic.KeyValuePair<Variable, VarValuesList> values)
        {
            Variable = new DebugNeuron(values.Key, false);
            Values = new System.Collections.Generic.List<DebugNeuron>();
            foreach (var i in values.Value.Data)
            {
                Values.Add(new DebugNeuron(i, false));
            }
        }

        #region Prop

        #region Variable

        /// <summary>
        ///     Gets the variable for which we are displaying the values.
        /// </summary>
        public DebugNeuron Variable { get; internal set; }

        #endregion

        #region Values

        /// <summary>
        ///     Gets the values for the variable.
        /// </summary>
        public System.Collections.Generic.List<DebugNeuron> Values { get; internal set; }

        #endregion

        #endregion
    }
}