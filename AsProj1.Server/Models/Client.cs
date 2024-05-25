using Microsoft.AspNetCore.Http.HttpResults;

namespace AsProj1.Server.Models
{
    public class Client
    {
        public int ClientId { get; set; }

        public string ClientName { get; set; }

        public string ClientEmail { get; set; }

        public string ClientPhoneNumber { get; set; }

        public int ClientMedRecordNumber { get; set;}

        public string ClientDiagnosisDetails { get; set; }

        public string ClientTreatmentPlan { get; set; }
    }
}


