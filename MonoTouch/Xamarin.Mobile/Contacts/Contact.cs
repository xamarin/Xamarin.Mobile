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

using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using MonoTouch.AddressBook;
using MonoTouch.CoreGraphics;
using MonoTouch.Foundation;
using MonoTouch.UIKit;
using System;
using System.Runtime.InteropServices;
using Xamarin.Media;

namespace Xamarin.Contacts
{
	public class Contact
	{
		public Contact()
		{
		}
		
		internal Contact (ABPerson person)
		{
			Id = person.Id.ToString();
			this.person = person;
		}

		public string Id
		{
			get;
			private set;
		}

		public bool IsAggregate
		{
			get;
			private set;
		}

		public string DisplayName
		{
			get;
			set;
		}

		public string Prefix
		{
			get;
			set;
		}

		public string FirstName
		{
			get;
			set;
		}

		public string MiddleName
		{
			get;
			set;
		}

		public string LastName
		{
			get;
			set;
		}

		public string Nickname
		{
			get;
			set;
		}

		public string Suffix
		{
			get;
			set;
		}

        public DateTime Birthday
		{
			get;
			set;
		}
        
		internal List<Relationship> relationships = new List<Relationship>();
		public IEnumerable<Relationship> Relationships
		{
			get { return this.relationships; }
			set { this.relationships = new List<Relationship> (value); }
		}

		internal List<Address> addresses = new List<Address>();
		public IEnumerable<Address> Addresses
		{
			get { return this.addresses; }
			set { this.addresses = new List<Address> (value); }
		}

		internal List<InstantMessagingAccount> imAccounts = new List<InstantMessagingAccount>();
		public IEnumerable<InstantMessagingAccount> InstantMessagingAccounts
		{
			get { return this.imAccounts; }
			set { this.imAccounts = new List<InstantMessagingAccount> (value); }
		}

		internal List<Website> websites = new List<Website>();
		public IEnumerable<Website> Websites
		{
			get { return this.websites; }
			set { this.websites = new List<Website> (value); }
		}

		internal List<Organization> organizations = new List<Organization>();
		public IEnumerable<Organization> Organizations
		{
			get { return this.organizations; }
			set { this.organizations = new List<Organization> (value); }
		}

		internal List<Note> notes = new List<Note>();
		public IEnumerable<Note> Notes
		{
			get { return this.notes; }
			set { this.notes = new List<Note> (value); }
		}

		internal List<Email> emails = new List<Email>();
		public IEnumerable<Email> Emails
		{
			get { return this.emails; }
			set { this.emails = new List<Email> (value); }
		}

		internal List<Phone> phones = new List<Phone>();
		public IEnumerable<Phone> Phones
		{
			get { return this.phones; }
			set { this.phones = new List<Phone> (value); }
		}

		public UIImage GetThumbnail()
		{
			if (!this.person.HasImage)
				return null;

			IntPtr data;
			lock (this.person)
				data = ABPersonCopyImageDataWithFormat (person.Handle, ABPersonImageFormat.Thumbnail);

			if (data == IntPtr.Zero)
				return null;

			return new UIImage (new NSData (data));
		}

		public Task<MediaFile> SaveThumbnailAsync (string path)
		{
			if (path == null)
				throw new ArgumentNullException ("path");

			return Task<MediaFile>.Factory.StartNew (s =>
			{
				string p = (string) s;

				using (UIImage img = GetThumbnail())
				{
					if (img == null)
						return null;

					using (NSDataStream stream = new NSDataStream (img.AsJPEG()))
					using (Stream fs = File.OpenWrite (p))
					{
						stream.CopyTo (fs);
						fs.Flush();
					}
				}

				return new MediaFile (p, () => File.OpenRead (path));
			}, path);
		}

		private readonly ABPerson person;

		[DllImport ("/System/Library/Frameworks/AddressBook.framework/AddressBook")]
		private static extern IntPtr ABPersonCopyImageDataWithFormat (IntPtr handle, ABPersonImageFormat format);
	}
}