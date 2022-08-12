using System;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Threading.Tasks;
using Dapper;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Configuration;

namespace XCV.Services
{
    /// <summary>
    /// A helper class for general database requests and the database connection string.
    /// </summary>
    public class DatabaseUtils
    {
        /// <summary>
        /// Connection string for the database.
        /// </summary>
        public string ConnectionString => _sqlConnectionStringBuilder.ConnectionString;

        private readonly SqlConnectionStringBuilder _sqlConnectionStringBuilder;

        private readonly IConfiguration _configuration;

        public DatabaseUtils(IConfiguration config)
        {
            _configuration = config;

            var machineName = Environment.MachineName;
            var pipelineActive = machineName.Contains("runner");
            var dockerActive = Environment.GetEnvironmentVariable("DOTNET_RUNNING_IN_CONTAINER") == "true";

            if (dockerActive)
            {
                _sqlConnectionStringBuilder = new SqlConnectionStringBuilder(
                    _configuration.GetConnectionString("DockerDb"));
            }
            else if (pipelineActive)
            {
                _sqlConnectionStringBuilder = new SqlConnectionStringBuilder(
                    _configuration.GetConnectionString("DatabaseGitlabCi"));
            }
            else
            {
                _sqlConnectionStringBuilder = new SqlConnectionStringBuilder(
                    _configuration.GetConnectionString("Database"));
            }
        }
        /// <summary>
        /// Creates all the tables given by /Files/SQLQuery.sql 
        /// </summary>
        public async Task LoadTables()
        {
            var createQueries = File.ReadAllText(Path.Combine(".", "Files", "SQLQuery.sql"));
            using IDbConnection connection = new SqlConnection(ConnectionString);
            await connection.ExecuteAsync(createQueries);
        }

        /// <summary>
        /// Destroys all the database tables.
        /// </summary>
        public async Task DestroyDatabase()
        {
            var dropAllText = File.ReadAllText(Path.Combine(".", "Files", "DROP_ALL_TABLES.sql"));
            using IDbConnection connection = new SqlConnection(ConnectionString);
            await connection.ExecuteAsync(dropAllText);
        }
    }
}