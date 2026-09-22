using System;
using System.Collections.Generic;
using System.Linq;

namespace AdministradorProcesos
{
    // ============================================================
    // PLANIFICADORES
    //
    // Cada planificador recibe:
    //   - procesos: SOLO los procesos que aun les falta ejecutar
    //     (RestanteRafaga > 0 y no cancelados). Los ya terminados
    //     o cancelados se filtran antes de llamar aqui.
    //   - tiempoInicio: el reloj desde el que se debe planificar.
    //     Normalmente 0, pero si el usuario cancela un proceso a
    //     mitad de la simulacion, se vuelve a planificar el resto
    //     desde el instante en que ocurrio la cancelacion.
    //
    // Todos devuelven la lista de segmentos (diagrama de Gantt)
    // y dejan escrito Finalizacion en cada ProcesoSimulado que
    // alcanza a terminar dentro de ese calculo.
    // ============================================================

    public interface IPlanificador
    {
        string Nombre { get; }
        string Descripcion { get; }
        bool EsPreemptivo { get; }
        List<SegmentoGantt> Planificar(List<ProcesoSimulado> procesos, int tiempoInicio, int quantum);
    }

    // Base comun para los algoritmos NO preemptivos (FCFS, SJF, Prioridad):
    // una vez que un proceso empieza a correr, lo hace hasta terminar su
    // rafaga completa. Solo cambia el criterio con el que se elige el
    // siguiente proceso quisiera correr.
    internal static class PlanificadorNoPreemptivo
    {
        public static List<SegmentoGantt> Ejecutar(
            List<ProcesoSimulado> procesos,
            int tiempoInicio,
            Func<List<ProcesoSimulado>, ProcesoSimulado> elegirSiguiente)
        {
            var pendientes = procesos.Where(p => p.RestanteRafaga > 0 && !p.Cancelado).ToList();
            var gantt = new List<SegmentoGantt>();
            int tiempoActual = tiempoInicio;

            while (pendientes.Count > 0)
            {
                var candidatos = pendientes.Where(p => p.Llegada <= tiempoActual).ToList();

                if (candidatos.Count == 0)
                {
                    // Nadie ha llegado todavia: el CPU queda ocioso hasta la proxima llegada.
                    tiempoActual = pendientes.Min(p => p.Llegada);
                    candidatos = pendientes.Where(p => p.Llegada <= tiempoActual).ToList();
                }

                var elegido = elegirSiguiente(candidatos);
                var enEspera = candidatos.Where(p => p != elegido).Select(p => p.Nombre).ToList();

                int inicio = tiempoActual;
                int duracion = elegido.RestanteRafaga;
                tiempoActual += duracion;

                elegido.RestanteRafaga = 0;
                elegido.Finalizacion = tiempoActual;

                gantt.Add(new SegmentoGantt { Nombre = elegido.Nombre, Inicio = inicio, Fin = tiempoActual, EnEspera = enEspera });

                pendientes.Remove(elegido);
            }

            return gantt;
        }
    }

    // FCFS: First Come, First Served. El que llega primero, corre primero. Angoly Camila Araujo Mayen 9959-24-17623
    public class FcfsPlanificador : IPlanificador
    {
        public string Nombre => "FCFS";
        public string Descripcion => "First Come, First Served: los procesos corren en el orden en que llegan, sin interrupciones.";
        public bool EsPreemptivo => false;

        public List<SegmentoGantt> Planificar(List<ProcesoSimulado> procesos, int tiempoInicio, int quantum)
        {
            return PlanificadorNoPreemptivo.Ejecutar(procesos, tiempoInicio,
                candidatos => candidatos.OrderBy(p => p.Llegada).ThenBy(p => p.Nombre).First());
        }
    }

    // SJF no preemptivo: Shortest Job First. De los que ya llegaron, corre
    // primero el que tiene la rafaga de CPU mas corta. Angoly Camila Araujo Mayen 9959-24-17623
    public class SjfPlanificador : IPlanificador
    {
        public string Nombre => "SJF";
        public string Descripcion => "Shortest Job First (no preemptivo): entre los procesos ya llegados, corre primero el que tiene la rafaga de CPU mas corta.";
        public bool EsPreemptivo => false;

        public List<SegmentoGantt> Planificar(List<ProcesoSimulado> procesos, int tiempoInicio, int quantum)
        {
            return PlanificadorNoPreemptivo.Ejecutar(procesos, tiempoInicio,
                candidatos => candidatos.OrderBy(p => p.RestanteRafaga).ThenBy(p => p.Llegada).ThenBy(p => p.Nombre).First());
        }
    }

    // Prioridad no preemptiva. Convencion: numero mas bajo = mayor prioridad.
    public class PrioridadPlanificador : IPlanificador
    {
        public string Nombre => "Prioridad";
        public string Descripcion => "Entre los procesos ya llegados, corre primero el de mayor prioridad (numero mas bajo = mas prioritario).";
        public bool EsPreemptivo => false;

        public List<SegmentoGantt> Planificar(List<ProcesoSimulado> procesos, int tiempoInicio, int quantum)
        {
            return PlanificadorNoPreemptivo.Ejecutar(procesos, tiempoInicio,
                candidatos => candidatos.OrderBy(p => p.Prioridad).ThenBy(p => p.Llegada).ThenBy(p => p.Nombre).First());
        }
    }

    // Round Robin: preemptivo por quantum, con cola circular.
    public class RoundRobinPlanificador : IPlanificador
    {
        public string Nombre => "Round Robin";
        public string Descripcion => "Cada proceso corre por turnos de duracion fija (quantum). Si no termina, vuelve al final de la cola.";
        public bool EsPreemptivo => true;

        public List<SegmentoGantt> Planificar(List<ProcesoSimulado> procesos, int tiempoInicio, int quantum)
        {
            if (quantum < 1) quantum = 1;

            var pendientesTotal = procesos.Where(p => p.RestanteRafaga > 0 && !p.Cancelado).ToList();
            var cola = new Queue<ProcesoSimulado>();
            var pendientesPorLlegar = pendientesTotal.OrderBy(p => p.Llegada).ToList();
            var gantt = new List<SegmentoGantt>();

            int tiempoActual = tiempoInicio;
            var enCola = new HashSet<string>();

            void EncolarLlegadas()
            {
                foreach (var p in pendientesPorLlegar.ToList())
                {
                    if (p.Llegada <= tiempoActual && !enCola.Contains(p.Nombre))
                    {
                        cola.Enqueue(p);
                        enCola.Add(p.Nombre);
                        pendientesPorLlegar.Remove(p);
                    }
                }
            }

            EncolarLlegadas();
            if (cola.Count == 0 && pendientesPorLlegar.Count > 0)
            {
                tiempoActual = pendientesPorLlegar.Min(p => p.Llegada);
                EncolarLlegadas();
            }

            while (cola.Count > 0)
            {
                var actual = cola.Dequeue();
                enCola.Remove(actual.Nombre);

                var enEspera = cola.Select(p => p.Nombre).ToList();

                int ejecutado = Math.Min(quantum, actual.RestanteRafaga);
                int inicio = tiempoActual;
                tiempoActual += ejecutado;
                actual.RestanteRafaga -= ejecutado;

                gantt.Add(new SegmentoGantt { Nombre = actual.Nombre, Inicio = inicio, Fin = tiempoActual, EnEspera = enEspera });

                EncolarLlegadas();

                if (actual.RestanteRafaga > 0)
                {
                    cola.Enqueue(actual);
                    enCola.Add(actual.Nombre);
                }
                else
                {
                    actual.Finalizacion = tiempoActual;
                }

                if (cola.Count == 0 && pendientesPorLlegar.Count > 0)
                {
                    tiempoActual = pendientesPorLlegar.Min(p => p.Llegada);
                    EncolarLlegadas();
                }
            }

            return gantt;
        }
    }
}
