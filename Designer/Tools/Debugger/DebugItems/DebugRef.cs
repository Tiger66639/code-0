// --------------------------------------------------------------------------------------------------------------------
// <copyright file="DebugRef.cs" company="">
//   
// </copyright>
// <summary>
//   Base class for items that represent children of clusters or 'to' parts of
//   links.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace JaStDev.HAB.Designer
{
    /// <summary>
    ///     Base class for items that represent children of clusters or 'to' parts of
    ///     links.
    /// </summary>
    public class DebugRef : DebugItem
    {
        #region fields

        /// <summary>The f points to.</summary>
        private DebugNeuron fPointsTo;

        #endregion

        #region PointsTo

        /// <summary>
        ///     Gets the other side of the link compared to the owner of this object.
        /// </summary>
        public DebugNeuron PointsTo
        {
            get
            {
                return fPointsTo;
            }

            internal set
            {
                fPointsTo = value;
                OnPropertyChanged("PointsTo");
            }
        }

        #endregion

        #region Owner

        /// <summary>
        ///     Gets the owner of this <see cref="DebugLink" /> .
        /// </summary>
        public DebugNeuron Owner { get; internal set;

            // don't need to raise prop changed,  should only be set at start.
        }

        #endregion
    }
}