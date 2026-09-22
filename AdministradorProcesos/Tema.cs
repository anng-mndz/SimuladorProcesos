using System;
using System.Drawing;
using System.Windows.Forms;

namespace AdministradorProcesos
{
    // Autor: Angel Méndez
    // Carnet: 9959-24-6845
    //
    // ============================================================
    // TEMA VISUAL
    // Paleta y helpers de estilo compartidos por todos los
    // formularios, para que la app se vea consistente.
    // ============================================================
    public static class Tema
    {
        // Colores base usados en toda la interfaz (fondo, paneles, botones, textos, bordes).
        public static readonly Color Fondo = Color.FromArgb(244, 246, 250);
        public static readonly Color Panel = Color.White;
        public static readonly Color Primario = Color.FromArgb(45, 108, 223);
        public static readonly Color PrimarioOscuro = Color.FromArgb(30, 80, 180);
        public static readonly Color Peligro = Color.FromArgb(214, 69, 65);
        public static readonly Color PeligroOscuro = Color.FromArgb(180, 50, 47);
        public static readonly Color Exito = Color.FromArgb(45, 160, 110);
        public static readonly Color ExitoOscuro = Color.FromArgb(30, 130, 90);
        public static readonly Color Advertencia = Color.FromArgb(230, 160, 20);
        public static readonly Color TextoSuave = Color.FromArgb(95, 101, 115);
        public static readonly Color TextoFuerte = Color.FromArgb(32, 38, 52);
        public static readonly Color BordeGrid = Color.FromArgb(226, 230, 237);
        public static readonly Color BordeSeccion = Color.FromArgb(230, 233, 239);

        // Fuentes base usadas en títulos, subtítulos, secciones, texto normal y botones.
        public static readonly Font FuenteTitulo = new Font("Segoe UI Semibold", 17f, FontStyle.Bold);
        public static readonly Font FuenteSubtitulo = new Font("Segoe UI", 10.5f);
        public static readonly Font FuenteSeccion = new Font("Segoe UI Semibold", 11f, FontStyle.Bold);
        public static readonly Font FuenteNormal = new Font("Segoe UI", 9.5f);
        public static readonly Font FuenteChica = new Font("Segoe UI", 8.5f);
        public static readonly Font FuenteBoton = new Font("Segoe UI Semibold", 9.5f, FontStyle.Bold);

        // Paleta fija para diferenciar procesos en Gantt, cola y chips de estado.
        private static readonly Color[] PaletaProcesos = new[]
        {
            Color.FromArgb(66, 133, 244),  // azul
            Color.FromArgb(219, 68, 55),   // rojo
            Color.FromArgb(244, 160, 0),   // naranja
            Color.FromArgb(15, 157, 88),   // verde
            Color.FromArgb(171, 71, 188),  // morado
            Color.FromArgb(0, 172, 193),   // cian
            Color.FromArgb(255, 112, 67),  // coral
            Color.FromArgb(124, 179, 66),  // lima
        };

        // Asigna siempre el mismo color a un mismo nombre de proceso,
        // usando el hash del nombre para elegir un índice fijo dentro
        // de la paleta (así el color de "P1" no cambia entre repintados).
        public static Color ColorParaProceso(string nombre)
        {
            if (string.IsNullOrEmpty(nombre)) return PaletaProcesos[0];
            int indice = Math.Abs(nombre.GetHashCode()) % PaletaProcesos.Length;
            return PaletaProcesos[indice];
        }

        // Crea un botón plano con el estilo base del tema (colores, cursor,
        // borde opcional y efecto hover), reutilizado por los demás
        // métodos CrearBoton* para no repetir código.
        public static Button CrearBoton(string texto, Color fondo, Color texto2, Color? borde = null)
        {
            var b = new Button
            {
                Text = texto,
                Font = FuenteBoton,
                BackColor = fondo,
                ForeColor = texto2,
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand,
                UseVisualStyleBackColor = false
            };
            b.FlatAppearance.BorderSize = borde.HasValue ? 1 : 0;
            if (borde.HasValue) b.FlatAppearance.BorderColor = borde.Value;
            b.FlatAppearance.MouseOverBackColor = ControlPaint.Dark(fondo, 0.06f);
            return b;
        }

        // Variantes rápidas de CrearBoton para los estilos más usados:
        // primario (azul), éxito (verde), peligro (rojo) y secundario (borde).
        public static Button CrearBotonPrimario(string texto) => CrearBoton(texto, Primario, Color.White);
        public static Button CrearBotonExito(string texto) => CrearBoton(texto, Exito, Color.White);
        public static Button CrearBotonPeligro(string texto) => CrearBoton(texto, Peligro, Color.White);
        public static Button CrearBotonSecundario(string texto)
        {
            var b = CrearBoton(texto, Color.White, Primario, Primario);
            b.FlatAppearance.MouseOverBackColor = Color.FromArgb(235, 241, 253);
            return b;
        }

        // Crea una etiqueta de encabezado de sección, con formato
        // "numero   texto" (ej. "1   Configuración").
        public static Label CrearEncabezadoSeccion(string numero, string texto)
        {
            return new Label
            {
                Text = $"{numero}   {texto}",
                Font = FuenteSeccion,
                ForeColor = TextoFuerte,
                AutoSize = true,
                Anchor = AnchorStyles.Top | AnchorStyles.Left
            };
        }

        // Crea una línea delgada horizontal usada para separar secciones
        // visualmente dentro del formulario.
        public static Panel CrearSeparador(int ancho)
        {
            return new Panel
            {
                Height = 1,
                Width = ancho,
                BackColor = BordeSeccion,
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right
            };
        }

        // Aplica el estilo visual del tema (colores, bordes, encabezados,
        // fuentes, selección, filas alternadas) a un DataGridView existente,
        // para que todas las tablas de la app se vean iguales.
        public static void EstilizarGrid(DataGridView g)
        {
            g.BackgroundColor = Panel;
            g.BorderStyle = BorderStyle.None;
            g.CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal;
            g.GridColor = BordeGrid;
            g.RowHeadersVisible = false;
            g.EnableHeadersVisualStyles = false;
            g.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(237, 241, 248);
            g.ColumnHeadersDefaultCellStyle.ForeColor = Color.FromArgb(55, 62, 80);
            g.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI Semibold", 9.5f, FontStyle.Bold);
            g.ColumnHeadersHeight = 34;
            g.DefaultCellStyle.Font = FuenteNormal;
            g.DefaultCellStyle.SelectionBackColor = Color.FromArgb(210, 227, 252);
            g.DefaultCellStyle.SelectionForeColor = Color.Black;
            g.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(249, 250, 252);
            g.RowTemplate.Height = 30;
        }
    }
}
