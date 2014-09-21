
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
using System.Runtime.Serialization.Formatters;
using lessons;
using Android.Util;

namespace lessons
{
	[Activity (Label = "Настройки", Icon = "@drawable/icon", ScreenOrientation = Android.Content.PM.ScreenOrientation.Nosensor, Theme = "@android:style/Theme.Holo.Light")]
	public class Settings : Activity
	{
		protected override void OnCreate (Bundle bundle)
		{
			base.OnCreate (bundle);
			SetContentView (Resource.Layout.Settings);
			// Create your application here
			RadioGroup type = FindViewById<RadioGroup> (Resource.Id.type);
			RadioGroup date = FindViewById<RadioGroup> (Resource.Id.dateSelectType);
			//Intent Main = new Intent (this, typeof(MainActivity));
			if (loadPref ("date")) 
				date.Check (Resource.Id.calendar);
			 else
				date.Check (Resource.Id.picker);
			if (loadPref ("type")) 
				type.Check (Resource.Id.teachers);
			else
				type.Check (Resource.Id.students);
			date.CheckedChange += delegate {
				if (date.CheckedRadioButtonId==Resource.Id.calendar){
					savePref(true,"date");
				}
				else{
					savePref(false,"date");
				}
			};
			type.CheckedChange += delegate {
				if (type.CheckedRadioButtonId==Resource.Id.students){
					savePref(false,"type");
				}
				else{
					savePref(true,"type");
				}
			};
		}
		public void savePref (bool val,string name)
		{
			GetSharedPreferences ("Settings",FileCreationMode.Private).Edit ().PutBoolean (name, val).Commit ();
		}

		public bool loadPref (string name)
		{
			if (GetSharedPreferences ("Settings",FileCreationMode.Private).Contains (name))
				return	GetSharedPreferences ("Settings",FileCreationMode.Private).GetBoolean (name,false);
			else
				return false;
		}
	}
}

