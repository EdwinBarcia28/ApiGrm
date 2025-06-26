using ApiGrm.DTO.InscripcionNacimiento;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using System.Data;

namespace ApiGrm.Controllers
{
    [Route("api/[controller]")]
    //[AllowAnonymous]
    [ApiController]
    public class DeclaranteController : Controller
    {
        private readonly IConfiguration _configuration;

        public DeclaranteController(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        [Authorize(AuthenticationSchemes = "Bearer")]
        [HttpPost]
        [Route("info-declarante")]
        public async Task<IActionResult> GetDeclarantes([FromBody] FiltroDeclaranteDTO request)
        {
            try
            {
                List<CiudadanoDTO> ciudadanos = new List<CiudadanoDTO>();
                using (var conexion = new SqlConnection(_configuration["ConnectionStrings:conexionBaseWeb"]))
                {
                    await conexion.OpenAsync();

                    using (SqlCommand comd = new SqlCommand("[NewGrm].[ConsultaCiudadanoNacimiento]", conexion))
                    {
                        comd.CommandType = CommandType.StoredProcedure;
                        comd.Parameters.Add("@Opcion", SqlDbType.Int).Value = request.Opcion;
                        comd.Parameters.Add("@Filtro", SqlDbType.NVarChar).Value = request.Filtro;

                        using (SqlDataReader reader = await comd.ExecuteReaderAsync() ){
                            while (await reader.ReadAsync())
                            {
                                CiudadanoDTO ciudadano = new CiudadanoDTO
                                {
                                    Ciudadano = reader["Ciudadano"].ToString(),
                                    Identificacion = reader["Identificacion"].ToString(),
                                    Sexo = reader["Sexo"].ToString(),
                                    FechaNacimiento = reader["FechaNacimiento"].ToString(),
                                    Nacionalidad = reader["Nacionalidad"].ToString(),
                                    EstadoCivil = reader["EstadoCivil"].ToString()
                                };
                                
                                ciudadanos.Add(ciudadano);
                            }
                        }
                    }
                }

                if (ciudadanos.Count > 0)
                {
                    return StatusCode(StatusCodes.Status200OK, new { error = 0, ciudadanos, mensaje = $"Se Consulto los Conductores con Exito 🙃" });
                }
                else
                {
                    return StatusCode(StatusCodes.Status200OK, new { error = 1, mensaje = $"No se encontraron registros relacionados al criterio de busqueda seleccionado" });
                }

            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new { error = 1, mensaje = $"Error al Obtener los ciudadanos: {ex.Message}" });
            }
        }

    }
}
