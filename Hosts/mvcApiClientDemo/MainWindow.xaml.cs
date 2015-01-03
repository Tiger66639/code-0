//-----------------------------------------------------------------------
// <copyright file="MainWindow.xaml.cs">
//     Copyright (c) 2008-2012 Jan Bogaerts. All rights reserved.
// </copyright> 
// <authorJan Bogaerts</author>
// <email>Jan.Bogaerts@telenet.be</email>
// <date>16/12/2011</date>
//-----------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Net;
using System.IO;

namespace mvcApiClientDemo
{
   /// <summary>
   /// Interaction logic for MainWindow.xaml
   /// </summary>
   public partial class MainWindow : Window
   {
      CookieContainer iCookies = new CookieContainer();

      public MainWindow()
      {
         InitializeComponent();
         Connect();
      }

      /// <summary>
      /// sends a new message to the server.
      /// </summary>
      /// <param name="sender"></param>
      /// <param name="e"></param>
      private void Button_Click(object sender, RoutedEventArgs e)
      {
         HttpWebRequest iRequest = (HttpWebRequest)WebRequest.Create("http://jan.bragisoft.com/api/send");     //create a webrequest. 'send' is the default synchronous way for sending data. It will block untill a result was found.
         iRequest.Method = "POST";                                                                             //need to do a post when sending out a message.                                                  
         iRequest.ContentType = "application/x-www-form-urlencoded";                                           //this is important to include, otherwise the parameters are incorrectly formmatted.
         iRequest.CookieContainer = iCookies;                                                                  //pass the cookie to the new request.
         string iParams = string.Format("value={0}", TxtInput.Text);                                           //build the params for the http request
         byte[] iBytes = Encoding.ASCII.GetBytes(iParams);                                                     //need to copy these params to the request as a byte array.
         iRequest.ContentLength = iBytes.Length;
         using (Stream iStream = iRequest.GetRequestStream())
            iStream.Write(iBytes, 0, iBytes.Length);
         HttpWebResponse WebResp = (HttpWebResponse)iRequest.GetResponse();
         StreamReader iAnswer = new StreamReader(WebResp.GetResponseStream());
         TxtOutput.Text = iAnswer.ReadToEnd();
      }


      /// <summary>
      /// checks if the connection is open, and if not, opens it so that you have a cookie.
      /// </summary>
      private void Connect()
      {
         HttpWebRequest iRequest = (HttpWebRequest)WebRequest.Create("http://jan.bragisoft.com/api/Connect");//the first connection can be done with connect, but you could also have used a 'send' for this.
         iRequest.Method = "GET";                                                                     //connecting can be done with a simple get, no post required, even if the 'send' is used.
         iRequest.ContentType = "application/x-www-form-urlencoded";
         iRequest.CookieContainer = iCookies;                                                         //assign the empty cookie, which will contain the cookie data after the GetResponse() call.
         HttpWebResponse WebResp = (HttpWebResponse)iRequest.GetResponse();
         StreamReader iAnswer = new StreamReader(WebResp.GetResponseStream());                        //get the response from the server.
         TxtOutput.Text = iAnswer.ReadToEnd();
      }

      /// <summary>
      /// for closing the connection, strictly speaking, not required, but it's cleaner.
      /// </summary>
      /// <param name="sender"></param>
      /// <param name="e"></param>
      private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
      {
         HttpWebRequest iRequest = (HttpWebRequest)WebRequest.Create("http://jan.bragisoft.com/api/disconnect");  //always need to use disconnect if we want to log off. A session will automatically log off after 15 minutes.
         iRequest.Method = "POST";                                                                    //disconnect needs to be a post, otherwise it wont work.
         iRequest.ContentLength = 0;                                                                  //it's a post, so we need to provide a length, otherwise .net complains.
         iRequest.CookieContainer = iCookies;
         HttpWebResponse WebResp = (HttpWebResponse)iRequest.GetResponse();                           //send the request, don't really care about a response.
      }
   }
}