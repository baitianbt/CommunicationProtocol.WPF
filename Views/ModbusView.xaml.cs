using Prism.Regions;
using System.Windows.Controls;

namespace CommunicationProtocol.WPF.Views
{
    /// <summary>
    /// ModbusView.xaml 的交互逻辑
    /// </summary>
    public partial class ModbusView : UserControl, INavigationAware
    {
        /// <summary>
        /// 构造函数
        /// </summary>
        public ModbusView()
        {
            InitializeComponent();
        }
        
        /// <summary>
        /// 判断是否可以导航到该视图
        /// </summary>
        public bool IsNavigationTarget(NavigationContext navigationContext)
        {
            return true;
        }
        
        /// <summary>
        /// 导航离开时触发
        /// </summary>
        public void OnNavigatedFrom(NavigationContext navigationContext)
        {
        }
        
        /// <summary>
        /// 导航到达时触发
        /// </summary>
        public void OnNavigatedTo(NavigationContext navigationContext)
        {
            // 获取导航参数中的视图名称
            if (DataContext is ViewModels.ModbusViewModel viewModel)
            {
                // 添加视图名称参数
                var parameters = new NavigationParameters();
                parameters.Add("ViewName", navigationContext.Uri.ToString());
                
                // 更新视图模型
                viewModel.UpdateFromNavigationParameters(parameters);
            }
        }
    }
}