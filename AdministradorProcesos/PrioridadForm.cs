//Karina Alejandra Arriaza Ortiz
//9959-24-14190
using System.Windows.Forms;

namespace AdministradorProcesos
{

    /// Formulario que implementa la simulación del algoritmo de planificación
    /// por Prioridad. Hereda la lógica común de <see cref="SimulacionFormBase"/>
    /// y solo define el planificador específico y los datos de ejemplo.

    public class PrioridadForm : SimulacionFormBase
    {

        /// Instancia del planificador que aplica el algoritmo de Prioridad.
        /// Se crea una única vez para todo el formulario.

        private readonly IPlanificador planificador = new PrioridadPlanificador();

        /// Expone el planificador de Prioridad a la clase base,
        /// que lo usa para ejecutar la simulación.

        protected override IPlanificador Planificador => planificador;

        /// Indica a la clase base que este algoritmo sí utiliza
        /// el campo de Prioridad, por lo que debe mostrarse en la grilla.

        protected override bool UsaPrioridad => true;

        /// Indica a la clase base que este algoritmo NO utiliza Quantum
        /// (el Quantum es propio de algoritmos como Round Robin),
        /// por lo que ese campo se oculta en la interfaz.

        protected override bool UsaQuantum => false;

        /// Constructor del formulario. Inicializa la interfaz gráfica
        /// mostrando el título "Prioridad" y la descripción propia
        /// del algoritmo, tomada del planificador.

        public PrioridadForm()
        {
            InicializarUI("Prioridad", planificador.Descripcion);
        }


        /// Carga procesos de ejemplo en la grilla al abrir el formulario,
        /// útil para que el usuario vea el simulador funcionando
        /// sin tener que cargar datos manualmente.

        protected override void CargarDatosDeEjemplo(DataGridView grid)
        {
            // Cada fila representa: Nombre del proceso, Tiempo de llegada, 
            // Duración (ráfaga de CPU), Prioridad (menor número = mayor prioridad, según convención habitual)
            grid.Rows.Add("P1", 0, 4, 3);
            grid.Rows.Add("P2", 1, 3, 1);
            grid.Rows.Add("P3", 2, 5, 2);
        }
    }
}