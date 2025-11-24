using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;

namespace DemoUP_.Pages
{
    public partial class MainPage : Page
    {
        private string _currentUserRole;
        private DispatcherTimer _timer;
        private MainWindow _mainWindow;

        public MainPage(string userRole, MainWindow mainWindow)
        {
            InitializeComponent();
            _currentUserRole = userRole;
            _mainWindow = mainWindow;
            InitializeUI();
            SetupNavigation();
            StartClock();
        }

        private void InitializeUI()
        {
            // Устанавливаем информацию о пользователе
            string roleDisplay;
            switch (_currentUserRole)
            {
                case "Администратор":
                    roleDisplay = "Администратор";
                    break;
                case "Менеджер":
                    roleDisplay = "Менеджер";
                    break;
                case "Авторизированныйклиент":
                    roleDisplay = "Клиент";
                    break;
                default:
                    roleDisplay = "Гость";
                    break;
            }

            UserInfoText.Text = $"Роль: {roleDisplay}";

            // Настраиваем видимость элементов в зависимости от роли
            SetupRoleBasedAccess();
        }

        private void SetupRoleBasedAccess()
        {
            // Все роли видят просмотр товаров
            ProductsViewButton.Visibility = Visibility.Visible;

            // Менеджер и администратор видят просмотр заказов
            if (_currentUserRole == "Менеджер" || _currentUserRole == "Администратор")
            {
                OrdersViewButton.Visibility = Visibility.Visible;
            }

            // Только администратор видит управление
            if (_currentUserRole == "Администратор")
            {
                ProductsManageButton.Visibility = Visibility.Visible;
                OrdersManageButton.Visibility = Visibility.Visible;
            }
        }

        private void SetupNavigation()
        {
            NavigateToProductsView();
        }

        private void NavigateToProductsView()
        {
            CurrentPageTitle.Text = "Просмотр товаров";
            SetActiveButton(ProductsViewButton);
            UpdateStatus("Режим просмотра товаров");
            ContentFrame.Navigate(new ProductsPage(_currentUserRole));
        }

        private void NavigateToOrdersView()
        {
            CurrentPageTitle.Text = "Просмотр заказов";
            SetActiveButton(OrdersViewButton);
            UpdateStatus("Режим просмотра заказов");

            ContentFrame.Navigate(new OrderPage(_currentUserRole));
        }

        private void NavigateToProductsManagement()
        {
            CurrentPageTitle.Text = "Управление товарами";
            SetActiveButton(ProductsManageButton);
            UpdateStatus("Режим управления товарами");

            ContentFrame.Navigate(new ProductsManagePage());
        }

        private void NavigateToOrdersManagement()
        {
            CurrentPageTitle.Text = "Управление заказами";
            SetActiveButton(OrdersManageButton);
            UpdateStatus("Режим управления заказами");

            ContentFrame.Navigate(new OrdersManagePage());
        }

        private void SetActiveButton(Button activeButton)
        {
            // Сбрасываем все кнопки к обычному стилю
            ProductsViewButton.Style = (Style)FindResource("NavButtonStyle");
            OrdersViewButton.Style = (Style)FindResource("NavButtonStyle");
            ProductsManageButton.Style = (Style)FindResource("NavButtonStyle");
            OrdersManageButton.Style = (Style)FindResource("NavButtonStyle");

            // Устанавливаем активный стиль для выбранной кнопки
            if (activeButton != null)
            {
                activeButton.Style = (Style)FindResource("ActiveNavButtonStyle");
            }
        }

        private void UpdateStatus(string message)
        {
            StatusText.Text = message;
        }

        private void StartClock()
        {
            _timer = new DispatcherTimer();
            _timer.Interval = TimeSpan.FromSeconds(1);
            _timer.Tick += (s, e) =>
            {
                TimeTextBlock.Text = DateTime.Now.ToString("dd.MM.yyyy HH:mm:ss");
            };
            _timer.Start();
        }

        // Обработчики событий навигации
        private void ProductsViewButton_Click(object sender, RoutedEventArgs e)
        {
            NavigateToProductsView();
        }

        private void OrdersViewButton_Click(object sender, RoutedEventArgs e)
        {
            NavigateToOrdersView();
        }

        private void ProductsManageButton_Click(object sender, RoutedEventArgs e)
        {
            NavigateToProductsManagement();
        }

        private void OrdersManageButton_Click(object sender, RoutedEventArgs e)
        {
            NavigateToOrdersManagement();
        }

        // Обработчик кнопки "Назад"
        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            if (ContentFrame.CanGoBack)
            {
                ContentFrame.GoBack();
                UpdateStatus("Возврат к предыдущей странице");
            }
            else
            {
                UpdateStatus("Нет страниц для возврата");
            }
        }

        private void LogoutButton_Click(object sender, RoutedEventArgs e)
        {
            var result = MessageBox.Show("Вы уверены, что хотите выйти?", "Подтверждение выхода",
                MessageBoxButton.YesNo, MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                _timer?.Stop();
                _mainWindow?.ShowAuthPage();
            }
        }
    }
}