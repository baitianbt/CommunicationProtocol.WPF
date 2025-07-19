using CommunicationProtocol.WPF.Services.LoggingService;
using Prism.Commands;
using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Threading;

namespace CommunicationProtocol.WPF.ViewModels
{
    /// <summary>
    /// TCP通信视图模型
    /// </summary>
    public class TcpViewModel : BindableBase
    {
        private readonly ILoggingService _loggingService;
        private TcpListener _tcpListener;
        private TcpClient _tcpClient;
        private CancellationTokenSource _listenerCts;
        private List<TcpConnection> _tcpConnections = new List<TcpConnection>();
        private DispatcherTimer _autoSendTimer;

        /// <summary>
        /// TCP连接信息类
        /// </summary>
        public class TcpConnection
        {
            /// <summary>
            /// 连接ID
            /// </summary>
            public int Id { get; set; }

            /// <summary>
            /// 远程终端点
            /// </summary>
            public string RemoteEndPoint { get; set; }

            /// <summary>
            /// 连接状态
            /// </summary>
            public string Status { get; set; }

            /// <summary>
            /// 状态颜色
            /// </summary>
            public Brush StatusColor { get; set; }

            /// <summary>
            /// TCP客户端对象
            /// </summary>
            public TcpClient Client { get; set; }

            /// <summary>
            /// 取消令牌源
            /// </summary>
            public CancellationTokenSource Cts { get; set; }
        }

        #region 模式属性

        private bool _isServer = true;
        /// <summary>
        /// 是否为服务器模式
        /// </summary>
        public bool IsServer
        {
            get => _isServer;
            set
            {
                SetProperty(ref _isServer, value);

                if (IsClient != !value)
                {
                    IsClient = !value;
                }
                RaisePropertyChanged(nameof(ServerParametersVisibility));
                RaisePropertyChanged(nameof(ClientParametersVisibility));
            }
        }

        private bool _isClient;
        /// <summary>
        /// 是否为客户端模式
        /// </summary>
        public bool IsClient
        {
            get => _isClient;
            set
            {
                SetProperty(ref _isClient, value);
                if (IsServer != !value)
                {
                    IsServer = !value;
                }
                RaisePropertyChanged(nameof(ServerParametersVisibility));
                RaisePropertyChanged(nameof(ClientParametersVisibility));
            }
        }

        /// <summary>
        /// 服务器参数可见性
        /// </summary>
        public Visibility ServerParametersVisibility => IsServer ? Visibility.Visible : Visibility.Collapsed;

        /// <summary>
        /// 客户端参数可见性
        /// </summary>
        public Visibility ClientParametersVisibility => IsClient ? Visibility.Visible : Visibility.Collapsed;

        #endregion

        #region 服务器参数属性

        private ObservableCollection<string> _localIPs;
        /// <summary>
        /// 本地IP地址列表
        /// </summary>
        public ObservableCollection<string> LocalIPs
        {
            get => _localIPs;
            set => SetProperty(ref _localIPs, value);
        }

        private string _selectedLocalIP = "0.0.0.0";
        /// <summary>
        /// 选中的本地IP地址
        /// </summary>
        public string SelectedLocalIP
        {
            get => _selectedLocalIP;
            set => SetProperty(ref _selectedLocalIP, value);
        }

        private int _serverPort = 8000;
        /// <summary>
        /// 服务器端口
        /// </summary>
        public int ServerPort
        {
            get => _serverPort;
            set => SetProperty(ref _serverPort, value);
        }

        #endregion

        #region 客户端参数属性

        private string _serverIP = "127.0.0.1";
        /// <summary>
        /// 服务器IP地址
        /// </summary>
        public string ServerIP
        {
            get => _serverIP;
            set => SetProperty(ref _serverIP, value);
        }

        private int _clientPort = 8000;
        /// <summary>
        /// 客户端端口
        /// </summary>
        public int ClientPort
        {
            get => _clientPort;
            set => SetProperty(ref _clientPort, value);
        }

        #endregion

        #region 收发设置属性

        private bool _hexSend;
        /// <summary>
        /// 是否以十六进制发送
        /// </summary>
        public bool HexSend
        {
            get => _hexSend;
            set => SetProperty(ref _hexSend, value);
        }

        private bool _hexDisplay;
        /// <summary>
        /// 是否以十六进制显示
        /// </summary>
        public bool HexDisplay
        {
            get => _hexDisplay;
            set => SetProperty(ref _hexDisplay, value);
        }

        private bool _sendNewLine = true;
        /// <summary>
        /// 是否发送新行
        /// </summary>
        public bool SendNewLine
        {
            get => _sendNewLine;
            set => SetProperty(ref _sendNewLine, value);
        }

        private bool _showSend = true;
        /// <summary>
        /// 是否显示发送内容
        /// </summary>
        public bool ShowSend
        {
            get => _showSend;
            set => SetProperty(ref _showSend, value);
        }

        private bool _autoSend;
        /// <summary>
        /// 是否自动发送
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
                RaisePropertyChanged(nameof(AutoSendVisibility));
            }
        }

        /// <summary>
        /// 自动发送设置可见性
        /// </summary>
        public Visibility AutoSendVisibility => AutoSend ? Visibility.Visible : Visibility.Collapsed;

        private int _autoSendInterval = 1000;
        /// <summary>
        /// 自动发送间隔(毫秒)
        /// </summary>
        public int AutoSendInterval
        {
            get => _autoSendInterval;
            set
            {
                if (value < 100) value = 100; // 最小100ms
                SetProperty(ref _autoSendInterval, value);
                if (_autoSendTimer != null)
                {
                    _autoSendTimer.Interval = TimeSpan.FromMilliseconds(value);
                }
            }
        }

        private bool _autoScroll = true;
        /// <summary>
        /// 是否自动滚动
        /// </summary>
        public bool AutoScroll
        {
            get => _autoScroll;
            set => SetProperty(ref _autoScroll, value);
        }

        private bool _showTime = true;
        /// <summary>
        /// 是否显示时间
        /// </summary>
        public bool ShowTime
        {
            get => _showTime;
            set => SetProperty(ref _showTime, value);
        }

        #endregion

        #region 数据属性

        private string _dataToSend = string.Empty;
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
        /// 发送字节计数
        /// </summary>
        public int SendCount
        {
            get => _sendCount;
            set => SetProperty(ref _sendCount, value);
        }

        private int _receiveCount;
        /// <summary>
        /// 接收字节计数
        /// </summary>
        public int ReceiveCount
        {
            get => _receiveCount;
            set => SetProperty(ref _receiveCount, value);
        }

        #endregion

        #region 连接状态属性

        private bool _isRunning;
        /// <summary>
        /// 是否正在运行
        /// </summary>
        public bool IsRunning
        {
            get => _isRunning;
            set
            {
                SetProperty(ref _isRunning, value);
                RaisePropertyChanged(nameof(StartButtonVisibility));
                RaisePropertyChanged(nameof(StopButtonVisibility));
                RaisePropertyChanged(nameof(CanSend));
            }
        }

        /// <summary>
        /// 启动按钮可见性
        /// </summary>
        public Visibility StartButtonVisibility => IsRunning ? Visibility.Collapsed : Visibility.Visible;

        /// <summary>
        /// 停止按钮可见性
        /// </summary>
        public Visibility StopButtonVisibility => IsRunning ? Visibility.Visible : Visibility.Collapsed;

        /// <summary>
        /// 是否可以发送数据
        /// </summary>
        public bool CanSend => IsRunning && (IsClient || (Connections != null && Connections.Count > 0));

        private ObservableCollection<TcpConnection> _connections;
        /// <summary>
        /// TCP连接列表
        /// </summary>
        public ObservableCollection<TcpConnection> Connections
        {
            get => _connections;
            set => SetProperty(ref _connections, value);
        }

        private TcpConnection _selectedConnection;
        /// <summary>
        /// 选中的TCP连接
        /// </summary>
        public TcpConnection SelectedConnection
        {
            get => _selectedConnection;
            set => SetProperty(ref _selectedConnection, value);
        }

        #endregion

        #region 命令

        /// <summary>
        /// 启动命令
        /// </summary>
        public DelegateCommand StartCommand { get; private set; }

        /// <summary>
        /// 停止命令
        /// </summary>
        public DelegateCommand StopCommand { get; private set; }

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
        /// 清零发送计数命令
        /// </summary>
        public DelegateCommand ClearSendCountCommand { get; private set; }

        /// <summary>
        /// 清零接收计数命令
        /// </summary>
        public DelegateCommand ClearReceiveCountCommand { get; private set; }

        #endregion

        /// <summary>
        /// 构造函数
        /// </summary>
        public TcpViewModel(ILoggingService loggingService)
        {
            _loggingService = loggingService;
            _loggingService.Info("TCP通信模块已初始化");

            // 初始化命令
            StartCommand = new DelegateCommand(Start);
            StopCommand = new DelegateCommand(Stop);
            SendDataCommand = new DelegateCommand(SendData);
            ClearSendDataCommand = new DelegateCommand(() => DataToSend = string.Empty);
            ClearReceiveDataCommand = new DelegateCommand(() => ReceivedData = string.Empty);
            SaveReceiveDataCommand = new DelegateCommand(SaveReceiveData);
            ClearSendCountCommand = new DelegateCommand(() => SendCount = 0);
            ClearReceiveCountCommand = new DelegateCommand(() => ReceiveCount = 0);

            // 初始化连接列表
            Connections = new ObservableCollection<TcpConnection>();

            // 初始化本地IP地址列表
            InitLocalIPs();
        }

        /// <summary>
        /// 初始化本地IP地址列表
        /// </summary>
        private void InitLocalIPs()
        {
            LocalIPs = new ObservableCollection<string>();
            LocalIPs.Add("0.0.0.0"); // 所有接口

            try
            {
                // 获取本机所有IP地址
                string hostName = Dns.GetHostName();
                IPHostEntry hostEntry = Dns.GetHostEntry(hostName);
                foreach (IPAddress ip in hostEntry.AddressList)
                {
                    if (ip.AddressFamily == AddressFamily.InterNetwork) // IPv4
                    {
                        LocalIPs.Add(ip.ToString());
                    }
                }
            }
            catch (Exception ex)
            {
                _loggingService.Error(ex, "获取本地IP地址失败");
                LocalIPs.Add("127.0.0.1");
            }
        }

        /// <summary>
        /// 启动TCP通信
        /// </summary>
        private void Start()
        {
            try
            {
                if (IsServer)
                {
                    StartServer();
                }
                else
                {
                    StartClient();
                }

                IsRunning = true;
                _loggingService.Info($"TCP{(IsServer ? "服务器" : "客户端")}已启动");
                AddReceiveData($"TCP{(IsServer ? "服务器" : "客户端")}已启动", "系统");
            }
            catch (Exception ex)
            {
                _loggingService.Error(ex, $"启动TCP{(IsServer ? "服务器" : "客户端")}失败");
                MessageBox.Show($"启动失败: {ex.Message}", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// 启动TCP服务器
        /// </summary>
        private void StartServer()
        {
            // 清空现有连接
            StopAllConnections();
            Connections.Clear();

            // 创建TCP监听器
            IPAddress ipAddress = IPAddress.Parse(SelectedLocalIP);
            _tcpListener = new TcpListener(ipAddress, ServerPort);
            _tcpListener.Start();

            // 开始接受客户端连接
            _listenerCts = new CancellationTokenSource();
            AcceptClientsAsync(_listenerCts.Token);

            _loggingService.Info($"TCP服务器已启动，监听 {SelectedLocalIP}:{ServerPort}");
        }

        /// <summary>
        /// 异步接受客户端连接
        /// </summary>
        private async void AcceptClientsAsync(CancellationToken cancellationToken)
        {
            int connectionId = 1;

            try
            {
                while (!cancellationToken.IsCancellationRequested)
                {
                    // 等待客户端连接
                    TcpClient client = await _tcpListener.AcceptTcpClientAsync();

                    // 创建新的连接对象
                    var connection = new TcpConnection
                    {
                        Id = connectionId++,
                        Client = client,
                        RemoteEndPoint = client.Client.RemoteEndPoint.ToString(),
                        Status = "已连接",
                        StatusColor = Brushes.Green,
                        Cts = new CancellationTokenSource()
                    };

                    // 添加到连接列表
                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        Connections.Add(connection);
                        RaisePropertyChanged(nameof(CanSend));
                    });

                    _loggingService.Info($"客户端已连接: {connection.RemoteEndPoint}");
                    AddReceiveData($"客户端已连接: {connection.RemoteEndPoint}", "系统");

                    // 开始接收数据
                    _tcpConnections.Add(connection);
                    ReceiveDataAsync(connection);
                }
            }
            catch (Exception ex) when (!(ex is OperationCanceledException))
            {
                _loggingService.Error(ex, "接受客户端连接时发生错误");
                if (!cancellationToken.IsCancellationRequested)
                {
                    AddReceiveData($"接受客户端连接错误: {ex.Message}", "错误");
                }
            }
        }

        /// <summary>
        /// 启动TCP客户端
        /// </summary>
        private async void StartClient()
        {
            // 创建TCP客户端
            _tcpClient = new TcpClient();

            try
            {
                // 连接到服务器
                await _tcpClient.ConnectAsync(ServerIP, ClientPort);

                // 创建连接对象
                var connection = new TcpConnection
                {
                    Id = 1,
                    Client = _tcpClient,
                    RemoteEndPoint = $"{ServerIP}:{ClientPort}",
                    Status = "已连接",
                    StatusColor = Brushes.Green,
                    Cts = new CancellationTokenSource()
                };

                // 添加到连接列表
                Application.Current.Dispatcher.Invoke(() =>
                {
                    Connections.Clear();
                    Connections.Add(connection);
                    SelectedConnection = connection;
                    RaisePropertyChanged(nameof(CanSend));
                });

                _loggingService.Info($"已连接到服务器: {ServerIP}:{ClientPort}");
                AddReceiveData($"已连接到服务器: {ServerIP}:{ClientPort}", "系统");

                // 开始接收数据
                _tcpConnections.Add(connection);
                ReceiveDataAsync(connection);
            }
            catch (Exception ex)
            {
                _tcpClient.Close();
                _tcpClient = null;
                throw new Exception($"连接服务器失败: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// 停止TCP通信
        /// </summary>
        private void Stop()
        {
            try
            {
                if (IsServer)
                {
                    // 停止服务器
                    if (_tcpListener != null)
                    {
                        _listenerCts?.Cancel();
                        _tcpListener.Stop();
                        _tcpListener = null;
                    }
                }
                else
                {
                    // 停止客户端
                    if (_tcpClient != null)
                    {
                        _tcpClient.Close();
                        _tcpClient = null;
                    }
                }

                // 停止所有连接
                StopAllConnections();

                // 停止自动发送
                if (AutoSend)
                {
                    AutoSend = false;
                }

                // 清空连接列表
                Application.Current.Dispatcher.Invoke(() =>
                {
                    Connections.Clear();
                    RaisePropertyChanged(nameof(CanSend));
                });

                IsRunning = false;
                _loggingService.Info($"TCP{(IsServer ? "服务器" : "客户端")}已停止");
                AddReceiveData($"TCP{(IsServer ? "服务器" : "客户端")}已停止", "系统");
            }
            catch (Exception ex)
            {
                _loggingService.Error(ex, $"停止TCP{(IsServer ? "服务器" : "客户端")}失败");
                AddReceiveData($"停止失败: {ex.Message}", "错误");
            }
        }

        /// <summary>
        /// 停止所有连接
        /// </summary>
        private void StopAllConnections()
        {
            foreach (var conn in _tcpConnections)
            {
                try
                {
                    conn.Cts?.Cancel();
                    conn.Client?.Close();
                }
                catch (Exception ex)
                {
                    _loggingService.Error(ex, $"关闭连接失败: {conn.RemoteEndPoint}");
                }
            }
            _tcpConnections.Clear();
        }

        /// <summary>
        /// 异步接收数据
        /// </summary>
        private async void ReceiveDataAsync(TcpConnection connection)
        {
            try
            {
                var cancellationToken = connection.Cts.Token;
                
                    NetworkStream stream = connection.Client.GetStream();
                    byte[] buffer = new byte[1024];

                    while (!cancellationToken.IsCancellationRequested && connection.Client.Connected)
                    {
                        try
                        {
                            int bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length, cancellationToken);

                            if (bytesRead > 0)
                            {
                                // 更新接收计数
                                ReceiveCount += bytesRead;

                                // 在UI线程上更新接收区
                                Application.Current.Dispatcher.Invoke(() =>
                                {
                                    if (HexDisplay)
                                    {
                                        AddReceiveData(BitConverter.ToString(buffer, 0, bytesRead).Replace("-", " "), "接收");
                                    }
                                    else
                                    {
                                        AddReceiveData(Encoding.UTF8.GetString(buffer, 0, bytesRead), "接收");
                                    }
                                });

                                _loggingService.Debug($"从 {connection.RemoteEndPoint} 接收数据: {bytesRead} 字节");
                            }
                            else
                            {
                                // 客户端断开连接
                                break;
                            }
                        }
                        catch (IOException)
                        {
                            // 连接已关闭
                            break;
                        }
                    }
                
            }
            catch (Exception ex) when (!(ex is OperationCanceledException))
            {
                _loggingService.Error(ex, $"接收数据错误: {connection.RemoteEndPoint}");
            }
            finally
            {
                // 连接已断开
                connection.Client.Close();
                _tcpConnections.Remove(connection);

                Application.Current.Dispatcher.Invoke(() =>
                {
                    if (Connections.Contains(connection))
                    {
                        connection.Status = "已断开";
                        connection.StatusColor = Brushes.Red;

                        // 更新UI上的状态
                        var index = Connections.IndexOf(connection);
                        Connections[index] = connection;

                        RaisePropertyChanged(nameof(CanSend));
                    }
                });

                _loggingService.Info($"连接已断开: {connection.RemoteEndPoint}");
                AddReceiveData($"连接已断开: {connection.RemoteEndPoint}", "系统");
            }
        }

        /// <summary>
        /// 发送数据
        /// </summary>
        private void SendData()
        {
            if (string.IsNullOrEmpty(DataToSend))
            {
                return;
            }

            try
            {
                // 准备发送数据
                string dataToSend = DataToSend;
                if (SendNewLine)
                {
                    dataToSend += "\r\n";
                }

                byte[] data;
                if (HexSend)
                {
                    // 十六进制发送
                    data = HexStringToByteArray(dataToSend);
                }
                else
                {
                    // 文本发送
                    data = Encoding.UTF8.GetBytes(dataToSend);
                }

                // 发送数据
                if (IsClient)
                {
                    // 客户端模式，发送到服务器
                    if (_tcpClient != null && _tcpClient.Connected)
                    {
                        _tcpClient.GetStream().Write(data, 0, data.Length);
                        SendCount += data.Length;
                        _loggingService.Debug($"客户端发送数据: {data.Length} 字节");
                    }
                }
                else
                {
                    // 服务器模式，发送到选中的客户端
                    if (SelectedConnection != null && SelectedConnection.Client.Connected)
                    {
                        SelectedConnection.Client.GetStream().Write(data, 0, data.Length);
                        SendCount += data.Length;
                        _loggingService.Debug($"向 {SelectedConnection.RemoteEndPoint} 发送数据: {data.Length} 字节");
                    }
                    else
                    {
                        throw new Exception("未选择有效的客户端连接");
                    }
                }

                // 显示发送数据
                if (ShowSend)
                {
                    if (HexDisplay)
                    {
                        AddReceiveData(BitConverter.ToString(data).Replace("-", " "), "发送");
                    }
                    else
                    {
                        AddReceiveData(Encoding.UTF8.GetString(data), "发送");
                    }
                }
            }
            catch (Exception ex)
            {
                _loggingService.Error(ex, "发送数据失败");
                MessageBox.Show($"发送数据失败: {ex.Message}", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
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
                string fileName = $"TcpData_{DateTime.Now:yyyyMMdd_HHmmss}.txt";
                string filePath = System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), fileName);

                // 保存数据内容
                System.IO.File.WriteAllText(filePath, ReceivedData);

                // 记录日志
                _loggingService.Info($"TCP通信数据已保存至: {filePath}");

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
            if (CanSend)
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
        ~TcpViewModel()
        {
            Stop();
        }
    }
}