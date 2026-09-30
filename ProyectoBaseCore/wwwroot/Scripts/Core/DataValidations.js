class DataValidations {
    static Required = (v) => (v !== null && v !== undefined && v.toString().trim() !== "") || "Este campo es obligatorio";

    static MinLength = (min) => (v) => (v && v.length >= min) || `Mínimo ${min} caracteres`;
    static MaxLength = (max) => (v) => (v && v.length <= max) || `Máximo ${max} caracteres`;

    static Email = (v) => /^[^\s@]+@[^\s@]+\.[^\s@]+$/.test(v) || "Formato de email inválido";
    static Numeric = (v) => (!isNaN(parseFloat(v)) && isFinite(v)) || "Debe ser un número";
}