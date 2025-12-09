// AuthPage.xaml.cs
using System;
using System.Data.Entity;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace DemoUP_.Pages
{
    public partial class AuthPage : Page
    {
        private Entities _context;

        // Временный класс для хранения данных пользователя
        public class UserData
        {
            public int Id { get; set; }
            public string Login { get; set; }
            public string RoleName { get; set; }
            public string FIO { get; set; }
            public int RoleId { get; set; }
            public int FIOId { get; set; }
        }

        // Статическое свойство для хранения текущего пользователя
        public static UserData CurrentUser { get; set; }

        public AuthPage()
        {
            InitializeComponent();
            _context = Entities.GetContext();

            LoginTextBox.TextChanged += LoginTextBox_TextChanged;
            PasswordBox.PasswordChanged += PasswordBox_PasswordChanged;
        }

        private void LoginButton_Click(object sender, RoutedEventArgs e)
        {
            string login = LoginTextBox.Text.Trim();
            string password = PasswordBox.Password;

            if (string.IsNullOrEmpty(login) || string.IsNullOrEmpty(password))
            {
                ShowError("Пожалуйста, заполните все поля");
                return;
            }

            try
            {
                // Проверяем подключение к БД
                if (!_context.Database.Exists())
                {
                    ShowError("Ошибка подключения к базе данных");
                    return;
                }

                var user = AuthenticateUser(login, password);

                if (user != null)
                {
                    HideError();

                    // Сохраняем информацию о пользователе
                    CurrentUser = user;

                    if (Window.GetWindow(this) is MainWindow mainWindow)
                    {
                        // Передаем и роль, и ФИО в MainWindow
                        mainWindow.ShowMainPage(user.RoleName, user.FIO);
                    }
                }
                else
                {
                    ShowError("Неверный логин или пароль");
                }
            }
            catch (Exception ex)
            {
                ShowError($"Ошибка авторизации: {ex.Message}");
            }
        }

        private void GuestButton_Click(object sender, RoutedEventArgs e)
        {
            HideError();

            // Создаем гостевого пользователя - используем русское название роли
            CurrentUser = new UserData { RoleName = "Гость", FIO = "Гость" };

            if (Window.GetWindow(this) is MainWindow mainWindow)
            {
                // Для гостя передаем роль "Гость"
                mainWindow.ShowMainPage("Гость", "Гость");
            }
        }

        private UserData AuthenticateUser(string login, string password)
        {
            // Ищем пользователя в базе данных с включением связанных данных
            var user = _context.Users
                .Include(u => u.Role)
                .Include(u => u.FIO1)
                .FirstOrDefault(u => u.Login == login && u.Password == password);

            if (user != null)
            {
                // Получаем ФИО из связанной таблицы FIO
                // Предполагается, что в таблице FIO есть свойство FIO1 (столбец FIO)
                string fio = "Не указано";
                if (user.FIO1 != null)
                {
                    // Если в классе FIO свойство называется FIO1 (как в Users)
                    fio = user.FIO1.FIO1 ?? "Не указано";

                    // ИЛИ если в классе FIO есть отдельное свойство для ФИО
                    // fio = user.FIO1.FullName ?? "Не указано";
                }

                return new UserData
                {
                    Id = user.ID,
                    Login = user.Login,
                    RoleName = user.Role?.Role_users ?? "User",
                    FIO = fio, // Используем полученное ФИО
                    RoleId = user.Role_users ?? 0,
                    FIOId = user.FIO ?? 0
                };
            }

            return null;
        }

        private void ShowError(string message)
        {
            ErrorTextBlock.Text = message;
            ErrorTextBlock.Visibility = Visibility.Visible;
        }

        private void HideError()
        {
            ErrorTextBlock.Visibility = Visibility.Collapsed;
        }

        private void LoginTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            HideError();
        }

        private void PasswordBox_PasswordChanged(object sender, RoutedEventArgs e)
        {
            HideError();
        }
    }
}