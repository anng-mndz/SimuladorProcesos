using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Windows.Forms;

namespace AdministradorProcesos
{
    // Autor: Angel Méndez
    // Carnet: 9959-24-6845
    //
    // Este archivo contiene los controles personalizados (paneles) encargados
    // de dibujar la parte visual de la simulación: el diagrama de Gantt,
    // el panel de "quién está corriendo / quién espera" y el panel de
    // estados de cada proceso.

    // Dibuja el diagrama de Gantt a color, mostrando solo los segmentos
    // hasta "PasoVisible" (para el modo paso a paso).
    public class GanttPanel : Panel
    {
        [System.ComponentModel.Browsable(false)]
        [System.ComponentModel.DesignerSerializationVisibility(System.ComponentModel.DesignerSerializationVisibility.Hidden)]
        public List<SegmentoGantt> Segmentos { get; set; } = new List<SegmentoGantt>();

        [System.ComponentModel.Browsable(false)]
        [System.ComponentModel.DesignerSerializationVisibility(System.ComponentModel.DesignerSerializationVisibility.Hidden)]
        public int PasoVisible { get; set; } = 0;

        public GanttPanel()
        {
            DoubleBuffered = true;
            BackColor = Color.White;
        }

        // Se ejecuta cada vez que el panel necesita redibujarse.
        // Dibuja un rectángulo de color por cada segmento visible del Gantt,
        // resalta el último segmento dibujado y muestra los tiempos
        // de inicio/fin debajo de la barra.
        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            var g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;

            // Si aún no hay segmentos o no se ha avanzado ningún paso,
            // se muestra un mensaje de ayuda en vez del diagrama.
            if (Segmentos == null || Segmentos.Count == 0 || PasoVisible == 0)
            {
                using var fuenteVacio = new Font("Segoe UI", 9.5f, FontStyle.Italic);
                g.DrawString("Presiona \"Iniciar simulacion\" y luego \"Siguiente\" para ver el primer turno.",
                    fuenteVacio, Brushes.Gray, 12, 14);
                return;
            }

            // Calcula la escala (píxeles por unidad de tiempo) según el
            // tiempo total de la simulación y el ancho disponible del panel.
            int tiempoTotal = Segmentos.Max(s => s.Fin);
            int margen = 20;
            float anchoUtil = Width - margen * 2;
            float pxPorUnidad = anchoUtil / Math.Max(tiempoTotal, 1);
            int alturaBarra = 46;
            int y = 26;

            using var fuenteEtiqueta = new Font("Segoe UI Semibold", 9.5f, FontStyle.Bold);
            using var fuenteTiempo = new Font("Segoe UI", 8f);

            // Recorre solo los segmentos hasta PasoVisible (para el modo
            // paso a paso) y dibuja cada uno como una barra de color.
            for (int i = 0; i < PasoVisible && i < Segmentos.Count; i++)
            {
                var seg = Segmentos[i];
                float x = margen + seg.Inicio * pxPorUnidad;
                float ancho = (seg.Fin - seg.Inicio) * pxPorUnidad;
                var color = Tema.ColorParaProceso(seg.Nombre);
                bool esUltimo = (i == PasoVisible - 1);

                var rect = new RectangleF(x, y, Math.Max(ancho - 2, 1), alturaBarra);
                using (var brush = new SolidBrush(color))
                    g.FillRectangle(brush, rect);

                // Al último segmento dibujado se le agrega un borde
                // más grueso para resaltarlo como el turno actual.
                if (esUltimo)
                {
                    using var penResaltado = new Pen(Color.FromArgb(35, 40, 55), 3);
                    g.DrawRectangle(penResaltado, x, y, Math.Max(ancho - 2, 1), alturaBarra);
                }

                // Dibuja el nombre del proceso centrado dentro de la barra,
                // solo si hay espacio suficiente.
                var tamNombre = g.MeasureString(seg.Nombre, fuenteEtiqueta);
                if (ancho > tamNombre.Width)
                {
                    g.DrawString(seg.Nombre, fuenteEtiqueta, Brushes.White,
                        x + (ancho - tamNombre.Width) / 2, y + (alturaBarra - tamNombre.Height) / 2);
                }

                // Muestra el tiempo de inicio de cada barra, y el tiempo
                // de fin solo en la última barra dibujada.
                g.DrawString(seg.Inicio.ToString(), fuenteTiempo, Brushes.DimGray, x - 4, y + alturaBarra + 4);
                if (i == PasoVisible - 1)
                    g.DrawString(seg.Fin.ToString(), fuenteTiempo, Brushes.DimGray, x + ancho - 8, y + alturaBarra + 4);
            }

            // Línea base debajo de todas las barras del Gantt.
            using var penBase = new Pen(Tema.BordeGrid, 1);
            g.DrawLine(penBase, margen, y + alturaBarra + 20, Width - margen, y + alturaBarra + 20);
        }
    }

    // Dibuja, como tarjetas de color, quien esta CORRIENDO ahora
    // y quienes estan EN ESPERA en ese mismo instante de la simulacion.
    public class ColaPanel : Panel
    {
        [System.ComponentModel.Browsable(false)]
        [System.ComponentModel.DesignerSerializationVisibility(System.ComponentModel.DesignerSerializationVisibility.Hidden)]
        public string Ejecutando { get; set; }

        [System.ComponentModel.Browsable(false)]
        [System.ComponentModel.DesignerSerializationVisibility(System.ComponentModel.DesignerSerializationVisibility.Hidden)]
        public List<string> EnEspera { get; set; } = new List<string>();

        public ColaPanel()
        {
            DoubleBuffered = true;
            BackColor = Color.White;
        }

        // Dibuja la tarjeta del proceso que está corriendo actualmente,
        // seguida de una flecha y las tarjetas (más pequeñas y semitransparentes)
        // de los procesos que están en cola de espera.
        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            var g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;

            using var fuenteEtiqueta = new Font("Segoe UI Semibold", 9f, FontStyle.Bold);
            using var fuenteChico = new Font("Segoe UI", 7.5f);

            int x = 16;
            int y = 10;
            int alturaTarjeta = 46;

            // Si no hay proceso ejecutando ni procesos en espera,
            // se muestra un mensaje de ayuda.
            if (string.IsNullOrEmpty(Ejecutando) && (EnEspera == null || EnEspera.Count == 0))
            {
                using var fuenteVacio = new Font("Segoe UI", 9f, FontStyle.Italic);
                g.DrawString("Aqui veras quien esta corriendo y quien espera en cada turno.",
                    fuenteVacio, Brushes.Gray, x, y + 12);
                return;
            }

            // Tarjeta del proceso en ejecución (color sólido, con borde).
            if (!string.IsNullOrEmpty(Ejecutando))
            {
                var color = Tema.ColorParaProceso(Ejecutando);
                int ancho = 112;
                var rect = new Rectangle(x, y, ancho, alturaTarjeta);

                using (var brush = new SolidBrush(color))
                    g.FillRectangle(brush, rect);
                using (var pen = new Pen(Color.FromArgb(35, 40, 55), 2))
                    g.DrawRectangle(pen, rect);

                g.DrawString("CORRIENDO", fuenteChico, Brushes.White, x + 8, y + 6);
                g.DrawString(Ejecutando, fuenteEtiqueta, Brushes.White, x + 8, y + 20);

                x += ancho + 24;

                // Flecha y etiqueta "espera →" que separan la tarjeta
                // en ejecución de las tarjetas en espera.
                using var penFlecha = new Pen(Tema.TextoSuave, 2);
                g.DrawLine(penFlecha, x - 18, y + alturaTarjeta / 2, x - 6, y + alturaTarjeta / 2);
                g.DrawString("espera →", fuenteChico, Brushes.Gray, x - 18, y + alturaTarjeta / 2 + 8);
            }

            // Tarjetas de los procesos en espera (color más claro/semitransparente).
            if (EnEspera != null)
            {
                foreach (var nombre in EnEspera)
                {
                    var color = Tema.ColorParaProceso(nombre);
                    int ancho = 92;
                    var rect = new Rectangle(x, y + 4, ancho, alturaTarjeta - 8);

                    using (var brush = new SolidBrush(Color.FromArgb(60, color.R, color.G, color.B)))
                        g.FillRectangle(brush, rect);
                    using (var pen = new Pen(color, 2))
                        g.DrawRectangle(pen, rect);

                    var tam = g.MeasureString(nombre, fuenteEtiqueta);
                    g.DrawString(nombre, fuenteEtiqueta, new SolidBrush(Color.FromArgb(50, 55, 70)),
                        x + (ancho - tam.Width) / 2, y + (alturaTarjeta - 8 - tam.Height) / 2 + 4);

                    x += ancho + 10;
                }
            }
        }
    }

    // Panel de "administracion": muestra cada proceso como una tarjeta
    // (chip) coloreada segun su estado actual (Nuevo, Listo, Ejecutando,
    // Terminado, Cancelado). Es lo que reemplaza el simple diagrama de
    // Gantt como unica fuente de informacion.
    public class EstadosPanel : FlowLayoutPanel
    {
        public EstadosPanel()
        {
            AutoScroll = true;
            WrapContents = true;
            FlowDirection = FlowDirection.LeftToRight;
            BackColor = Color.White;
            Padding = new Padding(8);
        }

        // Reconstruye todas las tarjetas (chips) del panel a partir de la
        // lista de procesos y su estado actual. Si no hay procesos, muestra
        // un mensaje de ayuda en su lugar.
        public void Actualizar(IEnumerable<(string Nombre, EstadoProceso Estado)> items)
        {
            SuspendLayout();
            Controls.Clear();

            bool hayAlguno = false;
            foreach (var item in items)
            {
                hayAlguno = true;
                Controls.Add(CrearChip(item.Nombre, item.Estado));
            }

            if (!hayAlguno)
            {
                Controls.Add(new Label
                {
                    Text = "Agrega procesos e inicia la simulacion para ver sus estados aqui.",
                    Font = new Font("Segoe UI", 9f, FontStyle.Italic),
                    ForeColor = Color.Gray,
                    AutoSize = true,
                    Margin = new Padding(6, 14, 6, 6)
                });
            }

            ResumeLayout();
        }

        // Crea la tarjeta (Label estilizado) para un proceso individual,
        // usando el color y la etiqueta que le corresponden según su estado.
        private Label CrearChip(string nombre, EstadoProceso estado)
        {
            var (colorFondo, colorTexto, etiqueta) = EstiloEstado(estado);
            return new Label
            {
                Text = $"{nombre}\n{etiqueta}",
                AutoSize = true,
                TextAlign = ContentAlignment.MiddleCenter,
                BackColor = colorFondo,
                ForeColor = colorTexto,
                Font = new Font("Segoe UI Semibold", 9f, FontStyle.Bold),
                Padding = new Padding(12, 7, 12, 7),
                Margin = new Padding(6),
            };
        }

        // Devuelve el color de fondo, color de texto y etiqueta a mostrar
        // según el estado del proceso (Nuevo, Listo, Ejecutando, Terminado, Cancelado).
        private (Color fondo, Color texto, string etiqueta) EstiloEstado(EstadoProceso estado)
        {
            switch (estado)
            {
                case EstadoProceso.Nuevo:
                    return (Color.FromArgb(235, 237, 242), Tema.TextoSuave, "Nuevo");
                case EstadoProceso.Listo:
                    return (Color.FromArgb(255, 240, 205), Color.FromArgb(150, 105, 10), "Listo");
                case EstadoProceso.Ejecutando:
                    return (Tema.Exito, Color.White, "Ejecutando");
                case EstadoProceso.Terminado:
                    return (Color.FromArgb(222, 226, 233), Tema.TextoFuerte, "Terminado");
                case EstadoProceso.Cancelado:
                    return (Tema.Peligro, Color.White, "Cancelado");
                default:
                    return (Color.White, Color.Black, "");
            }
        }
    }
}