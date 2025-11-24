using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Data.Entity;

namespace DemoUP_.Pages
{
    public partial class OrdersManagePage : Page
    {
        public OrdersManagePage()
        {
            InitializeComponent();
            LoadOrders();
        }

        private void LoadOrders()
        {
            try
            {
                using (var context = new Entities())
                {
                    var orders = context.Orders
                        .Include("Status")
                        .Include("PVZ")
                        .Include("FIO")
                        .Include("Sostav")
                        .Include("Sostav.Product")
                        .Include("Sostav.Product.Title")
                        .ToList();

                    OrdersGrid.ItemsSource = orders;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки заказов: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void RefreshButton_Click(object sender, RoutedEventArgs e)
        {
            LoadOrders();
        }
        private void AddOrderButton_Click(object sender, RoutedEventArgs e)
        {
            var addWindow = new AddEditOrderWindow();
            if (addWindow.ShowDialog() == true)
            {
                LoadOrders();
                MessageBox.Show("Заказ успешно добавлен", "Успех",
                    MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private void EditOrderButton_Click(object sender, RoutedEventArgs e)
        {
            var selectedOrder = OrdersGrid.SelectedItem as Orders;
            if (selectedOrder != null)
            {
                var editWindow = new AddEditOrderWindow(selectedOrder);
                if (editWindow.ShowDialog() == true)
                {
                    LoadOrders();
                    MessageBox.Show("Заказ успешно отредактирован", "Успех",
                        MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            else
            {
                MessageBox.Show("Выберите заказ для редактирования", "Предупреждение",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void UpdateOrderStatusButton_Click(object sender, RoutedEventArgs e)
        {
            var selectedOrder = OrdersGrid.SelectedItem as Orders;
            if (selectedOrder != null)
            {
                // Логика обновления статуса заказа
                try
                {
                    using (var context = new Entities())
                    {
                        var orderToUpdate = context.Orders.Find(selectedOrder.ID_order);
                        if (orderToUpdate != null)
                        {
                            // Здесь можно добавить диалог для выбора нового статуса
                            // orderToUpdate.Status_order = newStatusId;
                            context.SaveChanges();
                            LoadOrders();
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
    }
}