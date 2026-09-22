using System;
using System.Drawing;
using System.Windows.Forms;

namespace AdministradorProcesos
{
    // Ventana de inicio: reemplaza al antiguo administrador de procesos
    // de Windows. Desde aqui se accede a cada algoritmo en su propio
    // formulario.
    public class MenuPrincipalForm : Form
    {
        public MenuPrincipalForm()
        {
            Text = "Simulador de Planificacion de Procesos";
            Width = 720;
            Height = 560;
            StartPosition = FormStartPosition.CenterScreen;
            BackColor = Tema.Fondo;
            Font = Tema.FuenteNormal;
            MinimumSize = new Size(640, 520);

            ConfigurarControles();
        }

        private void ConfigurarControles()
        {
            int x = 40;
            int anchoContenido = 640;
            int y = 34;

            var lblTitulo = new Label
            {
                Text = "Simulador de Planificacion de Procesos",
                Font = Tema.FuenteTitulo,
                ForeColor = Tema.TextoFuerte,
                Location = new Point(x, y),
                AutoSize = true
            };
            Controls.Add(lblTitulo);
            y += 36;

            var lblSubtitulo = new Label
            {
                Text = "Proyecto de Sistemas Operativos — elige un algoritmo para ver la simulacion,\npaso a paso, con administracion en vivo de los procesos.",
                Font = Tema.FuenteSubtitulo,
                ForeColor = Tema.TextoSuave,
                Location = new Point(x, y),
                Size = new Size(anchoContenido, 44),
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right
            };
            Controls.Add(lblSubtitulo);
            y += 60;

            var sep = Tema.CrearSeparador(anchoContenido);
            sep.Location = new Point(x, y);
            Controls.Add(sep);
            y += 24;

            AgregarTarjetaAlgoritmo(x, ref y, anchoContenido,
                "FCFS", "First Come, First Served",
                "Los procesos corren en el orden en que llegan, sin interrupciones. El mas simple de todos.",
                () => new FcfsForm());

            AgregarTarjetaAlgoritmo(x, ref y, anchoContenido,
                "SJF", "Shortest Job First",
                "Entre los procesos ya llegados, corre primero el que tiene la rafaga de CPU mas corta.",
                () => new SjfForm());

            AgregarTarjetaAlgoritmo(x, ref y, anchoContenido,
                "Round Robin", "Por turnos con quantum",
                "Cada proceso corre por turnos de duracion fija. Si no termina, vuelve al final de la cola.",
                () => new RoundRobinForm());

            AgregarTarjetaAlgoritmo(x, ref y, anchoContenido,
                "Prioridad", "Planificacion por prioridad",
                "Entre los procesos ya llegados, corre primero el de mayor prioridad.",
                () => new PrioridadForm());

            y += 10;
            var btnSalir = Tema.CrearBotonSecundario("Salir");
            btnSalir.Bounds = new Rectangle(x, y, 120, 36);
            btnSalir.Anchor = AnchorStyles.Top | AnchorStyles.Left;
            btnSalir.Click += (s, e) => Close();
            Controls.Add(btnSalir);
        }

        private void AgregarTarjetaAlgoritmo(int x, ref int y, int ancho, string nombre, string subnombre, string descripcion, Func<Form> crearFormulario)
        {
            var tarjeta = new Panel
            {
                Location = new Point(x, y),
                Size = new Size(ancho, 78),
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right,
                BackColor = Tema.Panel,
                BorderStyle = BorderStyle.FixedSingle
            };

            var lblNombre = new Label
            {
                Text = nombre,
                Font = new Font("Segoe UI Semibold", 12f, FontStyle.Bold),
                ForeColor = Tema.TextoFuerte,
                Location = new Point(18, 12),
                AutoSize = true
            };
            var lblSub = new Label
            {
                Text = subnombre,
                Font = Tema.FuenteChica,
                ForeColor = Tema.Primario,
                Location = new Point(18, 36),
                AutoSize = true
            };
            var lblDesc = new Label
            {
                Text = descripcion,
                Font = Tema.FuenteChica,
                ForeColor = Tema.TextoSuave,
                Location = new Point(18, 54),
                Size = new Size(ancho - 190, 20),
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right
            };

            var btnAbrir = Tema.CrearBotonPrimario("Abrir simulacion →");
            btnAbrir.Bounds = new Rectangle(ancho - 190, 20, 170, 36);
            btnAbrir.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            btnAbrir.Click += (s, e) =>
            {
                using var form = crearFormulario();
                form.ShowDialog(this);
            };

            tarjeta.Controls.Add(lblNombre);
            tarjeta.Controls.Add(lblSub);
            tarjeta.Controls.Add(lblDesc);
            tarjeta.Controls.Add(btnAbrir);

            Controls.Add(tarjeta);
            y += 90;
        }
    }
}
