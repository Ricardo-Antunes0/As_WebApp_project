using AsProj1.Server.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using OpenTelemetry;
using OpenTelemetry.Trace;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;

namespace AsProj1.Server.Controllers
{
    public class AccessCodeRequest
    {
        public string UserType { get; set; }
        public string Code { get; set; }
        public int ClientId { get; set; }
    }

    public class ProfileUpdateRequest
{
    public string UserEmail { get; set; }
    public ClientData EditedData { get; set; }
    public Boolean KnowCode {  get; set; }
}


    [Route("api/[controller]")]
    [ApiController]
    public class CheckAccessCodeController : ControllerBase
    {

        private readonly IConfiguration _configuration;
        private readonly ILogger<LoginController> _logger;
        private readonly ActivitySource atividade;


        public CheckAccessCodeController(IConfiguration configuration, ILogger<LoginController> logger)
        {
            _configuration = configuration;
            _logger = logger;
            atividade = new ActivitySource("Check acess");
        }


        // ATUALIZAR DADOS SO PARA UTILIZADORES HELPDESK
        [HttpPut]
        public IActionResult UpdateProfile([FromBody] ProfileUpdateRequest updatedData)
        {
            try
            {
                using (var activity = atividade.StartActivity())
                {
                   
                    // Obter o tipo de utilizador com base no Email do utilizador autenticado
                    string userType = GetUserTypeByEmail(updatedData.UserEmail);


                    // Verificar se o usuário autenticado é um cliente
                    if (userType != "Helpdesk")
                    {
                        return Unauthorized("Acesso não autorizado.");
                    }

                    int knowCode = updatedData.KnowCode ? 1 : 0;


                    string sqlDataSource = _configuration.GetConnectionString("AS1DB");

                    using (SqlConnection connection = new SqlConnection(sqlDataSource))
                    {
                        connection.Open();
                        if (updatedData.KnowCode)
                        {
                            using (SqlCommand command = new SqlCommand("UpdateClientData", connection))
                            {
                                command.CommandType = CommandType.StoredProcedure;
                                command.Parameters.AddWithValue("@UserType", userType);
                                command.Parameters.AddWithValue("@UserMail", updatedData.EditedData.EmailAddress);
                                command.Parameters.AddWithValue("@FullName", updatedData.EditedData.FullName);
                                command.Parameters.AddWithValue("@PhoneNumber", updatedData.EditedData.PhoneNumber);
                                command.Parameters.AddWithValue("@ClientId", updatedData.EditedData.ClientId);
                                command.Parameters.AddWithValue("@MedReportId", updatedData.EditedData.MedReports[0].MedReportId);
                                command.Parameters.AddWithValue("@MedRecordNumber", updatedData.EditedData.MedicalRecordNumber);
                                command.Parameters.AddWithValue("@DiagnosisDetails", updatedData.EditedData.MedReports[0].DiagnosisDetails);
                                command.Parameters.AddWithValue("@TreatmentPlan", updatedData.EditedData.MedReports[0].TreatmentPlan);
                                command.Parameters.AddWithValue("@KnowCode", knowCode);

                                command.ExecuteNonQuery();

                            }
                        }
                        else
                        {
                            using (SqlCommand command = new SqlCommand("UpdateClientData", connection))
                            {
                                command.CommandType = CommandType.StoredProcedure;
                                command.Parameters.AddWithValue("@UserType", userType);
                                command.Parameters.AddWithValue("@FullName", updatedData.EditedData.FullName);
                                command.Parameters.AddWithValue("@ClientId", updatedData.EditedData.ClientId);
                                command.Parameters.AddWithValue("@MedReportId", updatedData.EditedData.MedReports[0].MedReportId);
                                command.Parameters.AddWithValue("@MedRecordNumber", updatedData.EditedData.MedicalRecordNumber);
                                command.Parameters.AddWithValue("@KnowCode", knowCode);

                               command.ExecuteNonQuery();

                            }
                        }
                    }   
                } 
                return Ok("Perfil atualizado com sucesso");
            }
            catch (Exception ex) {

                return BadRequest($"Erro ao atualizar perfil: {ex.Message}");
            }
        }


        [HttpPost]
        public IActionResult CheckAccessCode([FromBody] AccessCodeRequest accessCodeRequest)
        {
            if (accessCodeRequest.UserType != "Helpdesk")
            {
                return Unauthorized("Acesso não autorizado" + ".");
            }

            string enteredCode = accessCodeRequest.Code;
            string userType = accessCodeRequest.UserType;
            int clientId = accessCodeRequest.ClientId;

            string storedCode = GetStoredAccessCode(clientId);

            if (BCrypt.Net.BCrypt.Verify(enteredCode, storedCode))
            {
                _logger.LogInformation("Codigo de acesso Correto!!!!!!");
                var clientData = GetClientData(clientId, userType);
                return Ok(clientData);
            }
            else
            {
                _logger.LogInformation("Codigo de acesso incorreto!!!!!!");
                return BadRequest("Código de acesso incorreto");
            }
        }


        private string GetStoredAccessCode(int clientId)
        {
            try
            {
                string sqlDataSource = _configuration.GetConnectionString("AS1DB");

                using (SqlConnection connection = new SqlConnection(sqlDataSource))
                {
                    connection.Open();

                    using (SqlCommand command = new SqlCommand("SELECT AccessCodeHash FROM dbo.Clients WHERE ClientId = @ClientId", connection))
                    {
                        command.Parameters.AddWithValue("@ClientId", clientId);

                        object result = command.ExecuteScalar();

                        if (result != null)
                        {
                            return result.ToString();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"Erro ao obter AccessCodeHash: {ex.Message}");
            }
            return string.Empty;
        }


        private ClientData? GetClientData(int clientId, string userType)
        {
            try
            {
                string sqlDataSource = _configuration.GetConnectionString("AS1DB");

                using (SqlConnection connection = new SqlConnection(sqlDataSource))
                {
                    connection.Open();

                    using (SqlCommand command = new SqlCommand("GetClientData", connection))
                    {
                        command.CommandType = CommandType.StoredProcedure;
                        command.Parameters.AddWithValue("@UserMail", "");
                        command.Parameters.AddWithValue("@UserType", userType);
                        command.Parameters.AddWithValue("@ClientId", clientId);

                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            DataTable dataTable = new DataTable();
                            dataTable.Load(reader);

                            var clientData = new ClientData();

                            var client = new ClientData
                            {
                                ClientId = Convert.ToInt32(dataTable.Rows[0]["ClientId"]),
                                FullName = dataTable.Rows[0]["FullName"].ToString(),
                                EmailAddress = dataTable.Rows[0]["EmailAddress"].ToString(),
                                PhoneNumber = dataTable.Rows[0]["PhoneNumber"].ToString(),
                                MedicalRecordNumber = dataTable.Rows[0]["MedicalRecordNumber"].ToString(),
                                MedReports = new List<MedReport>(),
                            };


                            foreach (DataRow row in dataTable.Rows)
                            {
                                if (row["MedReportId"] != DBNull.Value)
                                {
                                    var medReport = new MedReport
                                    {
                                        MedReportId = Convert.ToInt32(row["MedReportId"]),
                                        MedicalRecordNumber = row["MedicalRecordNumber"].ToString(),
                                        DiagnosisDetails = row["DiagnosisDetails"].ToString(),
                                        TreatmentPlan = row["TreatmentPlan"].ToString(),
                                    };
                                    client.MedReports.Add(medReport);
                                }  
                            }
                            return client;
                        }
                    }
                }

                return null;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error: {ex.Message}");
                return null;
            }
        }

        private string GetUserTypeByEmail(string email)
        {
            string query = @"SELECT UserType FROM dbo.Users WHERE UserMail = @Email";
            string sqlDataSource = _configuration.GetConnectionString("AS1DB");
            using (SqlConnection mycon = new SqlConnection(sqlDataSource))
            {
                mycon.Open();
                using (SqlCommand command = new SqlCommand(query, mycon))
                {
                    command.Parameters.AddWithValue("@Email", email);
                    return (string)command.ExecuteScalar();
                }
            }
        }


    }
}
