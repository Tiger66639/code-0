// --------------------------------------------------------------------------------------------------------------------
// <copyright file="NNLClassNode.cs" company="">
//   
// </copyright>
// <summary>
//   a node for the class type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace JaStDev.HAB.Parsers
{
    /// <summary>
    ///     a node for the class type.
    /// </summary>
    internal class NNLClassNode : NNLNode
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="NNLClassNode" /> class.
        /// </summary>
        public NNLClassNode()
            : base(NodeType.Class)
        {
        }

        /// <summary>write the item to a <paramref name="stream"/> so it can be read in
        ///     without having to recompile the entire code.</summary>
        /// <param name="stream"></param>
        public override void Write(System.IO.BinaryWriter stream)
        {
            base.Write(stream);
        }

        /// <summary>The read.</summary>
        /// <param name="reader">The reader.</param>
        public override void Read(System.IO.BinaryReader reader)
        {
            base.Read(reader);
        }
    }
}