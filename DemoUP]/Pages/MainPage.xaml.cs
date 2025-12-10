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
            }
            else
            {
                // Для остальных пользователей
                if (_currentUserRole == "Менеджер" || _currentUserRole == "Администратор")
                {
                    OrdersViewButton.Visibility = Visibility.Visible;
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
            // Показываем кнопку всегда, но меняем её активность
            BackButton.Visibility = Visibility.Visible;
            BackButton.IsEnabled = ContentFrame.CanGoBack;
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
            }
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


        private void SetActiveButton(Button activeButton)
        {
            // Сбрасываем стили всех кнопок
            ProductsViewButton.Style = (Style)FindResource("NavButtonStyle");
            OrdersViewButton.Style = (Style)FindResource("NavButtonStyle");

            // Убрана строка: ProductsManageButton.Style = (Style)FindResource("NavButtonStyle");

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
            var result = MessageBox.Show("Вы уверены, что хотите выйти из системы?\n" +
                                        "Все несохраненные данные будут потеряны.", "Подтверждение выхода",
                MessageBoxButton.YesNo, MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                _timer?.Stop();
                _mainWindow?.ShowAuthPage();
            }
        }
    }
}