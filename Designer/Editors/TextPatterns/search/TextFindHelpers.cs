// --------------------------------------------------------------------------------------------------------------------
// <copyright file="TextFindHelpers.cs" company="">
//   
// </copyright>
// <summary>
//   The text find helpers.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace JaStDev.HAB.Designer
{
    /// <summary>The text find helpers.</summary>
    internal class TextFindHelpers
    {
        /// <summary>Creates a displayPath to all <see cref="ResponseForOutput"/> objects
        ///     that refer the argument.</summary>
        /// <param name="neuron">The neuron.</param>
        /// <param name="value">The value.</param>
        internal static void ListAllResponseForRefsForOutput(Neuron neuron, string value)
        {
            try
            {
                ResponseSearchData iData = new ResponseSearchData() { ToSearch = neuron };
                iData.ResultFolder = new Search.DisplayPathSetFolder();
                iData.ResultFolder.Title = value;
                Search.SearchResults.Default.Items.Add(iData.ResultFolder);
                foreach (TextPatternEditor i in BrainData.Current.Editors.AllTextPatternEditors())
                {
                    // we walk through every editor instead of searching the other way. this is slower, but far easier to code and maintain (don't need to build neuron paths in 2 locations (here and in OutputPattern))
                    bool iPrevIsOpen = i.IsOpen;
                    i.IsOpen = true;
                    try
                    {
                        iData.ResultSet = null; // for each new editor, 
                        iData.Editor = i;
                        foreach (PatternRule r in i.Items)
                        {
                            bool iIsLoaded = r.IsLoaded;
                            r.IsLoaded = true;
                            try
                            {
                                foreach (ResponsesForGroup g in r.ResponsesFor)
                                {
                                    foreach (ResponseForOutput o in g.ResponseFor)
                                    {
                                        ProcessResponseFor(o, iData);
                                    }
                                }
                            }
                            finally
                            {
                                r.IsLoaded = iIsLoaded;
                            }
                        }
                    }
                    finally
                    {
                        i.IsOpen = iPrevIsOpen;
                    }
                }

                WindowMain.Current.ActivateTool(ToolsList.Default.SearchResultsTool);

                    // make certain it is visible and active
                Search.SearchResults.Default.SelectedIndex = Search.SearchResults.Default.Items.Count - 1;
            }
            catch (System.Exception e)
            {
                LogService.Log.LogError("List all outputs", "Error: " + e.ToString());
                System.Windows.MessageBox.Show(
                    "Failed to find all. See the log for more details.", 
                    "Go to output", 
                    System.Windows.MessageBoxButton.OK, 
                    System.Windows.MessageBoxImage.Error);
            }
        }

        /// <summary>The process response for.</summary>
        /// <param name="o">The o.</param>
        /// <param name="data">The data.</param>
        private static void ProcessResponseFor(ResponseForOutput o, ResponseSearchData data)
        {
            if (o.Item == data.ToSearch)
            {
                if (data.ResultSet == null)
                {
                    data.ResultSet = new Search.DisplayPathSet();
                    data.ResultSet.Title = data.Editor.Name;
                    data.ResultFolder.Items.Add(data.ResultSet);
                }

                Search.DisplayPath iPath = o.GetDisplayPathFromThis();

                    // needs to be done from this thread, otherwise, the cursor might have moved on.
                iPath.Title = o.Expression;
                data.ResultSet.Items.Add(iPath);
            }
        }

        /// <summary>The process outputs.</summary>
        /// <param name="o">The o.</param>
        /// <param name="data">The data.</param>
        private static void ProcessOutputs(ResponseForOutput o, ResponseSearchData data)
        {
            if (o.Item == data.ToSearch)
            {
                if (data.ResultSet == null)
                {
                    data.ResultSet = new Search.DisplayPathSet();
                    data.ResultSet.Title = data.Editor.Name;
                    data.ResultFolder.Items.Add(data.ResultSet);
                }

                Search.DisplayPath iPath = o.GetDisplayPathFromThis();

                    // needs to be done from this thread, otherwise, the cursor might have moved on.
                iPath.Title = o.Expression;
                data.ResultSet.Items.Add(iPath);
            }
        }

        /// <summary>The list all outputs for response for ref.</summary>
        /// <param name="neuron">The neuron.</param>
        /// <param name="value">The value.</param>
        internal static void ListAllOutputsForResponseForRef(Neuron neuron, string value)
        {
            try
            {
                ResponseSearchData iData = new ResponseSearchData() { ToSearch = neuron };
                iData.ResultFolder = new Search.DisplayPathSetFolder();
                iData.ResultFolder.Title = value;
                Search.SearchResults.Default.Items.Add(iData.ResultFolder);
                foreach (TextPatternEditor i in BrainData.Current.Editors.AllTextPatternEditors())
                {
                    // we walk through every editor instead of searching the other way. this is slower, but far easier to code and maintain (don't need to build neuron paths in 2 locations (here and in OutputPattern))
                    bool iPrevIsOpen = i.IsOpen;
                    i.IsOpen = true;
                    try
                    {
                        iData.ResultSet = null; // for each new editor, 
                        iData.Editor = i;
                        foreach (PatternRule r in i.Items)
                        {
                            bool iIsLoaded = r.IsLoaded;
                            r.IsLoaded = true;
                            try
                            {
                                foreach (ResponsesForGroup g in r.ResponsesFor)
                                {
                                    foreach (ResponseForOutput o in g.ResponseFor)
                                    {
                                        ProcessOutputs(o, iData);
                                    }
                                }
                            }
                            finally
                            {
                                r.IsLoaded = iIsLoaded;
                            }
                        }
                    }
                    finally
                    {
                        i.IsOpen = iPrevIsOpen;
                    }
                }

                WindowMain.Current.ActivateTool(ToolsList.Default.SearchResultsTool);

                    // make certain it is visible and active
                Search.SearchResults.Default.SelectedIndex = Search.SearchResults.Default.Items.Count - 1;
            }
            catch (System.Exception e)
            {
                LogService.Log.LogError("List all outputs", "Error: " + e.ToString());
                System.Windows.MessageBox.Show(
                    "Failed to find all. See the log for more details.", 
                    "Go to output", 
                    System.Windows.MessageBoxButton.OK, 
                    System.Windows.MessageBoxImage.Error);
            }
        }

        /// <summary>The go to output.</summary>
        /// <param name="neuron">The neuron.</param>
        internal static void GoToOutput(Neuron neuron)
        {
            try
            {
                Search.DisplayPath iPath = null;
                foreach (TextPatternEditor i in BrainData.Current.Editors.AllTextPatternEditors())
                {
                    // we walk through every editor instead of searching the other way. this is slower, but far easier to code and maintain (don't need to build neuron paths in 2 locations (here and in OutputPattern))
                    bool iPrevIsOpen = i.IsOpen;
                    i.IsOpen = true;
                    try
                    {
                        foreach (PatternRule r in i.Items)
                        {
                            bool iIsLoaded = r.IsLoaded;
                            r.IsLoaded = true;
                            try
                            {
                                iPath = FindIn(r, neuron);
                                if (iPath != null)
                                {
                                    break;
                                }
                            }
                            finally
                            {
                                r.IsLoaded = iIsLoaded;
                            }
                        }

                        if (iPath == null)
                        {
                            iPath = FindInQ(i, neuron);
                        }

                        if (iPath != null)
                        {
                            break;
                        }
                    }
                    finally
                    {
                        i.IsOpen = iPrevIsOpen;
                    }
                }

                if (iPath != null)
                {
                    iPath.SelectPathResult();

                        // needs to be done after any rule is closed otherwise selectPathResult's assignment will be overwritten again.
                }
            }
            catch (System.Exception e)
            {
                LogService.Log.LogError("Go to output", "Error: " + e.ToString());
                System.Windows.MessageBox.Show(
                    "Failed to go to output pattern. See the log for more details.", 
                    "Go to output", 
                    System.Windows.MessageBoxButton.OK, 
                    System.Windows.MessageBoxImage.Error);
            }
        }

        /// <summary>The find in q.</summary>
        /// <param name="i">The i.</param>
        /// <param name="neuron">The neuron.</param>
        /// <returns>The <see cref="DisplayPath"/>.</returns>
        private static Search.DisplayPath FindInQ(TextPatternEditor i, Neuron neuron)
        {
            foreach (PatternRuleOutput c in i.Questions)
            {
                foreach (OutputPattern o in c.Outputs)
                {
                    if (o.Item == neuron)
                    {
                        return o.GetDisplayPathFromThis(); // needs to be done while the data is open
                    }
                }
            }

            return null;
        }

        /// <summary>The find in.</summary>
        /// <param name="r">The r.</param>
        /// <param name="neuron">The neuron.</param>
        /// <returns>The <see cref="DisplayPath"/>.</returns>
        private static Search.DisplayPath FindIn(PatternRule r, Neuron neuron)
        {
            foreach (PatternRuleOutput c in r.Conditionals)
            {
                foreach (OutputPattern o in c.Outputs)
                {
                    if (o.Item == neuron)
                    {
                        return o.GetDisplayPathFromThis();
                    }
                }
            }

            foreach (OutputPattern o in r.Outputs)
            {
                if (o.Item == neuron)
                {
                    return o.GetDisplayPathFromThis();
                }
            }

            return null;
        }

        #region inner types

        /// <summary>
        ///     contains all the data for processing the
        ///     <see cref="TextFindHelpers.ListAllResponseForRefsForOutput" />
        ///     function.
        /// </summary>
        private class ResponseSearchData
        {
            /// <summary>Gets or sets the result folder.</summary>
            public Search.DisplayPathSetFolder ResultFolder { get; set; }

            /// <summary>Gets or sets the result set.</summary>
            public Search.DisplayPathSet ResultSet { get; set; }

            /// <summary>Gets or sets the editor.</summary>
            public TextPatternEditor Editor { get; set; }

            /// <summary>Gets or sets the to search.</summary>
            public Neuron ToSearch { get; set; }
        }

        #endregion
    }
}