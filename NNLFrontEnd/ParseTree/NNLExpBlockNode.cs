// --------------------------------------------------------------------------------------------------------------------
// <copyright file="NNLExpBlockNode.cs" company="">
//   
// </copyright>
// <summary>
//   to render expresion blocks, which were part of the original export.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace JaStDev.HAB.Parsers
{
    /// <summary>
    ///     to render expresion blocks, which were part of the original export.
    /// </summary>
    internal class NNLExpBlockNode : NNLNode
    {
        /// <summary>The f content.</summary>
        private NNLFunctionNode fContent;

        /// <summary>Initializes a new instance of the <see cref="NNLExpBlockNode"/> class.</summary>
        public NNLExpBlockNode()
            : base(NodeType.Neuron)
        {
        }

        /// <summary>
        ///     the content for 'this'.
        /// </summary>
        public NNLFunctionNode Content
        {
            get
            {
                return fContent;
            }

            set
            {
                fContent = value;
                if (value != null)
                {
                    value.Parent = this;
                }
            }
        }

        /// <summary>The render.</summary>
        /// <param name="renderTo">The render to.</param>
        internal override void Render(NNLModuleCompiler renderTo)
        {
            if (Item == null)
            {
                var iCluster = FindObject(renderTo) as ExpressionsBlock;

                if (iCluster == null)
                {
                    iCluster = NeuronFactory.Get<ExpressionsBlock>();
                    Brain.Current.Add(iCluster);
                    Item = iCluster; // need to do this so we can add it to the rendering engine.
                    renderTo.Add(this);

                        // only do this when newly created here. When we found the item in the network, don't register again, cause it's not part of the code (the code contained an id, as a refernce to already existing item).
                }

                Content.Render(renderTo, (ulong)PredefinedNeurons.Code);

                    // need to call a render, so that the children (params) are also rendered)
                iCluster.SetFirstOutgoingLinkTo((ulong)PredefinedNeurons.Statements, Content.Item);
                Item = iCluster;

                    // we buffer the value so we can find it quickly again. Also used to indicate that it is created.
                RenderFunctions(renderTo);
                RenderLinks(renderTo);
                ProcessAttributes(renderTo);
            }
        }

        /// <summary>write the item to a <paramref name="stream"/> so it can be read in
        ///     without having to recompile the entire code.</summary>
        /// <param name="stream"></param>
        public override void Write(System.IO.BinaryWriter stream)
        {
            base.Write(stream);
            if (Content != null)
            {
                stream.Write(true);
                Content.Write(stream);
            }
            else
            {
                stream.Write(false);
            }
        }

        /// <summary>Reads the specified reader.</summary>
        /// <param name="reader">The reader.</param>
        public override void Read(System.IO.BinaryReader reader)
        {
            base.Read(reader);
            if (reader.ReadBoolean())
            {
                Content = new NNLFunctionNode();
                Content.Read(reader);
            }
        }
    }
}