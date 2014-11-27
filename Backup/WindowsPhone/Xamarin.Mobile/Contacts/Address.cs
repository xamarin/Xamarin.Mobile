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

namespace Xamarin.Contacts
{
	public enum AddressType
	{
		Work,
		Home,
		Other
	}

	public class Address
	{
		public AddressType Type
		{
			get;
			set;
		}

		public string Label
		{
			get;
			set;
		}

		public string StreetAddress
		{
			get;
			set;
		}

		public string City
		{
			get;
			set;
		}

		public string Region
		{
			get;
			set;
		}

		public string Country
		{
			get;
			set;
		}

		public string PostalCode
		{
			get;
			set;
		}
	}
}