using DemoUP_.Pages;
using System;
using System.Windows;

namespace DemoUP_
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            var resourceDict = new ResourceDictionary();
            resourceDict.Source = new Uri("Dictionary.xaml", UriKind.Relative);
            Application.Current.Resources.MergedDictionaries.Add(resourceDict);

            InitializeComponent();
            ShowAuthPage();
        }

        public void ShowAuthPage()
        {
            MainFrame.Navigate(new AuthPage());
        }

        // Изменяем метод для приема ФИО пользователя
        public void ShowMainPage(string userRole, string userName = null)
        {
            // Для гостя передаем "Гость" как имя
            if (string.IsNullOrEmpty(userName))
            {
                userName = userRole == "Guest" ? "Гость" : "Пользователь";
            }

            MainFrame.Navigate(new MainPage(userRole, userName, this));
        }
    }
}