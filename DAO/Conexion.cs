using DTO;
using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Extensions.Options;
using Sap.Data.Hana;

namespace DAO
{
    public class Conexion
    {
        private readonly DatabaseSettings _dbSettings;
        private readonly LIMSDatabaseSettings _limsDBSettings;
        private readonly HANADatabaseSettings _hanaDBSettings;

        public Conexion(IOptions<DatabaseSettings> options, IOptions<LIMSDatabaseSettings> limsDBSettings, IOptions<HANADatabaseSettings>
            hanaBSettings)
        {
            _dbSettings = options.Value;
            _limsDBSettings = limsDBSettings.Value;
            _hanaDBSettings = hanaBSettings.Value;
        }
        public SqlConnection CrearConexion(string nombreBaseDatos)
        {
            var connectionString =
            $"Server={_dbSettings.Server};" +
            $"Database={nombreBaseDatos};" +
            $"User Id={_dbSettings.User};" +
            $"Password={_dbSettings.Password};" +
            $"Connection Timeout=60;" +
            $"Command Timeout=60;" +
            $"Trusted_Connection=True;" +
            $"Integrated Security=False;" +
            $"TrustServerCertificate=True;";

            return new SqlConnection(connectionString);
        }

        public SqlConnection CrearCnLims(string nombreBD)
        {
            var connectionString =
            $"Server={_limsDBSettings.Server};" +
            $"Database={_limsDBSettings.Database};" +
            $"User Id={_limsDBSettings.User};" +
            $"Password={_limsDBSettings.Password};" +
            $"Connection Timeout=30;" +
            $"Trusted_Connection=True;" +
            $"Integrated Security=False;" +
            $"TrustServerCertificate=True;";

            return new SqlConnection(connectionString);

        }
        public HanaConnection CrearConexionHana(string nombreBD)
        {
            var connectionString =
            $"Server={_hanaDBSettings.Server};" +
            $"User Id={_hanaDBSettings.User};" +
            $"Password={_hanaDBSettings.Password};" +
            $"CurrentSchema={_hanaDBSettings.Schema};";

            return new HanaConnection(connectionString);

        }
    }
}
