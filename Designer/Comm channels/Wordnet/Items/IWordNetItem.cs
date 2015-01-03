// --------------------------------------------------------------------------------------------------------------------
// <copyright file="IWordNetItem.cs" company="">
//   
// </copyright>
// <summary>
//   The WordNetItemOwner interface.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace JaStDev.HAB.Designer
{
    /// <summary>The WordNetItemOwner interface.</summary>
    public interface IWordNetItemOwner
    {
        /// <summary>
        ///     Gets the list of children for this item.
        /// </summary>
        System.Collections.ObjectModel.ObservableCollection<WordNetItem> Children { get; }
    }
}