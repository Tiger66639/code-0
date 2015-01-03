// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ProjectStreamingOperation.cs" company="">
//   
// </copyright>
// <summary>
//   base class for all <see cref="ProjectOperation" /> s that do something
//   with the streaming of project data. Provides some default functions and
//   types.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace JaStDev.HAB.Designer
{
    /// <summary>
    ///     base class for all <see cref="ProjectOperation" /> s that do something
    ///     with the streaming of project data. Provides some default functions and
    ///     types.
    /// </summary>
    public class ProjectStreamingOperation : ProjectOperation
    {
        /// <summary>The f files.</summary>
        protected string[] fFiles; // this list of files that need to be imported.

        /// <summary>The f problems.</summary>
        protected bool fProblems; // flag to keep track if there were problems.

        /// <summary>The f operation name.</summary>
        private readonly string fOperationName;

        /// <summary>Initializes a new instance of the <see cref="ProjectStreamingOperation"/> class. Initializes a new instance of the<see cref="ProjectStreamingOperation"/> class.</summary>
        /// <param name="operationName">Name of the operation.</param>
        public ProjectStreamingOperation(string operationName)
        {
            fOperationName = operationName;
        }

        /// <summary>reparses items that had parse errors.</summary>
        /// <param name="parseErrors"></param>
        /// <param name="stillToResolve"></param>
        protected void ResolveProblems(System.Collections.Generic.List<ParseErrors> parseErrors, System.Collections.Generic.List<ToResolve> stillToResolve)
        {
            foreach (var iFile in parseErrors)
            {
                try
                {
                    foreach (var iError in iFile.Errors)
                    {
                        // write out all the parse errors that occured, at the end, when everything else got imported corretly.
                        iError.Parse();

                            // first try a repars, could be that there was an error because of the missing topics.
                        if (iError.HasError)
                        {
                            LogService.Log.LogError(
                                fOperationName, 
                                string.Format("topic: {0}, error: {1}", iFile.FileName, iError.ParseError));
                        }
                    }
                }
                catch (System.Exception e)
                {
                    LogService.Log.LogError(
                        fOperationName, 
                        string.Format(
                            "An error occured while trying to imort the topic '{0}', with the error: {1}", 
                            iFile.FileName, 
                            e.Message));
                    fProblems = true;
                }
            }

            try
            {
                TopicXmlStreamer.ResolveForwardRefs(stillToResolve);
            }
            catch (System.Exception e)
            {
                LogService.Log.LogError(fOperationName, e.Message);
                fProblems = true;
            }
        }

        /// <summary>calculates the size of all the files, for the process tracker.</summary>
        /// <param name="tracker">The tracker.</param>
        protected void LoadFileSizes(Search.ProcessTrackerItem tracker)
        {
            tracker.TotalCount = 0;
            foreach (var iFile in fFiles)
            {
                var iInfo = new System.IO.FileInfo(iFile);
                tracker.TotalCount += iInfo.Length;
            }
        }

        #region internal types

        /// <summary>
        ///     So we can store the errors, handle them after all topics are done +
        ///     still know which file had the problem.
        /// </summary>
        protected class ParseErrors
        {
            /// <summary>The errors.</summary>
            public System.Collections.Generic.List<ParsableTextPatternBase> Errors =
                new System.Collections.Generic.List<ParsableTextPatternBase>();

            /// <summary>The file name.</summary>
            public string FileName;
        }

        /// <summary>The to resolve.</summary>
        public class ToResolve
        {
            /// <summary>Initializes a new instance of the <see cref="ToResolve"/> class.</summary>
            public ToResolve()
            {
                Questions = false;
            }

            /// <summary>Gets or sets the topic.</summary>
            public string Topic { get; set; }

            /// <summary>Gets or sets the rule.</summary>
            public string Rule { get; set; }

            /// <summary>Gets or sets the condition.</summary>
            public string Condition { get; set; }

            /// <summary>Gets or sets the output.</summary>
            public string Output { get; set; }

            /// <summary>Gets or sets the global.</summary>
            public string global { get; set; }

            /// <summary>Gets or sets the response for.</summary>
            public string ResponseFor { get; set; }

            /// <summary>
            ///     use the questions list of the topic or not.
            /// </summary>
            /// <value>
            ///     <c>true</c> if questions; otherwise, <c>false</c> .
            /// </value>
            public bool Questions { get; set; }

            /// <summary>
            ///     Gets or sets the ouput pattern to add the reference to. this is
            ///     depcreated: in the old system, reponesFor were stored underneath
            ///     output statments. now they are the top items of conditinals
            ///     (GroupToAddRefTo)
            /// </summary>
            /// <value>
            ///     The add <see langword="ref" /> to.
            /// </value>
            public OutputPattern AddRefTo { get; set; }

            /// <summary>
            ///     in the new system, this is where the resolved items should be
            ///     added.
            /// </summary>
            public ResponsesForGroup GroupToAddRefTo { get; set; }

            /// <summary>
            ///     the topic/rule that leads to AddrefTo, so we can print proper error
            ///     texts.
            /// </summary>
            /// <value>
            ///     The add <see langword="ref" /> to path.
            /// </value>
            public string AddRefToPath { get; set; }

            /// <summary>
            ///     Returns a <see cref="string" /> that represents this instance.
            /// </summary>
            /// <returns>
            ///     A <see cref="string" /> that represents this instance.
            /// </returns>
            public override string ToString()
            {
                if (string.IsNullOrEmpty(Topic) == false)
                {
                    if (Questions == false)
                    {
                        if (string.IsNullOrEmpty(Condition) == false)
                        {
                            if (string.IsNullOrEmpty(ResponseFor) == false)
                            {
                                return string.Format("{0}.{1}.{2}.{3}.{4}", Topic, Rule, ResponseFor, Condition, Output);
                            }

                            return string.Format("{0}.{1}.{2}.{3}", Topic, Rule, Condition, Output);
                        }

                        if (string.IsNullOrEmpty(ResponseFor) == false)
                        {
                            return string.Format("{0}.{1}.{2}.{3}", Topic, Rule, ResponseFor, Output);
                        }

                        return string.Format("{0}.{1}.{2}", Topic, Rule, Output);
                    }

                    if (string.IsNullOrEmpty(Condition) == false)
                    {
                        return string.Format("{0}.Questions.{1}.{2}", Topic, Condition, Output);
                    }

                    return string.Format("{0}.Questions.{1}", Topic, Output);
                }

                if (string.IsNullOrEmpty(Condition) == false)
                {
                    return string.Format("{0}.{1}.{2}", global, Condition, Output);
                }

                return string.Format("{0}.{1}", global, Output);
            }
        }

        #endregion
    }
}