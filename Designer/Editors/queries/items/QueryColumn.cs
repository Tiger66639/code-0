// --------------------------------------------------------------------------------------------------------------------
// <copyright file="QueryColumn.cs" company="">
//   
// </copyright>
// <summary>
//   defines a single result column.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace JaStDev.HAB.Designer
{
    /// <summary>
    ///     defines a single result column.
    /// </summary>
    public class QueryColumn : EditorColumn
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="QueryColumn" /> class.
        /// </summary>
        public QueryColumn()
        {
        }

        /// <summary>Initializes a new instance of the <see cref="QueryColumn"/> class.</summary>
        /// <param name="index">The index.</param>
        public QueryColumn(int index)
        {
            Index = index;
            Width = 60;
            Name = "Column " + (Index + 1);
        }

        /// <summary>Reads the data from the stream.</summary>
        /// <param name="source">The source to read from.</param>
        internal void Read(System.IO.BinaryReader source)
        {
            Name = source.ReadString();
            Index = source.ReadInt32();
            Width = source.ReadDouble();
        }

        /// <summary>Writes the data to the stream.</summary>
        /// <param name="dest">The dest.</param>
        internal void Write(System.IO.BinaryWriter dest)
        {
            dest.Write(Name);
            dest.Write(Index);
            dest.Write(Width);
        }
    }
}