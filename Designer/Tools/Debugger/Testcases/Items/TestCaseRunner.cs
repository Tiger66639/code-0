// --------------------------------------------------------------------------------------------------------------------
// <copyright file="TestCaseRunner.cs" company="">
//   
// </copyright>
// <summary>
//   Provides a mechanisme to run a test case scenario.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace JaStDev.HAB.Designer.Test
{
    /// <summary>
    ///     Provides a mechanisme to run a test case scenario.
    /// </summary>
    public class TestCaseRunner : Data.ObservableObject
    {
        /// <summary>
        ///     Occurs when the testcase is finished.
        /// </summary>
        public event System.EventHandler Finished;

        /// <summary>The stop.</summary>
        internal void Stop()
        {
            fStop = true;
        }

        #region Fields

        /// <summary>The f case to run.</summary>
        private TestCase fCaseToRun;

        /// <summary>The f run on.</summary>
        private ITesteable fRunOn;

        /// <summary>The f stop.</summary>
        private bool fStop; // this var is set when the test needs to be stopped.

        /// <summary>The f outputhandle.</summary>
        private readonly System.Threading.ManualResetEvent fOutputhandle = new System.Threading.ManualResetEvent(false);

                                                           // used to wait for a result of the textsin.

        /// <summary>The f threads done handle.</summary>
        private readonly System.Threading.AutoResetEvent fThreadsDoneHandle = new System.Threading.AutoResetEvent(false);

                                                         // used to wait until all the threads are done.

        /// <summary>The f max time out.</summary>
        private System.TimeSpan fMaxTimeOut = new System.TimeSpan(0, 10, 0);

        /// <summary>The f last output.</summary>
        private string fLastOutput;

                       // gets the string that was last returned by the textsin. Used to pass the result back from textsin event handler to test routine.

        /// <summary>The f item watch.</summary>
        private readonly System.Diagnostics.Stopwatch fItemWatch = new System.Diagnostics.Stopwatch();

                                                      // the watch to time a single test, so we don't need to create it each time.

        /// <summary>The f test watch.</summary>
        private readonly System.Diagnostics.Stopwatch fTestWatch = new System.Diagnostics.Stopwatch();

                                                      // the watch to time the entire test.
        #endregion

        #region Prop

        #region CaseToRun

        /// <summary>
        ///     Gets/sets the the case that needs to be run.
        /// </summary>
        public TestCase CaseToRun
        {
            get
            {
                return fCaseToRun;
            }

            set
            {
                fCaseToRun = value;
                OnPropertyChanged("CaseToRun");
            }
        }

        #endregion

        #region RunOn

        /// <summary>
        ///     Gets/sets the Text sin on which to run the test.
        /// </summary>
        public ITesteable RunOn
        {
            get
            {
                return fRunOn;
            }

            set
            {
                fRunOn = value;
                OnPropertyChanged("RunOn");
            }
        }

        #endregion

        #region LastResult

        /// <summary>
        ///     Gets the value of the last test case result
        /// </summary>
        public bool LastResult { get; internal set; }

        #endregion

        #region MaxTimeOut

        /// <summary>
        ///     Gets/sets the timespan that determins the  amount of time to wait on a result before it is signalled as invalid.
        ///     This has an initial value of 10 minutes (should be long enough).
        /// </summary>
        public System.TimeSpan MaxTimeOut
        {
            get
            {
                return fMaxTimeOut;
            }

            set
            {
                fMaxTimeOut = value;
                OnPropertyChanged("MaxTimeOut");
            }
        }

        #endregion

        #region TestDuration

        /// <summary>
        ///     Gets the time that the entire test took.
        /// </summary>
        public System.TimeSpan TestDuration { get; internal set; }

        #endregion

        #endregion

        #region Functions

        /// <summary>
        ///     Starts running the test in an async way.
        /// </summary>
        public void Run()
        {
            if (CaseToRun == null)
            {
                throw new System.InvalidOperationException("Please select a test case to run!");
            }

            if (RunOn == null)
            {
                throw new System.InvalidOperationException(
                    "Please select a Text sensory interface to run the test case on!");
            }

            System.Action iAction = InternalRun;
            iAction.BeginInvoke(null, null);
        }

        /// <summary>The internal run.</summary>
        private void InternalRun()
        {
            try
            {
                RunOn.Textsin.TextOut += RunOn_TextOut;
                ThreadManager.Default.ActivityStopped += Default_ActivityStopped;
                try
                {
                    fTestWatch.Start();
                    LastResult = true;
                    foreach (var i in CaseToRun.Items)
                    {
                        ExecuteItem(i);
                        if (fStop)
                        {
                            break; // if a stop is requested, try to exit the test as soon as possible.
                        }
                    }
                }
                finally
                {
                    fTestWatch.Stop();
                    TestDuration = fTestWatch.Elapsed;
                    RunOn.Textsin.TextOut -= RunOn_TextOut;
                    System.Windows.Application.Current.Dispatcher.BeginInvoke(new System.Action(OnFinished));
                }
            }
            catch (System.Exception e)
            {
                System.Windows.MessageBox.Show(
                    string.Format("The test run has failed unexpectedly with the following error: {0}", e), 
                    "Test case", 
                    System.Windows.MessageBoxButton.OK, 
                    System.Windows.MessageBoxImage.Error);
            }
        }

        /// <summary>Executes a single test case item and all of it's children.</summary>
        /// <param name="toRun">To run.</param>
        private void ExecuteItem(TestCaseItem toRun)
        {
            if (fStop)
            {
                return; // if a stop is requested, try to exit the test as soon as possible.
            }

            if (toRun.IsEnabled)
            {
                var iRes = false;
                var iResult = new TestCaseResultItem();
                ProcessorManager.Current.TotalProcessorCount = 0;

                    // we reset the processor count, so we can retrieve the total nr of processors that were used.
                toRun.IsRunning = true;
                fItemWatch.Reset(); // make certain we count the time from 0 for each test.
                fItemWatch.Start();
                try
                {
                    fLastOutput = null; // we don't want to accumulate the previous results.
                    fOutputhandle.Reset(); // just before we start, make certain that the waithandle is ready.
                    RunOn.SendTextToSinAsync(toRun.InputString);

                        // send the input string async through the channel so the user can see the actions.
                    iRes =
                        System.Threading.WaitHandle.WaitAll(
                            new System.Threading.WaitHandle[] { fOutputhandle, fThreadsDoneHandle }, 
                            MaxTimeOut);

                        // we wait for both  timers, this makes certain that we don't accidentally signal the handle (when threads are done) for the next test.
                }
                finally
                {
                    toRun.IsRunning = false;
                    fItemWatch.Stop();
                }

                if (iRes)
                {
                    iResult.Result = fLastOutput;
                    if (string.IsNullOrEmpty(toRun.ValidationRegex) == false)
                    {
                        if (string.IsNullOrEmpty(fLastOutput))
                        {
                            iResult.IsPassed = false;
                        }
                        else
                        {
                            var iRegex = new System.Text.RegularExpressions.Regex(toRun.ValidationRegex);
                            iResult.IsPassed = iRegex.IsMatch(fLastOutput); // check if the result passes the regex.
                        }
                    }
                    else
                    {
                        iResult.IsPassed = true;
                    }
                }
                else
                {
                    ProcessorManager.Current.StopAndUnDeadLock();

                        // if there was a timeout, it could be that there are still processorrs running, kill them before we do any other test.
                    iResult.IsPassed = false;
                    LastResult = false;
                }

                iResult.Duration = fItemWatch.Elapsed;
                iResult.TotalThreads = ProcessorManager.Current.TotalProcessorCount;
                toRun.AddResult(iResult);
                if (iResult.IsPassed == true)
                {
                    // if the test failed, don't do the sub items.
                    System.Windows.Application.Current.Dispatcher.BeginInvoke(
                        new System.Action<bool>(toRun.SetIsExpanded), 
                        true); // so that the result of the children are visible. Async, cause it update the UI
                    foreach (var i in toRun.Items)
                    {
                        // process all the sub items after the entire item has been validated.
                        ExecuteItem(i);
                    }
                }
            }
        }

        /// <summary>
        ///     Called when the run is finished.
        /// </summary>
        protected void OnFinished()
        {
            if (Finished != null)
            {
                Finished(this, System.EventArgs.Empty);
            }
        }

        /// <summary>Handles the TextOut event of the RunOn TextSin.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="JaStDev.HAB.OutputEventArgs&lt;System.String&gt;"/> instance containing the event data.</param>
        private void RunOn_TextOut(object sender, OutputEventArgs<string> e)
        {
            if (string.IsNullOrEmpty(fLastOutput))
            {
                fLastOutput = e.Value;
            }
            else
            {
                fLastOutput = fLastOutput + e.Value;
            }

            fOutputhandle.Set();
        }

        /// <summary>Handles the ActivityStopped event of the Default control. When there is no output, we can use this to see when
        ///     things are done.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        private void Default_ActivityStopped(object sender, System.EventArgs e)
        {
            fOutputhandle.Set();

                // we set both handles. This makes certain that, if there was no output, we don't get stuck. + we don't advance untill all threads are done to run the next test.
            fThreadsDoneHandle.Set();
        }

        #endregion
    }
}