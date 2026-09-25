using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Telemetria_VIAJES_TaskAPI_NEW.Model
{
    public class Viajes_Model
    {
        public string? Numero_Json { get; set; }
        public int Economico_Model { get; set; }
        public string? Nombre_Model { get; set; }
        public string? Fecha_Inicial_Model { get; set; }
        public string? Hora_Inicial_Model { get; set; }
        public string? Punto_Inicial_Model { get; set; }
        public string? Fecha_Final_Model { get; set; }
        public string? Hora_Final_Model { get; set; }
        public string? Punto_Final_Model { get; set; }
        public string? Duracion_Model { get; set; }
        public string? Horas_Motor_Model { get; set; }
        public string? Duracion_Punto_Final_Model { get; set; }
        public string? Kilometraje_Model { get; set; }
        public string? Kilometraje_Inicial_Model { get; set; }
        public string? Kilometraje_Final_Model { get; set; }
        public string? Velocidad_Maxima_Model { get; set; }
        public string? Revo_Media_Motor_Model { get; set; }
        public string? Revo_Max_Motor_Model { get; set; }
        public string? Temp_Media_Model { get; set; }

        public string? Temp_Max_Model { get; set; }
        public string? Consumo_FLS_Model { get; set; }


        public string? Consumo_ralenti_ABSF { get; set; }
        public string? Tipo_Reporte_Model { get; set; }

    }
}
