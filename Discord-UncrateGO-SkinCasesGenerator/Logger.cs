using System;
using System.Collections.Generic;

namespace Discord_UncrateGO_SkinCasesGenerator
{
    public class Logger
    {
        public static void Log(string message, LogLevel logLevel = LogLevel.Info)
        {
            string severity = "";

            var color = Console.ForegroundColor;
            switch (logLevel)
            {
                case LogLevel.Debug:
                    Console.ForegroundColor = ConsoleColor.Gray;
                    severity = "Debug";
                    break;
                case LogLevel.Info:
                    Console.ForegroundColor = ConsoleColor.White;
                    severity = "Info";
                    break;
                case LogLevel.Warning:
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    severity = "WARNING";
                    break;
                case LogLevel.Error:
                    Console.ForegroundColor = ConsoleColor.Red;
                    severity = "ERROR";
                    break;
                case LogLevel.Critical:
                    Console.ForegroundColor = ConsoleColor.DarkRed;
                    severity = "CRITICAL";
                    break;
            }

            Console.WriteLine($"[{severity}] | " + message);
            Console.ForegroundColor = color;
        }

        public enum LogLevel { Debug, Info, Warning, Error, Critical}

        public static void LogToFile(List<string> messages, string path)
        {
            using (System.IO.StreamWriter file =
                new System.IO.StreamWriter(path))
            {
                foreach (string line in messages)
                {
                    file.WriteLine(line);
                }
            }

        }
        public static void LogToFile(string messages, string path)
        {
            using (System.IO.StreamWriter file =
                new System.IO.StreamWriter(path))
            {

                file.WriteLine(messages);
            }

        }
    }
}
