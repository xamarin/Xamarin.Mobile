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
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace Xamarin.Mobile.Tests
{
	class MockQueryProvider
		: IQueryProvider
	{
		public Expression LastExpression
		{
			get;
			private set;
		}

		public IQueryable CreateQuery (Expression expression)
		{
			throw new NotImplementedException();
		}

		public object Execute (Expression expression)
		{
			LastExpression = expression;
			return null;
		}

		public IQueryable<TElement> CreateQuery<TElement> (Expression expression)
		{
			return new Query<TElement> (this, expression);
		}

		public TResult Execute<TResult> (Expression expression)
		{
			((IQueryProvider) this).Execute (expression);
			return default (TResult);
		}
	}

	class MockQueryable<T>
		: IQueryable<T>
	{
		public Type ElementType
		{
			get { return typeof (T); }
		}

		public Expression Expression
		{
			get { return Expression.Constant (this); }
		}

		public IQueryProvider Provider
		{
			get { return this.provider; }
		}

		public Expression LastExpression
		{
			get { return this.provider.LastExpression; }
		}

		public IEnumerator<T> GetEnumerator()
		{
			return Enumerable.Empty<T>().GetEnumerator();
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}

		private readonly MockQueryProvider provider = new MockQueryProvider();
	}
}