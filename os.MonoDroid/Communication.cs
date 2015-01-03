//-----------------------------------------------------------------------
// <copyright file="Communication.cs">
//     Copyright (c) 2008-2012 Jan Bogaerts. All rights reserved.
// </copyright> 
// <authorJan Bogaerts</author>
// <email>Jan.Bogaerts@telenet.be</email>
// <date>16/05/2012</date>
//-----------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using JaStDev.LogService;


namespace os.MonoDroid
{
   /// <summary>
   /// provides entry points to perforrm communication tasks (like sending sms).
   /// </summary>
   public class Communication
   {
      /// <summary>
      /// the callback int that identifies the event.
      /// </summary>
      public const int TWIDGIT_REQUEST_CODE = 2564; 

      /// <summary>
      /// sends an sms to the specified address.
      /// </summary>
      /// <param name="message"></param>
      /// <param name="phoneNr"></param>
      public static void SendSMS(string phoneNr, string message)
      {
#if ANDROID
         CommunicationAndroid.SendSMS(phoneNr, message);
#else
         Log.LogInfo("Communication.SendSMS", string.Format("sending sms to: {0}, message: {1}", phoneNr, message));
#endif
      }

      /// <summary>
      /// Sends an email 
      /// </summary>
      /// <param name="address"></param>
      /// <param name="content"></param>
      /// <param name="attachements"></param>
      public static void SendEmail(string[] address, string subject, string content, string[] attachements = null, string[] cc = null)
      {
#if ANDROID
         CommunicationAndroid.SendEmail(address, subject, content, attachements, cc);
#else
         Log.LogInfo("Communication.SendEmail", string.Format("sending email to: {0}, subject: {1}, body: {2}, attach:{3}", address, subject, content, attachements));
#endif
      }

      /// <summary>
      /// Sends an email.
      /// </summary>
      /// <param name="address">The address.</param>
      /// <param name="subject">The subject.</param>
      /// <param name="content">The content.</param>
      public static void SendEmail(string address, string subject, string content)
      {
#if ANDROID
         CommunicationAndroid.SendEmail(new string[] { address }, subject, content);
#else
         Log.LogInfo("Communication.SendEmail", string.Format("sending email to: {0}, subject: {1}, body: {2}", address, subject, content));
#endif
      }


      /// <summary>
      /// Starts an email app
      /// </summary>
      public static void StartEmail()
      {
#if ANDROID
         CommunicationAndroid.StartEmail();
#else
         Log.LogInfo("Communication.StartEmail", "sending email");
#endif
      }

      /// <summary>
      /// Starts the gmail app
      /// </summary>
      public static void StartGmail()
      {
#if ANDROID
         CommunicationAndroid.StartGmail();
#else
         Log.LogInfo("Communication.StartGmail", "sending email through gemail");
#endif
      }

      /// <summary>
      /// initiates a phone call to the specified number. When the phone call is done, the system returns to the Aici screen.
      /// </summary>
      /// <param name="number"></param>
      public static void MakePhoneCall(string number)
      {
#if ANDROID
         CommunicationAndroid.MakePhoneCall(number);
         Log.LogInfo("Communication.MakePhoneCall", string.Format("calling: {0}", number));
#else
         throw new NotImplementedException();
#endif
      }

      /// <summary>
      /// sends an sms to the specified address.
      /// </summary>
      /// <param name="message"></param>
      /// <param name="phoneNr"></param>
      public static void SendTweet(string message)
      {
#if ANDROID
         CommunicationAndroid.SendTweet(message);
#else
         Log.LogInfo("Communication.SendTweet", string.Format("sending tweet: {0}", message));
#endif
      }
   }
}