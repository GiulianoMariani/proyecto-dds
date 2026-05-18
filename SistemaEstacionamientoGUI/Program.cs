using System;
using System.Data;
using System.Drawing;
using System.Windows.Forms;
using MySql.Data.MySqlClient;

namespace SistemaEstacionamientoGUI
{
    public class MainForm : Form
    {
        // Controles de la interfaz principal
        private TextBox txtPatente;
        private Button btnIngresar;
        private Button btnRetirar;
        private Button btnReporte; // Nuevo botón
        private DataGridView gridVehiculos;
        private Label lblMensaje;

        // Configuración
        private string cadenaConexion = "Server=localhost;Database=EstacionamientoDB;User ID=root;Password=negrito123;";
        private decimal TarifaPorHora = 500.00m;

        public MainForm()
        {
            // Configurar la Ventana Principal (Formulario)
            this.Text = "Sistema de Estacionamiento";
            this.Size = new Size(580, 420); // Ancho aumentado para acomodar el nuevo botón
            this.StartPosition = FormStartPosition.CenterScreen;

            // Crear Controles
            Label lblPatente = new Label() { Text = "Patente:", Location = new Point(20, 25), AutoSize = true };
            txtPatente = new TextBox() { Location = new Point(80, 22), Width = 110, CharacterCasing = CharacterCasing.Upper };

            btnIngresar = new Button() { Text = "Ingresar", Location = new Point(210, 20), Width = 85, BackColor = Color.LightGreen };
            btnIngresar.Click += BtnIngresar_Click;

            btnRetirar = new Button() { Text = "Retirar", Location = new Point(305, 20), Width = 85, BackColor = Color.LightCoral };
            btnRetirar.Click += BtnRetirar_Click;

            // CONFIGURACIÓN DEL NUEVO BOTÓN
            btnReporte = new Button() { Text = "Cobros del Día", Location = new Point(405, 20), Width = 130, BackColor = Color.LightSkyBlue };
            btnReporte.Click += BtnReporte_Click;

            gridVehiculos = new DataGridView() { 
                Location = new Point(20, 70), 
                Size = new Size(520, 250), 
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
            this.Controls.Add(btnReporte); // Agregado a la interfaz
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
                    CargarVehiculos();
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

        // EVENTO QUE ABRE LA NUEVA PESTAÑA/VENTANA
        private void BtnReporte_Click(object sender, EventArgs e)
        {
            // Instanciamos la nueva ventana pasándole la conexión a la BD
            ReporteForm ventanaReporte = new ReporteForm(cadenaConexion);
            // ShowDialog() hace que se abra como una ventana emergente obligatoria (modal)
            ventanaReporte.ShowDialog(); 
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
                        gridVehiculos.DataSource = dt;
                    }
                }
                catch (Exception ex)
                {
                    lblMensaje.Text = "Error al cargar los vehículos: " + ex.Message;
                }
            }
        }
    }


    // ==========================================
    //  VENTANA DE REPORTES DEL DÍA
    // ==========================================
    public class ReporteForm : Form
    {
        public ReporteForm(string conexionString)
        {
            // Configurar esta ventana secundaria
            this.Text = "Reporte de Caja Diario";
            this.Size = new Size(320, 220);
            this.StartPosition = FormStartPosition.CenterParent; // Se centra respecto a la ventana principal
            this.FormBorderStyle = FormBorderStyle.FixedDialog; // Evita que el usuario le cambie el tamaño
            this.MaximizeBox = false;
            this.MinimizeBox = false;

            // Controles de diseño
            Label lblTitulo = new Label() { Text = "RECAUDACIÓN DE HOY", Location = new Point(20, 15), AutoSize = true, Font = new Font("Arial", 11, FontStyle.Bold), ForeColor = Color.DarkSlateGray };
            Label lblTotal = new Label() { Location = new Point(20, 55), AutoSize = true, Font = new Font("Arial", 14, FontStyle.Bold), ForeColor = Color.DarkGreen };
            Label lblCantidad = new Label() { Location = new Point(20, 95), AutoSize = true, Font = new Font("Arial", 10, FontStyle.Regular) };
            
            Button btnCerrar = new Button() { Text = "Cerrar", Location = new Point(110, 140), Width = 80 };
            btnCerrar.Click += (s, ev) => this.Close(); // Cierra esta pestañita

            // Consultar la Base de Datos para traer las estadísticas del día
            using (MySqlConnection conexion = new MySqlConnection(conexionString))
            {
                try
                {
                    conexion.Open();
                    // SUM calcula el dinero total, COUNT cuenta cuántos autos salieron hoy (CURDATE())
                    string query = "SELECT SUM(CostoTotal) AS Total, COUNT(Id) AS Cantidad FROM Vehiculos WHERE Estado = 'Retirado' AND DATE(HoraSalida) = CURDATE()";
                    
                    using (MySqlCommand cmd = new MySqlCommand(query, conexion))
                    {
                        using (MySqlDataReader reader = cmd.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                // Validamos si es NULL (por si todavía no cobraste nada hoy)
                                decimal total = reader.IsDBNull(reader.GetOrdinal("Total")) ? 0.00m : reader.GetDecimal("Total");
                                int cantidad = reader.GetInt32("Cantidad");

                                lblTotal.Text = $"Total: ${total}";
                                lblCantidad.Text = $"Vehículos retirados hoy: {cantidad}";
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    lblTotal.Text = "Error de conexión";
                    lblCantidad.Text = ex.Message;
                }
            }

            // Agregar elementos visuales a la pestañita
            this.Controls.Add(lblTitulo);
            this.Controls.Add(lblTotal);
            this.Controls.Add(lblCantidad);
            this.Controls.Add(btnCerrar);
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