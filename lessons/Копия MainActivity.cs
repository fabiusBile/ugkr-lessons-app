using System;

using Android.App;
using Android.Content;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Android.OS;
using System.IO;
using System.Net;
using System.Text.RegularExpressions;
using Android.Net;
using Java.Lang;



namespace lessons
{
	[Activity (Label = "Расписание УГКР", MainLauncher = true, Icon = "@drawable/icon", ScreenOrientation=Android.Content.PM.ScreenOrientation.Nosensor, Theme="@android:style/Theme.Holo.Light")]
	public class MainActivity : Activity
	{
		protected override void OnCreate (Bundle bundle)
		{

	

			base.OnCreate (bundle);

			SetContentView (Resource.Layout.Main);
			//RequestWindowFeature (WindowFeatures.NoTitle);
			TextView text = FindViewById<TextView> (Resource.Id.text);
			Button today = FindViewById<Button> (Resource.Id.today);
			Button tomorrow = FindViewById<Button> (Resource.Id.tomorrow);
			Button OnDate = FindViewById<Button> (Resource.Id.toDate);
			Spinner spinner = FindViewById<Spinner> (Resource.Id.spinner1);
			ViewSwitcher viewSwitcher = FindViewById<ViewSwitcher> (Resource.Id.viewSwitcher1);
			DateTime dateToday = DateTime.Today;
			DateTime dateTomorrow = DateTime.Today.AddDays(1);
			DatePicker datePicker = FindViewById<DatePicker> (Resource.Id.datePicker);
			ConnectivityManager cm = (ConnectivityManager)GetSystemService (Context.ConnectivityService);

			spinner.SetSelection(loadingGroup());

			today.Click += delegate {
				savingGroup(spinner.SelectedItemPosition);
				if (cm.ActiveNetworkInfo!=null)
				Page_Load(dateToday);
				else 
					text.Text="Проверьте соединение с интернетом";
				viewSwitcher.ShowNext();

			};
			tomorrow.Click += delegate {
				savingGroup(spinner.SelectedItemPosition);
				if (cm.ActiveNetworkInfo!=null)
				Page_Load(dateTomorrow);
				else 
					text.Text="Проверьте соединение с интернетом";
				viewSwitcher.ShowNext ();

			};
			OnDate.Click += delegate {
				savingGroup(spinner.SelectedItemPosition);
				if (cm.ActiveNetworkInfo!=null)
				Page_Load(datePicker.DateTime);
				else 
					text.Text="Проверьте соединение с интернетом";
				viewSwitcher.ShowNext ();
			};
		}
		public void savingGroup(int index){
			GetPreferences (FileCreationMode.Private).Edit().PutInt("curentGroup",index).Commit();
		}
		public int loadingGroup(){
			if (GetPreferences (FileCreationMode.Private).Contains ("curentGroup"))
				return	GetPreferences (FileCreationMode.Private).GetInt ("curentGroup", 0);
			else
				return 0;
		}
		public override bool OnKeyUp (Keycode keyCode, KeyEvent e)
		{

			if (keyCode == Keycode.Back) {
				ViewSwitcher viewSwitcher = FindViewById<ViewSwitcher> (Resource.Id.viewSwitcher1);
				if (viewSwitcher.CurrentView!=viewSwitcher.GetChildAt(0))
					viewSwitcher.ShowPrevious();
				return true;
			}
			else return base.OnKeyUp (keyCode, e);
		}
		protected void Page_Load(DateTime date)
		{
			string[] groups = Resources.GetTextArray (Resource.Array.group_codes);
			Spinner spinner = FindViewById<Spinner> (Resource.Id.spinner1);
			TextView text = FindViewById<TextView> (Resource.Id.text);
			HttpWebRequest objWebRequest;
			HttpWebResponse objWebResponse;
			StreamReader streamReader;
			string url ="http://study.ugkr.ru/rasp.php?act=1&group="+groups [spinner.SelectedItemId]+"&date="+date.Year.ToString()+'-'+date.Month.ToString()+'-'+date.Day.ToString(); 
			objWebRequest = (HttpWebRequest)WebRequest.Create(url);
			objWebRequest.Method = "GET";
			objWebResponse = (HttpWebResponse)objWebRequest.GetResponse();
			streamReader = new StreamReader(objWebResponse.GetResponseStream(),System.Text.Encoding.GetEncoding(1251));
			string strHTML = streamReader.ReadToEnd();
			System.Text.Encoding.GetEncoding(1251);
			string rasp = Regex.Match(strHTML,"(Расписание учебной группы).*(</td>)").ToString();
			int count;
			string curDate; 
			if (date.ToBinary()==DateTime.Today.ToBinary())
				curDate = "сегодня";
			else if (date.ToBinary()==DateTime.Today.AddDays(1).ToBinary())
				curDate = "завтра";
			else 
				curDate = "дату " + date.Year.ToString () + '-' + date.Month.ToString () + '-' + date.Day.ToString ();
			string output="Расписание на "+curDate+":\n\n"; 
			if (Regex.IsMatch (rasp, "(<span style='color:#0033FF' >).{3,310}(<br>)")==true) {
				count = Regex.Matches (rasp, "(<span style='color:#0033FF' >).*?(<br>)").Count;
				if (count > 0) {
					for (int i = 0; i != count; i++) {
						string temp = Regex.Match (rasp, "(<span style='color:#0033FF' >).*?(<br>)").ToString ();

						string curEscapes = Regex.Escape (temp);
						rasp = Regex.Replace (rasp, "(" + curEscapes + ")", " ").ToString ();
						temp = Regex.Replace (temp, "(<.*?>)", "").ToString ();
						output += temp + "\n\n";
					}
				}
			}
			else
				output="Расписание на "+curDate+" отсутствует";
			streamReader.Close();
			objWebResponse.Close();
			objWebRequest.Abort();
			text.Text = output;
		}
	}
}


