namespace RECIBOS_OFRENDA
{
    internal class ReceiptData
    {
        public string Titulo { get; set; } = "RECIBO DE OFRENDA";
        public string NumeroRecibo { get; set; } = "000001";
        public string Fecha { get; set; } = "";
        public string Nombre { get; set; } = "";
        public string Documento { get; set; } = "";
        public string Concepto { get; set; } = "OFRENDA";
        public string Valor { get; set; } = "";
        public string Referencia { get; set; } = "";
        public string Pie { get; set; } = "Gracias por su aporte";
    }
}
