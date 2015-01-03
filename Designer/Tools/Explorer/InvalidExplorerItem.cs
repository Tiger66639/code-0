// --------------------------------------------------------------------------------------------------------------------
// <copyright file="InvalidExplorerItem.cs" company="">
//   
// </copyright>
// <summary>
//   An explorer item that indicates an error was encountered while trying to
//   retrieve this value. has as extar info a description of the error that
//   was encountered + the log item?.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace JaStDev.HAB.Designer
{
    /// <summary>
    ///     An explorer item that indicates an error was encountered while trying to
    ///     retrieve this value. has as extar info a description of the error that
    ///     was encountered + the log item?.
    /// </summary>
    public class InvalidExplorerItem : ExplorerItem
    {
        /// <summary>Initializes a new instance of the <see cref="InvalidExplorerItem"/> class.</summary>
        /// <param name="id">The id.</param>
        /// <param name="error">The error.</param>
        public InvalidExplorerItem(ulong id, string error)
            : base(id)
        {
            Error = error;
        }

        #region Error

        /// <summary>
        ///     Gets the error message that was returned while trying to retrieve the
        ///     item.
        /// </summary>
        public string Error { get; private set; }

        #endregion
    }
}