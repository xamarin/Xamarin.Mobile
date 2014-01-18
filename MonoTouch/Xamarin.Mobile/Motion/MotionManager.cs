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
using MonoTouch.CoreMotion;
using MonoTouch.Foundation;

namespace Xamarin {
	public class MotionManager {

		static CMMotionManager motionManager = new CMMotionManager ();

		public event EventHandler<MotionVector> OnMotion;
		public event EventHandler<Exception> OnError;

		public MotionManager ()
		{
		}

		public void Start ()
		{
			motionManager.StartAccelerometerUpdates (NSOperationQueue.CurrentQueue, (data, error) => {
				if (error != null) {
					var ex = new NSErrorException (error);
					var errorHandler = OnError;
					if (errorHandler != null) {
						errorHandler(this, ex);
					}
				} else {
					var vector = new MotionVector () { 
						X = data.Acceleration.X,
						Y = data.Acceleration.Y,
						Z = data.Acceleration.Z
					};

					var motionHandler = OnMotion;
					if (motionHandler != null) {
						motionHandler(this, vector);
					}
				}
			});
		}
	}
}

