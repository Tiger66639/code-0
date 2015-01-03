// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ProcessTrackerItem.cs" company="">
//   
// </copyright>
// <summary>
//   base class for the searcher process. allows us to register a general
//   puprose, long running process to display status info and cancel the
//   oepration.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace JaStDev.HAB.Designer.Search
{
    /// <summary>
    ///     base class for the searcher process. allows us to register a general
    ///     puprose, long running process to display status info and cancel the
    ///     oepration.
    /// </summary>
    public class ProcessTrackerItem : Data.OwnedObject<ProcessTracker>
    {
        /// <summary>Initializes a new instance of the <see cref="ProcessTrackerItem"/> class.</summary>
        /// <param name="owner">The owner.</param>
        internal ProcessTrackerItem(ProcessTracker owner)
        {
            Owner = owner;
        }

        #region TotalCount

        /// <summary>
        ///     Gets/sets the total nr of items that still need to be processed, for
        ///     all currently running searches.
        /// </summary>
        public double TotalCount
        {
            get
            {
                return fTotalCount;
            }

            set
            {
                if (fTotalCount != value)
                {
                    Owner.UpdateTotalCount(value - fTotalCount);
                    fTotalCount = value;
                    OnPropertyChanged("TotalCount");
                }
            }
        }

        #endregion

        #region CurrentPos

        /// <summary>
        ///     Gets/sets the position of the current item that is being/has been
        ///     processed.
        /// </summary>
        public double CurrentPos
        {
            get
            {
                return fCurrentPos;
            }

            set
            {
                if (fCurrentPos != value)
                {
                    Owner.UpdateCurrentPos(value - fCurrentPos);
                    fCurrentPos = value;
                    OnPropertyChanged("CurrentPos");
                }
            }
        }

        #endregion

        #region IsRunning

        /// <summary>
        ///     Gets the value that indicates if this search is still running or not.
        /// </summary>
        public virtual bool IsRunning
        {
            get
            {
                return fIsRunning;
            }

            internal set
            {
                if (value != fIsRunning)
                {
                    fIsRunning = value;
                    if (value)
                    {
                        Owner.IncRunCount();
                    }
                    else
                    {
                        Owner.DecRunCount();
                        CurrentPos = 0;

                            // reset the current position when we stop. This also removes the position from the global list.
                    }
                }
            }
        }

        #endregion

        #region IsCanceled

        /// <summary>
        ///     Gets the value that indicates if this search has been canceled (but
        ///     not yet stopped) or not.
        /// </summary>
        public bool IsCanceled
        {
            get
            {
                return fIsCanceled;
            }

            internal set
            {
                fIsCanceled = value;
                OnPropertyChanged("IsCanceled");
            }
        }

        #endregion

        #region fields

        /// <summary>The f total count.</summary>
        private double fTotalCount;

        /// <summary>The f current pos.</summary>
        private double fCurrentPos;

        /// <summary>The f is canceled.</summary>
        private bool fIsCanceled;

        /// <summary>The f is running.</summary>
        private bool fIsRunning;

        #endregion
    }
}