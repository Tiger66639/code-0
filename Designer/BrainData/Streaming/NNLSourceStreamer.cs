// --------------------------------------------------------------------------------------------------------------------
// <copyright file="NNLSourceStreamer.cs" company="">
//   
// </copyright>
// <summary>
//   writes/reads nnl code to text.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace JaStDev.HAB.Designer
{
    using System.Linq;

    /// <summary>
    ///     writes/reads nnl code to text.
    /// </summary>
    internal class NNLSourceStreamer : Parsers.ISourceRendererDict
    {
        /// <summary>Exports the editor to the specified file name in NNL code.</summary>
        /// <param name="fileName">Name of the file.</param>
        /// <param name="toExport">To export.</param>
        internal static void Export(string fileName, EditorBase toExport)
        {
            var iStreamer = new NNLSourceStreamer();
            using (
                var iFile = new System.IO.FileStream(
                    fileName, 
                    System.IO.FileMode.Create, 
                    System.IO.FileAccess.ReadWrite))
            {
                using (var iWriter = new System.IO.StreamWriter(iFile))
                {
                    var iRenderer = new Parsers.NNLSourceRenderer(iStreamer, iWriter);
                    if (toExport == null)
                    {
                        // all the code of the entire project needs to be exported.
                        var iToRender = (from i in BrainData.Current.Editors.AllCodeEditors() select i.Item).ToList();
                        iRenderer.Render(iToRender);
                    }
                    else if (toExport is EditorFolder)
                    {
                        var iToRender =
                            (from i in ((EditorFolder)toExport).Items.AllCodeEditors() select i.Item).ToList();
                        iRenderer.Render(iToRender);
                    }
                    else if (toExport is NeuronEditor)
                    {
                        iRenderer.Render(((NeuronEditor)toExport).Item);
                    }
                }
            }
        }

        #region ISourceRendererDict Members

        /// <summary>Gets the name for the object.</summary>
        /// <param name="id"></param>
        /// <returns>The <see cref="string"/>.</returns>
        public string GetName(ulong id)
        {
            var iData = BrainData.Current.NeuronInfo[id];
            if (string.IsNullOrEmpty(iData.Title))
            {
                return "obj" + id;
            }

            return iData.Title;
        }

        /// <summary>Checks if the object has a <see langword="static"/> name (non
        ///     rendered).</summary>
        /// <param name="id"></param>
        /// <returns>The <see cref="bool"/>.</returns>
        public bool HasName(ulong id)
        {
            var iData = BrainData.Current.NeuronInfo[id];
            return string.IsNullOrEmpty(iData.Title) == false;
        }

        /// <summary>Create a <see langword="static"/> name for the object.</summary>
        /// <param name="id"></param>
        /// <returns>The <see cref="string"/>.</returns>
        public string BuildName(ulong id)
        {
            return id.ToString();
        }

        /// <summary>Returns a description as text (no xml formatting allowed) of the item,
        ///     if there is any.</summary>
        /// <param name="toRender"></param>
        /// <returns>The <see cref="string"/>.</returns>
        public string GetDescriptionText(ulong toRender)
        {
            var iData = BrainData.Current.NeuronInfo[toRender];
            if (string.IsNullOrEmpty(iData.DescriptionText) == false)
            {
                var iDoc = iData.Description;
                return new System.Windows.Documents.TextRange(iDoc.ContentStart, iDoc.ContentEnd).Text;
            }

            return null;
        }

        #endregion
    }
}