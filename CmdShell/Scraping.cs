// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Scraping.cs" company="">
//   
// </copyright>
// <summary>
//   The scraping.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace CmdShell
{
    using System.Linq;

    /// <summary>The scraping.</summary>
    public class Scraping
    {
        #region ctor

        /// <summary>Initializes a new instance of the <see cref="Scraping"/> class.</summary>
        /// <param name="path">The path.</param>
        /// <param name="asXml">The as Xml.</param>
        private Scraping(string path, bool asXml)
        {
            fPath = path;
            var iRequest = System.Net.WebRequest.Create(new System.Uri(path, System.UriKind.Absolute));
            var iResponse = iRequest.GetResponse();
            fDocument = new System.IO.StreamReader(iResponse.GetResponseStream());
            if (asXml)
            {
                var doc = new System.Xml.XPath.XPathDocument(fDocument);
                fNavigator = doc.CreateNavigator();
            }
            else
            {
                var iConverted = ConvertHtml(fDocument);
                fNavigator = iConverted.CreateNavigator();
            }
        }

        #endregion

        /// <summary>Converts a HTML file to xml so that it can better be parsed.</summary>
        /// <param name="reader">The reader.</param>
        /// <returns>The <see cref="XmlDocument"/>.</returns>
        private static System.Xml.XmlDocument ConvertHtml(System.IO.TextReader reader)
        {
            var sgmlReader = new Sgml.SgmlReader(); // setup SGMLReader
            sgmlReader.DocType = "HTML";
            sgmlReader.WhitespaceHandling = System.Xml.WhitespaceHandling.All;
            sgmlReader.CaseFolding = Sgml.CaseFolding.ToLower;
            sgmlReader.InputStream = reader;

            // create document
            var doc = new System.Xml.XmlDocument();
            doc.PreserveWhitespace = true;
            doc.XmlResolver = null;
            doc.Load(sgmlReader);
            return doc;
        }

        /// <summary>Scrapes text values from the specified navigator.</summary>
        /// <param name="xpath">The xpath.</param>
        /// <returns>The <see cref="List"/>.</returns>
        private System.Collections.Generic.List<string> ScrapeTextInternal(string xpath)
        {
            var iRes = new System.Collections.Generic.List<string>();
            var iExpr = fNavigator.Compile(xpath);

            if (fNamespaceManager != null)
            {
                iExpr.SetContext(fNamespaceManager);
            }

            var iIterator = fNavigator.Select(iExpr);
            while (iIterator.MoveNext())
            {
                var nav2 = iIterator.Current.Clone();
                iRes.Add(nav2.Value);
            }

            return iRes;
        }

        /// <summary>Scrapes text values from the specified navigator.</summary>
        /// <param name="xpath">The xpath.</param>
        /// <returns>The <see cref="List"/>.</returns>
        private System.Collections.Generic.List<int> ScrapeIntInternal(string xpath)
        {
            var iRes = new System.Collections.Generic.List<int>();
            var iExpr = fNavigator.Compile(xpath);
            var iIterator = fNavigator.Select(iExpr);
            while (iIterator.MoveNext())
            {
                var nav2 = iIterator.Current.Clone();
                iRes.Add(nav2.ValueAsInt);
            }

            return iRes;
        }

        /// <summary>Scrapes text values from the specified navigator.</summary>
        /// <param name="xpath">The xpath.</param>
        /// <returns>The <see cref="List"/>.</returns>
        private System.Collections.Generic.List<double> ScrapeDoubleInternal(string xpath)
        {
            var iRes = new System.Collections.Generic.List<double>();
            var iExpr = fNavigator.Compile(xpath);
            var iIterator = fNavigator.Select(iExpr);
            while (iIterator.MoveNext())
            {
                var nav2 = iIterator.Current.Clone();
                iRes.Add(nav2.ValueAsDouble);
            }

            return iRes;
        }

        /// <summary>Scrapes text values from the specified navigator.</summary>
        /// <param name="xpath">The xpath.</param>
        /// <returns>The <see cref="List"/>.</returns>
        private System.Collections.Generic.List<System.DateTime> ScrapeDateInternal(string xpath)
        {
            var iRes = new System.Collections.Generic.List<System.DateTime>();
            var iExpr = fNavigator.Compile(xpath);
            var iIterator = fNavigator.Select(iExpr);
            while (iIterator.MoveNext())
            {
                var nav2 = iIterator.Current.Clone();
                iRes.Add(nav2.ValueAsDateTime);
            }

            return iRes;
        }

        #region fields

        /// <summary>The f path.</summary>
        private string fPath;

        /// <summary>The f document.</summary>
        private System.IO.StreamReader fDocument;

        /// <summary>The f navigator.</summary>
        private readonly System.Xml.XPath.XPathNavigator fNavigator;

        /// <summary>The f namespace manager.</summary>
        private System.Xml.XmlNamespaceManager fNamespaceManager;

        /// <summary>The f scraper.</summary>
        private static readonly System.Collections.Generic.List<Scraping> fScraper =
            new System.Collections.Generic.List<Scraping>(); // the entry points.

        #endregion

        #region statics

        /// <summary>Adds an XML <see langword="namespace"/> and <paramref name="prefix"/>
        ///     so that the xml parser/xpath evaluator can handle the ns's correctly.</summary>
        /// <param name="ID">The ID of the scraper to use.</param>
        /// <param name="prefix">The prefix.</param>
        /// <param name="uri">The URI.</param>
        public static void AddXmlNamespace(int ID, string prefix, string uri)
        {
            Scraping iScraper = null;
            lock (fScraper)
                if (fScraper.Count < ID && ID > 0)
                {
                    iScraper = fScraper[ID];
                }

            if (iScraper != null && iScraper.fNavigator != null)
            {
                if (iScraper.fNamespaceManager == null)
                {
                    iScraper.fNamespaceManager = new System.Xml.XmlNamespaceManager(iScraper.fNavigator.NameTable);
                }

                iScraper.fNamespaceManager.AddNamespace(prefix, uri);
            }
        }

        /// <summary>Closes the scraper if there is any.</summary>
        /// <param name="ID">The ID.</param>
        public static void CloseScraper(int ID)
        {
            lock (fScraper)
            {
                if (fScraper.Count < ID && ID > 0)
                {
                    var iScraper = fScraper[ID];
                    if (iScraper != null && iScraper.fDocument != null)
                    {
                        iScraper.fDocument.Close();
                        iScraper.fDocument = null;
                        fScraper[ID] = null;
                    }

                    var iFound = (from i in fScraper where i != null select i).FirstOrDefault();

                        // check if there are any scrapers left, if not, clear out the list.
                    if (iFound == null)
                    {
                        fScraper.Clear();
                    }
                }
            }
        }

        /// <summary>Opens the scraper so that multiple scrapes can be done on the same
        ///     document.</summary>
        /// <param name="path">The path.</param>
        /// <returns>The <see cref="int"/>.</returns>
        public static int OpenScraper(string path)
        {
            var iScraper = new Scraping(path, true);
            lock (fScraper)
            {
                fScraper.Add(iScraper);
                return fScraper.Count - 1;
            }
        }

        /// <summary>Opens the scraper and loads the <paramref name="path"/> as a HTML
        ///     file, so that it first gets converted to an xml file.</summary>
        /// <param name="path">The path.</param>
        /// <returns>The <see cref="int"/>.</returns>
        public static int OpenScraperHTML(string path)
        {
            var iScraper = new Scraping(path, false);
            lock (fScraper)
            {
                fScraper.Add(iScraper);
                return fScraper.Count - 1;
            }
        }

        /// <summary>Runs the specified xPath on the file found in the path. When the file
        ///     is an html file, it is first properly converted to xml, otherwise, the
        ///     file should be in xml format.</summary>
        /// <param name="path">The path.</param>
        /// <param name="xpath">The xpath.</param>
        /// <returns>the value(s) found in the result.</returns>
        public static System.Collections.Generic.List<string> ScrapeText(string path, string xpath)
        {
            var iScrape = new Scraping(path, true);
            return iScrape.ScrapeTextInternal(xpath);
        }

        /// <summary>Runs the specified xPath on the file found in the path. When the file
        ///     is an html file, it is first properly converted to xml, otherwise, the
        ///     file should be in xml format.</summary>
        /// <param name="path">The path.</param>
        /// <param name="xpath">The xpath.</param>
        /// <returns>the value(s) found in the result.</returns>
        public static System.Collections.Generic.List<int> ScrapeInt(string path, string xpath)
        {
            var iScrape = new Scraping(path, true);
            return iScrape.ScrapeIntInternal(xpath);
        }

        /// <summary>Runs the specified xPath on the file found in the path. When the file
        ///     is an html file, it is first properly converted to xml, otherwise, the
        ///     file should be in xml format.</summary>
        /// <param name="path">The path.</param>
        /// <param name="xpath">The xpath.</param>
        /// <returns>the value(s) found in the result.</returns>
        public static System.Collections.Generic.List<double> ScrapeDouble(string path, string xpath)
        {
            var iScrape = new Scraping(path, true);
            return iScrape.ScrapeDoubleInternal(xpath);
        }

        /// <summary>Runs the specified xPath on the file found in the path. When the file
        ///     is an html file, it is first properly converted to xml, otherwise, the
        ///     file should be in xml format.</summary>
        /// <param name="path">The path.</param>
        /// <param name="xpath">The xpath.</param>
        /// <returns>the value(s) found in the result.</returns>
        public static System.Collections.Generic.List<System.DateTime> ScrapeDate(string path, string xpath)
        {
            var iScrape = new Scraping(path, true);
            return iScrape.ScrapeDateInternal(xpath);
        }

        /// <summary>Runs the specified xPath on the file found in the path. When the file
        ///     is an html file, it is first properly converted to xml, otherwise, the
        ///     file should be in xml format.</summary>
        /// <param name="ID">The ID.</param>
        /// <param name="xpath">The xpath.</param>
        /// <returns>the value(s) found in the result.</returns>
        public static System.Collections.Generic.List<string> ScrapeText(int ID, string xpath)
        {
            Scraping iScraper = null;
            lock (fScraper)
                if (fScraper.Count > ID && ID >= 0)
                {
                    iScraper = fScraper[ID];
                }

            if (iScraper != null)
            {
                return iScraper.ScrapeTextInternal(xpath);
            }

            throw new System.InvalidOperationException("There is no scraper open");
        }

        /// <summary>Runs the specified xPath on the file found in the path. When the file
        ///     is an html file, it is first properly converted to xml, otherwise, the
        ///     file should be in xml format.</summary>
        /// <param name="ID">The ID.</param>
        /// <param name="xpath">The xpath.</param>
        /// <returns>the value(s) found in the result.</returns>
        public static System.Collections.Generic.List<int> ScrapeInt(int ID, string xpath)
        {
            Scraping iScraper = null;
            lock (fScraper)
                if (fScraper.Count > ID && ID >= 0)
                {
                    iScraper = fScraper[ID];
                }

            if (iScraper != null)
            {
                return iScraper.ScrapeIntInternal(xpath);
            }

            throw new System.InvalidOperationException("There is no scraper open");
        }

        /// <summary>Runs the specified xPath on the file found in the path. When the file
        ///     is an html file, it is first properly converted to xml, otherwise, the
        ///     file should be in xml format.</summary>
        /// <param name="ID">The ID.</param>
        /// <param name="xpath">The xpath.</param>
        /// <returns>the value(s) found in the result.</returns>
        public static System.Collections.Generic.List<double> ScrapeDouble(int ID, string xpath)
        {
            Scraping iScraper = null;
            lock (fScraper)
                if (fScraper.Count > ID && ID >= 0)
                {
                    iScraper = fScraper[ID];
                }

            if (iScraper != null)
            {
                return iScraper.ScrapeDoubleInternal(xpath);
            }

            throw new System.InvalidOperationException("There is no scraper open");
        }

        /// <summary>Runs the specified xPath on the already open file . When the file is
        ///     an html file, it is first properly converted to xml, otherwise, the
        ///     file should be in xml format.</summary>
        /// <param name="ID">The ID.</param>
        /// <param name="xpath">The xpath.</param>
        /// <returns>the value(s) found in the result.</returns>
        public static System.Collections.Generic.List<System.DateTime> ScrapeDate(int ID, string xpath)
        {
            Scraping iScraper = null;
            lock (fScraper)
                if (fScraper.Count > ID && ID >= 0)
                {
                    iScraper = fScraper[ID];
                }

            if (iScraper != null)
            {
                return iScraper.ScrapeDateInternal(xpath);
            }

            throw new System.InvalidOperationException("There is no scraper open");
        }

        #endregion
    }
}