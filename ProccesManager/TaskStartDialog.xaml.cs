using System.Windows;

namespace ProccesManager
{
    /// <summary>
    /// Логика взаимодействия для TaskStartDialog.xaml
    /// </summary>
    public partial class TaskStartDialog : Window
    {
        public string ProcessName => ProcessNameTextBox.Text;
        public TaskStartDialog()
        {
            InitializeComponent();
        }

        private void Ok_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void Browse_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new Microsoft.Win32.OpenFileDialog
            {
                Filter = "Исполняемые файлы (*.exe)|*.exe",  // Устанавливаем фильтр для .exe файлов
                Multiselect = true  // Разрешаем выбирать несколько файлов
            };

            // Открытие диалога выбора файлов
            if (dialog.ShowDialog() == true)
            {
                // Если выбрано несколько файлов, присваиваем их в текстовое поле через запятую
                ProcessNameTextBox.Text = string.Join(", ", dialog.FileNames);
            }
        }
    }
}
