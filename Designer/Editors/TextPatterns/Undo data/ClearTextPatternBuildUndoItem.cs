// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ClearTextPatternBuildUndoItem.cs" company="">
//   
// </copyright>
// <summary>
//   Undoes the clearing of a textpattern compilation, so it will recompile
//   the text pattern.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace JaStDev.HAB.Designer
{
    /// <summary>
    ///     Undoes the clearing of a textpattern compilation, so it will recompile
    ///     the text pattern.
    /// </summary>
    public class ClearTextPatternBuildUndoItem : TextPatternBaseUndoItem
    {
        /// <summary>Performs all the actions stored in the undo item, thereby undoing the
        ///     action.</summary>
        /// <param name="caller">The undo managaer that is calling this method.</param>
        public override void Execute(UndoSystem.UndoStore caller)
        {
            var iUndo = new BuildTextPatternUndoItem();
            iUndo.Pattern = Pattern;
            WindowMain.UndoStore.AddCustomUndoItem(iUndo);

            try
            {
                Neuron iRule = Pattern.FindFirstClusteredBy((ulong)PredefinedNeurons.PatternRule);
                if (iRule == null)
                {
                    Neuron iFilters = Pattern.FindFirstClusteredBy((ulong)PredefinedNeurons.TopicFilter);
                    if (iFilters == null)
                    {
                        TryBuildResponseFor();
                    }
                    else
                    {
                        TryBuildTopicFilfter(iFilters);
                    }
                }
                else
                {
                    TryBuildInput(iRule);
                }
            }
            catch
            {
                // don't do anyithing with the exception, it's a parse error, so the object isn't parsed again, so it will get a reparse when focus is lost
                // and then display the error (without generating extra undo data).
            }
        }

        /// <summary>The try build input.</summary>
        /// <param name="iRule">The i rule.</param>
        private void TryBuildInput(Neuron iRule)
        {
            var iRuleName = BrainHelper.GetTextFrom(iRule.FindFirstOut((ulong)PredefinedNeurons.NameOfMember));

            Neuron iTopic = iRule.FindFirstClusteredBy((ulong)PredefinedNeurons.TextPatternTopic);
            if (iTopic == null)
            {
                LogService.Log.LogError(
                    "Undo/redo", 
                    string.Format("Can't find topic of pattern: {0}.{1} ", iRuleName, Pattern.Text));
            }
            else
            {
                var iTopicName = BrainHelper.GetTextFrom(iTopic.FindFirstOut((ulong)PredefinedNeurons.NameOfMember));

                if (iTopic.FindFirstOut((ulong)PredefinedNeurons.Local) == null)
                {
                    // check if the topic is local, if so, it needs to be parsed local to the topic, otherwise it will be local to nothing.
                    iTopic = null;
                }

                var iParser = new Parsers.InputParser(Pattern, iTopicName, iTopic);
                iParser.Parse();
            }
        }

        /// <summary>The try build topic filfter.</summary>
        /// <param name="iFilters">The i filters.</param>
        private void TryBuildTopicFilfter(Neuron iFilters)
        {
            var iTopic = iFilters.FindFirstIn((ulong)PredefinedNeurons.TopicFilter);
            if (iTopic != null)
            {
                var iTopicName = BrainHelper.GetTextFrom(iTopic.FindFirstOut((ulong)PredefinedNeurons.NameOfMember));

                var iLocalTo = Brain.Current[(ulong)PredefinedNeurons.TextPatternTopic];
                var iParser = new Parsers.InputParser(Pattern, iTopicName, iLocalTo);
                iParser.Parse();
            }
        }

        /// <summary>
        ///     checks if the item is a 'responsefor' value and if so, builds it like
        ///     that.
        /// </summary>
        private void TryBuildResponseFor()
        {
            try
            {
                Neuron iList = Pattern.FindFirstClusteredBy((ulong)PredefinedNeurons.ResponseForOutputs);
                if (iList != null)
                {
                    var iCond = iList.FindFirstIn((ulong)PredefinedNeurons.ResponseForOutputs);
                    if (iCond != null)
                    {
                        Neuron iLocalTo = iCond.FindFirstClusteredBy((ulong)PredefinedNeurons.ResponseForOutputs);
                        if (iLocalTo != null)
                        {
                            var iUndo = new BuildTextPatternUndoItem();
                            iUndo.Pattern = Pattern;
                            WindowMain.UndoStore.AddCustomUndoItem(iUndo);

                            var iRule = iLocalTo.FindFirstIn((ulong)PredefinedNeurons.ResponseForOutputs);
                            if (iRule != null)
                            {
                                var iRuleName =
                                    BrainHelper.GetTextFrom(iRule.FindFirstOut((ulong)PredefinedNeurons.NameOfMember));

                                Neuron iTopic = iRule.FindFirstClusteredBy((ulong)PredefinedNeurons.TextPatternTopic);
                                if (iTopic == null)
                                {
                                    LogService.Log.LogError(
                                        "Undo/redo", 
                                        string.Format("Can't find topic of pattern: {0}.{1} ", iRuleName, Pattern.Text));
                                }
                                else
                                {
                                    var iTopicName =
                                        BrainHelper.GetTextFrom(
                                            iTopic.FindFirstOut((ulong)PredefinedNeurons.NameOfMember));

                                    var iParser = new Parsers.InputParser(Pattern, iTopicName, iLocalTo);
                                    iParser.Parse();
                                }
                            }

                            return;
                        }
                    }
                }
            }
            catch
            {
                // don't do anyithing with the exception, it's a parse error, so the object isn't parsed again, so it will get a reparse when focus is lost
                // and then display the error (without generating extra undo data).
            }

            LogService.Log.LogError("Undo/redo", "Can't find rule of pattern: " + Pattern.Text);
        }
    }
}