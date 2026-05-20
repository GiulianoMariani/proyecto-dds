using System;
using System.Data;
using System.Drawing;
using System.Windows.Forms;

namespace SistemaEstacionamientoGUI
{
    public class MainForm : Form
    {
        // Controles
        private TextBox txtPatente;
        private ComboBox cmbTipoVehiculo;
        private Button btnIngresar;
        private Button btnRetirar;
        private Button btnReporte;
        private DataGridView gridVehiculos;
        private Label lblMensaje;
        private Panel pnlHeader; // Panel superior decorativo

        private GestorBaseDatos db = new GestorBaseDatos();

        // Colores de la paleta profesional
        Color ColorPrimario = Color.FromArgb(44, 62, 80);    // Azul oscuro (Midnight Blue)
        Color ColorExito = Color.FromArgb(39, 174, 96);      // Verde Esmeralda
        Color ColorPeligro = Color.FromArgb(192, 57, 43);    // Rojo Alizarin
        Color ColorInfo = Color.FromArgb(52, 152, 219);      // Azul claro (Peter River)
        Color ColorFondo = Color.FromArgb(236, 240, 241);    // Gris muy claro (Clouds)

        public MainForm()
        {
            // Configuración de la Ventana
            this.Text = "Parking Control Pro";
            this.Size = new Size(800, 500);
            this.BackColor = ColorFondo;
            this.Font = new Font("Segoe UI", 9F, FontStyle.Regular);
            this.FormBorderStyle = FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.StartPosition = FormStartPosition.CenterScreen;

            // --- PANEL DE CABECERA ---
            pnlHeader = new Panel();
            pnlHeader.Dock = DockStyle.Top;
            pnlHeader.Height = 60;
            pnlHeader.BackColor = ColorPrimario;

            Label lblTitulo = new Label();
            lblTitulo.Text = "SISTEMA DE GESTIÓN DE ESTACIONAMIENTO";
            lblTitulo.ForeColor = Color.White;
            lblTitulo.Font = new Font("Segoe UI", 14F, FontStyle.Bold);
            lblTitulo.AutoSize = false;
            lblTitulo.TextAlign = ContentAlignment.MiddleCenter;
            lblTitulo.Dock = DockStyle.Fill;
            pnlHeader.Controls.Add(lblTitulo);

            // --- CONTROLES DE ENTRADA (Grupo superior) ---
            Label lblPatente = new Label() { Text = "PATENTE:", Location = new Point(30, 85), AutoSize = true, Font = new Font("Segoe UI", 9F, FontStyle.Bold) };
            txtPatente = new TextBox() { Location = new Point(30, 105), Width = 120, Font = new Font("Segoe UI", 12F), CharacterCasing = CharacterCasing.Upper };

            Label lblTipo = new Label() { Text = "TIPO DE VEHÍCULO:", Location = new Point(170, 85), AutoSize = true, Font = new Font("Segoe UI", 9F, FontStyle.Bold) };
            cmbTipoVehiculo = new ComboBox() { Location = new Point(170, 105), Width = 130, Font = new Font("Segoe UI", 10F), DropDownStyle = ComboBoxStyle.DropDownList };
            cmbTipoVehiculo.Items.AddRange(new string[] { "Auto", "Moto", "Camioneta", "Camión" });
            cmbTipoVehiculo.SelectedIndex = 0;

            // --- BOTONES ESTILIZADOS ---
            btnIngresar = CrearBoton("INGRESAR", 320, 103, ColorExito);
            btnIngresar.Click += BtnIngresar_Click;

            btnRetirar = CrearBoton("RETIRAR", 440, 103, ColorPeligro);
            btnRetirar.Click += BtnRetirar_Click;

            btnReporte = CrearBoton("CAJA DIARIA", 630, 103, ColorInfo);
            btnReporte.Width = 120;
            btnReporte.Click += BtnReporte_Click;

            // --- DATA GRID VIEW (TABLA) ---
            gridVehiculos = new DataGridView();
            gridVehiculos.Location = new Point(30, 160);
            gridVehiculos.Size = new Size(720, 240);
            gridVehiculos.BackgroundColor = Color.White;
            gridVehiculos.BorderStyle = BorderStyle.None;
            gridVehiculos.AllowUserToAddRows = false;
            gridVehiculos.ReadOnly = true;
            gridVehiculos.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            gridVehiculos.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            gridVehiculos.RowHeadersVisible = false;
            gridVehiculos.CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal;
            gridVehiculos.EnableHeadersVisualStyles = false; // Permite cambiar color al encabezado

            // Estilo del encabezado
            gridVehiculos.ColumnHeadersHeight = 40;
            gridVehiculos.ColumnHeadersDefaultCellStyle.BackColor = ColorPrimario;
            gridVehiculos.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
            gridVehiculos.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            gridVehiculos.ColumnHeadersDefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;

            // Estilo de las celdas
            gridVehiculos.DefaultCellStyle.SelectionBackColor = Color.FromArgb(189, 195, 199);
            gridVehiculos.DefaultCellStyle.SelectionForeColor = Color.Black;
            gridVehiculos.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(242, 243, 244);
            
            gridVehiculos.CellClick += GridVehiculos_CellClick;

            // --- MENSAJES DE ESTADO ---
            lblMensaje = new Label() { 
                Location = new Point(30, 420), 
                Size = new Size(720, 30),
                Text = "Listo para operar.",
                TextAlign = ContentAlignment.MiddleLeft,
                Font = new Font("Segoe UI", 9F, FontStyle.Italic),
                ForeColor = ColorPrimario
            };

            // Agregar controles al Form
            this.Controls.Add(pnlHeader);
            this.Controls.Add(lblPatente);
            this.Controls.Add(txtPatente);
            this.Controls.Add(lblTipo);
            this.Controls.Add(cmbTipoVehiculo);
            this.Controls.Add(btnIngresar);
            this.Controls.Add(btnRetirar);
            this.Controls.Add(btnReporte);
            this.Controls.Add(gridVehiculos);
            this.Controls.Add(lblMensaje);

            ActualizarTabla();
        }

        // Función ayudante para crear botones con estilo uniforme
        private Button CrearBoton(string texto, int x, int y, Color colorFondo)
        {
            Button btn = new Button();
            btn.Text = texto;
            btn.Location = new Point(x, y);
            btn.Size = new Size(110, 35);
            btn.BackColor = colorFondo;
            btn.ForeColor = Color.White;
            btn.FlatStyle = FlatStyle.Flat;
            btn.FlatAppearance.BorderSize = 0;
            btn.Cursor = Cursors.Hand;
            btn.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            return btn;
        }

        private void BtnIngresar_Click(object sender, EventArgs e)
        {
            string patente = txtPatente.Text.Trim();
            if (string.IsNullOrEmpty(patente)) { lblMensaje.Text = "⚠ Error: Debe ingresar una patente."; return; }

            string tipo = cmbTipoVehiculo.SelectedItem.ToString();
            try
            {
                db.IngresarVehiculo(patente, tipo);
                lblMensaje.Text = $"✅ {tipo} con patente {patente} ingresado correctamente.";
                txtPatente.Clear();
                ActualizarTabla();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void BtnRetirar_Click(object sender, EventArgs e)
        {
            string patente = txtPatente.Text.Trim();
            if (string.IsNullOrEmpty(patente)) { lblMensaje.Text = "⚠ Error: Seleccione o escriba una patente."; return; }

            try
            {
                string ticket = db.RetirarVehiculo(patente);
                MessageBox.Show(ticket, "Ticket de Salida", MessageBoxButtons.OK, MessageBoxIcon.Information);
                lblMensaje.Text = $"💸 Vehículo {patente} retirado y cobrado.";
                txtPatente.Clear();
                ActualizarTabla();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Aviso", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void BtnReporte_Click(object sender, EventArgs e)
        {
            ReporteForm ventanaReporte = new ReporteForm(db);
            ventanaReporte.ShowDialog();
        }

        private void GridVehiculos_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0)
            {
                txtPatente.Text = gridVehiculos.Rows[e.RowIndex].Cells["Patente"].Value.ToString();
            }
        }

        private void ActualizarTabla()
        {
            gridVehiculos.DataSource = db.ObtenerVehiculosEstacionados();
        }
    }

    // --- VENTANA DE REPORTE ESTILIZADA ---
    public class ReporteForm : Form
    {
        public ReporteForm(GestorBaseDatos db)
        {
            this.Text = "Balance Diario";
            this.Size = new Size(350, 250);
            this.BackColor = Color.White;
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;

            Panel pnlBorde = new Panel() { Dock = DockStyle.Top, Height = 10, BackColor = Color.FromArgb(52, 152, 219) };
            
            Label lblTitulo = new Label() { 
                Text = "CAJA DEL DÍA", 
                Location = new Point(20, 30), 
                AutoSize = true, 
                Font = new Font("Segoe UI", 14F, FontStyle.Bold), 
                ForeColor = Color.FromArgb(44, 62, 80) 
            };

            Label lblTotal = new Label() { 
                Location = new Point(20, 80), 
                Size = new Size(300, 40),
                Font = new Font("Segoe UI", 20F, FontStyle.Bold), 
                ForeColor = Color.FromArgb(39, 174, 96) 
            };

            Label lblCantidad = new Label() { 
                Location = new Point(20, 130), 
                AutoSize = true, 
                Font = new Font("Segoe UI", 10F, FontStyle.Regular),
                ForeColor = Color.Gray
            };
            
            Button btnCerrar = new Button() { 
                Text = "ENTENDIDO", 
                Location = new Point(100, 170), 
                Width = 150, 
                Height = 35,
                BackColor = Color.FromArgb(44, 62, 80),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };
            btnCerrar.Click += (s, ev) => this.Close();

            try
            {
                db.ObtenerReporteDiario(out decimal total, out int cantidad);
                lblTotal.Text = $"$ {total:N2}"; // Formato moneda
                lblCantidad.Text = $"Total de vehículos retirados: {cantidad}";
            }
            catch { lblTotal.Text = "$ 0.00"; }

            this.Controls.Add(pnlBorde);
            this.Controls.Add(lblTitulo);
            this.Controls.Add(lblTotal);
            this.Controls.Add(lblCantidad);
            this.Controls.Add(btnCerrar);
        }
    }

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