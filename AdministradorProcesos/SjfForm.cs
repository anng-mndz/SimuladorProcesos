//Angoly Camila Araujo Mayen
//9959-24-17623

using System.Windows.Forms;

namespace AdministradorProcesos
{
    public class SjfForm : SimulacionFormBase
    {
        private readonly IPlanificador planificador = new SjfPlanificador();
        protected override IPlanificador Planificador => planificador;
        protected override bool UsaPrioridad => false;
        protected override bool UsaQuantum => false;

        public SjfForm()
        {
            InicializarUI("SJF — Shortest Job First", planificador.Descripcion);
        }

        protected override void CargarDatosDeEjemplo(DataGridView grid)
        {
            grid.Rows.Add("P1", 0, 6);
            grid.Rows.Add("P2", 1, 2);
            grid.Rows.Add("P3", 2, 4);
            grid.Rows.Add("P4", 3, 1);
        }
    }
}
