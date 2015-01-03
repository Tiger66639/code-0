// --------------------------------------------------------------------------------------------------------------------
// <copyright file="CsvWriter.cs" company="">
//   
// </copyright>
// <summary>
//   A <see cref="System.IO.StreamWriter" /> that provides a way to write a list of
//   neurons as a CSV line. This can be used to build up csv files or memory
//   streams.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace JaStDev.HAB.Designer.CustomConduitSupport
{
    /// <summary>
    ///     A <see cref="System.IO.StreamWriter" /> that provides a way to write a list of
    ///     neurons as a CSV line. This can be used to build up csv files or memory
    ///     streams.
    /// </summary>
    public class CsvWriter : System.IO.StreamWriter
    {
        /// <summary>Initializes a new instance of the <see cref="CsvWriter"/> class.</summary>
        /// <param name="stream">The stream.</param>
        /// <param name="separator">The separator.</param>
        public CsvWriter(System.IO.Stream stream, char separator = ',')
            : base(stream)
        {
            Separator = separator;
        }

        /// <summary>Initializes a new instance of the <see cref="CsvWriter"/> class.</summary>
        /// <param name="filename">The filename.</param>
        /// <param name="separator">The separator.</param>
        public CsvWriter(string filename, char separator = ',')
            : base(filename)
        {
            Separator = separator;
        }

        /// <summary>
        ///     gets the character used to seperate the fields.
        /// </summary>
        public char Separator { get; set; }

        /// <summary>Writes a single row to a CSV file.</summary>
        /// <param name="values">The row to be written</param>
        public void WriteLine(System.Collections.Generic.IList<Neuron> values)
        {
            var iRes = new System.Text.StringBuilder();
            var firstColumn = true;
            foreach (var iVal in values)
            {
                string iText;
                if (iVal != null && iVal.ID != (ulong)PredefinedNeurons.Empty)
                {
                    // empty also needs to be rendered as empty string.
                    iText = BrainHelper.GetTextFrom(iVal);
                }
                else
                {
                    iText = string.Empty;
                }

                if (!firstColumn)
                {
                    // Add separator if this isn't the first value
                    iRes.Append(Separator);
                }

                if (iText.IndexOfAny(new[] { '"', ',' }) != -1)
                {
                    // Implement special handling for values that contain comma or quote Enclose in quotes and double up any double quotes
                    iRes.AppendFormat("\"{0}\"", iText.Replace("\"", "\"\""));
                }
                else
                {
                    iRes.Append(iText);
                }

                firstColumn = false;
            }

            WriteLine(iRes.ToString());
        }
    }
}