// --------------------------------------------------------------------------------------------------------------------
// <copyright file="AssetPanel.cs" company="">
//   
// </copyright>
// <summary>
//   A treeViewPanel For the asset editor. It uses
//   <see cref="AssetPanelItem" /> objects.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace JaStDev.HAB.Designer.WPF.Controls
{
    /// <summary>
    ///     A treeViewPanel For the asset editor. It uses
    ///     <see cref="AssetPanelItem" /> objects.
    /// </summary>
    public class AssetPanel : TreeViewPanel
    {
        /// <summary>Creates a new container. Overwite this if you want custom containers
        ///     for the objects.</summary>
        /// <param name="data">The data.</param>
        /// <returns>The <see cref="TreeViewPanelItem"/>.</returns>
        protected override TreeViewPanelItem CreateContainer(ITreeViewPanelItem data)
        {
            return new AssetPanelItem();
        }
    }
}