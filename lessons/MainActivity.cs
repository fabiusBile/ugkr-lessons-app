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
using Android.Util;
using System.ComponentModel.Design;



namespace lessons
{
	[Activity (Label = "Расписание УГКР", MainLauncher = true, Icon = "@drawable/icon", ScreenOrientation = Android.Content.PM.ScreenOrientation.Nosensor, Theme = "@android:style/Theme.Holo.Light")]
	public class MainActivity : Activity
	{
		static string[] groups;//Массив, содержащий список групп
		static TextView text; //Полный текст, выдаваемый пользователю
		static Spinner spinner;//Выпадающий список для выбора группы
		static string output; //Вывод функции, получающей расписание
		static string curDate; 
		static Context context;
		static ViewSwitcher viewSwitcher; //вью свитчер, отвечающий за главный экран и экран расписания
		static System.Threading.Thread LoadThread; //Поток, отвечающий за загрузку расписания
		static ViewSwitcher mainViewSwitcher;

		public override bool OnPrepareOptionsMenu(IMenu menu) {
			MenuInflater.Inflate(Resource.Menu.actionbar, menu);
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
			popup.Show ();
		}
		protected override void OnCreate (Bundle bundle)
		{
			context = this;
			groups = Resources.GetTextArray (Resource.Array.group_codes);

			base.OnCreate (bundle);

			SetContentView (Resource.Layout.Main);
			//RequestWindowFeature (WindowFeatures.NoTitle);
			text = FindViewById<TextView> (Resource.Id.text);
			DatePicker datePicker = FindViewById<DatePicker> (Resource.Id.datePicker);
			Button today = FindViewById<Button> (Resource.Id.today);
			Button tomorrow = FindViewById<Button> (Resource.Id.tomorrow);
			Button OnDate = FindViewById<Button> (Resource.Id.toDate);
			//	Button settings = FindViewById<Button> (Resource.Id.settingsButton);
			mainViewSwitcher = FindViewById<ViewSwitcher> (Resource.Id.viewSwitcherMain);
			spinner = FindViewById<Spinner> (Resource.Id.spinner1);
			viewSwitcher = FindViewById<ViewSwitcher> (Resource.Id.viewSwitcher1);
			DateTime dateToday = DateTime.Today;
			DateTime dateTomorrow = DateTime.Today.AddDays (1);
			ConnectivityManager cm = (ConnectivityManager)GetSystemService (Context.ConnectivityService);

			spinner.SetSelection (loadingGroup ());

		
			//Обработчики нажатий кнопок
//			settings.Click += delegate { //Открытие экрана настроек			
//				mainViewSwitcher.ShowNext();
//			};
			today.Click += delegate { //На сегодня
				savingGroup (spinner.SelectedItemPosition);
				if (cm.ActiveNetworkInfo != null) { //Если присутствует соединение с интернетом - запустить функцию, 
					StartLoadingThread (dateToday);// получающую расписание с сайта
				} else {
					text.Text = "Проверьте соединение с интернетом";//Иначе, выводит ошибку
					viewSwitcher.ShowNext ();

				}

			};
			tomorrow.Click += delegate { //На завтра
				savingGroup (spinner.SelectedItemPosition);
				if (cm.ActiveNetworkInfo != null) {
					StartLoadingThread (dateTomorrow);
				} else {
					text.Text = "Проверьте соединение с интернетом";
					viewSwitcher.ShowNext ();

				}
				
			};
			OnDate.Click += delegate {//На дату
				savingGroup (spinner.SelectedItemPosition);
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
			//Построение урл
			string url = "http://study.ugkr.ru/rasp.php?act=1&group=" + groups [spinner.SelectedItemId] + "&date=" + date.Year.ToString () + '-' + date.Month.ToString () + '-' + date.Day.ToString (); 
			if (date.ToBinary () == DateTime.Today.ToBinary ()) //В зависимости от даты, будет написано "расписание на сегодня",
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
				} else
				LoadThread.Abort ();
			};
			PageLoad pageLoad = new PageLoad (url, progressDialog); //Посылает функции, запускающей поток загрузки расписания
			//путь до страницы с ним и всплывающее окно загрузки

		}

		public void savingGroup (int index)
		{
			//Сохраняет выбор группы 
			GetPreferences (FileCreationMode.Private).Edit ().PutInt ("curentGroup", index).Commit ();
		}

		public int loadingGroup ()
		{
			//Если пользователь ранее выбирал группу - при последуещем открытии приложения, она будет вновь выбранна 
			if (GetPreferences (FileCreationMode.Private).Contains ("curentGroup"))
				return	GetPreferences (FileCreationMode.Private).GetInt ("curentGroup", 0);
			else
				return 0;
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
			} else
				return base.OnKeyUp (keyCode, e);
		}

	
		//Класс, работающий со вторым потоком 
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
				string rasp = Regex.Match (strHTML, "(Расписание учебной группы).*(</td>)").ToString (); //Находит кусок с расписанием по регулярному выражению
				int count;
				output = "Расписание на " + curDate + ":\n\n";  
				if (Regex.IsMatch (rasp, "(<span style='color:#0033FF' >).{3,310}(<br>)") == true) {  //Если на странице имеется расписание
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
				streamReader.Close ();
				objWebResponse.Close ();
				objWebRequest.Abort ();
				progressDialog.Dismiss (); //закрывает всплывающий загрузочный экран
			}
		}
	}
}


