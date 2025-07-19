using System;

namespace CommunicationProtocol.WPF.Services.LoggingService
{
    /// <summary>
    /// 日志服务接口
    /// </summary>
    public interface ILoggingService
    {
        /// <summary>
        /// 记录调试信息
        /// </summary>
        /// <param name="message">日志信息</param>
        void Debug(string message);
        
        /// <summary>
        /// 记录调试信息
        /// </summary>
        /// <param name="message">日志信息</param>
        /// <param name="args">格式化参数</param>
        void Debug(string message, params object[] args);
        
        /// <summary>
        /// 记录信息
        /// </summary>
        /// <param name="message">日志信息</param>
        void Info(string message);
        
        /// <summary>
        /// 记录信息
        /// </summary>
        /// <param name="message">日志信息</param>
        /// <param name="args">格式化参数</param>
        void Info(string message, params object[] args);
        
        /// <summary>
        /// 记录警告信息
        /// </summary>
        /// <param name="message">日志信息</param>
        void Warn(string message);
        
        /// <summary>
        /// 记录警告信息
        /// </summary>
        /// <param name="message">日志信息</param>
        /// <param name="args">格式化参数</param>
        void Warn(string message, params object[] args);
        
        /// <summary>
        /// 记录错误信息
        /// </summary>
        /// <param name="message">日志信息</param>
        void Error(string message);
        
        /// <summary>
        /// 记录错误信息
        /// </summary>
        /// <param name="message">日志信息</param>
        /// <param name="args">格式化参数</param>
        void Error(string message, params object[] args);
        
        /// <summary>
        /// 记录错误信息
        /// </summary>
        /// <param name="ex">异常</param>
        /// <param name="message">日志信息</param>
        void Error(Exception ex, string message);
        
        /// <summary>
        /// 记录错误信息
        /// </summary>
        /// <param name="ex">异常</param>
        /// <param name="message">日志信息</param>
        /// <param name="args">格式化参数</param>
        void Error(Exception ex, string message, params object[] args);
        
        /// <summary>
        /// 记录致命错误信息
        /// </summary>
        /// <param name="message">日志信息</param>
        void Fatal(string message);
        
        /// <summary>
        /// 记录致命错误信息
        /// </summary>
        /// <param name="message">日志信息</param>
        /// <param name="args">格式化参数</param>
        void Fatal(string message, params object[] args);
        
        /// <summary>
        /// 记录致命错误信息
        /// </summary>
        /// <param name="ex">异常</param>
        /// <param name="message">日志信息</param>
        void Fatal(Exception ex, string message);
        
        /// <summary>
        /// 记录致命错误信息
        /// </summary>
        /// <param name="ex">异常</param>
        /// <param name="message">日志信息</param>
        /// <param name="args">格式化参数</param>
        void Fatal(Exception ex, string message, params object[] args);
    }
} 