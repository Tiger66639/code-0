//-----------------------------------------------------------------------
// <copyright file="Contacts.cs">
//     Copyright (c) 2008-2012 Jan Bogaerts. All rights reserved.
// </copyright> 
// <authorJan Bogaerts</author>
// <email>Jan.Bogaerts@telenet.be</email>
// <date>23/02/2012</date>
//-----------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using JaStDev.HAB;

namespace os.MonoDroid
{
   /// <summary>
   ///provides access to the contacts content provider.
   /// </summary>
   public class AiciContacts
   {
      /// <summary>
      /// Retrieves a single contact value (ex: name, email,...) for the specified id.
      /// </summary>
      /// <param name="id"></param>
      /// <param name="value"></param>
      /// <returns></returns>
      public static string GetContactInfo(string id, string value)
      {
#if ANDROID
         return ContactsAndroid.GetContactInfo(id, value);
#else
         return "this is the info";
#endif
      }

      /// <summary>
      /// Tries to find the contact id in the database for the specified name
      /// </summary>
      /// <param name="name">The name.</param>
      /// <returns></returns>
      public static string GetContactId(string name)
      {
#if ANDROID
         return ContactsAndroid.GetContactId(name);
#else
         return "this is the id";
#endif
      }

      /// <summary>
      /// Gets the  phone nr of the specified contact. 
      /// See: http://developer.android.com/reference/android/provider/ContactsContract.CommonDataKinds.Phone.html
      /// for a list of supported phone types.
      /// </summary>
      /// <param name="id">The id.</param>
      /// <param name="phoneType">Type of the phone.</param>
      /// <returns>the phone nr of null of nothing was found</returns>
      public static string GetContactPhoneNr(string id, int phoneType = int.MinValue)
      {
#if ANDROID
         return ContactsAndroid.GetContactPhoneNr(id, phoneType);
#else
         return "124-123-124";
#endif
      }

      /// <summary>
      /// Determines whether the contact with the specified lookup id has a phone nr.
      /// </summary>
      /// <param name="id">The Lookup id.</param>
      /// <returns>
      ///   <c>true</c> if [has phone nr] ; otherwise, <c>false</c>.
      /// </returns>
      public static bool HasPhoneNr(string id)
      {
#if ANDROID
         return ContactsAndroid.HasPhoneNr(id);
#else
         return true;
#endif
      }

      /// <summary>
      /// Retrieves the email address of a contact.
      /// for more info on the addresstype, see:
      /// http://developer.android.com/reference/android/provider/ContactsContract.CommonDataKinds.Email.html
      /// </summary>
      /// <param name="id">the id of the contact</param>
      /// <param name="addressType">the address type to use. If ommited, the default will be used.</param>
      /// <returns></returns>
      public static string GetContactEmail(string id, int addressType = int.MinValue)
      {
#if ANDROID
         return ContactsAndroid.GetContactEmail(id, addressType);
#else
         throw new NotImplementedException();
#endif
      }

      /// <summary>
      /// Instructs the network to load all the contact info. This is done through the wordnet sensory interface
      /// </summary>
      /// <param name="names"></param>
      /// <returns></returns>
      public static void LoadContactsInfo(Processor proc = null)
      {
#if ANDROID
         ContactsAndroid.LoadContactsInfo(proc);
#else
         throw new NotImplementedException();
#endif
      }
      #if ANDROID
      public static string TestGetID()
      {
         return ContactsAndroid.TestGetID();
      }
#endif
   }
}