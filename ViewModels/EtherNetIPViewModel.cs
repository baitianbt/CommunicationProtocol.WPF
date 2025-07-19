using Prism.Commands;
using Prism.Mvvm;
using System;
using System.Collections.ObjectModel;
using System.Windows;
using CommunicationProtocol.WPF.Services.LoggingService;

namespace CommunicationProtocol.WPF.ViewModels
{
    /// <summary>
    /// EtherNet/IP视图模型
    /// </summary>
    public class EtherNetIPViewModel : BindableBase
    {
        private readonly ILoggingService _loggingService;

        #region 连接参数属性

        private ObservableCollection<string> _deviceTypes;
        /// <summary>
        /// 设备类型列表
        /// </summary>
        public ObservableCollection<string> DeviceTypes
        {
            get => _deviceTypes;
            set => SetProperty(ref _deviceTypes, value);
        }

        private string _selectedDeviceType = "ControlLogix";
        /// <summary>
        /// 选中的设备类型
        /// </summary>
        public string SelectedDeviceType
        {
            get => _selectedDeviceType;
            set => SetProperty(ref _selectedDeviceType, value);
        }

        private string _ipAddress = "192.168.1.100";
        /// <summary>
        /// IP地址
        /// </summary>
        public string IpAddress
        {
            get => _ipAddress;
            set => SetProperty(ref _ipAddress, value);
        }

        private int _port = 44818;
        /// <summary>
        /// 端口号
        /// </summary>
        public int Port
        {
            get => _port;
            set => SetProperty(ref _port, value);
        }

        private string _path = "1,0";
        /// <summary>
        /// 通信路径
        /// </summary>
        public string Path
        {
            get => _path;
            set => SetProperty(ref _path, value);
        }

        private int _timeout = 3000;
        /// <summary>
        /// 超时时间(ms)
        /// </summary>
        public int Timeout
        {
            get => _timeout;
            set => SetProperty(ref _timeout, value);
        }

        #endregion

        #region 连接状态属性

        private bool _isConnected;
        /// <summary>
        /// 是否已连接
        /// </summary>
        public bool IsConnected
        {
            get => _isConnected;
            set
            {
                SetProperty(ref _isConnected, value);
                RaisePropertyChanged(nameof(ConnectionStatus));
                RaisePropertyChanged(nameof(ConnectionStatusStyle));
                RaisePropertyChanged(nameof(ConnectButtonVisibility));
                RaisePropertyChanged(nameof(DisconnectButtonVisibility));
                RaisePropertyChanged(nameof(CanWrite));
            }
        }

        /// <summary>
        /// 连接状态文本
        /// </summary>
        public string ConnectionStatus => IsConnected ? "已连接" : "未连接";

        /// <summary>
        /// 连接状态样式
        /// </summary>
        public string ConnectionStatusStyle => IsConnected ? "ConnectedTextBlockStyle" : "DisconnectedTextBlockStyle";

        /// <summary>
        /// 连接按钮可见性
        /// </summary>
        public Visibility ConnectButtonVisibility => IsConnected ? Visibility.Collapsed : Visibility.Visible;

        /// <summary>
        /// 断开按钮可见性
        /// </summary>
        public Visibility DisconnectButtonVisibility => IsConnected ? Visibility.Visible : Visibility.Collapsed;

        #endregion

        #region 数据操作属性

        private ObservableCollection<string> _operationTypes;
        /// <summary>
        /// 操作类型列表
        /// </summary>
        public ObservableCollection<string> OperationTypes
        {
            get => _operationTypes;
            set => SetProperty(ref _operationTypes, value);
        }

        private string _selectedOperationType = "读数据";
        /// <summary>
        /// 选中的操作类型
        /// </summary>
        public string SelectedOperationType
        {
            get => _selectedOperationType;
            set
            {
                SetProperty(ref _selectedOperationType, value);
                IsReadOperation = value == "读数据";
                RaisePropertyChanged(nameof(CanWrite));
            }
        }

        private string _tagName;
        /// <summary>
        /// 标签名称
        /// </summary>
        public string TagName
        {
            get => _tagName;
            set => SetProperty(ref _tagName, value);
        }

        private int _elementCount = 1;
        /// <summary>
        /// 元素数量
        /// </summary>
        public int ElementCount
        {
            get => _elementCount;
            set => SetProperty(ref _elementCount, value);
        }

        private ObservableCollection<string> _dataTypes;
        /// <summary>
        /// 数据类型列表
        /// </summary>
        public ObservableCollection<string> DataTypes
        {
            get => _dataTypes;
            set => SetProperty(ref _dataTypes, value);
        }

        private string _selectedDataType = "DINT";
        /// <summary>
        /// 选中的数据类型
        /// </summary>
        public string SelectedDataType
        {
            get => _selectedDataType;
            set => SetProperty(ref _selectedDataType, value);
        }

        private bool _isReadOperation = true;
        /// <summary>
        /// 是否为读操作
        /// </summary>
        public bool IsReadOperation
        {
            get => _isReadOperation;
            set => SetProperty(ref _isReadOperation, value);
        }

        /// <summary>
        /// 是否可以写入
        /// </summary>
        public bool CanWrite => IsConnected && !IsReadOperation;

        private string _dataValue;
        /// <summary>
        /// 数据值
        /// </summary>
        public string DataValue
        {
            get => _dataValue;
            set => SetProperty(ref _dataValue, value);
        }

        #endregion

        #region 日志属性

        private bool _autoScroll = true;
        /// <summary>
        /// 是否自动滚动日志
        /// </summary>
        public bool AutoScroll
        {
            get => _autoScroll;
            set => SetProperty(ref _autoScroll, value);
        }

        private bool _showTimestamp = true;
        /// <summary>
        /// 是否显示时间戳
        /// </summary>
        public bool ShowTimestamp
        {
            get => _showTimestamp;
            set => SetProperty(ref _showTimestamp, value);
        }

        private string _logContent = string.Empty;
        /// <summary>
        /// 日志内容
        /// </summary>
        public string LogContent
        {
            get => _logContent;
            set => SetProperty(ref _logContent, value);
        }

        #endregion

        #region 命令

        /// <summary>
        /// 连接命令
        /// </summary>
        public DelegateCommand ConnectCommand { get; private set; }

        /// <summary>
        /// 断开命令
        /// </summary>
        public DelegateCommand DisconnectCommand { get; private set; }

        /// <summary>
        /// 读取命令
        /// </summary>
        public DelegateCommand ReadCommand { get; private set; }

        /// <summary>
        /// 写入命令
        /// </summary>
        public DelegateCommand WriteCommand { get; private set; }

        /// <summary>
        /// 保存日志命令
        /// </summary>
        public DelegateCommand SaveLogCommand { get; private set; }

        /// <summary>
        /// 清空日志命令
        /// </summary>
        public DelegateCommand ClearLogCommand { get; private set; }

        #endregion

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="loggingService">日志服务</param>
        public EtherNetIPViewModel(ILoggingService loggingService)
        {
            _loggingService = loggingService;

            InitializeCollections();
            InitializeCommands();

            _loggingService.Info("EtherNet/IP视图模型已初始化");
        }

        /// <summary>
        /// 初始化集合
        /// </summary>
        private void InitializeCollections()
        {
            // 初始化设备类型列表
            DeviceTypes = new ObservableCollection<string>
            {
                "ControlLogix",
                "CompactLogix",
                "MicroLogix",
                "PLC-5",
                "SLC 500"
            };

            // 初始化操作类型列表
            OperationTypes = new ObservableCollection<string>
            {
                "读数据",
                "写数据"
            };

            // 初始化数据类型列表
            DataTypes = new ObservableCollection<string>
            {
                "BOOL",
                "SINT",
                "INT",
                "DINT",
                "REAL",
                "ARRAY_BOOL",
                "ARRAY_SINT",
                "ARRAY_INT",
                "ARRAY_DINT",
                "ARRAY_REAL",
                "STRING"
            };
        }

        /// <summary>
        /// 初始化命令
        /// </summary>
        private void InitializeCommands()
        {
            ConnectCommand = new DelegateCommand(Connect);
            DisconnectCommand = new DelegateCommand(Disconnect);
            ReadCommand = new DelegateCommand(Read);
            WriteCommand = new DelegateCommand(Write);
            SaveLogCommand = new DelegateCommand(SaveLog);
            ClearLogCommand = new DelegateCommand(ClearLog);
        }

        /// <summary>
        /// 连接设备
        /// </summary>
        private void Connect()
        {
            try
            {
                // 这里应该添加实际的连接代码，这里只是模拟
                _loggingService.Info("开始连接EtherNet/IP设备...");
                _loggingService.Info($"EtherNet/IP连接参数: 设备类型={SelectedDeviceType}, IP={IpAddress}, 端口={Port}, 通信路径={Path}, 超时={Timeout}ms");

                // 模拟连接延迟
                System.Threading.Thread.Sleep(500);

                // 连接成功
                IsConnected = true;

                // 记录日志
                AddLog("连接成功", "系统");
                _loggingService.Info("EtherNet/IP设备连接成功");
            }
            catch (Exception ex)
            {
                // 连接失败
                IsConnected = false;

                // 记录日志
                AddLog($"连接失败: {ex.Message}", "错误");
                _loggingService.Error(ex, "EtherNet/IP设备连接失败");

                // 显示错误消息
                MessageBox.Show($"连接失败: {ex.Message}", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// 断开连接
        /// </summary>
        private void Disconnect()
        {
            try
            {
                // 这里应该添加实际的断开连接代码，这里只是模拟
                _loggingService.Info("断开EtherNet/IP设备连接...");

                // 模拟断开延迟
                System.Threading.Thread.Sleep(200);

                // 断开成功
                IsConnected = false;

                // 记录日志
                AddLog("断开连接", "系统");
                _loggingService.Info("EtherNet/IP设备断开成功");
            }
            catch (Exception ex)
            {
                // 记录日志
                AddLog($"断开连接失败: {ex.Message}", "错误");
                _loggingService.Error(ex, "EtherNet/IP设备断开失败");

                // 显示错误消息
                MessageBox.Show($"断开连接失败: {ex.Message}", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// 读取数据
        /// </summary>
        private void Read()
        {
            try
            {
                if (!IsConnected)
                {
                    MessageBox.Show("请先连接设备", "提示", MessageBoxButton.OK, MessageBoxImage.Information);
                    return;
                }

                if (string.IsNullOrWhiteSpace(TagName))
                {
                    MessageBox.Show("请输入标签名称", "提示", MessageBoxButton.OK, MessageBoxImage.Information);
                    return;
                }

                // 记录读取信息
                AddLog($"开始读取标签: {TagName}, 类型={SelectedDataType}, 元素数量={ElementCount}", "发送");

                // 这里应该添加实际的读取代码，这里只是模拟
                System.Threading.Thread.Sleep(300);

                // 模拟生成数据
                string result = string.Empty;
                Random random = new Random();

                // 根据不同的数据类型生成不同的结果
                switch (SelectedDataType)
                {
                    case "BOOL":
                        result = random.Next(2) == 1 ? "True" : "False";
                        break;
                    case "SINT":
                        result = random.Next(-128, 127).ToString();
                        break;
                    case "INT":
                        result = random.Next(-32768, 32767).ToString();
                        break;
                    case "DINT":
                        result = random.Next(-2147483647, 2147483647).ToString();
                        break;
                    case "REAL":
                        result = Math.Round(random.NextDouble() * 1000, 2).ToString();
                        break;
                    case "ARRAY_BOOL":
                        for (int i = 0; i < ElementCount; i++)
                        {
                            result += $"[{i}]: {(random.Next(2) == 1 ? "True" : "False")}\r\n";
                        }
                        break;
                    case "ARRAY_SINT":
                        for (int i = 0; i < ElementCount; i++)
                        {
                            result += $"[{i}]: {random.Next(-128, 127)}\r\n";
                        }
                        break;
                    case "ARRAY_INT":
                        for (int i = 0; i < ElementCount; i++)
                        {
                            result += $"[{i}]: {random.Next(-32768, 32767)}\r\n";
                        }
                        break;
                    case "ARRAY_DINT":
                        for (int i = 0; i < ElementCount; i++)
                        {
                            result += $"[{i}]: {random.Next(-2147483647, 2147483647)}\r\n";
                        }
                        break;
                    case "ARRAY_REAL":
                        for (int i = 0; i < ElementCount; i++)
                        {
                            result += $"[{i}]: {Math.Round(random.NextDouble() * 1000, 2)}\r\n";
                        }
                        break;
                    case "STRING":
                        char[] chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789".ToCharArray();
                        int length = random.Next(5, 20);
                        for (int i = 0; i < length; i++)
                        {
                            result += chars[random.Next(chars.Length)];
                        }
                        break;
                    default:
                        result = "未知数据类型";
                        break;
                }

                // 更新数据值
                DataValue = result;

                // 记录日志
                AddLog($"读取成功，结果: {result}", "接收");
                _loggingService.Info($"EtherNet/IP读取成功: 标签={TagName}, 类型={SelectedDataType}");
            }
            catch (Exception ex)
            {
                // 记录日志
                AddLog($"读取失败: {ex.Message}", "错误");
                _loggingService.Error(ex, "EtherNet/IP读取失败");

                // 显示错误消息
                MessageBox.Show($"读取失败: {ex.Message}", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// 写入数据
        /// </summary>
        private void Write()
        {
            try
            {
                if (!IsConnected)
                {
                    MessageBox.Show("请先连接设备", "提示", MessageBoxButton.OK, MessageBoxImage.Information);
                    return;
                }

                if (string.IsNullOrWhiteSpace(TagName))
                {
                    MessageBox.Show("请输入标签名称", "提示", MessageBoxButton.OK, MessageBoxImage.Information);
                    return;
                }

                if (string.IsNullOrWhiteSpace(DataValue))
                {
                    MessageBox.Show("请输入要写入的数据", "提示", MessageBoxButton.OK, MessageBoxImage.Information);
                    return;
                }

                // 记录写入信息
                AddLog($"开始写入标签: {TagName}, 类型={SelectedDataType}, 值={DataValue}", "发送");

                // 这里应该添加实际的写入代码，这里只是模拟
                System.Threading.Thread.Sleep(300);

                // 记录日志
                AddLog("写入成功", "接收");
                _loggingService.Info($"EtherNet/IP写入成功: 标签={TagName}, 类型={SelectedDataType}, 值={DataValue}");
            }
            catch (Exception ex)
            {
                // 记录日志
                AddLog($"写入失败: {ex.Message}", "错误");
                _loggingService.Error(ex, "EtherNet/IP写入失败");

                // 显示错误消息
                MessageBox.Show($"写入失败: {ex.Message}", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// 保存日志
        /// </summary>
        private void SaveLog()
        {
            try
            {
                // 这里应该添加实际的保存日志代码，这里只是模拟
                string logFileName = $"EtherNetIPLog_{DateTime.Now:yyyyMMdd_HHmmss}.txt";
                string logFilePath = System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), logFileName);

                // 保存日志内容
                System.IO.File.WriteAllText(logFilePath, LogContent);

                // 记录日志
                _loggingService.Info($"EtherNet/IP日志已保存至: {logFilePath}");

                // 显示成功消息
                MessageBox.Show($"日志已保存至: {logFilePath}", "成功", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                // 记录日志
                _loggingService.Error(ex, "保存EtherNet/IP日志失败");

                // 显示错误消息
                MessageBox.Show($"保存日志失败: {ex.Message}", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// 清空日志
        /// </summary>
        private void ClearLog()
        {
            // 清空日志内容
            LogContent = string.Empty;

            // 记录日志
            _loggingService.Info("EtherNet/IP日志已清空");
        }

        /// <summary>
        /// 添加日志
        /// </summary>
        /// <param name="message">日志消息</param>
        /// <param name="type">日志类型</param>
        private void AddLog(string message, string type)
        {
            string logEntry = string.Empty;

            // 添加时间戳
            if (ShowTimestamp)
            {
                logEntry += $"[{DateTime.Now:HH:mm:ss.fff}] ";
            }

            // 添加类型
            if (!string.IsNullOrEmpty(type))
            {
                logEntry += $"[{type}] ";
            }

            // 添加消息
            logEntry += message + "\r\n\r\n";

            // 添加到日志内容
            LogContent += logEntry;

            // 记录到系统日志
            if (type == "错误")
            {
                _loggingService.Error($"EtherNet/IP错误: {message}");
            }
            else
            {
                _loggingService.Debug($"EtherNet/IP: {message}");
            }
        }
    }
}