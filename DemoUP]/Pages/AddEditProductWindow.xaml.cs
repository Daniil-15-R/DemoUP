using Microsoft.Win32;
using System;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace DemoUP_.Pages
{
    public partial class AddEditProductWindow : Window
    {
        private Product _product;
        private bool _isEditMode;
        private string _currentPhoto;
        private static bool _isWindowOpen = false;

        public AddEditProductWindow()
        {
            if (_isWindowOpen)
            {
                MessageBox.Show("Окно редактирования товара уже открыто!", "Предупреждение",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            InitializeComponent();
            _isEditMode = false;
            _isWindowOpen = true;
            Title = "Добавление товара";
            LoadComboBoxData();
        }

        public AddEditProductWindow(Product product)
        {
            if (_isWindowOpen)
            {
                MessageBox.Show("Окно редактирования товара уже открыто!", "Предупреждение",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            InitializeComponent();
            _product = product;
            _isEditMode = true;
            _isWindowOpen = true;
            Title = "Редактирование товара";
            LoadComboBoxData();
            LoadProductData();
        }

        private void LoadComboBoxData()
        {
            try
            {
                using (var context = new Entities())
                {
                    // Загрузка категорий
                    var categories = context.Gender.ToList();
                    CategoryComboBox.ItemsSource = categories;

                    // Загрузка производителей
                    var manufacturers = context.Manufacture.ToList();
                    ManufacturerComboBox.ItemsSource = manufacturers;

                    // Поставщики больше не загружаются в ComboBox
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки данных: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void LoadProductData()
        {
            if (_product != null)
            {
                // Показываем ID только в режиме редактирования
                IdPanel.Visibility = Visibility.Visible;
                IdTextBox.Text = _product.ID.ToString();

                ArticulTextBox.Text = _product.Articuls;
                TitleTextBox.Text = _product.Title?.Title_product;
                DescriptionTextBox.Text = _product.Description;
                CostTextBox.Text = _product.Cost.ToString();
                CountTextBox.Text = _product.Count.ToString();
                DiscountTextBox.Text = _product.Discount.ToString();
                UnitTextBox.Text = _product.Unit;

                // Устанавливаем выбранные значения в ComboBox
                if (_product.Gender != null)
                    CategoryComboBox.SelectedItem = _product.Gender;

                if (_product.Manufacture1 != null)
                    ManufacturerComboBox.SelectedItem = _product.Manufacture1;

                // Заполняем текстовое поле поставщика
                if (_product.Supplier1 != null)
                    SupplierTextBox.Text = _product.Supplier1.Supplier1;

                // Загрузка изображения
                if (!string.IsNullOrEmpty(_product.Photo) && File.Exists(_product.Photo))
                {
                    LoadImageFromPath(_product.Photo);
                }
            }
        }

        private void LoadImageFromPath(string imagePath)
        {
            try
            {
                var bitmap = new BitmapImage();
                bitmap.BeginInit();
                bitmap.UriSource = new Uri(imagePath);
                bitmap.EndInit();
                ProductImage.Source = bitmap;
                _currentPhoto = imagePath;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки изображения: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void LoadImageButton_Click(object sender, RoutedEventArgs e)
        {
            var openFileDialog = new OpenFileDialog
            {
                Filter = "Image files (*.jpg; *.jpeg; *.png; *.bmp)|*.jpg;*.jpeg;*.png;*.bmp|All files (*.*)|*.*",
                Title = "Выберите изображение товара"
            };

            if (openFileDialog.ShowDialog() == true)
            {
                try
                {
                    // Проверяем размер изображения
                    var imageInfo = new FileInfo(openFileDialog.FileName);
                    if (imageInfo.Length > 5 * 1024 * 1024) // 5MB limit
                    {
                        MessageBox.Show("Размер изображения не должен превышать 5MB", "Ошибка",
                            MessageBoxButton.OK, MessageBoxImage.Warning);
                        return;
                    }

                    // Ресайз изображения до 300x200
                    var resizedImage = ResizeImage(openFileDialog.FileName, 300, 200);

                    // Сохраняем в папку приложения
                    var appFolder = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "ProductImages");
                    if (!Directory.Exists(appFolder))
                        Directory.CreateDirectory(appFolder);

                    var fileName = $"product_{DateTime.Now:yyyyMMddHHmmssfff}.png";
                    var newImagePath = Path.Combine(appFolder, fileName);

                    // Сохраняем ресайзнутое изображение
                    SaveBitmapImage(resizedImage, newImagePath);

                    // Удаляем старое изображение если оно было
                    if (!string.IsNullOrEmpty(_currentPhoto) && File.Exists(_currentPhoto))
                    {
                        try
                        {
                            File.Delete(_currentPhoto);
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show($"Не удалось удалить старое изображение: {ex.Message}", "Предупреждение",
                                MessageBoxButton.OK, MessageBoxImage.Warning);
                        }
                    }

                    // Устанавливаем новое изображение
                    ProductImage.Source = resizedImage;
                    _currentPhoto = newImagePath;

                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка загрузки изображения: {ex.Message}", "Ошибка",
                        MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private BitmapImage ResizeImage(string imagePath, int maxWidth, int maxHeight)
        {
            var originalBitmap = new BitmapImage(new Uri(imagePath));

            double scaleX = (double)maxWidth / originalBitmap.PixelWidth;
            double scaleY = (double)maxHeight / originalBitmap.PixelHeight;
            double scale = Math.Min(scaleX, scaleY);

            var transformedBitmap = new TransformedBitmap(originalBitmap,
                new ScaleTransform(scale, scale));

            var bitmapImage = new BitmapImage();
            var memoryStream = new System.IO.MemoryStream();

            var encoder = new PngBitmapEncoder();
            encoder.Frames.Add(BitmapFrame.Create(transformedBitmap));
            encoder.Save(memoryStream);

            bitmapImage.BeginInit();
            bitmapImage.StreamSource = memoryStream;
            bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
            bitmapImage.EndInit();
            bitmapImage.Freeze();

            return bitmapImage;
        }

        private void SaveBitmapImage(BitmapImage image, string filePath)
        {
            var encoder = new PngBitmapEncoder();
            encoder.Frames.Add(BitmapFrame.Create(image));

            using (var fileStream = new FileStream(filePath, FileMode.Create))
            {
                encoder.Save(fileStream);
            }
        }

        private void RemoveImageButton_Click(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrEmpty(_currentPhoto) && File.Exists(_currentPhoto))
            {
                try
                {
                    File.Delete(_currentPhoto);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Не удалось удалить изображение: {ex.Message}", "Ошибка",
                        MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }

            ProductImage.Source = new BitmapImage(new Uri("/DemoUP_;component/Pages/Image/picture.png", UriKind.Relative));
            _currentPhoto = null;
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            if (ValidateData())
            {
                try
                {
                    using (var context = new Entities())
                    {
                        if (_isEditMode && _product != null)
                        {
                            // Редактирование существующего товара
                            var dbProduct = context.Product
                                .Include(p => p.Title)
                                .Include(p => p.Gender)
                                .Include(p => p.Manufacture1)
                                .Include(p => p.Supplier1)
                                .FirstOrDefault(p => p.ID == _product.ID);

                            if (dbProduct != null)
                            {
                                UpdateProductData(dbProduct, context);
                            }
                        }
                        else
                        {
                            // Добавление нового товара
                            var newProduct = new Product();
                            UpdateProductData(newProduct, context);
                            context.Product.Add(newProduct);
                        }

                        context.SaveChanges();
                        DialogResult = true;
                        Close();
                    }
                }
                catch (System.Data.Entity.Validation.DbEntityValidationException ex)
                {
                    var errorMessages = ex.EntityValidationErrors
                        .SelectMany(x => x.ValidationErrors)
                        .Select(x => x.ErrorMessage);
                    var fullErrorMessage = string.Join("; ", errorMessages);
                    MessageBox.Show($"Ошибка валидации: {fullErrorMessage}", "Ошибка",
                        MessageBoxButton.OK, MessageBoxImage.Error);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка сохранения: {ex.Message}\n\nВнутреннее исключение: {ex.InnerException?.Message}", "Ошибка",
                        MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void UpdateProductData(Product product, Entities context)
        {
            product.Articuls = ArticulTextBox.Text.Trim();

            // Работа с названием (Title)
            if (product.Title == null)
            {
                var newTitle = new Title { Title_product = TitleTextBox.Text.Trim() };
                context.Title.Add(newTitle);
                product.Title = newTitle;
                product.Title_product = newTitle.ID;
            }
            else
            {
                product.Title.Title_product = TitleTextBox.Text.Trim();
            }

            // Устанавливаем выбранные значения из ComboBox
            if (CategoryComboBox.SelectedItem is Gender selectedCategory)
            {
                product.Gender = selectedCategory;
                product.Category = selectedCategory.ID;
            }

            if (ManufacturerComboBox.SelectedItem is Manufacture selectedManufacturer)
            {
                product.Manufacture1 = selectedManufacturer;
                product.Manufacture = selectedManufacturer.ID;
            }

            // Работа с поставщиком (теперь текстовое поле)
            if (!string.IsNullOrWhiteSpace(SupplierTextBox.Text))
            {
                // Ищем существующего поставщика
                var existingSupplier = context.Supplier.FirstOrDefault(s => s.Supplier1 == SupplierTextBox.Text.Trim());

                if (existingSupplier != null)
                {
                    product.Supplier1 = existingSupplier;
                    product.Supplier = existingSupplier.ID;
                }
                else
                {
                    // Создаем нового поставщика
                    var newSupplier = new Supplier { Supplier1 = SupplierTextBox.Text.Trim() };
                    context.Supplier.Add(newSupplier);
                    product.Supplier1 = newSupplier;
                    product.Supplier = newSupplier.ID;
                }
            }

            product.Description = DescriptionTextBox.Text.Trim();
            product.Cost = short.Parse(CostTextBox.Text);
            product.Count = int.Parse(CountTextBox.Text);
            product.Discount = int.Parse(DiscountTextBox.Text);
            product.Unit = UnitTextBox.Text.Trim();
            product.Photo = _currentPhoto;
        }

        private bool ValidateData()
        {
            // Проверка названия
            if (string.IsNullOrWhiteSpace(TitleTextBox.Text))
            {
                MessageBox.Show("Введите название товара", "Ошибка валидации",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                TitleTextBox.Focus();
                return false;
            }

            // Проверка артикула
            if (string.IsNullOrWhiteSpace(ArticulTextBox.Text))
            {
                MessageBox.Show("Введите артикул товара", "Ошибка валидации",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                ArticulTextBox.Focus();
                return false;
            }

            // Проверка категории
            if (CategoryComboBox.SelectedItem == null)
            {
                MessageBox.Show("Выберите категорию товара", "Ошибка валидации",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                CategoryComboBox.Focus();
                return false;
            }

            // Проверка производителя
            if (ManufacturerComboBox.SelectedItem == null)
            {
                MessageBox.Show("Выберите производителя товара", "Ошибка валидации",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                ManufacturerComboBox.Focus();
                return false;
            }

            // Проверка поставщика (теперь текстовое поле)
            if (string.IsNullOrWhiteSpace(SupplierTextBox.Text))
            {
                MessageBox.Show("Введите поставщика товара", "Ошибка валидации",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                SupplierTextBox.Focus();
                return false;
            }

            // Проверка цены
            if (!short.TryParse(CostTextBox.Text, out short cost) || cost < 0)
            {
                MessageBox.Show("Введите корректную цену (целое неотрицательное число)", "Ошибка валидации",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                CostTextBox.Focus();
                return false;
            }

            // Проверка количества
            if (!int.TryParse(CountTextBox.Text, out int count) || count < 0)
            {
                MessageBox.Show("Количество не может быть отрицательным", "Ошибка валидации",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                CountTextBox.Focus();
                return false;
            }

            // Проверка скидки
            if (!int.TryParse(DiscountTextBox.Text, out int discount) || discount < 0 || discount > 100)
            {
                MessageBox.Show("Скидка должна быть от 0 до 100%", "Ошибка валидации",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                DiscountTextBox.Focus();
                return false;
            }

            // Проверка единицы измерения
            if (string.IsNullOrWhiteSpace(UnitTextBox.Text))
            {
                MessageBox.Show("Введите единицу измерения", "Ошибка валидации",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                UnitTextBox.Focus();
                return false;
            }

            return true;
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            _isWindowOpen = false;
        }
    }
}