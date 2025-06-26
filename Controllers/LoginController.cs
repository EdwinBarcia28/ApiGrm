using ApiGrm.DTO.InscripcionNacimiento;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using Novell.Directory.Ldap;
using System.Security.Claims;
using System.Text;

namespace ApiGrm.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class LoginController : ControllerBase
    {
        private readonly IConfiguration _configuration;

        public LoginController(IConfiguration configuration)
        {
            _configuration = configuration;
        }


        [NonAction]
        public string GenerateTokenJwt(string username)
        {
            var secretKey = _configuration["Jwt:Key"] ?? "";
            var audienceToken = "https://localhost:7223";
            var issuerToken = "https://localhost:7223";
            var expireTime = 300;

            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));
            var signingCredentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256Signature);

            ClaimsIdentity claimsIdentity = new ClaimsIdentity(new[] { new Claim(ClaimTypes.Name, username) });

            var tokenHandler = new System.IdentityModel.Tokens.Jwt.JwtSecurityTokenHandler();
            var jwtSecurityToken = tokenHandler.CreateJwtSecurityToken(
                audience: audienceToken,
                issuer: issuerToken,
                subject: claimsIdentity,
                notBefore: DateTime.UtcNow,
                expires: DateTime.UtcNow.AddMinutes(Convert.ToInt32(expireTime)),
                signingCredentials: signingCredentials);

            var jwtTokenString = tokenHandler.WriteToken(jwtSecurityToken);
            return jwtTokenString;
        }


        [HttpPost]
        [Route("authenticate")]
        public async Task<IActionResult> Login([FromBody] LoginDTO request)
        {
            string ldapHost = "192.168.1.157";
            int ldapPort = 389;
            string dominio = "crcgnet";
            string baseDn = "DC=crcgnet,DC=int";

            try
            {
                using var connection = new LdapConnection();
                await connection.ConnectAsync(ldapHost, ldapPort);

                string userDn = $"{request.Username}@{dominio}";
                await connection.BindAsync(userDn, request.Password);

                string searchFilter = $"(&(objectClass=user)(sAMAccountName={request.Username}))";

                var searchResults = await connection.SearchAsync(
                    baseDn,
                    LdapConnection.ScopeSub,
                    searchFilter,
                    new[] {
                        "cn", "sAMAccountName", "userPrincipalName", "mail", "displayName", "givenName", "sn",
                        "telephoneNumber", "mobile", "department", "title", "company", "physicalDeliveryOfficeName",
                        "memberOf", "whenCreated", "lastLogonTimestamp", "userAccountControl", "distinguishedName", "manager"
                    },
                    false
                );

                try
                {
                    if (await searchResults.HasMoreAsync())
                    {
                        var entry = await searchResults.NextAsync();

                        var attributes = entry.GetAttributeSet();

                        string Nombre = "",
                               Email = "";

                        //string nombre = "", 
                        //       sAMAccountName = "",
                        //       userPrincipalName = "",
                        //       mail = "",
                        //       displayname = "",
                        //       givenName = "",
                        //       sn = "",
                        //       telephoneNumber = "",
                        //       mobile = "",
                        //       departament = "",
                        //       title = "",
                        //       company = "",
                        //       physicalDeliveryOfficeName = "",
                        //       memberOf = "",
                        //       whenCreated = "",
                        //       lastLogonTimestamp = "",
                        //       userAccountControl = "",
                        //       distinguishedName = "",
                        //       manager = "";

                        foreach (var attrObj in attributes)
                        {
                            var attr = (LdapAttribute)attrObj;

                            switch (attr.Name.ToLower())
                            {
                                case "cn": Nombre = attr.StringValue; break;
                                case "mail": Email = attr.StringValue; break;
                            }

                            //switch (attr.Name.ToLower())
                            //{
                            //    case "cn": nombre = attr.StringValue; break;
                            //    case "samaccountname": sAMAccountName = attr.StringValue; break;
                            //    case "userprincipalname": userPrincipalName = attr.StringValue; break;
                            //    case "mail": mail = attr.StringValue; break;
                            //    case "displayname": displayname = attr.StringValue; break;
                            //    case "givenname": givenName = attr.StringValue; break;
                            //    case "sn": sn = attr.StringValue; break;
                            //    case "telephonenumber": telephoneNumber = attr.StringValue; break;
                            //    case "mobile": mobile = attr.StringValue; break;
                            //    case "department": departament = attr.StringValue; break;
                            //    case "title": title = attr.StringValue; break;
                            //    case "company": company = attr.StringValue; break;
                            //    case "physicaldeliveryofficename": physicalDeliveryOfficeName = attr.StringValue; break;
                            //    case "memberof": memberOf = attr.StringValue; break;
                            //    case "whencreated": whenCreated = attr.StringValue; break;
                            //    case "lastlogontimestamp": lastLogonTimestamp = attr.StringValue; break;
                            //    case "useraccountcontrol": userAccountControl = attr.StringValue; break;
                            //    case "distinguishedname": distinguishedName = attr.StringValue; break;
                            //    case "manager": manager = attr.StringValue; break;
                            //}

                        }

                        var token = GenerateTokenJwt(request.Username);

                        return StatusCode(StatusCodes.Status200OK, new { error = 0, token, message = "¡Bienvenido! 😎", user = new { NombreUsuario = request.Username, Nombre, Email }});
                    }

                    return NotFound(new
                    {
                        codigo = 2,
                        mensaje = "Usuario autenticado pero no se encontró en el directorio"
                    });
                }
                catch (LdapReferralException refEx)
                {
                    return StatusCode(500, new
                    {
                        codigo = 3,
                        mensaje = "Referral recibido: el usuario podría estar en otro servidor de AD",
                        detalle = refEx.Message
                    });
                }
            }
            catch (LdapException ex)
            {
                return Unauthorized(new
                {
                    codigo = 1,
                    mensaje = "Usuario o contraseña inválidos",
                    detalle = ex.Message
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    codigo = 1,
                    mensaje = "Error interno",
                    detalle = ex.Message
                });
            }
        }


        //[HttpPost]
        //[Route("authenticate")]
        //public async Task<IActionResult> Login([FromBody] LoginDTO request)
        //{
        //    string ldapHost = "192.168.1.157";
        //    int ldapPort = 389;
        //    string dominio = "crcgnet";
        //    string baseDn = "DC=crcgnet,DC=int";

        //    try
        //    {
        //        using var connection = new LdapConnection();
        //        await connection.ConnectAsync(ldapHost, ldapPort);

        //        string userDn = $"{request.Username}@{dominio}";
        //        await connection.BindAsync(userDn, request.Password);

        //        //string searchFilter = $"(&(objectClass=user)(sAMAccountName={request.Username}))";

        //        string searchFilter = "(&(objectClass=user)(objectCategory=person))";

        //        var searchResults = await connection.SearchAsync(
        //            baseDn,
        //            LdapConnection.ScopeSub,
        //            searchFilter,
        //            new[] { "cn", "mail", "department", "title" },
        //            false
        //        );

        //        try
        //        {
        //            if (await searchResults.HasMoreAsync())
        //            {
        //                var entry = await searchResults.NextAsync();

        //                var attributes = entry.GetAttributeSet();

        //                string nombre = "", correo = "", departamento = "", cargo = "";

        //                foreach (var attrObj in attributes)
        //                {
        //                    var attr = (LdapAttribute)attrObj;
        //                    switch (attr.Name.ToLower())
        //                    {
        //                        case "cn": nombre = attr.StringValue; break;
        //                        case "mail": correo = attr.StringValue; break;
        //                        case "department": departamento = attr.StringValue; break;
        //                        case "title": cargo = attr.StringValue; break;
        //                    }
        //                }

        //                return Ok(new
        //                {
        //                    codigo = 0,
        //                    mensaje = "Login correcto",
        //                    usuario = request.Username,
        //                    nombre,
        //                    correo,
        //                    departamento,
        //                    cargo
        //                });
        //            }

        //            return NotFound(new
        //            {
        //                codigo = 2,
        //                mensaje = "Usuario autenticado pero no se encontró en el directorio"
        //            });
        //        }
        //        catch (LdapReferralException refEx)
        //        {
        //            return StatusCode(500, new
        //            {
        //                codigo = 3,
        //                mensaje = "Referral recibido: el usuario podría estar en otro servidor de AD",
        //                detalle = refEx.Message
        //            });
        //        }
        //    }
        //    catch (LdapException ex)
        //    {
        //        return Unauthorized(new
        //        {
        //            codigo = 1,
        //            mensaje = "Usuario o contraseña inválidos",
        //            detalle = ex.Message
        //        });
        //    }
        //    catch (Exception ex)
        //    {
        //        return StatusCode(500, new
        //        {
        //            codigo = 1,
        //            mensaje = "Error interno",
        //            detalle = ex.Message
        //        });
        //    }
        //}



    }
}
