using System;
using System.Data;
using System.Drawing;
using System.Windows.Forms;
using MySql.Data.MySqlClient;

namespace SistemaEstacionamientoGUI
{
    public class MainForm : Form
    {
        // Controles de la interfaz
        private TextBox txtPatente;
        private Button btnIngresar;
        private Button btnRetirar;
        private DataGridView gridVehiculos;
        private Label lblMensaje;

        // Configuración
        private string cadenaConexion = "Server=localhost;Database=EstacionamientoDB;User ID=root;Password=negrito123;";
        private decimal TarifaPorHora = 500.00m;

        public MainForm()
        {
            // Configurar la Ventana (Formulario)
            this.Text = "Sistema de Estacionamiento";
            this.Size = new Size(500, 420);
            this.StartPosition = FormStartPosition.CenterScreen;

            // Crear Controles
            Label lblPatente = new Label() { Text = "Patente:", Location = new Point(20, 25), AutoSize = true };
            txtPatente = new TextBox() { Location = new Point(80, 22), Width = 120, CharacterCasing = CharacterCasing.Upper };

            btnIngresar = new Button() { Text = "Ingresar", Location = new Point(220, 20), BackColor = Color.LightGreen };
            btnIngresar.Click += BtnIngresar_Click;

            btnRetirar = new Button() { Text = "Retirar", Location = new Point(310, 20), BackColor = Color.LightCoral };
            btnRetirar.Click += BtnRetirar_Click;

            gridVehiculos = new DataGridView() { 
                Location = new Point(20, 70), 
                Size = new Size(440, 250), 
                ReadOnly = true, 
                AllowUserToAddRows = false,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill
            };

            lblMensaje = new Label() { Location = new Point(20, 335), AutoSize = true, ForeColor = Color.DarkBlue };

            // Agregar controles a la ventana
            this.Controls.Add(lblPatente);
            this.Controls.Add(txtPatente);
            this.Controls.Add(btnIngresar);
            this.Controls.Add(btnRetirar);
            this.Controls.Add(gridVehiculos);
            this.Controls.Add(lblMensaje);

            // Cargar los datos al abrir
            CargarVehiculos();
        }

        private void BtnIngresar_Click(object sender, EventArgs e)
        {
            string patente = txtPatente.Text.Trim();
            if (string.IsNullOrEmpty(patente))
            {
                lblMensaje.Text = "Por favor, ingrese una patente.";
                return;
            }

            using (MySqlConnection conexion = new MySqlConnection(cadenaConexion))
            {
                try
                {
                    conexion.Open();
                    string query = "INSERT INTO Vehiculos (Patente, HoraEntrada) VALUES (@patente, @horaEntrada)";
                    using (MySqlCommand cmd = new MySqlCommand(query, conexion))
                    {
                        cmd.Parameters.AddWithValue("@patente", patente);
                        cmd.Parameters.AddWithValue("@horaEntrada", DateTime.Now);
                        cmd.ExecuteNonQuery();
                    }
                    lblMensaje.Text = $"Vehículo {patente} ingresado correctamente.";
                    txtPatente.Clear();
                    CargarVehiculos(); // Actualizar la tabla visual
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error: " + ex.Message, "Error de Base de Datos", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void BtnRetirar_Click(object sender, EventArgs e)
        {
            string patente = txtPatente.Text.Trim();
            if (string.IsNullOrEmpty(patente))
            {
                lblMensaje.Text = "Ingrese la patente a retirar.";
                return;
            }

            using (MySqlConnection conexion = new MySqlConnection(cadenaConexion))
            {
                try
                {
                    conexion.Open();
                    string selectQuery = "SELECT Id, HoraEntrada FROM Vehiculos WHERE Patente = @patente AND Estado = 'Estacionado'";
                    int idVehiculo = 0;
                    DateTime horaEntrada = DateTime.MinValue;

                    using (MySqlCommand selectCmd = new MySqlCommand(selectQuery, conexion))
                    {
                        selectCmd.Parameters.AddWithValue("@patente", patente);
                        using (MySqlDataReader reader = selectCmd.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                idVehiculo = reader.GetInt32("Id");
                                horaEntrada = reader.GetDateTime("HoraEntrada");
                            }
                            else
                            {
                                lblMensaje.Text = "Vehículo no encontrado o ya retirado.";
                                return;
                            }
                        }
                    }

                    DateTime horaSalida = DateTime.Now;
                    double horasTotales = Math.Ceiling((horaSalida - horaEntrada).TotalHours);
                    if (horasTotales == 0) horasTotales = 1;

                    decimal costoTotal = (decimal)horasTotales * TarifaPorHora;

                    string updateQuery = "UPDATE Vehiculos SET HoraSalida = @horaSalida, CostoTotal = @costo, Estado = 'Retirado' WHERE Id = @id";
                    using (MySqlCommand updateCmd = new MySqlCommand(updateQuery, conexion))
                    {
                        updateCmd.Parameters.AddWithValue("@horaSalida", horaSalida);
                        updateCmd.Parameters.AddWithValue("@costo", costoTotal);
                        updateCmd.Parameters.AddWithValue("@id", idVehiculo);
                        updateCmd.ExecuteNonQuery();
                    }

                    // Mostrar el Ticket en una ventana emergente
                    string ticket = $"--- TICKET DE SALIDA ---\n\nPatente: {patente}\nEntrada: {horaEntrada}\nSalida: {horaSalida}\nTiempo facturado: {horasTotales} hora(s)\n\nTOTAL A PAGAR: ${costoTotal}";
                    MessageBox.Show(ticket, "Ticket Generado", MessageBoxButtons.OK, MessageBoxIcon.Information);

                    lblMensaje.Text = $"Vehículo {patente} retirado. Cobro: ${costoTotal}";
                    txtPatente.Clear();
                    CargarVehiculos();
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error: " + ex.Message, "Error de Base de Datos", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void CargarVehiculos()
        {
            using (MySqlConnection conexion = new MySqlConnection(cadenaConexion))
            {
                try
                {
                    conexion.Open();
                    string query = "SELECT Patente, HoraEntrada AS 'Hora de Ingreso' FROM Vehiculos WHERE Estado = 'Estacionado'";
                    using (MySqlDataAdapter adapter = new MySqlDataAdapter(query, conexion))
                    {
                        DataTable dt = new DataTable();
                        adapter.Fill(dt);
                        gridVehiculos.DataSource = dt; // Muestra los datos en la grilla automáticamente
                    }
                }
                catch (Exception ex)
                {
                    lblMensaje.Text = "Error al cargar los vehículos: " + ex.Message;
                }
            }
        }
    }

    // Punto de entrada de la aplicación
    static class Program
    {
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new MainForm());
        }
    }
}