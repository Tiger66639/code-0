// --------------------------------------------------------------------------------------------------------------------
// <copyright file="NDBSerializer.cs" company="">
//   
// </copyright>
// <summary>
//   (De)serializes neurons for the <see cref="JaStDev.HAB.Storage.NDB" />
//   storage system.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace JaStDev.HAB.Storage.NDB
{
    /// <summary>
    ///     (De)serializes neurons for the <see cref="JaStDev.HAB.Storage.NDB" />
    ///     storage system.
    /// </summary>
    internal class NDBSerializer
    {
        /// <summary>
        ///     Used to find the index of the typename, so we can reserialize again
        ///     later on. WARNING: this is a shared resource with multiple tables, so
        ///     when we access, lock the dict.
        /// </summary>
        private readonly System.Collections.Generic.Dictionary<ulong, System.Type> fTypeNames;

        /// <summary>Initializes a new instance of the <see cref="NDBSerializer"/> class.</summary>
        /// <param name="typeNames">The type names.</param>
        public NDBSerializer(System.Collections.Generic.Dictionary<ulong, System.Type> typeNames)
        {
            System.Diagnostics.Debug.Assert(typeNames != null);
            fTypeNames = typeNames;
        }

        /// <summary>Serializes the neuron to the specified stream.</summary>
        /// <param name="writer">The stream.</param>
        /// <param name="item">The item to save.</param>
        public void Serialize(CompactBinaryWriter writer, Neuron item)
        {
            System.Type iType;
            var iId = item.TypeOfNeuronID;
            lock (fTypeNames)
            {
                var iItemType = item.GetType();

                    // we don't use the fully qualified name, which was done in the beginning, cause than we are stuck to the specific assembly version in which the original neuron was declared, and can't upgrade the assembly.
                if (fTypeNames.TryGetValue(iId, out iType))
                {
                    // if the id is already used (like sins (ex) that don't reimplement 'TypeOfNeuron', than we try to generate one. -> sins do implement this now, but the trick is still a great way to gurantee unique types.
                    if (iType != iItemType)
                    {
                        iId = 1;
                        while (fTypeNames.ContainsKey(iId) && iId < ulong.MaxValue)
                        {
                            // find the first available empty id that we can use.
                            iId++;
                        }

                        if (iId < ulong.MaxValue)
                        {
                            fTypeNames.Add(iId, iItemType);
                        }
                    }
                }
                else
                {
                    fTypeNames.Add(iId, iItemType);
                }
            }

            writer.WriteUlong(iId);
            item.Write(writer);
        }

        /// <summary>Deserializes a neuron from the specified stream.</summary>
        /// <param name="reader">The stream to read the data from.</param>
        /// <returns>The <see cref="LinkResolverData"/>.</returns>
        public LinkResolverData Deserialize(CompactBinaryReader reader)
        {
            reader.Version = 0;

                // at first, we always read regular ulongs, to get the id nr, can't get out of that, this was part of the original format.
            System.Type iType;
            var iId = reader.ReadUInt64();
            lock (fTypeNames)
                if (fTypeNames.TryGetValue(iId, out iType) == false)
                {
                    throw new System.InvalidOperationException("Unkown neuron type found in stream.");
                }

            if (iType != null)
            {
                var iNeuron = NeuronFactory.Get(iType);
                if (iNeuron != null)
                {
                    var iRes = iNeuron.Read(reader);
                    return iRes;
                }

                throw new System.InvalidOperationException(
                    string.Format("Failed to create neuron for type: {0}", iType));
            }

            throw new System.InvalidOperationException("Unkown neuron type found in stream.");
        }
    }
}