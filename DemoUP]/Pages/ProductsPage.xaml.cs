using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data.Entity;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace DemoUP_.Pages
{
    public partial class ProductsPage : Page
    {
        private string _currentUserRole;
        private List<Product> _allProducts;
        private ObservableCollection<ProductViewModel> _displayedProducts;
        private List<Supplier> _allSuppliers;

        public bool IsManagerOrAdmin => _currentUserRole == "Менеджер" || _currentUserRole == "Администратор";
        public bool IsGuest => _currentUserRole == "Гость";

        public ProductsPage(string userRole)
        {
            InitializeComponent();
            _currentUserRole = userRole;

            LoadProducts();

            // Для гостя не загружаем фильтры и поставщиков
            if (!IsGuest)
            {
                LoadSortOptions();
                LoadSuppliers();
            }

            InitializeUI();
        }

        private void InitializeUI()
        {
            // Для гостя скрываем ВСЮ панель управления
            if (IsGuest)
            {
                ControlPanelBorder.Visibility = Visibility.Collapsed;
            }
            else
            {
                // Для не-гостей показываем панель управления
                ControlPanelBorder.Visibility = Visibility.Visible;

                // Показываем кнопку добавления только для менеджеров/админов
                AddProductButton.Visibility = IsManagerOrAdmin ? Visibility.Visible : Visibility.Collapsed;
            }
        }

        private void LoadProducts()
        {
            try
            {
                using (var context = new Entities())
                {
                    _allProducts = context.Product
                        .Include("Title")
                        .Include("Supplier1")
                        .Include("Manufacture1")
                        .Include("Gender")
                        .ToList();

                    ApplyFiltersAndSort();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки товаров: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);

                _allProducts = GetSampleProducts();
                ApplyFiltersAndSort();
            }
        }

        private void LoadSortOptions()
        {
            try
            {
                var sortOptions = new List<SortOption>
        {
            new SortOption { Id = 0, Name = "По умолчанию" },
            new SortOption { Id = 1, Name = "По количеству на складе (возрастание)" },
            new SortOption { Id = 2, Name = "По количеству на складе (убывание)" }
        };

                SortComboBox.ItemsSource = sortOptions;
                SortComboBox.SelectedIndex = 0;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки опций сортировки: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void LoadSuppliers()
        {
            try
            {
                using (var context = new Entities())
                {
                    _allSuppliers = context.Supplier.ToList();

                    // Создаем список для комбобокса
                    var supplierList = new List<SupplierItem>
                    {
                        new SupplierItem { Id = 0, Name = "Все поставщики" }
                    };

                    // Добавляем всех поставщиков
                    supplierList.AddRange(_allSuppliers.Select(s => new SupplierItem
                    {
                        Id = s.ID,
                        Name = s.Supplier1
                    }));

                    SupplierFilterComboBox.ItemsSource = supplierList;
                    SupplierFilterComboBox.SelectedIndex = 0;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки поставщиков: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // Вспомогательные классы для элементов комбобоксов
        public class SortOption
        {
            public int Id { get; set; }
            public string Name { get; set; }
        }

        public class FilterOption
        {
            public int Id { get; set; }
            public string Name { get; set; }
        }

        public class SupplierItem
        {
            public int Id { get; set; }
            public string Name { get; set; }
        }

        private List<Product> GetSampleProducts()
        {
            return new List<Product>
            {
                new Product
                {
                    ID = 1,
                    Articuls = "ART001",
                    Title = new Title { Title_product = "Кроссовки спортивные" },
                    Description = "Удобные кроссовки для занятий спортом",
                    Manufacture1 = new Manufacture { Manufacture1 = "Nike" },
                    Supplier1 = new Supplier { Supplier1 = "ООО Спорттовары", ID = 1 },
                    Cost = 4500,
                    Unit = "пара",
                    Count = 15,
                    Discount = 20
                },
                new Product
                {
                    ID = 2,
                    Articuls = "ART002",
                    Title = new Title { Title_product = "Туфли классические" },
                    Description = "Элегантные туфли для офиса",
                    Manufacture1 = new Manufacture { Manufacture1 = "Ecco" },
                    Supplier1 = new Supplier { Supplier1 = "ИП Иванов", ID = 2 },
                    Cost = 3200,
                    Unit = "пара",
                    Count = 8,
                    Discount = 10
                },
                new Product
                {
                    ID = 3,
                    Articuls = "ART003",
                    Title = new Title { Title_product = "Сапоги зимние" },
                    Description = "Теплые сапоги для холодной погоды",
                    Manufacture1 = new Manufacture { Manufacture1 = "Columbia" },
                    Supplier1 = new Supplier { Supplier1 = "ООО Одежда", ID = 3 },
                    Cost = 2800,
                    Unit = "пара",
                    Count = 0,
                    Discount = 0
                }
            };
        }

        private void ApplyFiltersAndSort()
        {
            if (_allProducts == null) return;

            var filteredProducts = _allProducts.AsEnumerable();

            if (!IsGuest)
            {
                if (!string.IsNullOrEmpty(SearchTextBox?.Text))
                {
                    var searchText = SearchTextBox.Text.ToLower();
                    filteredProducts = filteredProducts.Where(p =>
                        (p.Title?.Title_product?.ToLower().Contains(searchText) == true) ||
                        (p.Articuls?.ToLower().Contains(searchText) == true) ||
                        (p.Description?.ToLower().Contains(searchText) == true) ||
                        (p.Manufacture1?.Manufacture1?.ToLower().Contains(searchText) == true) ||
                        (p.Supplier1?.Supplier1?.ToLower().Contains(searchText) == true) ||
                        (p.Gender?.Gender_product?.ToLower().Contains(searchText) == true));
                }

                // Применяем фильтр по поставщику
                if (SupplierFilterComboBox?.SelectedItem != null && SupplierFilterComboBox.SelectedIndex > 0)
                {
                    var selectedItem = (SupplierItem)SupplierFilterComboBox.SelectedItem;

                    // Если выбран реальный поставщик (не "Все поставщики")
                    if (selectedItem.Id > 0)
                    {
                        filteredProducts = filteredProducts.Where(p => p.Supplier1?.ID == selectedItem.Id);
                    }
                }

                // Сортировка
                if (SortComboBox?.SelectedItem != null && SortComboBox.SelectedIndex > 0)
                {
                    var selectedItem = (SortOption)SortComboBox.SelectedItem;

                    switch (selectedItem.Id)
                    {
                        case 1: // По количеству на складе (возрастание)
                            filteredProducts = filteredProducts.OrderBy(p => p.Count);
                            break;
                        case 2: // По количеству на складе (убывание)
                            filteredProducts = filteredProducts.OrderByDescending(p => p.Count);
                            break;
                    }
                }
            }

            _displayedProducts = new ObservableCollection<ProductViewModel>(
                filteredProducts.Select(p => new ProductViewModel(p, IsManagerOrAdmin, IsGuest)));

            ProductsItemsControl.ItemsSource = _displayedProducts;

            NoProductsText.Visibility = _displayedProducts.Any() ? Visibility.Collapsed : Visibility.Visible;
        }

        private void ProductBorder_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                if (sender is Border border)
                {
                    // Получаем DataContext Border'а (ProductViewModel)
                    if (border.DataContext is ProductViewModel viewModel)
                    {
                        // Проверяем, может ли пользователь редактировать этот товар
                        if (viewModel.CanEdit)
                        {
                            // Находим товар в списке
                            var product = _allProducts.FirstOrDefault(p => p.ID == viewModel.ID);
                            if (product != null)
                            {
                                // Открытие окна редактирования товара
                                var editWindow = new AddEditProductWindow(product);
                                if (editWindow.ShowDialog() == true)
                                {
                                    // Обновляем список товаров и поставщиков
                                    LoadProducts();
                                    if (!IsGuest)
                                    {
                                        LoadSortOptions();
                                        LoadSuppliers();
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        // Обработчики событий - для гостя они не вызываются, так как панель управления скрыта
        private void SearchTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (!IsGuest)
            {
                ApplyFiltersAndSort();
            }
        }

        private void SortComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (!IsGuest)
            {
                ApplyFiltersAndSort();
            }
        }

        private void FilterComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (!IsGuest)
            {
                ApplyFiltersAndSort();
            }
        }

        private void SupplierFilterComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (!IsGuest)
            {
                ApplyFiltersAndSort();
            }
        }

        private void AddProductButton_Click(object sender, RoutedEventArgs e)
        {
            // Для гостя эта кнопка не видна, но на всякий случай проверяем
            if (!IsGuest)
            {
                // ОТКРЫТИЕ ОКНА ДОБАВЛЕНИЯ ТОВАРА
                var addWindow = new AddEditProductWindow();
                if (addWindow.ShowDialog() == true)
                {
                    // Обновляем список товаров и поставщиков
                    LoadProducts();
                    if (!IsGuest)
                    {
                        LoadSortOptions();
                        LoadSuppliers();
                    }
                }
            }
        }

        // УДАЛЕН: EditProductButton_Click - теперь редактирование по клику на товар

        private void DeleteProductButton_Click(object sender, RoutedEventArgs e)
        {
            // Для гостя кнопки удаления не видны, но на всякий случай проверяем
            if (!IsGuest && sender is Button button && button.Tag is int productId)
            {
                var result = MessageBox.Show("Вы уверены, что хотите удалить этот товар?",
                    "Подтверждение удаления", MessageBoxButton.YesNo, MessageBoxImage.Question);

                if (result == MessageBoxResult.Yes)
                {
                    try
                    {
                        using (var context = new Entities())
                        {
                            // Проверяем, есть ли товар в составе заказов
                            var hasOrders = context.Sostav.Any(s => s.ID_articul_product == productId);

                            if (hasOrders)
                            {
                                MessageBox.Show("Нельзя удалить товар, который присутствует в заказах. " +
                                    "Сначала удалите все связанные записи в заказах.",
                                    "Ошибка удаления", MessageBoxButton.OK, MessageBoxImage.Error);
                                return;
                            }

                            var product = context.Product.FirstOrDefault(p => p.ID == productId);
                            if (product != null)
                            {
                                context.Product.Remove(product);
                                context.SaveChanges();
                                LoadProducts(); // Обновляем список
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Ошибка при удалении товара: {ex.Message}", "Ошибка",
                            MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
        }
    }

    // Обновленный ViewModel для товара
    public class ProductViewModel
    {
        public int ID { get; set; }
        public string Articuls { get; set; }
        public string TitleName { get; set; }
        public string CategoryName { get; set; }
        public string Description { get; set; }
        public string ManufacturerName { get; set; }
        public string SupplierName { get; set; }
        public decimal Cost { get; set; }
        public string Unit { get; set; }
        public int Count { get; set; }
        public int Discount { get; set; }

        // Новые свойства для оформления
        public bool HasDiscount => Discount > 0;
        public bool HasBigDiscount => Discount > 15;
        public bool IsOutOfStock => Count == 0;

        public decimal DiscountPrice => Cost * (100 - Discount) / 100;
        public string DiscountText => $"Скидка {Discount}%";

        public string OriginalPriceText
        {
            get
            {
                if (HasDiscount)
                {
                    return $"{Cost:N0}";
                }
                return $"{Cost:N0}";
            }
        }

        public Brush PriceColor => HasDiscount ? Brushes.Red : Brushes.Black;
        public TextDecorationCollection PriceDecorations => HasDiscount ? TextDecorations.Strikethrough : null;

        public Brush BackgroundColor
        {
            get
            {
                if (HasBigDiscount)
                    return new SolidColorBrush((Color)ColorConverter.ConvertFromString("#2E8B57"));
                if (IsOutOfStock)
                    return Brushes.LightBlue;
                return Brushes.White;
            }
        }

        public Brush DiscountBackground => HasBigDiscount ? Brushes.DarkGreen : Brushes.Green;

        public string StockStatus => IsOutOfStock ? "Нет в наличии" : "В наличии";
        public Brush StockStatusColor => IsOutOfStock ? Brushes.Red : Brushes.Green;

        public bool CanEdit { get; set; }
        public bool CanViewDetails { get; set; } // Для гостя - возможность просмотра деталей

        public string ImagePath { get; set; }

        // НОВОЕ СВОЙСТВО: Подсказка при наведении
        public string EditToolTip => CanEdit ? "Нажмите для редактирования товара" : "Просмотр товара";

        public ProductViewModel(Product product, bool canEdit, bool isGuest)
        {
            ID = product.ID;
            Articuls = product.Articuls ?? "Без артикула";
            TitleName = product.Title?.Title_product ?? "Без названия";
            CategoryName = product.Gender?.Gender_product ?? "Обувь";
            Description = product.Description ?? "Описание отсутствует";
            ManufacturerName = product.Manufacture1?.Manufacture1 ?? "Не указан";
            SupplierName = product.Supplier1?.Supplier1 ?? "Не указан";
            Cost = product.Cost;
            Unit = product.Unit ?? "шт.";
            Count = product.Count;
            Discount = product.Discount;
            CanEdit = canEdit && !isGuest; // Гость не может редактировать
            CanViewDetails = isGuest; // Гость может только просматривать

            // Формируем путь к изображению
            ImagePath = GetImagePath(product.Photo);
        }

        private string GetImagePath(string photoFileName)
        {
            // Если фото не указано, возвращаем заглушку
            if (string.IsNullOrEmpty(photoFileName))
            {
                return "/Pages/Image/picture.png";
            }

            // Если путь уже содержит "://" (http:// или https://), оставляем как есть
            if (photoFileName.Contains("://"))
            {
                return photoFileName;
            }

            // Если это абсолютный путь Windows (C:\...)
            if (photoFileName.Length > 2 && photoFileName[1] == ':' && photoFileName[2] == '\\')
            {
                try
                {
                    // Извлекаем только имя файла из полного пути
                    string fileName = System.IO.Path.GetFileName(photoFileName);

                    // Проверяем, существует ли файл в локальной папке приложения
                    string localPath = $"/Pages/Image/{fileName}";

                    return localPath;
                }
                catch
                {
                    // В случае ошибки парсинга пути возвращаем заглушку
                    return "/Pages/Image/picture.png";
                }
            }

            // Если это уже относительный путь или просто имя файла
            if (!photoFileName.StartsWith("/") && !photoFileName.StartsWith("\\"))
            {
                return $"/Pages/Image/{photoFileName}";
            }

            return photoFileName;
        }
    }
}