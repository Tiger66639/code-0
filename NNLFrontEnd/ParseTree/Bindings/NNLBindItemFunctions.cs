// --------------------------------------------------------------------------------------------------------------------
// <copyright file="NNLBindItemFunctions.cs" company="">
//   
// </copyright>
// <summary>
//   a bind item for function sections.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace JaStDev.HAB.Parsers
{
    /// <summary>
    ///     a bind item for function sections.
    /// </summary>
    internal class NNLBindItemFunctions : NNLBindItemBase
    {
        /// <summary>The f functions.</summary>
        private System.Collections.Generic.Dictionary<string, NNLFunctionNode> fFunctions;

        #region Functions

        /// <summary>
        ///     Gets the list of functions that are defined in this section.
        /// </summary>
        public System.Collections.Generic.Dictionary<string, NNLFunctionNode> Functions
        {
            get
            {
                if (fFunctions == null)
                {
                    fFunctions = new System.Collections.Generic.Dictionary<string, NNLFunctionNode>();
                }

                return fFunctions;
            }
        }

        #endregion

        /// <summary>Renders the specified render to.</summary>
        /// <param name="renderTo">The render to.</param>
        internal override void Render(NNLModuleCompiler renderTo)
        {
            base.Render(renderTo);
            if (fFunctions != null)
            {
                foreach (var i in fFunctions)
                {
                    i.Value.Render(renderTo);
                }
            }
        }

        /// <summary>Reads the content from the specified binary reader.</summary>
        /// <param name="reader">The reader.</param>
        public override void Read(System.IO.BinaryReader reader)
        {
            base.Read(reader);
            var iCount = reader.ReadInt32(); // read the list of bindings
            while (iCount > 0)
            {
                var iName = reader.ReadString();
                var iNew = new NNLFunctionNode();
                iNew.Read(reader);
                Functions.Add(iName, iNew);
                iCount--;
            }
        }

        /// <summary>The write.</summary>
        /// <param name="stream">The stream.</param>
        public override void Write(System.IO.BinaryWriter stream)
        {
            base.Write(stream);
            if (fFunctions != null)
            {
                stream.Write(fFunctions.Count);
                foreach (var i in fFunctions)
                {
                    stream.Write(i.Key);
                    i.Value.Write(stream);
                }
            }
            else
            {
                stream.Write(0);
            }
        }
    }
}