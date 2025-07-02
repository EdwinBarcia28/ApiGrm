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
                                    IdSexo = Convert.ToInt32(reader["IdSexo"].ToString()),
                                    Sexo = reader["Sexo"].ToString(),
                                    FechaNacimiento = reader["FechaNacimiento"].ToString(),
                                    IdNacionalidad = Convert.ToInt32(reader["IdNacionalidad"].ToString()),
                                    Nacionalidad = reader["Nacionalidad"].ToString(),
                                    IdEstado = Convert.ToInt32(reader["IdEstado"].ToString()),
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

        [Authorize(AuthenticationSchemes = "Bearer")]
        [HttpGet]
        [Route("sexo")]
        public async Task<IActionResult> GetSexo()
        {
            try
            {
                List<SexoDTO> sexos = new List<SexoDTO>();
                using (var conexion = new SqlConnection(_configuration["ConnectionStrings:conexionBaseGrm"]))
                {
                    await conexion.OpenAsync();

                    using (SqlCommand comd = new SqlCommand("[admin].[CargarSexo]", conexion))
                    {
                        comd.CommandType = CommandType.StoredProcedure;
                        using (SqlDataReader reader = await comd.ExecuteReaderAsync())
                        {
                            while (await reader.ReadAsync())
                            {
                                SexoDTO sexo = new SexoDTO
                                {
                                    Codigo = Convert.ToInt32(reader["Codigo"].ToString()),
                                    Descripcion = reader["Descripcion"].ToString()
                                };

                                sexos.Add(sexo);
                            }
                        }
                    }
                }

                if (sexos.Count > 0)
                {
                    return StatusCode(StatusCodes.Status200OK, new { error = 0, sexos, mensaje = $"Se Consulto los sexos con Exito 🙃" });
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

        [Authorize(AuthenticationSchemes = "Bearer")]
        [HttpGet]
        [Route("nacionalidad")]
        public async Task<IActionResult> GetNacionalidad()
        {
            try
            {
                List<NacionalidadDTO> nacionalidades = new List<NacionalidadDTO>();
                using (var conexion = new SqlConnection(_configuration["ConnectionStrings:conexionBaseGrm"]))
                {
                    await conexion.OpenAsync();

                    using (SqlCommand comd = new SqlCommand("[admin].[CargarNacionalidad]", conexion))
                    {
                        comd.CommandType = CommandType.StoredProcedure;
                        using (SqlDataReader reader = await comd.ExecuteReaderAsync())
                        {
                            while (await reader.ReadAsync())
                            {
                                NacionalidadDTO nacionalidad = new NacionalidadDTO
                                {
                                    Codigo = Convert.ToInt32(reader["Codigo"].ToString()),
                                    Nacionalidad = reader["Nacionalidad"].ToString()
                                };

                                nacionalidades.Add(nacionalidad);
                            }
                        }
                    }
                }

                if (nacionalidades.Count > 0)
                {
                    return StatusCode(StatusCodes.Status200OK, new { error = 0, nacionalidades, mensaje = $"Se Consulto las nacionalidades con Exito 🙃" });
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

        [Authorize(AuthenticationSchemes = "Bearer")]
        [HttpGet]
        [Route("estado-civil")]
        public async Task<IActionResult> GetEstadoCivil()
        {
            try
            {
                List<EstadoCivilDTO> estados = new List<EstadoCivilDTO>();
                using (var conexion = new SqlConnection(_configuration["ConnectionStrings:conexionBaseGrm"]))
                {
                    await conexion.OpenAsync();

                    using (SqlCommand comd = new SqlCommand("[admin].[CargarEstadoCivil]", conexion))
                    {
                        comd.CommandType = CommandType.StoredProcedure;
                        using (SqlDataReader reader = await comd.ExecuteReaderAsync())
                        {
                            while (await reader.ReadAsync())
                            {
                                EstadoCivilDTO estado = new EstadoCivilDTO
                                {
                                    Codigo = Convert.ToInt32(reader["Codigo"].ToString()),
                                    Descripcion = reader["Descripcion"].ToString()
                                };

                                estados.Add(estado);
                            }
                        }
                    }
                }

                if (estados.Count > 0)
                {
                    return StatusCode(StatusCodes.Status200OK, new { error = 0, estados , mensaje = $"Se Consulto las nacionalidades con Exito 🙃" });
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
