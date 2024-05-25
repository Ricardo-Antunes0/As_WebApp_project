    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Mvc;
    using System.Data.SqlClient;
    using System.Data;
    using AsProj1.Server.Models;
    using System.Net.Mail;
using System.Diagnostics;

    namespace AsProj1.Server.Controllers
    {
      
        public class ClientData
        {
            public int ClientId { get; set; }
            public string FullName { get; set; }
            public string EmailAddress { get; set; }
            public string PhoneNumber { get; set; }
            public string MedicalRecordNumber { get; set; }
            public List<MedReport> MedReports { get; set; }
    }

        public class MedReport
        {
            public int MedReportId { get; set; }
            public string MedicalRecordNumber { get; set; }
            public string DiagnosisDetails { get; set; }
            public string TreatmentPlan { get; set; }
       
        }


        [Route("api/[controller]")]
        [ApiController]
        public class ProfileController : ControllerBase
        {

            private readonly IConfiguration _configuration;

            public ProfileController(IConfiguration configuration)
            {
                _configuration = configuration;
            }



        // UPDATE PROFILE CLIENTE

        [HttpPut]
        public IActionResult UpdateProfile([FromBody] ClientData updatedData)
        {
            try {
                // Obter o tipo de utilizador com base no Email do utilizador autenticado
                string userType = GetUserTypeByEmail(updatedData.EmailAddress);

                
                // Verificar se o usuário autenticado é um cliente
                if (userType != "Client")
                {
                    //return BadRequest("Utilizador não é um cliente");
                    return Unauthorized("Acesso não autorizado.");
                }
                

                string sqlDataSource = _configuration.GetConnectionString("AS1DB");

                using (SqlConnection connection = new SqlConnection(sqlDataSource))
                {
                    connection.Open();
                    Debug.WriteLine("ABRIU");

                    using (SqlCommand command = new SqlCommand("UpdateClientData", connection))
                    {   
                        command.CommandType = CommandType.StoredProcedure;
                        command.Parameters.AddWithValue("@UserType", userType);
                        command.Parameters.AddWithValue("@UserMail", updatedData.EmailAddress);
                        command.Parameters.AddWithValue("@FullName", updatedData.FullName);
                        command.Parameters.AddWithValue("@PhoneNumber", updatedData.PhoneNumber);
                  
                        command.ExecuteNonQuery();
                    }
                }

                return Ok("Perfil atualizado com sucesso");
            }
            catch (Exception ex)
            {
                return BadRequest($"Erro ao atualizar perfil: {ex.Message}");
            }
        }




        // FUNCAO PARA obter os dados do perfil tant opara HELPDESK como para CLIENTES usando um stored procedure GetClientData
        [HttpGet]
            public JsonResult GetProfile(string email)
            {
                try
                {
                    string userType = GetUserTypeByEmail(email);

                    string sqlDataSource = _configuration.GetConnectionString("AS1DB");

                    using (SqlConnection connection = new SqlConnection(sqlDataSource))
                    {
                        connection.Open();


                        using (SqlCommand command = new SqlCommand("GetClientData", connection))
                        {
                            command.CommandType = CommandType.StoredProcedure;
                            command.Parameters.AddWithValue("@UserMail", email);
                            command.Parameters.AddWithValue("@UserType", userType);
                            command.Parameters.AddWithValue("@ClientId", 0);

                            using (SqlDataReader reader = command.ExecuteReader())
                            {
                               
                                DataTable dataTable = new DataTable();
                                dataTable.Load(reader);

                               
                                var clientData = new List<ClientData>();
                               

                                foreach (DataRow row in dataTable.Rows)
                                {

                                    var client = new ClientData
                                    {
                                        ClientId = Convert.ToInt32(row["ClientId"]),
                                        FullName = row["FullName"].ToString(),
                                        EmailAddress = row["EmailAddress"].ToString(),
                                        PhoneNumber = row["PhoneNumber"].ToString(),
                                        MedicalRecordNumber = row["MedicalRecordNumber"].ToString(),
                                        MedReports = new List<MedReport>(),
                                    };

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

                                    clientData.Add(client);
                                }
                            return new JsonResult(new {UserType = userType, ClientData = clientData});
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    string errorMessage = ex != null ? ex.Message : "An error occurred";
                    return new JsonResult($"Error: {errorMessage}");
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
