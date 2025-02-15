using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Windows;

namespace ProccesManager
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public ObservableCollection<ProcessInfo> Processes { get; set; } = new ObservableCollection<ProcessInfo>();
        public ObservableCollection<AppInfo> Applications { get; set; } = new ObservableCollection<AppInfo>();

        public MainWindow()
        {
            InitializeComponent();
            DataContext = this;
        }

        // Загрузка информации о процессах
        private void LoadProcess()
        {
            Processes.Clear();
            foreach (var process in Process.GetProcesses().OrderBy(p => p.ProcessName))
            {
                try
                {
                    Processes.Add(new ProcessInfo
                    {
                        ProcessName = process.ProcessName,
                        Id = process.Id,
                        MemoryUsage = Math.Round(process.WorkingSet64 / 1024 / 1024.0, 2)
                    });
                }
                catch (Exception ex)
                {
                    System.Windows.Forms.MessageBox.Show(ex.Message);
                }
            }
            StatusTextBlock.Text = $"Запущено процессов: {Processes.Count}";
        }

        // Загрузка информации о приложениях
        private void LoadApplications()
        {
            Applications.Clear();
            foreach (var process in Process.GetProcesses().Where(p => !string.IsNullOrWhiteSpace(
                p.MainWindowTitle)))
            {
                try
                {
                    Applications.Add(new AppInfo
                    {
                        AppName = process.MainWindowTitle,
                        StartTime = process.StartTime.ToString("HH:mm:ss")
                    });
                }
                catch { }
            }
        }

        // Событие загрузки окна
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            LoadProcess();
            LoadApplications();
        }

        // Запуск нового процесса
        private void StartNewTask_Click(object sender, RoutedEventArgs e)
        {

            var dialog = new TaskStartDialog();
            if (dialog.ShowDialog() == true)
            {
                try
                {
                    Process.Start(dialog.ProcessName);
                    LoadProcess();
                }
                catch (Exception ex)
                {
                    System.Windows.Forms.MessageBox.Show($"Ошибка: {ex.Message}");
                }
            }

        }

        // Закрытие приложения
        private void Exit_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }

        // Завершение дерева процессов
        private void KillProccessTree_Click(object sender, RoutedEventArgs e)
        {
            if (ViewProcList.SelectedItem is ProcessInfo selectedProcess)
            {
                try
                {
                    var process = Process.GetProcessById(selectedProcess.Id);
                    foreach (var child in Process.GetProcesses().Where(p => p.SessionId == process.SessionId))
                    {
                        try 
                        {
                            child.Kill(); 
                        } 
                        catch { }
                    }
                    process.Kill();
                    LoadProcess();
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
            }
        }

        // Завершение процесса
        private void KillProccess_Click(object sender, RoutedEventArgs e)
        {
            if (ViewProcList.SelectedItem is ProcessInfo selectedProcess)
            {
                try
                {
                    Process.GetProcessById(selectedProcess.Id)?.Kill();
                    LoadProcess();
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
            }
        }

        // Обновление списка процессов и приложений
        private void Refresh_Click(object sender, RoutedEventArgs e)
        {
            LoadProcess();
            LoadApplications();
        }

        private void ViewProcList_ContextMenuOpening(object sender, System.Windows.Controls.ContextMenuEventArgs e)
        {
            if (ViewProcList.SelectedItems != null)
            {
                e.Handled = true;
            }
        }
    }
}
