// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ISelectDisplayPathForThes.cs" company="">
//   
// </copyright>
// <summary>
//   this <see langword="interface" /> should be implemetned by all
//   <see cref="DisplayPathItem" /> objects that can select a thesaurus item.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace JaStDev.HAB.Designer.Search
{
    /// <summary>
    ///     this <see langword="interface" /> should be implemetned by all
    ///     <see cref="DisplayPathItem" /> objects that can select a thesaurus item.
    /// </summary>
    internal interface ISelectDisplayPathForThes
    {
        /// <summary>Selects the object related to the specified <paramref name="item"/>
        ///     and returns this.</summary>
        /// <param name="dataSource">The data Source.</param>
        /// <param name="item"></param>
        /// <returns>The <see cref="object"/>.</returns>
        object SelectFrom(Thesaurus dataSource, object item);
    }
}