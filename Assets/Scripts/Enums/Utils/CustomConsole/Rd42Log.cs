using System;
using System.Diagnostics;
using UnityEngine;
using Object = UnityEngine.Object;
using Debug = UnityEngine.Debug;

namespace RD42.Utils
{
    public class Rd42Log 
    {
        #region Variables

        private const string infoColor = nameof(Color.white);
        private const string warningColor = nameof(Color.yellow);
        private const string errorColor = nameof(Color.red);
        internal static ILogger Logger => Debug.unityLogger;

        #endregion

        #region Log

        /// <summary>
        /// Logs a message to the CustomLogger.
        /// </summary>
        /// <param name="message">String or object to be converted to string representation for display.</param>
        [Conditional("DEBUG")]
        public static void Info(object message)
        {
            Logger.Log(LogType.Log, FormatMessage(message));
        }

        /// <summary>
        /// Logs a message to the CustomLogger.
        /// </summary>
        /// <param name="message">String or object to be converted to string representation for display.</param>
        /// <param name="context">Object to which the message applies.</param>
        [Conditional("DEBUG")]
        public static void Info(Object context, object message)
        {
            Logger.Log(LogType.Log, FormatMessage(message), context);
        }

        /// <summary>
        /// Logs a message to the CustomLogger.
        /// </summary>
        /// <param name="tag"> Used to identify the source of a log message. It usually identifies the class where the log call occurs.</param>
        /// <param name="message">String or object to be converted to string representation for display.</param>
        [Conditional("DEBUG")]
        public static void Info(string tag, object message)
        {
            Logger.Log(LogType.Log, FormatMessageWithTag(infoColor, tag, message));
        }

        /// <summary>
        /// Logs a message to the CustomLogger.
        /// </summary>
        /// <param name="tag"> Used to identify the source of a log message. It usually identifies the class where the log call occurs.</param>
        /// <param name="message">String or object to be converted to string representation for display.</param>
        /// /// <param name="context">Object to which the message applies.</param>
        [Conditional("DEBUG")]
        public static void Info(Object context, string tag, object message)
        {
            Logger.Log(LogType.Log, FormatMessageWithTag(infoColor, tag, message), context);
        }

        #endregion

        #region WarningLog

        /// <summary>
        /// Logs a warning message to the CustomLogger.
        /// </summary>
        /// <param name="message">String or object to be converted to string representation for display.</param>
        [Conditional("DEBUG")]
        public static void Warning(object message)
        {
            Logger.Log(LogType.Warning, FormatMessage(message));
        }

        /// <summary>
        /// Logs a warning message to the CustomLogger.
        /// </summary>
        /// <param name="message">String or object to be converted to string representation for display.</param>
        /// <param name="context">Object to which the message applies.</param>
        [Conditional("DEBUG")]
        public static void Warning(Object context, object message)
        {
            Logger.Log(LogType.Warning, FormatMessage(message), context);
        }

        /// <summary>
        /// Logs a warning message to the CustomLogger.
        /// </summary>
        /// <param name="tag"> Used to identify the source of a log message. It usually identifies the class where the log call occurs.</param>
        /// <param name="message">String or object to be converted to string representation for display.</param>
        [Conditional("DEBUG")]
        public static void Warning(string tag, object message)
        {
            Logger.Log(LogType.Warning, FormatMessageWithTag(warningColor, tag, message));
        }

        /// <summary>
        /// Logs a warning message to the CustomLogger.
        /// </summary>
        /// <param name="tag"> Used to identify the source of a log message. It usually identifies the class where the log call occurs.</param>
        /// <param name="message">String or object to be converted to string representation for display.</param>
        /// /// <param name="context">Object to which the message applies.</param>
        [Conditional("DEBUG")]
        public static void Warning(Object context, string tag, object message)
        {
            Logger.Log(LogType.Warning, FormatMessageWithTag(warningColor, tag, message), context);
        }

        #endregion

        #region ErrorLog

        /// <summary>
        /// Logs an error message to the CustomLogger.
        /// </summary>
        /// <param name="message">String or object to be converted to string representation for display.</param>
        [Conditional("DEBUG")]
        public static void Error(object message)
        {
            Logger.Log(LogType.Error, FormatMessage(message));
        }

        /// <summary>
        /// Logs an error message to the CustomLogger.
        /// </summary>
        /// <param name="message">String or object to be converted to string representation for display.</param>
        /// <param name="context">Object to which the message applies.</param>
        [Conditional("DEBUG")]
        public static void LogError(Object context, object message)
        {
            Logger.Log(LogType.Error, FormatMessage(message), context);
        }

        /// <summary>
        /// Logs an error message to the CustomLogger.
        /// </summary>
        /// <param name="tag"> Used to identify the source of a log message. It usually identifies the class where the log call occurs.</param>
        /// <param name="message">String or object to be converted to string representation for display.</param>
        [Conditional("DEBUG")]
        public static void Error(string tag, object message)
        {
            Logger.Log(LogType.Error, FormatMessageWithTag(errorColor, tag, message));
        }

        /// <summary>
        /// Logs an error message to the CustomLogger.
        /// </summary>
        /// <param name="tag"> Used to identify the source of a log message. It usually identifies the class where the log call occurs.</param>
        /// <param name="message">String or object to be converted to string representation for display.</param>
        /// /// <param name="context">Object to which the message applies.</param>
        [Conditional("DEBUG")]
        public static void Error(Object context, string tag, object message)
        {
            Logger.Log(LogType.Error, FormatMessageWithTag(errorColor, tag, message), context);
        }

        #endregion

        #region ExceptionLog

        /// <summary>
        /// Logs an error message to the CustomLogger.
        /// </summary>
        /// <param name="exception">Runtime exception.</param>
        [Conditional("DEBUG")]
        public static void Exception(Exception exception)
        {
            Logger.Log(LogType.Error, FormatMessage(exception.Message));
        }

        /// <summary>
        /// Logs an error message to the CustomLogger.
        /// </summary>
        /// <param name="exception">Runtime exception.</param>
        /// <param name="context">Object to which the message applies.</param>
        [Conditional("DEBUG")]
        public static void Exception(Object context, Exception exception)
        {
            Logger.Log(LogType.Error, FormatMessage(exception.Message), context);
        }

        /// <summary>
        /// Logs an error message to the CustomLogger.
        /// </summary>
        /// <param name="tag"> Used to identify the source of a log message. It usually identifies the class where the log call occurs.</param>
        /// <param name="exception">Runtime exception.</param>
        [Conditional("DEBUG")]
        public static void Exception(string tag, Exception exception)
        {
            Logger.Log(LogType.Error, FormatMessageWithTag(errorColor, tag, exception.Message));
        }

        /// <summary>
        /// Logs an error message to the CustomLogger.
        /// </summary>
        /// <param name="tag"> Used to identify the source of a log message. It usually identifies the class where the log call occurs.</param>
        /// <param name="exception">Runtime exception.</param>
        /// /// <param name="context">Object to which the message applies.</param>
        [Conditional("DEBUG")]
        public static void Exception(Object context, string tag, Exception exception)
        {
            Logger.Log(LogType.Error, FormatMessageWithTag(errorColor, tag, exception.Message), context);
        }

        #endregion

        #region Static Commons
        private static object FormatMessage(object message)
        {
            return $"<color={infoColor}>{message}</color>";
        }

        private static object FormatMessageWithTag(string color, string tag, object message)
        {
            return $"<color={color}><b>[{tag}] </b></color><color={infoColor}>{message}</color>";
        }
        #endregion

        #region Public Utilities (TO WRAP)
        /// <summary>
        /// Logs a message to the CustomLogger using a custom Color.
        /// </summary>
        /// <param name="tag"> Used to identify the source of a log message. It usually identifies the class where the log call occurs.</param>
        /// <param name="message">String or object to be converted to string representation for display.</param>
        /// <param name="color">String that represent the color chosen.</param>
        /// /// <param name="context">Object to which the message applies.</param>
        [Conditional("DEBUG")]
        public static void Info(Object context, string tag, object message, string color)
        {
            Logger.Log(LogType.Log, FormatMessageWithTag(color, tag, message), context);
        }
        #endregion
    }
}