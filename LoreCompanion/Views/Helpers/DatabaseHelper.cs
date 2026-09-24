using System.Data;
using LoreCompanion.Models;
using LoreCompanion.Utilities;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Serilog;

namespace LoreCompanion.Views.Helpers
{
    public static class DatabaseHelper
    {
        public const string NoCaseCollation = "NOCASE";
        private static readonly TimeSpan StaleLockThreshold = TimeSpan.FromMinutes(5);

        private static ILogger Logger { get; } = LogManager.GetLogger();

        public static async Task ClearStaleMigrationLockAsync(
            LoreDbContext context,
            CancellationToken cancellationToken)
        {
            var connection = context.Database.GetDbConnection();

            if (connection.State != ConnectionState.Open)
            {
                await connection.OpenAsync(cancellationToken);
            }

            try
            {
                var cutoff = DateTimeOffset.UtcNow - StaleLockThreshold;

                await using var command = connection.CreateCommand();

                command.CommandText = """
                                      DELETE FROM "__EFMigrationsLock"
                                      WHERE "Id" = 1 AND "Timestamp" < $cutoff;
                                      """;

                var parameter = command.CreateParameter();
                parameter.ParameterName = "$cutoff";
                parameter.Value = cutoff.ToString("yyyy-MM-dd HH:mm:ss.fffffffzzz");
                command.Parameters.Add(parameter);

                var rowsDeleted = await command.ExecuteNonQueryAsync(cancellationToken);

                if (rowsDeleted > 0)
                {
                    Logger.Warning("Cleared a stale __EFMigrationsLock row older than {Threshold}", StaleLockThreshold);
                }
            }
            catch (SqliteException e) when (e.SqliteErrorCode == 1) // "no such table"
            {
                // Fresh database — the lock table hasn't been created yet. Nothing to clean up.
            }
            catch (Exception e)
            {
                Logger.Error(e, "Unexpected failure to clear stale __EFMigrationsLock row");
            }
        }
    }
}