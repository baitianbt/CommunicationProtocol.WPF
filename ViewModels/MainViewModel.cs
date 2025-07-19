using Prism.Commands;
using Prism.Mvvm;
using Prism.Regions;
using System;
using System.Reflection;
using CommunicationProtocol.WPF.Services.LoggingService;

namespace CommunicationProtocol.WPF.ViewModels
{
    /// <summary>
    /// 主窗口视图模型
    /// </summary>
    public class MainViewModel : BindableBase
    {
        private readonly IRegionManager _regionManager;
        private readonly ILoggingService _loggingService;
        
        private string _statusMessage = "就绪";
        private string _version = $"v{Assembly.GetExecutingAssembly().GetName().Version}";
        
        /// <summary>
        /// 状态消息
        /// </summary>
        public string StatusMessage
        {
            get => _statusMessage;
            set => SetProperty(ref _statusMessage, value);
        }
        
        /// <summary>
        /// 版本信息
        /// </summary>
        public string Version
        {
            get => _version;
            set => SetProperty(ref _version, value);
        }
        
        /// <summary>
        /// 导航命令
        /// </summary>
        public DelegateCommand<string> NavigateCommand { get; private set; }
        
        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="regionManager">区域管理器</param>
        /// <param name="loggingService">日志服务</param>
        public MainViewModel(IRegionManager regionManager, ILoggingService loggingService)
        {
            _regionManager = regionManager;
            _loggingService = loggingService;
            
            // 初始化命令
            NavigateCommand = new DelegateCommand<string>(Navigate);
            
            _loggingService.Info("主窗口视图模型已初始化");
        }
        
        /// <summary>
        /// 导航到指定视图
        /// </summary>
        /// <param name="navigationPath">导航路径</param>
        private void Navigate(string navigationPath)
        {
            if (string.IsNullOrEmpty(navigationPath))
                return;
                
            _loggingService.Debug($"导航到 {navigationPath}");
            _regionManager.RequestNavigate("ContentRegion", navigationPath);
            
            // 更新状态栏
            StatusMessage = $"当前模块: {navigationPath}";
        }
    }
} 