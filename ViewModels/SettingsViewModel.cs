using CommunicationProtocol.WPF.Services.LoggingService;
using Prism.Commands;
using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Windows;
using System.Windows.Input;

namespace CommunicationProtocol.WPF.ViewModels
{
    /// <summary>
    /// 设置视图模型
    /// </summary>
    public class SettingsViewModel : BindableBase
    {
        private readonly ILoggingService _loggingService;

        #region 主题设置属性

        private bool _isLightTheme = true;
        /// <summary>
        /// 是否为亮色主题
        /// </summary>
        public bool IsLightTheme
        {
            get => _isLightTheme;
            set
            {
                SetProperty(ref _isLightTheme, value);
                if (value)
                {
                    IsDarkTheme = false;
                }
            }
        }

        private bool _isDarkTheme;
        /// <summary>
        /// 是否为暗色主题
        /// </summary>
        public bool IsDarkTheme
        {
            get => _isDarkTheme;
            set
            {
                SetProperty(ref _isDarkTheme, value);
                if (value)
                {
                    IsLightTheme = false;
                }
            }
        }

        private ObservableCollection<string> _primaryColors;
        /// <summary>
        /// 主色调列表
        /// </summary>
        public ObservableCollection<string> PrimaryColors
        {
            get => _primaryColors;
            set => SetProperty(ref _primaryColors, value);
        }

        private string _selectedPrimaryColor = "DeepPurple";
        /// <summary>
        /// 选中的主色调
        /// </summary>
        public string SelectedPrimaryColor
        {
            get => _selectedPrimaryColor;
            set => SetProperty(ref _selectedPrimaryColor, value);
        }

        private ObservableCollection<string> _accentColors;
        /// <summary>
        /// 强调色列表
        /// </summary>
        public ObservableCollection<string> AccentColors
        {
            get => _accentColors;
            set => SetProperty(ref _accentColors, value);
        }

        private string _selectedAccentColor = "Lime";
        /// <summary>
        /// 选中的强调色
        /// </summary>
        public string SelectedAccentColor
        {
            get => _selectedAccentColor;
            set => SetProperty(ref _selectedAccentColor, value);
        }

        #endregion

        #region 语言设置属性

        private ObservableCollection<string> _languages;
        /// <summary>
        /// 语言列表
        /// </summary>
        public ObservableCollection<string> Languages
        {
            get => _languages;
            set => SetProperty(ref _languages, value);
        }

        private string _selectedLanguage = "中文";
        /// <summary>
        /// 选中的语言
        /// </summary>
        public string SelectedLanguage
        {
            get => _selectedLanguage;
            set => SetProperty(ref _selectedLanguage, value);
        }

        #endregion

        #region 启动设置属性

        private bool _autoConnectLastDevice;
        /// <summary>
        /// 是否自动连接上次的设备
        /// </summary>
        public bool AutoConnectLastDevice
        {
            get => _autoConnectLastDevice;
            set => SetProperty(ref _autoConnectLastDevice, value);
        }

        private bool _checkUpdateOnStartup = true;
        /// <summary>
        /// 是否启动时自动检查更新
        /// </summary>
        public bool CheckUpdateOnStartup
        {
            get => _checkUpdateOnStartup;
            set => SetProperty(ref _checkUpdateOnStartup, value);
        }

        #endregion

        #region 数据导出设置属性

        private string _defaultExportPath;
        /// <summary>
        /// 默认导出路径
        /// </summary>
        public string DefaultExportPath
        {
            get => _defaultExportPath;
            set => SetProperty(ref _defaultExportPath, value);
        }

        private ObservableCollection<string> _exportFormats;
        /// <summary>
        /// 导出格式列表
        /// </summary>
        public ObservableCollection<string> ExportFormats
        {
            get => _exportFormats;
            set => SetProperty(ref _exportFormats, value);
        }

        private string _selectedExportFormat = "CSV";
        /// <summary>
        /// 选中的导出格式
        /// </summary>
        public string SelectedExportFormat
        {
            get => _selectedExportFormat;
            set => SetProperty(ref _selectedExportFormat, value);
        }

        #endregion

        #region 日志设置属性

        private ObservableCollection<string> _logLevels;
        /// <summary>
        /// 日志级别列表
        /// </summary>
        public ObservableCollection<string> LogLevels
        {
            get => _logLevels;
            set => SetProperty(ref _logLevels, value);
        }

        private string _selectedAppLogLevel = "信息";
        /// <summary>
        /// 选中的应用程序日志级别
        /// </summary>
        public string SelectedAppLogLevel
        {
            get => _selectedAppLogLevel;
            set => SetProperty(ref _selectedAppLogLevel, value);
        }

        private string _selectedCommLogLevel = "调试";
        /// <summary>
        /// 选中的通讯日志级别
        /// </summary>
        public string SelectedCommLogLevel
        {
            get => _selectedCommLogLevel;
            set => SetProperty(ref _selectedCommLogLevel, value);
        }

        private string _logFilePath;
        /// <summary>
        /// 日志文件路径
        /// </summary>
        public string LogFilePath
        {
            get => _logFilePath;
            set => SetProperty(ref _logFilePath, value);
        }

        private int _maxLogDays = 30;
        /// <summary>
        /// 最大日志保留天数
        /// </summary>
        public int MaxLogDays
        {
            get => _maxLogDays;
            set => SetProperty(ref _maxLogDays, value);
        }

        private int _maxLogFileSize = 10;
        /// <summary>
        /// 单个日志文件最大大小(MB)
        /// </summary>
        public int MaxLogFileSize
        {
            get => _maxLogFileSize;
            set => SetProperty(ref _maxLogFileSize, value);
        }

        #endregion

        #region 关于信息属性

        private string _versionInfo = "版本 1.0.0";
        /// <summary>
        /// 版本信息
        /// </summary>
        public string VersionInfo
        {
            get => _versionInfo;
            set => SetProperty(ref _versionInfo, value);
        }

        /// <summary>
        /// 开源库信息项
        /// </summary>
        public class OpenSourceLibraryInfo
        {
            /// <summary>
            /// 名称
            /// </summary>
            public string Name { get; set; }

            /// <summary>
            /// 版本
            /// </summary>
            public string Version { get; set; }

            /// <summary>
            /// 许可证
            /// </summary>
            public string License { get; set; }

            /// <summary>
            /// 项目URL
            /// </summary>
            public string Url { get; set; }
        }

        private ObservableCollection<OpenSourceLibraryInfo> _openSourceLibraries;
        /// <summary>
        /// 开源库信息列表
        /// </summary>
        public ObservableCollection<OpenSourceLibraryInfo> OpenSourceLibraries
        {
            get => _openSourceLibraries;
            set => SetProperty(ref _openSourceLibraries, value);
        }

        #endregion

        #region 命令

        /// <summary>
        /// 浏览导出路径命令
        /// </summary>
        public DelegateCommand BrowseExportPathCommand { get; private set; }

        /// <summary>
        /// 浏览日志路径命令
        /// </summary>
        public DelegateCommand BrowseLogPathCommand { get; private set; }

        /// <summary>
        /// 打开日志
        /// </summary>
        public DelegateCommand OpenLogFolderCommand { get; private set; }

        /// <summary>
        /// 清空日志文件命令
        /// </summary>
        public DelegateCommand ClearAllLogsCommand { get; private set; }

        /// <summary>
        /// 检查更新命令
        /// </summary>
        public DelegateCommand CheckUpdateCommand { get; private set; }

        /// <summary>
        /// 打开库URL命令
        /// </summary>
        public DelegateCommand<string> OpenLibraryUrlCommand { get; private set; }

        #endregion

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="loggingService">日志服务</param>
        public SettingsViewModel(ILoggingService loggingService)
        {
            _loggingService = loggingService;

            InitializeCollections();
            InitializeCommands();

            // 设置默认路径
            DefaultExportPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            LogFilePath = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Logs");

            _loggingService.Info("设置视图模型已初始化");
        }

        /// <summary>
        /// 初始化集合
        /// </summary>
        private void InitializeCollections()
        {
            // 初始化主色调列表
            PrimaryColors = new ObservableCollection<string>
            {
                "Red",
                "Pink",
                "Purple",
                "DeepPurple",
                "Indigo",
                "Blue",
                "LightBlue",
                "Cyan",
                "Teal",
                "Green",
                "LightGreen",
                "Lime",
                "Yellow",
                "Amber",
                "Orange",
                "DeepOrange",
                "Brown",
                "Grey",
                "BlueGrey"
            };

            // 初始化强调色列表
            AccentColors = new ObservableCollection<string>
            {
                "Red",
                "Pink",
                "Purple",
                "DeepPurple",
                "Indigo",
                "Blue",
                "LightBlue",
                "Cyan",
                "Teal",
                "Green",
                "LightGreen",
                "Lime",
                "Yellow",
                "Amber",
                "Orange",
                "DeepOrange"
            };

            // 初始化语言列表
            Languages = new ObservableCollection<string>
            {
                "中文",
                "English",
                "日本語",
                "Deutsch",
                "Français",
                "Español"
            };

            // 初始化导出格式列表
            ExportFormats = new ObservableCollection<string>
            {
                "CSV",
                "Excel",
                "JSON",
                "XML",
                "TXT"
            };

            // 初始化日志级别列表
            LogLevels = new ObservableCollection<string>
            {
                "调试",
                "信息",
                "警告",
                "错误",
                "致命"
            };

            // 初始化开源库信息列表
            OpenSourceLibraries = new ObservableCollection<OpenSourceLibraryInfo>
            {
                new OpenSourceLibraryInfo
                {
                    Name = "MaterialDesignThemes",
                    Version = "4.9.0",
                    License = "MIT License",
                    Url = "https://github.com/MaterialDesignInXAML/MaterialDesignInXamlToolkit"
                },
                new OpenSourceLibraryInfo
                {
                    Name = "Prism",
                    Version = "8.1.97",
                    License = "MIT License",
                    Url = "https://github.com/PrismLibrary/Prism"
                },
                new OpenSourceLibraryInfo
                {
                    Name = "NLog",
                    Version = "5.2.7",
                    License = "BSD 3-Clause License",
                    Url = "https://github.com/NLog/NLog"
                },
                new OpenSourceLibraryInfo
                {
                    Name = "LiveCharts",
                    Version = "0.9.7",
                    License = "MIT License",
                    Url = "https://github.com/beto-rodriguez/LiveCharts"
                },
                new OpenSourceLibraryInfo
                {
                    Name = "S7NetPlus",
                    Version = "0.20.0",
                    License = "MIT License",
                    Url = "https://github.com/S7NetPlus/s7netplus"
                },
                new OpenSourceLibraryInfo
                {
                    Name = "NModbus",
                    Version = "3.0.78",
                    License = "MIT License",
                    Url = "https://github.com/NModbus/NModbus"
                },
                new OpenSourceLibraryInfo
                {
                    Name = "OPC UA SDK",
                    Version = "1.4.372.56",
                    License = "OPC Foundation License",
                    Url = "https://github.com/OPCFoundation/UA-.NETStandard"
                }
            };
        }

        /// <summary>
        /// 初始化命令
        /// </summary>
        private void InitializeCommands()
        {
            BrowseExportPathCommand = new DelegateCommand(BrowseExportPath);
            BrowseLogPathCommand = new DelegateCommand(BrowseLogPath);
            OpenLogFolderCommand = new DelegateCommand(OpenLogFolder);
            ClearAllLogsCommand = new DelegateCommand(ClearAllLogs);
            CheckUpdateCommand = new DelegateCommand(CheckUpdate);
            OpenLibraryUrlCommand = new DelegateCommand<string>(OpenLibraryUrl);
        }

        /// <summary>
        /// 浏览导出路径
        /// </summary>
        private void BrowseExportPath()
        {
            try
            {
                // 这里应该添加实际的文件夹选择对话框代码，这里只是模拟
                _loggingService.Info("浏览导出路径...");

                // 模拟选择文件夹
                DefaultExportPath = System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "Exports");

                // 记录日志
                _loggingService.Info($"已选择导出路径: {DefaultExportPath}");
            }
            catch (Exception ex)
            {
                // 记录日志
                _loggingService.Error(ex, "选择导出路径失败");

                // 显示错误消息
                MessageBox.Show($"选择导出路径失败: {ex.Message}", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// 浏览日志路径
        /// </summary>
        private void BrowseLogPath()
        {
            try
            {
                // 这里应该添加实际的文件夹选择对话框代码，这里只是模拟
                _loggingService.Info("浏览日志路径...");

                // 模拟选择文件夹
                LogFilePath = System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "CommunicationProtocol", "Logs");

                // 记录日志
                _loggingService.Info($"已选择日志路径: {LogFilePath}");
            }
            catch (Exception ex)
            {
                // 记录日志
                _loggingService.Error(ex, "选择日志路径失败");

                // 显示错误消息
                MessageBox.Show($"选择日志路径失败: {ex.Message}", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// 打开日志文件夹
        /// </summary>
        private void OpenLogFolder()
        {
            try
            {
                // 检查文件夹是否存在
                if (!System.IO.Directory.Exists(LogFilePath))
                {
                    // 创建文件夹
                    System.IO.Directory.CreateDirectory(LogFilePath);
                }

                // 打开文件夹
                Process.Start("explorer.exe", LogFilePath);

                // 记录日志
                _loggingService.Info($"已打开日志文件夹: {LogFilePath}");
            }
            catch (Exception ex)
            {
                // 记录日志
                _loggingService.Error(ex, "打开日志文件夹失败");

                // 显示错误消息
                MessageBox.Show($"打开日志文件夹失败: {ex.Message}", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// 清空日志文件
        /// </summary>
        private void ClearAllLogs()
        {
            try
            {
                // 弹出确认对话框
                MessageBoxResult result = MessageBox.Show("确定要清空所有日志文件吗？此操作不可恢复。", "确认", MessageBoxButton.YesNo, MessageBoxImage.Warning);

                if (result == MessageBoxResult.Yes)
                {
                    // 这里应该添加实际的清空日志文件代码，这里只是模拟
                    _loggingService.Info("开始清空日志文件...");

                    // 模拟清空延迟
                    System.Threading.Thread.Sleep(500);

                    // 记录日志
                    _loggingService.Info("日志文件已清空");

                    // 显示成功消息
                    MessageBox.Show("日志文件已清空", "成功", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                // 记录日志
                _loggingService.Error(ex, "清空日志文件失败");

                // 显示错误消息
                MessageBox.Show($"清空日志文件失败: {ex.Message}", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// 检查更新
        /// </summary>
        private void CheckUpdate()
        {
            try
            {
                // 这里应该添加实际的检查更新代码，这里只是模拟
                _loggingService.Info("开始检查更新...");

                // 模拟检查延迟
                System.Threading.Thread.Sleep(1000);

                // 记录日志
                _loggingService.Info("已是最新版本");

                // 显示消息
                MessageBox.Show("您的软件已是最新版本", "检查更新", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                // 记录日志
                _loggingService.Error(ex, "检查更新失败");

                // 显示错误消息
                MessageBox.Show($"检查更新失败: {ex.Message}", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// 打开库URL
        /// </summary>
        private void OpenLibraryUrl(string url)
        {
            try
            {
                // 打开URL
                Process.Start(new ProcessStartInfo
                {
                    FileName = url,
                    UseShellExecute = true
                });

                // 记录日志
                _loggingService.Info($"已打开URL: {url}");
            }
            catch (Exception ex)
            {
                // 记录日志
                _loggingService.Error(ex, $"打开URL失败: {url}");

                // 显示错误消息
                MessageBox.Show($"打开URL失败: {ex.Message}", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
