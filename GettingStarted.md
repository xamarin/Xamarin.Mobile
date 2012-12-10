Xamarin.Mobile is an API for accessing common platform features, such as
reading the user's address book and using the camera, across iOS,
Android, and Windows Phone.

## Examples

To access the address book:

```csharp
var book = new AddressBook ();
foreach (Contact contact in book.OrderBy (c => c.LastName)) {
	Console.WriteLine ("{0} {1}", contact.FirstName, contact.LastName);
}
```

To get the user's location:

```csharp
var locator = new Geolocator { DesiredAccuracy = 50 };
locator.GetPositionAsync (timeout: 10000).ContinueWith (t => {
	Console.WriteLine ("Position Status: {0}", t.Result.Timestamp);
	Console.WriteLine ("Position Latitude: {0}", t.Result.Latitude);
	Console.WriteLine ("Position Longitude: {0}", t.Result.Longitude);
});
```

To take a photo:

```csharp
var picker = new MediaPicker ();
picker.TakePhotoAsync (new StoreCameraMediaOptions {
	Name = "test.jpg",
	Directory = "MediaPickerSample"
}).ContinueWith (t => {
	if (t.IsCanceled) {
		Console.WriteLine ("User canceled");
		return;
	}
	Console.WriteLine (t.Result.Path);
});
```
