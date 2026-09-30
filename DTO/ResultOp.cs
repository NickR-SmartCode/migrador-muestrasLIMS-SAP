namespace DTO
{
    public enum ResultOpErrores
    {
        SUCCESS = 0,
        VALIDATION_ERROR = 1,
        NOT_FOUND = 2,
        UNAUTHORIZED = 3,
        INTERNAL_SERVER_ERROR = 500,
        CANCELLED = 50,
        TIMEOUT = 61
    }

    public class ResultOpErr
    {
        public string Msg { get; set; } = string.Empty;
        public ResultOpErrores Tipo { get; set; }
        public string Detalle { get; set; } = string.Empty;
        public Dictionary<string, string[]> ErroresCampos { get; set; } = new Dictionary<string, string[]>();
    }

    public class ResultOp<T>
    {
        public bool Exito { get; set; }
        public T? Datos { get; set; } 
        public ResultOpErr? Error { get; set; }



        public static ResultOp<T> Ok(T data) =>
            new ResultOp<T> { Exito = true, Datos = data };

        public static ResultOp<T> Fallo(string message, ResultOpErrores type = ResultOpErrores.VALIDATION_ERROR) =>
            new ResultOp<T>
            {
                Exito = false,
                Error = new ResultOpErr { Msg = message, Tipo = type }
            };
    }
}
