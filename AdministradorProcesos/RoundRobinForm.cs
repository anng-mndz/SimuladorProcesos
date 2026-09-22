using System.Windows.Forms;

namespace AdministradorProcesos
{
    public class RoundRobinForm : SimulacionFormBase
    {
        private readonly IPlanificador planificador = new RoundRobinPlanificador();
        protected override IPlanificador Planificador => planificador;
        protected override bool UsaPrioridad => false;
        protected override bool UsaQuantum => true;

        public RoundRobinForm()
        {
            InicializarUI("Round Robin", planificador.Descripcion);
        }

        protected override void CargarDatosDeEjemplo(DataGridView grid)
        {
            grid.Rows.Add("P1", 0, 4);
            grid.Rows.Add("P2", 1, 3);
            grid.Rows.Add("P3", 2, 5);
        }
    }
}
