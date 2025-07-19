using NLog;
using System;

namespace CommunicationProtocol.WPF.Services.LoggingService
{
    /// <summary>
    /// 日志服务实现
    /// </summary>
    public class LoggingService : ILoggingService
    {
        private readonly ILogger _logger;
        
        /// <summary>
        /// 构造函数
        /// </summary>
        public LoggingService()
        {
            _logger = LogManager.GetCurrentClassLogger();
        }
        
        /// <summary>
        /// 记录调试信息
        /// </summary>
        /// <param name="message">日志信息</param>
        public void Debug(string message)
        {
            _logger.Debug(message);
        }
        
        /// <summary>
        /// 记录调试信息
        /// </summary>
        /// <param name="message">日志信息</param>
        /// <param name="args">格式化参数</param>
        public void Debug(string message, params object[] args)
        {
            _logger.Debug(message, args);
        }
        
        /// <summary>
        /// 记录信息
        /// </summary>
        /// <param name="message">日志信息</param>
        public void Info(string message)
        {
            _logger.Info(message);
        }
        
        /// <summary>
        /// 记录信息
        /// </summary>
        /// <param name="message">日志信息</param>
        /// <param name="args">格式化参数</param>
        public void Info(string message, params object[] args)
        {
            _logger.Info(message, args);
        }
        
        /// <summary>
        /// 记录警告信息
        /// </summary>
        /// <param name="message">日志信息</param>
        public void Warn(string message)
        {
            _logger.Warn(message);
        }
        
        /// <summary>
        /// 记录警告信息
        /// </summary>
        /// <param name="message">日志信息</param>
        /// <param name="args">格式化参数</param>
        public void Warn(string message, params object[] args)
        {
            _logger.Warn(message, args);
        }
        
        /// <summary>
        /// 记录错误信息
        /// </summary>
        /// <param name="message">日志信息</param>
        public void Error(string message)
        {
            _logger.Error(message);
        }
        
        /// <summary>
        /// 记录错误信息
        /// </summary>
        /// <param name="message">日志信息</param>
        /// <param name="args">格式化参数</param>
        public void Error(string message, params object[] args)
        {
            _logger.Error(message, args);
        }
        
        /// <summary>
        /// 记录错误信息
        /// </summary>
        /// <param name="ex">异常</param>
        /// <param name="message">日志信息</param>
        public void Error(Exception ex, string message)
        {
            _logger.Error(ex, message);
        }
        
        /// <summary>
        /// 记录错误信息
        /// </summary>
        /// <param name="ex">异常</param>
        /// <param name="message">日志信息</param>
        /// <param name="args">格式化参数</param>
        public void Error(Exception ex, string message, params object[] args)
        {
            _logger.Error(ex, message, args);
        }
        
        /// <summary>
        /// 记录致命错误信息
        /// </summary>
        /// <param name="message">日志信息</param>
        public void Fatal(string message)
        {
            _logger.Fatal(message);
        }
        
        /// <summary>
        /// 记录致命错误信息
        /// </summary>
        /// <param name="message">日志信息</param>
        /// <param name="args">格式化参数</param>
        public void Fatal(string message, params object[] args)
        {
            _logger.Fatal(message, args);
        }
        
        /// <summary>
        /// 记录致命错误信息
        /// </summary>
        /// <param name="ex">异常</param>
        /// <param name="message">日志信息</param>
        public void Fatal(Exception ex, string message)
        {
            _logger.Fatal(ex, message);
        }
        
        /// <summary>
        /// 记录致命错误信息
        /// </summary>
        /// <param name="ex">异常</param>
        /// <param name="message">日志信息</param>
        /// <param name="args">格式化参数</param>
        public void Fatal(Exception ex, string message, params object[] args)
        {
            _logger.Fatal(ex, message, args);
        }
    }
} 