using System;
using System.Data;
using System.Drawing;
using System.Windows.Forms;
using System.Text.RegularExpressions; // <-- NECESARIO PARA LAS VALIDACIONES

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
        private Panel pnlHeader;

        private GestorBaseDatos db = new GestorBaseDatos();

        // Colores
        Color ColorPrimario = Color.FromArgb(44, 62, 80);
        Color ColorExito = Color.FromArgb(39, 174, 96);
        Color ColorPeligro = Color.FromArgb(192, 57, 43);
        Color ColorInfo = Color.FromArgb(52, 152, 219);
        Color ColorFondo = Color.FromArgb(236, 240, 241);

        public MainForm()
        {
            this.Text = "Parking Control Pro";
            this.Size = new Size(800, 500);
            this.BackColor = ColorFondo;
            this.Font = new Font("Segoe UI", 9F, FontStyle.Regular);
            this.FormBorderStyle = FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.StartPosition = FormStartPosition.CenterScreen;

            // --- PANEL DE CABECERA ---
            pnlHeader = new Panel() { Dock = DockStyle.Top, Height = 60, BackColor = ColorPrimario };
            Label lblTitulo = new Label() { 
                Text = "SISTEMA DE GESTIÓN DE ESTACIONAMIENTO", 
                ForeColor = Color.White, Font = new Font("Segoe UI", 14F, FontStyle.Bold), 
                AutoSize = false, TextAlign = ContentAlignment.MiddleCenter, Dock = DockStyle.Fill 
            };
            pnlHeader.Controls.Add(lblTitulo);

            // --- CONTROLES DE ENTRADA ---
            Label lblPatente = new Label() { Text = "PATENTE:", Location = new Point(30, 85), AutoSize = true, Font = new Font("Segoe UI", 9F, FontStyle.Bold) };
            txtPatente = new TextBox() { Location = new Point(30, 105), Width = 120, Font = new Font("Segoe UI", 12F), CharacterCasing = CharacterCasing.Upper };

            Label lblTipo = new Label() { Text = "TIPO DE VEHÍCULO:", Location = new Point(170, 85), AutoSize = true, Font = new Font("Segoe UI", 9F, FontStyle.Bold) };
            cmbTipoVehiculo = new ComboBox() { Location = new Point(170, 105), Width = 130, Font = new Font("Segoe UI", 10F), DropDownStyle = ComboBoxStyle.DropDownList };
            cmbTipoVehiculo.Items.AddRange(new string[] { "Auto", "Moto", "Camioneta", "Camión" });
            cmbTipoVehiculo.SelectedIndex = 0;

            // --- BOTONES ---
            btnIngresar = CrearBoton("INGRESAR", 320, 103, ColorExito);
            btnIngresar.Click += BtnIngresar_Click;

            btnRetirar = CrearBoton("RETIRAR", 440, 103, ColorPeligro);
            btnRetirar.Click += BtnRetirar_Click;

            btnReporte = CrearBoton("CAJA DIARIA", 630, 103, ColorInfo);
            btnReporte.Width = 120;
            btnReporte.Click += BtnReporte_Click;

            // --- TABLA ---
            gridVehiculos = new DataGridView() { 
                Location = new Point(30, 160), Size = new Size(720, 240), BackgroundColor = Color.White, 
                BorderStyle = BorderStyle.None, AllowUserToAddRows = false, ReadOnly = true, 
                SelectionMode = DataGridViewSelectionMode.FullRowSelect, AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill, 
                RowHeadersVisible = false, CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal, 
                EnableHeadersVisualStyles = false 
            };
            gridVehiculos.ColumnHeadersHeight = 40;
            gridVehiculos.ColumnHeadersDefaultCellStyle.BackColor = ColorPrimario;
            gridVehiculos.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
            gridVehiculos.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            gridVehiculos.ColumnHeadersDefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            gridVehiculos.DefaultCellStyle.SelectionBackColor = Color.FromArgb(189, 195, 199);
            gridVehiculos.DefaultCellStyle.SelectionForeColor = Color.Black;
            gridVehiculos.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(242, 243, 244);
            gridVehiculos.CellClick += GridVehiculos_CellClick;

            lblMensaje = new Label() { Location = new Point(30, 420), Size = new Size(720, 30), Text = "Listo para operar.", TextAlign = ContentAlignment.MiddleLeft, Font = new Font("Segoe UI", 9F, FontStyle.Italic), ForeColor = ColorPrimario };

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

            // Carga inicial
            ActualizarTabla();

            // === TEMPORIZADOR DE ACTUALIZACIÓN AUTOMÁTICA ===
            System.Windows.Forms.Timer timerRefresco = new System.Windows.Forms.Timer();
            timerRefresco.Interval = 5000; // Refresca cada 5 segundos
            timerRefresco.Tick += (s, ev) => ActualizarTabla();
            timerRefresco.Start();
        }

        // =========================================================
        // MÉTODOS DE LA INTERFAZ
        // =========================================================

        private Button CrearBoton(string texto, int x, int y, Color colorFondo)
        {
            Button btn = new Button() { Text = texto, Location = new Point(x, y), Size = new Size(110, 35), BackColor = colorFondo, ForeColor = Color.White, FlatStyle = FlatStyle.Flat, Cursor = Cursors.Hand, Font = new Font("Segoe UI", 9F, FontStyle.Bold) };
            btn.FlatAppearance.BorderSize = 0;
            return btn;
        }

        private string ProcesarPatente(string patenteIngresada)
        {
            // 1. Limpiamos espacios y guiones, y pasamos a mayúsculas
            string limpia = patenteIngresada.Replace(" ", "").Replace("-", "").ToUpper();

            // 2. Validamos longitud mínima (5) y máxima (9)
            if (limpia.Length < 5 || limpia.Length > 9)
                throw new Exception("La patente debe tener entre 5 y 9 caracteres.");

            // 3. Validamos que solo contenga letras y números (sin símbolos)
            if (!Regex.IsMatch(limpia, @"^[A-Z0-9]+$"))
                throw new Exception("La patente solo puede contener letras y números.");

            // 4. Debe tener al menos una letra Y al menos un número
            if (!Regex.IsMatch(limpia, @"[A-Z]") || !Regex.IsMatch(limpia, @"[0-9]"))
                throw new Exception("La patente no es válida. Debe contener letras y números.");
            
            // 5. FORMATEO SUAVE (Modelos Argentinos)
            if (Regex.IsMatch(limpia, @"^[A-Z]{3}[0-9]{3}$")) return limpia.Insert(3, " "); // Auto Viejo: AAA 123
            if (Regex.IsMatch(limpia, @"^[A-Z]{2}[0-9]{3}[A-Z]{2}$")) return limpia.Insert(2, " ").Insert(6, " "); // Auto Nuevo: AB 123 CD
            if (Regex.IsMatch(limpia, @"^[A-Z]{1}[0-9]{3}[A-Z]{3}$")) return limpia.Insert(1, " ").Insert(5, " "); // Moto Nueva: A 123 BCD

            // Si es extranjera o de otro formato válido
            return limpia;
        }

        private void BtnIngresar_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(txtPatente.Text.Trim())) { lblMensaje.Text = "⚠ Error: Debe ingresar una patente."; return; }

            try
            {
                string patenteProcesada = ProcesarPatente(txtPatente.Text.Trim());
                string tipo = cmbTipoVehiculo.SelectedItem.ToString();

                db.IngresarVehiculo(patenteProcesada, tipo);
                
                lblMensaje.Text = $"✅ {tipo} con patente {patenteProcesada} ingresado correctamente.";
                txtPatente.Clear();
                ActualizarTabla();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error de Validación", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void BtnRetirar_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(txtPatente.Text.Trim())) { lblMensaje.Text = "⚠ Error: Seleccione o escriba una patente."; return; }

            try
            {
                string patenteProcesada = ProcesarPatente(txtPatente.Text.Trim());

                string ticket = db.RetirarVehiculo(patenteProcesada);
                MessageBox.Show(ticket, "Ticket de Salida", MessageBoxButtons.OK, MessageBoxIcon.Information);
                lblMensaje.Text = $"💸 Vehículo {patenteProcesada} retirado y cobrado.";
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
            try
            {
                gridVehiculos.DataSource = db.ObtenerVehiculosEstacionados();
            }
            catch (Exception ex)
            {
                lblMensaje.Text = "Error al actualizar: " + ex.Message;
            }
        }
    }

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
            Label lblTitulo = new Label() { Text = "CAJA DEL DÍA", Location = new Point(20, 30), AutoSize = true, Font = new Font("Segoe UI", 14F, FontStyle.Bold), ForeColor = Color.FromArgb(44, 62, 80) };
            Label lblTotal = new Label() { Location = new Point(20, 80), Size = new Size(300, 40), Font = new Font("Segoe UI", 20F, FontStyle.Bold), ForeColor = Color.FromArgb(39, 174, 96) };
            Label lblCantidad = new Label() { Location = new Point(20, 130), AutoSize = true, Font = new Font("Segoe UI", 10F, FontStyle.Regular), ForeColor = Color.Gray };
            Button btnCerrar = new Button() { Text = "ENTENDIDO", Location = new Point(100, 170), Width = 150, Height = 35, BackColor = Color.FromArgb(44, 62, 80), ForeColor = Color.White, FlatStyle = FlatStyle.Flat };
            btnCerrar.Click += (s, ev) => this.Close();

            try {
                db.ObtenerReporteDiario(out decimal total, out int cantidad);
                lblTotal.Text = $"$ {total:N2}";
                lblCantidad.Text = $"Total de vehículos retirados: {cantidad}";
            } catch { lblTotal.Text = "$ 0.00"; }

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