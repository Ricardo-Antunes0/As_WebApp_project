using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Data.SqlClient;
using System.Data;
using BCrypt.Net;
using OpenTelemetry.Metrics;
using Microsoft.Extensions.Configuration;
using AsProj1.Server.Models;
using Microsoft.Extensions.Logging;
using System.Diagnostics.Metrics;


namespace AsProj1.Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class LoginController : ControllerBase
    {

        private readonly IConfiguration _configuration;
        private readonly ILogger<LoginController> _logger;
        private readonly Counter<int> _loginAttemptsCounter;
        private readonly Meter meter = new Meter("LOGINNN");
  

        public LoginController(IConfiguration configuration, ILogger<LoginController> logger)
        {
            _configuration = configuration;
            _logger = logger;
            _loginAttemptsCounter = meter.CreateCounter<int>("LoginAttempts");

            System.Diagnostics.Debug.WriteLine("Contador de tentativas de login criado com sucesso.");

        }


        [HttpPost]
        public JsonResult Authenticate(Login credentials)
        {
            try
            {   
                // Verificar as credenciais no banco de dados
                var isAuthenticated = ValidateCredentials(credentials.Email, credentials.Password);

                // Incrementar o contador de tentativas de login
                _loginAttemptsCounter.Add(1);

                if (isAuthenticated.isValid)
                {
                    _logger.LogInformation("Login bem sucedido!!!!!!!!!!!!!!!!!!!");
                    return new JsonResult(new { Email = credentials.Email, UserType = isAuthenticated.UserType });
                }
                else
                {
                    _logger.LogInformation("Login mal sucedido!!!!!!!!!!!!");
                    return new JsonResult("Falha na autenticação. Verifique as suas credenciais.");
                }
            }
            catch (Exception ex)
            {
                return new JsonResult($"Erro durante a autenticação: {ex.Message}");
            }
        }
        private (bool isValid, string UserType) ValidateCredentials(string email, string password)
        {
            string query = @"SELECT PasswordHash, UserType FROM dbo.Users WHERE UserMail = @Email";
            string sqlDataSource = _configuration.GetConnectionString("AS1DB");

            using (SqlConnection mycon = new SqlConnection(sqlDataSource))
            {
                mycon.Open();

                using (SqlCommand command = new SqlCommand(query, mycon))
                {
                    command.Parameters.AddWithValue("@Email", email);

                   
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            var hashedPassword = reader["PasswordHash"].ToString();
                            var userType = reader["UserType"].ToString();

                            // Verifica se o e-mail existe e se a senha fornecida corresponde ao hash no banco de dados
                            if (hashedPassword != null && BCrypt.Net.BCrypt.Verify(password, hashedPassword))
                            {
                                return (true, userType); // Credenciais válidas
                            }
                            else
                            {
                                return (false, userType);
                            }
                        }
                    }
                }
                return (false, null);
            }
        }
    }
}