// --------------------------------------------------------------------------------------------------------------------
// <copyright file="TypeData.cs" company="">
//   
// </copyright>
// <summary>
//   Manages all the data of a single type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace JaStDev.HAB.Designer
{
    /// <summary>
    ///     Manages all the data of a single type.
    /// </summary>
    public class TypeData : ReflectionData
    {
        #region Fields

        /// <summary>The f type.</summary>
        private System.Type fType;

        #endregion

        #region ctor

        /// <summary>Initializes a new instance of the <see cref="TypeData"/> class.</summary>
        /// <param name="type">The type.</param>
        /// <param name="owner">The owner.</param>
        public TypeData(System.Type type, object owner)
        {
            Owner = owner;

                // we set this before Type, so that 'FunctionData' can check if it is loaded or not when adding the items.
            Children = new Data.ObservedCollection<FunctionData>(this);
            Type = type;
        }

        #endregion

        #region Properties

        #region Children

        /// <summary>
        ///     Gets the list of all the types found in the
        ///     <see langword="namespace" />
        /// </summary>
        public Data.ObservedCollection<FunctionData> Children { get; private set; }

        #endregion

        #region Type

        /// <summary>
        ///     Gets the type that this object wraps.
        /// </summary>
        public System.Type Type
        {
            get
            {
                return fType;
            }

            internal set
            {
                fType = value;
                if (value != null)
                {
                    Name = value.Name;
                    foreach (
                        var i in
                            value.GetMethods(
                                System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.Public))
                    {
                        var iAllValue = true;
                        foreach (var iPar in i.GetParameters())
                        {
                            if (iPar.ParameterType.IsValueType == false && iPar.ParameterType.Name != "String"
                                && iPar.ParameterType.Name != "String[]"
                                && typeof(Neuron).IsAssignableFrom(iPar.ParameterType) == false)
                            {
                                iAllValue = false;
                                break;
                            }
                        }

                        if (iAllValue)
                        {
                            var iFunc = new FunctionData(i, this);
                            Children.Add(iFunc);
                        }
                    }
                }
                else
                {
                    Name = null;
                }

                OnPropertyChanged("Type");
                OnLoadedChanged();
            }
        }

        #endregion

        #region IsLoaded

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
                }

                OnLoadedChanged();
            }
        }

        #endregion

        #endregion
    }
}