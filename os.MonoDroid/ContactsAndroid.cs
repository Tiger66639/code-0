//-----------------------------------------------------------------------
// <copyright file="ContactsAndroid.cs">
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

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using JaStDev.HAB;
using Android.Database;
using Android.Provider;

namespace os.MonoDroid
{
   class ContactsAndroid
   {
      /// <summary>
      /// Retrieves a single contact value (ex: name, email,...) for the specified id.
      /// </summary>
      /// <param name="id"></param>
      /// <param name="value"></param>
      /// <returns></returns>
      public static string GetContactInfo(string id, string value)
      {
         Android.Net.Uri iUri = Android.Net.Uri.WithAppendedPath(ContactsContract.Contacts.ContentLookupUri, id);
         ICursor iCursor = ReflectionSin.Context.ContentResolver.Query(iUri, new String[] { value }, null, null, null);
         try
         {
            if (iCursor.Count > 0)
            {
               iCursor.MoveToFirst();
               return iCursor.GetString(0);
            }
            else
               return null;
         }
         finally
         {
            iCursor.Close();
         }
      }


      /// <summary>
      /// Gets the  phone nr of the specified contact. 
      /// See: http://developer.android.com/reference/android/provider/ContactsContract.CommonDataKinds.Phone.html
      /// for a list of supported phone types.
      /// When the phonetype is not supplied, the primary number is used, if there isn't a primary number, the first in the list is returned.
      /// </summary>
      /// <param name="id">The id.</param>
      /// <param name="phoneType">Type of the phone, if ommited, the default will be used.</param>
      /// <returns>the phone nr of null of nothing was found</returns>
      public static string GetContactPhoneNr(string id, int phoneType = int.MinValue)
      {
         int iId = GetIDFromLookup(id);
         string[] iCols;
         string iQuery;
         string[] iArgs;
         if (phoneType == int.MinValue)
         {
            iCols = new string[] { ContactsContract.CommonDataKinds.Phone.InterfaceConsts.ContactId, ContactsContract.CommonDataKinds.Phone.Number, ContactsContract.CommonDataKinds.Phone.InterfaceConsts.Type, ContactsContract.CommonDataKinds.Phone.InterfaceConsts.IsSuperPrimary };
            iQuery = string.Format("{0} = ? and {1} != 0", ContactsContract.CommonDataKinds.Phone.InterfaceConsts.ContactId, ContactsContract.CommonDataKinds.Phone.InterfaceConsts.IsSuperPrimary);
            iArgs = new String[] { iId.ToString() };                          
         }
         else
         {
            iCols = new string[] { ContactsContract.CommonDataKinds.Phone.InterfaceConsts.ContactId, ContactsContract.CommonDataKinds.Phone.Number, ContactsContract.CommonDataKinds.Phone.InterfaceConsts.Type };
            iQuery = string.Format("{0} = ? and {1} = ?", ContactsContract.CommonDataKinds.Phone.InterfaceConsts.ContactId, ContactsContract.CommonDataKinds.Phone.InterfaceConsts.Type);
            iArgs = new String[] { iId.ToString(), phoneType.ToString() };
         }

         ICursor iCur = ReflectionSin.Context.ContentResolver.Query(ContactsContract.CommonDataKinds.Phone.ContentUri, iCols, iQuery, iArgs, null);
         try
         {
            
            int iNrIndex = iCur.GetColumnIndex(ContactsContract.CommonDataKinds.Phone.Number);
            if (iCur.MoveToNext())
               return iCur.GetString(iNrIndex);
            else if (phoneType == int.MinValue)
            {
               iCur.Close();
               iQuery = string.Format("{0} = ?", ContactsContract.CommonDataKinds.Phone.InterfaceConsts.ContactId);
               iCur = ReflectionSin.Context.ContentResolver.Query(ContactsContract.CommonDataKinds.Phone.ContentUri, iCols, iQuery, iArgs, null);
               if (iCur.MoveToNext())
                  return iCur.GetString(iNrIndex);
            }
            return null;
         }
         finally
         {
            iCur.Close();
         }
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
         //String iId = null;

         ContentResolver iResolver = ReflectionSin.Context.ContentResolver;
         string[] iCols = new string[] { ContactsContract.Contacts.InterfaceConsts.HasPhoneNumber, ContactsContract.Contacts.InterfaceConsts.LookupKey }; //we get deleted as well to make certain that we don't get wrong ids.
         string iQuery = string.Format("{0} = ?", ContactsContract.Contacts.InterfaceConsts.LookupKey);
         string[] iArgs = new String[] { id }; 

         ICursor iCur = iResolver.Query(ContactsContract.Contacts.ContentUri, iCols, iQuery, iArgs, null);
         if (iCur.Count > 0)
         {
            try
            {
               if (iCur.MoveToNext())
                  return iCur.GetInt(iCur.GetColumnIndex(ContactsContract.Contacts.InterfaceConsts.HasPhoneNumber)) >= 1; //1 if there is a phone nr, 0 if there is none.
            }
            finally
            {
               iCur.Close();
            }
         }
         return false;
      }

      /// <summary>
      /// converts a loolup_id to a record id.
      /// </summary>
      /// <param name="id"></param>
      /// <returns></returns>
      private static int GetIDFromLookup(string id)
      {
         int iId = -1;
         ContentResolver iResolver = ReflectionSin.Context.ContentResolver;
         string[] iCols = new string[] { ContactsContract.Contacts.InterfaceConsts.Id, ContactsContract.Contacts.InterfaceConsts.LookupKey }; //we get deleted as well to make certain that we don't get wrong ids.
         string iQuery = string.Format("{0} = ?", ContactsContract.Contacts.InterfaceConsts.LookupKey);
         string[] iArgs = new string[] { id };
         ICursor iCur = iResolver.Query(ContactsContract.Contacts.ContentUri, iCols, iQuery, iArgs, null);
         if (iCur.Count > 0)
         {
            if (iCur.MoveToNext())
               iId = iCur.GetInt(iCur.GetColumnIndex(ContactsContract.Contacts.InterfaceConsts.Id));
            iCur.Close();
         }
         return iId;
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
         int iId = GetIDFromLookup(id);
         string[] iCols;
         string iQuery;
         string[] iArgs;
         if (addressType == int.MinValue)
         {
            iCols = new string[] { ContactsContract.CommonDataKinds.Email.InterfaceConsts.ContactId, ContactsContract.CommonDataKinds.Email.InterfaceConsts.Data, ContactsContract.CommonDataKinds.Email.InterfaceConsts.Type, ContactsContract.CommonDataKinds.Email.InterfaceConsts.IsSuperPrimary };
            iQuery = string.Format("{0} = ? and {1} != 0", ContactsContract.CommonDataKinds.Email.InterfaceConsts.ContactId, ContactsContract.CommonDataKinds.Email.InterfaceConsts.IsSuperPrimary);
            iArgs = new String[] { iId.ToString() };
         }
         else
         {
            iCols = new string[] { ContactsContract.CommonDataKinds.Email.InterfaceConsts.ContactId, ContactsContract.CommonDataKinds.Email.InterfaceConsts.Data, ContactsContract.CommonDataKinds.Email.InterfaceConsts.Type };
            iQuery = string.Format("{0} = ? and {1} = ?", ContactsContract.CommonDataKinds.Email.InterfaceConsts.ContactId, ContactsContract.CommonDataKinds.Email.InterfaceConsts.Type);
            iArgs = new String[] { iId.ToString(), addressType.ToString() };
         }

         ICursor iCur = ReflectionSin.Context.ContentResolver.Query(ContactsContract.CommonDataKinds.Email.ContentUri, iCols, iQuery, iArgs, null);
         try
         {

            int iNrIndex = iCur.GetColumnIndex(ContactsContract.CommonDataKinds.Email.InterfaceConsts.Data);
            if (iCur.MoveToNext())
               return iCur.GetString(iNrIndex);
            else if (addressType == int.MinValue)
            {
               iCur.Close();
               iQuery = string.Format("{0} = ?", ContactsContract.CommonDataKinds.Email.InterfaceConsts.ContactId);
               iCur = ReflectionSin.Context.ContentResolver.Query(ContactsContract.CommonDataKinds.Email.ContentUri, iCols, iQuery, iArgs, null);
               if (iCur.MoveToNext())
                  return iCur.GetString(iNrIndex);
            }
            return null;
         }
         finally
         {
            iCur.Close();
         }
      }

      /// <summary>
      /// Instructs the network to load all the contact info. This is done through the wordnet sensory interface
      /// </summary>
      /// <param name="names"></param>
      /// <returns></returns>
      public static void LoadContactsInfo(Processor proc = null)
      {
         ContentResolver iResolver = ReflectionSin.Context.ContentResolver;
         string[] iCols = new string[] { ContactsContract.ContactsColumns.LookupKey};
         ICursor iCur = iResolver.Query(ContactsContract.Contacts.ContentUri, iCols, null, null, null);
         if (iCur.Count > 0)
         {
            WordNetSin iWordnet = Brain.Current[(ulong)PredefinedNeurons.WordNetSin] as WordNetSin;
            int iIdIndex = iCur.GetColumnIndex(ContactsContract.ContactsColumns.LookupKey);

            List<Neuron> iToSend = new List<Neuron>();                                    //1 list for all items.
            while (iCur.MoveToNext())
            {
               string iId = iCur.GetString(iIdIndex);
               iWordnet.BeginProcess(iId, iToSend);

               String iWhere = string.Format("{0} = ? AND {1} = ? ", ContactsContract.DataColumns.Mimetype, ContactsContract.ContactsColumns.LookupKey);
               String[] iWhereParams = new String[] { ContactsContract.CommonDataKinds.StructuredName.ContentItemType, iId };
               ICursor iNames = iResolver.Query(ContactsContract.Data.ContentUri, null, iWhere, iWhereParams, null);
               while (iNames.MoveToNext())
               {
                  String iGiven = iNames.GetString(iNames.GetColumnIndex(ContactsContract.CommonDataKinds.StructuredName.GivenName));
                  if (string.IsNullOrEmpty(iGiven) == false)
                     iWordnet.AddToProcess((ulong)PredefinedNeurons.Noun, new string[] { "name", "given name" }, iGiven, iToSend);
                  String iFamily = iNames.GetString(iNames.GetColumnIndex(ContactsContract.CommonDataKinds.StructuredName.FamilyName));
                  if (string.IsNullOrEmpty(iFamily) == false)
                     iWordnet.AddToProcess((ulong)PredefinedNeurons.Noun, new string[] { "name", "family name" }, iFamily, iToSend);
                  String iDisplay = iNames.GetString(iNames.GetColumnIndex(ContactsContract.CommonDataKinds.StructuredName.DisplayName));
                  if (string.IsNullOrEmpty(iDisplay) == false)
                     iWordnet.AddToProcess((ulong)PredefinedNeurons.Noun, new string[] { "name", "display name" }, iDisplay, iToSend);
               }
               iNames.Close();
            }
            iWordnet.Process(iToSend, "sync contact data", proc);
            iCur.Close();
         }
      }

      /// <summary>
      /// purely for testing.
      /// </summary>
      /// <returns></returns>
      public static string TestGetID()
      {
         String iId = null;
        
         ContentResolver iResolver = ReflectionSin.Context.ContentResolver;
         string[] iCols = new string[] { ContactsContract.ContactsColumns.LookupKey}; //we get deleted as well to make certain that we don't get wrong ids.
         ICursor iCur = iResolver.Query(ContactsContract.Contacts.ContentUri, iCols, null, null, null);
         if (iCur.Count > 0)
         {
            if (iCur.MoveToNext())
               iId = iCur.GetString(iCur.GetColumnIndex(ContactsContract.ContactsColumns.LookupKey));
            iCur.Close();
         }
         return iId;
      }

      internal static string GetContactId(string name)
      {
         throw new NotImplementedException();
      }
   }
}