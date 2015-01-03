// --------------------------------------------------------------------------------------------------------------------
// <copyright file="CmdShell.cs" company="">
//   
// </copyright>
// <summary>
//   Provides some <see langword="public" /> <see langword="static" /> functions
//   to work with the command shell.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace CmdShell
{
    /// <summary>
    ///     Provides some <see langword="public" /> <see langword="static" /> functions
    ///     to work with the command shell.
    /// </summary>
    public class CmdShell
    {
        /// <summary>The f running apps.</summary>
        private static readonly System.Collections.Generic.Dictionary<string, System.Diagnostics.Process> fRunningApps =
            new System.Collections.Generic.Dictionary<string, System.Diagnostics.Process>();

        /// <summary>Starts a process.</summary>
        /// <param name="fileName">Name of the file.</param>
        public static void OpenDocument(string fileName)
        {
            var iExplorer = new System.Diagnostics.Process();
            iExplorer.StartInfo.FileName = fileName;
            iExplorer.Exited += process_Exited;
            iExplorer.Start();
        }

        /// <summary>Starts a process.</summary>
        /// <param name="processName">The process Name.</param>
        /// <param name="fileName">Name of the file.</param>
        public static void StartProcess(string processName, string fileName)
        {
            var iExplorer = new System.Diagnostics.Process();
            iExplorer.StartInfo.Arguments = fileName;
            iExplorer.StartInfo.FileName = processName;
            iExplorer.Exited += process_Exited;
            iExplorer.Start();
        }

        /// <summary>Tries to close a previously started process.</summary>
        /// <param name="fileName">Name of the file.</param>
        public static void CloseProcess(string fileName)
        {
            System.Diagnostics.Process iFound;
            if (fRunningApps.TryGetValue(fileName, out iFound))
            {
                iFound.CloseMainWindow();
            }
        }

        /// <summary>We need to track when an application is finished, so we remove the
        ///     reference.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        private static void process_Exited(object sender, System.EventArgs e)
        {
            var iSender = sender as System.Diagnostics.Process;
            fRunningApps.Remove(iSender.StartInfo.FileName);
            iSender.Exited -= process_Exited;
        }

        /// <summary>Starts a process and waits untill it is done.</summary>
        /// <param name="processName">Name of the process.</param>
        /// <param name="fileName">Name of the file.</param>
        public static void StartAndWaitForProcess(string processName, string fileName)
        {
            var iExplorer = new System.Diagnostics.Process();
            iExplorer.StartInfo.Arguments = fileName;
            iExplorer.StartInfo.FileName = processName;
            iExplorer.Start();
            iExplorer.WaitForExit(10000);
        }
    }
}