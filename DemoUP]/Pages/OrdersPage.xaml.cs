using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Data.Entity;

namespace DemoUP_.Pages
{
    public partial class OrderPage : Page
    {
        private string _currentUserRole;
        private ObservableCollection<OrderViewModel> _orders;

        public OrderPage(string userRole)
        {
            InitializeComponent();
            _currentUserRole = userRole;
            LoadOrders();
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

        // Метод для демонстрационных данных
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
    }

    // ViewModel для отображения заказа
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