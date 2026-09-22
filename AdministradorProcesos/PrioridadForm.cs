using System.Windows.Forms;

namespace AdministradorProcesos
{
    public class PrioridadForm : SimulacionFormBase
    {
        private readonly IPlanificador planificador = new PrioridadPlanificador();
        protected override IPlanificador Planificador => planificador;
        protected override bool UsaPrioridad => true;
        protected override bool UsaQuantum => false;

        public PrioridadForm()
        {
            InicializarUI("Prioridad", planificador.Descripcion);
        }

        protected override void CargarDatosDeEjemplo(DataGridView grid)
        {
            grid.Rows.Add("P1", 0, 4, 3);
            grid.Rows.Add("P2", 1, 3, 1);
            grid.Rows.Add("P3", 2, 5, 2);
        }
    }
}
