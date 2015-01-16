//
//  Copyright 2011-2013, Xamarin Inc.
//
//    Licensed under the Apache License, Version 2.0 (the "License");
//    you may not use this file except in compliance with the License.
//    You may obtain a copy of the License at
//
//        http://www.apache.org/licenses/LICENSE-2.0
//
//    Unless required by applicable law or agreed to in writing, software
//    distributed under the License is distributed on an "AS IS" BASIS,
//    WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//    See the License for the specific language governing permissions and
//    limitations under the License.
//

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Android.Content;
using Android.Content.Res;
using Android.Database;
using Android.Provider;

namespace Xamarin.Contacts
{
	public sealed class AddressBook
		: IQueryable<Contact>
	{
		public AddressBook (Context context)
		{
			if (context == null)
				throw new ArgumentNullException ("context");


			this.content = context.ContentResolver;
			this.resources = context.Resources;
			this.contactsProvider = new ContactQueryProvider (context.ContentResolver, context.Resources);
		}

		public bool IsReadOnly
		{
			get { return true; }
		}

		public bool SingleContactsSupported
		{
			get { return true; }
		}

		public bool AggregateContactsSupported
		{
			get { return true; }
		}

		public bool PreferContactAggregation
		{
			get { return !this.contactsProvider.UseRawContacts; }
			set { this.contactsProvider.UseRawContacts = !value; }
		}

		public bool LoadSupported
		{
			get { return true; }
		}

		public Task<bool> RequestPermission()
		{
			return Task.Factory.StartNew (() =>
			{
				try
				{
					ICursor cursor = this.content.Query (ContactsContract.Data.ContentUri, null, null, null, null);
					cursor.Dispose();

					return true;
				}
				catch (Java.Lang.SecurityException)
				{
					return false;
				}
			});
		}

		public IEnumerator<Contact> GetEnumerator()
		{
			return ContactHelper.GetContacts (!PreferContactAggregation, this.content, this.resources).GetEnumerator();
		}

		/// <summary>
		/// Attempts to load a contact for the specified <paramref name="id"/>.
		/// </summary>
		/// <param name="id"></param>
		/// <returns>The <see cref="Contact"/> if found, <c>null</c> otherwise.</returns>
		/// <exception cref="ArgumentNullException"><paramref name="id"/> is <c>null</c>.</exception>
		/// <exception cref="ArgumentException"><paramref name="id"/> is empty.</exception>
		public Contact Load (string id)
		{
			if (id == null)
				throw new ArgumentNullException ("id");
			if (id.Trim() == String.Empty)
				throw new ArgumentException ("Invalid ID", "id");

			Android.Net.Uri curi; string column;
			if (PreferContactAggregation)
			{
				curi = ContactsContract.Contacts.ContentUri;
				column = ContactsContract.ContactsColumns.LookupKey;
			}
			else
			{
				curi = ContactsContract.RawContacts.ContentUri;
				column = ContactsContract.RawContactsColumns.ContactId;
			}

			ICursor c = null;
			try
			{
				c = this.content.Query (curi, null, column + " = ?", new[] { id }, null);
				return (c.MoveToNext() ? ContactHelper.GetContact (!PreferContactAggregation, this.content, this.resources, c) : null);
			}
			finally
			{
				if (c != null)
					c.Deactivate();
			}
		}

        private Contact SaveNew(Contact contact)
        {
            if (contact == null)
                throw new ArgumentNullException("contact");
            if (contact.Id != null)
                throw new ArgumentException("Contact is not new", "contact");

            List<ContentProviderOperation> ops = new List<ContentProviderOperation>();

            ContentProviderOperation.Builder builder =
                ContentProviderOperation.NewInsert(ContactsContract.RawContacts.ContentUri);
            builder.WithValue(ContactsContract.RawContacts.InterfaceConsts.AccountType, null);
            builder.WithValue(ContactsContract.RawContacts.InterfaceConsts.AccountName, null);
            ops.Add(builder.Build());

            //Name
            builder = ContentProviderOperation.NewInsert(ContactsContract.Data.ContentUri);
            builder.WithValueBackReference(ContactsContract.Data.InterfaceConsts.RawContactId, 0);
            builder.WithValue(ContactsContract.Data.InterfaceConsts.Mimetype,
                              ContactsContract.CommonDataKinds.StructuredName.ContentItemType);
            builder.WithValue(ContactsContract.CommonDataKinds.StructuredName.FamilyName, contact.LastName);
			builder.WithValue(ContactsContract.CommonDataKinds.StructuredName.GivenName, contact.FirstName);
			builder.WithValue(ContactsContract.CommonDataKinds.StructuredName.MiddleName, contact.MiddleName);
			builder.WithValue(ContactsContract.CommonDataKinds.StructuredName.DisplayName, contact.Nickname);
			builder.WithValue(ContactsContract.CommonDataKinds.StructuredName.Prefix, contact.Prefix);
			builder.WithValue(ContactsContract.CommonDataKinds.StructuredName.Suffix, contact.Suffix);
            ops.Add(builder.Build());

            //Addresses
            foreach (var item in contact.Addresses)
            {
                builder = ContentProviderOperation.NewInsert(ContactsContract.Data.ContentUri);
                builder.WithValueBackReference(ContactsContract.Data.InterfaceConsts.RawContactId, 0);
                builder.WithValue(ContactsContract.Data.InterfaceConsts.Mimetype, ContactsContract.CommonDataKinds.StructuredPostal.ContentItemType);
                builder.WithValue(ContactsContract.CommonDataKinds.StructuredPostal.Street, item.StreetAddress);
                builder.WithValue(ContactsContract.CommonDataKinds.StructuredPostal.InterfaceConsts.Type, ContactsContract.CommonDataKinds.StructuredPostal.InterfaceConsts.TypeCustom);
                builder.WithValue(ContactsContract.CommonDataKinds.StructuredPostal.Postcode, item.PostalCode);
                builder.WithValue(ContactsContract.CommonDataKinds.StructuredPostal.City, item.City);
                builder.WithValue(ContactsContract.CommonDataKinds.StructuredPostal.Country, item.Country);
                builder.WithValue(ContactsContract.CommonDataKinds.StructuredPostal.Region, item.Region);
                ops.Add(builder.Build());
            }
            
            //Number
            foreach (var item in contact.Phones)
            {
                builder = ContentProviderOperation.NewInsert(ContactsContract.Data.ContentUri);
                builder.WithValueBackReference(ContactsContract.Data.InterfaceConsts.RawContactId, 0);
                builder.WithValue(ContactsContract.Data.InterfaceConsts.Mimetype,
                                  ContactsContract.CommonDataKinds.Phone.ContentItemType);
                builder.WithValue(ContactsContract.CommonDataKinds.Phone.Number, item.Number);
                builder.WithValue(ContactsContract.CommonDataKinds.Phone.InterfaceConsts.Type,
                                  ContactsContract.CommonDataKinds.Phone.InterfaceConsts.TypeCustom);
                builder.WithValue(ContactsContract.CommonDataKinds.Phone.InterfaceConsts.Label, item.Label);
                ops.Add(builder.Build());
            }

            //Email
            foreach (var item in contact.Emails)
            {
                builder = ContentProviderOperation.NewInsert(ContactsContract.Data.ContentUri);
                builder.WithValueBackReference(ContactsContract.Data.InterfaceConsts.RawContactId, 0);
                builder.WithValue(ContactsContract.Data.InterfaceConsts.Mimetype,
                                  ContactsContract.CommonDataKinds.Email.ContentItemType);
                builder.WithValue(ContactsContract.CommonDataKinds.Email.InterfaceConsts.Data, item.Address);
                builder.WithValue(ContactsContract.CommonDataKinds.Email.InterfaceConsts.Type,
                                  ContactsContract.CommonDataKinds.Email.InterfaceConsts.TypeCustom);
                builder.WithValue(ContactsContract.CommonDataKinds.Email.InterfaceConsts.Label, item.Label);
                ops.Add(builder.Build());
            }

            //Website
            foreach (var item in contact.Websites)
            {
                builder = ContentProviderOperation.NewInsert(ContactsContract.Data.ContentUri);
                builder.WithValueBackReference(ContactsContract.Data.InterfaceConsts.RawContactId, 0);
                builder.WithValue(ContactsContract.Data.InterfaceConsts.Mimetype, ContactsContract.CommonDataKinds.Website.ContentItemType);
                builder.WithValue(ContactsContract.CommonDataKinds.Website.Url, item.Address);
                builder.WithValue(ContactsContract.CommonDataKinds.Website.InterfaceConsts.Type, ContactsContract.CommonDataKinds.Website.InterfaceConsts.TypeCustom);
                builder.WithValue(ContactsContract.CommonDataKinds.Website.InterfaceConsts.Label, "Homepage");
                ops.Add(builder.Build());
            }

            ////Notes
            //foreach (var item in contact.Notes)
            //{
            //    builder = ContentProviderOperation.NewInsert(ContactsContract.Data.ContentUri);
            //    builder.WithValueBackReference(ContactsContract.Data.InterfaceConsts.RawContactId, 0);
            //    builder.WithValue(ContactsContract.Data.InterfaceConsts.Mimetype, ContactsContract.CommonDataKinds.Note.ContentItemType);
            //    ...
            //    ...
            //    ops.Add(builder.Build());
            //}

            //Company
            foreach (var item in contact.Organizations)
            {
                builder = ContentProviderOperation.NewInsert(ContactsContract.Data.ContentUri);
                builder.WithValueBackReference(ContactsContract.Data.InterfaceConsts.RawContactId, 0);
                builder.WithValue(ContactsContract.Data.InterfaceConsts.Mimetype,
                                  ContactsContract.CommonDataKinds.Organization.ContentItemType);
                builder.WithValue(ContactsContract.CommonDataKinds.Organization.InterfaceConsts.Data, item.Name);
                builder.WithValue(ContactsContract.CommonDataKinds.Organization.InterfaceConsts.Type,
                                  ContactsContract.CommonDataKinds.Organization.InterfaceConsts.TypeCustom);
                builder.WithValue(ContactsContract.CommonDataKinds.Organization.InterfaceConsts.Label, item.Name);
                ops.Add(builder.Build());
            }

            //Add the new contact
            ContentProviderResult[] res;
            try
            {
                res = this.content.ApplyBatch(ContactsContract.Authority, ops);
                //Toast.MakeText(context, context.Resources.GetString(Resource.String.contact_saved_message), ToastLength.Short).Show();
            }
            catch
            {
                //Toast.MakeText(context, context.Resources.GetString(Resource.String.contact_not_saved_message), ToastLength.Long).Show();
            }
            return contact;
        }

        public Contact SaveExisting(Contact contact)
        {
            if (contact == null)
                throw new ArgumentNullException("contact");
            if (String.IsNullOrWhiteSpace(contact.Id))
                throw new ArgumentException("Contact is not existing");

            throw new NotImplementedException();

            return Load(contact.Id);
        }

        public Contact Save(Contact contact)
        {
            if (contact == null)
                throw new ArgumentNullException("contact");

            return (string.IsNullOrWhiteSpace(contact.Id) ? SaveNew(contact) : SaveExisting(contact));
        }

		//public void Delete (Contact contact)
		//{
		//    if (contact == null)
		//        throw new ArgumentNullException ("contact");
		//    if (!String.IsNullOrWhiteSpace (contact.Id))
		//        throw new ArgumentException ("Contact is not a persisted instance", "contact");

		//    // TODO: Does this cascade?
		//    this.content.Delete (ContactsContract.RawContacts.ContentUri, ContactsContract.RawContactsColumns.ContactId + " = ?", new[] { contact.Id });
		//}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}

		Type IQueryable.ElementType
		{
			get { return typeof (Contact); }
		}

		Expression IQueryable.Expression
		{
			get { return Expression.Constant (this); }
		}

		IQueryProvider IQueryable.Provider
		{
			get { return this.contactsProvider; }
		}

		private readonly ContactQueryProvider contactsProvider;
		private readonly ContentResolver content;
		private readonly Resources resources;
	}
}