using AsProj1.Server.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Data.SqlClient;
using System.Data;
using BCrypt.Net;
using System.Net;

namespace AsProj1.Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RegisterController : ControllerBase
    {
        private readonly IConfiguration _configuration;

        public RegisterController(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        [HttpPost]
        public JsonResult Register(Register registrationData)
        {
            try
            {
                // Verificar se o e-mail já está em uso
                if (IsEmailInUse(registrationData.Email))
                {
                    return new JsonResult("E-mail já está em uso.");
                }

                /* FAZER A VERIFICACAO DOS CAMPOS e validação
                 * 
                // Verificar se o MedicalRecordNumber já está em uso
                if (IsMedicalRecordNumberInUse(registrationData.MedicalRecordNumber))
                {
                    return new JsonResult("Número de record médico já está em uso.");
                }
                */

                // realizar hash do acess code
                string hashedAcessCode = HashData(registrationData.AcessCode);
                string hashedPassword = HashData(registrationData.Password);

                // Salvar dados na tabela dbo.Users
                SaveUserToDatabase(registrationData.Email, hashedPassword, registrationData.UserType);

                if (registrationData.UserType == "Client")
                {
                  
                    SaveClientToDatabase(registrationData.Name, registrationData.PhoneNumber, registrationData.Email, registrationData.MedicalRecordNumber, hashedAcessCode);
                }

                return new JsonResult(new { Email = registrationData.Email, UserType = registrationData.UserType });
            }
            catch (Exception ex)
            {
                return new JsonResult($"Erro durante o registro: {ex.Message}");
            }
        }

        private bool IsEmailInUse(string email)
        {
            string query = @"SELECT COUNT(*) FROM dbo.Users WHERE UserMail = @Email";
            DataTable dt = new DataTable();
            string sqlDataSource = _configuration.GetConnectionString("AS1DB");
            using (SqlConnection mycon = new SqlConnection(sqlDataSource))
            {
                mycon.Open();
                using (SqlCommand command = new SqlCommand(query, mycon))
                {
                    command.Parameters.AddWithValue("@Email", email);
                    int count = (int)command.ExecuteScalar();
                    return count > 0;
                }
            }
        }


        // Funcao para fazer hash na password e access code
        private string HashData(string data)
        {
            return BCrypt.Net.BCrypt.HashPassword(data);
        }


        private void SaveUserToDatabase(string email, string hashedPassword, string userType)
        {
            string query = @"
                            INSERT INTO dbo.Users (UserMail, PasswordHash, UserType)
                            VALUES (@Email, @HashedPassword, @UserType)";

            string sqlDataSource = _configuration.GetConnectionString("AS1DB");

            using (SqlConnection mycon = new SqlConnection(sqlDataSource))
            {
                mycon.Open();

                using (SqlCommand command = new SqlCommand(query, mycon))
                {
                    command.Parameters.AddWithValue("@Email", email);
                    command.Parameters.AddWithValue("@HashedPassword", hashedPassword);
                    command.Parameters.AddWithValue("@UserType", userType);

                    command.ExecuteNonQuery();
                }
            }
        }

        private void SaveClientToDatabase(string name, string phoneNumber, string email, string medicalRecordNumber, string hashedAcessCode)
        {
            string query = @"
                BEGIN TRANSACTION;

                INSERT INTO dbo.Clients (FullName, EmailAddress, PhoneNumber, MedicalRecordNumber, AccessCodeHash)
                VALUES (@Fullname, @Email, @PhoneNumber, @MedicalRecordNumber, @AccessCodeHash);

                INSERT INTO dbo.MedReport (MedicalRecordNumber)
                VALUES (@MedicalRecordNumber);

                COMMIT TRANSACTION;";

            string sqlDataSource = _configuration.GetConnectionString("AS1DB");

            using (SqlConnection mycon = new SqlConnection(sqlDataSource))
            {
                mycon.Open();

                using (SqlCommand command = new SqlCommand(query, mycon))
                {
                    try
                    {
                        command.Parameters.AddWithValue("@Fullname", name);
                        command.Parameters.AddWithValue("@Email", email);
                        command.Parameters.AddWithValue("@PhoneNumber", phoneNumber);
                        command.Parameters.AddWithValue("@MedicalRecordNumber", medicalRecordNumber);
                        command.Parameters.AddWithValue("@AccessCodeHash", hashedAcessCode);
                        command.ExecuteNonQuery();

                        System.Diagnostics.Debug.WriteLine("Sucesso na inserção nos clientes");
                    }
                    catch (SqlException sqlEx)
                    {
                        System.Diagnostics.Debug.WriteLine($"Erro SQL: {sqlEx.Message}");
                        throw;
                    }
                }
            }
        }
    }
}
