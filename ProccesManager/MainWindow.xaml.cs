using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Security.Principal;
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
            // Если не админ – перезапускаем приложение с правами администратора
            if (!IsAdministrator())
            {
                try
                {
                    ProcessStartInfo proc = new ProcessStartInfo
                    {
                        UseShellExecute = true,
                        WorkingDirectory = Environment.CurrentDirectory,
                        FileName = Process.GetCurrentProcess().MainModule.FileName,
                        Verb = "runas" // Запрос прав админа
                    };
                    Process.Start(proc);
                }
                catch (Exception)
                {
                    // Если пользователь отказался или произошла ошибка – закрываем приложение
                    Application.Current.Shutdown();
                }
                Environment.Exit(0);
            }

            InitializeComponent();
            DataContext = this;
        }

        // Исправление 1: Используем using для WindowsIdentity, т.к. он реализует IDisposable
        private bool IsAdministrator()
        {
            using (WindowsIdentity identity = WindowsIdentity.GetCurrent())
            {
                WindowsPrincipal principal = new WindowsPrincipal(identity);
                return principal.IsInRole(WindowsBuiltInRole.Administrator);
            }
        }

        // Исправление 2: Освобождаем каждый объект Process после использования
        private void LoadProcess()
        {
            Processes.Clear();
            Process[] processList = Process.GetProcesses();
            foreach (var proc in processList.OrderBy(p => p.ProcessName))
            {
                try
                {
                    // Оборачиваем процесс в using, чтобы он корректно освободился после чтения свойств
                    using (proc)
                    {
                        Processes.Add(new ProcessInfo
                        {
                            ProcessName = proc.ProcessName,
                            Id = proc.Id,
                            MemoryUsage = Math.Round(proc.WorkingSet64 / 1024 / 1024.0, 2)
                        });
                    }
                }
                catch (Exception ex)
                {
                    System.Windows.Forms.MessageBox.Show(ex.Message);
                }
            }
            StatusTextBlock.Text = $"Запущено процессов: {Processes.Count}";
        }

        // Исправление 3: Тоже освобождаем объекты Process при загрузке приложений
        private void LoadApplications()
        {
            Applications.Clear();
            Process[] processList = Process.GetProcesses();
            foreach (var proc in processList)
            {
                try
                {
                    using (proc)
                    {
                        if (!string.IsNullOrWhiteSpace(proc.MainWindowTitle))
                        {
                            Applications.Add(new AppInfo
                            {
                                AppName = proc.MainWindowTitle,
                                StartTime = proc.StartTime.ToString("HH:mm:ss")
                            });
                        }
                    }
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

        // Исправление 4: Оборачиваем Process.Start в using, чтобы освобождать возвращаемый объект
        private void StartNewTask_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new TaskStartDialog();
            if (dialog.ShowDialog() == true)
            {
                try
                {
                    var newProc = Process.Start(dialog.ProcessName);
                    if (newProc != null)
                    {
                        using (newProc)
                        {
                            // Запускаем процесс и тут же освобождаем объект
                        }
                    }
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

        // Исправление 5: В методе KillProccessTree освобождаем все объекты Process
        private void KillProccessTree_Click(object sender, RoutedEventArgs e)
        {
            if (ViewProcList.SelectedItem is ProcessInfo selectedProcess)
            {
                try
                {
                    using (var proc = Process.GetProcessById(selectedProcess.Id))
                    {
                        // Получаем список процессов для завершения
                        Process[] allProcesses = Process.GetProcesses();
                        foreach (var child in allProcesses.Where(p => p.SessionId == proc.SessionId))
                        {
                            try
                            {
                                using (child)
                                {
                                    child.Kill();
                                }
                            }
                            catch { }
                        }
                        proc.Kill();
                    }
                    LoadProcess();
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
            }
        }

        // Исправление 6: Освобождаем объект Process после завершения процесса
        private void KillProccess_Click(object sender, RoutedEventArgs e)
        {
            if (ViewProcList.SelectedItem is ProcessInfo selectedProcess)
            {
                try
                {
                    using (var proc = Process.GetProcessById(selectedProcess.Id))
                    {
                        proc?.Kill();
                    }
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
