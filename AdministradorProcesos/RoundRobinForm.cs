using System.Windows.Forms;

namespace AdministradorProcesos
{
    /// <summary>
    /// Autor: Angel Méndez
    /// Carnet: 9959-24-6845
    /// 
    /// Formulario que implementa la simulación del algoritmo de planificación
    /// Round Robin. Hereda de SimulacionFormBase y define el planificador
    /// específico (RoundRobinPlanificador), indicando que este algoritmo
    /// utiliza quantum de tiempo pero no maneja prioridades.
    /// </summary>
    public class RoundRobinForm : SimulacionFormBase
    {
        // Instancia del planificador Round Robin utilizado por este formulario
        private readonly IPlanificador planificador = new RoundRobinPlanificador();

        // Expone el planificador Round Robin a la clase base
        protected override IPlanificador Planificador => planificador;

        // Round Robin no utiliza prioridad de procesos
        protected override bool UsaPrioridad => false;

        // Round Robin sí utiliza quantum de tiempo
        protected override bool UsaQuantum => true;

        /// <summary>
        /// Constructor: inicializa la interfaz de usuario con el título
        /// "Round Robin" y la descripción del planificador.
        /// </summary>
        public RoundRobinForm()
        {
            InicializarUI("Round Robin", planificador.Descripcion);
        }

        /// <summary>
        /// Carga datos de ejemplo (procesos P1, P2, P3) en el grid,
        /// con su tiempo de llegada y ráfaga, para pruebas rápidas
        /// de la simulación.
        /// </summary>
        protected override void CargarDatosDeEjemplo(DataGridView grid)
        {
            grid.Rows.Add("P1", 0, 4);
            grid.Rows.Add("P2", 1, 3);
            grid.Rows.Add("P3", 2, 5);
        }
    }
}