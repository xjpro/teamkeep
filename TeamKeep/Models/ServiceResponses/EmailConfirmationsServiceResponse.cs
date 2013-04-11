using System.Collections.Generic;
using TeamKeep.Models.DataModels;

namespace TeamKeep.Models.ServiceResponses
{
    public class EmailConfirmationsServiceResponse : ServiceResponse
    {
        public List<Availability> UpdatedAvailabilities { get; set; }
    }
}