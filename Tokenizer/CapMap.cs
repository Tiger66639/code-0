// --------------------------------------------------------------------------------------------------------------------
// <copyright file="CapMap.cs" company="">
//   
// </copyright>
// <summary>
//   used to keep track of the capitalization of a word. This is a helper
//   class that the tokenizer uses to keep track of all the capital letters in
//   a word, so we can rebuild this in the output (the network looses this
//   info during mapping).
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace JaStDev.HAB.Parsers
{
    /// <summary>
    ///     used to keep track of the capitalization of a word. This is a helper
    ///     class that the tokenizer uses to keep track of all the capital letters in
    ///     a word, so we can rebuild this in the output (the network looses this
    ///     info during mapping).
    /// </summary>
    public class CapMap
    {
        /// <summary>The f values.</summary>
        private readonly System.Collections.Generic.List<bool> fValues = new System.Collections.Generic.List<bool>();

        /// <summary>
        ///     Gets a value indicating whether the first letter is upper case while
        ///     the rest wasn't.
        /// </summary>
        /// <value>
        ///     <c>true</c> if this instance is first upper; otherwise, <c>false</c> .
        /// </value>
        public bool IsFirstUpper
        {
            get
            {
                if (fValues.Count > 0)
                {
                    if (fValues[0])
                    {
                        for (var i = 1; i < fValues.Count; i++)
                        {
                            if (fValues[i])
                            {
                                return false;
                            }
                        }

                        return true;
                    }
                }

                return false;
            }
        }

        /// <summary>
        ///     Gets a value indicating whether all the letters were in uppercase.
        /// </summary>
        /// <value>
        ///     <c>true</c> if this instance is all upper; otherwise, <c>false</c> .
        /// </value>
        public bool IsAllUpper
        {
            get
            {
                for (var i = 0; i < fValues.Count; i++)
                {
                    if (fValues[i] == false)
                    {
                        return false;
                    }
                }

                return true;
            }
        }

        /// <summary>
        ///     Gets a value indicating whether all the letters were in uppercase.
        /// </summary>
        /// <value>
        ///     <c>true</c> if this instance is all upper; otherwise, <c>false</c> .
        /// </value>
        public bool IsNoUpper
        {
            get
            {
                for (var i = 0; i < fValues.Count; i++)
                {
                    if (fValues[i])
                    {
                        return false;
                    }
                }

                return true;
            }
        }

        /// <summary>stores wether the letter at the specified <paramref name="index"/> is
        ///     capitalized or not. When there is a gap between the previous call and
        ///     the current (or there was no previous call), the empty slots are
        ///     designated as non capitals.</summary>
        /// <param name="value">if set to <c>true</c> [value].</param>
        /// <param name="index">The index.</param>
        public void AddCap(bool value, int index)
        {
            while (index > fValues.Count)
            {
                fValues.Add(false);
            }

            fValues.Add(value);
        }

        /// <summary>Gets the indexes at which an upper case should be positioned, based on
        ///     the current map.</summary>
        /// <returns>The <see cref="IEnumerable"/>.</returns>
        public System.Collections.Generic.IEnumerable<int> GetUpperCasePoints()
        {
            var iCount = 0;
            foreach (var i in fValues)
            {
                if (i)
                {
                    yield return iCount;
                }

                iCount++;
            }
        }
    }
}