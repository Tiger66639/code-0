// --------------------------------------------------------------------------------------------------------------------
// <copyright file="NNLOSFunction.cs" company="">
//   
// </copyright>
// <summary>
//   a statement node taht references an os function that was loaded through
//   the reflection sin. This is used in the rendering stage, to solve the
//   pathTerminus + to make certain that it is rendered correctly.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace JaStDev.HAB.Parsers
{
    /// <summary>
    ///     a statement node taht references an os function that was loaded through
    ///     the reflection sin. This is used in the rendering stage, to solve the
    ///     pathTerminus + to make certain that it is rendered correctly.
    /// </summary>
    internal class NNLOSFunction : NNLStatementNode
    {
        /// <summary>Initializes a new instance of the <see cref="NNLOSFunction"/> class.</summary>
        public NNLOSFunction()
            : base(NodeType.Neuron)
        {
        }
    }
}