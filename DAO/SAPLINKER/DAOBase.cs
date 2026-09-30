using DTO;
using Microsoft.Data.SqlClient;
using Microsoft.Identity.Client;
using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;

namespace DAO
{
    public abstract class DAOBase<T> where T : class, new()
    {
        protected readonly string _nombreTabla;
        private readonly Conexion _conexion;
        private bool crearConexionLIMS = false;

        protected DAOBase(Conexion conexion, string nombreTabla)
        {
            _conexion = conexion;
            _nombreTabla = nombreTabla;
        }


        protected abstract T Mapear(SqlDataReader reader);

        protected SqlConnection CrearConexion(string bd)
        {
            if (crearConexionLIMS)
            {
                return _conexion.CrearCnLims(bd);
            }
            return _conexion.CrearConexion(bd);
        }

        protected void UsarConexionLIMS(bool estado)
        {
            crearConexionLIMS = estado;
        }


        #region Helpers de Construcción SQL

        private string ConstruirSelect(string[]? columnas) =>
            (columnas != null && columnas.Length > 0) ? string.Join(", ", columnas) : "*";

        private string ConstruirTop(int? limite) =>
            (limite.HasValue && limite.Value > 0) ? $"TOP ({limite.Value})" : "";

        private string ConstruirWhere(List<FiltroBusqueda>? filtros, bool considerarInactivos = false)
        {
            List<string> condiciones = new List<string>();
            if (!considerarInactivos) condiciones.Add("Estado = 1");
            else condiciones.Add("1=1");

            if (filtros != null && filtros.Count > 0)
            {
                foreach (var f in filtros) condiciones.Add($"({f.Condicion})");
            }

            return "WHERE " + string.Join(" AND ", condiciones);
        }

        protected TValue? ObtenerValor<TValue>(SqlDataReader reader, string columnName)
        {
            int ordinal;
            try
            {
                ordinal = reader.GetOrdinal(columnName);
            }
            catch
            {
                return default;
            }

            if (reader.IsDBNull(ordinal)) return default;

            object value = reader.GetValue(ordinal);

            try
            {
                if (value is TValue valorDirecto)
                    return valorDirecto;

                Type targetType = typeof(TValue);

                Type? underlyingType = Nullable.GetUnderlyingType(targetType);
                targetType = underlyingType ?? targetType;

                return (TValue)Convert.ChangeType(value, targetType);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error de casteo en {columnName}: {ex.Message}");
                return default;
            }
        }

        #endregion

        #region Operaciones Síncronas

        public virtual List<T> Listar(string bd, string[]? columnas = null, List<FiltroBusqueda>? filtros = null, int? limite = null, bool considerarInactivos = false)
        {
            var lista = new List<T>();
            string sql = $"SELECT {ConstruirTop(limite)} {ConstruirSelect(columnas)} FROM {_nombreTabla} {ConstruirWhere(filtros, considerarInactivos)}";

            using var conn = CrearConexion(bd);
            conn.Open();
            using var cmd = new SqlCommand(sql, conn);
            if (filtros != null) foreach (var f in filtros) cmd.Parameters.Add(f.Parametro);

            using var reader = cmd.ExecuteReader();
            while (reader.Read()) lista.Add(Mapear(reader));
            return lista;
        }

        public virtual T? ObtenerPorId(int id, string bd, string[]? columnas = null, bool considerarInactivos = false)
        {
            string sql = $"SELECT {ConstruirSelect(columnas)} FROM {_nombreTabla} WHERE Id = @id AND (@considerarInactivos = 1 OR Estado = 1)";
            using var conn = CrearConexion(bd);
            conn.Open();
            using var cmd = new SqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@id", id);
            cmd.Parameters.AddWithValue("@considerarInactivos", considerarInactivos ? 1 : 0);
            using var reader = cmd.ExecuteReader();
            return reader.Read() ? Mapear(reader) : null;
        }

        public virtual void EliminarSuave(int id, string bd, int idUsuario)
        {
            string sql = $@"UPDATE {_nombreTabla} SET Estado = 0, IdUsuarioEliminacion = @idUsr, FechaEliminacion = GETDATE() WHERE Id = @id";
            using var conn = CrearConexion(bd);
            conn.Open();
            using var cmd = new SqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@id", id);
            cmd.Parameters.AddWithValue("@idUsr", idUsuario);
            cmd.ExecuteNonQuery();
        }

        public virtual ResultadoPaginado<T> ListarPaginado(ListarPaginadoPeticion peticion, string bd, Action<SqlCommand>? agregarParametrosExtra = null)
        {
            var resultado = new ResultadoPaginado<T> { Data = new List<T>() };
            int omitir = (peticion.Pagina - 1) * peticion.Cantidad;

            using var conn = CrearConexion(bd);
            conn.Open();
            using var cmd = new SqlCommand($"USP_{_nombreTabla}_ListarPaginado", conn) { CommandType = CommandType.StoredProcedure };
            cmd.Parameters.AddWithValue("@Omitir", omitir);
            cmd.Parameters.AddWithValue("@Cantidad", peticion.Cantidad);
            cmd.Parameters.AddWithValue("@Busqueda", (object?)peticion.Busqueda ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@Estado", (object?)peticion.Estado ?? DBNull.Value);
            if (!string.IsNullOrEmpty(peticion.FechaIni))
            {
                cmd.Parameters.AddWithValue("@FechaIni", (object?)peticion.FechaIni ?? DBNull.Value);
            }
            if (!string.IsNullOrEmpty(peticion.FechaFin))
            {
                cmd.Parameters.AddWithValue("@FechaFin", (object?)peticion.FechaFin ?? DBNull.Value);
            }
            agregarParametrosExtra?.Invoke(cmd);

            using var reader = cmd.ExecuteReader();
            if (reader.Read()) resultado.TotalSinFiltrar = Convert.ToInt32(reader[0]);
            if (reader.NextResult() && reader.Read()) resultado.TotalFiltrado = Convert.ToInt32(reader[0]);
            if (reader.NextResult()) while (reader.Read()) resultado.Data.Add(Mapear(reader));

            return resultado;
        }

        public virtual void Upsert(T entidad, string bd, int idUsuario, Action<SqlCommand, T> mapearParametros)
        {
            using var conn = CrearConexion(bd);
            conn.Open();
            using var cmd = new SqlCommand($"USP_{_nombreTabla}_Upsert", conn) { CommandType = CommandType.StoredProcedure };
            mapearParametros(cmd, entidad);
            cmd.Parameters.AddWithValue("@IdUsuarioAuditoria", idUsuario);
            cmd.ExecuteNonQuery();
        }

        #endregion

        #region Operaciones Asíncronas

        public virtual async Task<List<T>> ListarAsync(string bd, string[]? columnas = null, List<FiltroBusqueda>? filtros = null, int? limite = null, bool considerarInactivos = false)
        {
            var lista = new List<T>();
            string sql = $"SELECT {ConstruirTop(limite)} {ConstruirSelect(columnas)} FROM {_nombreTabla} {ConstruirWhere(filtros, considerarInactivos)}";

            using var conn = CrearConexion(bd);
            await conn.OpenAsync();
            using var cmd = new SqlCommand(sql, conn);
            if (filtros != null) foreach (var f in filtros) cmd.Parameters.Add(f.Parametro);

            using var reader = await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync()) lista.Add(Mapear(reader));
            return lista;
        }

        public virtual async Task<T?> ObtenerPorIdAsync(int id, string bd, string[]? columnas = null, bool considerarInactivos = false)
        {
            string sql = $"SELECT {ConstruirSelect(columnas)} FROM {_nombreTabla} WHERE Id = @id AND (@considerarInactivos = 1 OR Estado = 1)";
            using var conn = CrearConexion(bd);
            await conn.OpenAsync();
            using var cmd = new SqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@id", id);
            cmd.Parameters.AddWithValue("@considerarInactivos", considerarInactivos ? 1 : 0);
            using var reader = await cmd.ExecuteReaderAsync();
            return await reader.ReadAsync() ? Mapear(reader) : null;
        }

        public virtual async Task EliminarSuaveAsync(int id, string bd, int idUsuario)
        {
            string sql = $@"UPDATE {_nombreTabla} SET Estado = 0, IdUsuarioEliminacion = @idUsr, FechaEliminacion = GETDATE() WHERE Id = @id";
            using var conn = CrearConexion(bd);
            await conn.OpenAsync();
            using var cmd = new SqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@id", id);
            cmd.Parameters.AddWithValue("@idUsr", idUsuario);
            await cmd.ExecuteNonQueryAsync();
        }

        public virtual async Task<ResultadoPaginado<T>> ListarPaginadoAsync(ListarPaginadoPeticion peticion,
        string bd, Action<SqlCommand>? agregarParametrosExtra = null, CancellationToken? ct = null)
        {
            var resultado = new ResultadoPaginado<T> { Data = new List<T>() };
            int omitir = (peticion.Pagina - 1) * peticion.Cantidad;

            using var conn = CrearConexion(bd);
            await conn.OpenAsync();
            using var cmd = new SqlCommand($"USP_{_nombreTabla}_ListarPaginado", conn) { CommandType = CommandType.StoredProcedure };
            cmd.Parameters.AddWithValue("@Omitir", omitir);
            cmd.Parameters.AddWithValue("@Cantidad", peticion.Cantidad);
            cmd.Parameters.AddWithValue("@Busqueda", string.IsNullOrEmpty(peticion.Busqueda) ? DBNull.Value : peticion.Busqueda);
            cmd.Parameters.AddWithValue("@Estado", (object?)peticion.Estado ?? DBNull.Value);

            if (!string.IsNullOrEmpty(peticion.FechaIni))
            {
                cmd.Parameters.AddWithValue("@FechaIni", (object?)peticion.FechaIni ?? DBNull.Value);
            }
            if (!string.IsNullOrEmpty(peticion.FechaFin))
            {
                cmd.Parameters.AddWithValue("@FechaFin", (object?)peticion.FechaFin ?? DBNull.Value);
            }

            agregarParametrosExtra?.Invoke(cmd);

            cmd.CommandTimeout = 60;
            using var reader = await cmd.ExecuteReaderAsync();

            if (await reader.ReadAsync()) resultado.TotalSinFiltrar = Convert.ToInt32(reader[0]);

            if (await reader.NextResultAsync() && await reader.ReadAsync()) resultado.TotalFiltrado = Convert.ToInt32(reader[0]);

            if (await reader.NextResultAsync())
            {
                while (await reader.ReadAsync()) resultado.Data.Add(Mapear(reader));
            }

            return resultado;
        }

        public virtual async Task UpsertAsync(T entidad, string bd, int idUsuario, Action<SqlCommand, T> mapearParametros)
        {
            using var conn = CrearConexion(bd);
            await conn.OpenAsync();
            using var cmd = new SqlCommand($"USP_{_nombreTabla}_Upsert", conn) { CommandType = CommandType.StoredProcedure };
            mapearParametros(cmd, entidad);
            cmd.Parameters.AddWithValue("@IdUsuarioAuditoria", idUsuario);
            await cmd.ExecuteNonQueryAsync();
        }

        public virtual async Task ActualizarEstadoAsync(int id, string bd, int idUsuario, int nuevoEstado)
        {
            string sql = $@"UPDATE {_nombreTabla} SET 
                            Estado = @nuevoEstado, 
                            IdUsuarioModificacion = CASE WHEN @nuevoEstado = 1 THEN @idUsr ELSE IdUsuarioModificacion END, 
                            IdUsuarioEliminacion = CASE WHEN @nuevoEstado = 0 THEN @idUsr ELSE IdUsuarioEliminacion END, 
                            FechaEliminacion = CASE WHEN @nuevoEstado = 0 THEN GETDATE() ELSE FechaEliminacion END, 
                            FechaModificacion = CASE WHEN @nuevoEstado = 1 THEN GETDATE() ELSE FechaModificacion END 
                            WHERE Id = @id";

            using var conn = CrearConexion(bd);
            await conn.OpenAsync();
            using var cmd = new SqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@id", id);
            cmd.Parameters.AddWithValue("@idUsr", idUsuario);
            cmd.Parameters.AddWithValue("@nuevoEstado", nuevoEstado);
            await cmd.ExecuteNonQueryAsync();
        }

        #endregion

        public class FiltroBusqueda
        {
            public string Condicion { get; set; }
            public SqlParameter Parametro { get; set; }

            public FiltroBusqueda(string condicion, string nombreParam, object valor)
            {
                Condicion = condicion;
                Parametro = new SqlParameter(nombreParam, valor ?? DBNull.Value);
            }
        }
    }
}