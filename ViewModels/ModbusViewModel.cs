using Prism.Commands;
using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Media;
using CommunicationProtocol.WPF.Services.LoggingService;
using System.IO.Ports;
using Prism.Regions;

namespace CommunicationProtocol.WPF.ViewModels
{
    /// <summary>
    /// Modbus视图模型
    /// </summary>
    public class ModbusViewModel : BindableBase
    {
        private readonly ILoggingService _loggingService;

        #region 通讯参数属性

        private bool _isRtu = true;
        /// <summary>
        /// 是否为RTU模式
        /// </summary>
        public bool IsRtu
        {
            get => _isRtu;
            set
            {
                if (SetProperty(ref _isRtu, value))
                {
                    if (_isTcp == value) // 只有在 IsTcp 值不一致时才设置
                        IsTcp = !value;

                    RaisePropertyChanged(nameof(RtuParametersVisibility));
                    RaisePropertyChanged(nameof(TcpParametersVisibility));
                }
            }
        }

        private bool _isTcp;
        /// <summary>
        /// 是否为TCP模式
        /// </summary>
        public bool IsTcp
        {
            get => _isTcp;
            set
            {
                if (SetProperty(ref _isTcp, value))
                {
                    if (_isRtu == value) // 只有在 IsRtu 值不一致时才设置
                        IsRtu = !value;

                    RaisePropertyChanged(nameof(RtuParametersVisibility));
                    RaisePropertyChanged(nameof(TcpParametersVisibility));
                }
            }
        }


        /// <summary>
        /// RTU参数可见性
        /// </summary>
        public Visibility RtuParametersVisibility => IsRtu ? Visibility.Visible : Visibility.Collapsed;

        /// <summary>
        /// TCP参数可见性
        /// </summary>
        public Visibility TcpParametersVisibility => IsTcp ? Visibility.Visible : Visibility.Collapsed;

        // RTU参数
        private ObservableCollection<string> _portNames;
        /// <summary>
        /// 可用串口列表
        /// </summary>
        public ObservableCollection<string> PortNames
        {
            get => _portNames;
            set => SetProperty(ref _portNames, value);
        }

        private string _portName;
        /// <summary>
        /// 选中的串口
        /// </summary>
        public string PortName
        {
            get => _portName;
            set => SetProperty(ref _portName, value);
        }

        private ObservableCollection<int> _baudRates;
        /// <summary>
        /// 波特率列表
        /// </summary>
        public ObservableCollection<int> BaudRates
        {
            get => _baudRates;
            set => SetProperty(ref _baudRates, value);
        }

        private int _baudRate = 9600;
        /// <summary>
        /// 选中的波特率
        /// </summary>
        public int BaudRate
        {
            get => _baudRate;
            set => SetProperty(ref _baudRate, value);
        }

        private ObservableCollection<int> _dataBits;
        /// <summary>
        /// 数据位列表
        /// </summary>
        public ObservableCollection<int> DataBits
        {
            get => _dataBits;
            set => SetProperty(ref _dataBits, value);
        }

        private int _dataBit = 8;
        /// <summary>
        /// 选中的数据位
        /// </summary>
        public int DataBit
        {
            get => _dataBit;
            set => SetProperty(ref _dataBit, value);
        }

        private ObservableCollection<StopBits> _stopBits;
        /// <summary>
        /// 停止位列表
        /// </summary>
        public ObservableCollection<StopBits> StopBits
        {
            get => _stopBits;
            set => SetProperty(ref _stopBits, value);
        }

        private StopBits _stopBit = System.IO.Ports.StopBits.One;
        /// <summary>
        /// 选中的停止位
        /// </summary>
        public StopBits StopBit
        {
            get => _stopBit;
            set => SetProperty(ref _stopBit, value);
        }

        private ObservableCollection<Parity> _parityBits;
        /// <summary>
        /// 校验位列表
        /// </summary>
        public ObservableCollection<Parity> ParityBits
        {
            get => _parityBits;
            set => SetProperty(ref _parityBits, value);
        }

        private Parity _parityBit = Parity.None;
        /// <summary>
        /// 选中的校验位
        /// </summary>
        public Parity ParityBit
        {
            get => _parityBit;
            set => SetProperty(ref _parityBit, value);
        }

        // TCP参数
        private string _ipAddress = "127.0.0.1";
        /// <summary>
        /// IP地址
        /// </summary>
        public string IpAddress
        {
            get => _ipAddress;
            set => SetProperty(ref _ipAddress, value);
        }

        private int _port = 502;
        /// <summary>
        /// 端口号
        /// </summary>
        public int Port
        {
            get => _port;
            set => SetProperty(ref _port, value);
        }

        // 公共参数
        private byte _slaveId = 1;
        /// <summary>
        /// 从站地址
        /// </summary>
        public byte SlaveId
        {
            get => _slaveId;
            set => SetProperty(ref _slaveId, value);
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

        /// <summary>
        /// 功能码项
        /// </summary>
        public class FunctionCodeItem
        {
            public string Display { get; set; }
            public byte Value { get; set; }
        }

        private ObservableCollection<FunctionCodeItem> _functionCodes;
        /// <summary>
        /// 功能码列表
        /// </summary>
        public ObservableCollection<FunctionCodeItem> FunctionCodes
        {
            get => _functionCodes;
            set => SetProperty(ref _functionCodes, value);
        }

        private FunctionCodeItem _selectedFunctionCode;
        /// <summary>
        /// 选中的功能码
        /// </summary>
        public FunctionCodeItem SelectedFunctionCode
        {
            get => _selectedFunctionCode;
            set
            {
                SetProperty(ref _selectedFunctionCode, value);
                // 更新是否为读操作
                if (value != null)
                {
                    IsReadOperation = value.Value == 1 || value.Value == 2 || value.Value == 3 || value.Value == 4;
                }
                RaisePropertyChanged(nameof(CanWrite));
            }
        }

        private ushort _startAddress;
        /// <summary>
        /// 起始地址
        /// </summary>
        public ushort StartAddress
        {
            get => _startAddress;
            set => SetProperty(ref _startAddress, value);
        }

        private ushort _quantity = 1;
        /// <summary>
        /// 读取数量
        /// </summary>
        public ushort Quantity
        {
            get => _quantity;
            set => SetProperty(ref _quantity, value);
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

        private string _selectedDataType = "UInt16";
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

        private bool _showDirection = true;
        /// <summary>
        /// 是否显示发送/接收方向
        /// </summary>
        public bool ShowDirection
        {
            get => _showDirection;
            set => SetProperty(ref _showDirection, value);
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
        // 在ModbusViewModel.cs的构造函数中添加导航参数处理
        public ModbusViewModel(ILoggingService loggingService, NavigationParameters navigationParameters = null)
        {
            _loggingService = loggingService;

            InitializeCollections();
            InitializeCommands();

            // 根据导航参数设置通讯模式
            if (navigationParameters != null)
            {
                string viewName = navigationParameters.GetValue<string>("ViewName");
                if (!string.IsNullOrEmpty(viewName))
                {
                    switch (viewName)
                    {
                        case "ModbusRtu":
                            IsRtu = true;
                            _loggingService.Info("从首页导航到Modbus RTU视图");
                            break;
                        case "ModbusTcp":
                            IsTcp = true;
                            _loggingService.Info("从首页导航到Modbus TCP视图");
                            break;
                    }
                }
            }

            _loggingService.Info("Modbus视图模型已初始化");
        }



        // 在ModbusViewModel.cs中添加以下方法
        /// <summary>
        /// 从导航参数更新视图模型状态
        /// </summary>
        /// <param name="parameters">导航参数</param>
        public void UpdateFromNavigationParameters(NavigationParameters parameters)
        {
            string viewName = parameters.GetValue<string>("ViewName");
            if (!string.IsNullOrEmpty(viewName))
            {
                switch (viewName)
                {
                    case "ModbusRtu":
                        IsRtu = true;
                        _loggingService.Info("从导航参数设置为Modbus RTU模式");
                        break;
                    case "ModbusTcp":
                        IsTcp = true;
                        _loggingService.Info("从导航参数设置为Modbus TCP模式");
                        break;
                }
            }
        }


        /// <summary>
        /// 初始化集合
        /// </summary>
        private void InitializeCollections()
        {
            // 初始化串口列表
            PortNames = new ObservableCollection<string>(SerialPort.GetPortNames());
            if (PortNames.Count > 0)
            {
                PortName = PortNames[0];
            }

            // 初始化波特率列表
            BaudRates = new ObservableCollection<int>
            {
                1200, 2400, 4800, 9600, 19200, 38400, 57600, 115200
            };

            // 初始化数据位列表
            DataBits = new ObservableCollection<int>
            {
                7, 8
            };

            // 初始化停止位列表
            StopBits = new ObservableCollection<StopBits>
            {
                System.IO.Ports.StopBits.One,
                System.IO.Ports.StopBits.OnePointFive,
                System.IO.Ports.StopBits.Two
            };

            // 初始化校验位列表
            ParityBits = new ObservableCollection<Parity>
            {
                Parity.None,
                Parity.Odd,
                Parity.Even,
                Parity.Mark,
                Parity.Space
            };

            // 初始化功能码列表
            FunctionCodes = new ObservableCollection<FunctionCodeItem>
            {
                new FunctionCodeItem { Display = "01 - 读线圈状态", Value = 1 },
                new FunctionCodeItem { Display = "02 - 读离散输入状态", Value = 2 },
                new FunctionCodeItem { Display = "03 - 读保持寄存器", Value = 3 },
                new FunctionCodeItem { Display = "04 - 读输入寄存器", Value = 4 },
                new FunctionCodeItem { Display = "05 - 写单个线圈", Value = 5 },
                new FunctionCodeItem { Display = "06 - 写单个寄存器", Value = 6 },
                new FunctionCodeItem { Display = "15 - 写多个线圈", Value = 15 },
                new FunctionCodeItem { Display = "16 - 写多个寄存器", Value = 16 }
            };
            SelectedFunctionCode = FunctionCodes[2]; // 默认选择读保持寄存器

            // 初始化数据类型列表
            DataTypes = new ObservableCollection<string>
            {
                "Bit",
                "Int16",
                "UInt16",
                "Int32",
                "UInt32",
                "Int64",
                "UInt64",
                "Float",
                "Double",
                "Hex",
                "ASCII"
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
                _loggingService.Info("开始连接Modbus设备...");

                // 根据模式记录不同的连接信息
                if (IsRtu)
                {
                    _loggingService.Info($"Modbus RTU连接参数: 串口={PortName}, 波特率={BaudRate}, 数据位={DataBit}, 停止位={StopBit}, 校验位={ParityBit}, 从站地址={SlaveId}, 超时={Timeout}ms");
                }
                else
                {
                    _loggingService.Info($"Modbus TCP连接参数: IP={IpAddress}, 端口={Port}, 从站地址={SlaveId}, 超时={Timeout}ms");
                }

                // 模拟连接延迟
                System.Threading.Thread.Sleep(500);

                // 连接成功
                IsConnected = true;

                // 记录日志
                AddLog("连接成功", "系统");
                _loggingService.Info("Modbus设备连接成功");
            }
            catch (Exception ex)
            {
                // 连接失败
                IsConnected = false;

                // 记录日志
                AddLog($"连接失败: {ex.Message}", "错误");
                _loggingService.Error(ex, "Modbus设备连接失败");

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
                _loggingService.Info("断开Modbus设备连接...");

                // 模拟断开延迟
                System.Threading.Thread.Sleep(200);

                // 断开成功
                IsConnected = false;

                // 记录日志
                AddLog("断开连接", "系统");
                _loggingService.Info("Modbus设备断开成功");
            }
            catch (Exception ex)
            {
                // 记录日志
                AddLog($"断开连接失败: {ex.Message}", "错误");
                _loggingService.Error(ex, "Modbus设备断开失败");

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

                if (SelectedFunctionCode == null)
                {
                    MessageBox.Show("请选择功能码", "提示", MessageBoxButton.OK, MessageBoxImage.Information);
                    return;
                }

                // 记录读取信息
                string functionName = SelectedFunctionCode.Display.Split('-')[1].Trim();
                AddLog($"开始{functionName}，地址:{StartAddress}，数量:{Quantity}", "发送");

                // 这里应该添加实际的读取代码，这里只是模拟
                System.Threading.Thread.Sleep(300);

                // 模拟生成数据
                string result = string.Empty;
                Random random = new Random();

                // 根据不同的数据类型生成不同的结果
                switch (SelectedDataType)
                {
                    case "Bit":
                        // 生成布尔值列表
                        for (int i = 0; i < Quantity; i++)
                        {
                            result += $"[{StartAddress + i}]: {(random.Next(2) == 1 ? "1" : "0")}\r\n";
                        }
                        break;
                    case "Int16":
                        // 生成Int16值列表
                        for (int i = 0; i < Quantity; i++)
                        {
                            result += $"[{StartAddress + i}]: {random.Next(-32768, 32767)}\r\n";
                        }
                        break;
                    case "UInt16":
                        // 生成UInt16值列表
                        for (int i = 0; i < Quantity; i++)
                        {
                            result += $"[{StartAddress + i}]: {random.Next(0, 65535)}\r\n";
                        }
                        break;
                    case "Int32":
                        // 生成Int32值列表
                        for (int i = 0; i < Quantity; i += 2)
                        {
                            if (i + 1 < Quantity)
                            {
                                result += $"[{StartAddress + i}-{StartAddress + i + 1}]: {random.Next(-2147483647, 2147483647)}\r\n";
                            }
                        }
                        break;
                    case "UInt32":
                        // 生成UInt32值列表
                        for (int i = 0; i < Quantity; i += 2)
                        {
                            if (i + 1 < Quantity)
                            {
                                result += $"[{StartAddress + i}-{StartAddress + i + 1}]: {random.Next(0, 2147483647)}\r\n";
                            }
                        }
                        break;
                    case "Float":
                        // 生成Float值列表
                        for (int i = 0; i < Quantity; i += 2)
                        {
                            if (i + 1 < Quantity)
                            {
                                result += $"[{StartAddress + i}-{StartAddress + i + 1}]: {Math.Round(random.NextDouble() * 100, 2)}\r\n";
                            }
                        }
                        break;
                    case "Hex":
                        // 生成Hex值列表
                        for (int i = 0; i < Quantity; i++)
                        {
                            result += $"[{StartAddress + i}]: 0x{random.Next(0, 65535):X4}\r\n";
                        }
                        break;
                    default:
                        // 默认生成UInt16值列表
                        for (int i = 0; i < Quantity; i++)
                        {
                            result += $"[{StartAddress + i}]: {random.Next(0, 65535)}\r\n";
                        }
                        break;
                }

                // 更新数据值
                DataValue = result;

                // 记录日志
                AddLog($"读取成功，结果:\r\n{result}", "接收");
                _loggingService.Info($"Modbus读取成功: {functionName}, 地址:{StartAddress}, 数量:{Quantity}");
            }
            catch (Exception ex)
            {
                // 记录日志
                AddLog($"读取失败: {ex.Message}", "错误");
                _loggingService.Error(ex, "Modbus读取失败");

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

                if (SelectedFunctionCode == null)
                {
                    MessageBox.Show("请选择功能码", "提示", MessageBoxButton.OK, MessageBoxImage.Information);
                    return;
                }

                if (string.IsNullOrWhiteSpace(DataValue))
                {
                    MessageBox.Show("请输入要写入的数据", "提示", MessageBoxButton.OK, MessageBoxImage.Information);
                    return;
                }

                // 记录写入信息
                string functionName = SelectedFunctionCode.Display.Split('-')[1].Trim();
                AddLog($"开始{functionName}，地址:{StartAddress}，值:{DataValue}", "发送");

                // 这里应该添加实际的写入代码，这里只是模拟
                System.Threading.Thread.Sleep(300);

                // 记录日志
                AddLog("写入成功", "接收");
                _loggingService.Info($"Modbus写入成功: {functionName}, 地址:{StartAddress}, 值:{DataValue}");
            }
            catch (Exception ex)
            {
                // 记录日志
                AddLog($"写入失败: {ex.Message}", "错误");
                _loggingService.Error(ex, "Modbus写入失败");

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
                string logFileName = $"ModbusLog_{DateTime.Now:yyyyMMdd_HHmmss}.txt";
                string logFilePath = System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), logFileName);

                // 保存日志内容
                System.IO.File.WriteAllText(logFilePath, LogContent);

                // 记录日志
                _loggingService.Info($"Modbus日志已保存至: {logFilePath}");

                // 显示成功消息
                MessageBox.Show($"日志已保存至: {logFilePath}", "成功", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                // 记录日志
                _loggingService.Error(ex, "保存Modbus日志失败");

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
            _loggingService.Info("Modbus日志已清空");
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
            if (ShowDirection && !string.IsNullOrEmpty(type))
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
                _loggingService.Error($"Modbus错误: {message}");
            }
            else
            {
                _loggingService.Debug($"Modbus: {message}");
            }
        }
    }
}