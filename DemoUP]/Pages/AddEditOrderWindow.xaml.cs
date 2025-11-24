using System;
using System.Linq;
using System.Net;
using System.Windows;

namespace DemoUP_.Pages
{
    public partial class AddEditOrderWindow : Window
    {
        private Orders _currentOrder = new Orders();
        private bool _isEdit = false;

        public AddEditOrderWindow(Orders order = null)
        {
            InitializeComponent();

            if (order != null)
            {
                _currentOrder = order;
                _isEdit = true;
                Title = "Редактирование заказа";
            }
            else
            {
                _currentOrder.Date_order = DateTime.Now;
                _currentOrder.Date_delivery = DateTime.Now.AddDays(7);
                _currentOrder.Code = GenerateOrderCode();
                Title = "Добавление заказа";
            }

            DataContext = _currentOrder;
            LoadComboBoxData();
        }

        private int GenerateOrderCode()
        {
            return new Random().Next(1000, 9999);
        }

        private void LoadComboBoxData()
        {
            try
            {
                using (var context = new Entities())
                {
                    // Загружаем ПВЗ
                    cmbPVZ.ItemsSource = context.PVZ.ToList();
                    cmbPVZ.SelectedValuePath = "ID";

                    // Загружаем статусы
                    cmbStatus.ItemsSource = context.Status.ToList();
                    cmbStatus.SelectedValuePath = "ID";

                    // Загружаем клиентов
                    cmbClient.ItemsSource = context.FIO.ToList();
                    cmbClient.SelectedValuePath = "ID";

                    // Загружаем составы заказов
                    cmbSostav.ItemsSource = context.Sostav.ToList();
                    cmbSostav.SelectedValuePath = "ID";

                    // Если редактируем существующий заказ, устанавливаем значения
                    if (_isEdit)
                    {
                        cmbPVZ.SelectedValue = _currentOrder.Adress_PVZ;
                        cmbStatus.SelectedValue = _currentOrder.Status_order;
                        cmbClient.SelectedValue = _currentOrder.Glient_FIO;
                        cmbSostav.SelectedValue = _currentOrder.ID_sostav;
                    }
                    else
                    {
                        // Устанавливаем значения по умолчанию для нового заказа
                        if (cmbClient.Items.Count > 0)
                            cmbClient.SelectedIndex = 0;
                        if (cmbSostav.Items.Count > 0)
                            cmbSostav.SelectedIndex = 0;
                        if (cmbStatus.Items.Count > 0)
                            cmbStatus.SelectedIndex = 0;
                        if (cmbPVZ.Items.Count > 0)
                            cmbPVZ.SelectedIndex = 0;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки данных: {ex.Message}");
            }
        }

        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            // Валидация данных
            if (!ValidateData())
                return;

            try
            {
                // Убеждаемся, что все FK поля заполнены
                if (cmbPVZ.SelectedValue == null || cmbStatus.SelectedValue == null ||
                    cmbClient.SelectedValue == null || cmbSostav.SelectedValue == null)
                {
                    MessageBox.Show("Заполните все обязательные поля!");
                    return;
                }

                using (var context = new Entities())
                {
                    if (_isEdit)
                    {
                        // Редактирование существующего заказа
                        var orderToUpdate = context.Orders.Find(_currentOrder.ID_order);
                        if (orderToUpdate != null)
                        {
                            orderToUpdate.Numer_order = _currentOrder.Numer_order;
                            orderToUpdate.Date_order = _currentOrder.Date_order;
                            orderToUpdate.Date_delivery = _currentOrder.Date_delivery;
                            orderToUpdate.Adress_PVZ = (int)cmbPVZ.SelectedValue;
                            orderToUpdate.Status_order = (int)cmbStatus.SelectedValue;
                            orderToUpdate.Glient_FIO = (int)cmbClient.SelectedValue;
                            orderToUpdate.ID_sostav = (int)cmbSostav.SelectedValue;
                            orderToUpdate.Code = _currentOrder.Code;
                        }
                    }
                    else
                    {
                        // Создание нового заказа
                        var newOrder = new Orders
                        {
                            Numer_order = _currentOrder.Numer_order,
                            Date_order = _currentOrder.Date_order,
                            Date_delivery = _currentOrder.Date_delivery,
                            Adress_PVZ = (int)cmbPVZ.SelectedValue,
                            Status_order = (int)cmbStatus.SelectedValue,
                            Glient_FIO = (int)cmbClient.SelectedValue,
                            ID_sostav = (int)cmbSostav.SelectedValue,
                            Code = _currentOrder.Code
                            // ID_order будет сгенерирован базой данных автоматически
                        };

                        context.Orders.Add(newOrder);
                    }

                    context.SaveChanges();
                    MessageBox.Show("Данные успешно сохранены!");
                    this.DialogResult = true;
                    Close();
                }
            }
            catch (Exception ex)
            {
                string errorMessage = $"Ошибка сохранения: {ex.Message}";
                if (ex.InnerException != null)
                {
                    errorMessage += $"\nВнутренняя ошибка: {ex.InnerException.Message}";

                    // Дополнительная информация для отладки
                    if (ex.InnerException.InnerException != null)
                    {
                        errorMessage += $"\nДетали: {ex.InnerException.InnerException.Message}";
                    }
                }
                MessageBox.Show(errorMessage, "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private bool ValidateData()
        {
            if (_currentOrder.Numer_order <= 0)
            {
                MessageBox.Show("Введите корректный номер заказа!");
                txtOrderNumber.Focus();
                return false;
            }

            if (_currentOrder.Date_delivery < _currentOrder.Date_order)
            {
                MessageBox.Show("Дата доставки не может быть раньше даты заказа!");
                dpDeliveryDate.Focus();
                return false;
            }

            if (cmbPVZ.SelectedItem == null)
            {
                MessageBox.Show("Выберите ПВЗ!");
                cmbPVZ.Focus();
                return false;
            }

            if (cmbStatus.SelectedItem == null)
            {
                MessageBox.Show("Выберите статус!");
                cmbStatus.Focus();
                return false;
            }

            if (cmbClient.SelectedItem == null)
            {
                MessageBox.Show("Выберите клиента!");
                cmbClient.Focus();
                return false;
            }

            if (cmbSostav.SelectedItem == null)
            {
                MessageBox.Show("Выберите состав заказа!");
                cmbSostav.Focus();
                return false;
            }

            return true;
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
            Close();
        }
    }
}