using System;
using System.Data;
using MySql.Data.MySqlClient;

namespace SistemaEstacionamientoGUI
{
    public class GestorBaseDatos
    {
        
        private string cadenaConexion = "Server=localhost;Database=EstacionamientoDB;User ID=root;Password=negrito123;";

        
        public void IngresarVehiculo(string patente, string tipoVehiculo)
        {
            using (MySqlConnection conexion = new MySqlConnection(cadenaConexion))
            {
                conexion.Open();
                string query = "INSERT INTO Vehiculos (Patente, TipoVehiculo, HoraEntrada) VALUES (@patente, @tipoVehiculo, @horaEntrada)";
                using (MySqlCommand cmd = new MySqlCommand(query, conexion))
                {
                    cmd.Parameters.AddWithValue("@patente", patente);
                    cmd.Parameters.AddWithValue("@tipoVehiculo", tipoVehiculo);
                    cmd.Parameters.AddWithValue("@horaEntrada", DateTime.Now);
                    cmd.ExecuteNonQuery();
                }
            }
        }

        
        public string RetirarVehiculo(string patente)
        {
            using (MySqlConnection conexion = new MySqlConnection(cadenaConexion))
            {
                conexion.Open();
                
                // Traemos también el TipoVehiculo de la base de datos
                string selectQuery = "SELECT Id, HoraEntrada, TipoVehiculo FROM Vehiculos WHERE Patente = @patente AND Estado = 'Estacionado'";
                int idVehiculo = 0;
                DateTime horaEntrada = DateTime.MinValue;
                string tipoVehiculo = "Auto"; // Por defecto

                using (MySqlCommand selectCmd = new MySqlCommand(selectQuery, conexion))
                {
                    selectCmd.Parameters.AddWithValue("@patente", patente);
                    using (MySqlDataReader reader = selectCmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            idVehiculo = reader.GetInt32("Id");
                            horaEntrada = reader.GetDateTime("HoraEntrada");
                            tipoVehiculo = reader.GetString("TipoVehiculo");
                        }
                        else
                        {
                            throw new Exception("Vehículo no encontrado o ya retirado.");
                        }
                    }
                }

                // Asignar tarifa por hora según el tipo de vehículo
                decimal tarifaPorHora = 500.00m; // Tarifa base (Auto)
                switch (tipoVehiculo)
                {
                    case "Moto":
                        tarifaPorHora = 200.00m;
                        break;
                    case "Auto":
                        tarifaPorHora = 500.00m;
                        break;
                    case "Camioneta":
                        tarifaPorHora = 800.00m;
                        break;
                    case "Camión":
                        tarifaPorHora = 1200.00m;
                        break;
                }

                // Calcular tiempo y costo total
                DateTime horaSalida = DateTime.Now;
                double horasTotales = Math.Ceiling((horaSalida - horaEntrada).TotalHours);
                if (horasTotales == 0) horasTotales = 1;

                decimal costoTotal = (decimal)horasTotales * tarifaPorHora;

                // Actualizar registro en la BD
                string updateQuery = "UPDATE Vehiculos SET HoraSalida = @horaSalida, CostoTotal = @costo, Estado = 'Retirado' WHERE Id = @id";
                using (MySqlCommand updateCmd = new MySqlCommand(updateQuery, conexion))
                {
                    updateCmd.Parameters.AddWithValue("@horaSalida", horaSalida);
                    updateCmd.Parameters.AddWithValue("@costo", costoTotal);
                    updateCmd.Parameters.AddWithValue("@id", idVehiculo);
                    updateCmd.ExecuteNonQuery();
                }

                return $"--- TICKET DE SALIDA ---\n\nPatente: {patente}\nTipo: {tipoVehiculo}\nTarifa/Hora: ${tarifaPorHora}\nEntrada: {horaEntrada}\nSalida: {horaSalida}\nTiempo facturado: {horasTotales} hora(s)\n\nTOTAL A PAGAR: ${costoTotal}";
            }
        }

        //  Agregamos el Tipo a la consulta para que se vea en la BD
        public DataTable ObtenerVehiculosEstacionados()
        {
            using (MySqlConnection conexion = new MySqlConnection(cadenaConexion))
            {
                conexion.Open();
                string query = "SELECT Patente, TipoVehiculo AS 'Tipo', HoraEntrada AS 'Hora de Ingreso' FROM Vehiculos WHERE Estado = 'Estacionado'";
                using (MySqlDataAdapter adapter = new MySqlDataAdapter(query, conexion))
                {
                    DataTable dt = new DataTable();
                    adapter.Fill(dt);
                    return dt;
                }
            }
        }

        public void ObtenerReporteDiario(out decimal total, out int cantidad)
        {
            total = 0.00m;
            cantidad = 0;

            using (MySqlConnection conexion = new MySqlConnection(cadenaConexion))
            {
                conexion.Open();
                string query = "SELECT SUM(CostoTotal) AS Total, COUNT(Id) AS Cantidad FROM Vehiculos WHERE Estado = 'Retirado' AND DATE(HoraSalida) = CURDATE()";
                
                using (MySqlCommand cmd = new MySqlCommand(query, conexion))
                {
                    using (MySqlDataReader reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            total = reader.IsDBNull(reader.GetOrdinal("Total")) ? 0.00m : reader.GetDecimal("Total");
                            cantidad = reader.GetInt32("Cantidad");
                        }
                    }
                }
            }
        }
    }
}