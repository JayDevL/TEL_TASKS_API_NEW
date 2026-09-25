

using Microsoft.Data.SqlClient;
using System.Data;
using System.Globalization;
using System.Net;
using System.Numerics;
using System.Reflection.Metadata.Ecma335;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;
using Telemetria_VIAJES_TaskAPI_NEW.Model;
using static System.Runtime.InteropServices.JavaScript.JSType;
using Serilog;
using System.Runtime.InteropServices.JavaScript;

namespace Telemetria_VIAJES_TaskAPI_NEW
{
    internal class Program
    {

        static string urlApi = "https://hst-api.wialon.com/wialon/ajax.html?";
        static string TKN = "11e68802040b6eab269a470d3cb79ca04C10C149B979CC7F35AC644BCC5C938F1B126F1A";

        static string connectionString = "Server=localhost;Database=BICYR_TEL;Trusted_Connection=True;MultipleActiveResultSets=true;TrustServerCertificate=True";

        //public static string connectionString = "Server=10.15.27.18;Database=BICYR_TEL;User Id=User;Password=Xalapa2011;TrustServerCertificate=True";

        static string SID = "NO_SESSION";

        static string Workspace = "";

        static string TipoIntervalosFechas = "";

        static bool IntervaloFechasPersonalizado = true;

        static FromTo_Local_Date DateUnix_Local = new FromTo_Local_Date(1788242400, 1789970399);


        // --------------------- PERSONALIZACION DE FECHAS --------------------- //
        public static long[] getTimeYesterday()
        {
            // 1. Obtener la fecha de HOY en tiempo LOCAL (ej. 10 de septiembre)
            // Usamos .Today porque automáticamente pone la hora a las 00:00:00
            DateTime todayLocal = DateTime.Today;

            // 2. Obtener AYER local (ej. 9 de septiembre a las 00:00:00)
            DateTime yesterdayStart = todayLocal.AddDays(-1);

            // 3. Crear el final de ayer agregando 1 día y restando 1 segundo (23:59:59)
            DateTime yesterdayEnd = todayLocal.AddSeconds(-1);

            // 4. Convertir directamente a Timestamp Unix (esto respeta tu zona horaria para Wialon)
            long from = ((DateTimeOffset)yesterdayStart).ToUnixTimeSeconds();
            long to = ((DateTimeOffset)yesterdayEnd).ToUnixTimeSeconds();

            return new long[] { from, to };
        }

        public static DateTime ConvertUnixToDate(long Unix)
        {
            DateTime UnixEpoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

            DateTime Date_Convert = UnixEpoch.AddSeconds(Unix);

            return Date_Convert.ToLocalTime();

        }

        public class FromTo_Local_Date
        {
            public long From_Local { get; set; }
            public long To_Local { get; set; }

            public FromTo_Local_Date(long from, long to)
            {
                this.From_Local = from;
                this.To_Local = to;
            }

        }

        //---- INICIO DE PORGRAMA ------------------

        static async Task Main(string[] args)
        {

            string rutaBase = AppContext.BaseDirectory;

            string rutaLog = Path.Combine(rutaBase, "LOG", "LOG_VIAJES_&_EFUEL_.txt");

            Log.Logger = new LoggerConfiguration()
                .WriteTo.File(rutaLog, rollingInterval: RollingInterval.Day)
                .WriteTo.Console()
                .CreateLogger();

            DateTime From_Local = ConvertUnixToDate(DateUnix_Local.From_Local);
            DateTime To_Local = ConvertUnixToDate(DateUnix_Local.To_Local);

            DateTime[] datesForUser;

            if (IntervaloFechasPersonalizado)
            {
                TipoIntervalosFechas = "PERSONALIZADO";
            }
            else
            {
                TipoIntervalosFechas = "FUNCION - getTimeYesterday()";
            }

            if (connectionString.StartsWith("Server=localhost"))
            {
                Workspace = "LOCAL";

                if (IntervaloFechasPersonalizado)
                {
                    datesForUser = new DateTime[] { From_Local, To_Local };
                }
                else
                {
                    long[] TiempoProductivo = getTimeYesterday();

                    DateTime From_Productive_Date = ConvertUnixToDate(TiempoProductivo[0]);
                    DateTime To_Productive_Date = ConvertUnixToDate(TiempoProductivo[1]);

                    datesForUser = new DateTime[] { From_Productive_Date, To_Productive_Date };
                }

            }
            else
            {
                Workspace = "PRODUCTIVO";

                if (IntervaloFechasPersonalizado)
                {
                    datesForUser = new DateTime[] { From_Local, To_Local };
                }
                else
                {
                    long[] TiempoProductivo = getTimeYesterday();

                    DateTime From_Productive_Date = ConvertUnixToDate(TiempoProductivo[0]);
                    DateTime To_Productive_Date = ConvertUnixToDate(TiempoProductivo[1]);

                    datesForUser = new DateTime[] { From_Productive_Date, To_Productive_Date };
                }

            }

            Log.Information("------------------------------------------ API WIALON - NEW Versión 1.0 -------------------------------------\n");
            Log.Information($"Workspace: {Workspace}");
            Log.Information($"Tipo de intervalo de fechas: {TipoIntervalosFechas}");
            Log.Information($"Fecha de consultas: From: {datesForUser[0]} ---- To: {datesForUser[1]}");
            Log.Information("• TEMPLATES : VIAJES (Telemetria & eFuel)");
            Log.Information($"• FECHA DE EJECUCIÓN: {DateTime.Now}\n");
            //Inicio Consulta de datos a Wialon API


            Log.Information("-------------------------------------------------------------------------\n");

            Thread.Sleep(3500);

            /* ID : 15, NOMBRE: Viajes Telemetria Grupal 2 */
            /* ID : 11, NOMBRE: Viajes eFuel NUEVO */

            string[] ListNames = new string[] { "TEL - Viajes 2 Telemetria", "TEL - Viajes eFuel" };

            int[] ListTemplates = new int[] { 15, 11 };

            int[] ListGroups = new int[] { 28584368, 27725182 };


            for (int i = 0; i < ListTemplates.Length; i++)
            {
                if (i > 0)
                {
                    Log.Information("------------ INICIANDO NUEVO REPORTE ------------------\n");
                    Thread.Sleep(2500);
                }


                if (await API_LOGIN(ListNames[i]))
                {
                    Thread.Sleep(1500);

                    switch (ListTemplates[i])
                    {
                        case 15:

                            Log.Information("※ ===== VIAJES TELEMETRIA 2. ======== ※\n");

                            break;

                        case 11:
                            Log.Information("※ ===== VIAJES eFuel. ====== ※\n");

                            break;

                        default:
                            Log.Information("※ ==== REPORTE DESCONOCIDO. ======= ※\n");

                            break;
                    }

                    Thread.Sleep(1500);

                    if (await EXEC_REPORT(SID, ListTemplates[i], ListGroups[i], ListNames[i]))
                    {
                        bool Get_Status_Report_ciclo = true;

                        while (Get_Status_Report_ciclo)
                        {
                            int ReporteStatus = await STATUS_REPORT(SID, ListNames[i]);

                            switch (ReporteStatus)
                            {
                                case 1:


                                    Log.Information("• IN LINE\n");

                                    break;

                                case 2:


                                    Log.Information("• Working Server\n");

                                    break;

                                case 4:


                                    Log.Information("✓ REPORT READY\n");

                                    Get_Status_Report_ciclo = false;

                                    if (await APPLY_REPORT(SID))
                                    {

                                        List<Viajes_Model> Viajes_Structurados = await JsonAComodo(ListTemplates[i].ToString(), ListNames[i]);

                                        /*
                                        Console.WriteLine("\n VISTA DE  Viajes_Structurados \n");

                                        // DEBUGG //

                                        foreach (var item in Viajes_Structurados)
                                        {
                                            Console.WriteLine($"Numero: {item.Numero_Json}");
                                            Console.WriteLine($"Economico: {item.Economico_Model}");
                                            Console.WriteLine($"Nombre: {item.Nombre_Model}");
                                            Console.WriteLine($"Fecha Inicial: {item.Fecha_Inicial_Model}");
                                            Console.WriteLine($"Hora Inicial: {item.Hora_Inicial_Model}");
                                            Console.WriteLine($"Punto Inicial: {item.Punto_Inicial_Model}");
                                            Console.WriteLine($"Fecha Final: {item.Fecha_Final_Model}");
                                            Console.WriteLine($"Hora Final: {item.Hora_Final_Model}");
                                            Console.WriteLine($"Punto Final: {item.Punto_Final_Model}");
                                            Console.WriteLine($"Duración: {item.Duracion_Model}");
                                            Console.WriteLine($"Horas Motor: {item.Horas_Motor_Model}");
                                            Console.WriteLine($"Duración Punto Final: {item.Duracion_Punto_Final_Model}");
                                            Console.WriteLine($"Kilometraje: {item.Kilometraje_Model}");
                                            Console.WriteLine($"Kilometraje Inicial: {item.Kilometraje_Inicial_Model}");
                                            Console.WriteLine($"Kilometraje Final: {item.Kilometraje_Final_Model}");
                                            Console.WriteLine($"Velocidad Máxima: {item.Velocidad_Maxima_Model}");
                                            Console.WriteLine($"Revoluciones Media Motor: {item.Revo_Media_Motor_Model}");
                                            Console.WriteLine($"Revoluciones Máx Motor: {item.Revo_Max_Motor_Model}");
                                            Console.WriteLine($"Temperatura Máxima: {item.Temp_Media_Model}");
                                            Console.WriteLine($"Temperatura Máxima: {item.Temp_Max_Model}");
                                            Console.WriteLine($"Consumo FLS: {item.Consumo_FLS_Model}");
                                            Console.WriteLine($"Consumo_ralenti_ABSF: {item.Consumo_ralenti_ABSF}");
                                            Console.WriteLine($"Tipo Reporte: {item.Tipo_Reporte_Model}");
                                            Console.WriteLine("---------------------------------------------");

                                            //Console.ReadLine();

                                        }

                                        // END DEBUG //
                                         */

                                        Thread.Sleep(1500);

                                        if (Viajes_Structurados.Count > 0)
                                        {
                                            DataTable TablaInsertar = Creacion_Table(Viajes_Structurados, ListNames[i]);

                                            if (TablaInsertar.Rows.Count > 0)
                                            {
                                                if (InserIntoDB(TablaInsertar, ListNames[i]))
                                                {

                                                    Log.Information("✓ TAREA FINALIZADA CORRECTAMENTE\n");
                                                }
                                                else
                                                {
                                                    Log.Information("✕ TAREA FINALIZADA CON ERRORES\n");
                                                }
                                            }

                                            TablaInsertar.Reset();
                                        }
                                        else
                                        {

                                            Log.Information("● [EMPTY DATA] : 0 REGISTROS VALIDOS ENCONTRADOS || Viajes_Structurados\n");

                                            LOG_Viajes(ListNames[i], 0, "0 REGISTROS DE VIAJES ENCONTRADOS", false);

                                            Log.Information("✓ TAREA FINALIZADA CORRECTAMENTE\n");
                                        }
                                    }
                                    break;

                                case 8:


                                    Log.Error("• Reporte Cancelado\n");

                                    LOG_Viajes(ListNames[i], 0, $"Reporte Cancelado | Status:{ReporteStatus}", true, "STATUS REPORT");

                                    Get_Status_Report_ciclo = false;
                                    break;

                                case 16:


                                    Log.Error("• Reporte Invalido / Inexistente\n");

                                    LOG_Viajes(ListNames[i], 0, $"Reporte Invalido / Inexistente | Status:{ReporteStatus}", true, "STATUS REPORT");

                                    Get_Status_Report_ciclo = false;
                                    break;

                                case -1:


                                    Log.Error("• Error al generar reporte. Status -1 :: CONTACTA A DESARROLLO\n");

                                    LOG_Viajes(ListNames[i], 0, $"Error al generar reporte. Status -1", true, "STATUS REPORT");

                                    Get_Status_Report_ciclo = false;
                                    break;
                            }

                            Thread.Sleep(1500);
                        }
                    }
                    else
                    {


                        Log.Error("✕ EXEC REPORT ERROR");
                        Log.Error("✕ TAREA FINALIZADA CON ERRORES\n");
                    }


                }
                else
                {


                    Log.Error("✕ INICIO DE SESSIÓN NO OTORGADO");
                    Log.Error("✕ TAREA FINALIZADA CON ERRORES\n");
                }
            }


            Log.Information("------------ FIN DE EXTRACCIÓN DE DATOS A WIALON API  - VIAJES (Telemetria & eFuel) ------------\n");

            Thread.Sleep(4500);

            //Console.readline();

        }

        static async private Task<bool> API_LOGIN(string NameReport)
        {

            string? Response_Login_Body_string = "";

            if (SID != "NO_SESSION")
            {
                Log.Information("• SESSION ACTIVA\n");
                //Console.WriteLine($"• SID: {SID}");

                await Clean_Reports(SID);

                return true;
            }

            try
            {
                using (HttpClient client = new HttpClient())
                {
                    ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
                    client.Timeout = TimeSpan.FromMinutes(5);


                    Log.Information($"⟫ [LOGG IN]\n");

                    var svc_param = "token/login";

                    string jsonParams = JsonSerializer.Serialize(new { token = TKN });

                    var params_Text = new Dictionary<string, string>
                    {   {"svc", svc_param},
                        {"params", jsonParams }
                    };

                    //DEBUGG 
                    /*
                    var options = new JsonSerializerOptions
                    {
                        WriteIndented = true,
                        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
                    };

                    string Params_Text_Serializado = JsonSerializer.Serialize(params_Text, options);

                    Console.WriteLine("Params_Solicitud_Explicita: " + Params_Text_Serializado);
                    
                    Console.readline();

                     */

                    FormUrlEncodedContent Login_ContentParams = new FormUrlEncodedContent(params_Text);
                    HttpResponseMessage Login_Response = await client.PostAsync(urlApi, Login_ContentParams);

                    Login_Response.EnsureSuccessStatusCode();

                    //DEBUGG
                    Response_Login_Body_string = await Login_Response.Content.ReadAsStringAsync();
                    //

                    //Respuesta de la API DINMAICA
                    JsonNode Response_Login_Body_Dynamic = JsonNode.Parse(Response_Login_Body_string) ?? "SIN_RESPONSE_LOGIN_BODY";

                    //Validación de elemento eid dentro de la respuesta de la API
                    if (Response_Login_Body_Dynamic["eid"] != null)
                    {
                        SID = Response_Login_Body_Dynamic["eid"]?.ToString() ?? "NO_SESSION";
                    }

                    Log.Information($"✓ Loggeado con éxito\n");
                    //Console.WriteLine($"• SID: {SID}");

                    if (await Set_Locale(SID, NameReport))
                    {
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }

            }
            catch (Exception ex)
            {


                Log.Error($"• Error al loggearse en API\n");
                Log.Error($"Error: {ex.Message}\n");
                Log.Error($"response: {Response_Login_Body_string ?? "SIN RESPUESTA"}");

                LOG_Viajes(NameReport, 0, $"Error al loggearse en API: {ex.Message}", true, "LOG_ERROR");

                return false;
            }
        }

        //Asignación de localización de fechas para la consulta de datos a Wialon API
        static async private Task<bool> Set_Locale(string SID, string NameReport)
        {
            using (HttpClient client = new HttpClient())
            {
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
                client.Timeout = TimeSpan.FromMinutes(5);



                Log.Information($"⟫ [CONFIG TIME_ZONE]\n");

                var svc_param = "render/set_locale";

                string JsonParams = JsonSerializer.Serialize(
                new
                {
                    tzOffset = -21600,
                    language = "es",
                    flags = 0,
                    formatDate = "%25E-%25m-%25Y %25H:%25M:%25S"
                });

                var Params_Text = new Dictionary<string, string>
                {   {"svc", svc_param},
                    {"params", JsonParams },
                    {"sid", SID }
                };

                var options = new JsonSerializerOptions
                {
                    WriteIndented = true,
                    Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
                };

                /*
                string Params_Text_Serializado = JsonSerializer.Serialize(Params_Text, options);
                Console.WriteLine("Params_Solicitud_Explicita: " + Params_Text_Serializado);

                //Console.readline();
                 */

                FormUrlEncodedContent SetLocale_ContentParams = new FormUrlEncodedContent(Params_Text);
                HttpResponseMessage SetLocale_Response = await client.PostAsync(urlApi, SetLocale_ContentParams);

                SetLocale_Response.EnsureSuccessStatusCode();

                string Response_SetLocale_Body_string = await SetLocale_Response.Content.ReadAsStringAsync();

                JsonNode Response_SetLocale_Body_Dynamic = JsonNode.Parse(Response_SetLocale_Body_string) ?? "SIN_RESPONSE_SETLOCALE_BODY";

                if (Response_SetLocale_Body_Dynamic["error"] != null)
                {


                    Log.Error($"• Error al setear la localización de fechas en API\n");
                    Log.Error($"Error: {Response_SetLocale_Body_Dynamic["error"].ToString()}\n");
                    Log.Error($"response: {Response_SetLocale_Body_string ?? "SIN RESPUESTA"}\n");

                    LOG_Viajes(NameReport, 0, $"Exception al SETEAR TIME ZONE : {Response_SetLocale_Body_string}", true, "SET LOCALE");
                    return false;
                }
                else
                {

                    Log.Information($"✓ CONFIGURACIÓN DE ZONA HORARIA TERMINADA\n");
                    return true;
                }
            }
        }

        static async private Task<bool> EXEC_REPORT(string SID, int Report, int groupID, string NameReport)
        {
            try
            {
                using (HttpClient client = new HttpClient())
                {
                    ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;

                    client.Timeout = TimeSpan.FromMinutes(5);


                    Log.Information($"⟫ [GENERING REPORT]\n");

                    // Regresa el tiempo de un día antes en un arreglo tipo [desde,hasta] en segundos
                    long[] timeYesterday = getTimeYesterday();

                    var svc_param = "report/exec_report";

                    var params_Report = "";

                    if (IntervaloFechasPersonalizado)
                    {
                        params_Report = JsonSerializer.Serialize(new
                        {
                            reportResourceId = 25970206, // id de los recursos de los reportes de wialon
                            reportTemplateId = Report,
                            reportTemplate = 0,
                            reportObjectId = groupID,
                            reportObjectSecId = 0,
                            remoteExec = 1,
                            interval = new { flags = 16777216, from = DateUnix_Local.From_Local, to = DateUnix_Local.To_Local }
                        });

                    }
                    else
                    {
                        params_Report = JsonSerializer.Serialize(new
                        {
                            reportResourceId = 25970206, // id de los recursos de los reportes de wialon
                            reportTemplateId = Report,
                            reportTemplate = 0,
                            reportObjectId = groupID,
                            reportObjectSecId = 0,
                            remoteExec = 1,
                            interval = new { flags = 16777216, from = timeYesterday[0], to = timeYesterday[1] }
                        });
                    }


                    var Params_Final = new Dictionary<string, string>
                    {
                        {"svc", svc_param},
                        {"params", params_Report },
                        {"sid", SID }
                    };

                    //DEBUGG 
                    var options = new JsonSerializerOptions
                    {
                        WriteIndented = true,
                        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,

                    };

                    /*

                    string Params_Text_Serializado = JsonSerializer.Serialize(Params_Final, options);

                    Console.WriteLine("Params_Solicitud_Explicita: " + Params_Text_Serializado);
                    // DEBUGG //

                    //Console.readline();

                     */

                    FormUrlEncodedContent ExecReport_ContentParams = new FormUrlEncodedContent(Params_Final);
                    HttpResponseMessage ExecReport_Response = await client.PostAsync(urlApi, ExecReport_ContentParams);

                    ExecReport_Response.EnsureSuccessStatusCode();

                    string Response_ExecReport_Body_string = await ExecReport_Response.Content.ReadAsStringAsync();

                    JsonNode Response_ExecReport_Body_Dynamic = JsonNode.Parse(Response_ExecReport_Body_string) ?? "SIN_RESPONSE_EXECREPORT_BODY";

                    if (Response_ExecReport_Body_Dynamic["remoteExec"] != null)
                    {


                        Log.Information($"✓ REPORTE GENERADO EXITOSAMENTE\n");
                        //Console.WriteLine(Response_ExecReport_Body_string);
                        return true;
                    }
                    else
                    {
                        throw new Exception($"Error al ejecutar el reporte. Response: {Response_ExecReport_Body_string ?? "SIN RESPUESTA"} \n");
                    }
                }

            }
            catch (Exception ex)
            {

                Log.Error("! Error al ejecutar el reporte\n");
                Log.Error($"Error: {ex.Message}\n");

                LOG_Viajes(NameReport, 0, $"Error al ejecutar reporte: {ex.Message}", true, "EXEC_REPORT");

                return false;
            }
        }
        static async private Task<int> STATUS_REPORT(string SID, string ReportName)
        {
            try
            {
                using (HttpClient client = new HttpClient())
                {
                    ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
                    client.Timeout = TimeSpan.FromMinutes(5);


                    Log.Information($"⁐ [WAITING SERVER]\n");

                    var svc_param = "report/get_report_status";

                    var Params_Final = new Dictionary<string, string>
                    {
                        {"svc", svc_param},
                        {"params","{}"},
                        {"sid", SID }
                    };

                    FormUrlEncodedContent StatusReport_ContentParams = new FormUrlEncodedContent(Params_Final);
                    HttpResponseMessage StatusReport_Response = await client.PostAsync(urlApi, StatusReport_ContentParams);

                    StatusReport_Response.EnsureSuccessStatusCode();

                    string Response_StatusReport_Body_string = await StatusReport_Response.Content.ReadAsStringAsync();

                    JsonNode Response_StatusReport_Body_Dynamic = JsonNode.Parse(Response_StatusReport_Body_string);

                    string status_string = Response_StatusReport_Body_Dynamic["status"]?.ToString() ?? "0";

                    int StatusReport = 0;

                    if (!string.IsNullOrEmpty(status_string))
                    {

                        if (int.TryParse(status_string, out int parsedStatus))
                        {
                            StatusReport = parsedStatus;
                            //Console.WriteLine($"• REPORT STATUS: {StatusReport}");

                        }
                        else
                        {

                            Log.Error("! Error al convertir el estado de la respuesta del server\n");
                            StatusReport = -1;

                        }

                        return StatusReport;

                    }
                    else
                    {
                        throw new Exception(Response_StatusReport_Body_string);
                    }
                }

            }
            catch (Exception ex)
            {

                Log.Error("! Error al consultar el estado del reporte\n");
                Log.Error($"Error: {ex.Message}\n");

                LOG_Viajes(ReportName, 0, $"GET STATUS REPORT: {ex.Message}", true, "HTTP_STATUS_REPORT");

                return -1;
            }
        }
        static async private Task<bool> APPLY_REPORT(string SID)
        {
            try
            {
                using (HttpClient client = new HttpClient())
                {
                    SecurityProtocolType protocol = SecurityProtocolType.Tls12;
                    client.Timeout = TimeSpan.FromMinutes(5);


                    Log.Information($"⟫ [APPLY REPORT]\n");

                    var svc_param = "report/apply_report_result";

                    Dictionary<string, string> Params_Final = new Dictionary<string, string>
                    {
                        {"svc",svc_param},
                        {"params","{}"},
                        {"sid",SID}
                    };

                    FormUrlEncodedContent ApplyReport_ContentParams = new FormUrlEncodedContent(Params_Final);
                    HttpResponseMessage ApplyReport_Response = await client.PostAsync(urlApi, ApplyReport_ContentParams);

                    ApplyReport_Response.EnsureSuccessStatusCode();

                    string ApplyReport_Content = await ApplyReport_Response.Content.ReadAsStringAsync();

                    JsonNode ApplyReport_Dynamic = JsonNode.Parse(ApplyReport_Content) ?? "SIN CONTENT APPLY REPORT";

                    //Console.WriteLine($"Apply Report: {ApplyReport_Content}");

                    if (ApplyReport_Dynamic["error"] != null)
                    {
                        throw new Exception($"{ApplyReport_Content}");
                    }

                    Log.Information("✓ REPORT APPLIED\n");
                    return true;

                }


            }
            catch (Exception ex)
            {

                Log.Error("ERROR AL APLICAR REPORTE\n");
                Log.Error($"ERROR: {ex.Message}\n");

                return false;

            }
        }

        static async private Task<List<JsonArray>> EXTRACT_DATA(string SID, string ReportName)
        {

            // 1. Declaramos la lista Data_R que almacenará los arreglos 'c'
            List<JsonArray> Data_R = new List<JsonArray>();

            try
            {
                using (HttpClient client = new HttpClient())
                {
                    ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
                    client.Timeout = TimeSpan.FromMinutes(5);


                    Log.Information($"⟫ [GET DATA]\n");

                    var svc_param = "report/select_result_rows";

                    string Params_Report = JsonSerializer.Serialize(new
                    {
                        tableIndex = 0,
                        config = new
                        {
                            type = "range",
                            data = new
                            {
                                from = 0,
                                to = -1,
                                level = -1,
                                unitInfo = 1
                            }
                        }
                    });

                    var Params_Final = new Dictionary<string, string>
                    {
                        {"svc", svc_param },
                        {"params", Params_Report },
                        {"sid", SID }
                    };


                    //DEBUGG 
                    var options = new JsonSerializerOptions
                    {
                        WriteIndented = true,
                        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,

                    };

                    string Params_Text_Serializado = JsonSerializer.Serialize(Params_Final, options);

                    //Console.WriteLine("Params_Solicitud_Explicita: " + Params_Text_Serializado);
                    // DEBUGG //

                    //Console.readline();

                    FormUrlEncodedContent GetData_ContentParams = new FormUrlEncodedContent(Params_Final);

                    HttpResponseMessage Response_select_Result_Rows = await client.PostAsync(urlApi, GetData_ContentParams);

                    Response_select_Result_Rows.EnsureSuccessStatusCode();

                    string Response_GetData_Body_string = await Response_select_Result_Rows.Content.ReadAsStringAsync();

                    //Console.WriteLine($"Response Result_Rows: {Response_GetData_Body_string}");
                    ////Console.readline();

                    JsonNode Response_GetData_Body_Dynamic = JsonNode.Parse(Response_GetData_Body_string) ?? "SIN_RESPONSE_GETDATA_BODY";

                    //------------------------------- Asignación de DE DATOS en vector DESDE EL JSON ------------------------------------------------------

                    // 2. Comprobamos que el nodo principal sea un arreglo JSON válido
                    if (Response_GetData_Body_Dynamic is JsonArray rootArray)
                    {
                        // 3. Iteramos sobre cada viaje principal en la raíz
                        foreach (JsonNode viajePrincipal in rootArray)
                        {
                            // 4. Verificamos si existe la propiedad "r" y si es un arreglo
                            if (viajePrincipal?["r"] is JsonArray subViajesArray)
                            {
                                // 5. Iteramos sobre cada sub-viaje dentro de "r"
                                foreach (JsonNode subViaje in subViajesArray)
                                {
                                    // 6. Extraemos la propiedad "c" y la agregamos a nuestra lista Data_R
                                    if (subViaje?["c"] is JsonArray columnasDatos)
                                    {
                                        Data_R.Add(columnasDatos);
                                    }
                                    else
                                    {

                                        Log.Warning("El nodo 'c' no es un arreglo JSON válido o no existe en el sub-viaje.\n");
                                    }
                                }
                            }
                            else
                            {

                                Log.Warning("El nodo 'r' no es un arreglo JSON válido o no existe en el viaje principal.\n");
                            }
                        }
                    }
                    else
                    {
                        throw new Exception("El nodo principal no es un arreglo JSON válido.");
                    }

                    /*
                    //Console.readline();

                    //Console.WriteLine($"DATA_R: \n");

                    int item = 0;

                    foreach (var itemJson in Data_R)
                    {
                        Console.WriteLine($"{item} : {itemJson}\n");
                        item++;
                    }

                     */
                    //Console.readline();

                    Log.Information($"• {Data_R.Count} Registros extraidos\n");

                    return Data_R;
                }
            }
            catch (Exception ex)
            {

                Log.Error("Error al Extraer data de reporte\n");
                Log.Error($"ERROR: {ex.Message}\n");

                LOG_Viajes(ReportName, 0, $"Error Response: {ex.Message}", true, "HTTP_O_READ_JSON_DATA");

                return Data_R;
            }
        }

        static private void LOG_Viajes(string tipoReporte, int totalInsert, string Mensaje, bool error, string? error_Desc = "", string? Fecha_rango = "")
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {

                    conn.Open();

                    string Command = "INSERT INTO LOG_TASK_API(OPERACION,TIPO_REPORTE,FECHA,HORA,TABLA,TOTAL_INSERT,MENSAJE,ERROR,TIPO_ERROR,FECHA_RANGO)VALUES(@op,@TRep,@Fecha,@Hora,@TablaName,@TotalInsert,@Msj,@Error,@TError,@FechaRango);";

                    using (SqlCommand cmd = new SqlCommand(Command, conn))

                        try
                        {
                            cmd.CommandType = CommandType.Text;

                            cmd.Parameters.Add("@op", SqlDbType.VarChar, 100).Value = "INSERT";
                            cmd.Parameters.Add("@TRep", SqlDbType.VarChar, 100).Value = tipoReporte;
                            cmd.Parameters.Add("@Fecha", SqlDbType.Date).Value = DateTime.Now;
                            cmd.Parameters.Add("@Hora", SqlDbType.Time).Value = DateTime.Now.TimeOfDay;
                            cmd.Parameters.Add("@TablaName", SqlDbType.VarChar, 100).Value = "TEL_VIAJES_TELEMETRIA";
                            cmd.Parameters.Add("@TotalInsert", SqlDbType.Int).Value = totalInsert;
                            cmd.Parameters.Add("@Msj", SqlDbType.VarChar, int.MaxValue).Value = Mensaje;
                            cmd.Parameters.Add("@Error", SqlDbType.Bit).Value = error;
                            cmd.Parameters.Add("@TError", SqlDbType.VarChar, 80).Value = error_Desc;
                            cmd.Parameters.Add("@FechaRango", SqlDbType.VarChar, 60).Value = Fecha_rango;

                            cmd.ExecuteNonQuery();


                            Log.Information("[LOG DB INSERT SUCCESS]\n");

                        }
                        catch (Exception ex)
                        {

                            Log.Error($"LOG IN DB - Exception SqlCommand : {ex.Message}\n");
                        }
                }
            }
            catch (Exception ex)
            {

                Log.Error($"LOG IN DB - Exception SQL CONNECTION : {ex.Message}\n");
            }

        }

        static async private Task Clean_Reports(string SID)
        {

            Log.Information("⟫ [CLEAN REPORTS]\n");

            try
            {
                using (HttpClient client = new HttpClient())
                {
                    SecurityProtocolType protocol = SecurityProtocolType.Tls12;
                    client.Timeout = TimeSpan.FromMinutes(5);

                    var SVC_Param = "report/cleanup_result";

                    var Params_Fin = new Dictionary<string, string>
                    {
                        {"svc",SVC_Param},
                        {"params","{}"},
                        {"sid",SID}
                    };

                    FormUrlEncodedContent CleanReport_params = new FormUrlEncodedContent(Params_Fin);
                    HttpResponseMessage CleanReports_Response = await client.PostAsync(urlApi, CleanReport_params);

                    CleanReports_Response.EnsureSuccessStatusCode();

                    string Response_CleanResponse_Content_string = await CleanReports_Response.Content.ReadAsStringAsync();

                    JsonNode Response_CleanResponse_Content = JsonNode.Parse(Response_CleanResponse_Content_string) ?? "SIN_RESPUESTA_CLEANREPORT";

                    if (Response_CleanResponse_Content["error"] != null)
                    {
                        if (Int32.Parse(Response_CleanResponse_Content["error"]?.ToString() ?? "-1") == 0)
                        {
                            Log.Information("✓ Clean report Success\n");
                        }
                        else
                        {
                            throw new Exception(Response_CleanResponse_Content_string);
                        }
                    }
                    else
                    {
                        throw new Exception(Response_CleanResponse_Content_string);
                    }

                }


            }
            catch (Exception ex)
            {

                Log.Error($"Error al limpiar reportes: {ex.Message}\n");
            }
        }

        static private string EconomicoSalida(string Cadena)
        {
            try
            {
                string economico = Cadena.Split(' ')[0];

                //Console.WriteLine($"Conversion : {Cadena} :: {economico}");

                return economico;

            }
            catch (Exception ex)
            {

                Log.Error($"ERROR en EconomicoSalida: {ex.Message}\n");
                return "-9999";
            }
        }

        static private string NombreEconomicoSalida(string Cadena)
        {
            try
            {
                string[] economicoName = Cadena.Split(new char[] { ' ' }, 2); // [0]-248 [1]-TE-H ISUZU
                //Console.WriteLine($"Conversion : {Cadena} :: {economicoName[1]}");
                return economicoName[1];

            }
            catch (Exception ex)
            {

                Log.Error($"ERROR en EconomicoName: {ex.Message}\n");
                return "NO NAME";
            }
        }

        static private string[] FechasyHorasParts(string cadenaIn)
        {
            try
            {
                //Console.WriteLine($"CADENA ORIGINAL FechasyHorasParts : {cadenaIn}\n");

                string cadena = cadenaIn.Replace('.', '-');

                string Cadena_Part1 = cadena.Split(' ')[0];

                string[] CadenaIn2 = cadena.Split(new char[] { ' ' }, 2);

                if (CadenaIn2[1].Contains("am") || CadenaIn2[1].Contains("pm"))
                {
                    if (DateTime.TryParseExact(CadenaIn2[1], "hh:mm:ss tt", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime dt))
                    {
                        CadenaIn2[1] = dt.ToString("HH:mm:ss");
                    }
                }

                string[] Results = { Cadena_Part1, CadenaIn2[1] };

                //Console.WriteLine($"Conversion1 : {cadena} :: {Cadena_Part1}");
                //Console.WriteLine($"Conversion2 : {cadena} :: {CadenaIn2[1]}");

                return Results;
            }
            catch (Exception ex)
            {

                Log.Error($"FechasyHorasPart - Error al convertir ${cadenaIn} - exception: {ex.Message}\n");
                string[] Result_ex = { "F&H_Failed", "F&H_Failed" };
                return Result_ex;
            }
        }

        static async private Task<List<Viajes_Model>> JsonAComodo(string TipoReporte, string ReportName)
        {

            List<JsonArray> Reporte_Data = await EXTRACT_DATA(SID, ReportName);

            List<Viajes_Model> ListDataModel_Viajes = new List<Viajes_Model>();

            if (Reporte_Data.Count > 0)
            {
                try
                {

                    Log.Information($"⟫ [STRUCTURING DATA]\n");

                    //CASO DE data VACIO.  -- AÑADIR

                    foreach (JsonArray vector in Reporte_Data)
                    {
                        //AQUI PODRIAMOS VER COMO ESTA EL VECTOR COMPLETAMENTE

                        if (
                            (vector[2]?.ToString() == "-----" || vector[2]?.ToString() == string.Empty || vector[2]?.ToString() == "") ||
                            (vector[3]?.ToString() == "-----" || vector[3]?.ToString() == string.Empty || vector[3]?.ToString() == "") ||
                            (vector[4]?.ToString() == "-----" || vector[4]?.ToString() == string.Empty || vector[4]?.ToString() == "")
                            )
                        {
                            continue;
                        }
                        else
                        {

                            Viajes_Model Viajes_Row = new Viajes_Model();

                            Viajes_Row.Numero_Json = vector[0]?.ToString() ?? "N/N_IN_JSON";

                            Viajes_Row.Economico_Model = Int32.Parse(EconomicoSalida(vector[1]?.ToString() ?? "-9999"));

                            /*
                            if(Viajes_Row.Economico_Model == 540)
                            {
                                Console.ReadLine();
                            }
                             */

                            Viajes_Row.Nombre_Model = NombreEconomicoSalida(vector[1]?.ToString() ?? "NO_NAME");

                            if (vector[2] is JsonObject)
                            {
                                Viajes_Row.Fecha_Inicial_Model = FechasyHorasParts(vector[2]?["t"]?.ToString() ?? "0 0")[0];
                                Viajes_Row.Hora_Inicial_Model = FechasyHorasParts(vector[2]?["t"]?.ToString() ?? "0 0")[1];
                            }
                            else
                            {
                                Viajes_Row.Fecha_Inicial_Model = FechasyHorasParts(vector[2]?.ToString() ?? "0 0")[0];
                                Viajes_Row.Hora_Inicial_Model = FechasyHorasParts(vector[2]?.ToString() ?? "0 0")[1];
                            }


                            Viajes_Row.Punto_Inicial_Model = vector[3]?["t"]?.ToString() ?? "Punto_inicial_Dato";


                            if (vector[4] is JsonObject)
                            {
                                Viajes_Row.Fecha_Final_Model = FechasyHorasParts(vector[4]?["t"]?.ToString() ?? "FECHA &HORAS")[0];
                                Viajes_Row.Hora_Final_Model = FechasyHorasParts(vector[4]?["t"]?.ToString() ?? "FECHA &HORAS")[1];
                            }
                            else
                            {
                                Viajes_Row.Fecha_Final_Model = FechasyHorasParts(vector[4]?.ToString() ?? "FECHA &HORAS")[0];
                                Viajes_Row.Hora_Final_Model = FechasyHorasParts(vector[4]?.ToString() ?? "FECHA &HORAS")[1];
                            }


                            Viajes_Row.Punto_Final_Model = vector[5]?["t"]?.ToString() ?? "Punto_Final_Dato";

                            Viajes_Row.Duracion_Model = vector[6]?.ToString() ?? "Duracion_Motor";
                            Viajes_Row.Horas_Motor_Model = vector[7]?.ToString() ?? "Horas_Motor_Model";

                            Viajes_Row.Duracion_Punto_Final_Model = vector[8]?.ToString() ?? "Duracion_PuntoFinal";

                            Viajes_Row.Kilometraje_Model = vector[9]?.ToString().Split(' ')[0] ?? "Kilometraje";
                            Viajes_Row.Kilometraje_Inicial_Model = vector[10]?.ToString().Split(' ')[0] ?? "Kilometraje_Inicial";
                            Viajes_Row.Kilometraje_Final_Model = vector[11]?.ToString().Split(' ')[0] ?? "Kilometraje_Final";

                            //Vel_MAX
                            if (vector[12] is JsonObject)
                            {
                                Viajes_Row.Velocidad_Maxima_Model = vector[12]?["t"]?.ToString().Split(' ')[0] ?? "-1"; // caso {"t":"xx kms"}
                            }
                            else if (vector[12]?.ToString() == "-----" || string.IsNullOrEmpty(vector[13]?.ToString()))
                            {
                                Viajes_Row.Velocidad_Maxima_Model = "0"; // caso "------"
                            }
                            else
                            {
                                Viajes_Row.Velocidad_Maxima_Model = vector[12]?.ToString().Split(' ')[0] ?? "-1"; //caso "0 kms..."
                            }


                            //rev_media_motor
                            if (vector[13]?.ToString() == "-----" || string.IsNullOrEmpty(vector[13]?.ToString()))
                            {
                                Viajes_Row.Revo_Media_Motor_Model = "0";
                            }
                            else
                            {
                                Viajes_Row.Revo_Media_Motor_Model = vector[13]?.ToString().Split(' ')[0] ?? "0";
                            }

                            //rev_max_motor
                            if (vector[14]?.ToString() == "-----" || string.IsNullOrEmpty(vector[14]?.ToString()))
                            {
                                Viajes_Row.Revo_Max_Motor_Model = "0";
                            }
                            else
                            {
                                Viajes_Row.Revo_Max_Motor_Model = vector[14]?.ToString().Split(' ')[0] ?? "0";
                            }

                            //Temp_Media_Motor
                            if (vector[15]?.ToString() == "-----" || string.IsNullOrEmpty(vector[15]?.ToString()))
                            {
                                Viajes_Row.Temp_Media_Model = "0";
                            }
                            else
                            {
                                Viajes_Row.Temp_Media_Model = vector[15]?.ToString().Split(' ')[0] ?? "0";
                            }


                            //Temp_Max_Motor
                            if (vector[16]?.ToString() == "-----" || string.IsNullOrEmpty(vector[16]?.ToString()))
                            {
                                Viajes_Row.Temp_Max_Model = "0";
                            }
                            else
                            {
                                Viajes_Row.Temp_Max_Model = vector[16]?.ToString().Split(' ')[0] ?? "0";
                            }

                            Viajes_Row.Consumo_FLS_Model = vector[17]?.ToString().Split(' ')[0] ?? "Combustible_Cons";

                            Viajes_Row.Consumo_ralenti_ABSF = vector[18]?.ToString().Split(' ')[0] ?? "Consumido por AbsFCS";

                            Viajes_Row.Tipo_Reporte_Model = TipoReporte;

                            ListDataModel_Viajes.Add(Viajes_Row);

                            //Console.WriteLine($"numero: {Viajes_Row.Numero_Json}");
                            //Console.WriteLine($"economico: {Viajes_Row.Economico_Model}\n");

                        }
                    }


                    Log.Information("✓ STRUCTURE DATA SUCCESS\n");

                }
                catch (Exception ex)
                {

                    Log.Error($"Error al Estructurar datos | Revisa vectores: {ex.Message}\n");

                    LOG_Viajes(ReportName, 0, $"Error al Estructurar datos | Revisar vectores: {ex.Message}", true, "STRUCTURE_VECTORS_DATA");
                }
            }
            else
            {

                Log.Information("● [EMPTY DATA] : 0 REGISTROS VALIDOS ENCONTRADOS || EXTRACT_DATA\n");

                LOG_Viajes(ReportName, 0, "0 REGISTROS DE VIAJES ENCONTRADOS", false);
            }


            return ListDataModel_Viajes;
        }


        static private bool InserIntoDB(DataTable TableInsert, string NameReport)
        {

            Log.Information("⟫ [SQL INSERT]\n");

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                try
                {
                    conn.Open();


                    Log.Information("• Abriendo conexión\n");

                    using (SqlTransaction Trans = conn.BeginTransaction())
                    {
                        try
                        {
                            using (SqlBulkCopy BULKCOPY = new SqlBulkCopy(conn, SqlBulkCopyOptions.Default, Trans))
                            {
                                BULKCOPY.DestinationTableName = "TEL_VIAJES_TELEMETRIA";

                                Log.Information($"• Copiando datos en TABLA VIAJES TELEMETRIA\n");

                                BULKCOPY.WriteToServer(TableInsert);
                            }

                            Trans.Commit();


                            Log.Information($"• {TableInsert.Rows.Count} Datos insertados exitosamente ✓\n");


                            LOG_Viajes(NameReport, TableInsert.Rows.Count, $"Filas Insertadas: {TableInsert.Rows.Count}", false);

                            return true;

                        }
                        catch (Exception ex)
                        {

                            Log.Error($"SQL BULKCOPY EXCEPTION : {ex.ToString()}\n");

                            LOG_Viajes(NameReport, 0, $"Error al insertar datos: {ex.ToString()}", true, $"SQL BULKCOPY EXCEPTION");

                            Trans.Rollback();
                            return false;
                        }
                    }
                }
                catch (Exception ex)
                {

                    Log.Error($"SQL Connection EXCEPTION : {ex.ToString()}\n");

                    LOG_Viajes(NameReport, 0, $"SQL Connection EXCEPTION : {ex.ToString()}\n", true, "SQL Connection EXCEPTION");

                    return false;
                }
            }

        }


        static private DataTable Creacion_Table(List<Viajes_Model> Data_Structurada, string NameReport)
        {

            Log.Information("[CAREATING BULK-TABLE]\n");

            DataTable dt = new DataTable();

            try
            {
                dt.Columns.Add("ID", typeof(int));
                dt.Columns.Add("ECONOMICO", typeof(int));
                dt.Columns.Add("NOMBRE", typeof(string));
                dt.Columns.Add("FECHA_INICIAL", typeof(string));
                dt.Columns.Add("HORA_INICIAL", typeof(string));
                dt.Columns.Add("PUNTO_INICIAL", typeof(string));
                dt.Columns.Add("FECHA_FINAL", typeof(string));
                dt.Columns.Add("HORA_FINAL", typeof(string));
                dt.Columns.Add("PUNTO_FINAL", typeof(string));
                dt.Columns.Add("DURACION", typeof(string));
                dt.Columns.Add("HORAS_MOTOR", typeof(string));
                dt.Columns.Add("DURACION_PUNTO_FINAL", typeof(string));
                dt.Columns.Add("KILOMETRAJE", typeof(string));
                dt.Columns.Add("KILOMETRAJE_INICIAL", typeof(string));
                dt.Columns.Add("KILOMETRAJE_FINAL", typeof(string));
                dt.Columns.Add("VELOCIDAD_MAX", typeof(string));
                dt.Columns.Add("TIEMPO_ENTRE", typeof(string));
                dt.Columns.Add("REVO_MEDIA_MOTOR", typeof(string));
                dt.Columns.Add("REVO_MAX_MOTOR", typeof(string));
                dt.Columns.Add("TEMP_MEDIA", typeof(string));
                dt.Columns.Add("TEMP_MAX", typeof(string));
                dt.Columns.Add("CONSUMO_FLS", typeof(string));
                dt.Columns.Add("CONSUMO_RALENTI_ABSFCS", typeof(string));
                dt.Columns.Add("TIPO_REPORTE", typeof(string));


                foreach (var Viaje in Data_Structurada)
                {
                    dt.Rows.Add(
                        DBNull.Value,
                        Viaje.Economico_Model,
                        Viaje.Nombre_Model,
                        Viaje.Fecha_Inicial_Model,
                        Viaje.Hora_Inicial_Model,
                        Viaje.Punto_Inicial_Model,
                        Viaje.Fecha_Final_Model,
                        Viaje.Hora_Final_Model,
                        Viaje.Punto_Final_Model,
                        Viaje.Duracion_Model,
                        Viaje.Horas_Motor_Model,
                        Viaje.Duracion_Punto_Final_Model,
                        Viaje.Kilometraje_Model,
                        Viaje.Kilometraje_Inicial_Model,
                        Viaje.Kilometraje_Final_Model,
                        Viaje.Velocidad_Maxima_Model,
                        "0",
                        Viaje.Revo_Media_Motor_Model,
                        Viaje.Revo_Max_Motor_Model,
                        Viaje.Temp_Media_Model,
                        Viaje.Temp_Max_Model,
                        Viaje.Consumo_FLS_Model,
                        Viaje.Consumo_ralenti_ABSF,
                        Viaje.Tipo_Reporte_Model
                        );
                }


                Log.Information("• Table BulK Success ✓\n");

            }
            catch (Exception ex)
            {

                Log.Error($"Error al crear la tabla temporal: {ex.Message}\n");

                LOG_Viajes(NameReport, 0, $"Error New DATABLE: {ex.Message}", true, "BULK_TABLE");

            }

            return dt;
        }


    }
}

