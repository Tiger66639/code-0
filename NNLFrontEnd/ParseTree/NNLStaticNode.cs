// --------------------------------------------------------------------------------------------------------------------
// <copyright file="NNLStaticNode.cs" company="">
//   
// </copyright>
// <summary>
//   represents statics that were used.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace JaStDev.HAB.Parsers
{
    /// <summary>
    ///     represents statics that were used.
    /// </summary>
    internal class NNLStaticNode : NNLNeuronNode
    {
        /// <summary>Initializes a new instance of the <see cref="NNLStaticNode"/> class.</summary>
        /// <param name="type">The type.</param>
        public NNLStaticNode(NeuronType type)
            : base(type)
        {
        }

        /// <summary>renders this node to the specified module compiler.</summary>
        /// <param name="renderTo"></param>
        internal override void Render(NNLModuleCompiler renderTo)
        {
            if (Item == null)
            {
                switch (Neurontype)
                {
                    case NeuronType.Int:
                        Item = renderTo.GetInt((int)Value);
                        break;
                    case NeuronType.Double:
                        Item = renderTo.GetDouble((double)Value);
                        break;
                    case NeuronType.String:
                        if (InDict)
                        {
                            Item = TextNeuron.GetFor((string)Value);
                        }
                        else
                        {
                            Item = NeuronFactory.GetText((string)Value);
                            Brain.Current.Add(Item);
                        }

                        break;
                    default:
                        break;
                }

                renderTo.Add(Item);
            }
        }

        /// <summary>The get type decl.</summary>
        /// <param name="renderTo">The render to.</param>
        /// <returns>The <see cref="DeclType"/>.</returns>
        protected internal override DeclType GetTypeDecl(NNLModuleCompiler renderTo)
        {
            switch (Neurontype)
            {
                case NeuronType.Int:
                    return DeclType.Int;
                case NeuronType.Double:
                    return DeclType.Double;
                case NeuronType.String:
                    return DeclType.String;
                default:
                    return DeclType.Var;
            }
        }
    }
}