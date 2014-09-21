
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;

namespace lessons
{
	[Activity (Label = "Расписание звонков", Icon = "@drawable/icon", ScreenOrientation = Android.Content.PM.ScreenOrientation.Nosensor, Theme = "@android:style/Theme.Holo.Light")]			
	public class bells : Activity
	{
		protected override void OnCreate (Bundle bundle)
		{	
			base.OnCreate (bundle);
			SetContentView (Resource.Layout.bells);
			// Create your application here
		}
	}
}

