using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using System.Windows.Threading;

namespace DemoUP_.Pages
{
    public partial class MainPage : Page
    {
        private string _currentUserRole;
        private string _currentUserName;
        private DispatcherTimer _timer;
        private MainWindow _mainWindow;

        public MainPage(string userRole, string userName, MainWindow mainWindow)
        {
            InitializeComponent();
            _currentUserRole = userRole;
            _currentUserName = userName;
            _mainWindow = mainWindow;

            InitializeUI();
            SetupNavigation();
            StartClock();
            SetupBackButton();
        }

        private void InitializeUI()
        {
            UserInfoText.Text = $"Пользователь: {_currentUserName}";
            SetupRoleBasedAccess();
        }

        private void SetupRoleBasedAccess()
        {
            // Для всех пользователей показываем просмотр товаров
            ProductsViewButton.Visibility = Visibility.Visible;

            // Для гостя скрываем все остальные кнопки
            if (_currentUserRole == "Гость")
            {
                OrdersViewButton.Visibility = Visibility.Collapsed;
                ProductsManageButton.Visibility = Visibility.Collapsed;
                OrdersManageButton.Visibility = Visibility.Collapsed;
            }
            else
            {
                // Для остальных пользователей
                if (_currentUserRole == "Менеджер" || _currentUserRole == "Администратор")
                {
                    OrdersViewButton.Visibility = Visibility.Visible;
                }

                if (_currentUserRole == "Администратор")
                {
                    ProductsManageButton.Visibility = Visibility.Visible;
                    OrdersManageButton.Visibility = Visibility.Visible;
                }
            }
        }

        private void SetupNavigation()
        {
            // Подписываемся на события навигации
            ContentFrame.Navigated += ContentFrame_Navigated;
            NavigateToProductsView();
        }

        private void SetupBackButton()
        {
            // Обновляем видимость кнопки "Назад"
            UpdateBackButtonVisibility();

            // Обновляем состояние кнопки при изменении журнала навигации
            ContentFrame.Navigated += (s, e) => UpdateBackButtonVisibility();
        }

        private void UpdateBackButtonVisibility()
        {
            // Показываем кнопку "Назад", только если есть куда возвращаться
            BackButton.Visibility = ContentFrame.CanGoBack ? Visibility.Visible : Visibility.Collapsed;
        }

        private void ContentFrame_Navigated(object sender, NavigationEventArgs e)
        {
            // Обновляем заголовок и статус при навигации
            var page = e.Content as Page;
            if (page != null)
            {
                // Определяем заголовок страницы на основе ее типа
                if (page is ProductsPage)
                {
                    CurrentPageTitle.Text = "Просмотр товаров";
                    SetActiveButton(ProductsViewButton);
                    UpdateStatus("Режим просмотра товаров");
                }
                else if (page is OrderPage)
                {
                    CurrentPageTitle.Text = "Просмотр заказов";
                    SetActiveButton(OrdersViewButton);
                    UpdateStatus("Режим просмотра заказов");
                }
                else if (page is ProductsManagePage)
                {
                    CurrentPageTitle.Text = "Управление товарами";
                    SetActiveButton(ProductsManageButton);
                    UpdateStatus("Режим управления товарами");
                }
                else if (page is OrdersManagePage)
                {
                    CurrentPageTitle.Text = "Управление заказами";
                    SetActiveButton(OrdersManageButton);
                    UpdateStatus("Режим управления заказами");
                }
            }

            // Обновляем видимость кнопки "Назад"
            UpdateBackButtonVisibility();
        }

        private void NavigateToProductsView()
        {
            ContentFrame.Navigate(new ProductsPage(_currentUserRole));
        }

        private void NavigateToOrdersView()
        {
            ContentFrame.Navigate(new OrderPage(_currentUserRole));
        }

        private void NavigateToProductsManagement()
        {
            ContentFrame.Navigate(new ProductsManagePage());
        }

        private void NavigateToOrdersManagement()
        {
            ContentFrame.Navigate(new OrdersManagePage());
        }

        private void SetActiveButton(Button activeButton)
        {
            ProductsViewButton.Style = (Style)FindResource("NavButtonStyle");
            OrdersViewButton.Style = (Style)FindResource("NavButtonStyle");
            ProductsManageButton.Style = (Style)FindResource("NavButtonStyle");
            OrdersManageButton.Style = (Style)FindResource("NavButtonStyle");

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

        // Улучшенный обработчик кнопки "Назад"
        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            if (ContentFrame.CanGoBack)
            {
                // Получаем информацию о текущей странице перед возвратом
                var currentPage = ContentFrame.Content as Page;
                string currentPageName = currentPage?.GetType().Name ?? "неизвестная страница";

                // Возвращаемся назад
                ContentFrame.GoBack();

                // Обновляем статус
                UpdateStatus($"Возврат с: {currentPageName}");

                // Убираем запись из журнала навигации после перехода
                // Это предотвращает бесконечную историю переходов
                if (ContentFrame.CanGoBack)
                {
                    ContentFrame.RemoveBackEntry();
                }
            }
            else
            {
                UpdateStatus("Нет страниц для возврата");
            }

            // Обновляем видимость кнопки после перехода
            UpdateBackButtonVisibility();
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