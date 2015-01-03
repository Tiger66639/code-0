// --------------------------------------------------------------------------------------------------------------------
// <copyright file="FunctionValue.cs" company="">
//   
// </copyright>
// <summary>
//   a wrapper class for a list of debug neurons that represent a function
//   value (an argument or a return value).
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace JaStDev.HAB.Designer
{
    /// <summary>
    ///     a wrapper class for a list of debug neurons that represent a function
    ///     value (an argument or a return value).
    /// </summary>
    public class FunctionValue
    {
        /// <summary>Initializes a new instance of the <see cref="FunctionValue"/> class.</summary>
        /// <param name="index">The index.</param>
        /// <param name="values">The values.</param>
        public FunctionValue(int index, System.Collections.Generic.List<Neuron> values)
        {
            Values = new System.Collections.Generic.List<DebugNeuron>();
            foreach (var i in values)
            {
                Values.Add(new DebugNeuron(i));
            }

            Index = index;
        }

        #region index

        /// <summary>
        ///     gets the inde posiiton of this value.
        /// </summary>
        public int Index { get; private set; }

        #endregion

        #region Values

        /// <summary>
        ///     Gets the values for the variable.
        /// </summary>
        public System.Collections.Generic.List<DebugNeuron> Values { get; internal set; }

        #endregion
    }
}