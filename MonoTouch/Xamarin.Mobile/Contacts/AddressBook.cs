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
using System.Linq;
using System.Linq.Expressions;
using MonoTouch.AddressBook;
using System.Collections.Generic;
using System.Threading.Tasks;
using MonoTouch.UIKit;
using MonoTouch.Foundation;

namespace Xamarin.Contacts
{
	public class AddressBook
		: IEnumerable<Contact> //IQueryable<Contact>
	{
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
			get { return false; }
		}

		public bool PreferContactAggregation
		{
			get;
			set;
		}

		public bool LoadSupported
		{
			get { return true; }
		}

		public Task<bool> RequestPermission()
		{
			var tcs = new TaskCompletionSource<bool>();
			if (UIDevice.CurrentDevice.CheckSystemVersion (6, 0))
			{
				var status = ABAddressBook.GetAuthorizationStatus();
				if (status == ABAuthorizationStatus.Denied || status == ABAuthorizationStatus.Restricted)
					tcs.SetResult (false);
				else
				{
					if (this.addressBook == null)
					{
						this.addressBook = new ABAddressBook();
						this.provider = new ContactQueryProvider (this.addressBook);
					}

					if (status == ABAuthorizationStatus.NotDetermined)
					{
						this.addressBook.RequestAccess ((s,e) =>
						{
							tcs.SetResult (s);
							if (!s)
							{
								this.addressBook.Dispose();
								this.addressBook = null;
								this.provider = null;
							}
						});
					}
					else
						tcs.SetResult (true);
				}
			}
			else
				tcs.SetResult (true);

			return tcs.Task;
		}

		public IEnumerator<Contact> GetEnumerator()
		{
			CheckStatus();

			return this.addressBook.GetPeople().Select (ContactHelper.GetContact).GetEnumerator();
		}

		public Contact Load (string id)
		{
			if (String.IsNullOrWhiteSpace (id))
				throw new ArgumentNullException ("id");

			CheckStatus();

			int rowId;
			if (!Int32.TryParse (id, out rowId))
				throw new ArgumentException ("Not a valid contact ID", "id");
			
			ABPerson person = this.addressBook.GetPerson (rowId);
			if (person == null)
				return null;
			
			return ContactHelper.GetContact (person);
		}

        private NSString NSStringNotNil(string aString)
        {
            if (aString == null)
                return new NSString("");
            return new NSString(aString);
        }

        public void Save(Contact contact)
        {
            if (contact == null)
                throw new ArgumentNullException("contact");

            CheckStatus();

            //convert the Contact to a ABPerson
            ABPerson person = new ABPerson();

            ABMutableDictionaryMultiValue addresses = new ABMutableDictionaryMultiValue();
            foreach (var item in contact.Addresses)
            {
                NSMutableDictionary a = new NSMutableDictionary();
                a.Add(new NSString(ABPersonAddressKey.City), NSStringNotNil(item.City));
                a.Add(new NSString(ABPersonAddressKey.State), NSStringNotNil(item.Region));
                a.Add(new NSString(ABPersonAddressKey.Zip), NSStringNotNil(item.PostalCode));
                a.Add(new NSString(ABPersonAddressKey.Street), NSStringNotNil(item.StreetAddress));
                addresses.Add(a, NSStringNotNil(item.Label));
            }
            person.SetAddresses(addresses);

            //Todo contact.DisplayName missing in ABPerson?
            //DisplayName = RelatedNames ???
            //ABMutableStringMultiValue relatedNames = new ABMutableStringMultiValue();
            //relatedNames.Add(ABPersonProperty.RelatedNames.ToString(), NSStringNotNil(contact.DisplayName));
            //person.SetRelatedNames(relatedNames);

            if (contact.Birthday.Year > 1)
            {
                person.Birthday = DateTime.SpecifyKind(contact.Birthday, DateTimeKind.Local);
            }

            ABMutableStringMultiValue emails = new ABMutableStringMultiValue();
            foreach (var item in contact.Emails)
            {
                emails.Add(item.Address, NSStringNotNil(item.Label));
            }
            person.SetEmails(emails);

            person.FirstName = contact.FirstName;

            //contact.InstantMessagingAccounts missing in ABPerson?

            person.LastName = contact.LastName;
            person.MiddleName = contact.MiddleName;
            person.Nickname = contact.Nickname;

            string notes = "";
            foreach (var item in contact.Notes)
            {
                notes += item.Contents + Environment.NewLine + Environment.NewLine;
            }
            person.Note = notes;

            //Todo multiple organizations are not supportet?
            if (contact.Organizations.Count() > 0)
            {
                person.Organization = contact.Organizations.FirstOrDefault().Name;
            }

            ABMutableStringMultiValue phones = new ABMutableStringMultiValue();
            foreach (var item in contact.Phones)
            {
                phones.Add(item.Number, NSStringNotNil(item.Label));
            }
            person.SetPhones(phones);

            person.Prefix = contact.Prefix;

            ABMutableStringMultiValue relationships = new ABMutableStringMultiValue();
            foreach (var item in contact.Relationships)
            {
                relationships.Add(item.Name, NSStringNotNil(item.Type.ToString()));
            }
            person.SetRelatedNames(relationships);

            person.Suffix = contact.Suffix;

            ABMutableStringMultiValue websites = new ABMutableStringMultiValue();
            foreach (var item in contact.Websites)
            {
                websites.Add(item.Address, NSStringNotNil("Url"));
            }
            person.SetUrls(websites);

            this.addressBook.Add(person);
            this.addressBook.Save();
        }

		private ABAddressBook addressBook;
		private IQueryProvider provider;

		private void CheckStatus()
		{
			if (UIDevice.CurrentDevice.CheckSystemVersion (6, 0))
			{
				var status = ABAddressBook.GetAuthorizationStatus();
				if (status != ABAuthorizationStatus.Authorized)
					throw new System.Security.SecurityException ("AddressBook has not been granted permission");
			}

			if (this.addressBook == null)
			{
				this.addressBook = new ABAddressBook();
				this.provider = new ContactQueryProvider (this.addressBook);
			}
		}
		
		System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}
		
//		Type IQueryable.ElementType
//		{
//			get { return typeof(Contact); }
//		}
//		
//		Expression IQueryable.Expression
//		{
//			get { return Expression.Constant (this); }
//		}
//		
//		IQueryProvider IQueryable.Provider
//		{
//			get { return this.provider; }
//		}
	}
}