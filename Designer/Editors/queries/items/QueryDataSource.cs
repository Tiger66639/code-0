// --------------------------------------------------------------------------------------------------------------------
// <copyright file="QueryDataSource.cs" company="">
//   
// </copyright>
// <summary>
//   base class for all objects that can be used as a datasource for a query
//   (the pipes).
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace JaStDev.HAB.Designer
{
    /// <summary>
    ///     base class for all objects that can be used as a datasource for a query
    ///     (the pipes).
    /// </summary>
    /// <remarks>
    ///     are used in a <see langword="static" /> list, so that all
    ///     <see cref="QueryEditor" /> s can use the same list. Can be streamed
    ///     to/from xml, but not by using the <see cref="System.Xml.Serialization.IXmlSerializable" />
    ///     interface, cause we need to pass along the actual data as well.
    /// </remarks>
    public abstract class QueryDataSource : Data.NamedObject
    {
        /// <summary>
        ///     determins if the datasource has some extra properties to add.
        /// </summary>
        public abstract bool HasExtraData { get; }

        /// <summary>called when the datasource gets selected and a new pipe should be made</summary>
        /// <param name="value"></param>
        /// <returns>The <see cref="IQueryPipe"/>.</returns>
        public abstract Queries.IQueryPipe GetPipe();

        /// <summary>Determines whether the specified <paramref name="source"/> is the same
        ///     pipe type as what the object would generate.</summary>
        /// <param name="source">The source.</param>
        /// <returns><c>true</c> if the specified <paramref name="source"/> is pipe;
        ///     otherwise, <c>false</c> .</returns>
        public abstract bool IsPipe(Queries.IQueryPipe source);

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
    ///     no datasource.
    /// </summary>
    public class NoQueryDataSource : QueryDataSource
    {
        /// <summary>Initializes a new instance of the <see cref="NoQueryDataSource"/> class.</summary>
        public NoQueryDataSource()
        {
            Name = "No source";
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
        /// <returns>The <see cref="IQueryPipe"/>.</returns>
        public override Queries.IQueryPipe GetPipe()
        {
            return null;
        }

        /// <summary>Determines whether the specified <paramref name="source"/> is the same
        ///     pipe type as what the object would generate.</summary>
        /// <param name="source">The source.</param>
        /// <returns><c>true</c> if the specified <paramref name="source"/> is pipe;
        ///     otherwise, <c>false</c> .</returns>
        public override bool IsPipe(Queries.IQueryPipe source)
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
    ///     provides data from the words dict.
    /// </summary>
    public class WordsQueryDataSource : QueryDataSource
    {
        /// <summary>Initializes a new instance of the <see cref="WordsQueryDataSource"/> class.</summary>
        public WordsQueryDataSource()
        {
            Name = "Words dictionary";
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
        /// <returns>The <see cref="IQueryPipe"/>.</returns>
        public override Queries.IQueryPipe GetPipe()
        {
            return new Queries.WordsPipe();
        }

        /// <summary>Determines whether the specified <paramref name="source"/> is the same
        ///     pipe type as what the object would generate.</summary>
        /// <param name="source">The source.</param>
        /// <returns><c>true</c> if the specified <paramref name="source"/> is pipe;
        ///     otherwise, <c>false</c> .</returns>
        public override bool IsPipe(Queries.IQueryPipe source)
        {
            return source is Queries.WordsPipe;
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
    ///     provides data from the explorer.
    /// </summary>
    public class ExplorerQueryDataSource : QueryDataSource
    {
        /// <summary>Initializes a new instance of the <see cref="ExplorerQueryDataSource"/> class. 
        ///     Initializes a new instance of the<see cref="ExplorerQueryDataSource"/> class.</summary>
        public ExplorerQueryDataSource()
        {
            Name = "Explorer";
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
        /// <returns>The <see cref="IQueryPipe"/>.</returns>
        public override Queries.IQueryPipe GetPipe()
        {
            return new Queries.ExplorerPipe();
        }

        /// <summary>Determines whether the specified <paramref name="source"/> is the same
        ///     pipe type as what the object would generate.</summary>
        /// <param name="source">The source.</param>
        /// <returns><c>true</c> if the specified <paramref name="source"/> is pipe;
        ///     otherwise, <c>false</c> .</returns>
        public override bool IsPipe(Queries.IQueryPipe source)
        {
            return source is Queries.ExplorerPipe;
        }

        /// <summary>Converts an object into its XML representation.</summary>
        /// <param name="writer">The <see cref="System.Xml.XmlWriter"/> stream to which the object is serialized.</param>
        /// <param name="editor">The editor.</param>
        public override void WriteXml(System.Xml.XmlWriter writer, QueryEditor editor)
        {
            var iQuery = (Queries.Query)editor.Item;
            var iPipe = (Queries.ExplorerPipe)iQuery.DataSource;
            XmlStore.WriteElement(writer, "LowerRange", iPipe.LowerRange);
            XmlStore.WriteElement(writer, "UpperRange", iPipe.UpperRange);
        }

        /// <summary>Generates an object from its XML representation.</summary>
        /// <param name="reader">The <see cref="System.Xml.XmlReader"/> stream from which the object is
        ///     deserialized.</param>
        /// <param name="editor">The editor.</param>
        public override void ReadXml(System.Xml.XmlReader reader, QueryEditor editor)
        {
            var iQuery = (Queries.Query)editor.Item;
            var iPipe = (Queries.ExplorerPipe)iQuery.DataSource;
            ulong iFound = 0;
            if (XmlStore.TryReadElement(reader, "LowerRange", ref iFound))
            {
                iPipe.LowerRange = iFound;
            }

            if (XmlStore.TryReadElement(reader, "UpperRange", ref iFound))
            {
                iPipe.UpperRange = iFound;
            }
        }
    }

    /// <summary>
    ///     provides data from wordnet.
    /// </summary>
    public class WordNetQueryDataSource : QueryDataSource
    {
        /// <summary>Initializes a new instance of the <see cref="WordNetQueryDataSource"/> class. 
        ///     Initializes a new instance of the<see cref="ExplorerQueryDataSource"/> class.</summary>
        public WordNetQueryDataSource()
        {
            Name = "Wordnet";
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
        /// <returns>The <see cref="IQueryPipe"/>.</returns>
        public override Queries.IQueryPipe GetPipe()
        {
            return new Queries.WordNetPipe();
        }

        /// <summary>Determines whether the specified <paramref name="source"/> is the same
        ///     pipe type as what the object would generate.</summary>
        /// <param name="source">The source.</param>
        /// <returns><c>true</c> if the specified <paramref name="source"/> is pipe;
        ///     otherwise, <c>false</c> .</returns>
        public override bool IsPipe(Queries.IQueryPipe source)
        {
            return source is Queries.WordNetPipe;
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
    ///     provides data from the explorer.
    /// </summary>
    public class CustomConduitQueryDataSource : QueryDataSource
    {
        /// <summary>Initializes a new instance of the <see cref="CustomConduitQueryDataSource"/> class. 
        ///     Initializes a new instance of the<see cref="ExplorerQueryDataSource"/> class.</summary>
        public CustomConduitQueryDataSource()
        {
            Name = "Custom conduit";
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
        /// <returns>The <see cref="IQueryPipe"/>.</returns>
        public override Queries.IQueryPipe GetPipe()
        {
            var iRes = new Queries.CustomConduitPipe();
            iRes.EntryPoint = typeof(CustomConduitSupport.CSVConduit).FullName;
            iRes.Library = "CustomConduitSupport.dll"; // provide some default values.
            return iRes;
        }

        /// <summary>Determines whether the specified <paramref name="source"/> is the same
        ///     pipe type as what the object would generate.</summary>
        /// <param name="source">The source.</param>
        /// <returns><c>true</c> if the specified <paramref name="source"/> is pipe;
        ///     otherwise, <c>false</c> .</returns>
        public override bool IsPipe(Queries.IQueryPipe source)
        {
            return source is Queries.CustomConduitPipe;
        }

        /// <summary>This function is only useful if<see cref="JaStDev.HAB.Designer.QueryDataSource.HasExtraData"/> is
        ///     true. if a datasource requires a custom <paramref name="editor"/>
        ///     object instead of the pipe itself, this function can be overwritten to
        ///     provide a custom <paramref name="editor"/> for the pipe. By default,
        ///     this is null, meaning that the pipe itself will be used as editing
        ///     object.</summary>
        /// <param name="editor"></param>
        /// <returns>The <see cref="object"/>.</returns>
        public override object GetEditor(QueryEditor editor)
        {
            var iQuery = (Queries.Query)editor.Item;

            var iSelector = new CustomConduitSelector();
            iSelector.ExtraData = iQuery;
            var iPipe = (Queries.CustomConduitPipe)iQuery.DataSource;
            if (iPipe != null)
            {
                iSelector.CustomDll = iPipe.Library;
                iSelector.SelectedEntryPoint = iSelector.EntryPointTypes.IndexOf(System.Type.GetType(iPipe.EntryPoint));
                iSelector.Source = iPipe.FileName;
                iSelector.Process = iPipe.Conduit; // so that the settings get assigned to the correct conduit.
                iSelector.Destination = iPipe.Destination;
            }

            iSelector.PropertyChanged += iSelector_PropertyChanged;
            return iSelector;
        }

        /// <summary>makes certain that the underlying pipe is also updated correctly.</summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void iSelector_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            var iSelector = (CustomConduitSelector)sender;
            var iQuery = (Queries.Query)iSelector.ExtraData;
            
            var iPipe = (Queries.CustomConduitPipe)iQuery.DataSource;
            if (e.PropertyName == "CustomDll")
            {
                iPipe.Library = iSelector.CustomDll;
            }
            else if (e.PropertyName == "SelectedEntryPoint")
            {
                iPipe.EntryPoint = iSelector.EntryPointTypes[iSelector.SelectedEntryPoint].AssemblyQualifiedName;
            }
            else if (e.PropertyName == "Process")
            {
                iPipe.Conduit = iSelector.Process;
            }
            else if (e.PropertyName == "Source")
            {
                iPipe.FileName = iSelector.Source;
            }
            else if (e.PropertyName == "Destination")
            {
                iPipe.Destination = iSelector.Destination;
            }
        }

        /// <summary>Converts an object into its XML representation.</summary>
        /// <param name="writer">The <see cref="System.Xml.XmlWriter"/> stream to which the object is serialized.</param>
        /// <param name="editor">The editor.</param>
        public override void WriteXml(System.Xml.XmlWriter writer, QueryEditor editor)
        {
            var iQuery = (Queries.Query)editor.Item;
            var iPipe = (Queries.CustomConduitPipe)iQuery.DataSource;
            if (string.IsNullOrEmpty(iPipe.Library) == false)
            {
                XmlStore.WriteElement(writer, "Library", iPipe.Library);
            }

            if (string.IsNullOrEmpty(iPipe.EntryPoint) == false)
            {
                XmlStore.WriteElement(writer, "EntryPoint", iPipe.EntryPoint);
            }

            if (string.IsNullOrEmpty(iPipe.FileName) == false)
            {
                XmlStore.WriteElement(writer, "FileName", iPipe.FileName);
            }

            iPipe.Conduit.WriteXml(writer);
        }

        /// <summary>Generates an object from its XML representation.</summary>
        /// <param name="reader">The <see cref="System.Xml.XmlReader"/> stream from which the object is
        ///     deserialized.</param>
        /// <param name="editor">The editor.</param>
        public override void ReadXml(System.Xml.XmlReader reader, QueryEditor editor)
        {
            var iQuery = (Queries.Query)editor.Item;
            var iPipe = (Queries.CustomConduitPipe)iQuery.DataSource;
            var iFound = string.Empty;
            if (XmlStore.TryReadElement(reader, "Library", ref iFound))
            {
                iPipe.Library = iFound;
            }

            if (XmlStore.TryReadElement(reader, "EntryPoint", ref iFound))
            {
                iPipe.EntryPoint = iFound;
            }

            if (XmlStore.TryReadElement(reader, "FileName", ref iFound))
            {
                iPipe.FileName = iFound;
            }

            iPipe.Conduit.ReadXml(reader);
        }
    }
}