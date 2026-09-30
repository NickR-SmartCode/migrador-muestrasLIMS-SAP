
using DTO;
using Sap.Data.Hana; // Driver SAP.DATA.HANA v8.0
using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;


namespace DAO.SAPLINKER
{
    public abstract class HANADAOBase<T> where T : class, new()
    {
        protected readonly string _nombreTabla;
        private readonly Conexion _conexion;

        protected HANADAOBase(Conexion conexion, string nombreTabla)
        {
            _conexion = conexion;

            _nombreTabla = nombreTabla;
        }

        protected abstract T Mapear(HanaDataReader reader);

        protected HanaConnection CrearConexion(string bd)
        {
            return _conexion.CrearConexionHana(bd);
        }

        #region Helpers de Construcción SQL

        private string ConstruirSelect(string[]? columnas)  {
        return (columnas != null && columnas.Length > 0) ? string.Join(", ", columnas) : "*";
        }
            

        private string ConstruirLimit(int? limite) =>
            (limite.HasValue && limite.Value > 0) ? $"LIMIT {limite.Value}" : "";

        private string ConstruirWhere(List<FiltroBusqueda>? filtros)
        {
            if (filtros == null || filtros.Count == 0) return "";

            List<string> condiciones = new List<string>();
            foreach (var f in filtros) condiciones.Add($"({f.Condicion})");

            return "WHERE " + string.Join(" AND ", condiciones);
        }

        protected TValue? ObtenerValor<TValue>(HanaDataReader reader, string columnName)
        {
            int ordinal;
            try { ordinal = reader.GetOrdinal(columnName); }
            catch { return default; }

            if (reader.IsDBNull(ordinal)) return default;

            object value = reader.GetValue(ordinal);
            try
            {
                if (value is TValue valorDirecto) return valorDirecto;
                Type targetType = Nullable.GetUnderlyingType(typeof(TValue)) ?? typeof(TValue);
                return (TValue)Convert.ChangeType(value, targetType);
            }
            catch { return default; }
        }

        #endregion

        #region Operaciones

        /// <summary>
        /// Listado básico con filtros y límite (Sintaxis HANA LIMIT)
        /// </summary>
        public virtual async Task<List<T>> ListarAsync(string bd, string[]? columnas = null, List<FiltroBusqueda>? filtros = null, int? limite = null)
        {
            var lista = new List<T>();
            string sql = $"SELECT {ConstruirSelect(columnas)} FROM {_nombreTabla} {ConstruirWhere(filtros)} {ConstruirLimit(limite)}";

            using var conn = CrearConexion(bd);
            await conn.OpenAsync();
            using var cmd = new HanaCommand(sql, conn);
            if (filtros != null) foreach (var f in filtros) cmd.Parameters.Add(f.Parametro);

            using var reader = (HanaDataReader)await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync()) lista.Add(Mapear(reader));
            return lista;
        }

        /// <summary>
        /// Obtener por DocEntry (Primary Key estándar de SAP B1)
        /// </summary>
        public virtual async Task<T?> ObtenerPorDocEntryAsync(int docEntry, string bd, string[]? columnas = null)
        {
            string sql = $"SELECT {ConstruirSelect(columnas)} FROM {_nombreTabla} WHERE \"DocEntry\" = @docEntry";
            using var conn = CrearConexion(bd);
            await conn.OpenAsync();
            using var cmd = new HanaCommand(sql, conn);
            cmd.Parameters.Add(new HanaParameter("@docEntry", docEntry));

            using var reader = (HanaDataReader)await cmd.ExecuteReaderAsync();
            return await reader.ReadAsync() ? Mapear(reader) : null;
        }

        /// <summary>
        /// Listado Paginado construido dinámicamente para HANA (LIMIT / OFFSET)
        /// </summary>
        public virtual async Task<ResultadoPaginado<T>> ListarPaginadoAsync(ListarPaginadoPeticion peticion, string bd, string orderBy = "\"DocEntry\" DESC")
        {
            var resultado = new ResultadoPaginado<T> { Data = new List<T>() };
            int offset = (peticion.Pagina - 1) * peticion.Cantidad;


            string sqlCount = $"SELECT COUNT(*) FROM {_nombreTabla}";


            string sqlData = $@"SELECT * FROM {_nombreTabla} 
                                ORDER BY {orderBy} 
                                LIMIT {peticion.Cantidad} OFFSET {offset}";

            using var conn = CrearConexion(bd);
            await conn.OpenAsync();


            using (var cmdCount = new HanaCommand(sqlCount, conn))
            {
                resultado.TotalSinFiltrar = Convert.ToInt32(await cmdCount.ExecuteScalarAsync());
                resultado.TotalFiltrado = resultado.TotalSinFiltrar;
            }


            using (var cmdData = new HanaCommand(sqlData, conn))
            {
                using var reader = (HanaDataReader)await cmdData.ExecuteReaderAsync();
                while (await reader.ReadAsync())
                {
                    resultado.Data.Add(Mapear(reader));
                }
            }

            return resultado;
        }

        #endregion

      
    }
}
