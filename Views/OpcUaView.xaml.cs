using System.Windows.Controls;

namespace CommunicationProtocol.WPF.Views
{
    /// <summary>
    /// OpcUaView.xaml 的交互逻辑
    /// </summary>
    public partial class OpcUaView : UserControl
    {
        /// <summary>
        /// 构造函数
        /// </summary>
        public OpcUaView()
        {
            InitializeComponent();
        }

        /// <summary>
        /// 树视图节点选择改变事件处理
        /// </summary>
        private void TreeView_SelectedItemChanged(object sender, System.Windows.RoutedPropertyChangedEventArgs<object> e)
        {
            if (DataContext is ViewModels.OpcUaViewModel viewModel)
            {
                viewModel.SelectedNode = e.NewValue;
            }
        }
    }
} 