using System.Windows;
using CommunicationProtocol.WPF.ViewModels;
using Prism.Regions;

namespace CommunicationProtocol.WPF
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly IRegionManager _regionManager;

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="regionManager">Prism区域管理器</param>
        /// <param name="viewModel">主窗口视图模型</param>
        public MainWindow(IRegionManager regionManager, MainViewModel viewModel)
        {
            InitializeComponent();
            
            _regionManager = regionManager;
            DataContext = viewModel;
            
            // 加载默认视图
            Loaded += MainWindow_Loaded;
        }

        /// <summary>
        /// 窗口加载完成后，导航到首页
        /// </summary>
        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            _regionManager.RequestNavigate("ContentRegion", "Home");
        }
    }
} 