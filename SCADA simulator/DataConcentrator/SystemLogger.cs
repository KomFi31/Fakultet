/*
 * Omogucava centralizovano cuvanje dogadjaja SCADA sistema.
 * Svaka evidentirana akcija se upisuje u system.log fajl
 * zajedno sa vremenskim trenutkom kada se dogodila.
 */

using System;
using System.IO;

namespace DataConcentrator
{
    public static class SystemLogger
    {
        private static readonly object logLock = new object();

        private static readonly string logPath =
            Path.Combine(
                AppDomain.CurrentDomain.BaseDirectory,
                "system.log");

        public static void Log(string message)
        {
            lock (logLock)
            {
                string logEntry =
                    DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") +
                    " | " +
                    message;

                File.AppendAllText(
                    logPath,
                    logEntry + Environment.NewLine);
            }
        }

        public static void LogError(Exception exception)
        {
            Log(
                "ERROR | " +
                exception.GetType().Name +
                " | " +
                exception.Message);
        }
    }
}