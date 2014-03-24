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
using System.Linq;
using Android.Provider;
using NUnit.Framework;
using Xamarin.Contacts;

namespace Xamarin.Mobile.Tests.Tests
{
	[TestFixture]
	class ContentQueryTranslatorTests
	{
		[Test]
		public void FirstSingleClause()
		{
			Queryable.First (c => c.DisplayName == "Display Name");

			AssertSingularWithSingleClause();
			Assert.That (Translator.IsAny, Is.False);
			Assert.That (Translator.IsCount, Is.False);
			Assert.That (Translator.Take, Is.EqualTo (1));
		}

		[Test]
		public void FirstOrDefaultSingleClause()
		{
			Queryable.FirstOrDefault (c => c.DisplayName == "Display Name");

			AssertSingularWithSingleClause();
			Assert.That (Translator.IsAny, Is.False);
			Assert.That (Translator.IsCount, Is.False);
			Assert.That (Translator.Take, Is.EqualTo (1));
		}

		[Test]
		public void SingleOrDefaultSingleClause()
		{
			Queryable.SingleOrDefault (c => c.DisplayName == "Display Name");

			AssertSingularWithSingleClause();
			Assert.That (Translator.IsAny, Is.False);
			Assert.That (Translator.IsCount, Is.False);
			Assert.That (Translator.Take, Is.EqualTo (2));
		}

		[Test]
		public void SingleSingleClause()
		{
			Queryable.Single (c => c.DisplayName == "Display Name");

			AssertSingularWithSingleClause();
			Assert.That (Translator.IsAny, Is.False);
			Assert.That (Translator.IsCount, Is.False);
			Assert.That (Translator.Take, Is.EqualTo (2));
		}

		[Test]
		public void WhereSingleClause()
		{
			Queryable.Where (c => c.DisplayName == "Display Name").ToArray();

			AssertSingularWithSingleClause();
			Assert.That (Translator.IsAny, Is.False);
			Assert.That (Translator.IsCount, Is.False);
			Assert.That (Translator.Take, Is.EqualTo (-1));
		}

		[Test]
		public void CountSingleClause()
		{
			Queryable.Count (c => c.DisplayName == "Display Name");

			AssertSingularWithSingleClause();
			Assert.That (Translator.IsAny, Is.False);
			Assert.That (Translator.IsCount, Is.True);
			Assert.That (Translator.Take, Is.EqualTo (-1));
		}

		[Test]
		public void AnySingleClause()
		{
			Queryable.Any (c => c.DisplayName == "Display Name");

			AssertSingularWithSingleClause();
			Assert.That (Translator.IsAny, Is.True);
			Assert.That (Translator.IsCount, Is.False);
			Assert.That (Translator.Take, Is.EqualTo (-1));
		}

		public void AssertSingularWithSingleClause()
		{
			Translator.Translate (Queryable.LastExpression);
			Assert.That (Translator.Skip, Is.EqualTo (-1), "Skip was not -1");
			Assert.That (Translator.QueryString, Is.EqualTo ("(display_name = ?)"));
			Assert.That (Translator.Projections, Is.Null.Or.Empty, "Projections were present");
			Assert.That (Translator.ClauseParameters, Contains.Item ("Display Name"));
			Assert.That (Translator.ReturnType, Is.Null.Or.EqualTo (typeof (Contact)));
			Assert.That (Translator.Table, Is.EqualTo (ContactsContract.Contacts.ContentUri));
		}

		[Test]
		public void FirstTwoClausesSameTableSameMimetype()
		{
			Queryable.First (c => c.FirstName == "Foo" && c.LastName == "Bar");

			AssertSingularWithTwoClausesFromSameTableAndMimeType();
			Assert.That (Translator.IsAny, Is.False);
			Assert.That (Translator.IsCount, Is.False);
			Assert.That (Translator.Take, Is.EqualTo (1));
		}

		[Test]
		public void FirstOrDefaultTwoClausesSameTableSameMimetype()
		{
			Queryable.FirstOrDefault (c => c.FirstName == "Foo" && c.LastName == "Bar");

			AssertSingularWithTwoClausesFromSameTableAndMimeType();
			Assert.That (Translator.IsAny, Is.False);
			Assert.That (Translator.IsCount, Is.False);
			Assert.That (Translator.Take, Is.EqualTo (1));
		}

		[Test]
		public void SingleTwoClausesSameTableSameMimetype()
		{
			Queryable.Single (c => c.FirstName == "Foo" && c.LastName == "Bar");

			AssertSingularWithTwoClausesFromSameTableAndMimeType ();
			Assert.That (Translator.IsAny, Is.False);
			Assert.That (Translator.IsCount, Is.False);
			Assert.That (Translator.Take, Is.EqualTo (2));
		}

		[Test]
		public void SingleOrDefaultTwoClausesSameTableSameMimetype()
		{
			Queryable.SingleOrDefault (c => c.FirstName == "Foo" && c.LastName == "Bar");

			AssertSingularWithTwoClausesFromSameTableAndMimeType();
			Assert.That (Translator.IsAny, Is.False);
			Assert.That (Translator.IsCount, Is.False);
			Assert.That (Translator.Take, Is.EqualTo (2));
		}

		[Test]
		public void WhereSingleTwoClausesSameTableSameMimeType()
		{
			Queryable.Where (c => c.FirstName == "Foo" && c.LastName == "Bar").ToArray();

			AssertSingularWithTwoClausesFromSameTableAndMimeType();
			Assert.That (Translator.IsAny, Is.False);
			Assert.That (Translator.IsCount, Is.False);
			Assert.That (Translator.Take, Is.EqualTo (-1));
		}

		[Test]
		public void CountSingleTwoClausesSameTableSameMimeType()
		{
			Queryable.Count (c => c.FirstName == "Foo" && c.LastName == "Bar");

			AssertSingularWithTwoClausesFromSameTableAndMimeType();
			Assert.That (Translator.IsAny, Is.False);
			Assert.That (Translator.IsCount, Is.True);
			Assert.That (Translator.Take, Is.EqualTo (-1));
		}

		[Test]
		public void AnySingleTwoClausesSameTableSameMimeType()
		{
			Queryable.Any (c => c.FirstName == "Foo" && c.LastName == "Bar");

			AssertSingularWithTwoClausesFromSameTableAndMimeType();
			Assert.That (Translator.IsAny, Is.True);
			Assert.That (Translator.IsCount, Is.False);
			Assert.That (Translator.Take, Is.EqualTo (-1));
		}

		public void AssertSingularWithTwoClausesFromSameTableAndMimeType()
		{
			Translator.Translate (Queryable.LastExpression);

			Assert.That (Translator.Skip, Is.EqualTo (-1), "Skip was not -1");
			Assert.That (Translator.QueryString, Is.EqualTo (
				String.Format ("(({0} = ?) AND (({1} = ?) AND ({2} = ?)))",
					ContactsContract.DataColumns.Mimetype,
					ContactsContract.CommonDataKinds.StructuredName.GivenName,
					ContactsContract.CommonDataKinds.StructuredName.FamilyName)));
			Assert.That (Translator.Projections, Is.Null.Or.Empty, "Projections were present");
			Assert.That (Translator.ClauseParameters[0], Is.EqualTo (ContactsContract.CommonDataKinds.StructuredName.ContentItemType));
			Assert.That (Translator.ClauseParameters[1], Is.EqualTo ("Foo"));
			Assert.That (Translator.ClauseParameters[2], Is.EqualTo ("Bar"));
			Assert.That (Translator.ReturnType, Is.Null.Or.EqualTo (typeof (Contact)));
			Assert.That (Translator.Table, Is.EqualTo (ContactsContract.Data.ContentUri));
			Assert.That (Translator.SortString, Is.Null.Or.Empty);
		}

		[Test]
		public void WhereTwoClausesSameTableDifferentMimeType()
		{
			Queryable.Where (c => c.FirstName == "Foo" && c.DisplayName == "Foo Bar").ToArray();
			AssertWhereWithTwoClausesFromSameTableDifferentMimeType();
			Assert.That (Translator.IsAny, Is.False);
			Assert.That (Translator.IsCount, Is.False);
			Assert.That (Translator.Take, Is.EqualTo (-1), "Take was not -1");
			Assert.That (Translator.Skip, Is.EqualTo (-1), "Skip was not -1");
		}

		[Test]
		[Category ("Any"), Category ("TwoClausesSameTableDifferentMimeType")]
		public void AnyTwoClausesSameTableDifferentMimeType()
		{
			Queryable.Any (c => c.FirstName == "Foo" && c.DisplayName == "Foo Bar");
			AssertWhereWithTwoClausesFromSameTableDifferentMimeType();
			Assert.That (Translator.IsAny, Is.True);
			Assert.That (Translator.IsCount, Is.False);
			Assert.That (Translator.Take, Is.EqualTo (-1), "Take was not -1");
			Assert.That (Translator.Skip, Is.EqualTo (-1), "Skip was not -1");
		}

		[Test]
		[Category ("Count"), Category ("TwoClausesSameTableDifferentMimeType")]
		public void CountTwoClausesSameTableDifferentMimeType()
		{
			Queryable.Count (c => c.FirstName == "Foo" && c.DisplayName == "Foo Bar");
			AssertWhereWithTwoClausesFromSameTableDifferentMimeType();
			Assert.That (Translator.IsAny, Is.False);
			Assert.That (Translator.IsCount, Is.True);
			Assert.That (Translator.Take, Is.EqualTo (-1), "Take was not -1");
			Assert.That (Translator.Skip, Is.EqualTo (-1), "Skip was not -1");
		}

		[Test]
		public void FirstTwoClausesSameTableDifferentMimeType()
		{
			Queryable.First (c => c.FirstName == "Foo" && c.DisplayName == "Foo Bar");
			AssertWhereWithTwoClausesFromSameTableDifferentMimeType();
			Assert.That (Translator.IsAny, Is.False);
			Assert.That (Translator.IsCount, Is.False);
			Assert.That (Translator.Take, Is.EqualTo (1), "Take was not 1");
			Assert.That (Translator.Skip, Is.EqualTo (-1), "Skip was not -1");
		}

		[Test]
		public void FirstOrDefaultTwoClausesSameTableDifferentMimeType()
		{
			Queryable.FirstOrDefault (c => c.FirstName == "Foo" && c.DisplayName == "Foo Bar");
			AssertWhereWithTwoClausesFromSameTableDifferentMimeType();
			Assert.That (Translator.IsAny, Is.False);
			Assert.That (Translator.IsCount, Is.False);
			Assert.That (Translator.Take, Is.EqualTo (1), "Take was not 1");
			Assert.That (Translator.Skip, Is.EqualTo (-1), "Skip was not -1");
		}

		[Test]
		public void SingleTwoClausesSameTableDifferentMimetype()
		{
			Queryable.Single (c => c.FirstName == "Foo" && c.DisplayName == "Foo Bar");
			AssertWhereWithTwoClausesFromSameTableDifferentMimeType();
			Assert.That (Translator.IsAny, Is.False);
			Assert.That (Translator.IsCount, Is.False);
			Assert.That (Translator.Take, Is.EqualTo (2), "Take was not 2");
			Assert.That (Translator.Skip, Is.EqualTo (-1), "Skip was not -1");
		}

		[Test]
		public void SingleOrDefaultTwoClausesSameTableDifferentMimetype()
		{
			Queryable.SingleOrDefault (c => c.FirstName == "Foo" && c.DisplayName == "Foo Bar");
			AssertWhereWithTwoClausesFromSameTableDifferentMimeType ();
			Assert.That (Translator.IsAny, Is.False);
			Assert.That (Translator.IsCount, Is.False);
			Assert.That (Translator.Take, Is.EqualTo (2), "Take was not 2");
			Assert.That (Translator.Skip, Is.EqualTo (-1), "Skip was not -1");
		}

		public void AssertWhereWithTwoClausesFromSameTableDifferentMimeType()
		{
			Translator.Translate (Queryable.LastExpression);

			Assert.That (Translator.Skip, Is.EqualTo (-1), "Skip was not -1");
			Assert.That (Translator.QueryString, Is.EqualTo (
				String.Format ("(({0} = ?) AND (({1} = ?)))",
					ContactsContract.DataColumns.Mimetype,
					ContactsContract.CommonDataKinds.StructuredName.GivenName)));
			Assert.That (Translator.Projections, Is.Null.Or.Empty, "Projections were present");
			Assert.That (Translator.ClauseParameters[0], Is.EqualTo (ContactsContract.CommonDataKinds.StructuredName.ContentItemType));
			Assert.That (Translator.ClauseParameters[1], Is.EqualTo ("Foo"));
			Assert.That (Translator.ClauseParameters.Length, Is.EqualTo (2));
			Assert.That (Translator.ReturnType, Is.Null.Or.EqualTo (typeof (Contact)));
			Assert.That (Translator.Table, Is.EqualTo (ContactsContract.Data.ContentUri));
			Assert.That (Translator.SortString, Is.Null.Or.Empty);
		}

		[Test]
		public void WhereWithOrClauseFromSameTableDifferentMimeType()
		{
			Queryable.Where (c => c.FirstName == "Foo" || c.DisplayName == "Foo Bar").ToArray();
			AssertFallback();
		}

		[Test]
		public void WhereFallback()
		{
			Queryable.Where (c => c.Organizations.Any()).ToArray();
			AssertFallback();
		}

		[Test]
		public void FirstFallback()
		{
			Queryable.First (c => c.Organizations.Any());
			AssertFallback();
		}

		[Test]
		public void FirstOrDefaultFallback()
		{
			Queryable.FirstOrDefault (c => c.Organizations.Any());
			AssertFallback();
		}

		[Test]
		public void SingleFallback()
		{
			Queryable.Single (c => c.Organizations.Any());
			AssertFallback();
		}

		[Test]
		public void SingleOrDefaultFallback()
		{
			Queryable.SingleOrDefault (c => c.Organizations.Any());
			AssertFallback();
		}

		[Test]
		public void AnyFallback()
		{
			Queryable.Any (c => c.Organizations.Any());
			AssertFallback();
		}

		[Test]
		public void CountFallback()
		{
			Queryable.Count (c => c.Organizations.Any());
			AssertFallback();
		}

		public void AssertFallback()
		{
			Translator.Translate (Queryable.LastExpression);

			Assert.That (Translator.IsAny, Is.False);
			Assert.That (Translator.IsCount, Is.False);
			Assert.That (Translator.Skip, Is.EqualTo (-1));
			Assert.That (Translator.Take, Is.EqualTo (-1));
			Assert.That (Translator.Projections, Is.Null.Or.Empty);
			Assert.That (Translator.QueryString, Is.Null.Or.Empty);
			Assert.That (Translator.SortString, Is.Null.Or.Empty);
			Assert.That (Translator.ReturnType, Is.Null);
			Assert.That (Translator.Table, Is.EqualTo (ContactsContract.Contacts.ContentUri));
			Assert.That (Translator.ClauseParameters, Is.Null.Or.Empty);
		}

		[Test]
		public void Count()
		{
			Queryable.Count();
			Translator.Translate (Queryable.LastExpression);

			Assert.That (Translator.IsAny, Is.False);
			Assert.That (Translator.IsCount, Is.True);
			Assert.That (Translator.Take, Is.EqualTo (-1));
			Assert.That (Translator.Skip, Is.EqualTo (-1));
			Assert.That (Translator.QueryString, Is.Null.Or.Empty);
			Assert.That (Translator.ClauseParameters, Is.Null.Or.Empty);
			Assert.That (Translator.Projections, Is.Null.Or.Empty);
			Assert.That (Translator.SortString, Is.Null.Or.Empty);
			Assert.That (Translator.Table, Is.EqualTo (ContactsContract.Contacts.ContentUri));
			Assert.That (Translator.ReturnType, Is.Null);
		}

		[Test]
		public void Any()
		{
			Queryable.Any();
			Translator.Translate (Queryable.LastExpression);

			Assert.That (Translator.IsAny, Is.True);
			Assert.That (Translator.IsCount, Is.False);
			Assert.That (Translator.Take, Is.EqualTo (-1));
			Assert.That (Translator.Skip, Is.EqualTo (-1));
			Assert.That (Translator.QueryString, Is.Null);
			Assert.That (Translator.ClauseParameters, Is.Null.Or.Empty);
			Assert.That (Translator.Projections, Is.Null.Or.Empty);
			Assert.That (Translator.SortString, Is.Null.Or.Empty);
			Assert.That (Translator.Table, Is.EqualTo (ContactsContract.Contacts.ContentUri));
			Assert.That (Translator.ReturnType, Is.Null);
		}

		[Test]
		public void Skip()
		{
			Queryable.Skip (5).ToArray();
			Translator.Translate (Queryable.LastExpression);

			Assert.That (Translator.IsAny, Is.False);
			Assert.That (Translator.IsCount, Is.False);
			Assert.That (Translator.Take, Is.EqualTo (-1));
			Assert.That (Translator.Skip, Is.EqualTo (5));
			Assert.That (Translator.QueryString, Is.Null);
			Assert.That (Translator.ClauseParameters, Is.Null.Or.Empty);
			Assert.That (Translator.Projections, Is.Null.Or.Empty);
			Assert.That (Translator.SortString, Is.Null.Or.Empty);
			Assert.That (Translator.Table, Is.EqualTo (ContactsContract.Contacts.ContentUri));
			Assert.That (Translator.ReturnType, Is.Null);
		}

		private MockQueryable<Contact> Queryable
		{
			get { return this.queryable; }
		}

		private ContentQueryTranslator Translator
		{
			get { return this.translator; }
		}

		private MockQueryable<Contact> queryable;
		private ContentQueryTranslator translator;

		[SetUp]
		public void Setup()
		{
			this.queryable = new MockQueryable<Contact>();
			this.translator = new ContentQueryTranslator (this.queryable.Provider, new ContactTableFinder { UseRawContacts = false });
		}
	}
}