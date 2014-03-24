//
//  Copyright 2014, Xamarin Inc.
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
using System.Linq.Expressions;
using Android.Provider;
using NUnit.Framework;
using Xamarin.Contacts;

namespace Xamarin.Mobile.Tests.Tests
{
	[TestFixture]
	class ContactTableFinderTests
	{
		[Test]
		public void FindContactSelect()
		{
			var finder = new ContactTableFinder { UseRawContacts = false };
			var result = finder.Find (GetExpression<Contact, string> (c => c.DisplayName));
			Assert.That (result, Is.Not.Null);
			Assert.That (result.MimeType, Is.Null);
			Assert.That (result.Table, Is.EqualTo (ContactsContract.Contacts.ContentUri));
		}

		[Test]
		public void FindContactWhere()
		{
			var finder = new ContactTableFinder { UseRawContacts = false };
			var result = finder.Find (GetExpression<Contact, bool> (c => c.DisplayName == "Display Name"));
			Assert.That (result, Is.Not.Null);
			Assert.That (result.MimeType, Is.Null);
			Assert.That (result.Table, Is.EqualTo (ContactsContract.Contacts.ContentUri));
		}

		[Test]
		public void FindContactSelectData()
		{
			var finder = new ContactTableFinder { UseRawContacts = false };
			var result = finder.Find (GetExpression<Contact, string> (c => c.FirstName));
			Assert.That (result, Is.Not.Null);
			Assert.That (result.MimeType, Is.EqualTo (ContactsContract.CommonDataKinds.StructuredName.ContentItemType));
			Assert.That (result.Table, Is.EqualTo (ContactsContract.Data.ContentUri));
		}

		[Test]
		public void FindContactWhereData()
		{
			var finder = new ContactTableFinder { UseRawContacts = false };
			var result = finder.Find (GetExpression<Contact, bool> (c => c.FirstName == "First"));
			Assert.That (result, Is.Not.Null);
			Assert.That (result.MimeType, Is.EqualTo (ContactsContract.CommonDataKinds.StructuredName.ContentItemType));
			Assert.That (result.Table, Is.EqualTo (ContactsContract.Data.ContentUri));
		}

		Expression GetExpression<TIn, TOut> (Expression<Func<TIn, TOut>> expr)
		{
			return expr;
		}
	}
}