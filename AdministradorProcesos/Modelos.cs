using System.Collections.Generic;

namespace AdministradorProcesos
{
    // Estados de un proceso dentro de la simulacion, inspirados en el
    // diagrama de estados clasico de Sistemas Operativos (simplificado):
    // Nuevo -> Listo -> Ejecutando -> Terminado, con Cancelado como
    // salida anticipada (equivalente a "matar" el proceso).
    public enum EstadoProceso
    {
        Nuevo,
        Listo,
        Ejecutando,
        Terminado,
        Cancelado
    }

    public class ProcesoSimulado
    {
        public string Nombre { get; set; }
        public int Llegada { get; set; }
        public int Rafaga { get; set; }

        // Solo se usa en el algoritmo de Prioridad. Convencion: 1 = mas prioritario.
        public int Prioridad { get; set; } = 1;

        // Cuanto le falta por ejecutar. Se va reduciendo durante la simulacion
        // (en Round Robin baja por quantums; en el resto baja de una sola vez).
        public int RestanteRafaga { get; set; }

        public int Finalizacion { get; set; }
        public bool Cancelado { get; set; } = false;

        public int Retorno => Finalizacion - Llegada;
        public int Espera => Retorno - Rafaga;

        public ProcesoSimulado Clonar()
        {
            return new ProcesoSimulado
            {
                Nombre = Nombre,
                Llegada = Llegada,
                Rafaga = Rafaga,
                Prioridad = Prioridad,
                RestanteRafaga = RestanteRafaga,
                Finalizacion = Finalizacion,
                Cancelado = Cancelado
            };
        }
    }

    public class SegmentoGantt
    {
        public string Nombre { get; set; }
        public int Inicio { get; set; }
        public int Fin { get; set; }

        // Nombres de los procesos que quedaron esperando en la cola
        // justo en el momento en que este segmento empieza a correr.
        public List<string> EnEspera { get; set; } = new List<string>();
    }
}
