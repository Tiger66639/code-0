// --------------------------------------------------------------------------------------------------------------------
// <copyright file="RenderDataSource.cs" company="">
//   
// </copyright>
// <summary>
//   The render data source.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace JaStDev.HAB.Designer
{
    /// <summary>The render data source.</summary>
    public abstract class RenderDataSource : Data.NamedObject
    {
        /// <summary>
        ///     determins if the datasource has some extra properties to add.
        /// </summary>
        public abstract bool HasExtraData { get; }

        /// <summary>called when the datasource gets selected and a new pipe should be made</summary>
        /// <param name="value"></param>
        /// <returns>The <see cref="IRenderPipe"/>.</returns>
        public abstract Queries.IRenderPipe GetPipe();

        /// <summary>Determines whether the specified <paramref name="source"/> is the same
        ///     pipe type as what the object would generate.</summary>
        /// <param name="source">The source.</param>
        /// <returns><c>true</c> if the specified <paramref name="source"/> is pipe;
        ///     otherwise, <c>false</c> .</returns>
        public abstract bool IsPipe(Queries.IRenderPipe source);

        /// <summary>This function is only useful if<see cref="JaStDev.HAB.Designer.QueryDataSource.HasExtraData"/> is
        ///     true. if a datasource requires a custom <paramref name="editor"/>
        ///     object instead of the pipe itself, this function can be overwritten to
        ///     provide a custom <paramref name="editor"/> for the pipe. By default,
        ///     this is null, meaning that the pipe itself will be used as editing
        ///     object.</summary>
        /// <param name="editor">The editor.</param>
        /// <returns>The <see cref="object"/>.</returns>
        public virtual object GetEditor(QueryEditor editor)
        {
            return null;
        }

        #region IXmlSerializable Members

        /// <summary>Generates an object from its XML representation.</summary>
        /// <param name="reader">The <see cref="System.Xml.XmlReader"/> stream from which the object is
        ///     deserialized.</param>
        /// <param name="editor">The editor.</param>
        public abstract void ReadXml(System.Xml.XmlReader reader, QueryEditor editor);

        /// <summary>Converts an object into its XML representation.</summary>
        /// <param name="writer">The <see cref="System.Xml.XmlWriter"/> stream to which the object is serialized.</param>
        /// <param name="editor">The editor.</param>
        public abstract void WriteXml(System.Xml.XmlWriter writer, QueryEditor editor);

        #endregion
    }

    /// <summary>
    ///     a render data source to indicate that there is no output.
    /// </summary>
    public class NoRenderDataSource : RenderDataSource
    {
        /// <summary>Initializes a new instance of the <see cref="NoRenderDataSource"/> class.</summary>
        public NoRenderDataSource()
        {
            Name = "No output";
        }

        /// <summary>
        ///     determins if the datasource has some extra properties to add.
        /// </summary>
        public override bool HasExtraData
        {
            get
            {
                return false;
            }
        }

        /// <summary>called when the datasource gets selected and a new pipe should be made</summary>
        /// <returns>The <see cref="IRenderPipe"/>.</returns>
        public override Queries.IRenderPipe GetPipe()
        {
            return null;
        }

        /// <summary>Determines whether the specified <paramref name="source"/> is the same
        ///     pipe type as what the object would generate.</summary>
        /// <param name="source">The source.</param>
        /// <returns><c>true</c> if the specified <paramref name="source"/> is pipe;
        ///     otherwise, <c>false</c> .</returns>
        public override bool IsPipe(Queries.IRenderPipe source)
        {
            return source == null;
        }

        /// <summary>Converts an object into its XML representation.</summary>
        /// <param name="writer">The <see cref="System.Xml.XmlWriter"/> stream to which the object is serialized.</param>
        /// <param name="editor">The editor.</param>
        public override void WriteXml(System.Xml.XmlWriter writer, QueryEditor editor)
        {
            // nothing to write
        }

        /// <summary>Generates an object from its XML representation.</summary>
        /// <param name="reader">The <see cref="System.Xml.XmlReader"/> stream from which the object is
        ///     deserialized.</param>
        /// <param name="editor">The editor.</param>
        public override void ReadXml(System.Xml.XmlReader reader, QueryEditor editor)
        {
            // nothing to read
        }
    }

    /// <summary>
    ///     render to csv files.
    /// </summary>
    public class CsvRenderDataSource : RenderDataSource
    {
        /// <summary>Initializes a new instance of the <see cref="CsvRenderDataSource"/> class.</summary>
        public CsvRenderDataSource()
        {
            Name = "render to csv file";
        }

        /// <summary>
        ///     determins if the datasource has some extra properties to add.
        /// </summary>
        public override bool HasExtraData
        {
            get
            {
                return true;
            }
        }

        /// <summary>called when the datasource gets selected and a new pipe should be made</summary>
        /// <returns>The <see cref="IRenderPipe"/>.</returns>
        public override Queries.IRenderPipe GetPipe()
        {
            return new CustomConduitSupport.CsvRenderPipe();
        }

        /// <summary>Determines whether the specified <paramref name="source"/> is the same
        ///     pipe type as what the object would generate.</summary>
        /// <param name="source">The source.</param>
        /// <returns><c>true</c> if the specified <paramref name="source"/> is pipe;
        ///     otherwise, <c>false</c> .</returns>
        public override bool IsPipe(Queries.IRenderPipe source)
        {
            return source is CustomConduitSupport.CsvRenderPipe;
        }

        /// <summary>Converts an object into its XML representation.</summary>
        /// <param name="writer">The <see cref="System.Xml.XmlWriter"/> stream to which the object is serialized.</param>
        /// <param name="editor">The editor.</param>
        public override void WriteXml(System.Xml.XmlWriter writer, QueryEditor editor)
        {
            var iQuery = (Queries.Query)editor.Item;
            var iPipe = (CustomConduitSupport.CsvRenderPipe)iQuery.RenderTarget;
            XmlStore.WriteElement(writer, "FileName", iPipe.FileName);
            XmlStore.WriteElement(writer, "Append", iPipe.Append);
            XmlStore.WriteElement(writer, "CultureName", iPipe.CultureName);
        }

        /// <summary>Generates an object from its XML representation.</summary>
        /// <param name="reader">The <see cref="System.Xml.XmlReader"/> stream from which the object is
        ///     deserialized.</param>
        /// <param name="editor">The editor.</param>
        public override void ReadXml(System.Xml.XmlReader reader, QueryEditor editor)
        {
            var iQuery = (Queries.Query)editor.Item;
            var iPipe = (CustomConduitSupport.CsvRenderPipe)iQuery.RenderTarget;
            var iVal = string.Empty;
            var iBVal = true;
            if (XmlStore.TryReadElement(reader, "FileName", ref iVal))
            {
                iPipe.FileName = iVal;
            }

            if (XmlStore.TryReadElement(reader, "Append", ref iBVal))
            {
                iPipe.Append = iBVal;
            }

            if (XmlStore.TryReadElement(reader, "CultureName", ref iVal))
            {
                iPipe.CultureName = iVal;
            }
        }
    }
}