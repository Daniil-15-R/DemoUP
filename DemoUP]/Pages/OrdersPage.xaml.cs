using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Data.Entity;
using System.Windows.Input;

namespace DemoUP_.Pages
{
    public partial class OrderPage : Page
    {
        private string _currentUserRole;
        private ObservableCollection<OrderViewModel> _orders;
        private Border _selectedBorder;
        private OrderViewModel _selectedOrder;

        public bool IsAdmin => _currentUserRole == "Администратор";

        public OrderPage(string userRole)
        {
            InitializeComponent();
            _currentUserRole = userRole;

            // Скрываем/показываем кнопки управления в зависимости от роли
            SetupRoleBasedAccess();
            LoadOrders();
        }

        private void SetupRoleBasedAccess()
        {
            if (_currentUserRole == "Администратор")
            {
                // Показываем кнопки управления для администратора
                AddButton.Visibility = Visibility.Visible;
                DeleteButton.Visibility = Visibility.Visible;
            }
            else
            {
                // Скрываем кнопки управления для других ролей
                AddButton.Visibility = Visibility.Collapsed;
                DeleteButton.Visibility = Visibility.Collapsed;
            }
        }

        private void LoadOrders()
        {
            try
            {
                using (var context = new Entities())
                {
                    IQueryable<Orders> ordersQuery = context.Orders;

                    // Для менеджера и администратора показываем все заказы
                    // Для клиента нужно фильтровать по текущему пользователю
                    if (_currentUserRole == "Авторизированныйклиент")
                    {
                        // Здесь нужно добавить фильтрацию по текущему пользователю
                        // ordersQuery = ordersQuery.Where(o => o.Glient_FIO == currentUserId);
                    }

                    // Загружаем заказы с связанными данными
                    var orders = ordersQuery
                        .Include(o => o.FIO)
                        .Include(o => o.PVZ)
                        .Include(o => o.Status)
                        .Include(o => o.Sostav)
                        .Include(o => o.Sostav.Product)
                        .Include(o => o.Sostav.Product.Title)
                        .ToList();

                    // Преобразуем в ViewModel для отображения
                    _orders = new ObservableCollection<OrderViewModel>(
                        orders.Select(o => new OrderViewModel(o)));

                    OrdersItemsControl.ItemsSource = _orders;

                    // Показываем сообщение, если заказов нет
                    NoOrdersText.Visibility = _orders.Any() ? Visibility.Collapsed : Visibility.Visible;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки заказов: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);

                // Заглушка для демонстрации
                _orders = new ObservableCollection<OrderViewModel>(GetSampleOrders());
                OrdersItemsControl.ItemsSource = _orders;
                NoOrdersText.Visibility = _orders.Any() ? Visibility.Collapsed : Visibility.Visible;
            }
        }

        // ОБНОВЛЕННЫЙ ОБРАБОТЧИК: Одиночный клик для редактирования (как в ProductPage)
        private void OrderItem_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                // Получаем Border, на который кликнули
                if (sender is Border border)
                {
                    // Получаем DataContext Border'а (OrderViewModel)
                    if (border.DataContext is OrderViewModel viewModel)
                    {
                        // Сохраняем выбранный элемент
                        ClearSelection();
                        _selectedBorder = border;
                        _selectedOrder = viewModel;

                        // Устанавливаем выделение
                        border.BorderBrush = Brushes.Blue;
                        border.BorderThickness = new Thickness(2);

                        // Обновляем состояние кнопок
                        UpdateButtonsState();

                        // Если пользователь администратор, открываем редактирование сразу
                        if (IsAdmin)
                        {
                            EditSelectedOrder();
                        }
                    }
                }
            }
        }

        // Метод для редактирования выбранного заказа
        private void EditSelectedOrder()
        {
            if (_selectedOrder != null)
            {
                try
                {
                    using (var context = new Entities())
                    {
                        // Находим исходный заказ по номеру
                        Orders order = null;

                        if (int.TryParse(_selectedOrder.OrderNumber, out int orderNumber))
                        {
                            order = context.Orders
                                .Include(o => o.FIO)
                                .Include(o => o.PVZ)
                                .Include(o => o.Status)
                                .Include(o => o.Sostav)
                                .FirstOrDefault(o => o.Numer_order == orderNumber);
                        }
                        else
                        {
                            // Если OrderNumber начинается с "ORD", убираем префикс
                            var cleanOrderNumber = _selectedOrder.OrderNumber.Replace("ORD", "");
                            if (int.TryParse(cleanOrderNumber, out int cleanNumber))
                            {
                                order = context.Orders
                                    .Include(o => o.FIO)
                                    .Include(o => o.PVZ)
                                    .Include(o => o.Status)
                                    .Include(o => o.Sostav)
                                    .FirstOrDefault(o => o.Numer_order == cleanNumber);
                            }
                        }

                        if (order != null)
                        {
                            var editWindow = new AddEditOrderWindow(order);
                            if (editWindow.ShowDialog() == true)
                            {
                                LoadOrders();
                                ClearSelection();
                                _selectedOrder = null;
                                UpdateButtonsState();
                                MessageBox.Show("Заказ успешно отредактирован", "Успех",
                                    MessageBoxButton.OK, MessageBoxImage.Information);
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка при редактировании заказа: {ex.Message}", "Ошибка",
                        MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        // Упрощенный метод для сброса выделения
        private void ClearSelection()
        {
            if (_selectedBorder != null)
            {
                _selectedBorder.BorderBrush = Brushes.LightGray;
                _selectedBorder.BorderThickness = new Thickness(1);
                _selectedBorder = null;
            }
        }

        // Метод для обновления состояния кнопок
        private void UpdateButtonsState()
        {
            bool hasSelection = _selectedOrder != null;

            if (DeleteButton != null) DeleteButton.IsEnabled = hasSelection;
        }

        // Метод для демонстрационных данных (остается без изменений)
        private List<OrderViewModel> GetSampleOrders()
        {
            return new List<OrderViewModel>
            {
                new OrderViewModel
                {
                    OrderNumber = "ORD001",
                    ArticleNumber = "ART001",
                    ClientName = "Иванов Иван Иванович",
                    OrderDate = DateTime.Now.AddDays(-5),
                    DeliveryDate = DateTime.Now.AddDays(2),
                    DeliveryAddress = "г. Москва, ул. Ленина, д. 10",
                    OrderComposition = "Кроссовки спортивные (2 шт.), Туфли классические (1 шт.)",
                    StatusName = "В обработке",
                    ItemsCount = "3 товара",
                    StatusColor = Brushes.Orange
                },
                new OrderViewModel
                {
                    OrderNumber = "ORD002",
                    ArticleNumber = "ART002",
                    ClientName = "Петрова Анна Сергеевна",
                    OrderDate = DateTime.Now.AddDays(-3),
                    DeliveryDate = DateTime.Now.AddDays(1),
                    DeliveryAddress = "г. Москва, пр. Мира, д. 25",
                    OrderComposition = "Туфли вечерние (1 шт.)",
                    StatusName = "Доставляется",
                    ItemsCount = "1 товар",
                    StatusColor = Brushes.Blue
                },
                new OrderViewModel
                {
                    OrderNumber = "ORD003",
                    ArticleNumber = "ART003",
                    ClientName = "Сидоров Алексей Петрович",
                    OrderDate = DateTime.Now.AddDays(-1),
                    DeliveryDate = DateTime.Now.AddDays(5),
                    DeliveryAddress = "г. Москва, ул. Пушкина, д. 15",
                    OrderComposition = "Кроссовки беговые (1 шт.), Носки спортивные (3 пары)",
                    StatusName = "Выполнен",
                    ItemsCount = "2 позиции",
                    StatusColor = Brushes.Green
                }
            };
        }

        // Обработчики кнопок управления заказами

        private void AddButton_Click(object sender, RoutedEventArgs e)
        {
            // Проверяем права администратора
            if (_currentUserRole != "Администратор")
            {
                MessageBox.Show("Только администратор может добавлять заказы", "Ошибка прав",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var addWindow = new AddEditOrderWindow();
            if (addWindow.ShowDialog() == true)
            {
                LoadOrders();
                ClearSelection();
                _selectedOrder = null;
                UpdateButtonsState();
                MessageBox.Show("Заказ успешно добавлен", "Успех",
                    MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private void EditButton_Click(object sender, RoutedEventArgs e)
        {
            // Проверяем права администратора
            if (_currentUserRole != "Администратор")
            {
                MessageBox.Show("Только администратор может редактировать заказы", "Ошибка прав",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            EditSelectedOrder();
        }

        private void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            // Проверяем права администратора
            if (_currentUserRole != "Администратор")
            {
                MessageBox.Show("Только администратор может удалять заказы", "Ошибка прав",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (_selectedOrder != null)
            {
                var result = MessageBox.Show($"Вы уверены, что хотите удалить заказ #{_selectedOrder.OrderNumber}?",
                    "Подтверждение удаления", MessageBoxButton.YesNo, MessageBoxImage.Question);

                if (result == MessageBoxResult.Yes)
                {
                    try
                    {
                        using (var context = new Entities())
                        {
                            // Находим заказ по номеру
                            Orders orderToDelete = null;

                            if (int.TryParse(_selectedOrder.OrderNumber, out int orderNumber))
                            {
                                orderToDelete = context.Orders
                                    .Include(o => o.Sostav)
                                    .FirstOrDefault(o => o.Numer_order == orderNumber);
                            }
                            else
                            {
                                var cleanOrderNumber = _selectedOrder.OrderNumber.Replace("ORD", "");
                                if (int.TryParse(cleanOrderNumber, out int cleanNumber))
                                {
                                    orderToDelete = context.Orders
                                        .Include(o => o.Sostav)
                                        .FirstOrDefault(o => o.Numer_order == cleanNumber);
                                }
                            }

                            if (orderToDelete != null)
                            {
                                // Удаляем связанные записи в таблице Sostav
                                var sostavItems = context.Sostav
                                    .Where(s => s.ID_number_order == orderToDelete.ID_order)
                                    .ToList();

                                foreach (var item in sostavItems)
                                {
                                    context.Sostav.Remove(item);
                                }

                                // Удаляем сам заказ
                                context.Orders.Remove(orderToDelete);
                                context.SaveChanges();

                                LoadOrders();
                                ClearSelection();
                                _selectedOrder = null;
                                UpdateButtonsState();

                                MessageBox.Show("Заказ успешно удален", "Успех",
                                    MessageBoxButton.OK, MessageBoxImage.Information);
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Ошибка удаления заказа: {ex.Message}",
                            "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
            else
            {
                MessageBox.Show("Выберите заказ для удаления", "Предупреждение",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void UpdateStatusButton_Click(object sender, RoutedEventArgs e)
        {
            // Проверяем права администратора
            if (_currentUserRole != "Администратор")
            {
                MessageBox.Show("Только администратор может обновлять статусы", "Ошибка прав",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (_selectedOrder != null)
            {
                try
                {
                    using (var context = new Entities())
                    {
                        // Находим заказ по номеру
                        Orders order = null;

                        if (int.TryParse(_selectedOrder.OrderNumber, out int orderNumber))
                        {
                            order = context.Orders
                                .FirstOrDefault(o => o.Numer_order == orderNumber);
                        }
                        else
                        {
                            var cleanOrderNumber = _selectedOrder.OrderNumber.Replace("ORD", "");
                            if (int.TryParse(cleanOrderNumber, out int cleanNumber))
                            {
                                order = context.Orders
                                    .FirstOrDefault(o => o.Numer_order == cleanNumber);
                            }
                        }

                        if (order != null)
                        {
                            // Получаем все доступные статусы
                            var allStatuses = context.Status.ToList();

                            // Создаем окно выбора статуса
                            var statusWindow = new Window
                            {
                                Title = "Выбор нового статуса",
                                Width = 300,
                                Height = 150,
                                WindowStartupLocation = WindowStartupLocation.CenterOwner,
                                Owner = Window.GetWindow(this)
                            };

                            var stackPanel = new StackPanel { Margin = new Thickness(10) };

                            var comboBox = new ComboBox
                            {
                                ItemsSource = allStatuses,
                                DisplayMemberPath = "Status1",
                                SelectedValuePath = "ID_status"
                            };

                            // Устанавливаем текущий статус
                            comboBox.SelectedValue = order.Status_order;

                            var buttonPanel = new StackPanel
                            {
                                Orientation = Orientation.Horizontal,
                                HorizontalAlignment = HorizontalAlignment.Right,
                                Margin = new Thickness(0, 10, 0, 0)
                            };

                            var okButton = new Button
                            {
                                Content = "ОК",
                                Width = 80,
                                Margin = new Thickness(0, 0, 10, 0)
                            };

                            var cancelButton = new Button
                            {
                                Content = "Отмена",
                                Width = 80
                            };

                            bool dialogResult = false;

                            okButton.Click += (s, args) =>
                            {
                                dialogResult = true;
                                statusWindow.Close();
                            };

                            cancelButton.Click += (s, args) =>
                            {
                                dialogResult = false;
                                statusWindow.Close();
                            };

                            buttonPanel.Children.Add(okButton);
                            buttonPanel.Children.Add(cancelButton);

                            stackPanel.Children.Add(new TextBlock { Text = "Выберите новый статус:" });
                            stackPanel.Children.Add(comboBox);
                            stackPanel.Children.Add(buttonPanel);

                            statusWindow.Content = stackPanel;
                            statusWindow.ShowDialog();

                            if (dialogResult && comboBox.SelectedValue != null)
                            {
                                var orderToUpdate = context.Orders.Find(order.ID_order);
                                if (orderToUpdate != null)
                                {
                                    orderToUpdate.Status_order = (int)comboBox.SelectedValue;
                                    context.SaveChanges();
                                    LoadOrders();
                                    ClearSelection();
                                    _selectedOrder = null;
                                    UpdateButtonsState();

                                    MessageBox.Show("Статус заказа успешно обновлен", "Успех",
                                        MessageBoxButton.OK, MessageBoxImage.Information);
                                }
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка обновления статуса: {ex.Message}", "Ошибка",
                        MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            else
            {
                MessageBox.Show("Выберите заказ для обновления статуса", "Предупреждение",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void RefreshButton_Click(object sender, RoutedEventArgs e)
        {
            LoadOrders();
            ClearSelection();
            _selectedOrder = null;
            UpdateButtonsState();
        }
    }

    // ViewModel для отображения заказа (остается без изменений)
    public class OrderViewModel
    {
        public string OrderNumber { get; set; }
        public string ArticleNumber { get; set; }
        public string ClientName { get; set; }
        public DateTime OrderDate { get; set; }
        public DateTime DeliveryDate { get; set; }
        public string DeliveryAddress { get; set; }
        public string OrderComposition { get; set; }
        public string StatusName { get; set; }
        public string ItemsCount { get; set; }
        public Brush StatusColor { get; set; }

        public OrderViewModel() { }

        public OrderViewModel(Orders order)
        {
            // Используем правильные названия свойств из класса Orders
            OrderNumber = order.Numer_order.ToString();

            // Генерируем артикул на основе номера заказа, так как в модели его нет
            ArticleNumber = $"ART{order.Numer_order:D6}";

            ClientName = order.FIO?.FIO1 ?? "Неизвестный клиент";
            OrderDate = order.Date_order;
            DeliveryDate = order.Date_delivery;

            // Формируем адрес доставки
            DeliveryAddress = FormatDeliveryAddress(order.PVZ);

            // Формируем состав заказа
            OrderComposition = FormatOrderComposition(order.Sostav);

            StatusName = order.Status?.Status1 ?? "Неизвестный статус";
            ItemsCount = GetItemsCount(order.Sostav);
            StatusColor = GetStatusColor(order.Status?.Status1);
        }

        private string FormatDeliveryAddress(PVZ pvz)
        {
            if (pvz == null) return "Адрес не указан";

            var street = pvz.Street1?.Street1 ?? "Неизвестная улица";
            var home = pvz.Home_Number?.Home?.ToString() ?? "н/д";
            var index = pvz.Index_PVZ.ToString();
            var state = pvz.State ?? "г. Москва";

            return $"{state}, {street}, д. {home} (индекс: {index})";
        }

        private string FormatOrderComposition(Sostav sostav)
        {
            if (sostav == null) return "Состав не указан";

            var productName = sostav.Product?.Title?.Title_product ?? "Неизвестный товар";
            var count = sostav.Count?.ToString() ?? "0";

            return $"{productName} ({count} шт.)";
        }

        private string GetItemsCount(Sostav sostav)
        {
            if (sostav == null) return "0 товаров";

            var count = sostav.Count ?? 0;
            return count == 1 ? "1 товар" : $"{count} товара";
        }

        private Brush GetStatusColor(string status)
        {
            switch (status?.ToLower())
            {
                case "новый":
                case "в обработке":
                    return Brushes.Orange;
                case "доставляется":
                case "в пути":
                    return Brushes.Blue;
                case "выполнен":
                case "доставлен":
                    return Brushes.Green;
                case "отменен":
                    return Brushes.Red;
                default:
                    return Brushes.Gray;
            }
        }
    }
}