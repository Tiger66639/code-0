// --------------------------------------------------------------------------------------------------------------------
// <copyright file="FunctionData.cs" company="">
//   
// </copyright>
// <summary>
//   Contains all the data to manage a single function in a
//   <see cref="TypeData" /> .
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace JaStDev.HAB.Designer
{
    using System.Linq;

    /// <summary>
    ///     Contains all the data to manage a single function in a
    ///     <see cref="TypeData" /> .
    /// </summary>
    public class FunctionData : ReflectionData, INeuronInfo, INeuronWrapper
    {
        #region ctor

        /// <summary>Initializes a new instance of the <see cref="FunctionData"/> class.</summary>
        /// <param name="method">The method.</param>
        /// <param name="owner">The owner.</param>
        public FunctionData(System.Reflection.MethodInfo method, object owner)
        {
            Owner = owner; // we set before the method so that we can check if the method is loaded or not.
            Method = method;
        }

        #endregion

        #region Fields

        /// <summary>The f method.</summary>
        private System.Reflection.MethodInfo fMethod;

        /// <summary>The f description.</summary>
        private string fDescription;

        #endregion

        #region Prop

        #region Description

        /// <summary>
        ///     Gets the description of this function. This can be used as a tooltip.
        /// </summary>
        public string Description
        {
            get
            {
                return fDescription;
            }

            internal set
            {
                fDescription = value;
                OnPropertyChanged("Description");
            }
        }

        #endregion

        #region Method

        /// <summary>
        ///     Gets the method info object that this object wraps.
        /// </summary>
        public System.Reflection.MethodInfo Method
        {
            get
            {
                return fMethod;
            }

            internal set
            {
                fMethod = value;
                if (value != null)
                {
                    Name = BuildName(value);
                }
                else
                {
                    Name = null;
                }

                SetDescription(value);
                var iSin = Sin;
                if (iSin != null)
                {
                    var iMap = iSin.GetMethodId(value);
                    if (Brain.Current.IsValidID(iMap))
                    {
                        Item = Brain.Current[iMap];
                    }
                    else
                    {
                        Item = null;
                    }
                }
                else
                {
                    Item = null;
                }

                OnPropertyChanged("IsLoaded");
                OnPropertyChanged("Method");
                OnPropertyChanged("Item");
            }
        }

        /// <summary>Builds the name, including all the arguments of the function.</summary>
        /// <param name="value">The value.</param>
        /// <returns>The <see cref="string"/>.</returns>
        private string BuildName(System.Reflection.MethodInfo value)
        {
            var iStr = new System.Text.StringBuilder(value.ReturnType.Name);
            iStr.Append(" ");
            iStr.Append(value.Name);
            iStr.Append("(");

            var iParams = value.GetParameters();
            if (iParams.Length > 0)
            {
                iStr.Append(iParams[0].ParameterType.Name);
                iStr.Append(" ");
                iStr.Append(iParams[0].Name);
                for (var i = 1; i < iParams.Length; i++)
                {
                    iStr.Append(", ");
                    iStr.Append(iParams[i].ParameterType.Name);
                    iStr.Append(" ");
                    iStr.Append(iParams[i].Name);
                }
            }

            iStr.Append(")");
            return iStr.ToString();
        }

        /// <summary>The set description.</summary>
        /// <param name="value">The value.</param>
        private void SetDescription(System.Reflection.MethodInfo value)
        {
            var attrs = System.Attribute.GetCustomAttributes(value, true);

            var iDescription = (from i in attrs
                                where i is System.ComponentModel.DescriptionAttribute
                                select (System.ComponentModel.DescriptionAttribute)i).FirstOrDefault();
            if (iDescription != null)
            {
                Description = iDescription.Description;
            }
        }

        #endregion

        #region IsLoaded

        /// <summary>
        ///     Gets or sets the function(s)/children are loaded.
        /// </summary>
        /// <value>
        ///     <c>true</c> : all the children are loaded - the function is loaded
        ///     <c>false</c> : none of the children are loaded - the function is not
        ///     loaded. <c>null</c> : some loaded - invalid.
        /// </value>
        public override bool? IsLoaded
        {
            get
            {
                return Item != null;
            }

            set
            {
                var iSin = Sin;
                if (IsLoaded != value && iSin != null && AskForUnload(Item))
                {
                    OnPropertyChanging("IsLoaded", IsLoaded, value);
                    if (value == true)
                    {
                        Item = iSin.LoadMethod(Method);
                        var iData = BrainData.Current.NeuronInfo[Item];
                        MappedName = BrainData.Current.DesignerData.Chatbotdata.DoFunctionMap.GetUnique(Method.Name);

                            // need to have a unique name for each function, otherwise we can't use it in the script cause can't find unique items.
                    }
                    else
                    {
                        BrainData.Current.DesignerData.Chatbotdata.DoFunctionMap.Remove(Item.ID);

                            // make certain that it gets removed before the neuron gets deleted, otherwise the map removal might cause a problem (it needs the NeuronData object, which could already be removed).
                        iSin.UnloadMethod(Item);
                        Item = null;
                    }

                    OnPropertyChanged("Item");
                    OnLoadedChanged();
                }
            }
        }

        /// <summary>Checks if there is an <paramref name="item"/> and if so, checks if
        ///     there are any links on it, if so, it asks to continue with the unload.</summary>
        /// <param name="item">The item.</param>
        /// <returns>The <see cref="bool"/>.</returns>
        private bool AskForUnload(Neuron item)
        {
            if (item != null)
            {
                LockManager.Current.RequestLock(item, LockLevel.All, false);
                try
                {
                    if ((item.LinksInIdentifier != null && item.LinksInIdentifier.Count > 0)
                        || (item.LinksOutIdentifier != null && item.LinksOutIdentifier.Count > 0)
                        || (item.ClusteredByIdentifier != null && item.ClusteredByDirect.Count > 0))
                    {
                        var iRes =
                            System.Windows.MessageBox.Show(
                                "There are links/clusters referencing this neuron, proceed with unloading?", 
                                "Unload neuron", 
                                System.Windows.MessageBoxButton.YesNo, 
                                System.Windows.MessageBoxImage.Warning);
                        return iRes == System.Windows.MessageBoxResult.Yes;
                    }
                }
                finally
                {
                    LockManager.Current.ReleaseLock(item, LockLevel.All, false);
                }
            }

            return true; // unload is allowed cause there is no item or no refs to it.
        }

        #endregion

        #region Name

        /// <summary>
        ///     Gets/sets the name of the function as mapped to the network: this is
        ///     the name that can be used in the scripts.
        /// </summary>
        public string MappedName
        {
            get
            {
                if (NeuronInfo != null)
                {
                    return NeuronInfo.DisplayTitle;
                }

                return string.Empty;
            }

            set
            {
                if (value != NeuronInfo.DisplayTitle && Item != null)
                {
                    if (BrainData.Current.DesignerData.Chatbotdata.DoFunctionMap.IsUnique(value))
                    {
                        BrainData.Current.DesignerData.Chatbotdata.DoFunctionMap.Remove(Item.ID);

                            // we remove and add again, so that the mapping gets rebuild (a bit bogus: better would be to monitor the DisplaytTitle)
                        NeuronInfo.DisplayTitle = value;
                        BrainData.Current.DesignerData.Chatbotdata.DoFunctionMap.Add(Item.ID);
                    }
                    else
                    {
                        throw new System.InvalidOperationException("Unique name required!");
                    }

                    OnPropertyChanged("MappedName");
                }
            }
        }

        #endregion

        #region NeuronInfo (INeuronInfo Members)

        /// <summary>
        ///     Gets the extra info for the specified neuron. Can be null.
        /// </summary>
        /// <value>
        /// </value>
        public NeuronData NeuronInfo
        {
            get
            {
                if (Item != null)
                {
                    return BrainData.Current.NeuronInfo[Item.ID];
                }

                return null;
            }
        }

        #endregion

        #region Item (INeuronWrapper Members)

        /// <summary>
        ///     Gets the item.
        /// </summary>
        /// <value>
        ///     The item.
        /// </value>
        public Neuron Item { get; private set; }

        #endregion

        #endregion
    }
}