using NUnit.Framework;
using System.Net.Http;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using AsProj1.Server.Models;
using AsProj1.Server.Controllers;
using System.Text;

namespace AsProj1.Tests
{
    [TestFixture]
    public class ProfileControllerIntegrationTests
    {
        private HttpClient _httpClient;
        private string _baseUrl;

        [SetUp]
        public void Setup()
        {  
            _httpClient = new HttpClient();
            _baseUrl = "https://localhost:7095/api/Profile";
        }

        [TearDown]
        public void TearDown()
        {
            _httpClient.Dispose();
        }




        [Test]
        public async Task GetClientDetails_unmasked()
        {
            string fullNameExpected = "Teste";
            string phoneNumberExpected = "912345678";
            string medicalRecordNumberExpected = "199199199";


            var response = await _httpClient.GetAsync("https://localhost:7095/api/Profile?email=teste@ua.pt");
            
            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);

            if (response.IsSuccessStatusCode)
            {
                var responseBody = await response.Content.ReadAsStringAsync();
                Console.WriteLine("Response Body:");
                Console.WriteLine(responseBody);

                var userProfile = JsonConvert.DeserializeObject<UserProfileResponse>(responseBody);

                // Verificar os valores esperados
                Assert.AreEqual("Client", userProfile.UserType);

                Assert.IsNotNull(userProfile.ClientData);
                Assert.AreEqual(1, userProfile.ClientData.Count);

                var client = userProfile.ClientData[0];

                Assert.AreEqual(fullNameExpected, client.FullName);
                Assert.AreEqual(phoneNumberExpected, client.PhoneNumber);
                Assert.AreEqual(medicalRecordNumberExpected, client.MedicalRecordNumber);
               
            }
            else
            {
                Console.WriteLine("Failed to get response. Status code: " + response.StatusCode);
            }
        }


        [Test]
        public async Task GetClientDetails_masked_withoutCode()
        {
            string fullNameExpected = "Teste";
            string email = "tXXX@XXXX.com";
            string phoneNumberExpected = "xxxx";
            string medicalRecordNumberExpected = "xxxx";

            // Mudar a string em baixo para conter um email de um user do tipo helpdesk

            var response = await _httpClient.GetAsync("https://localhost:7095/api/Profile?email=helpdesk@ua.pt");

            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);

            if (response.IsSuccessStatusCode)
            {
                var responseBody = await response.Content.ReadAsStringAsync();
                Console.WriteLine("Response Body:");
                Console.WriteLine(responseBody);

                var userProfile = JsonConvert.DeserializeObject<UserProfileResponse>(responseBody);

                Assert.AreEqual("Helpdesk", userProfile.UserType);
                Assert.IsNotNull(userProfile.ClientData);
             

                var client = userProfile.ClientData[4];

                // Verificar que os dados estão mascarados 
                Assert.AreEqual(fullNameExpected, client.FullName);
                Assert.AreEqual(email, client.EmailAddress);
                Assert.AreEqual(phoneNumberExpected, client.PhoneNumber);
                Assert.AreEqual(medicalRecordNumberExpected, client.MedicalRecordNumber);
               
             
            }
            else
            {
                Console.WriteLine("Failed to get response. Status code: " + response.StatusCode);
            }
        }

        [Test]
        public async Task GetClientDetails_unmasked_withCode()
        {
            string fullNameExpected = "Teste";
            string email = "teste@ua.pt";
            string phoneNumberExpected = "912345678";
            string medicalRecordNumberExpected = "199199199";


            var accessCodeRequest = new AccessCodeRequest {
                UserType = "Helpdesk",
                Code = "0000",  // Codigo do cliente Teste
                ClientId = 4 // No meu caso, o cliente de teste era o cliente com o ID 4
            };

            var jsonRequestData = JsonConvert.SerializeObject(accessCodeRequest);

            var content = new StringContent(jsonRequestData, Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync("https://localhost:7095/api/CheckAccessCode", content);

            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);

            if (response.IsSuccessStatusCode)
            {
                var responseBody = await response.Content.ReadAsStringAsync();
                Console.WriteLine("Response Body:");
                Console.WriteLine(responseBody);

                var userProfile = JsonConvert.DeserializeObject<ClientData>(responseBody);

                Assert.IsNotNull(userProfile);
             
                var client = userProfile;

                // Verificar que os dados estão unmasked pois foi inserido corretamente o codigo
                Assert.AreEqual(fullNameExpected, client.FullName);
                Assert.AreEqual(email, client.EmailAddress);
                Assert.AreEqual(phoneNumberExpected, client.PhoneNumber);
                Assert.AreEqual(medicalRecordNumberExpected, client.MedicalRecordNumber);

            }
            else
            {
                Console.WriteLine("Failed to get response. Status code: " + response.StatusCode);
            }
        }


        [Test]
        public async Task GetClientDetails_unmasked_with_wrong_Code()
        {
           
            var accessCodeRequest = new AccessCodeRequest
            {
                UserType = "Helpdesk",
                Code = "1234",  // Codigo Errado do cliente "Teste"
                ClientId = 4 // No meu caso, o cliente de teste era o cliente com o ID 4
            };

            var jsonRequestData = JsonConvert.SerializeObject(accessCodeRequest);

            var content = new StringContent(jsonRequestData, Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync("https://localhost:7095/api/CheckAccessCode", content);
               
            // MENSagem de bar request pois foi inserido um codigo errado do cliente Teste ( clientId = 4)
            Assert.AreEqual(HttpStatusCode.BadRequest, response.StatusCode);
        }
    }

    public class UserProfileResponse
    {
        public string UserType { get; set; }
        public List<ClientData> ClientData { get; set; }
    }
}
