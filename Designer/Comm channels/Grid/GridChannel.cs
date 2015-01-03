// --------------------------------------------------------------------------------------------------------------------
// <copyright file="GridChannel.cs" company="">
//   
// </copyright>
// <summary>
//   a channel that wraps round the grid-sin. ideal for board games like go.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace JaStDev.HAB.Designer
{
    /// <summary>
    ///     a channel that wraps round the grid-sin. ideal for board games like go.
    /// </summary>
    public class GridChannel : CommChannel
    {
        /// <summary>The f current state.</summary>
        private readonly System.Collections.ObjectModel.ObservableCollection<GridCell> fCurrentState =
            new System.Collections.ObjectModel.ObservableCollection<GridCell>();

        #region Width

        /// <summary>
        ///     Gets/sets the width in pixels of the image that is supplied to the
        ///     brain.
        /// </summary>
        [System.Xml.Serialization.XmlIgnore]
        public int Width
        {
            get
            {
                return ((GridSin)Sin).Width;
            }

            set
            {
                var iSin = (GridSin)Sin;
                var iPrev = iSin.Width;
                if (iPrev != value && value > 0)
                {
                    // need at least 1 pixel.
                    OnPropertyChanging("Width", iPrev, value);
                    UpdateWidth(iPrev, value);
                    iSin.Width = value;
                    OnPropertyChanged("Width");
                }
            }
        }

        #endregion

        #region Height

        /// <summary>
        ///     Gets/sets the height, expressed in pixels of the image supplied to the
        ///     brain
        /// </summary>
        [System.Xml.Serialization.XmlIgnore]
        public int Height
        {
            get
            {
                return ((GridSin)Sin).Height;
            }

            set
            {
                var iSin = (GridSin)Sin;
                var iPrev = iSin.Height;
                if (value != iPrev && value > 0)
                {
                    // need at least 1 pixel
                    OnPropertyChanging("Height", iPrev, value);
                    UpdateHeight(iPrev, value);
                    iSin.Height = value;
                    OnPropertyChanged("Height");
                }
            }
        }

        #endregion

        /// <summary>
        ///     Gets the current state of the grid. This can be used to bind to from
        ///     the UI.
        /// </summary>
        /// <value>
        ///     The state of the current.
        /// </value>
        [System.Xml.Serialization.XmlIgnore]
        public System.Collections.ObjectModel.ObservableCollection<GridCell> CurrentState
        {
            get
            {
                return fCurrentState;
            }
        }

        /// <summary>Sets the Sensory <see langword="interface"/> that this object is a
        ///     wrapper of.</summary>
        /// <param name="sin">The sin.</param>
        protected internal override void SetSin(Sin sin)
        {
            var iPrev = Sin as GridSin;
            if (iPrev != null)
            {
                iPrev.GridOutput -= Sin_GridOutput;
            }

            base.SetSin(sin);
            if (sin != null)
            {
                // when we have a sin, we can init the collection of items: we know with and height.
                var iNrItems = Width * Height;
                for (var i = 0; i < iNrItems; i++)
                {
                    var iNew = new GridCell(null, this);
                    CurrentState.Add(iNew);
                }

                var iSin = Sin as GridSin;
                if (iSin != null)
                {
                    iSin.GridOutput -= Sin_GridOutput;
                }
            }
        }

        /// <summary>The sin_ grid output.</summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The e.</param>
        private void Sin_GridOutput(object sender, OutputEventArgs<GridOutputData> e)
        {
            var iPos = e.Value.X * e.Value.Y;
            if (e.Value.Values.Count > 0)
            {
                CurrentState[iPos].Value = e.Value.Values[0];
            }
            else
            {
                CurrentState[iPos].Value = null;
            }
        }

        /// <summary>called when the value of a gridcell is changed by the user.</summary>
        /// <param name="cell"></param>
        internal void ChangeValue(GridCell cell)
        {
            var iSin = (GridSin)Sin;
            if (iSin != null)
            {
                var iProc = ProcessorFactory.GetProcessor();
                var iPos = CurrentState.IndexOf(cell);
                var iWidth = Width;
                var iY = iPos / iWidth;
                var iX = iPos % iWidth;
                iSin.Process(iX, iY, cell.Values, iProc);
            }
        }

        /// <summary>adds/removes the required amount of cells from the collection so that
        ///     it represents the width*height again.</summary>
        /// <param name="prev">The prev.</param>
        /// <param name="value"></param>
        private void UpdateWidth(int prev, int value)
        {
            var iDiv = value - prev;
            if (iDiv < 0)
            {
                // we made the width smealler, so remove items at the end of each row.
                for (var i = value; i < CurrentState.Count; i += value)
                {
                    for (var u = iDiv; u < 0; u++)
                    {
                        CurrentState.RemoveAt(i);
                    }
                }
            }
            else
            {
                var iRows = Height;
                var iWidth = Width;
                for (var i = 1; i <= iRows; i++)
                {
                    // we start base 1 so we add at the end of the row.
                    for (var u = 0; u < iDiv; u++)
                    {
                        CurrentState.Insert(i * iWidth, new GridCell(null, this));
                    }
                }
            }
        }

        /// <summary>Adds/removes the last rows from the collection so that it fits the
        ///     width * height again</summary>
        /// <param name="prev">The prev.</param>
        /// <param name="value">The value.</param>
        private void UpdateHeight(int prev, int value)
        {
            var iDiv = value - prev;
            if (iDiv < 0)
            {
                // we made the width smealler, so remove items at the end of each row.
                iDiv = System.Math.Abs(iDiv) * Width;
                for (var i = 0; i < iDiv; i++)
                {
                    CurrentState.RemoveAt(CurrentState.Count - 1);
                }
            }
            else
            {
                iDiv = iDiv * Width;
                for (var i = 0; i < iDiv; i++)
                {
                    CurrentState.Add(new GridCell(null, this));
                }
            }
        }
    }
}