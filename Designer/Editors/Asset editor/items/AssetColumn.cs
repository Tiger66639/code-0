// --------------------------------------------------------------------------------------------------------------------
// <copyright file="AssetColumn.cs" company="">
//   
// </copyright>
// <summary>
//   All the data for a single column
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace JaStDev.HAB.Designer
{
    /// <summary>
    ///     All the data for a single column
    /// </summary>
    public class AssetColumn : EditorColumn
    {
        #region LinkID

        /// <summary>
        ///     Gets/sets the id of the link that should be used to get/set the data.
        /// </summary>
        public ulong LinkID { get; set; }

        #endregion

        /// <summary>
        ///     Gets/sets the index of the column in the list.
        /// </summary>
        public override int Index
        {
            get
            {
                return base.Index;
            }

            set
            {
                if (value != base.Index)
                {
                    base.Index = value; // originally, the OnPropChanged happened after the if statement.
                    if (value == 1 && Owner != null)
                    {
                        var iOwner = (ObjectEditor)Owner;
                        iOwner.TreeColumn = iOwner.Columns.IndexOf(this);
                    }
                }
            }
        }
    }
}