using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace DemoUP_.Pages
{
    public partial class ProductsManagePage : Page
    {
        public ProductsManagePage()
        {
            InitializeComponent();
            LoadProducts();
        }

        private void LoadProducts()
        {
            try
            {
                using (var context = new Entities())
                {
                    var products = context.Product
                        .Include("Title")
                        .Include("Gender")
                        .Include("Manufacture1")
                        .Include("Supplier1")
                        .ToList();

                    ProductsGrid.ItemsSource = products;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки товаров: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void AddProductButton_Click(object sender, RoutedEventArgs e)
        {
            var addWindow = new AddEditProductWindow();
            if (addWindow.ShowDialog() == true)
            {
                LoadProducts();
            }
        }

        private void EditProductButton_Click(object sender, RoutedEventArgs e)
        {
            var selectedProduct = ProductsGrid.SelectedItem as Product;
            if (selectedProduct != null)
            {
                var editWindow = new AddEditProductWindow(selectedProduct);
                if (editWindow.ShowDialog() == true)
                {
                    LoadProducts();
                }
            }
            else
            {
                MessageBox.Show("Выберите товар для редактирования", "Предупреждение",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void RefreshButton_Click(object sender, RoutedEventArgs e)
        {
            LoadProducts();
        }
    }
}