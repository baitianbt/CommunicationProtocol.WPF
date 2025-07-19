using Prism.Commands;
using Prism.Mvvm;
using Prism.Regions;
using CommunicationProtocol.WPF.Services.LoggingService;

namespace CommunicationProtocol.WPF.ViewModels
{
    /// <summary>
    /// 首页视图模型
    /// </summary>
    public class HomeViewModel : BindableBase
    {
        private readonly IRegionManager _regionManager;
        private readonly ILoggingService _loggingService;

        /// <summary>
        /// 导航命令
        /// </summary>
        public DelegateCommand<string> NavigateCommand { get; private set; }
        
        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="regionManager">区域管理器</param>
        /// <param name="loggingService">日志服务</param>
        public HomeViewModel(IRegionManager regionManager, ILoggingService loggingService)
        {
            _regionManager = regionManager;
            _loggingService = loggingService;
            
            // 初始化命令
            NavigateCommand = new DelegateCommand<string>(Navigate);
            
            _loggingService.Info("首页视图模型已初始化");
        }

        /// <summary>
        /// 导航到指定视图
        /// </summary>
        /// <param name="navigationPath">导航路径</param>
        private void Navigate(string navigationPath)
        {
            if (string.IsNullOrEmpty(navigationPath))
                return;
                
            _loggingService.Debug($"从首页导航到 {navigationPath}");
            _regionManager.RequestNavigate("ContentRegion", navigationPath);
        }
    }
} 