using System;

namespace SistemaEstacionamientoGUI
{
   
    public class Vehiculo
    {
        public int Id { get; set; }
        public string Patente { get; set; }
        public DateTime HoraEntrada { get; set; }
        public DateTime? HoraSalida { get; set; } // El ? (significa que puede ser NULL)
        public decimal? CostoTotal { get; set; }
        public string Estado { get; set; }
        public string TipoVehiculo { get; set; }
    }
}