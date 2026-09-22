using System.Collections.Generic;

namespace AdministradorProcesos
{
    // Autor: Angel Méndez
    // Carnet: 9959-24-6845
    //
    // Este archivo define las clases de datos (modelos) usadas por la
    // simulación de planificación de procesos: el estado de un proceso,
    // el proceso simulado en sí, y los segmentos del diagrama de Gantt.

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

    // Representa un proceso dentro de la simulación, con sus datos de
    // entrada (llegada, ráfaga, prioridad) y los valores que se calculan
    // conforme avanza la simulación (restante, finalización, etc.).
    public class ProcesoSimulado
    {
        // Nombre identificador del proceso (ej. "P1").
        public string Nombre { get; set; }

        // Tiempo en el que el proceso llega a la cola de listos.
        public int Llegada { get; set; }

        // Tiempo total de CPU que necesita el proceso (ráfaga).
        public int Rafaga { get; set; }

        // Solo se usa en el algoritmo de Prioridad. Convencion: 1 = mas prioritario.
        public int Prioridad { get; set; } = 1;

        // Cuanto le falta por ejecutar. Se va reduciendo durante la simulacion
        // (en Round Robin baja por quantums; en el resto baja de una sola vez).
        public int RestanteRafaga { get; set; }

        // Tiempo en el que el proceso terminó su ejecución.
        public int Finalizacion { get; set; }

        // Indica si el proceso fue cancelado (terminado anticipadamente)
        // en vez de completarse normalmente.
        public bool Cancelado { get; set; } = false;

        // Tiempo de retorno: cuanto tiempo pasó desde que llegó hasta que finalizó.
        public int Retorno => Finalizacion - Llegada;

        // Tiempo de espera: parte del retorno en la que el proceso no estuvo
        // ejecutándose (retorno menos el tiempo real de ráfaga).
        public int Espera => Retorno - Rafaga;

        // Crea una copia independiente de este proceso, útil para simular
        // distintos algoritmos sin modificar los datos originales.
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

    // Representa un tramo (barra) del diagrama de Gantt: un proceso
    // ejecutando desde "Inicio" hasta "Fin", junto con quiénes estaban
    // esperando en ese momento.
    public class SegmentoGantt
    {
        // Nombre del proceso que ejecuta durante este segmento.
        public string Nombre { get; set; }

        // Tiempo en el que inicia este segmento de ejecución.
        public int Inicio { get; set; }

        // Tiempo en el que termina este segmento de ejecución.
        public int Fin { get; set; }

        // Nombres de los procesos que quedaron esperando en la cola
        // justo en el momento en que este segmento empieza a correr.
        public List<string> EnEspera { get; set; } = new List<string>();
    }
}