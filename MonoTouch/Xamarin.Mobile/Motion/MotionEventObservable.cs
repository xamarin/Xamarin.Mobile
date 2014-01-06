//
//  Copyright 2011-2014, Xamarin Inc.
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
using System.Collections.Generic;

namespace Xamarin {
	internal class MotionEventObservable : IObservable<MotionVector> {
		private List<IObserver<MotionVector>> observers = new List<IObserver<MotionVector>>();

		public MotionEventObservable ()
		{
		}

		public IDisposable Subscribe (IObserver<MotionVector> observer)
		{
			observers.Add (observer);

			return new MotionEventDisposable (observer, observers);
		}

		public void OnNext(MotionVector value) 
		{
			Map (o => o.OnNext (value));
		}

		public void OnError(Exception ex) 
		{
			Map (o => o.OnError (ex));
		}

		public void OnCompleted () 
		{
			Map (o => o.OnCompleted ());
		}

		void Map(Action<IObserver<MotionVector>> value) 
		{
			foreach (var o in observers) {
				value (o);
			}
		}

		class MotionEventDisposable : IDisposable {

			IObserver<MotionVector> observer;
			List<IObserver<MotionVector>> observerList;

			public MotionEventDisposable(IObserver<MotionVector> observer, List<IObserver<MotionVector>> observerList) 
			{
				this.observer = observer;
				this.observerList = observerList;
			}

			public void Dispose ()
			{
				observerList.Remove (observer);
			}
		}
	}
}

