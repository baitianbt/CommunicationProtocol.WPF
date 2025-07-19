using Prism.Commands;
using Prism.Mvvm;
using System;
using System.Collections.ObjectModel;
using System.Windows;
using CommunicationProtocol.WPF.Services.LoggingService;

namespace CommunicationProtocol.WPF.ViewModels
{
    /// <summary>
    /// 西门子S7视图模型
    /// </summary>
    public class SiemensViewModel : BindableBase
    {
        private readonly ILoggingService _loggingService;

        #region 连接参数属性
        
        private ObservableCollection<string> _plcTypes;
        /// <summary>
        /// PLC类型列表
        /// </summary>
        public ObservableCollection<string> PlcTypes
        {
            get => _plcTypes;
            set => SetProperty(ref _plcTypes, value);
        }
        
        private string _selectedPlcType = "S7-1200/1500";
        /// <summary>
        /// 选中的PLC类型
        /// </summary>
        public string SelectedPlcType
        {
            get => _selectedPlcType;
            set => SetProperty(ref _selectedPlcType, value);
        }
        
        private string _ipAddress = "192.168.0.1";
        /// <summary>
        /// IP地址
        /// </summary>
        public string IpAddress
        {
            get => _ipAddress;
            set => SetProperty(ref _ipAddress, value);
        }
        
        private byte _rack = 0;
        /// <summary>
        /// 机架号
        /// </summary>
        public byte Rack
        {
            get => _rack;
            set => SetProperty(ref _rack, value);
        }
        
        private byte _slot = 1;
        /// <summary>
        /// 槽号
        /// </summary>
        public byte Slot
        {
            get => _slot;
            set => SetProperty(ref _slot, value);
        }
        
        private int _timeout = 1000;
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
        
        private string _variableAddress;
        /// <summary>
        /// 变量地址
        /// </summary>
        public string VariableAddress
        {
            get => _variableAddress;
            set => SetProperty(ref _variableAddress, value);
        }
        
        private int _length = 1;
        /// <summary>
        /// 数据长度
        /// </summary>
        public int Length
        {
            get => _length;
            set => SetProperty(ref _length, value);
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
        
        private string _selectedDataType = "Int";
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
        public SiemensViewModel(ILoggingService loggingService)
        {
            _loggingService = loggingService;
            
            InitializeCollections();
            InitializeCommands();
            
            _loggingService.Info("西门子S7视图模型已初始化");
        }
        
        /// <summary>
        /// 初始化集合
        /// </summary>
        private void InitializeCollections()
        {
            // 初始化PLC类型列表
            PlcTypes = new ObservableCollection<string>
            {
                "S7-200",
                "S7-300",
                "S7-400",
                "S7-1200/1500",
                "S7-200 Smart"
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
                "Bit",
                "Byte",
                "Int",
                "DInt",
                "Word",
                "DWord",
                "Real",
                "String",
                "Timer",
                "Counter"
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
                _loggingService.Info("开始连接西门子S7设备...");
                _loggingService.Info($"S7连接参数: 类型={SelectedPlcType}, IP={IpAddress}, 机架={Rack}, 槽位={Slot}, 超时={Timeout}ms");
                
                // 模拟连接延迟
                System.Threading.Thread.Sleep(500);
                
                // 连接成功
                IsConnected = true;
                
                // 记录日志
                AddLog("连接成功", "系统");
                _loggingService.Info("西门子S7设备连接成功");
            }
            catch (Exception ex)
            {
                // 连接失败
                IsConnected = false;
                
                // 记录日志
                AddLog($"连接失败: {ex.Message}", "错误");
                _loggingService.Error(ex, "西门子S7设备连接失败");
                
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
                _loggingService.Info("断开西门子S7设备连接...");
                
                // 模拟断开延迟
                System.Threading.Thread.Sleep(200);
                
                // 断开成功
                IsConnected = false;
                
                // 记录日志
                AddLog("断开连接", "系统");
                _loggingService.Info("西门子S7设备断开成功");
            }
            catch (Exception ex)
            {
                // 记录日志
                AddLog($"断开连接失败: {ex.Message}", "错误");
                _loggingService.Error(ex, "西门子S7设备断开失败");
                
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
                
                if (string.IsNullOrWhiteSpace(VariableAddress))
                {
                    MessageBox.Show("请输入变量地址", "提示", MessageBoxButton.OK, MessageBoxImage.Information);
                    return;
                }
                
                // 记录读取信息
                AddLog($"开始读取数据: 地址={VariableAddress}, 类型={SelectedDataType}, 长度={Length}", "发送");
                
                // 这里应该添加实际的读取代码，这里只是模拟
                System.Threading.Thread.Sleep(300);
                
                // 模拟生成数据
                string result = string.Empty;
                Random random = new Random();
                
                // 根据不同的数据类型生成不同的结果
                switch (SelectedDataType)
                {
                    case "Bit":
                        result = random.Next(2) == 1 ? "1" : "0";
                        break;
                    case "Byte":
                        result = random.Next(0, 255).ToString();
                        break;
                    case "Int":
                        result = random.Next(-32768, 32767).ToString();
                        break;
                    case "DInt":
                        result = random.Next(-2147483647, 2147483647).ToString();
                        break;
                    case "Word":
                        result = random.Next(0, 65535).ToString();
                        break;
                    case "DWord":
                        result = random.Next(0, 2147483647).ToString();
                        break;
                    case "Real":
                        result = Math.Round(random.NextDouble() * 100, 2).ToString();
                        break;
                    case "String":
                        char[] chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789".ToCharArray();
                        result = "";
                        for (int i = 0; i < Length; i++)
                        {
                            result += chars[random.Next(chars.Length)];
                        }
                        break;
                    case "Timer":
                        result = $"T#{random.Next(0, 59)}s_{random.Next(0, 999)}ms";
                        break;
                    case "Counter":
                        result = $"C#{random.Next(0, 999)}";
                        break;
                    default:
                        result = random.Next(0, 100).ToString();
                        break;
                }
                
                // 更新数据值
                DataValue = result;
                
                // 记录日志
                AddLog($"读取成功，结果: {result}", "接收");
                _loggingService.Info($"西门子S7读取成功: 地址={VariableAddress}, 类型={SelectedDataType}, 长度={Length}");
            }
            catch (Exception ex)
            {
                // 记录日志
                AddLog($"读取失败: {ex.Message}", "错误");
                _loggingService.Error(ex, "西门子S7读取失败");
                
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
                
                if (string.IsNullOrWhiteSpace(VariableAddress))
                {
                    MessageBox.Show("请输入变量地址", "提示", MessageBoxButton.OK, MessageBoxImage.Information);
                    return;
                }
                
                if (string.IsNullOrWhiteSpace(DataValue))
                {
                    MessageBox.Show("请输入要写入的数据", "提示", MessageBoxButton.OK, MessageBoxImage.Information);
                    return;
                }
                
                // 记录写入信息
                AddLog($"开始写入数据: 地址={VariableAddress}, 类型={SelectedDataType}, 值={DataValue}", "发送");
                
                // 这里应该添加实际的写入代码，这里只是模拟
                System.Threading.Thread.Sleep(300);
                
                // 记录日志
                AddLog("写入成功", "接收");
                _loggingService.Info($"西门子S7写入成功: 地址={VariableAddress}, 类型={SelectedDataType}, 值={DataValue}");
            }
            catch (Exception ex)
            {
                // 记录日志
                AddLog($"写入失败: {ex.Message}", "错误");
                _loggingService.Error(ex, "西门子S7写入失败");
                
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
                string logFileName = $"SiemensLog_{DateTime.Now:yyyyMMdd_HHmmss}.txt";
                string logFilePath = System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), logFileName);
                
                // 保存日志内容
                System.IO.File.WriteAllText(logFilePath, LogContent);
                
                // 记录日志
                _loggingService.Info($"西门子S7日志已保存至: {logFilePath}");
                
                // 显示成功消息
                MessageBox.Show($"日志已保存至: {logFilePath}", "成功", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                // 记录日志
                _loggingService.Error(ex, "保存西门子S7日志失败");
                
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
            _loggingService.Info("西门子S7日志已清空");
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
                _loggingService.Error($"西门子S7错误: {message}");
            }
            else
            {
                _loggingService.Debug($"西门子S7: {message}");
            }
        }
    }
} 