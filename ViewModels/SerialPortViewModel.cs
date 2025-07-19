using Prism.Commands;
using Prism.Mvvm;
using System;
using System.Collections.ObjectModel;
using System.IO.Ports;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;
using CommunicationProtocol.WPF.Services.LoggingService;

namespace CommunicationProtocol.WPF.ViewModels
{
    /// <summary>
    /// 串口通信视图模型
    /// </summary>
    public class SerialPortViewModel : BindableBase
    {
        private readonly ILoggingService _loggingService;
        private SerialPort _serialPort;
        private DispatcherTimer _autoSendTimer;

        #region 串口参数属性

        private ObservableCollection<string> _portNames;
        /// <summary>
        /// 串口号列表
        /// </summary>
        public ObservableCollection<string> PortNames
        {
            get => _portNames;
            set => SetProperty(ref _portNames, value);
        }

        private string _selectedPortName;
        /// <summary>
        /// 选中的串口号
        /// </summary>
        public string SelectedPortName
        {
            get => _selectedPortName;
            set => SetProperty(ref _selectedPortName, value);
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

        private int _selectedBaudRate = 9600;
        /// <summary>
        /// 选中的波特率
        /// </summary>
        public int SelectedBaudRate
        {
            get => _selectedBaudRate;
            set => SetProperty(ref _selectedBaudRate, value);
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

        private int _selectedDataBits = 8;
        /// <summary>
        /// 选中的数据位
        /// </summary>
        public int SelectedDataBits
        {
            get => _selectedDataBits;
            set => SetProperty(ref _selectedDataBits, value);
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

        private StopBits _selectedStopBits = System.IO.Ports.StopBits.One;
        /// <summary>
        /// 选中的停止位
        /// </summary>
        public StopBits SelectedStopBits
        {
            get => _selectedStopBits;
            set => SetProperty(ref _selectedStopBits, value);
        }

        private ObservableCollection<Parity> _parities;
        /// <summary>
        /// 校验位列表
        /// </summary>
        public ObservableCollection<Parity> Parities
        {
            get => _parities;
            set => SetProperty(ref _parities, value);
        }

        private Parity _selectedParity = Parity.None;
        /// <summary>
        /// 选中的校验位
        /// </summary>
        public Parity SelectedParity
        {
            get => _selectedParity;
            set => SetProperty(ref _selectedParity, value);
        }

        private ObservableCollection<Handshake> _flowControls;
        /// <summary>
        /// 流控制列表
        /// </summary>
        public ObservableCollection<Handshake> FlowControls
        {
            get => _flowControls;
            set => SetProperty(ref _flowControls, value);
        }

        private Handshake _selectedFlowControl = Handshake.None;
        /// <summary>
        /// 选中的流控制
        /// </summary>
        public Handshake SelectedFlowControl
        {
            get => _selectedFlowControl;
            set => SetProperty(ref _selectedFlowControl, value);
        }

        private bool _dtrEnable;
        /// <summary>
        /// DTR使能
        /// </summary>
        public bool DtrEnable
        {
            get => _dtrEnable;
            set => SetProperty(ref _dtrEnable, value);
        }

        private bool _rtsEnable;
        /// <summary>
        /// RTS使能
        /// </summary>
        public bool RtsEnable
        {
            get => _rtsEnable;
            set => SetProperty(ref _rtsEnable, value);
        }

        #endregion

        #region 发送接收属性

        private bool _hexSend;
        /// <summary>
        /// 十六进制发送
        /// </summary>
        public bool HexSend
        {
            get => _hexSend;
            set => SetProperty(ref _hexSend, value);
        }

        private bool _sendNewLine = true;
        /// <summary>
        /// 发送新行
        /// </summary>
        public bool SendNewLine
        {
            get => _sendNewLine;
            set => SetProperty(ref _sendNewLine, value);
        }

        private bool _hexDisplay;
        /// <summary>
        /// 十六进制显示
        /// </summary>
        public bool HexDisplay
        {
            get => _hexDisplay;
            set => SetProperty(ref _hexDisplay, value);
        }

        private bool _showSend = true;
        /// <summary>
        /// 显示发送
        /// </summary>
        public bool ShowSend
        {
            get => _showSend;
            set => SetProperty(ref _showSend, value);
        }

        private bool _autoScroll = true;
        /// <summary>
        /// 自动滚动
        /// </summary>
        public bool AutoScroll
        {
            get => _autoScroll;
            set => SetProperty(ref _autoScroll, value);
        }

        private bool _showTime = true;
        /// <summary>
        /// 显示时间
        /// </summary>
        public bool ShowTime
        {
            get => _showTime;
            set => SetProperty(ref _showTime, value);
        }

        private bool _autoSend;
        /// <summary>
        /// 自动发送
        /// </summary>
        public bool AutoSend
        {
            get => _autoSend;
            set
            {
                SetProperty(ref _autoSend, value);
                if (value)
                {
                    StartAutoSend();
                }
                else
                {
                    StopAutoSend();
                }
            }
        }

        private int _autoSendInterval = 1000;
        /// <summary>
        /// 自动发送间隔(ms)
        /// </summary>
        public int AutoSendInterval
        {
            get => _autoSendInterval;
            set
            {
                if (value < 10) value = 10; // 最小10毫秒
                SetProperty(ref _autoSendInterval, value);
                if (_autoSendTimer != null)
                {
                    _autoSendTimer.Interval = TimeSpan.FromMilliseconds(value);
                }
            }
        }

        private string _dataToSend;
        /// <summary>
        /// 要发送的数据
        /// </summary>
        public string DataToSend
        {
            get => _dataToSend;
            set => SetProperty(ref _dataToSend, value);
        }

        private string _receivedData = string.Empty;
        /// <summary>
        /// 接收到的数据
        /// </summary>
        public string ReceivedData
        {
            get => _receivedData;
            set => SetProperty(ref _receivedData, value);
        }

        private int _sendCount;
        /// <summary>
        /// 发送计数
        /// </summary>
        public int SendCount
        {
            get => _sendCount;
            set => SetProperty(ref _sendCount, value);
        }

        private int _receiveCount;
        /// <summary>
        /// 接收计数
        /// </summary>
        public int ReceiveCount
        {
            get => _receiveCount;
            set => SetProperty(ref _receiveCount, value);
        }

        #endregion

        #region 连接状态属性

        private bool _isPortOpen;
        /// <summary>
        /// 是否已打开串口
        /// </summary>
        public bool IsPortOpen
        {
            get => _isPortOpen;
            set
            {
                SetProperty(ref _isPortOpen, value);
                RaisePropertyChanged(nameof(ConnectionStatus));
                RaisePropertyChanged(nameof(ConnectionStatusStyle));
                RaisePropertyChanged(nameof(OpenPortButtonVisibility));
                RaisePropertyChanged(nameof(ClosePortButtonVisibility));
            }
        }

        /// <summary>
        /// 连接状态文本
        /// </summary>
        public string ConnectionStatus => IsPortOpen ? "已打开" : "已关闭";

        /// <summary>
        /// 连接状态样式
        /// </summary>
        public string ConnectionStatusStyle => IsPortOpen ? "ConnectedTextBlockStyle" : "DisconnectedTextBlockStyle";

        /// <summary>
        /// 打开串口按钮可见性
        /// </summary>
        public Visibility OpenPortButtonVisibility => IsPortOpen ? Visibility.Collapsed : Visibility.Visible;

        /// <summary>
        /// 关闭串口按钮可见性
        /// </summary>
        public Visibility ClosePortButtonVisibility => IsPortOpen ? Visibility.Visible : Visibility.Collapsed;

        #endregion

        #region 命令

        /// <summary>
        /// 刷新串口列表命令
        /// </summary>
        public DelegateCommand RefreshPortsCommand { get; private set; }

        /// <summary>
        /// 打开串口命令
        /// </summary>
        public DelegateCommand OpenPortCommand { get; private set; }

        /// <summary>
        /// 关闭串口命令
        /// </summary>
        public DelegateCommand ClosePortCommand { get; private set; }

        /// <summary>
        /// 发送数据命令
        /// </summary>
        public DelegateCommand SendDataCommand { get; private set; }

        /// <summary>
        /// 清空发送数据命令
        /// </summary>
        public DelegateCommand ClearSendDataCommand { get; private set; }

        /// <summary>
        /// 清空接收数据命令
        /// </summary>
        public DelegateCommand ClearReceiveDataCommand { get; private set; }

        /// <summary>
        /// 保存接收数据命令
        /// </summary>
        public DelegateCommand SaveReceiveDataCommand { get; private set; }

        /// <summary>
        /// 清空发送计数命令
        /// </summary>
        public DelegateCommand ClearSendCountCommand { get; private set; }

        /// <summary>
        /// 清空接收计数命令
        /// </summary>
        public DelegateCommand ClearReceiveCountCommand { get; private set; }

        #endregion

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="loggingService">日志服务</param>
        public SerialPortViewModel(ILoggingService loggingService)
        {
            _loggingService = loggingService;

            InitializeCollections();
            InitializeCommands();

            _loggingService.Info("串口通信视图模型已初始化");
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
                SelectedPortName = PortNames[0];
            }

            // 初始化波特率列表
            BaudRates = new ObservableCollection<int>
            {
                300, 600, 1200, 2400, 4800, 9600, 19200, 38400, 57600, 115200, 230400, 460800, 921600
            };

            // 初始化数据位列表
            DataBits = new ObservableCollection<int> { 5, 6, 7, 8 };

            // 初始化停止位列表
            StopBits = new ObservableCollection<StopBits>
            {
                System.IO.Ports.StopBits.One,
                System.IO.Ports.StopBits.OnePointFive,
                System.IO.Ports.StopBits.Two
            };

            // 初始化校验位列表
            Parities = new ObservableCollection<Parity>
            {
                Parity.None,
                Parity.Odd,
                Parity.Even,
                Parity.Mark,
                Parity.Space
            };

            // 初始化流控制列表
            FlowControls = new ObservableCollection<Handshake>
            {
                Handshake.None,
                Handshake.XOnXOff,
                Handshake.RequestToSend,
                Handshake.RequestToSendXOnXOff
            };
        }

        /// <summary>
        /// 初始化命令
        /// </summary>
        private void InitializeCommands()
        {
            RefreshPortsCommand = new DelegateCommand(RefreshPorts);
            OpenPortCommand = new DelegateCommand(OpenPort);
            ClosePortCommand = new DelegateCommand(ClosePort);
            SendDataCommand = new DelegateCommand(SendData);
            ClearSendDataCommand = new DelegateCommand(() => DataToSend = string.Empty);
            ClearReceiveDataCommand = new DelegateCommand(() => ReceivedData = string.Empty);
            SaveReceiveDataCommand = new DelegateCommand(SaveReceiveData);
            ClearSendCountCommand = new DelegateCommand(() => SendCount = 0);
            ClearReceiveCountCommand = new DelegateCommand(() => ReceiveCount = 0);
        }

        /// <summary>
        /// 刷新串口列表
        /// </summary>
        private void RefreshPorts()
        {
            try
            {
                PortNames.Clear();
                foreach (string port in SerialPort.GetPortNames())
                {
                    PortNames.Add(port);
                }

                if (PortNames.Count > 0)
                {
                    SelectedPortName = PortNames[0];
                }

                _loggingService.Info($"刷新串口列表成功，发现{PortNames.Count}个串口");
            }
            catch (Exception ex)
            {
                _loggingService.Error(ex, "刷新串口列表失败");
                MessageBox.Show($"刷新串口列表失败: {ex.Message}", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// 打开串口
        /// </summary>
        private void OpenPort()
        {
            try
            {
                if (string.IsNullOrEmpty(SelectedPortName))
                {
                    MessageBox.Show("请选择串口", "提示", MessageBoxButton.OK, MessageBoxImage.Information);
                    return;
                }

                // 创建并配置串口
                _serialPort = new SerialPort
                {
                    PortName = SelectedPortName,
                    BaudRate = SelectedBaudRate,
                    DataBits = SelectedDataBits,
                    StopBits = SelectedStopBits,
                    Parity = SelectedParity,
                    Handshake = SelectedFlowControl,
                    DtrEnable = DtrEnable,
                    RtsEnable = RtsEnable
                };

                // 设置接收事件处理
                _serialPort.DataReceived += SerialPort_DataReceived;

                // 打开串口
                _serialPort.Open();
                IsPortOpen = true;

                // 记录日志
                _loggingService.Info($"打开串口成功: {SelectedPortName}, {SelectedBaudRate}, {SelectedDataBits}, {SelectedStopBits}, {SelectedParity}");
                AddReceiveData($"打开串口成功: {SelectedPortName}", "系统");
            }
            catch (Exception ex)
            {
                _loggingService.Error(ex, "打开串口失败");
                MessageBox.Show($"打开串口失败: {ex.Message}", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// 关闭串口
        /// </summary>
        private void ClosePort()
        {
            try
            {
                if (_serialPort != null && _serialPort.IsOpen)
                {
                    // 停止自动发送
                    StopAutoSend();

                    // 关闭串口
                    _serialPort.Close();
                    _serialPort.DataReceived -= SerialPort_DataReceived;
                    _serialPort.Dispose();
                    _serialPort = null;

                    IsPortOpen = false;

                    // 记录日志
                    _loggingService.Info($"关闭串口成功: {SelectedPortName}");
                    AddReceiveData("关闭串口成功", "系统");
                }
            }
            catch (Exception ex)
            {
                _loggingService.Error(ex, "关闭串口失败");
                MessageBox.Show($"关闭串口失败: {ex.Message}", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// 发送数据
        /// </summary>
        private void SendData()
        {
            try
            {
                if (_serialPort == null || !_serialPort.IsOpen)
                {
                    MessageBox.Show("串口未打开", "提示", MessageBoxButton.OK, MessageBoxImage.Information);
                    return;
                }

                if (string.IsNullOrEmpty(DataToSend))
                {
                    return;
                }

                byte[] dataToSend;

                // 根据模式处理数据
                if (HexSend)
                {
                    // 十六进制模式
                    dataToSend = HexStringToByteArray(DataToSend);
                }
                else
                {
                    // 文本模式
                    string data = DataToSend;
                    if (SendNewLine)
                    {
                        data += "\r\n";
                    }

                    dataToSend = Encoding.UTF8.GetBytes(data);
                }

                // 发送数据
                _serialPort.Write(dataToSend, 0, dataToSend.Length);

                // 更新发送计数
                SendCount += dataToSend.Length;

                // 如果设置了显示发送，则在接收区显示发送的数据
                if (ShowSend)
                {
                    if (HexDisplay)
                    {
                        AddReceiveData(BitConverter.ToString(dataToSend).Replace("-", " "), "发送");
                    }
                    else
                    {
                        AddReceiveData(DataToSend + (SendNewLine ? "\r\n" : ""), "发送");
                    }
                }

                _loggingService.Debug($"串口发送数据: {dataToSend.Length} 字节");
            }
            catch (Exception ex)
            {
                _loggingService.Error(ex, "发送数据失败");
                MessageBox.Show($"发送数据失败: {ex.Message}", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// 串口数据接收事件处理
        /// </summary>
        private void SerialPort_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            try
            {
                // 确保串口已打开
                if (_serialPort == null || !_serialPort.IsOpen)
                {
                    return;
                }


                // 读取数据
                int bytesToRead = _serialPort.BytesToRead;
                byte[] buffer = new byte[bytesToRead];
                int bytesRead = _serialPort.Read(buffer, 0, bytesToRead);

                // 更新接收计数
                ReceiveCount += bytesRead;

                // 在UI线程上更新接收区
                Application.Current.Dispatcher.Invoke(() =>
                {
                    if (HexDisplay)
                    {
                        AddReceiveData(BitConverter.ToString(buffer).Replace("-", " "), "接收");
                    }
                    else
                    {
                        AddReceiveData(Encoding.UTF8.GetString(buffer), "接收");
                    }
                });

                _loggingService.Debug($"串口接收数据: {bytesRead} 字节");
            }
            catch (Exception ex)
            {
                _loggingService.Error(ex, "接收数据失败");
                Application.Current.Dispatcher.Invoke(() =>
                {
                    MessageBox.Show($"接收数据失败: {ex.Message}", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
                });
            }
        }

        /// <summary>
        /// 保存接收数据
        /// </summary>
        private void SaveReceiveData()
        {
            try
            {
                if (string.IsNullOrEmpty(ReceivedData))
                {
                    MessageBox.Show("没有接收数据可保存", "提示", MessageBoxButton.OK, MessageBoxImage.Information);
                    return;
                }

                // 这里应该添加实际的保存文件对话框代码，这里只是模拟
                string fileName = $"SerialData_{DateTime.Now:yyyyMMdd_HHmmss}.txt";
                string filePath = System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), fileName);

                // 保存数据内容
                System.IO.File.WriteAllText(filePath, ReceivedData);

                // 记录日志
                _loggingService.Info($"串口数据已保存至: {filePath}");

                // 显示成功消息
                MessageBox.Show($"数据已保存至: {filePath}", "成功", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                _loggingService.Error(ex, "保存接收数据失败");
                MessageBox.Show($"保存数据失败: {ex.Message}", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// 启动自动发送
        /// </summary>
        private void StartAutoSend()
        {
            if (_autoSendTimer == null)
            {
                _autoSendTimer = new DispatcherTimer();
                _autoSendTimer.Interval = TimeSpan.FromMilliseconds(AutoSendInterval);
                _autoSendTimer.Tick += AutoSendTimer_Tick;
                _autoSendTimer.Start();

                _loggingService.Info($"启动自动发送，间隔: {AutoSendInterval}ms");
                AddReceiveData($"启动自动发送，间隔: {AutoSendInterval}ms", "系统");
            }
            else
            {
                _autoSendTimer.Start();
            }
        }

        /// <summary>
        /// 停止自动发送
        /// </summary>
        private void StopAutoSend()
        {
            if (_autoSendTimer != null)
            {
                _autoSendTimer.Stop();
                _loggingService.Info("停止自动发送");
                AddReceiveData("停止自动发送", "系统");
            }
        }

        /// <summary>
        /// 自动发送定时器触发事件
        /// </summary>
        private void AutoSendTimer_Tick(object sender, EventArgs e)
        {
            if (IsPortOpen)
            {
                SendData();
            }
            else
            {
                StopAutoSend();
                AutoSend = false;
            }
        }

        /// <summary>
        /// 十六进制字符串转字节数组
        /// </summary>
        private byte[] HexStringToByteArray(string hex)
        {
            // 移除所有空格
            hex = hex.Replace(" ", "").Replace("\r", "").Replace("\n", "");

            // 处理非法字符
            System.Text.RegularExpressions.Regex regex = new System.Text.RegularExpressions.Regex("[^0-9A-Fa-f]");
            hex = regex.Replace(hex, "");

            // 如果长度为奇数，添加前导0
            if (hex.Length % 2 != 0)
            {
                hex = "0" + hex;
            }

            byte[] bytes = new byte[hex.Length / 2];
            for (int i = 0; i < hex.Length; i += 2)
            {
                bytes[i / 2] = Convert.ToByte(hex.Substring(i, 2), 16);
            }

            return bytes;
        }

        /// <summary>
        /// 添加接收数据
        /// </summary>
        private void AddReceiveData(string data, string type)
        {
            StringBuilder sb = new StringBuilder();

            // 添加时间戳
            if (ShowTime)
            {
                sb.Append($"[{DateTime.Now:HH:mm:ss.fff}] ");
            }

            // 添加类型
            if (!string.IsNullOrEmpty(type))
            {
                sb.Append($"[{type}] ");
            }

            // 添加数据
            sb.Append(data);

            // 更新接收区
            ReceivedData += sb.ToString();

            // 如果启用了自动滚动，确保添加足够的换行
            if (AutoScroll)
            {
                ReceivedData += "\r\n";
            }
        }

        /// <summary>
        /// 析构函数
        /// </summary>
        ~SerialPortViewModel()
        {
            ClosePort();
        }
    }
}