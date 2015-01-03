// --------------------------------------------------------------------------------------------------------------------
// <copyright file="IntNeuron.cs" company="">
//   
// </copyright>
// <summary>
//   A neuron that represents an integer Nr.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace JaStDev.HAB
{
    /// <summary>
    ///     A neuron that represents an integer Nr.
    /// </summary>
    [NeuronID((ulong)PredefinedNeurons.IntNeuron, typeof(Neuron))]
    public class IntNeuron : ValueNeuron
    {
        /// <summary>The f value.</summary>
        private int fValue;

        /// <summary>
        ///     Initializes a new instance of the <see cref="IntNeuron" /> class.
        /// </summary>
        internal IntNeuron()
        {
        }

        /// <summary>Initializes a new instance of the <see cref="IntNeuron"/> class.</summary>
        /// <param name="defaultValue">The initial value.</param>
        internal IntNeuron(int defaultValue)
        {
            fValue = defaultValue;
        }

        #region blob

        /// <summary>
        ///     Gets or sets the value of the neuron as a blob.
        /// </summary>
        /// <remarks>
        ///     This can be used during streaming of the neuron.
        /// </remarks>
        /// <value>
        ///     The object encapsulated by the neuron.
        /// </value>
        public override object Blob
        {
            get
            {
                return Value;
            }

            set
            {
                if (value is int)
                {
                    Value = (int)value;
                }
                else
                {
                    throw new System.InvalidOperationException();
                }
            }
        }

        #endregion

        #region TypeOfNeuron

        /// <summary>
        ///     Gets the type of this neuron expressed as a Neuron.
        /// </summary>
        /// <value>
        ///     <see cref="JaStDev.HAB.PredefinedNeurons.IntNeuron" /> .
        /// </value>
        public override ulong TypeOfNeuronID
        {
            get
            {
                return (ulong)PredefinedNeurons.IntNeuron;
            }
        }

        #endregion

        /// <summary>The set id.</summary>
        /// <param name="p">The p.</param>
        protected internal override void SetId(ulong p)
        {
            if (Settings.LogAddIntOrDouble && p != EmptyId && p != TempId)
            {
                LogService.Log.LogWarning("IntNeuron", "Got created");
            }

            base.SetId(p);
        }

        /// <summary>
        ///     Clears all the data from this instance.
        /// </summary>
        /// <remarks>
        ///     <para>
        ///         This function is automically called when a neuron is deleted. This
        ///         includes incomming and outgoing links, clustered by, children (if it
        ///         is a clusterr), and any possible values.
        ///     </para>
        ///     <para>
        ///         Descendents can enhance this function and clean more data.
        ///     </para>
        /// </remarks>
        protected internal override void Clear()
        {
            base.Clear();
            fValue = 0;
        }

        /// <summary>Copies all the data from this neuron to the argument.</summary>
        /// <remarks><para>By default, it only copies over all of the links (which includes the
        ///         'LinksOut' and 'LinksIn' lists.</para>
        /// <para>Inheriters should reimplement this function and copy any extra
        ///         information required for their specific type of neuron.</para>
        /// </remarks>
        /// <param name="copyTo">The object to copy their data to.</param>
        protected internal override void CopyTo(Neuron copyTo)
        {
            base.CopyTo(copyTo);
            var iCopyTo = copyTo as IntNeuron;
            if (iCopyTo != null)
            {
                iCopyTo.fValue = fValue;
            }
        }

        /// <summary>The to string.</summary>
        /// <returns>The <see cref="string"/>.</returns>
        public override string ToString()
        {
            return fValue.ToString();
        }

        /// <summary>The compare with.</summary>
        /// <param name="right">The neuron to compare it with.</param>
        /// <param name="op">The <see langword="operator"/> to use.</param>
        /// <returns>True if the <see langword="operator"/> is correct.</returns>
        protected internal override bool CompareWith(Neuron right, Neuron op)
        {
            if (right is IntNeuron)
            {
                var iVal = ((IntNeuron)right).Value;
                switch (op.ID)
                {
                    case (ulong)PredefinedNeurons.Equal:
                        return Value == iVal;
                    case (ulong)PredefinedNeurons.Smaller:
                        return Value < iVal;
                    case (ulong)PredefinedNeurons.SmallerOrEqual:
                        return Value <= iVal;
                    case (ulong)PredefinedNeurons.Bigger:
                        return Value > iVal;
                    case (ulong)PredefinedNeurons.BiggerOrEqual:
                        return Value >= iVal;
                    case (ulong)PredefinedNeurons.Different:
                        return Value != iVal;
                    default:
                        LogService.Log.LogError(
                            "IntNeuron.CompareWith", 
                            string.Format("Invalid operator found: {0}.", op));
                        return false;
                }
            }

            if (right is DoubleNeuron)
            {
                var iVal = ((DoubleNeuron)right).Value;
                switch (op.ID)
                {
                    case (ulong)PredefinedNeurons.Equal:
                        return Value == iVal;
                    case (ulong)PredefinedNeurons.Smaller:
                        return Value < iVal;
                    case (ulong)PredefinedNeurons.SmallerOrEqual:
                        return Value <= iVal;
                    case (ulong)PredefinedNeurons.Bigger:
                        return Value > iVal;
                    case (ulong)PredefinedNeurons.BiggerOrEqual:
                        return Value >= iVal;
                    case (ulong)PredefinedNeurons.Different:
                        return Value != iVal;
                    default:
                        LogService.Log.LogError(
                            "IntNeuron.CompareWith", 
                            string.Format("Invalid operator found: {0}.", op));
                        return false;
                }
            }

            return base.CompareWith(right, op);
        }

        /// <summary>The read xml.</summary>
        /// <param name="reader">The reader.</param>
        public override void ReadXml(System.Xml.XmlReader reader)
        {
            base.ReadXml(reader);
            fValue = XmlStore.ReadElement<int>(reader, "Value");
        }

        /// <summary>The write xml.</summary>
        /// <param name="writer">The xml writer to use</param>
        public override void WriteXml(System.Xml.XmlWriter writer)
        {
            base.WriteXml(writer);
            XmlStore.WriteElement(writer, "Value", fValue);
        }

        /// <summary>Reads the neuron in file version 1 format.</summary>
        /// <param name="reader">The reader.</param>
        /// <returns>The <see cref="LinkResolverData"/>.</returns>
        protected override LinkResolverData ReadV1(System.IO.BinaryReader reader)
        {
            var iRes = base.ReadV1(reader);
            fValue = reader.ReadInt32();
            return iRes;
        }

        /// <summary>Writes the neuron in version 1 format.</summary>
        /// <param name="writer">The writer.</param>
        protected override void WriteV1(System.IO.BinaryWriter writer)
        {
            base.WriteV1(writer);
            writer.Write(fValue);
        }

        /// <summary>returns if this object can return an int.</summary>
        /// <returns>The <see cref="bool"/>.</returns>
        public override bool CanGetInt()
        {
            return true;
        }

        /// <summary>Gets the <see langword="int"/> value.</summary>
        /// <param name="processor"></param>
        /// <returns>The <see cref="int"/>.</returns>
        public override int GetInt(Processor processor)
        {
            return Value;
        }

        /// <summary>Compares this neuron with the <paramref name="other"/> neuron. By
        ///     default, this first converts the neuron to a string and compares the 2
        ///     string values.</summary>
        /// <param name="other">The other.</param>
        /// <returns>The <see cref="int"/>.</returns>
        public override int CompareTo(Neuron other)
        {
            if (other is IntNeuron)
            {
                return Value.CompareTo(((IntNeuron)other).Value);
            }

            return base.CompareTo(other);
        }

        #region Value

        /// <summary>
        ///     Gets/sets the <see langword="double" /> value that this neuron
        ///     represents.
        /// </summary>
        public int Value
        {
            get
            {
                return fValue;
            }

            set
            {
                if (fValue != value)
                {
                    LockManager.Current.RequestLock(this, LockLevel.Value, true);

                        // we need to make the value assign + IsChanged assign a singleton operation, so that the Flusher can't get in between (if he can, we get the situation that the neuron is permantly stored in the cache, but not set as chagned, in wich case we can't unload.
                    try
                    {
                        fValue = value;
                    }
                    finally
                    {
                        LockManager.Current.ReleaseLock(this, LockLevel.Value, true);
                    }

                    SetIsChangedNoUnfreeze(true);

                        // don't unfreeze, otherwise changing the value would cause to many mem leaks.
                    if (Brain.Current.HasNeuronChangedEvents)
                    {
                        Brain.Current.OnNeuronChanged(new NeuronPropChangedEventArgs("Value", this));
                    }
                }
            }
        }

        /// <summary>used by the various calculation functions to set the value directly
        ///     without any locking.</summary>
        /// <param name="val"></param>
        internal void SetValueDirect(int val)
        {
            fValue = val;

            // SetChangedUnsave();
        }

        /// <summary>stores the initial value for the <see langword="int"/> in a thread
        ///     unsafe way.</summary>
        /// <param name="val"></param>
        internal void InitValue(int val)
        {
            fValue = val;
        }

        /// <summary>
        ///     Thread save way to increment the value.
        /// </summary>
        public void IncValue()
        {
            LockManager.Current.RequestLock(this, LockLevel.Value, true);

                // we need to make the value assign + IsChanged assign a singleton operation, so that the Flusher can't get in between (if he can, we get the situation that the neuron is permantly stored in the cache, but not set as chagned, in wich case we can't unload.
            try
            {
                fValue++;
            }
            finally
            {
                LockManager.Current.ReleaseLock(this, LockLevel.Value, true);
            }

            SetIsChangedNoUnfreeze(true); // don't unfreeze, otherwise changing the value would cause to many mem leaks.
            if (Brain.Current.HasNeuronChangedEvents)
            {
                Brain.Current.OnNeuronChanged(new NeuronPropChangedEventArgs("Value", this));
            }
        }

        /// <summary>
        ///     Thread save way to decrement the value.
        /// </summary>
        public void DecValue()
        {
            LockManager.Current.RequestLock(this, LockLevel.Value, true);

                // we need to make the value assign + IsChanged assign a singleton operation, so that the Flusher can't get in between (if he can, we get the situation that the neuron is permantly stored in the cache, but not set as chagned, in wich case we can't unload.
            try
            {
                fValue--;
            }
            finally
            {
                LockManager.Current.ReleaseLock(this, LockLevel.Value, true);
            }

            SetIsChangedNoUnfreeze(true); // don't unfreeze, otherwise changing the value would cause to many mem leaks.
            if (Brain.Current.HasNeuronChangedEvents)
            {
                Brain.Current.OnNeuronChanged(new NeuronPropChangedEventArgs("Value", this));
            }
        }

        #endregion
    }
}