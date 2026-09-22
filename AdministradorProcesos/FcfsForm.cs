using System.Windows.Forms;

namespace AdministradorProcesos
{
    public class FcfsForm : SimulacionFormBase
    {
        private readonly IPlanificador planificador = new FcfsPlanificador();
        protected override IPlanificador Planificador => planificador;
        protected override bool UsaPrioridad => false;
        protected override bool UsaQuantum => false;

        public FcfsForm()
        {
            InicializarUI("FCFS — First Come, First Served", planificador.Descripcion);
        }

        protected override void CargarDatosDeEjemplo(DataGridView grid)
        {
            grid.Rows.Add("P1", 0, 5);
            grid.Rows.Add("P2", 1, 3);
            grid.Rows.Add("P3", 2, 4);
        }
    }
}
