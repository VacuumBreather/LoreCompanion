using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using Caliburn.Micro;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using Serilog.Core;
using Serilog.Sinks.SystemConsole.Themes;

namespace LoreCompanion.Utilities
{
    public static class LogManager
    {
        /// <summary>Retrieves a logger instance for the specified generic type.</summary>
        /// <typeparam name="T">The type for which the logger instance is being retrieved.</typeparam>
        /// <returns>An instance of <see cref="ILogger"/> configured for the specified type.</returns>
        public static ILogger GetLogger<T>()
        {
            return GetLogger(typeof(T));
        }

        /// <summary>Retrieves an instance of the logger for the specified type, or a default logger if no type is provided.</summary>
        /// <param name="type">
        ///     The type for which the logger is created. If <see langword="null"/>, the logger is associated with
        ///     the calling type.
        /// </param>
        /// <returns>An instance of <see cref="ILogger"/> configured with the specified or inferred context.</returns>
        public static ILogger GetLogger(Type? type = null)
        {
            type ??= GetCallingType();
            type ??= typeof(LogManager);

            return Log.ForContext(Constants.SourceContextPropertyName, type.GetLogCategory());
        }

        /// <summary>Sets up Serilog with console, debug, and file logging in the specified app data folder.</summary>
        /// <param name="services">The service collection to configure.</param>
        /// <param name="appDataFolder">The application data folder.</param>
        /// <returns>The configured service collection.</returns>
        public static IServiceCollection AddConfiguredSerilog(this IServiceCollection services, string appDataFolder)
        {
            var result = services.AddSerilog((_, loggerConfiguration) => loggerConfiguration.Enrich.FromLogContext()
                                                 .MinimumLevel.Debug()
#if DEBUG
                                                 .WriteTo
                                                 .Console(
                                                     theme: AnsiConsoleTheme.Sixteen,
                                                     applyThemeToRedirectedOutput: true,
                                                     outputTemplate:
                                                     "[{Timestamp:HH:mm:ss} {Level:u3}] {SourceContext} - {Message:lj}{NewLine}{Exception}")
                                                 .WriteTo
                                                 .Debug(
                                                     outputTemplate:
                                                     "[{Timestamp:HH:mm:ss} {Level:u3}] {SourceContext} - {Message:lj}{NewLine}{Exception}")
                                                 .WriteTo.Logger(c => c.MinimumLevel.Information()
                                                                       .WriteTo.Async(a => a.File(
                                                                           Path.Combine(
                                                                               appDataFolder,
                                                                               "Logs",
                                                                               "log.txt"),
                                                                           fileSizeLimitBytes: 100 * 1024 * 1024,
                                                                           rollOnFileSizeLimit: true,
                                                                           rollingInterval: RollingInterval.Day,
                                                                           outputTemplate:
                                                                           "[{Timestamp:yyyy-MM-dd HH:mm:ss.fff} {Level:u3}] {SourceContext} - {Message:lj}{NewLine}{Exception}")))
#endif
                                                 .WriteTo.Logger(c => c.MinimumLevel.Fatal()
                                                                       .WriteTo.Async(a => a.File(
                                                                           Path.Combine(
                                                                               appDataFolder,
                                                                               "Logs",
                                                                               "crash_log.txt"),
                                                                           fileSizeLimitBytes: 100 * 1024 * 1024,
                                                                           rollOnFileSizeLimit: true,
                                                                           rollingInterval: RollingInterval.Day,
                                                                           outputTemplate:
                                                                           "[{Timestamp:yyyy-MM-dd HH:mm:ss.fff} {Level:u3}] {SourceContext} - {Message:lj}{NewLine}{Exception}"))));

            Caliburn.Micro.LogManager.GetLog = _ => LogWrapper.Default;

            return result;
        }

        private static string GetLogCategory(this Type type)
        {
            ArgumentNullException.ThrowIfNull(type);

            var typeName = type.GetGenericsFriendlyName();
            var typeNamespace = type.Namespace;

            return $"{typeNamespace}.{typeName}";
        }

        private static string GetGenericsFriendlyName(this Type type)
        {
            if (!type.IsGenericType)
            {
                return type.Name;
            }

            var genericTypeName = type.Name[..type.Name.IndexOf('`', StringComparison.Ordinal)];
            var genericArgs = type.GetGenericArguments();

            return $"{genericTypeName}<{string.Join(", ", genericArgs.Select(GetGenericsFriendlyName))}>";
        }

        private static Type? GetCallingType()
        {
            var stackTrace = new StackTrace();
            var frame = stackTrace.GetFrame(2); // 2 to skip the current method and its caller

            return frame?.GetMethod()?.DeclaringType;
        }

        private class LogWrapper : ILog
        {
            private LogWrapper()
            {
            }

            public static ILog Default { get; } = new LogWrapper();

            private static ILogger WrappedLogger { get; } = GetLogger<ILog>();

            [SuppressMessage("ReSharper", "TemplateIsNotCompileTimeConstantProblem")]
            public void Info(string format, params object[] args)
            {
                WrappedLogger.Verbose(format, args);
            }

            [SuppressMessage("ReSharper", "TemplateIsNotCompileTimeConstantProblem")]
            public void Warn(string format, params object[] args)
            {
                WrappedLogger.Warning(format, args);
            }

            public void Error(Exception exception)
            {
                WrappedLogger.Error(exception, "An exception occurred");
            }
        }
    }
}