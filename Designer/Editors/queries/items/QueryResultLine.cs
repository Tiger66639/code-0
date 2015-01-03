// --------------------------------------------------------------------------------------------------------------------
// <copyright file="QueryResultLine.cs" company="">
//   
// </copyright>
// <summary>
//   a wrapper class for a single result line of a query. This provides
//   selection features.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace JaStDev.HAB.Designer
{
    /// <summary>
    ///     a wrapper class for a single result line of a query. This provides
    ///     selection features.
    /// </summary>
    public class QueryResultLine : Data.OwnedObject<QueryEditor>
    {
        #region LineData

        /// <summary>
        ///     <para>
        ///         Gets the line data, represented as either ulongs or strings
        ///         (determined by the
        ///         <see cref="JaStDev.HAB.Designer.QueryResultLine.DataFormat" />
        ///     </para>
        ///     <para>This is how it is stored internally.</para>
        /// </summary>
        public System.IO.MemoryStream LineData { get; internal set; }

        #endregion

        /// <summary>
        ///     defines the type for each row: a string (0 = false) or
        ///     <see langword="ulong" /> = 1 = <see langword="true" />
        /// </summary>
        public System.Collections.BitArray DataFormat { get; private set; }

        /// <summary>
        ///     gets the nr of columsn. so that we can bind to this in wpf
        /// </summary>
        public int ColCount
        {
            get
            {
                return DataFormat.Count;
            }
        }

        /// <summary>
        ///     gets the data as debug neurons. This property is recreated with each
        ///     access, it is not stored.
        /// </summary>
        public System.Collections.Generic.List<object> Items
        {
            get
            {
                var iData = new object[Owner.Columns.Count];
                if (LineData != null)
                {
                    LineData.Position = 0; // always move to the first pos, otherwise we won't read the correct data
                    var iReader = new System.IO.BinaryReader(LineData);
                    for (var i = 0; i < DataFormat.Count; i++)
                    {
                        if (DataFormat[i])
                        {
                            var iId = iReader.ReadUInt64();
                            Neuron iFound;
                            if (Brain.Current.TryFindNeuron(iId, out iFound))
                            {
                                iData[Owner.Columns[i].Index] = new DebugNeuron(iFound);
                            }
                        }
                        else
                        {
                            iData[Owner.Columns[i].Index] = iReader.ReadString();
                        }
                    }
                }

                return new System.Collections.Generic.List<object>(iData);
            }
        }

        /// <summary>adds the specified data to the row.</summary>
        /// <param name="source"></param>
        internal void Add(System.Collections.Generic.IList<Neuron> source)
        {
            LineData = new System.IO.MemoryStream();
            var iWriter = new System.IO.BinaryWriter(LineData);
            DataFormat = new System.Collections.BitArray(source.Count);
            for (var iCount = 0; iCount < source.Count; iCount++)
            {
                var i = source[iCount];
                if (i.ID == Neuron.TempId)
                {
                    if (i is ValueNeuron)
                    {
                        iWriter.Write(((ValueNeuron)i).ToString());
                    }
                    else
                    {
                        iWriter.Write(string.Empty);
                    }

                    DataFormat[iCount] = false;
                }
                else
                {
                    iWriter.Write(i.ID);
                    DataFormat[iCount] = true;
                }
            }
        }
    }
}