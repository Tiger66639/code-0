// --------------------------------------------------------------------------------------------------------------------
// <copyright file="NNLBindItem.cs" company="">
//   
// </copyright>
// <summary>
//   a binding item for dot and arrow left/right path operarots.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace JaStDev.HAB.Parsers
{
    /// <summary>
    ///     a binding item for dot and arrow left/right path operarots.
    /// </summary>
    internal class NNLBindItem : NNLBindItemIndex
    {
        /// <summary>The f statics.</summary>
        private System.Collections.Generic.Dictionary<string, NNLBindItem> fStatics;

        #region Statics

        /// <summary>
        ///     Gets the list of statics defined for this binding item (other than the
        ///     setter and getter).
        /// </summary>
        public System.Collections.Generic.Dictionary<string, NNLBindItem> Statics
        {
            get
            {
                if (fStatics == null)
                {
                    fStatics = new System.Collections.Generic.Dictionary<string, NNLBindItem>();
                }

                return fStatics;
            }
        }

        #endregion

        /// <summary>renders this node to the specified module compiler.</summary>
        /// <param name="renderTo">The render to.</param>
        internal override void Render(NNLModuleCompiler renderTo)
        {
            base.Render(renderTo);
            if (fStatics != null)
            {
                foreach (var i in fStatics)
                {
                    i.Value.Render(renderTo);
                }
            }
        }

        /// <summary>makes certain taht all the references to other binding items are
        ///     resolved. need to make certain that the statics are also resolved.</summary>
        /// <param name="renderTo">the object to render to. Can be null, in case that the data was read
        ///     from file. In this case, when there are problems, an exception is
        ///     thrown instead of logged to the renderer.</param>
        internal override void ResolveReferences(NNLModuleCompiler renderTo)
        {
            base.ResolveReferences(renderTo);
            if (fStatics != null)
            {
                foreach (var i in fStatics)
                {
                    i.Value.ResolveReferences(renderTo);
                }
            }
        }

        /// <summary>Writes the specified stream.</summary>
        /// <param name="stream">The stream.</param>
        public override void Write(System.IO.BinaryWriter stream)
        {
            base.Write(stream);
            if (fStatics != null)
            {
                stream.Write(fStatics.Count);
                foreach (var i in fStatics)
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

        /// <summary>Reads the content from the specified binary reader.</summary>
        /// <param name="reader">The reader.</param>
        public override void Read(System.IO.BinaryReader reader)
        {
            base.Read(reader);
            var iCount = reader.ReadInt32(); // read the list of bindings
            while (iCount > 0)
            {
                var iName = reader.ReadString();
                var iNew = new NNLBindItem();
                iNew.Read(reader);
                Statics.Add(iName, iNew);
                iNew.Parent = this;
                iCount--;
            }
        }
    }
}