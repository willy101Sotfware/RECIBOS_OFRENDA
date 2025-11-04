namespace RECIBOS_OFRENDA
{
    internal class FacturaData
    {
        public string IdTransaccionApi { get; set; } = "";
        public string Documento { get; set; } = "";
        public string Referencia { get; set; } = "";
        public string Cliente { get; set; } = "";
        public string Descripcion { get; set; } = "";
        public string TipoRecaudo { get; set; } = "";
        public string TipoPago { get; set; } = "";
        public decimal Total { get; set; }
        public decimal TotalIngresado { get; set; }
        public decimal TotalDevuelta { get; set; }
        public string EstadoTransaccionVerb { get; set; } = "";
        public string FechaTexto { get; set; } = "";
    }
}
