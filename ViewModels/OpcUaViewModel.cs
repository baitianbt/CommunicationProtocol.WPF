using Prism.Commands;
using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Input;
using CommunicationProtocol.WPF.Services.LoggingService;

namespace CommunicationProtocol.WPF.ViewModels
{
    /// <summary>
    /// OPC UA视图模型
    /// </summary>
    public class OpcUaViewModel : BindableBase
    {
        private readonly ILoggingService _loggingService;

        #region 连接参数属性

        private string _endpointUrl = "opc.tcp://localhost:4840";
        /// <summary>
        /// 终结点URL
        /// </summary>
        public string EndpointUrl
        {
            get => _endpointUrl;
            set => SetProperty(ref _endpointUrl, value);
        }

        private ObservableCollection<string> _securityPolicies;
        /// <summary>
        /// 安全策略列表
        /// </summary>
        public ObservableCollection<string> SecurityPolicies
        {
            get => _securityPolicies;
            set => SetProperty(ref _securityPolicies, value);
        }

        private string _selectedSecurityPolicy = "None";
        /// <summary>
        /// 选中的安全策略
        /// </summary>
        public string SelectedSecurityPolicy
        {
            get => _selectedSecurityPolicy;
            set => SetProperty(ref _selectedSecurityPolicy, value);
        }

        private ObservableCollection<string> _securityModes;
        /// <summary>
        /// 消息安全模式列表
        /// </summary>
        public ObservableCollection<string> SecurityModes
        {
            get => _securityModes;
            set => SetProperty(ref _securityModes, value);
        }

        private string _selectedSecurityMode = "None";
        /// <summary>
        /// 选中的消息安全模式
        /// </summary>
        public string SelectedSecurityMode
        {
            get => _selectedSecurityMode;
            set => SetProperty(ref _selectedSecurityMode, value);
        }

        private ObservableCollection<string> _authenticationTypes;
        /// <summary>
        /// 认证类型列表
        /// </summary>
        public ObservableCollection<string> AuthenticationTypes
        {
            get => _authenticationTypes;
            set => SetProperty(ref _authenticationTypes, value);
        }

        private string _selectedAuthenticationType = "匿名";
        /// <summary>
        /// 选中的认证类型
        /// </summary>
        public string SelectedAuthenticationType
        {
            get => _selectedAuthenticationType;
            set
            {
                SetProperty(ref _selectedAuthenticationType, value);
                RaisePropertyChanged(nameof(UsernamePasswordVisibility));
                RaisePropertyChanged(nameof(CertificateVisibility));
            }
        }

        /// <summary>
        /// 用户名密码认证可见性
        /// </summary>
        public Visibility UsernamePasswordVisibility => _selectedAuthenticationType == "用户名密码" ? Visibility.Visible : Visibility.Collapsed;

        /// <summary>
        /// 证书认证可见性
        /// </summary>
        public Visibility CertificateVisibility => _selectedAuthenticationType == "证书" ? Visibility.Visible : Visibility.Collapsed;

        private string _username;
        /// <summary>
        /// 用户名
        /// </summary>
        public string Username
        {
            get => _username;
            set => SetProperty(ref _username, value);
        }

        private string _password;
        /// <summary>
        /// 密码
        /// </summary>
        public string Password
        {
            get => _password;
            set => SetProperty(ref _password, value);
        }

        private string _certificateFilePath;
        /// <summary>
        /// 证书文件路径
        /// </summary>
        public string CertificateFilePath
        {
            get => _certificateFilePath;
            set => SetProperty(ref _certificateFilePath, value);
        }

        private int _connectionTimeout = 10000;
        /// <summary>
        /// 连接超时(ms)
        /// </summary>
        public int ConnectionTimeout
        {
            get => _connectionTimeout;
            set => SetProperty(ref _connectionTimeout, value);
        }

        private int _sessionTimeout = 60000;
        /// <summary>
        /// 会话超时(ms)
        /// </summary>
        public int SessionTimeout
        {
            get => _sessionTimeout;
            set => SetProperty(ref _sessionTimeout, value);
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

        #region 节点浏览属性

        /// <summary>
        /// OPC UA节点类
        /// </summary>
        public class OpcUaNode
        {
            /// <summary>
            /// 节点ID
            /// </summary>
            public string NodeId { get; set; }

            /// <summary>
            /// 显示名称
            /// </summary>
            public string DisplayName { get; set; }

            /// <summary>
            /// 节点类型
            /// </summary>
            public string NodeClass { get; set; }

            /// <summary>
            /// 数据类型
            /// </summary>
            public string DataType { get; set; }

            /// <summary>
            /// 描述
            /// </summary>
            public string Description { get; set; }

            /// <summary>
            /// 图标名称
            /// </summary>
            public string NodeIcon
            {
                get
                {
                    switch (NodeClass)
                    {
                        case "Object": return "CubeOutline";
                        case "Variable": return "Variable";
                        case "Method": return "Function";
                        case "ObjectType": return "CubeUnfolded";
                        case "VariableType": return "FormatListBulleted";
                        case "ReferenceType": return "Link";
                        case "DataType": return "Database";
                        case "View": return "Eye";
                        default: return "HelpCircleOutline";
                    }
                }
            }

            /// <summary>
            /// 子节点
            /// </summary>
            public ObservableCollection<OpcUaNode> Children { get; set; } = new ObservableCollection<OpcUaNode>();
        }

        private ObservableCollection<OpcUaNode> _rootNodes = new ObservableCollection<OpcUaNode>();
        /// <summary>
        /// 根节点集合
        /// </summary>
        public ObservableCollection<OpcUaNode> RootNodes
        {
            get => _rootNodes;
            set => SetProperty(ref _rootNodes, value);
        }

        private object _selectedNode;
        /// <summary>
        /// 选中的节点
        /// </summary>
        public object SelectedNode
        {
            get => _selectedNode;
            set
            {
                SetProperty(ref _selectedNode, value);
                RaisePropertyChanged(nameof(CanReadSelectedNode));
                RaisePropertyChanged(nameof(CanSubscribeSelectedNode));

                // 如果选中了节点，更新当前浏览路径
                if (value is OpcUaNode node)
                {
                    CurrentBrowsePath = node.NodeId;
                    _loggingService.Debug($"选中节点: {node.NodeId} - {node.DisplayName}");
                }
            }
        }

        private string _currentBrowsePath;
        /// <summary>
        /// 当前浏览路径
        /// </summary>
        public string CurrentBrowsePath
        {
            get => _currentBrowsePath;
            set => SetProperty(ref _currentBrowsePath, value);
        }

        /// <summary>
        /// 是否可以读取选中节点
        /// </summary>
        public bool CanReadSelectedNode => IsConnected && _selectedNode is OpcUaNode node &&
                                          (node.NodeClass == "Variable" || node.NodeClass == "Property");

        /// <summary>
        /// 是否可以订阅选中节点
        /// </summary>
        public bool CanSubscribeSelectedNode => IsConnected && _selectedNode is OpcUaNode node &&
                                               (node.NodeClass == "Variable" || node.NodeClass == "Property");

        #endregion

        #region 节点读写属性

        private string _nodeId;
        /// <summary>
        /// 节点ID
        /// </summary>
        public string NodeId
        {
            get => _nodeId;
            set => SetProperty(ref _nodeId, value);
        }

        private string _nodeValue;
        /// <summary>
        /// 节点值
        /// </summary>
        public string NodeValue
        {
            get => _nodeValue;
            set => SetProperty(ref _nodeValue, value);
        }

        private bool _nodeIsReadOnly = true;
        /// <summary>
        /// 节点是否只读
        /// </summary>
        public bool NodeIsReadOnly
        {
            get => _nodeIsReadOnly;
            set => SetProperty(ref _nodeIsReadOnly, value);
        }

        private string _nodeDataType;
        /// <summary>
        /// 节点数据类型
        /// </summary>
        public string NodeDataType
        {
            get => _nodeDataType;
            set => SetProperty(ref _nodeDataType, value);
        }

        private string _nodeAccessRights;
        /// <summary>
        /// 节点访问权限
        /// </summary>
        public string NodeAccessRights
        {
            get => _nodeAccessRights;
            set => SetProperty(ref _nodeAccessRights, value);
        }

        private string _nodeTimestamp;
        /// <summary>
        /// 节点时间戳
        /// </summary>
        public string NodeTimestamp
        {
            get => _nodeTimestamp;
            set => SetProperty(ref _nodeTimestamp, value);
        }

        private string _nodeStatusCode;
        /// <summary>
        /// 节点状态码
        /// </summary>
        public string NodeStatusCode
        {
            get => _nodeStatusCode;
            set => SetProperty(ref _nodeStatusCode, value);
        }

        private string _nodeDescription;
        /// <summary>
        /// 节点描述
        /// </summary>
        public string NodeDescription
        {
            get => _nodeDescription;
            set => SetProperty(ref _nodeDescription, value);
        }

        /// <summary>
        /// 是否可以写入节点
        /// </summary>
        public bool CanWriteNode => IsConnected && !NodeIsReadOnly && !string.IsNullOrWhiteSpace(NodeId);

        #endregion

        #region 订阅属性

        /// <summary>
        /// 订阅节点信息类
        /// </summary>
        public class SubscribedNodeInfo
        {
            /// <summary>
            /// 节点ID
            /// </summary>
            public string NodeId { get; set; }

            /// <summary>
            /// 显示名称
            /// </summary>
            public string DisplayName { get; set; }

            /// <summary>
            /// 数据类型
            /// </summary>
            public string DataType { get; set; }

            /// <summary>
            /// 当前值
            /// </summary>
            public string Value { get; set; }

            /// <summary>
            /// 时间戳
            /// </summary>
            public string Timestamp { get; set; }

            /// <summary>
            /// 订阅ID
            /// </summary>
            public uint SubscriptionId { get; set; }
        }

        private ObservableCollection<SubscribedNodeInfo> _subscribedNodes = new ObservableCollection<SubscribedNodeInfo>();
        /// <summary>
        /// 已订阅节点列表
        /// </summary>
        public ObservableCollection<SubscribedNodeInfo> SubscribedNodes
        {
            get => _subscribedNodes;
            set => SetProperty(ref _subscribedNodes, value);
        }

        private int _publishingInterval = 1000;
        /// <summary>
        /// 发布间隔(ms)
        /// </summary>
        public int PublishingInterval
        {
            get => _publishingInterval;
            set => SetProperty(ref _publishingInterval, value);
        }

        /// <summary>
        /// 是否存在活跃订阅
        /// </summary>
        public bool HasActiveSubscription => SubscribedNodes.Count > 0;

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

        private ObservableCollection<string> _logLevels;
        /// <summary>
        /// 日志级别列表
        /// </summary>
        public ObservableCollection<string> LogLevels
        {
            get => _logLevels;
            set => SetProperty(ref _logLevels, value);
        }

        private string _selectedLogLevel = "信息";
        /// <summary>
        /// 选中的日志级别
        /// </summary>
        public string SelectedLogLevel
        {
            get => _selectedLogLevel;
            set => SetProperty(ref _selectedLogLevel, value);
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
        /// 浏览证书文件命令
        /// </summary>
        public DelegateCommand BrowseCertificateCommand { get; private set; }

        /// <summary>
        /// 刷新节点命令
        /// </summary>
        public DelegateCommand RefreshNodesCommand { get; private set; }

        /// <summary>
        /// 读取节点值命令
        /// </summary>
        public DelegateCommand ReadNodeValueCommand { get; private set; }

        /// <summary>
        /// 订阅节点命令
        /// </summary>
        public DelegateCommand SubscribeNodeCommand { get; private set; }

        /// <summary>
        /// 读取节点命令（根据ID）
        /// </summary>
        public DelegateCommand ReadNodeCommand { get; private set; }

        /// <summary>
        /// 写入节点命令
        /// </summary>
        public DelegateCommand WriteNodeCommand { get; private set; }

        /// <summary>
        /// 应用发布间隔命令
        /// </summary>
        public DelegateCommand ApplyPublishingIntervalCommand { get; private set; }

        /// <summary>
        /// 取消订阅节点命令
        /// </summary>
        public DelegateCommand<SubscribedNodeInfo> UnsubscribeNodeCommand { get; private set; }

        /// <summary>
        /// 取消全部订阅命令
        /// </summary>
        public DelegateCommand UnsubscribeAllCommand { get; private set; }

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
        public OpcUaViewModel(ILoggingService loggingService)
        {
            _loggingService = loggingService;

            InitializeCollections();
            InitializeCommands();

            _loggingService.Info("OPC UA视图模型已初始化");
        }

        /// <summary>
        /// 初始化集合
        /// </summary>
        private void InitializeCollections()
        {
            // 初始化安全策略列表
            SecurityPolicies = new ObservableCollection<string>
    {
        "None",
        "Basic128",
        "Basic256",
        "Basic256Sha256"
    };

            // 初始化消息安全模式列表
            SecurityModes = new ObservableCollection<string>
    {
        "None",
        "Sign",
        "SignAndEncrypt"
    };

            // 初始化认证类型列表
            AuthenticationTypes = new ObservableCollection<string>
    {
        "匿名",
        "用户名密码",
        "证书"
    };

            // 初始化日志级别列表
            LogLevels = new ObservableCollection<string>
    {
        "调试",
        "信息",
        "警告",
        "错误"
    };

            // 初始化一些示例节点（实际应该在连接后从服务器获取）
            InitializeExampleNodes();
        }

        /// <summary>
        /// 初始化示例节点（实际环境中应从服务器获取）
        /// </summary>
        private void InitializeExampleNodes()
        {
            var serverNode = new OpcUaNode
            {
                NodeId = "ns=0;i=2253",
                DisplayName = "服务器",
                NodeClass = "Object",
                Description = "OPC UA服务器对象"
            };

            var statusNode = new OpcUaNode
            {
                NodeId = "ns=0;i=2256",
                DisplayName = "服务器状态",
                NodeClass = "Variable",
                DataType = "ServerStatusDataType",
                Description = "服务器的当前状态"
            };

            var currentTimeNode = new OpcUaNode
            {
                NodeId = "ns=0;i=2258",
                DisplayName = "当前时间",
                NodeClass = "Variable",
                DataType = "DateTime",
                Description = "服务器的当前时间"
            };

            serverNode.Children.Add(statusNode);
            serverNode.Children.Add(currentTimeNode);

            var dataNode = new OpcUaNode
            {
                NodeId = "ns=2;s=Demo",
                DisplayName = "演示数据",
                NodeClass = "Object",
                Description = "演示数据对象"
            };

            var temperatureNode = new OpcUaNode
            {
                NodeId = "ns=2;s=Demo.Temperature",
                DisplayName = "温度",
                NodeClass = "Variable",
                DataType = "Double",
                Description = "温度传感器数据"
            };

            var pressureNode = new OpcUaNode
            {
                NodeId = "ns=2;s=Demo.Pressure",
                DisplayName = "压力",
                NodeClass = "Variable",
                DataType = "Double",
                Description = "压力传感器数据"
            };

            dataNode.Children.Add(temperatureNode);
            dataNode.Children.Add(pressureNode);

            RootNodes.Add(serverNode);
            RootNodes.Add(dataNode);
        }

        /// <summary>
        /// 初始化命令
        /// </summary>
        private void InitializeCommands()
        {
            ConnectCommand = new DelegateCommand(Connect);
            DisconnectCommand = new DelegateCommand(Disconnect);
            BrowseCertificateCommand = new DelegateCommand(BrowseCertificate);
            RefreshNodesCommand = new DelegateCommand(RefreshNodes);
            ReadNodeValueCommand = new DelegateCommand(ReadNodeValue);
            SubscribeNodeCommand = new DelegateCommand(SubscribeNode);
            ReadNodeCommand = new DelegateCommand(ReadNode);
            WriteNodeCommand = new DelegateCommand(WriteNode);
            ApplyPublishingIntervalCommand = new DelegateCommand(ApplyPublishingInterval);
            UnsubscribeNodeCommand = new DelegateCommand<SubscribedNodeInfo>(UnsubscribeNode);
            UnsubscribeAllCommand = new DelegateCommand(UnsubscribeAll);
            SaveLogCommand = new DelegateCommand(SaveLog);
            ClearLogCommand = new DelegateCommand(ClearLog);
        }

        /// <summary>
        /// 连接服务器
        /// </summary>
        private void Connect()
        {
            try
            {
                // 这里应该添加实际的连接代码，这里只是模拟
                _loggingService.Info("开始连接OPC UA服务器...");
                _loggingService.Info($"OPC UA连接参数: URL={EndpointUrl}, 安全策略={SelectedSecurityPolicy}, 安全模式={SelectedSecurityMode}, 认证类型={SelectedAuthenticationType}");

                // 根据认证类型记录不同的认证信息
                switch (SelectedAuthenticationType)
                {
                    case "用户名密码":
                        _loggingService.Info($"认证信息: 用户名={Username}");
                        break;
                    case "证书":
                        _loggingService.Info($"认证信息: 证书路径={CertificateFilePath}");
                        break;
                    default:
                        _loggingService.Info("认证信息: 匿名");
                        break;
                }

                // 模拟连接延迟
                System.Threading.Thread.Sleep(500);

                // 连接成功
                IsConnected = true;

                // 记录日志
                AddLog("连接成功", "系统");
                _loggingService.Info("OPC UA服务器连接成功");
            }
            catch (Exception ex)
            {
                // 连接失败
                IsConnected = false;

                // 记录日志
                AddLog($"连接失败: {ex.Message}", "错误");
                _loggingService.Error(ex, "OPC UA服务器连接失败");

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
                _loggingService.Info("断开OPC UA服务器连接...");

                // 取消所有订阅
                UnsubscribeAll();

                // 模拟断开延迟
                System.Threading.Thread.Sleep(200);

                // 断开成功
                IsConnected = false;

                // 记录日志
                AddLog("断开连接", "系统");
                _loggingService.Info("OPC UA服务器断开成功");
            }
            catch (Exception ex)
            {
                // 记录日志
                AddLog($"断开连接失败: {ex.Message}", "错误");
                _loggingService.Error(ex, "OPC UA服务器断开失败");

                // 显示错误消息
                MessageBox.Show($"断开连接失败: {ex.Message}", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// 浏览证书文件
        /// </summary>
        private void BrowseCertificate()
        {
            try
            {
                // 这里应该添加实际的文件对话框代码，这里只是模拟
                _loggingService.Info("浏览证书文件...");

                // 模拟选择证书文件
                CertificateFilePath = @"C:\Certificates\client.pfx";

                // 记录日志
                AddLog($"已选择证书文件: {CertificateFilePath}", "系统");
                _loggingService.Info($"已选择证书文件: {CertificateFilePath}");
            }
            catch (Exception ex)
            {
                // 记录日志
                AddLog($"选择证书文件失败: {ex.Message}", "错误");
                _loggingService.Error(ex, "选择证书文件失败");

                // 显示错误消息
                MessageBox.Show($"选择证书文件失败: {ex.Message}", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// 刷新节点
        /// </summary>
        private void RefreshNodes()
        {
            try
            {
                if (!IsConnected)
                {
                    MessageBox.Show("请先连接服务器", "提示", MessageBoxButton.OK, MessageBoxImage.Information);
                    return;
                }

                // 记录日志
                AddLog("刷新节点", "系统");
                _loggingService.Info("刷新OPC UA节点");

                // 这里应该添加实际的刷新节点代码，这里只是模拟
                // 模拟刷新延迟
                System.Threading.Thread.Sleep(300);

                // 清空并重新初始化节点
                RootNodes.Clear();
                InitializeExampleNodes();

                // 记录日志
                AddLog("节点刷新成功", "系统");
                _loggingService.Info("OPC UA节点刷新成功");
            }
            catch (Exception ex)
            {
                // 记录日志
                AddLog($"节点刷新失败: {ex.Message}", "错误");
                _loggingService.Error(ex, "OPC UA节点刷新失败");

                // 显示错误消息
                MessageBox.Show($"节点刷新失败: {ex.Message}", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// 读取所选节点值
        /// </summary>
        private void ReadNodeValue()
        {
            try
            {
                if (!IsConnected)
                {
                    MessageBox.Show("请先连接服务器", "提示", MessageBoxButton.OK, MessageBoxImage.Information);
                    return;
                }

                if (!(SelectedNode is OpcUaNode selectedNode))
                {
                    MessageBox.Show("请选择一个节点", "提示", MessageBoxButton.OK, MessageBoxImage.Information);
                    return;
                }

                if (selectedNode.NodeClass != "Variable" && selectedNode.NodeClass != "Property")
                {
                    MessageBox.Show("只能读取变量或属性节点", "提示", MessageBoxButton.OK, MessageBoxImage.Information);
                    return;
                }

                // 记录读取信息
                AddLog($"开始读取节点: {selectedNode.NodeId} - {selectedNode.DisplayName}", "发送");

                // 这里应该添加实际的读取代码，这里只是模拟
                System.Threading.Thread.Sleep(200);

                // 准备节点属性
                NodeId = selectedNode.NodeId;
                NodeDataType = selectedNode.DataType ?? "未知";
                NodeAccessRights = "读写";
                NodeTimestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff");
                NodeStatusCode = "Good";
                NodeDescription = selectedNode.Description ?? string.Empty;
                NodeIsReadOnly = false;

                // 生成模拟值
                string value = GenerateMockValueForDataType(selectedNode.DataType);
                NodeValue = value;

                // 记录日志
                AddLog($"读取成功，结果: {value}", "接收");
                _loggingService.Info($"OPC UA读取成功: {selectedNode.NodeId} - {selectedNode.DisplayName}");
            }
            catch (Exception ex)
            {
                // 记录日志
                AddLog($"读取失败: {ex.Message}", "错误");
                _loggingService.Error(ex, "OPC UA读取失败");

                // 显示错误消息
                MessageBox.Show($"读取失败: {ex.Message}", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// 订阅节点
        /// </summary>
        private void SubscribeNode()
        {
            try
            {
                if (!IsConnected)
                {
                    MessageBox.Show("请先连接服务器", "提示", MessageBoxButton.OK, MessageBoxImage.Information);
                    return;
                }

                if (!(SelectedNode is OpcUaNode selectedNode))
                {
                    MessageBox.Show("请选择一个节点", "提示", MessageBoxButton.OK, MessageBoxImage.Information);
                    return;
                }

                if (selectedNode.NodeClass != "Variable" && selectedNode.NodeClass != "Property")
                {
                    MessageBox.Show("只能订阅变量或属性节点", "提示", MessageBoxButton.OK, MessageBoxImage.Information);
                    return;
                }

                // 检查是否已订阅
                foreach (var subscribedNode in SubscribedNodes)
                {
                    if (subscribedNode.NodeId == selectedNode.NodeId)
                    {
                        MessageBox.Show("该节点已被订阅", "提示", MessageBoxButton.OK, MessageBoxImage.Information);
                        return;
                    }
                }

                // 记录订阅信息
                AddLog($"开始订阅节点: {selectedNode.NodeId} - {selectedNode.DisplayName}, 间隔: {PublishingInterval}ms", "发送");

                // 这里应该添加实际的订阅代码，这里只是模拟
                System.Threading.Thread.Sleep(200);

                // 创建订阅信息
                var nodeInfo = new SubscribedNodeInfo
                {
                    NodeId = selectedNode.NodeId,
                    DisplayName = selectedNode.DisplayName,
                    DataType = selectedNode.DataType ?? "未知",
                    Value = GenerateMockValueForDataType(selectedNode.DataType),
                    Timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff"),
                    SubscriptionId = (uint)new Random().Next(1, 1000)
                };

                // 添加到订阅列表
                SubscribedNodes.Add(nodeInfo);

                // 记录日志
                AddLog($"订阅成功，ID: {nodeInfo.SubscriptionId}", "接收");
                _loggingService.Info($"OPC UA订阅成功: {selectedNode.NodeId} - {selectedNode.DisplayName}");

                // 更新属性
                RaisePropertyChanged(nameof(HasActiveSubscription));
            }
            catch (Exception ex)
            {
                // 记录日志
                AddLog($"订阅失败: {ex.Message}", "错误");
                _loggingService.Error(ex, "OPC UA订阅失败");

                // 显示错误消息
                MessageBox.Show($"订阅失败: {ex.Message}", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// 根据ID读取节点
        /// </summary>
        private void ReadNode()
        {
            try
            {
                if (!IsConnected)
                {
                    MessageBox.Show("请先连接服务器", "提示", MessageBoxButton.OK, MessageBoxImage.Information);
                    return;
                }

                if (string.IsNullOrWhiteSpace(NodeId))
                {
                    MessageBox.Show("请输入节点ID", "提示", MessageBoxButton.OK, MessageBoxImage.Information);
                    return;
                }

                // 记录读取信息
                AddLog($"开始读取节点: {NodeId}", "发送");

                // 这里应该添加实际的读取代码，这里只是模拟
                System.Threading.Thread.Sleep(200);

                // 准备节点属性
                NodeDataType = "Double";
                NodeAccessRights = "读写";
                NodeTimestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff");
                NodeStatusCode = "Good";
                NodeDescription = "用户指定的节点";
                NodeIsReadOnly = false;

                // 生成模拟值
                string value = GenerateMockValueForDataType("Double");
                NodeValue = value;

                // 记录日志
                AddLog($"读取成功，结果: {value}", "接收");
                _loggingService.Info($"OPC UA读取成功: {NodeId}");
            }
            catch (Exception ex)
            {
                // 记录日志
                AddLog($"读取失败: {ex.Message}", "错误");
                _loggingService.Error(ex, "OPC UA读取失败");

                // 显示错误消息
                MessageBox.Show($"读取失败: {ex.Message}", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// 写入节点
        /// </summary>
        private void WriteNode()
        {
            try
            {
                if (!IsConnected)
                {
                    MessageBox.Show("请先连接服务器", "提示", MessageBoxButton.OK, MessageBoxImage.Information);
                    return;
                }

                if (string.IsNullOrWhiteSpace(NodeId))
                {
                    MessageBox.Show("请输入节点ID", "提示", MessageBoxButton.OK, MessageBoxImage.Information);
                    return;
                }

                if (string.IsNullOrWhiteSpace(NodeValue))
                {
                    MessageBox.Show("请输入要写入的值", "提示", MessageBoxButton.OK, MessageBoxImage.Information);
                    return;
                }

                // 记录写入信息
                AddLog($"开始写入节点: {NodeId}, 值: {NodeValue}", "发送");

                // 这里应该添加实际的写入代码，这里只是模拟
                System.Threading.Thread.Sleep(200);

                // 记录日志
                AddLog("写入成功", "接收");
                _loggingService.Info($"OPC UA写入成功: {NodeId}, 值: {NodeValue}");

                // 更新时间戳
                NodeTimestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff");
            }
            catch (Exception ex)
            {
                // 记录日志
                AddLog($"写入失败: {ex.Message}", "错误");
                _loggingService.Error(ex, "OPC UA写入失败");

                // 显示错误消息
                MessageBox.Show($"写入失败: {ex.Message}", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// 应用发布间隔
        /// </summary>
        private void ApplyPublishingInterval()
        {
            try
            {
                if (!IsConnected)
                {
                    MessageBox.Show("请先连接服务器", "提示", MessageBoxButton.OK, MessageBoxImage.Information);
                    return;
                }

                if (SubscribedNodes.Count == 0)
                {
                    MessageBox.Show("没有活跃的订阅", "提示", MessageBoxButton.OK, MessageBoxImage.Information);
                    return;
                }

                // 记录应用信息
                AddLog($"应用发布间隔: {PublishingInterval}ms", "系统");

                // 这里应该添加实际的应用代码，这里只是模拟
                System.Threading.Thread.Sleep(100);

                // 记录日志
                AddLog("发布间隔应用成功", "系统");
                _loggingService.Info($"OPC UA发布间隔应用成功: {PublishingInterval}ms");
            }
            catch (Exception ex)
            {
                // 记录日志
                AddLog($"应用发布间隔失败: {ex.Message}", "错误");
                _loggingService.Error(ex, "OPC UA应用发布间隔失败");

                // 显示错误消息
                MessageBox.Show($"应用发布间隔失败: {ex.Message}", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// 取消订阅节点
        /// </summary>
        private void UnsubscribeNode(SubscribedNodeInfo nodeInfo)
        {
            try
            {
                if (!IsConnected)
                {
                    MessageBox.Show("请先连接服务器", "提示", MessageBoxButton.OK, MessageBoxImage.Information);
                    return;
                }

                // 记录取消订阅信息
                AddLog($"取消订阅节点: {nodeInfo.NodeId} - {nodeInfo.DisplayName}, 订阅ID: {nodeInfo.SubscriptionId}", "发送");

                // 这里应该添加实际的取消订阅代码，这里只是模拟
                System.Threading.Thread.Sleep(100);

                // 从订阅列表中移除
                SubscribedNodes.Remove(nodeInfo);

                // 记录日志
                AddLog("取消订阅成功", "接收");
                _loggingService.Info($"OPC UA取消订阅成功: {nodeInfo.NodeId} - {nodeInfo.DisplayName}");

                // 更新属性
                RaisePropertyChanged(nameof(HasActiveSubscription));
            }
            catch (Exception ex)
            {
                // 记录日志
                AddLog($"取消订阅失败: {ex.Message}", "错误");
                _loggingService.Error(ex, "OPC UA取消订阅失败");

                // 显示错误消息
                MessageBox.Show($"取消订阅失败: {ex.Message}", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// 取消全部订阅
        /// </summary>
        private void UnsubscribeAll()
        {
            try
            {
                if (!IsConnected)
                {
                    MessageBox.Show("请先连接服务器", "提示", MessageBoxButton.OK, MessageBoxImage.Information);
                    return;
                }

                if (SubscribedNodes.Count == 0)
                {
                    MessageBox.Show("没有活跃的订阅", "提示", MessageBoxButton.OK, MessageBoxImage.Information);
                    return;
                }

                // 记录取消全部订阅信息
                AddLog("取消全部订阅", "系统");

                // 这里应该添加实际的取消全部订阅代码，这里只是模拟
                System.Threading.Thread.Sleep(200);

                // 清空订阅列表
                SubscribedNodes.Clear();

                // 记录日志
                AddLog("取消全部订阅成功", "系统");
                _loggingService.Info("OPC UA取消全部订阅成功");

                // 更新属性
                RaisePropertyChanged(nameof(HasActiveSubscription));
            }
            catch (Exception ex)
            {
                // 记录日志
                AddLog($"取消全部订阅失败: {ex.Message}", "错误");
                _loggingService.Error(ex, "OPC UA取消全部订阅失败");

                // 显示错误消息
                MessageBox.Show($"取消全部订阅失败: {ex.Message}", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
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
                string logFileName = $"OpcUaLog_{DateTime.Now:yyyyMMdd_HHmmss}.txt";
                string logFilePath = System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), logFileName);

                // 保存日志内容
                System.IO.File.WriteAllText(logFilePath, LogContent);

                // 记录日志
                _loggingService.Info($"OPC UA日志已保存至: {logFilePath}");

                // 显示成功消息
                MessageBox.Show($"日志已保存至: {logFilePath}", "成功", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                // 记录日志
                _loggingService.Error(ex, "保存OPC UA日志失败");

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
            _loggingService.Info("OPC UA日志已清空");
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
                _loggingService.Error($"OPC UA错误: {message}");
            }
            else
            {
                _loggingService.Debug($"OPC UA: {message}");
            }
        }

        /// <summary>
        /// 为指定的数据类型生成模拟值
        /// </summary>
        private string GenerateMockValueForDataType(string dataType)
        {
            var random = new Random();

            switch (dataType)
            {
                case "Boolean":
                    return random.Next(2) == 1 ? "True" : "False";
                case "Byte":
                    return random.Next(0, 255).ToString();
                case "Int16":
                    return random.Next(-32768, 32767).ToString();
                case "UInt16":
                    return random.Next(0, 65535).ToString();
                case "Int32":
                    return random.Next(-2147483647, 2147483647).ToString();
                case "UInt32":
                    return random.Next(0, 2147483647).ToString();
                case "Float":
                case "Double":
                    return Math.Round(random.NextDouble() * 100, 2).ToString();
                case "DateTime":
                    return DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff");
                case "String":
                    char[] chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789".ToCharArray();
                    string str = "";
                    for (int i = 0; i < 8; i++)
                    {
                        str += chars[random.Next(chars.Length)];
                    }
                    return str;
                default:
                    return random.Next(0, 100).ToString();
            }
        }

    }
}