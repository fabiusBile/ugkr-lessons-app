﻿using System;

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
using Android.Util;
using System.ComponentModel.Design;
using Android;
using Java.Sql;
using Android.Text;



namespace lessons
{
	[Activity (Label = "Расписание УГКР", MainLauncher = true, Icon = "@drawable/icon", ScreenOrientation = Android.Content.PM.ScreenOrientation.Nosensor, Theme = "@android:style/Theme.Holo.Light")]
	public class MainActivity : Activity
	{
		static string[] groups;//Массив, содержащий список групп
		static string[] teachers;
		static TextView text; //Полный текст, выдаваемый пользователю
		static Spinner spinner;//Выпадающий список для выбора группы
		static string output; //Вывод функции, получающей расписание
		static string curDate; 
		static Context context;
		static ViewSwitcher viewSwitcher; //вью свитчер, отвечающий за главный экран и экран расписания
		static System.Threading.Thread LoadThread; //Поток, отвечающий за загрузку расписания
		public DatePicker datePicker;
		static public bool type=false;// false - для студентов, true - для преподавателей 
		static public ArrayAdapter studentsAdp;
		static public ArrayAdapter teachersAdp;
	

		public override bool OnPrepareOptionsMenu(IMenu menu) {
			if (!menu.HasVisibleItems) {
				MenuInflater.Inflate (Resource.Menu.actionbar, menu);
			}
				return base.OnPrepareOptionsMenu(menu);
		}
		public override bool OnOptionsItemSelected(IMenuItem item)
		{
			if (item.ItemId==Resource.Id.overflow) {
				showPopup (FindViewById<View> (Resource.Id.overflow));
				return true;
			}
			return base.OnOptionsItemSelected (item);
		}
		//public override bool OnMenuItemClick(IMenuItem item)
		public void showPopup(View v){
			PopupMenu popup = new PopupMenu (this, v);
			MenuInflater inflater = popup.MenuInflater;	
			popup.Inflate (Resource.Menu.popup);
			popup.MenuItemClick += (s1,item) => {
				switch(item.Item.ItemId){	
				case Resource.Id.settings:
					Intent i = new Intent(this,typeof(Settings));
						StartActivity(i);
					break;
				case Resource.Id.bells:
					StartActivity(new Intent(this,typeof(bells)));
					break;
				}
			};
			popup.Show ();
		}
		protected override  void OnResume (){
			base.OnResume ();
			UpdateSettings ();
		}
		protected override void OnCreate (Bundle bundle)
		{
			context = this;
			groups = Resources.GetTextArray (Resource.Array.group_codes);
			teachers = Resources.GetTextArray (Resource.Array.teachers_codes);

			string[] groupNames = Resources.GetTextArray (Resource.Array.groups);
			string[] teachersNames = Resources.GetTextArray (Resource.Array.teachers);
			studentsAdp = new ArrayAdapter<string> (this, Android.Resource.Layout.SimpleSpinnerDropDownItem, groupNames);
			teachersAdp  = new ArrayAdapter<string> (this, Android.Resource.Layout.SimpleSpinnerDropDownItem, teachersNames);

			base.OnCreate (bundle);

			SetContentView (Resource.Layout.Main);
			//RequestWindowFeature (WindowFeatures.NoTitle);
			text = FindViewById<TextView> (Resource.Id.text);
			datePicker = FindViewById<DatePicker> (Resource.Id.datePicker);
			Button today = FindViewById<Button> (Resource.Id.today);
			Button tomorrow = FindViewById<Button> (Resource.Id.tomorrow);
			Button OnDate = FindViewById<Button> (Resource.Id.toDate);
			//	Button settings = FindViewById<Button> (Resource.Id.settingsButton)
			spinner = FindViewById<Spinner> (Resource.Id.spinner1);
			viewSwitcher = FindViewById<ViewSwitcher> (Resource.Id.viewSwitcher1);
			DateTime dateToday = DateTime.Today;
			DateTime dateTomorrow = DateTime.Today.AddDays (1);
			ConnectivityManager cm = (ConnectivityManager)GetSystemService (Context.ConnectivityService);

			UpdateSettings ();


			spinner.ItemSelected += delegate {
				savingGroup (spinner.SelectedItemPosition);
			};
			//Обработчики нажатий кнопок
			today.Click += delegate { //На сегодня
				if (cm.ActiveNetworkInfo != null) { //Если присутствует соединение с интернетом - запустить функцию, 
					bool todayType = GetPreferences (FileCreationMode.Private).GetBoolean("todayType",false);
					long todayId = GetPreferences (FileCreationMode.Private).GetLong("todayId",-1);
					if (todayType!=type||todayId!=spinner.SelectedItemId||GetPreferences (FileCreationMode.Private).GetLong("todayDate",(DateTime.Today.ToBinary() -1))!=dateToday.ToBinary()){
						StartLoadingThread (dateToday);// получающую расписание с сайта
					} else{
						text.Text=GetPreferences (FileCreationMode.Private).GetString("ForToday","Расписание на сегодня отсутствует");
						viewSwitcher.ShowNext ();
					}
				} else {
					text.Text = "Проверьте соединение с интернетом";//Иначе, выводит ошибку
					viewSwitcher.ShowNext ();

				}

			};
			tomorrow.Click += delegate { //На завтра
				if (cm.ActiveNetworkInfo != null) {
					StartLoadingThread (dateTomorrow);
				} else {
					text.Text = "Проверьте соединение с интернетом";
					viewSwitcher.ShowNext ();
				}
			};
			OnDate.Click += delegate {//На дату
				if (cm.ActiveNetworkInfo != null) {
					StartLoadingThread (datePicker.DateTime);
				} else {
					text.Text = "Проверьте соединение с интернетом";
					viewSwitcher.ShowNext ();
				}
			};

		}
		//Функция, строящая путь к расписанию, на основании выбора даты и группы

		protected void StartLoadingThread (DateTime date)
		{
			string act = (!type) ? "?act=1&group=" : "?act=4&prep=";
			string t;
			if (type) {
				t = teachers [spinner.SelectedItemId];
			} else{
				t = groups [spinner.SelectedItemId];
			}
			//Построение урл
			string url = "http://study.ugkr.ru/rasp.php" + act + t  + "&date=" + date.Year.ToString () + '-' + date.Month.ToString () + '-' + date.Day.ToString (); 
			if (date.ToBinary () == DateTime.Today.ToBinary ())  //В зависимости от даты, будет написано "расписание на сегодня",
				curDate = "сегодня";							// "расписание на завтра" или "расписание на дату..."
			else if (date.ToBinary () == DateTime.Today.AddDays (1).ToBinary ())
				curDate = "завтра";
			else
				curDate = "дату " + date.Year.ToString () + '-' + date.Month.ToString () + '-' + date.Day.ToString ();
			//Инициализация всплывающеего окна загрузки
			ProgressDialog	progressDialog = ProgressDialog.Show (context, "Загрузка...", "Расписание загружается, пожалуйста подождите...", true, true);
			//Останавливает загрузку расписания, если пользователь закрывает всплывающее окно загрузки
			progressDialog.DismissEvent += delegate {
				if (!LoadThread.IsAlive) {
					viewSwitcher.ShowNext ();
					text.Text = output;
					if (date==DateTime.Today){
						SaveForToday(output);
					}
				} else
				LoadThread.Abort ();
			};
			PageLoad pageLoad = new PageLoad (url, progressDialog,type); //Посылает функции, запускающей поток загрузки расписания
			//путь до страницы с ним и всплывающее окно загрузки

		}
		public void SaveForToday(string data){
				long today = DateTime.Today.ToBinary();
				GetPreferences (FileCreationMode.Private).Edit ().PutString ("ForToday", data).Commit ();
				GetPreferences (FileCreationMode.Private).Edit ().PutLong ("todayDate", today).Commit ();
				GetPreferences (FileCreationMode.Private).Edit ().PutBoolean ("todayType", type).Commit ();
				GetPreferences (FileCreationMode.Private).Edit ().PutLong ("todayId", spinner.SelectedItemId).Commit ();
		}
		public  void UpdateSettings(){
			if (loadPref ("date")) {
				datePicker.SpinnersShown = false;
				datePicker.CalendarViewShown = true;
			}
			else {
				datePicker.SpinnersShown = true;
				datePicker.CalendarViewShown = false;
			}

			if (loadPref ("type")) {
				spinner.Adapter = teachersAdp;
				if (type==loadPref("type"))
					spinner.SetSelection (loadingGroup ());
				type = true;
			} else {
				spinner.Adapter = studentsAdp;
				if (type==loadPref("type"))
					spinner.SetSelection (loadingGroup ());
				type = false;
			}
		}
		public  void savingGroup (int index)
		{
			//Сохраняет выбор группы 
			GetPreferences (FileCreationMode.Private).Edit ().PutInt ("curentGroup", index).Commit ();
		}

		public  int loadingGroup ()
		{
			//Если пользователь ранее выбирал группу - при последуещем открытии приложения, она будет вновь выбранна 
			if (GetPreferences (FileCreationMode.Private).Contains ("curentGroup"))
				return	GetPreferences (FileCreationMode.Private).GetInt ("curentGroup", 0);
			else
				return 0;
		}
		public bool loadPref (string name)
		{
			if (GetSharedPreferences ("Settings",FileCreationMode.Private).Contains (name))
				return	GetSharedPreferences ("Settings",FileCreationMode.Private).GetBoolean (name,false);
			else
				return false;
		}
		public override bool OnKeyUp (Keycode keyCode, KeyEvent e)
		{
			//При нажатии физической кнопки назад, возвращает на главный экран
			if (keyCode == Keycode.Back) {
				ViewSwitcher viewSwitcher = FindViewById<ViewSwitcher> (Resource.Id.viewSwitcher1); 
				ViewSwitcher viewSwitcherMain = FindViewById<ViewSwitcher> (Resource.Id.viewSwitcherMain); 
				if (viewSwitcher.CurrentView != viewSwitcher.GetChildAt (0)) {
					viewSwitcher.ShowPrevious ();

				} else if (viewSwitcherMain.CurrentView != viewSwitcherMain.GetChildAt (0)) {
					viewSwitcherMain.ShowPrevious ();

				}
				return true;
			} else if (keyCode == Keycode.Menu) {
				showPopup (FindViewById<View> (Resource.Id.overflow));
				return true;
			}
			else
				return base.OnKeyUp (keyCode, e);
		}

	
		//Класс, работающий со вторым потоком 
		class PageLoad
		{
			System.Threading.Thread thread;
			ProgressDialog progressDialog;
			bool type;
			public PageLoad (string url, ProgressDialog pd, bool t)
			{ //Конструктор, получающий путь к странице с расписанием 
				thread = new System.Threading.Thread (this.Page_Load); 
				progressDialog = pd;
				LoadThread = thread;
				type=t;
				thread.Start (url);//запускает поток, и передает ему путь 
			}
			//Функция, получающая расписание с сайта
			protected void Page_Load (object url)
			{
				HttpWebRequest objWebRequest;
				HttpWebResponse objWebResponse;
				StreamReader streamReader;
				 
				objWebRequest = (HttpWebRequest)WebRequest.Create (url.ToString ()); //Создает html-запрос
				objWebRequest.Method = "GET";
				objWebResponse = (HttpWebResponse)objWebRequest.GetResponse (); //Отправляет html-запрос
				//Создает поток данных из кода html страницы
				streamReader = new StreamReader (objWebResponse.GetResponseStream (), System.Text.Encoding.GetEncoding (1251)); 
				string strHTML = streamReader.ReadToEnd (); //Читает поток в переменную 
				System.Text.Encoding.GetEncoding (1251);
				string rasp = (!type) ? Regex.Match (strHTML, "(Расписание учебной группы).*(</td>)").ToString () : Regex.Match (strHTML, "(<span class=\"prep_rasp_name\">).*?(</td>)").ToString ();  //Находит кусок с расписанием по регулярному выражению
				int count;
				output = "Расписание на " + curDate + ":\n\n";  
				if (!type) {
					if (Regex.IsMatch (rasp, "(<span style='color:#0033FF' >).*?(<br>)") == true) {  //Если на странице имеется расписание
						count = Regex.Matches (rasp, "(<span style='color:#0033FF' >).*?(<br>)").Count; //Определяет число предметов
						if (count > 0) {
							for (int i = 0; i != count; i++) {
								string temp = Regex.Match (rasp, "(<span style='color:#0033FF' >).*?(<br>)").ToString (); //Находит строку расписания 
								string curEscapes = Regex.Escape (temp);
								rasp = Regex.Replace (rasp, "(" + curEscapes + ")", " ").ToString ();   //Очищает текст от эксейп-последовательностей
								temp = Regex.Replace (temp, "(<.*?>)", "").ToString (); 				//и от html-тегов
								output += temp + "\n\n";												//добавляет перенос строки 																									
							}																			//TODO убрать эти костыли и написать нормальную регулярку
						}
					} else
						output = "Расписание на " + curDate + " отсутствует"; 
				}
				else {
					if (Regex.IsMatch (strHTML, "(<span class=\"prep_rasp_name\">).*?(</td>)"))
						Console.WriteLine ("234234");
					else
						Console.WriteLine (url);
					Console.WriteLine (rasp);
					if (Regex.IsMatch (rasp, "(<span class=prep_rasp_para>).*?(<br>)") == true) {  //Если на странице имеется расписание
						Console.WriteLine ("2314");
						count = Regex.Matches (rasp, "(<span class=prep_rasp_para>).*?(<br>)").Count; //Определяет число предметов
						if (count > 0) {
							for (int i = 0; i != count; i++) {
								string temp = Regex.Match (rasp, "(<span class=prep_rasp_para>).*?(<br>)").ToString (); //Находит строку расписания 
								string curEscapes = Regex.Escape (temp);
								rasp = Regex.Replace (rasp, "(" + curEscapes + ")", " ").ToString ();   //Очищает текст от эксейп-последовательностей
								temp = Regex.Replace (temp, "(<.*?>)", "").ToString (); 				//и от html-тегов
								output += temp + "\n\n";												//добавляет перенос строки 																									
							}																			//TODO убрать эти костыли и написать нормальную регулярку
						}
					} else
						output = "Расписание на " + curDate + " отсутствует"; 
				}
				streamReader.Close ();
				objWebResponse.Close ();
				objWebRequest.Abort ();
				progressDialog.Dismiss (); //закрывает всплывающий загрузочный экран
			}
		}
	}
}


