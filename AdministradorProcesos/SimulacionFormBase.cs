using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace AdministradorProcesos
{
    // ============================================================
    // FORMULARIO BASE DE SIMULACION
    //
    // Contiene todo lo que comparten los 4 algoritmos:
    //   1) Tabla de procesos de entrada
    //   2) Diagrama de Gantt paso a paso (con reproduccion automatica)
    //   3) Panel de estados (Nuevo/Listo/Ejecutando/Terminado/Cancelado)
    //   4) Administracion en vivo: pausar/reanudar y cancelar un
    //      proceso individual a mitad de la simulacion
    //   5) Cola de "quien corre / quien espera"
    //   6) Resultados finales y promedios
    //
    // Cada algoritmo concreto (FcfsForm, SjfForm, PrioridadForm,
    // RoundRobinForm) solo define su nombre, su planificador y que
    // columnas extra necesita la tabla de entrada.
    // ============================================================
    public abstract class SimulacionFormBase : Form
    {
        protected abstract IPlanificador Planificador { get; }
        protected abstract bool UsaPrioridad { get; }
        protected abstract bool UsaQuantum { get; }
        protected abstract void CargarDatosDeEjemplo(DataGridView grid);

        // --- Controles ---
        private DataGridView gridEntrada;
        private NumericUpDown numQuantum;
        private Label lblQuantum;
        private Button btnAgregarFila;
        private Button btnIniciarSimulacion;
        private Button btnReiniciar;

        private GanttPanel panelGantt;
        private Button btnPasoAnterior;
        private Button btnPasoSiguiente;
        private Button btnAutoplay;
        private Label lblPasoActual;

        private EstadosPanel panelEstados;

        private ComboBox comboCancelar;
        private Button btnCancelar;
        private Label lblAdminInfo;

        private ColaPanel panelCola;

        private DataGridView gridResultado;
        private Label lblPromedios;

        private System.Windows.Forms.Timer timerAutoplay;

        // --- Estado de la simulacion ---
        private List<ProcesoSimulado> procesosActuales = new List<ProcesoSimulado>();
        private List<SegmentoGantt> segmentosActuales = new List<SegmentoGantt>();
        private int pasoActual = 0;
        private bool simulacionActiva = false;

        protected SimulacionFormBase()
        {
            Width = 920;
            Height = 900;
            StartPosition = FormStartPosition.CenterParent;
            BackColor = Tema.Fondo;
            Font = Tema.FuenteNormal;
            MinimumSize = new Size(860, 700);
            AutoScroll = true;
        }

        protected void InicializarUI(string titulo, string descripcionAlgoritmo)
        {
            Text = titulo;
            int x = 26;
            int anchoContenido = 820;
            int y = 20;

            var lblTitulo = new Label
            {
                Text = titulo,
                Font = Tema.FuenteTitulo,
                ForeColor = Tema.TextoFuerte,
                Location = new Point(x, y),
                AutoSize = true
            };
            Controls.Add(lblTitulo);
            y += 34;

            var lblDescripcion = new Label
            {
                Text = descripcionAlgoritmo,
                Font = Tema.FuenteSubtitulo,
                ForeColor = Tema.TextoSuave,
                Location = new Point(x, y),
                Size = new Size(anchoContenido, 40),
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right
            };
            Controls.Add(lblDescripcion);
            y += 46;

            var btnVolver = Tema.CrearBotonSecundario("← Volver al menu");
            btnVolver.Bounds = new Rectangle(x + anchoContenido - 150, 22, 150, 32);
            btnVolver.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            btnVolver.Click += (s, e) => Close();
            Controls.Add(btnVolver);

            // ---------- 1) Tabla de entrada ----------
            var lblEntrada = Tema.CrearEncabezadoSeccion("1)", "Procesos de entrada");
            lblEntrada.Location = new Point(x, y);
            Controls.Add(lblEntrada);
            y += 28;

            gridEntrada = new DataGridView
            {
                Location = new Point(x, y),
                Size = new Size(anchoContenido, 130),
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right,
                AllowUserToAddRows = true,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill
            };
            Tema.EstilizarGrid(gridEntrada);
            gridEntrada.Columns.Add("Nombre", "Nombre");
            gridEntrada.Columns.Add("Llegada", "Llegada");
            gridEntrada.Columns.Add("Rafaga", "Rafaga (CPU)");
            if (UsaPrioridad)
                gridEntrada.Columns.Add("Prioridad", "Prioridad (1 = mas alta)");
            Controls.Add(gridEntrada);
            y += 138;

            btnAgregarFila = Tema.CrearBotonSecundario("+ Agregar proceso");
            btnAgregarFila.Bounds = new Rectangle(x, y, 170, 34);
            btnAgregarFila.Anchor = AnchorStyles.Top | AnchorStyles.Left;
            btnAgregarFila.Click += (s, e) =>
            {
                if (UsaPrioridad) gridEntrada.Rows.Add("", 0, 1, 1);
                else gridEntrada.Rows.Add("", 0, 1);
            };
            Controls.Add(btnAgregarFila);

            if (UsaQuantum)
            {
                lblQuantum = new Label
                {
                    Text = "Quantum:",
                    Location = new Point(x + 190, y + 8),
                    AutoSize = true,
                    Anchor = AnchorStyles.Top | AnchorStyles.Left
                };
                Controls.Add(lblQuantum);

                numQuantum = new NumericUpDown
                {
                    Location = new Point(x + 260, y + 3),
                    Size = new Size(64, 28),
                    Minimum = 1,
                    Maximum = 100,
                    Value = 2,
                    Anchor = AnchorStyles.Top | AnchorStyles.Left
                };
                Controls.Add(numQuantum);
            }

            btnIniciarSimulacion = Tema.CrearBotonPrimario("▶  Iniciar simulacion");
            btnIniciarSimulacion.Bounds = new Rectangle(x + anchoContenido - 340, y, 200, 34);
            btnIniciarSimulacion.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            btnIniciarSimulacion.Click += BtnIniciarSimulacion_Click;
            Controls.Add(btnIniciarSimulacion);

            btnReiniciar = Tema.CrearBotonSecundario("↺  Reiniciar");
            btnReiniciar.Bounds = new Rectangle(x + anchoContenido - 130, y, 130, 34);
            btnReiniciar.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            btnReiniciar.Click += (s, e) => ReiniciarSimulacion();
            Controls.Add(btnReiniciar);

            y += 48;
            var sep1 = Tema.CrearSeparador(anchoContenido);
            sep1.Location = new Point(x, y);
            Controls.Add(sep1);
            y += 18;

            // ---------- 2) Diagrama de Gantt ----------
            var lblGantt = Tema.CrearEncabezadoSeccion("2)", "Linea de tiempo (Gantt)");
            lblGantt.Location = new Point(x, y);
            Controls.Add(lblGantt);
            y += 28;

            panelGantt = new GanttPanel
            {
                Location = new Point(x, y),
                Size = new Size(anchoContenido, 100),
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right,
                BorderStyle = BorderStyle.FixedSingle
            };
            Controls.Add(panelGantt);
            y += 110;

            btnPasoAnterior = Tema.CrearBotonSecundario("◀ Anterior");
            btnPasoAnterior.Bounds = new Rectangle(x, y, 120, 34);
            btnPasoAnterior.Enabled = false;
            btnPasoAnterior.Click += (s, e) => { DetenerAutoplay(); CambiarPaso(-1); };
            Controls.Add(btnPasoAnterior);

            btnPasoSiguiente = Tema.CrearBotonPrimario("Siguiente ▶");
            btnPasoSiguiente.Bounds = new Rectangle(x + 130, y, 120, 34);
            btnPasoSiguiente.Enabled = false;
            btnPasoSiguiente.Click += (s, e) => { DetenerAutoplay(); CambiarPaso(1); };
            Controls.Add(btnPasoSiguiente);

            btnAutoplay = Tema.CrearBotonExito("▶▶ Reproduccion automatica");
            btnAutoplay.Bounds = new Rectangle(x + 260, y, 220, 34);
            btnAutoplay.Enabled = false;
            btnAutoplay.Click += (s, e) => ToggleAutoplay();
            Controls.Add(btnAutoplay);

            lblPasoActual = new Label
            {
                Location = new Point(x, y + 40),
                Size = new Size(anchoContenido, 24),
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right,
                Text = "Presiona \"Iniciar simulacion\" para comenzar.",
                ForeColor = Tema.TextoSuave
            };
            Controls.Add(lblPasoActual);
            y += 74;

            var sep2 = Tema.CrearSeparador(anchoContenido);
            sep2.Location = new Point(x, y);
            Controls.Add(sep2);
            y += 18;

            // ---------- 3) Estados de los procesos ----------
            var lblEstados = Tema.CrearEncabezadoSeccion("3)", "Estado de cada proceso");
            lblEstados.Location = new Point(x, y);
            Controls.Add(lblEstados);
            y += 28;

            panelEstados = new EstadosPanel
            {
                Location = new Point(x, y),
                Size = new Size(anchoContenido, 100),
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right,
                BorderStyle = BorderStyle.FixedSingle
            };
            Controls.Add(panelEstados);
            y += 110;

            // ---------- 4) Administrar (cancelar un proceso en vivo) ----------
            var lblAdmin = Tema.CrearEncabezadoSeccion("4)", "Administrar procesos (control en vivo)");
            lblAdmin.Location = new Point(x, y);
            Controls.Add(lblAdmin);
            y += 28;

            comboCancelar = new ComboBox
            {
                Location = new Point(x, y),
                Size = new Size(260, 30),
                DropDownStyle = ComboBoxStyle.DropDownList,
                Enabled = false
            };
            Controls.Add(comboCancelar);

            btnCancelar = Tema.CrearBotonPeligro("Cancelar proceso seleccionado");
            btnCancelar.Bounds = new Rectangle(x + 270, y - 2, 230, 34);
            btnCancelar.Enabled = false;
            btnCancelar.Click += BtnCancelar_Click;
            Controls.Add(btnCancelar);

            lblAdminInfo = new Label
            {
                Location = new Point(x + 512, y + 3),
                Size = new Size(anchoContenido - 512, 26),
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right,
                Font = Tema.FuenteChica,
                ForeColor = Tema.TextoSuave,
                Text = "Puedes cancelar un proceso en cualquier momento, como si lo \"mataras\"."
            };
            Controls.Add(lblAdminInfo);
            y += 46;

            var sep3 = Tema.CrearSeparador(anchoContenido);
            sep3.Location = new Point(x, y);
            Controls.Add(sep3);
            y += 18;

            // ---------- 5) Cola ----------
            var lblCola = Tema.CrearEncabezadoSeccion("5)", "Quien corre ahora y quien espera");
            lblCola.Location = new Point(x, y);
            Controls.Add(lblCola);
            y += 28;

            panelCola = new ColaPanel
            {
                Location = new Point(x, y),
                Size = new Size(anchoContenido, 64),
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right,
                BorderStyle = BorderStyle.FixedSingle
            };
            Controls.Add(panelCola);
            y += 78;

            var sep4 = Tema.CrearSeparador(anchoContenido);
            sep4.Location = new Point(x, y);
            Controls.Add(sep4);
            y += 18;

            // ---------- 6) Resultados ----------
            var lblResultado = Tema.CrearEncabezadoSeccion("6)", "Resultados finales por proceso");
            lblResultado.Location = new Point(x, y);
            Controls.Add(lblResultado);
            y += 28;

            gridResultado = new DataGridView
            {
                Location = new Point(x, y),
                Size = new Size(anchoContenido, 130),
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right,
                ReadOnly = true,
                AllowUserToAddRows = false,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill
            };
            Tema.EstilizarGrid(gridResultado);
            gridResultado.Columns.Add("Nombre", "Nombre");
            gridResultado.Columns.Add("Estado", "Estado");
            gridResultado.Columns.Add("Finalizacion", "Finalizacion");
            gridResultado.Columns.Add("Retorno", "T. Retorno");
            gridResultado.Columns.Add("Espera", "T. Espera");
            Controls.Add(gridResultado);
            y += 140;

            lblPromedios = new Label
            {
                Location = new Point(x, y),
                Size = new Size(anchoContenido, 50),
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right,
                Text = "",
                Font = Tema.FuenteSeccion,
                ForeColor = Tema.TextoFuerte
            };
            Controls.Add(lblPromedios);
            y += 60;

            timerAutoplay = new System.Windows.Forms.Timer { Interval = 900 };
            timerAutoplay.Tick += (s, e) => CambiarPaso(1);

            CargarDatosDeEjemplo(gridEntrada);

            // Asegura que el formulario tenga espacio de scroll suficiente
            // para todas las secciones, sin verse amontonado.
            AutoScrollMinSize = new Size(0, y + 30);
        }

        // ================= Simulacion =================

        private void BtnIniciarSimulacion_Click(object sender, EventArgs e)
        {
            var procesos = new List<ProcesoSimulado>();

            foreach (DataGridViewRow row in gridEntrada.Rows)
            {
                if (row.IsNewRow) continue;
                if (row.Cells["Nombre"].Value == null || string.IsNullOrWhiteSpace(row.Cells["Nombre"].Value.ToString())) continue;

                try
                {
                    var proceso = new ProcesoSimulado
                    {
                        Nombre = row.Cells["Nombre"].Value.ToString().Trim(),
                        Llegada = Convert.ToInt32(row.Cells["Llegada"].Value),
                        Rafaga = Convert.ToInt32(row.Cells["Rafaga"].Value)
                    };
                    if (UsaPrioridad)
                        proceso.Prioridad = row.Cells["Prioridad"].Value != null ? Convert.ToInt32(row.Cells["Prioridad"].Value) : 1;

                    proceso.RestanteRafaga = proceso.Rafaga;
                    procesos.Add(proceso);
                }
                catch
                {
                    MessageBox.Show("Revisa que 'Llegada', 'Rafaga'" + (UsaPrioridad ? " y 'Prioridad'" : "") + " sean numeros validos en todas las filas.",
                        "Datos invalidos", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }
            }

            if (procesos.Count == 0)
            {
                MessageBox.Show("Agrega al menos un proceso.", "Falta informacion", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (procesos.Select(p => p.Nombre).Distinct().Count() != procesos.Count)
            {
                MessageBox.Show("No puede haber dos procesos con el mismo nombre.", "Nombres repetidos", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            procesosActuales = procesos;
            simulacionActiva = true;
            pasoActual = 0;
            RecalcularDesde(0);

            ActualizarVistaCompleta();

            btnPasoAnterior.Enabled = false;
            btnPasoSiguiente.Enabled = segmentosActuales.Count > 0;
            btnAutoplay.Enabled = segmentosActuales.Count > 0;
            comboCancelar.Enabled = true;
            btnCancelar.Enabled = true;

            lblPasoActual.Text = $"Listo. {segmentosActuales.Count} turno(s) en total — presiona \"Siguiente\" o la reproduccion automatica.";
        }

        private void ReiniciarSimulacion()
        {
            DetenerAutoplay();
            simulacionActiva = false;
            procesosActuales = new List<ProcesoSimulado>();
            segmentosActuales = new List<SegmentoGantt>();
            pasoActual = 0;

            panelGantt.Segmentos = segmentosActuales;
            panelGantt.PasoVisible = 0;
            panelGantt.Invalidate();

            panelCola.Ejecutando = null;
            panelCola.EnEspera = new List<string>();
            panelCola.Invalidate();

            panelEstados.Actualizar(Enumerable.Empty<(string, EstadoProceso)>());

            gridResultado.Rows.Clear();
            lblPromedios.Text = "";
            lblPasoActual.Text = "Presiona \"Iniciar simulacion\" para comenzar.";

            btnPasoAnterior.Enabled = false;
            btnPasoSiguiente.Enabled = false;
            btnAutoplay.Enabled = false;
            comboCancelar.Enabled = false;
            comboCancelar.Items.Clear();
            btnCancelar.Enabled = false;
        }

        // Vuelve a calcular los segmentos futuros a partir de "tiempoDesde",
        // conservando lo que ya se jugo (historial) sin tocarlo.
        private void RecalcularDesde(int tiempoDesde)
        {
            var historial = segmentosActuales.Take(pasoActual).ToList();

            // IMPORTANTE: Planificar() consume (deja en 0) el RestanteRafaga
            // de cada proceso porque calcula la simulacion completa de una
            // sola vez. Si no reconstruimos aqui cuanto le falta realmente a
            // cada proceso vivo (Rafaga total menos lo que ya se ejecuto en
            // el historial), el planificador cree que ya no queda nada
            // pendiente y devuelve una lista vacia de turnos futuros. Eso es
            // lo que provocaba que, al cancelar un proceso, el boton
            // "Siguiente" se deshabilitara aunque a otros procesos les
            // faltara correr.
            foreach (var p in procesosActuales)
            {
                if (p.Cancelado) continue;
                int ejecutado = historial.Where(s => s.Nombre == p.Nombre).Sum(s => s.Fin - s.Inicio);
                p.RestanteRafaga = p.Rafaga - ejecutado;
            }

            int quantum = UsaQuantum ? (int)numQuantum.Value : 1;

            var futuros = Planificador.Planificar(procesosActuales, tiempoDesde, quantum);
            segmentosActuales = historial.Concat(futuros).ToList();

            panelGantt.Segmentos = segmentosActuales;
            panelGantt.PasoVisible = pasoActual;
            panelGantt.Invalidate();
        }

        private void CambiarPaso(int direccion)
        {
            if (!simulacionActiva) return;

            pasoActual = Math.Max(0, Math.Min(segmentosActuales.Count, pasoActual + direccion));
            ActualizarVistaCompleta();
        }

        private void ActualizarVistaCompleta()
        {
            panelGantt.PasoVisible = pasoActual;
            panelGantt.Invalidate();

            btnPasoAnterior.Enabled = pasoActual > 0;
            btnPasoSiguiente.Enabled = pasoActual < segmentosActuales.Count;
            btnAutoplay.Enabled = pasoActual < segmentosActuales.Count;
            if (pasoActual >= segmentosActuales.Count) DetenerAutoplay();

            if (pasoActual == 0)
            {
                panelCola.Ejecutando = null;
                panelCola.EnEspera = new List<string>();
            }
            else
            {
                var seg = segmentosActuales[pasoActual - 1];
                string espera = seg.EnEspera.Count > 0 ? string.Join(", ", seg.EnEspera) : "nadie";
                lblPasoActual.Text = $"Turno {pasoActual} de {segmentosActuales.Count}:  corre {seg.Nombre} de t={seg.Inicio} a t={seg.Fin}.   En espera: {espera}.";

                panelCola.Ejecutando = seg.Nombre;
                panelCola.EnEspera = seg.EnEspera;
            }
            panelCola.Invalidate();

            if (pasoActual == 0)
                lblPasoActual.Text = segmentosActuales.Count > 0
                    ? $"Listo. {segmentosActuales.Count} turno(s) en total — presiona \"Siguiente\" para ver el primero."
                    : "Todos los procesos fueron cancelados antes de ejecutar ningun turno.";

            ActualizarPanelEstados();
            ActualizarComboCancelar();

            bool huboUltimoTurno = pasoActual == segmentosActuales.Count;
            bool quedanProcesosVivos = procesosActuales.Any(p => !p.Cancelado && CalcularEstado(p.Nombre) != EstadoProceso.Terminado);

            if (huboUltimoTurno && !quedanProcesosVivos)
                MostrarResultadosFinales();
            else
            {
                gridResultado.Rows.Clear();
                lblPromedios.Text = "";
            }
        }

        // Calcula el estado de un proceso mirando SOLO lo que ya se ha
        // mostrado hasta "pasoActual" (no usa informacion del futuro).
        private EstadoProceso CalcularEstado(string nombreProceso)
        {
            var proceso = procesosActuales.First(p => p.Nombre == nombreProceso);
            if (proceso.Cancelado) return EstadoProceso.Cancelado;

            int tiempoActual = pasoActual == 0 ? 0 : segmentosActuales[pasoActual - 1].Fin;
            if (proceso.Llegada > tiempoActual) return EstadoProceso.Nuevo;

            int ejecutadoAcumulado = segmentosActuales.Take(pasoActual)
                .Where(s => s.Nombre == nombreProceso)
                .Sum(s => s.Fin - s.Inicio);

            if (ejecutadoAcumulado >= proceso.Rafaga) return EstadoProceso.Terminado;

            if (pasoActual > 0 && segmentosActuales[pasoActual - 1].Nombre == nombreProceso)
                return EstadoProceso.Ejecutando;

            return EstadoProceso.Listo;
        }

        private void ActualizarPanelEstados()
        {
            var items = procesosActuales
                .OrderBy(p => p.Nombre)
                .Select(p => (p.Nombre, CalcularEstado(p.Nombre)));
            panelEstados.Actualizar(items);
        }

        private void ActualizarComboCancelar()
        {
            string seleccionPrevia = comboCancelar.SelectedItem as string;

            var cancelables = procesosActuales
                .Where(p => !p.Cancelado && CalcularEstado(p.Nombre) != EstadoProceso.Terminado)
                .Select(p => p.Nombre)
                .OrderBy(n => n)
                .ToList();

            comboCancelar.Items.Clear();
            foreach (var nombre in cancelables) comboCancelar.Items.Add(nombre);

            if (cancelables.Count == 0)
            {
                btnCancelar.Enabled = false;
            }
            else
            {
                btnCancelar.Enabled = simulacionActiva;
                int indice = seleccionPrevia != null ? cancelables.IndexOf(seleccionPrevia) : -1;
                comboCancelar.SelectedIndex = indice >= 0 ? indice : 0;
            }
        }

        private void BtnCancelar_Click(object sender, EventArgs e)
        {
            if (comboCancelar.SelectedItem == null) return;
            string nombre = comboCancelar.SelectedItem.ToString();
            var proceso = procesosActuales.First(p => p.Nombre == nombre);

            var confirmar = MessageBox.Show(
                $"¿Cancelar el proceso \"{nombre}\"? Se detendra en este punto de la simulacion y el resto de procesos se replanificara desde aqui.",
                "Confirmar cancelacion",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Warning);
            if (confirmar != DialogResult.Yes) return;

            DetenerAutoplay();

            int tiempoActual = pasoActual == 0 ? 0 : segmentosActuales[pasoActual - 1].Fin;
            proceso.Cancelado = true;
            proceso.RestanteRafaga = 0;
            proceso.Finalizacion = tiempoActual;

            RecalcularDesde(tiempoActual);
            ActualizarVistaCompleta();

            lblAdminInfo.Text = $"\"{nombre}\" fue cancelado en t={tiempoActual}. El resto se replanifico desde ese instante.";
        }

        private void ToggleAutoplay()
        {
            if (timerAutoplay.Enabled) DetenerAutoplay();
            else
            {
                if (pasoActual >= segmentosActuales.Count) return;
                timerAutoplay.Start();
                btnAutoplay.Text = "⏸  Pausar";
                btnAutoplay.BackColor = Tema.Advertencia;
            }
        }

        private void DetenerAutoplay()
        {
            timerAutoplay.Stop();
            btnAutoplay.Text = "▶▶ Reproduccion automatica";
            btnAutoplay.BackColor = Tema.Exito;
        }

        private void MostrarResultadosFinales()
        {
            gridResultado.Rows.Clear();
            foreach (var p in procesosActuales.OrderBy(p => p.Nombre))
            {
                if (p.Cancelado)
                    gridResultado.Rows.Add(p.Nombre, "Cancelado", $"t={p.Finalizacion}", "—", "—");
                else
                    gridResultado.Rows.Add(p.Nombre, "Terminado", p.Finalizacion, p.Retorno, p.Espera);
            }

            var terminados = procesosActuales.Where(p => !p.Cancelado).ToList();
            if (terminados.Count > 0)
            {
                double promedioEspera = terminados.Average(p => p.Espera);
                double promedioRetorno = terminados.Average(p => p.Retorno);
                string quantumTxt = UsaQuantum ? $"   |   Quantum: {(int)numQuantum.Value}" : "";
                lblPromedios.Text =
                    $"Simulacion completa  →  Tiempo promedio de espera: {promedioEspera:0.00}   |   " +
                    $"Tiempo promedio de retorno: {promedioRetorno:0.00}{quantumTxt}";
            }
            else
            {
                lblPromedios.Text = "Simulacion completa → todos los procesos fueron cancelados antes de terminar.";
            }
        }
    }
}
