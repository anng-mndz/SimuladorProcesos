using System;
using System.Drawing;
using System.IO;
using System.Reflection;
using System.Windows.Forms;
using Microsoft.Web.WebView2.WinForms;

namespace AdministradorProcesos
{
    // ============================================================
    // VENTANA DEL MANUAL DE USUARIO
    //
    // El PDF del manual va incrustado como recurso embebido dentro
    // del propio .exe (carpeta Manual/ManualDeUsuario.pdf). Al abrir
    // esta ventana, se extrae una sola vez a una carpeta temporal y
    // se muestra con WebView2, que trae su propio visor de PDF
    // (el mismo motor de Microsoft Edge), todo dentro de esta misma
    // ventana del programa, sin depender de ningun lector externo.
    // ============================================================
    public class ManualForm : Form
    {
        private const string RecursoManual = "AdministradorProcesos.Manual.ManualDeUsuario.pdf";

        private WebView2 visor;
        private Label lblError;

        public ManualForm()
        {
            Text = "Manual de usuario";
            Width = 900;
            Height = 800;
            StartPosition = FormStartPosition.CenterParent;
            MinimumSize = new Size(600, 500);
            BackColor = Tema.Fondo;

            visor = new WebView2
            {
                Dock = DockStyle.Fill
            };
            Controls.Add(visor);

            lblError = new Label
            {
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleCenter,
                Font = Tema.FuenteNormal,
                ForeColor = Tema.TextoSuave,
                Padding = new Padding(30),
                Visible = false
            };
            Controls.Add(lblError);

            Load += ManualForm_Load;
        }

        private async void ManualForm_Load(object sender, EventArgs e)
        {
            try
            {
                string rutaPdf = ExtraerManualATemporal();
                await visor.EnsureCoreWebView2Async(null);
                visor.CoreWebView2.Navigate(new Uri(rutaPdf).AbsoluteUri);
            }
            catch (Exception ex)
            {
                // Si el runtime de WebView2 (Edge) no esta instalado en este equipo,
                // o falla por cualquier otra razon, se ofrece abrir el manual con el
                // lector de PDF que tenga el sistema en vez de dejar la ventana en blanco.
                visor.Visible = false;
                lblError.Visible = true;
                lblError.Text =
                    "No se pudo mostrar el manual dentro del programa " +
                    "(se requiere el componente WebView2 de Microsoft Edge).\n\n" +
                    "Detalle: " + ex.Message + "\n\n" +
                    "Presiona el boton de abajo para abrirlo con el lector de PDF del sistema.";

                var btnAbrirExterno = Tema.CrearBotonPrimario("Abrir manual con el lector del sistema");
                btnAbrirExterno.Anchor = AnchorStyles.None;
                btnAbrirExterno.Size = new Size(320, 40);
                btnAbrirExterno.Location = new Point(
                    (lblError.Width - btnAbrirExterno.Width) / 2,
                    lblError.Height / 2 + 60);
                btnAbrirExterno.Click += (s, ev) =>
                {
                    try
                    {
                        string rutaPdf = ExtraerManualATemporal();
                        var psi = new System.Diagnostics.ProcessStartInfo(rutaPdf) { UseShellExecute = true };
                        System.Diagnostics.Process.Start(psi);
                    }
                    catch (Exception ex2)
                    {
                        MessageBox.Show("No se pudo abrir el manual: " + ex2.Message,
                            "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                };
                lblError.Controls.Add(btnAbrirExterno);
            }
        }

        // Copia el PDF embebido en el ensamblado a un archivo temporal y devuelve su ruta.
        // Si ya existe (de una apertura anterior en esta misma sesion de Windows), lo reutiliza.
        private static string ExtraerManualATemporal()
        {
            string destino = Path.Combine(Path.GetTempPath(), "AdministradorProcesos_ManualDeUsuario.pdf");

            var asamblea = Assembly.GetExecutingAssembly();
            using (var recurso = asamblea.GetManifestResourceStream(RecursoManual))
            {
                if (recurso == null)
                    throw new InvalidOperationException("El recurso incrustado del manual no se encontro: " + RecursoManual);

                using (var archivo = new FileStream(destino, FileMode.Create, FileAccess.Write))
                {
                    recurso.CopyTo(archivo);
                }
            }

            return destino;
        }
    }
}