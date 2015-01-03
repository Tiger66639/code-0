// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Global.cs" company="">
//   
// </copyright>
// <summary>
//   An expression type that is able to store a list of neurons. The contents
//   of a global remain valid for as long as the processor is alive.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace JaStDev.HAB
{
    /// <summary>
    ///     An expression type that is able to store a list of neurons. The contents
    ///     of a global remain valid for as long as the processor is alive.
    /// </summary>
    [NeuronID((ulong)PredefinedNeurons.Global, typeof(Neuron))]
    public class Global : Variable
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="Global" /> class.
        /// </summary>
        internal Global()
        {
        }

        #region TypeOfNeuron

        /// <summary>
        ///     Gets the type of this neuron expressed as a Neuron.
        /// </summary>
        /// <value>
        ///     <see cref="JaStDev.HAB.PredefinedNeurons.Variable" /> .
        /// </value>
        public override ulong TypeOfNeuronID
        {
            get
            {
                return (ulong)PredefinedNeurons.Global;
            }
        }

        #endregion

        /// <summary>gets the result of the variable without putting anything on the stack
        ///     but by simply returning the value.</summary>
        /// <param name="proc"></param>
        /// <returns>The <see cref="List"/>.</returns>
        protected internal override System.Collections.Generic.List<Neuron> ExtractValue(Processor proc)
        {
            System.Collections.Generic.List<Neuron> iRes = null;
            var iDict = proc.GlobalValues;
            System.Diagnostics.Debug.Assert(iDict != null);
            if (iDict.TryGetValue(this, out iRes) == false)
            {
                var iValue = Value;
                if (iValue != null)
                {
                    iRes = SolveResultExp(iValue, proc);
                    iDict.Add(this, iRes);
                }
            }

            return iRes;
        }

        /// <summary>The get value without init.</summary>
        /// <param name="proc">The proc.</param>
        /// <returns>The <see cref="IEnumerable"/>.</returns>
        public override System.Collections.Generic.IEnumerable<Neuron> GetValueWithoutInit(Processor proc)
        {
            System.Collections.Generic.List<Neuron> iRes = null;
            if (proc != null)
            {
                var iDict = proc.GlobalValues;
                System.Diagnostics.Debug.Assert(iDict != null);
                if (iDict.TryGetValue(this, out iRes) == false)
                {
                    iRes = new System.Collections.Generic.List<Neuron>();
                }
            }

            return iRes;
        }

        /// <summary>The store value.</summary>
        /// <param name="value">The value.</param>
        /// <param name="proc">The proc.</param>
        protected internal override void StoreValue(System.Collections.Generic.List<Neuron> value, Processor proc)
        {
            proc.StoreGlobalValue(this, value);
        }
    }
}