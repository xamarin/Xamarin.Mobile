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
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Runtime.InteropServices;
using System.Threading;

#if __UNIFIED__
using CoreGraphics;
using AssetsLibrary;
using Foundation;
using ImageIO;
using MobileCoreServices;
using UIKit;
using NSAction = global::System.Action;
#else
using MonoTouch.AssetsLibrary;
using MonoTouch.Foundation;
using MonoTouch.ImageIO;
using MonoTouch.MobileCoreServices;
using MonoTouch.UIKit;

using CGRect = global::System.Drawing.RectangleF;
using nfloat = global::System.Single;
#endif

namespace Xamarin.Media
{
	internal class MediaPickerDelegate
		: UIImagePickerControllerDelegate
	{
		internal MediaPickerDelegate (UIViewController viewController, UIImagePickerControllerSourceType sourceType, StoreCameraMediaOptions options)
		{
			this.viewController = viewController;
			this.source = sourceType;
			this.options = options ?? new StoreCameraMediaOptions();

			if (viewController != null) {
				UIDevice.CurrentDevice.BeginGeneratingDeviceOrientationNotifications();
				this.observer = NSNotificationCenter.DefaultCenter.AddObserver (UIDevice.OrientationDidChangeNotification, DidRotate);
			}
		}
		
		public UIPopoverController Popover
		{
			get;
			set;
		}
		
		public UIView View
		{
			get { return this.viewController.View; }
		}

		public Task<MediaFile> Task
		{
			get { return tcs.Task; }
		}

		public override void FinishedPickingMedia (UIImagePickerController picker, NSDictionary info)
		{
			MediaFile mediaFile;
			switch ((NSString)info[UIImagePickerController.MediaType])
			{
				case MediaPicker.TypeImage:
					mediaFile = GetPictureMediaFile (info);
					break;

				case MediaPicker.TypeMovie:
					mediaFile = GetMovieMediaFile (info);
					break;

				default:
					throw new NotSupportedException();
			}

            if (UIDevice.CurrentDevice.UserInterfaceIdiom == UIUserInterfaceIdiom.Phone)
            {
                UIApplication.SharedApplication.SetStatusBarStyle(MediaPicker.StatusBarStyle, false);
            }

			Dismiss (picker, () => this.tcs.TrySetResult (mediaFile));
		}

		public override void Canceled (UIImagePickerController picker)
		{
            if (UIDevice.CurrentDevice.UserInterfaceIdiom == UIUserInterfaceIdiom.Phone)
            {
                UIApplication.SharedApplication.SetStatusBarStyle(MediaPicker.StatusBarStyle, false);
            }

			Dismiss (picker, () => this.tcs.TrySetCanceled());
		}

		public void DisplayPopover (bool hideFirst = false)
		{
			if (Popover == null)
				return;

			var swidth = UIScreen.MainScreen.Bounds.Width;
			var sheight= UIScreen.MainScreen.Bounds.Height;

			nfloat width = 400;
			nfloat height = 300;

			if (this.orientation == null)
			{
				if (IsValidInterfaceOrientation (UIDevice.CurrentDevice.Orientation))
					this.orientation = UIDevice.CurrentDevice.Orientation;
				else
					this.orientation = GetDeviceOrientation (this.viewController.InterfaceOrientation);
			}

			nfloat x, y;
			if (this.orientation == UIDeviceOrientation.LandscapeLeft || this.orientation == UIDeviceOrientation.LandscapeRight)
			{
				y = (swidth / 2) - (height / 2);
				x = (sheight / 2) - (width / 2);
			}
			else
			{
				x = (swidth / 2) - (width / 2);
				y = (sheight / 2) - (height / 2);
			}

			if (hideFirst && Popover.PopoverVisible)
				Popover.Dismiss (animated: false);

			Popover.PresentFromRect (new CGRect (x, y, width, height), View, 0, animated: true);
		}

		private UIDeviceOrientation? orientation;
		private NSObject observer;
		private readonly UIViewController viewController;
		private readonly UIImagePickerControllerSourceType source;
		private TaskCompletionSource<MediaFile> tcs = new TaskCompletionSource<MediaFile>();
		private readonly StoreCameraMediaOptions options;

		private bool IsCaptured
		{
			get { return this.source == UIImagePickerControllerSourceType.Camera; }
		}
		
		private void Dismiss (UIImagePickerController picker, NSAction onDismiss)
		{
            if (this.viewController == null) {
                onDismiss();
                tcs = new TaskCompletionSource<MediaFile>();
            }
			else {
				NSNotificationCenter.DefaultCenter.RemoveObserver (this.observer);
				UIDevice.CurrentDevice.EndGeneratingDeviceOrientationNotifications();

				this.observer.Dispose();

				if (Popover != null) {
					Popover.Dismiss (animated: true);
					Popover.Dispose();
					Popover = null;

					onDismiss();
				} else {
					picker.DismissViewController (true, onDismiss);
					picker.Dispose();
				}
			}
		}

		private void DidRotate (NSNotification notice)
		{
			UIDevice device = (UIDevice)notice.Object;
			if (!IsValidInterfaceOrientation (device.Orientation) || Popover == null)
				return;
			if (this.orientation.HasValue && IsSameOrientationKind (this.orientation.Value, device.Orientation))
				return;

			if (UIDevice.CurrentDevice.CheckSystemVersion (6, 0))
			{
				if (!GetShouldRotate6 (device.Orientation))
					return;
			}
			else if (!GetShouldRotate (device.Orientation))
				return;

			UIDeviceOrientation? co = this.orientation;
			this.orientation = device.Orientation;

			if (co == null)
				return;

			DisplayPopover (hideFirst: true);
		}

		private bool GetShouldRotate (UIDeviceOrientation orientation)
		{
			UIInterfaceOrientation iorientation = UIInterfaceOrientation.Portrait;
			switch (orientation)
			{
				case UIDeviceOrientation.LandscapeLeft:
					iorientation = UIInterfaceOrientation.LandscapeLeft;
					break;
					
				case UIDeviceOrientation.LandscapeRight:
					iorientation = UIInterfaceOrientation.LandscapeRight;
					break;
					
				case UIDeviceOrientation.Portrait:
					iorientation = UIInterfaceOrientation.Portrait;
					break;
					
				case UIDeviceOrientation.PortraitUpsideDown:
					iorientation = UIInterfaceOrientation.PortraitUpsideDown;
					break;
					
				default: return false;
			}

			return this.viewController.ShouldAutorotateToInterfaceOrientation (iorientation);
		}

		private bool GetShouldRotate6 (UIDeviceOrientation orientation)
		{
			if (!this.viewController.ShouldAutorotate())
				return false;

			UIInterfaceOrientationMask mask = UIInterfaceOrientationMask.Portrait;
			switch (orientation)
			{
				case UIDeviceOrientation.LandscapeLeft:
					mask = UIInterfaceOrientationMask.LandscapeLeft;
					break;
					
				case UIDeviceOrientation.LandscapeRight:
					mask = UIInterfaceOrientationMask.LandscapeRight;
					break;
					
				case UIDeviceOrientation.Portrait:
					mask = UIInterfaceOrientationMask.Portrait;
					break;
					
				case UIDeviceOrientation.PortraitUpsideDown:
					mask = UIInterfaceOrientationMask.PortraitUpsideDown;
					break;
					
				default: return false; 
			}

			return this.viewController.GetSupportedInterfaceOrientations().HasFlag (mask);
		}

		private MediaFile GetPictureMediaFile (NSDictionary info)
		{
			string path = GetOutputPath (MediaPicker.TypeImage,
				              options.Directory ?? ((IsCaptured) ? String.Empty : "temp"),
				              options.Name);

			var image = (UIImage)info[UIImagePickerController.EditedImage];
			NSUrl referenceUrl = (NSUrl)info[UIImagePickerController.ReferenceUrl];

			// if the image is coming directly from the camera
			if (image == null && referenceUrl == null)
			{
				image = (UIImage)info[UIImagePickerController.OriginalImage];
			}

			// if this is an EditedImage or direct camera image
			if (image != null)
			{
				NSDictionary metadata = (NSDictionary)info[UIImagePickerController.MediaMetadata];
				NSMutableDictionary mutableMetadata = new NSMutableDictionary(metadata);

				mutableMetadata.SetValueForKey(new NSNumber(options.JpegCompressionQuality), new NSString("kCGImageDestinationLossyCompressionQuality"));

				NSMutableData destData = new NSMutableData();
				#if __UNIFIED__
				CGImageDestination destination = CGImageDestination.Create(destData, UTType.JPEG, 1);
				#else
				CGImageDestination destination = CGImageDestination.FromData(destData, UTType.JPEG, 1);
				#endif
				destination.AddImage(image.CGImage, mutableMetadata);

				destination.Close();
				NSError saveError = null;
				destData.Save(path, NSDataWritingOptions.Atomic, out saveError);

				if (saveError != null)
				{
					throw new InvalidOperationException("Failed to save image: " + saveError.Description);
				}
			}
			else
			{
				// image is coming from the camera roll
				// use the original image file data without re-encoding it
				// unlike the direct camera image above, this one will also
				// include GPS EXIF data if photo was taken by the Camera app
				SaveOriginalImage(referenceUrl, path);
			}

			Action<bool> dispose = null;
			if (this.source != UIImagePickerControllerSourceType.Camera)
				dispose = d => File.Delete (path);

			return new MediaFile (path, () => File.OpenRead (path), dispose);
		}

		private static string SaveOriginalImage(NSUrl referenceUrl, string path)
		{
			ALAssetsLibrary library = new ALAssetsLibrary();
			var doneSignal = new ManualResetEvent(false);
			byte[] imageData = null;
			NSError assetError = null;

			// even though AssetForUrl is async, the callbacks aren't called
			// unless we use another thread due to blocking from our WaitOne below.
			System.Threading.Tasks.Task.Run(() =>  {
				library.AssetForUrl(referenceUrl, asset =>  {
					try
					{
						long size = asset.DefaultRepresentation.Size;
						imageData = new byte[size];
						IntPtr buffer = Marshal.AllocHGlobal(imageData.Length);
						NSError bytesError;

						asset.DefaultRepresentation.GetBytes(buffer, 0, (uint)size, out bytesError);

						if (bytesError != null)
						{
							assetError = bytesError;
							return;
						}

						Marshal.Copy(buffer, imageData, 0, imageData.Length);
					}
					finally
					{
						asset.Dispose();
						doneSignal.Set();
					}
				}, error =>  {
					assetError = error;
					doneSignal.Set();
				});
			});

			if (!doneSignal.WaitOne(TimeSpan.FromSeconds(10)))
			{
				throw new TimeoutException("Timed out getting asset");
			}

			library.Dispose();

			if (assetError != null || imageData == null)
			{
				throw new InvalidOperationException("Failed to get asset for URL");
			}

			using (FileStream fs = File.OpenWrite(path))
			{
				fs.Write(imageData, 0, imageData.Length);
				fs.Flush();
			}

			return path;
		}
		
		private MediaFile GetMovieMediaFile (NSDictionary info)
		{
			NSUrl url = (NSUrl)info[UIImagePickerController.MediaURL];

			string path = GetOutputPath (MediaPicker.TypeMovie,
				options.Directory ?? ((IsCaptured) ? String.Empty : "temp"),
				this.options.Name ?? Path.GetFileName (url.Path));

			File.Move (url.Path, path);

			Action<bool> dispose = null;
			if (this.source != UIImagePickerControllerSourceType.Camera)
				dispose = d => File.Delete (path);

			return new MediaFile (path, () => File.OpenRead (path), dispose);
		}
		
		private static string GetUniquePath (string type, string path, string name)
		{
			bool isPhoto = (type == MediaPicker.TypeImage);
			string ext = Path.GetExtension (name);
			if (ext == String.Empty)
				ext = ((isPhoto) ? ".jpg" : ".mp4");

			name = Path.GetFileNameWithoutExtension (name);

			string nname = name + ext;
			int i = 1;
			while (File.Exists (Path.Combine (path, nname)))
				nname = name + "_" + (i++) + ext;

			return Path.Combine (path, nname);
		}

		private static string GetOutputPath (string type, string path, string name)
		{
			path = Path.Combine (Environment.GetFolderPath (Environment.SpecialFolder.Personal), path);
			Directory.CreateDirectory (path);

			if (String.IsNullOrWhiteSpace (name))
			{
				string timestamp = DateTime.Now.ToString ("yyyMMdd_HHmmss");
				if (type == MediaPicker.TypeImage)
					name = "IMG_" + timestamp + ".jpg";
				else
					name = "VID_" + timestamp + ".mp4";
			}

			return Path.Combine (path, GetUniquePath (type, path, name));
		}
		
		private static bool IsValidInterfaceOrientation (UIDeviceOrientation self)
		{
			return (self != UIDeviceOrientation.FaceUp && self != UIDeviceOrientation.FaceDown && self != UIDeviceOrientation.Unknown);
		}
		
		private static bool IsSameOrientationKind (UIDeviceOrientation o1, UIDeviceOrientation o2)
		{
			if (o1 == UIDeviceOrientation.FaceDown || o1 == UIDeviceOrientation.FaceUp)
				return (o2 == UIDeviceOrientation.FaceDown || o2 == UIDeviceOrientation.FaceUp);
			if (o1 == UIDeviceOrientation.LandscapeLeft || o1 == UIDeviceOrientation.LandscapeRight)
				return (o2 == UIDeviceOrientation.LandscapeLeft || o2 == UIDeviceOrientation.LandscapeRight);
			if (o1 == UIDeviceOrientation.Portrait || o1 == UIDeviceOrientation.PortraitUpsideDown)
				return (o2 == UIDeviceOrientation.Portrait || o2 == UIDeviceOrientation.PortraitUpsideDown);
			
			return false;
		}
		
		private static UIDeviceOrientation GetDeviceOrientation (UIInterfaceOrientation self)
		{
			switch (self)
			{
				case UIInterfaceOrientation.LandscapeLeft:
					return UIDeviceOrientation.LandscapeLeft;
				case UIInterfaceOrientation.LandscapeRight:
					return UIDeviceOrientation.LandscapeRight;
				case UIInterfaceOrientation.Portrait:
					return UIDeviceOrientation.Portrait;
				case UIInterfaceOrientation.PortraitUpsideDown:
					return UIDeviceOrientation.PortraitUpsideDown;
				default:
					throw new InvalidOperationException();
			}
		}
	}
}
