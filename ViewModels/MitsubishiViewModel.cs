using CommunicationProtocol.WPF.Services.LoggingService;
using Prism.Commands;
using Prism.Mvvm;
using System;
using System.Collections.ObjectModel;
using System.Text;
using System.Windows;

namespace CommunicationProtocol.WPF.ViewModels
{
    /// <summary>
    /// 三菱MC协议视图模型
    /// </summary>
    public class MitsubishiViewModel : BindableBase
    {
        private readonly ILoggingService _loggingService;

        #region 连接参数属性

        private bool _isBinary = true;
        /// <summary>
        /// 是否为二进制格式
        /// </summary>
        public bool IsBinary
        {
            get => _isBinary;
            set
            {
                SetProperty(ref _isBinary, value);
                IsAscii = !value;
            }
        }

        private bool _isAscii;
        /// <summary>
        /// 是否为ASCII格式
        /// </summary>
        public bool IsAscii
        {
            get => _isAscii;
            set
            {
                SetProperty(ref _isAscii, value);
                IsBinary = !value;
            }
        }

        private ObservableCollection<string> _plcTypes;
        /// <summary>
        /// PLC型号列表
        /// </summary>
        public ObservableCollection<string> PlcTypes
        {
            get => _plcTypes;
            set => SetProperty(ref _plcTypes, value);
        }

        private string _selectedPlcType = "Q/L系列";
        /// <summary>
        /// 选中的PLC型号
        /// </summary>
        public string SelectedPlcType
        {
            get => _selectedPlcType;
            set => SetProperty(ref _selectedPlcType, value);
        }

        private string _ipAddress = "192.168.3.39";
        /// <summary>
        /// IP地址
        /// </summary>
        public string IpAddress
        {
            get => _ipAddress;
            set => SetProperty(ref _ipAddress, value);
        }

        private int _port = 1025;
        /// <summary>
        /// 端口号
        /// </summary>
        public int Port
        {
            get => _port;
            set => SetProperty(ref _port, value);
        }

        private byte _networkNumber = 0;
        /// <summary>
        /// 网络编号
        /// </summary>
        public byte NetworkNumber
        {
            get => _networkNumber;
            set => SetProperty(ref _networkNumber, value);
        }

        private byte _plcNumber = 0xFF;
        /// <summary>
        /// PLC编号
        /// </summary>
        public byte PlcNumber
        {
            get => _plcNumber;
            set => SetProperty(ref _plcNumber, value);
        }

        private ushort _moduleIONumber = 0;
        /// <summary>
        /// 模块IO编号
        /// </summary>
        public ushort ModuleIONumber
        {
            get => _moduleIONumber;
            set => SetProperty(ref _moduleIONumber, value);
        }

        private byte _stationNumber = 0;
        /// <summary>
        /// 站号
        /// </summary>
        public byte StationNumber
        {
            get => _stationNumber;
            set => SetProperty(ref _stationNumber, value);
        }

        private int _timeout = 5000;
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

        private string _deviceAddress;
        /// <summary>
        /// 设备地址
        /// </summary>
        public string DeviceAddress
        {
            get => _deviceAddress;
            set => SetProperty(ref _deviceAddress, value);
        }

        private ushort _points = 1;
        /// <summary>
        /// 点数
        /// </summary>
        public ushort Points
        {
            get => _points;
            set => SetProperty(ref _points, value);
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

        private string _selectedDataType = "Word";
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

        private bool _showHex;
        /// <summary>
        /// 是否显示十六进制
        /// </summary>
        public bool ShowHex
        {
            get => _showHex;
            set => SetProperty(ref _showHex, value);
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
        public MitsubishiViewModel(ILoggingService loggingService)
        {
            _loggingService = loggingService;

            InitializeCollections();
            InitializeCommands();

            _loggingService.Info("三菱MC协议视图模型已初始化");
        }

        /// <summary>
        /// 初始化集合
        /// </summary>
        private void InitializeCollections()
        {
            // 初始化PLC型号列表
            PlcTypes = new ObservableCollection<string>
            {
                "Q/L系列",
                "QnA系列",
                "iQ-R系列",
                "FX系列"
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
                "Word",
                "DWord",
                "Float",
                "String"
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
                _loggingService.Info("开始连接三菱PLC设备...");
                _loggingService.Info($"MC协议连接参数: 格式={(IsBinary ? "二进制(3E)" : "ASCII(1E)")}, PLC型号={SelectedPlcType}, IP={IpAddress}, 端口={Port}, 超时={Timeout}ms");

                // 模拟连接延迟
                System.Threading.Thread.Sleep(500);

                // 连接成功
                IsConnected = true;

                // 记录日志
                AddLog("连接成功", "系统");
                _loggingService.Info("三菱PLC设备连接成功");
            }
            catch (Exception ex)
            {
                // 连接失败
                IsConnected = false;

                // 记录日志
                AddLog($"连接失败: {ex.Message}", "错误");
                _loggingService.Error(ex, "三菱PLC设备连接失败");

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
                _loggingService.Info("断开三菱PLC设备连接...");

                // 模拟断开延迟
                System.Threading.Thread.Sleep(200);

                // 断开成功
                IsConnected = false;

                // 记录日志
                AddLog("断开连接", "系统");
                _loggingService.Info("三菱PLC设备断开成功");
            }
            catch (Exception ex)
            {
                // 记录日志
                AddLog($"断开连接失败: {ex.Message}", "错误");
                _loggingService.Error(ex, "三菱PLC设备断开失败");

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

                if (string.IsNullOrWhiteSpace(DeviceAddress))
                {
                    MessageBox.Show("请输入设备地址", "提示", MessageBoxButton.OK, MessageBoxImage.Information);
                    return;
                }

                // 记录读取信息
                AddLog($"开始读取设备地址: {DeviceAddress}, 点数={Points}, 类型={SelectedDataType}", "发送");

                // 这里应该添加实际的读取代码，这里只是模拟
                System.Threading.Thread.Sleep(300);

                // 模拟生成数据
                string result = string.Empty;
                Random random = new Random();

                // 根据不同的数据类型生成不同的结果
                switch (SelectedDataType)
                {
                    case "Bit":
                        for (int i = 0; i < Points; i++)
                        {
                            result += $"[{i}]: {(random.Next(2) == 1 ? "ON" : "OFF")}\r\n";
                        }
                        break;
                    case "Byte":
                        for (int i = 0; i < Points; i++)
                        {
                            byte value = (byte)random.Next(0, 256);
                            result += $"[{i}]: {value} (0x{value:X2})\r\n";
                        }
                        break;
                    case "Word":
                        for (int i = 0; i < Points; i++)
                        {
                            ushort value = (ushort)random.Next(0, 65536);
                            result += $"[{i}]: {value} (0x{value:X4})\r\n";
                        }
                        break;
                    case "DWord":
                        for (int i = 0; i < Points; i += 2)
                        {
                            uint value = (uint)random.Next(0, int.MaxValue);
                            result += $"[{i}-{i + 1}]: {value} (0x{value:X8})\r\n";
                        }
                        break;
                    case "Float":
                        for (int i = 0; i < Points; i += 2)
                        {
                            float value = (float)(random.NextDouble() * 1000);
                            result += $"[{i}-{i + 1}]: {value:F2}\r\n";
                        }
                        break;
                    case "String":
                        char[] chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789".ToCharArray();
                        string str = "";
                        for (int i = 0; i < Points * 2; i++)
                        {
                            str += chars[random.Next(chars.Length)];
                        }
                        result = str;
                        break;
                    default:
                        result = "未知数据类型";
                        break;
                }

                // 更新数据值
                DataValue = result;

                // 记录日志
                string hexLog = ShowHex ? "\r\n" + GenerateHexDump(System.Text.Encoding.ASCII.GetBytes("模拟数据包")) : "";
                AddLog($"读取成功，结果: {result}{hexLog}", "接收");
                _loggingService.Info($"三菱MC读取成功: 地址={DeviceAddress}, 类型={SelectedDataType}");
            }
            catch (Exception ex)
            {
                // 记录日志
                AddLog($"读取失败: {ex.Message}", "错误");
                _loggingService.Error(ex, "三菱MC读取失败");

                // 显示错误消息
                MessageBox.Show($"读取失败: {ex.Message}", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        public static string GenerateHexDump(byte[] bytes, int bytesPerLine = 16)
        {
            if (bytes == null)
                return "<null>";

            int lineCount = (bytes.Length + bytesPerLine - 1) / bytesPerLine;
            StringBuilder result = new StringBuilder();

            for (int i = 0; i < lineCount; i++)
            {
                int start = i * bytesPerLine;
                int end = Math.Min(start + bytesPerLine, bytes.Length);

                // 地址偏移
                result.AppendFormat("{0:X8}  ", start);

                // 十六进制内容
                for (int j = start; j < end; j++)
                {
                    result.AppendFormat("{0:X2} ", bytes[j]);
                }

                // 填充空白，使 ASCII 区域对齐
                int padding = bytesPerLine - (end - start);
                for (int j = 0; j < padding; j++)
                {
                    result.Append("   ");
                }

                result.Append(" ");

                // ASCII 内容
                for (int j = start; j < end; j++)
                {
                    char c = (char)bytes[j];
                    result.Append(char.IsControl(c) ? '.' : c);
                }

                result.AppendLine();
            }

            return result.ToString();
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

                if (string.IsNullOrWhiteSpace(DeviceAddress))
                {
                    MessageBox.Show("请输入设备地址", "提示", MessageBoxButton.OK, MessageBoxImage.Information);
                    return;
                }

                if (string.IsNullOrWhiteSpace(DataValue))
                {
                    MessageBox.Show("请输入要写入的数据", "提示", MessageBoxButton.OK, MessageBoxImage.Information);
                    return;
                }

                // 记录写入信息
                AddLog($"开始写入设备地址: {DeviceAddress}, 值={DataValue}, 类型={SelectedDataType}", "发送");

                // 这里应该添加实际的写入代码，这里只是模拟
                System.Threading.Thread.Sleep(300);

                // 记录日志
                string hexLog = ShowHex ? "\r\n" + GenerateHexDump(System.Text.Encoding.ASCII.GetBytes("模拟数据包")) : "";
                AddLog($"写入成功{hexLog}", "接收");
                _loggingService.Info($"三菱MC写入成功: 地址={DeviceAddress}, 类型={SelectedDataType}, 值={DataValue}");
            }
            catch (Exception ex)
            {
                // 记录日志
                AddLog($"写入失败: {ex.Message}", "错误");
                _loggingService.Error(ex, "三菱MC写入失败");

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
                string logFileName = $"MitsubishiLog_{DateTime.Now:yyyyMMdd_HHmmss}.txt";
                string logFilePath = System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), logFileName);

                // 保存日志内容
                System.IO.File.WriteAllText(logFilePath, LogContent);

                // 记录日志
                _loggingService.Info($"三菱MC日志已保存至: {logFilePath}");

                // 显示成功消息
                MessageBox.Show($"日志已保存至: {logFilePath}", "成功", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                // 记录日志
                _loggingService.Error(ex, "保存三菱MC日志失败");

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