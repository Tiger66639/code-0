// --------------------------------------------------------------------------------------------------------------------
// <copyright file="NamespaceData.cs" company="">
//   
// </copyright>
// <summary>
//   Contains all the types of a single <see langword="namespace" /> in an
//   assembly formatted to be displayed in a
//   <see cref="ReflectionChannelView" /> .
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace JaStDev.HAB.Designer
{
    /// <summary>
    ///     Contains all the types of a single <see langword="namespace" /> in an
    ///     assembly formatted to be displayed in a
    ///     <see cref="ReflectionChannelView" /> .
    /// </summary>
    public class NamespaceData : ReflectionData
    {
        #region ctor

        /// <summary>
        ///     Initializes a new instance of the <see cref="NamespaceData" /> class.
        /// </summary>
        public NamespaceData()
        {
            Children = new Data.ObservedCollection<TypeData>(this);
        }

        #endregion

        #region Children

        /// <summary>
        ///     Gets the list of all the types found in the
        ///     <see langword="namespace" />
        /// </summary>
        public Data.ObservedCollection<TypeData> Children { get; private set; }

        #endregion

        /// <summary>
        ///     Gets or sets the function(s)/children are loaded.
        /// </summary>
        /// <remarks>
        ///     Setting to <see langword="null" /> is not processed.
        /// </remarks>
        /// <value>
        ///     <c>true</c> : all the children are loaded - the function is loaded
        ///     <c>false</c> : none of the children are loaded - the function is not
        ///     loaded. <c>null</c> : some loaded - invalid.
        /// </value>
        public override bool? IsLoaded
        {
            get
            {
                if (Children.Count > 0)
                {
                    var iRes = Children[0].IsLoaded;
                    for (var i = 1; i < Children.Count; i++)
                    {
                        if (Children[i].IsLoaded != iRes)
                        {
                            return null;
                        }
                    }

                    return iRes;
                }

                return null;
            }

            set
            {
                if (value.HasValue)
                {
                    foreach (var i in Children)
                    {
                        i.IsLoaded = value;
                    }

                    OnLoadedChanged();
                }
            }
        }
    }
}