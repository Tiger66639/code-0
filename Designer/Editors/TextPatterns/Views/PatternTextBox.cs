// --------------------------------------------------------------------------------------------------------------------
// <copyright file="PatternTextBox.cs" company="">
//   
// </copyright>
// <summary>
//   A custom textbox that automatically registers the correct dictionary
//   files for the application.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace JaStDev.HAB.Designer.WPF.Controls
{
    /// <summary>
    ///     A custom textbox that automatically registers the correct dictionary
    ///     files for the application.
    /// </summary>
    public class PatternTextBox : System.Windows.Controls.TextBox
    {
        /// <summary>Initializes a new instance of the <see cref="PatternTextBox"/> class. 
        ///     Initializes a new instance of the <see cref="PatternTextBox"/>
        ///     class.</summary>
        public PatternTextBox()
        {
            AddDictionaries();
        }

        /// <summary>The add dictionaries.</summary>
        private void AddDictionaries()
        {
            var iDicts = System.Windows.Controls.SpellCheck.GetCustomDictionaries(this);
            var iPath =
                new System.Uri(
                    System.IO.Path.Combine(
                        Properties.Settings.Default.CustomSpellingDictsPath, 
                        Properties.Resources.CustomSpellingDict));
            iDicts.Add(iPath);
            iPath =
                new System.Uri(
                    System.IO.Path.Combine(
                        Properties.Settings.Default.CustomSpellingDictsPath, 
                        Properties.Resources.IgnoreAllDict));
            iDicts.Add(iPath);
        }
    }
}