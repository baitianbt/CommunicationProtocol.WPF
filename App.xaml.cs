using Prism.DryIoc;
using Prism.Ioc;
using System.Windows;
using CommunicationProtocol.WPF.Views;
using CommunicationProtocol.WPF.Services.LoggingService;
using NLog;
using NLog.Config;
using NLog.Targets;
using System;
using System.IO;

namespace CommunicationProtocol.WPF
{
    /// <summary>
    /// App.xaml 的交互逻辑
    /// </summary>
    public partial class App : PrismApplication
    {
        /// <summary>
        /// 配置NLog
        /// </summary>
        private void ConfigureNLog()
        {
            var config = new LoggingConfiguration();

            // 创建日志目录
            string logDirectory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Logs");
            if (!Directory.Exists(logDirectory))
                Directory.CreateDirectory(logDirectory);

            // 文件目标 - 应用程序日志
            var fileTarget = new FileTarget("fileTarget")
            {
                FileName = Path.Combine(logDirectory, "app_${shortdate}.log"),
                Layout = "${longdate} ${level:uppercase=true} ${message} ${exception:format=tostring}",
                ArchiveFileName = Path.Combine(logDirectory, "archives", "app_{#}.log"),
                ArchiveNumbering = ArchiveNumberingMode.Date,
                ArchiveEvery = FileArchivePeriod.Day,
                MaxArchiveFiles = 30
            };

            // 文件目标 - 通讯日志
            var commFileTarget = new FileTarget("commFileTarget")
            {
                FileName = Path.Combine(logDirectory, "communication_${shortdate}.log"),
                Layout = "${longdate} ${level:uppercase=true} ${message}",
                ArchiveFileName = Path.Combine(logDirectory, "archives", "communication_{#}.log"),
                ArchiveNumbering = ArchiveNumberingMode.Date,
                ArchiveEvery = FileArchivePeriod.Day,
                MaxArchiveFiles = 30
            };

            // 控制台目标 (调试用)
            var consoleTarget = new ConsoleTarget("consoleTarget")
            {
                Layout = "${longdate} ${level:uppercase=true} ${message}"
            };

            // 规则
            config.AddRule(LogLevel.Debug, LogLevel.Fatal, fileTarget);
            config.AddRule(LogLevel.Debug, LogLevel.Fatal, commFileTarget, "Communication*");
            config.AddRule(LogLevel.Info, LogLevel.Fatal, consoleTarget);

            // 应用配置
            LogManager.Configuration = config;
        }

        /// <summary>
        /// 应用程序启动时执行
        /// </summary>
        protected override void OnStartup(StartupEventArgs e)
        {
            // 配置NLog
            ConfigureNLog();

            // 获取日志记录器
            var logger = LogManager.GetCurrentClassLogger();

            try
            {
                logger.Info("应用程序启动");
                base.OnStartup(e);
            }
            catch (Exception ex)
            {
                logger.Fatal(ex, "应用程序启动失败");
                MessageBox.Show($"应用程序启动失败: {ex.Message}", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
                Current.Shutdown();
            }
        }

        /// <summary>
        /// 应用程序关闭时执行
        /// </summary>
        protected override void OnExit(ExitEventArgs e)
        {
            var logger = LogManager.GetCurrentClassLogger();
            logger.Info("应用程序关闭");
            LogManager.Shutdown();
            base.OnExit(e);
        }

        /// <summary>
        /// 创建Shell窗口
        /// </summary>
        protected override Window CreateShell()
        {
            return Container.Resolve<MainWindow>();
        }

        /// <summary>
        /// 注册依赖服务
        /// </summary>
        protected override void RegisterTypes(IContainerRegistry containerRegistry)
        {
            // 注册服务
            containerRegistry.RegisterSingleton<ILoggingService, LoggingService>();
            // 注册视图
            containerRegistry.RegisterForNavigation<HomeView>("Home");
            containerRegistry.RegisterForNavigation<ModbusView>("Modbus");
            containerRegistry.RegisterForNavigation<ModbusView>("ModbusRtu");
            containerRegistry.RegisterForNavigation<ModbusView>("ModbusTcp");
            containerRegistry.RegisterForNavigation<SiemensView>("Siemens");
            containerRegistry.RegisterForNavigation<OpcUaView>("OpcUa");
            containerRegistry.RegisterForNavigation<EtherNetIPView>("EtherNetIP"); // 添加EtherNet/IP
            containerRegistry.RegisterForNavigation<MitsubishiView>("Mitsubishi"); // 添加三菱MC
            containerRegistry.RegisterForNavigation<SerialPortView>("SerialPort"); // 添加串口通信
            containerRegistry.RegisterForNavigation<TcpView>("Tcp"); // 添加TCP通信
            containerRegistry.RegisterForNavigation<SettingsView>("Settings");

        }
    }
}