using System.Collections.Generic;
using Teamkeep.Models.DataModels;

namespace Teamkeep.Models.ServiceResponses
{
    public class EmailConfirmationsServiceResponse : ServiceResponse
    {
        public List<Availability> UpdatedAvailabilities { get; set; }
    }
}