// --------------------------------------------------------------------------------------------------------------------
// <copyright file="EditorColumn.cs" company="">
//   
// </copyright>
// <summary>
//   can be used as the base for objects that represent a column.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace JaStDev.HAB.Designer
{
    /// <summary>
    ///     can be used as the base for objects that represent a column.
    /// </summary>
    public class EditorColumn : Data.NamedObject
    {
        /// <summary>The f index.</summary>
        private int fIndex;

        /// <summary>The f width.</summary>
        private double fWidth;

        #region Width

        /// <summary>
        ///     Gets/sets the width of the value column on the UI.
        /// </summary>
        public double Width
        {
            get
            {
                return fWidth;
            }

            set
            {
                if (value != fWidth)
                {
                    fWidth = value;
                    OnPropertyChanged("Width");
                }
            }
        }

        #endregion

        #region Index

        /// <summary>
        ///     Gets/sets the index of the column in the list.
        /// </summary>
        public virtual int Index
        {
            get
            {
                return fIndex;
            }

            set
            {
                if (value != fIndex)
                {
                    fIndex = value;
                    OnPropertyChanged("Index");
                }
            }
        }

        #endregion
    }
}