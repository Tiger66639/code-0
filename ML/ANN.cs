// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ANN.cs" company="">
//   
// </copyright>
// <summary>
//   The default artificial neural network algorithm implementation. Tries to
//   stream as much data to disk as possible. Use this version if you need to
//   run a lot of ANNS at the same time.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace ML
{
    /// <summary>
    ///     The default artificial neural network algorithm implementation. Tries to
    ///     stream as much data to disk as possible. Use this version if you need to
    ///     run a lot of ANNS at the same time.
    /// </summary>
    public class ANN : AnnBase
    {
        /// <summary>Initializes a new instance of the <see cref="ANN"/> class.</summary>
        /// <param name="root">Root.</param>
        public ANN(ulong root)
            : base(root)
        {
        }

        /// <summary>Initializes a new instance of the <see cref="ANN"/> class.</summary>
        /// <param name="root">The root.</param>
        public ANN(JaStDev.HAB.Neuron root)
            : base(root)
        {
        }

        /// <summary><see cref="Create"/> a new layered neural network with the specified
        ///     nrInputs, <paramref name="nrOutputs"/> and nrLayers. After creation, the
        ///     network's connections should still be created.</summary>
        /// <param name="nrInputs">The expected Nr of inputs for the layer.</param>
        /// <param name="nrOutputs">The expected nr of outputs for the layer.</param>
        /// <param name="nrLayers">Nr layers.</param>
        /// <returns>The <see cref="Neuron"/>.</returns>
        public static JaStDev.HAB.Neuron Create(int nrInputs, int nrOutputs, int nrLayers)
        {
            throw new System.NotImplementedException();
        }

        #region functions

        /// <summary>Pushes the specified <paramref name="inputs"/> through the network and
        ///     returns the results.</summary>
        /// <param name="inputs">The inputs. This list must be the same size as the expected nr of inputs.</param>
        /// <returns>The <see cref="IList"/>.</returns>
        public System.Collections.IList Process(System.Collections.IList inputs)
        {
            throw new System.NotImplementedException();
        }

        #endregion
    }
}