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
using System.Threading;
using System.Runtime.InteropServices;
using Java.Net;
using System.ComponentModel;



namespace lessons
{
	[Activity (Label = "Расписание УГКР", MainLauncher = true, Icon = "@drawable/icon", ScreenOrientation = Android.Content.PM.ScreenOrientation.Nosensor, Theme = "@android:style/Theme.Holo.Light")]
	public class MainActivity : Activity
	{
		static string[] groups;
		static TextView text;
		static Spinner spinner;
		static string output;
		static string curDate;
		static Context context;
		static ViewSwitcher viewSwitcher;
		static System.Threading.Thread LoadThread;

		protected override void OnCreate (Bundle bundle)
		{
			context = this;
			groups = Resources.GetTextArray (Resource.Array.group_codes);

			base.OnCreate (bundle);

			SetContentView (Resource.Layout.Main);
			//RequestWindowFeature (WindowFeatures.NoTitle);
			text = FindViewById<TextView> (Resource.Id.text);
			Button today = FindViewById<Button> (Resource.Id.today);
			Button tomorrow = FindViewById<Button> (Resource.Id.tomorrow);
			Button OnDate = FindViewById<Button> (Resource.Id.toDate);
			spinner = FindViewById<Spinner> (Resource.Id.spinner1);
			viewSwitcher = FindViewById<ViewSwitcher> (Resource.Id.viewSwitcher1);
			DateTime dateToday = DateTime.Today;
			DateTime dateTomorrow = DateTime.Today.AddDays (1);
			DatePicker datePicker = FindViewById<DatePicker> (Resource.Id.datePicker);
			ConnectivityManager cm = (ConnectivityManager)GetSystemService (Context.ConnectivityService);

			spinner.SetSelection (loadingGroup ());


			today.Click += delegate {
				savingGroup (spinner.SelectedItemPosition);
				if (cm.ActiveNetworkInfo != null) {
					StartLoadingThread (dateToday);
				} else {
					text.Text = "Проверьте соединение с интернетом";
					viewSwitcher.ShowNext ();
				}

			};
			tomorrow.Click += delegate {
				savingGroup (spinner.SelectedItemPosition);
				if (cm.ActiveNetworkInfo != null) {
					StartLoadingThread (dateTomorrow);
				} else {
					text.Text = "Проверьте соединение с интернетом";
					viewSwitcher.ShowNext ();
				}

			};
			OnDate.Click += delegate {
				savingGroup (spinner.SelectedItemPosition);
				if (cm.ActiveNetworkInfo != null) {
					StartLoadingThread (datePicker.DateTime);
				} else {
					text.Text = "Проверьте соединение с интернетом";
					viewSwitcher.ShowNext ();
				}

			};
		}

		protected void StartLoadingThread (DateTime date)
		{
			//	spinner = FindViewById<Spinner> (Resource.Id.spinner1);
			string url = "http://study.ugkr.ru/rasp.php?act=1&group=" + groups [spinner.SelectedItemId] + "&date=" + date.Year.ToString () + '-' + date.Month.ToString () + '-' + date.Day.ToString ();
			if (date.ToBinary () == DateTime.Today.ToBinary ())
				curDate = "сегодня";
			else if (date.ToBinary () == DateTime.Today.AddDays (1).ToBinary ())
				curDate = "завтра";
			else
				curDate = "дату " + date.Year.ToString () + '-' + date.Month.ToString () + '-' + date.Day.ToString ();
			ProgressDialog	progressDialog = ProgressDialog.Show (context, "Загрузка...", "Расписание загружается, пожалуйста подождите...", true, true);
			progressDialog.DismissEvent += delegate {
				if (!LoadThread.IsAlive) {
					viewSwitcher.ShowNext ();
					text.Text = output;
				} else
				LoadThread.Abort ();
			};
			PageLoad pageLoad = new PageLoad (url, progressDialog);

		}

		public void savingGroup (int index)
		{
			GetPreferences (FileCreationMode.Private).Edit ().PutInt ("curentGroup", index).Commit ();
		}

		public int loadingGroup ()
		{
			if (GetPreferences (FileCreationMode.Private).Contains ("curentGroup"))
				return	GetPreferences (FileCreationMode.Private).GetInt ("curentGroup", 0);
			else
				return 0;
		}

		public override bool OnKeyUp (Keycode keyCode, KeyEvent e)
		{

			if (keyCode == Keycode.Back) {
				ViewSwitcher viewSwitcher = FindViewById<ViewSwitcher> (Resource.Id.viewSwitcher1);
				if (viewSwitcher.CurrentView != viewSwitcher.GetChildAt (0))
					viewSwitcher.ShowPrevious ();
				return true;
			} else
				return base.OnKeyUp (keyCode, e);
		}

		class PageLoad
		{


			System.Threading.Thread thread;
			ProgressDialog progressDialog;

			public PageLoad (string url, ProgressDialog pd)
			{ //Конструктор, получающий путь к странице с расписанием
				thread = new System.Threading.Thread (this.Page_Load);
				progressDialog = pd;
				LoadThread = thread;
				thread.Start (url);//запускает поток, и передает ему путь 
			}

			protected void Page_Load (object url)
			{
				HttpWebRequest objWebRequest;
				HttpWebResponse objWebResponse;
				StreamReader streamReader;
				 
				objWebRequest = (HttpWebRequest)WebRequest.Create (url.ToString ());
				objWebRequest.Method = "GET";
				objWebResponse = (HttpWebResponse)objWebRequest.GetResponse ();
				streamReader = new StreamReader (objWebResponse.GetResponseStream (), System.Text.Encoding.GetEncoding (1251));
				string strHTML = streamReader.ReadToEnd ();
				System.Text.Encoding.GetEncoding (1251);
				string rasp = Regex.Match (strHTML, "(Расписание учебной группы).*(</td>)").ToString ();
				int count;
				output = "Расписание на " + curDate + ":\n\n"; 
				if (Regex.IsMatch (rasp, "(<span style='color:#0033FF' >).{3,310}(<br>)") == true) {
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
				} else
					output = "Расписание на " + curDate + " отсутствует";
				streamReader.Close ();
				objWebResponse.Close ();
				objWebRequest.Abort ();
				progressDialog.Dismiss ();
			}
		}
	}
}


