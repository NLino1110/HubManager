using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Oracle.ManagedDataAccess.Client;

namespace ApiTradeHub.Controllers.DbManager
{
    [Route("api/[controller]")]
    [ApiController]
    public class OptimizeTools
    {
        DataSourceManager.AppDbContext appDbContext = new DataSourceManager.AppDbContext();

        [HttpGet]
        public string ReindexAndDefragmentTables()
        {            
            using (OracleConnection connection = (OracleConnection) appDbContext.Database.GetDbConnection())
            {
                connection.Open();

                OracleCommand command = new OracleCommand();
                command.Connection = connection;

                // Obtener la lista de tablas de la base de datos
                command.CommandText = "SELECT table_name FROM user_tables";
                OracleDataReader reader = command.ExecuteReader();

                // Recorrer todas las tablas
                while (reader.Read())
                {
                    string tableName = reader.GetString(0);

                    // Ejecutar los comandos de reindexación y defragmentación para cada tabla
                    ReindexTable(connection, tableName);
                    DefragmentTable(connection, tableName);
                }

                reader.Close();
            }

            return "";
        }

        private void ReindexTable(OracleConnection connection, string tableName)
        {
            OracleCommand command = new OracleCommand();
            command.Connection = connection;

            // Reindexar tabla
            command.CommandText = $"ALTER INDEX ALL ON {tableName} REBUILD";
            command.ExecuteNonQuery();

            Console.WriteLine($"Tabla {tableName} reindexada.");
        }

        private void DefragmentTable(OracleConnection connection, string tableName)
        {
            OracleCommand command = new OracleCommand();
            command.Connection = connection;

            // Defragmentar tabla
            command.CommandText = $"ALTER TABLE {tableName} MOVE";
            command.ExecuteNonQuery();

            Console.WriteLine($"Tabla {tableName} defragmentada.");
        }
    }
}
