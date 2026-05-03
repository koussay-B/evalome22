using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace api.services.CloudinaryService
{
    public class CloudinarySettings
    {
        public required string CloudName { get; set; }
        public required string ApiKey { get; set; }
        public required string ApiSecret { get; set; }
    }
}