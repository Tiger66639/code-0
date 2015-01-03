// --------------------------------------------------------------------------------------------------------------------
// <copyright file="SearchExpression.cs" company="">
//   
// </copyright>
// <summary>
//   An expression that performs a search on links.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace JaStDev.HAB
{
    /// <summary>
    ///     An expression that performs a search on links.
    /// </summary>
    [NeuronID((ulong)PredefinedNeurons.SearchExpression, typeof(Neuron))]
    public class SearchExpression : SimpleResultExpression
    {
        /// <summary>Initializes a new instance of the <see cref="SearchExpression"/> class. Constructor that creates correct initial links.</summary>
        /// <param name="toSearch">The object to search, if expression, it will be executed before the
        ///     search.</param>
        /// <param name="listId">The neuron that identifies the list that should be searched. If this
        ///     is an expression, it is executed first.</param>
        /// <param name="searchFor">Defines all the meanings that should be allowed in the result set.</param>
        public SearchExpression(Neuron toSearch, Neuron listId, Neuron searchFor)
        {
            var iNew = new Link(toSearch, this, (ulong)PredefinedNeurons.ToSearch);
            iNew = new Link(listId, this, (ulong)PredefinedNeurons.ListToSearch);
            iNew = new Link(searchFor, this, (ulong)PredefinedNeurons.SearchFor);
        }

        /// <summary>Prevents a default instance of the <see cref="SearchExpression"/> class from being created. 
        ///     Default constructor.</summary>
        private SearchExpression()
        {
        }

        #region SearchFor

        /// <summary>
        ///     Gets/sets the (list of) meaning(s) that will be searched for. May be
        ///     <see langword="null" />
        /// </summary>
        /// <remarks>
        ///     If this is a <see cref="ResultExpression" /> , it will be solved
        ///     before the search is done. If this is a regular neuron, this is used
        ///     as the search criteria. To search for all the children of a cluster,
        ///     use a sub search expression.
        /// </remarks>
        public Neuron SearchFor
        {
            get
            {
                return FindFirstOut((ulong)PredefinedNeurons.SearchFor);
            }

            set
            {
                SetFirstOutgoingLinkTo((ulong)PredefinedNeurons.SearchFor, value);
            }
        }

        #endregion

        #region InfoToSearchFor

        /// <summary>
        ///     Gets/sets the (list of) info values that will be searched for. May be
        ///     null. Only valid for In and Out
        ///     <see cref="JaStDev.HAB.SearchExpression.ListToSearch" /> .
        /// </summary>
        /// <remarks>
        ///     If this is a <see cref="ResultExpression" /> , it will be solved
        ///     before the search is done. If this is a regular neuron, this is used
        ///     as the search criteria. To search for all the children of a cluster,
        ///     use a sub search expression.
        /// </remarks>
        public Neuron InfoToSearchFor
        {
            get
            {
                return FindFirstOut((ulong)PredefinedNeurons.InfoToSearchFor);
            }

            set
            {
                SetFirstOutgoingLinkTo((ulong)PredefinedNeurons.InfoToSearchFor, value);
            }
        }

        #endregion

        #region ListToSearch

        /// <summary>
        ///     Gets/sets the list that needs searching. This usually references the
        ///     word 'In' or 'Out' for their respective lists.
        /// </summary>
        /// <remarks>
        ///     If this is a <see cref="ResultExpression" /> , it will be solved
        ///     before the search is done.
        /// </remarks>
        public Neuron ListToSearch
        {
            get
            {
                return FindFirstOut((ulong)PredefinedNeurons.ListToSearch);
            }

            set
            {
                SetFirstOutgoingLinkTo((ulong)PredefinedNeurons.ListToSearch, value);
            }
        }

        #endregion

        /// <summary>
        ///     Gets/sets the object who's list will be searched.
        /// </summary>
        /// <remarks>
        ///     If this is a <see cref="ResultExpression" /> , it will be solved
        ///     before the search is done. To search in the list of a result
        ///     expression itself, use a <see cref="Variable" /> .
        /// </remarks>
        public Neuron ToSearch
        {
            get
            {
                return FindFirstOut((ulong)PredefinedNeurons.ToSearch);
            }

            set
            {
                SetFirstOutgoingLinkTo((ulong)PredefinedNeurons.ToSearch, value);
            }
        }

        #region TypeOfNeuron

        /// <summary>
        ///     Gets the type of this neuron expressed as a Neuron.
        /// </summary>
        /// <value>
        ///     <see cref="JaStDev.HAB.PredefinedNeurons.SearchExpression" /> .
        /// </value>
        public override ulong TypeOfNeuronID
        {
            get
            {
                return (ulong)PredefinedNeurons.SearchExpression;
            }
        }

        #endregion

        /// <summary>The get value.</summary>
        /// <param name="processor">The processor.</param>
        /// <exception cref="NotSupportedException"></exception>
        internal override void GetValue(Processor processor)
        {
            // List<Neuron> iSearchList = Neuron.SolveResultExp(ToSearch, processor).ToList();
            // if (iSearchList != null)                                                            //if there is no object to search, don't write an error, simply return an empty result.
            // {
            // List<Neuron> iListsToSearch = Neuron.SolveResultExp(ListToSearch, processor).ToList();
            // if (iListsToSearch != null)
            // {
            // SearchData iSearchData = new SearchData();
            // iSearchData.Result = new List<Neuron>();
            // iSearchData.SearchFor = (from i in Neuron.SolveResultExp(SearchFor, processor) select i.ID).ToList();
            // iSearchData.InfoToSearchFor = (from i in Neuron.SolveResultExp(InfoToSearchFor, processor) select i.ID).ToList();
            // foreach (Neuron iToSearch in iSearchList)
            // {
            // foreach (Neuron iList in iListsToSearch)
            // {
            // switch (iList.ID)
            // {
            // case (ulong)PredefinedNeurons.In:
            // using (LinksAccessorOld iLinksIn = iToSearch.LinksIn)
            // {
            // iSearchData.ToSearch = iLinksIn.Items;
            // SearchForItemsIn(iSearchData, true);
            // }
            // break;
            // case (ulong)PredefinedNeurons.Out:
            // using (LinksAccessorOld iLinksOut = iToSearch.LinksOut)
            // iSearchData.ToSearch = iLinksOut.Items;
            // SearchForItemsIn(iSearchData, false);
            // break;
            // default:
            // break;
            // }
            // }
            // }
            // return iSearchData.Result;
            // }
            // }
            // return null;
            LogService.Log.LogError(
                "SearchExpression", 
                "expression has been depricated. It is no longer supported, please use one of the instructions to perform searches.");
            throw new System.NotSupportedException();
        }

        ///// <summary>
        ///// Searches a list of neurons using the specified criteria.
        ///// </summary>
        ///// <param name="search">The search data, containing the result set.</param>
        ///// <param name="list">The actual list that needs to be searched.</param>
        // private void SearchForItemsIn(SearchData search, IList<Neuron> list)
        // {
        // }

        /// <summary>Searches a list of links and returns</summary>
        /// <param name="searchData">Searchdata: result list, meanings to search, info to search, list to
        ///     search.</param>
        /// <param name="getFrom">true: return the from field, otherwise the to field</param>
        private void SearchForItemsIn(SearchData searchData, bool getFrom)
        {
            // if (searchData.ToSearch != null && searchData.ToSearch.Count > 0)
            // {
            // bool iHasSearchFor =searchData.SearchFor != null && searchData.SearchFor.Count > 0;
            // bool iHasSearchInfo = searchData.InfoToSearchFor != null && searchData.InfoToSearchFor.Count > 0;
            // if (iHasSearchFor == true && iHasSearchInfo == true)
            // {
            // if (getFrom == true)
            // searchData.Result.Concat(from i in searchData.ToSearch
            // where searchData.SearchFor.Contains(i.MeaningID) == true && i.Info.Contains(searchData.InfoToSearchFor) == true
            // select i.From);
            // else
            // searchData.Result.Concat(from i in searchData.ToSearch
            // where searchData.SearchFor.Contains(i.MeaningID) == true && i.Info.Contains(searchData.InfoToSearchFor) == true
            // select i.To);
            // }
            // else if (iHasSearchFor == true)
            // {
            // if (getFrom == true)
            // searchData.Result.Concat(from i in searchData.ToSearch where searchData.SearchFor.Contains(i.MeaningID) == true select i.From);
            // else
            // searchData.Result.Concat(from i in searchData.ToSearch where searchData.SearchFor.Contains(i.MeaningID) == true select i.To);
            // }
            // else if (iHasSearchInfo == true)
            // {
            // if (getFrom == true)
            // searchData.Result.Concat(from i in searchData.ToSearch where i.Info.Contains(searchData.InfoToSearchFor) == true select i.From);       //direct access to Info is thread safe: object is IDisposable.
            // else
            // searchData.Result.Concat(from i in searchData.ToSearch where i.Info.Contains(searchData.InfoToSearchFor) == true select i.To);
            // }
            // else if (getFrom == true)
            // searchData.Result.Concat(from i in searchData.ToSearch select i.From);                                                                     //nothing to filter on, so copy over the intire search list
            // else
            // searchData.Result.Concat(from i in searchData.ToSearch select i.To);                                                                     //nothing to filter on, so copy over the intire search list
            // }
            LogService.Log.LogError(
                "SearchExpression", 
                "expression has been depricated. It is no longer supported, please use one of the instructions to perform searches.");
        }

        /// <summary>The load code.</summary>
        /// <param name="alreadyProcessed">The already processed.</param>
        protected internal override void LoadCode(System.Collections.Generic.HashSet<Neuron> alreadyProcessed)
        {
            // don't do anything, isn't used anymore
        }

        /// <summary>small optimizer, checks if the code is loaded alrady or not. This is
        ///     used to see if a start point needs to be loaded or not, whithout
        ///     having to set up mem all the time.</summary>
        /// <returns>The <see cref="bool"/>.</returns>
        protected internal override bool IsCodeLoaded()
        {
            return false;
        }

        #region internal types

        /// <summary>The search data.</summary>
        private class SearchData
        {
            /// <summary>Gets or sets the search for.</summary>
            public System.Collections.Generic.List<ulong> SearchFor { get; set; }

            // we use ulong to compare against, a little slower in compare, but don't tax the brain by getting the object reference from the brain.

            /// <summary>Gets or sets the info to search for.</summary>
            public System.Collections.Generic.List<ulong> InfoToSearchFor { get; set; }

            /// <summary>Gets or sets the result.</summary>
            public System.Collections.Generic.List<Neuron> Result { get; set; }

            /// <summary>Gets or sets the to search.</summary>
            public System.Collections.Generic.IList<Link> ToSearch { get; set; }
        }

        #endregion
    }
}