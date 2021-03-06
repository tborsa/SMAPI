﻿using System;
using System.Collections.Generic;
using System.Linq;

namespace StardewModdingAPI.Framework
{
    /// <summary>Encapsulates monitoring and logic for a given module.</summary>
    internal class Monitor : IMonitor
    {
        /*********
        ** Properties
        *********/
        /// <summary>The name of the module which logs messages using this instance.</summary>
        private readonly string Source;

        /// <summary>The log file to which to write messages.</summary>
        private readonly LogFileManager LogFile;

        /// <summary>The maximum length of the <see cref="LogLevel"/> values.</summary>
        private static readonly int MaxLevelLength = (from level in Enumerable.Cast<LogLevel>(Enum.GetValues(typeof(LogLevel))) select level.ToString().Length).Max();

        /// <summary>The console text color for each log level.</summary>
        private static readonly Dictionary<LogLevel, ConsoleColor> Colors = new Dictionary<LogLevel, ConsoleColor>
        {
            [LogLevel.Trace] = ConsoleColor.DarkGray,
            [LogLevel.Debug] = ConsoleColor.DarkGray,
            [LogLevel.Info] = ConsoleColor.White,
            [LogLevel.Warn] = ConsoleColor.Yellow,
            [LogLevel.Error] = ConsoleColor.Red,
            [LogLevel.Alert] = ConsoleColor.Magenta
        };


        /*********
        ** Accessors
        *********/
        /// <summary>Whether the current console supports color codes.</summary>
        internal static readonly bool ConsoleSupportsColor = Monitor.GetConsoleSupportsColor();

        /// <summary>Whether to show trace messages in the console.</summary>
        internal bool ShowTraceInConsole { get; set; }

        /// <summary>Whether to write anything to the console. This should be disabled if no console is available.</summary>
        internal bool WriteToConsole { get; set; } = true;


        /*********
        ** Public methods
        *********/
        /// <summary>Construct an instance.</summary>
        /// <param name="source">The name of the module which logs messages using this instance.</param>
        /// <param name="logFile">The log file to which to write messages.</param>
        public Monitor(string source, LogFileManager logFile)
        {
            // validate
            if (string.IsNullOrWhiteSpace(source))
                throw new ArgumentException("The log source cannot be empty.");
            if (logFile == null)
                throw new ArgumentNullException(nameof(logFile), "The log file manager cannot be null.");

            // initialise
            this.Source = source;
            this.LogFile = logFile;
        }

        /// <summary>Log a message for the player or developer.</summary>
        /// <param name="message">The message to log.</param>
        /// <param name="level">The log severity level.</param>
        public void Log(string message, LogLevel level = LogLevel.Debug)
        {
            this.LogImpl(this.Source, message, Monitor.Colors[level], level);
        }

        /// <summary>Immediately exit the game without saving. This should only be invoked when an irrecoverable fatal error happens that risks save corruption or game-breaking bugs.</summary>
        /// <param name="reason">The reason for the shutdown.</param>
        public void ExitGameImmediately(string reason)
        {
            Program.ExitGameImmediately(this.Source, reason);
            Program.gamePtr.Exit();
        }

        /// <summary>Log a fatal error message.</summary>
        /// <param name="message">The message to log.</param>
        internal void LogFatal(string message)
        {
            Console.BackgroundColor = ConsoleColor.Red;
            this.LogImpl(this.Source, message, ConsoleColor.White, LogLevel.Error);
        }

        /// <summary>Log a message for the player or developer, using the specified console color.</summary>
        /// <param name="source">The name of the mod logging the message.</param>
        /// <param name="message">The message to log.</param>
        /// <param name="color">The console color.</param>
        /// <param name="level">The log level.</param>
        [Obsolete("This method is provided for backwards compatibility and otherwise should not be used. Use " + nameof(Monitor) + "." + nameof(Monitor.Log) + " instead.")]
        internal void LegacyLog(string source, string message, ConsoleColor color, LogLevel level = LogLevel.Debug)
        {
            this.LogImpl(source, message, color, level);
        }


        /*********
        ** Private methods
        *********/
        /// <summary>Write a message line to the log.</summary>
        /// <param name="source">The name of the mod logging the message.</param>
        /// <param name="message">The message to log.</param>
        /// <param name="color">The console color.</param>
        /// <param name="level">The log level.</param>
        private void LogImpl(string source, string message, ConsoleColor color, LogLevel level)
        {
            // generate message
            string levelStr = level.ToString().ToUpper().PadRight(Monitor.MaxLevelLength);
            message = $"[{DateTime.Now:HH:mm:ss} {levelStr} {source}] {message}";

            // log
            if (this.WriteToConsole && (this.ShowTraceInConsole || level != LogLevel.Trace))
            {
                if (Monitor.ConsoleSupportsColor)
                {
                    Console.ForegroundColor = color;
                    Console.WriteLine(message);
                    Console.ResetColor();
                }
                else
                    Console.WriteLine(message);
            }
            this.LogFile.WriteLine(message);
        }

        /// <summary>Test whether the current console supports color formatting.</summary>
        private static bool GetConsoleSupportsColor()
        {
            try
            {
                Console.ForegroundColor = Console.ForegroundColor;
                return true;
            }
            catch (Exception)
            {
                return false; // Mono bug
            }
        }
    }
}