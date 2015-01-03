//-----------------------------------------------------------------------
// <copyright file="Scraping.cs">
//     Copyright (c) 2008-2012 Jan Bogaerts. All rights reserved.
// </copyright> 
// <authorJan Bogaerts</author>
// <email>Jan.Bogaerts@telenet.be</email>
// <date>16/04/2012</date>
//-----------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Xml.XPath;
using System.Xml;
using System.Net;

namespace CmdShell
{
   public class Scraping
   {
      #region fields
      string fPath;
      StreamReader fDocument;
      XPathNavigator fNavigator;
      XmlNamespaceManager fNamespaceManager;

      static List<Scraping> fScraper = new List<Scraping>();                         //the entry points.

      #endregion

      #region ctor
      /// <summary>
      /// Initializes a new instance of the <see cref="Scraping"/> class.
      /// </summary>
      /// <param name="path">The path.</param>
      /// <param name="xpath">The xpath.</param>
      private Scraping(string path, bool asXml)
      {
         fPath = path;
         WebRequest iRequest = HttpWebRequest.Create(new Uri(path, UriKind.Absolute));
         WebResponse iResponse = iRequest.GetResponse();
         fDocument = new StreamReader(iResponse.GetResponseStream());
         if (asXml == true)
         {
            XPathDocument doc = new XPathDocument(fDocument);
            fNavigator = doc.CreateNavigator();
         }
         else
         {
            throw new NotSupportedException();
         }
      }
      #endregion


      #region statics

      /// <summary>
      /// Adds an XML namespace and prefix so that the xml parser/xpath evaluator can handle the ns's correctly.
      /// </summary>
      /// <param name="ID">The ID of the scraper to use.</param>
      /// <param name="prefix">The prefix.</param>
      /// <param name="uri">The URI.</param>
      public static void AddXmlNamespace(int ID, string prefix, string uri)
      {
         Scraping iScraper = null;
         lock (fScraper)
            if (fScraper.Count < ID && ID > 0)
               iScraper = fScraper[ID];
         if (iScraper != null && iScraper.fNavigator != null)
         {
            if (iScraper.fNamespaceManager == null)
               iScraper.fNamespaceManager = new XmlNamespaceManager(iScraper.fNavigator.NameTable);
            iScraper.fNamespaceManager.AddNamespace(prefix, uri);
         }
      }


      /// <summary>
      /// Closes the scraper if there is any.
      /// </summary>
      public static void CloseScraper(int ID)
      {
         lock (fScraper)
         {
            if (fScraper.Count < ID && ID > 0)
            {
               Scraping iScraper = fScraper[ID];
               if (iScraper != null && iScraper.fDocument != null)
               {
                  iScraper.fDocument.Close();
                  iScraper.fDocument = null;
                  fScraper[ID] = null;
               }
               var iFound = (from i in fScraper where i != null select i).FirstOrDefault();        //check if there are any scrapers left, if not, clear out the list.
               if (iFound == null)
                  fScraper.Clear();
            }
         }
      }

      /// <summary>
      /// Opens the scraper so that multiple scrapes can be done on the same document.
      /// </summary>
      /// <param name="path">The path.</param>
      public static int OpenScraper(string path)
      {
         Scraping iScraper = new Scraping(path, true);
         lock (fScraper)
         {
            fScraper.Add(iScraper);
            return fScraper.Count - 1;
         }
      }

      /// <summary>
      /// Opens the scraper and loads the path as a HTML file, so that it first gets converted to an xml file.
      /// </summary>
      /// <param name="path">The path.</param>
      public static int OpenScraperHTML(string path)
      {
         throw new NotImplementedException();   //android doesn't yet support html scraping.
      }

      /// <summary>
      /// Runs the specified xPath on the file found in the path. When the file is an html file, it is first properly converted
      /// to xml, otherwise, the file should be in xml format.
      /// </summary>
      /// <param name="path">The path.</param>
      /// <param name="xpath">The xpath.</param>
      /// <returns>the value(s) found in the result.</returns>
      public static List<string> ScrapeText(string path, string xpath)
      {
         Scraping iScrape = new Scraping(path, true);
         return iScrape.ScrapeTextInternal(xpath);
      }

      /// <summary>
      /// Runs the specified xPath on the file found in the path. When the file is an html file, it is first properly converted
      /// to xml, otherwise, the file should be in xml format.
      /// </summary>
      /// <param name="path">The path.</param>
      /// <param name="xpath">The xpath.</param>
      /// <returns>the value(s) found in the result.</returns>
      public static List<int> ScrapeInt(string path, string xpath)
      {
         Scraping iScrape = new Scraping(path, true);
         return iScrape.ScrapeIntInternal(xpath);
      }

      /// <summary>
      /// Runs the specified xPath on the file found in the path. When the file is an html file, it is first properly converted
      /// to xml, otherwise, the file should be in xml format.
      /// </summary>
      /// <param name="path">The path.</param>
      /// <param name="xpath">The xpath.</param>
      /// <returns>the value(s) found in the result.</returns>
      public static List<double> ScrapeDouble(string path, string xpath)
      {
         Scraping iScrape = new Scraping(path, true);
         return iScrape.ScrapeDoubleInternal(xpath);
      }

      /// <summary>
      /// Runs the specified xPath on the file found in the path. When the file is an html file, it is first properly converted
      /// to xml, otherwise, the file should be in xml format.
      /// </summary>
      /// <param name="path">The path.</param>
      /// <param name="xpath">The xpath.</param>
      /// <returns>the value(s) found in the result.</returns>
      public static List<DateTime> ScrapeDate(string path, string xpath)
      {
         Scraping iScrape = new Scraping(path, true);
         return iScrape.ScrapeDateInternal(xpath);
      }

      /// <summary>
      /// Runs the specified xPath on the file found in the path. When the file is an html file, it is first properly converted
      /// to xml, otherwise, the file should be in xml format.
      /// </summary>
      /// <param name="path">The path.</param>
      /// <param name="xpath">The xpath.</param>
      /// <returns>the value(s) found in the result.</returns>
      public static List<string> ScrapeText(int ID, string xpath)
      {
         Scraping iScraper = null;
         lock (fScraper)
            if (fScraper.Count > ID && ID >= 0)
               iScraper = fScraper[ID];
         if (iScraper != null)
            return iScraper.ScrapeTextInternal(xpath);
         throw new InvalidOperationException("There is no scraper open");

      }

      /// <summary>
      /// Runs the specified xPath on the file found in the path. When the file is an html file, it is first properly converted
      /// to xml, otherwise, the file should be in xml format.
      /// </summary>
      /// <param name="path">The path.</param>
      /// <param name="xpath">The xpath.</param>
      /// <returns>the value(s) found in the result.</returns>
      public static List<int> ScrapeInt(int ID, string xpath)
      {
         Scraping iScraper = null;
         lock (fScraper)
            if (fScraper.Count > ID && ID >= 0)
               iScraper = fScraper[ID];
         if (iScraper != null)
            return iScraper.ScrapeIntInternal(xpath);
         throw new InvalidOperationException("There is no scraper open");
      }

      /// <summary>
      /// Runs the specified xPath on the file found in the path. When the file is an html file, it is first properly converted
      /// to xml, otherwise, the file should be in xml format.
      /// </summary>
      /// <param name="path">The path.</param>
      /// <param name="xpath">The xpath.</param>
      /// <returns>the value(s) found in the result.</returns>
      public static List<double> ScrapeDouble(int ID, string xpath)
      {
         Scraping iScraper = null;
         lock (fScraper)
            if (fScraper.Count > ID && ID >= 0)
               iScraper = fScraper[ID];
         if (iScraper != null)
            return iScraper.ScrapeDoubleInternal(xpath);
         throw new InvalidOperationException("There is no scraper open");
      }

      /// <summary>
      /// Runs the specified xPath on the already open file . When the file is an html file, it is first properly converted
      /// to xml, otherwise, the file should be in xml format.
      /// </summary>
      /// <param name="path">The path.</param>
      /// <param name="xpath">The xpath.</param>
      /// <returns>the value(s) found in the result.</returns>
      public static List<DateTime> ScrapeDate(int ID, string xpath)
      {
         Scraping iScraper = null;
         lock (fScraper)
            if (fScraper.Count > ID && ID >= 0)
               iScraper = fScraper[ID];
         if (iScraper != null)
            return iScraper.ScrapeDateInternal(xpath);
         throw new InvalidOperationException("There is no scraper open");
      }

      #endregion



      /// <summary>
      /// Scrapes text values from the specified navigator.
      /// </summary>
      /// <param name="nav">The nav.</param>
      /// <param name="xpath">The xpath.</param>
      /// <returns></returns>
      private List<string> ScrapeTextInternal(string xpath)
      {
         List<string> iRes = new List<string>();
         XPathExpression iExpr = fNavigator.Compile(xpath);

         if (fNamespaceManager != null)
            iExpr.SetContext(fNamespaceManager);

         XPathNodeIterator iIterator = fNavigator.Select(iExpr);
         while (iIterator.MoveNext())
         {
            XPathNavigator nav2 = iIterator.Current.Clone();
            iRes.Add(nav2.Value);
         }
         return iRes;
      }

      /// <summary>
      /// Scrapes text values from the specified navigator.
      /// </summary>
      /// <param name="nav">The nav.</param>
      /// <param name="xpath">The xpath.</param>
      /// <returns></returns>
      private List<int> ScrapeIntInternal(string xpath)
      {
         List<int> iRes = new List<int>();
         XPathExpression iExpr = fNavigator.Compile(xpath);
         XPathNodeIterator iIterator = fNavigator.Select(iExpr);
         while (iIterator.MoveNext())
         {
            XPathNavigator nav2 = iIterator.Current.Clone();
            iRes.Add(nav2.ValueAsInt);
         }
         return iRes;
      }

      /// <summary>
      /// Scrapes text values from the specified navigator.
      /// </summary>
      /// <param name="nav">The nav.</param>
      /// <param name="xpath">The xpath.</param>
      /// <returns></returns>
      private List<double> ScrapeDoubleInternal(string xpath)
      {
         List<double> iRes = new List<double>();
         XPathExpression iExpr = fNavigator.Compile(xpath);
         XPathNodeIterator iIterator = fNavigator.Select(iExpr);
         while (iIterator.MoveNext())
         {
            XPathNavigator nav2 = iIterator.Current.Clone();
            iRes.Add(nav2.ValueAsDouble);
         }
         return iRes;
      }

      /// <summary>
      /// Scrapes text values from the specified navigator.
      /// </summary>
      /// <param name="nav">The nav.</param>
      /// <param name="xpath">The xpath.</param>
      /// <returns></returns>
      private List<DateTime> ScrapeDateInternal(string xpath)
      {
         List<DateTime> iRes = new List<DateTime>();
         XPathExpression iExpr = fNavigator.Compile(xpath);
         XPathNodeIterator iIterator = fNavigator.Select(iExpr);
         while (iIterator.MoveNext())
         {
            XPathNavigator nav2 = iIterator.Current.Clone();
            iRes.Add(nav2.ValueAsDateTime);
         }
         return iRes;
      }
   }
}