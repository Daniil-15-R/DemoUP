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

            // Загружаем главную страницу вместо страницы авторизации
            ShowMainPage("Guest"); // По умолчанию как гость
        }

        public void ShowAuthPage()
        {
            MainFrame.Navigate(new AuthPage());
        }

        public void ShowMainPage(string userRole)
        {
            MainFrame.Navigate(new MainPage(userRole, this));
        }
    }
}