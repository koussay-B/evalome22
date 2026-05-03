using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using api.data.Entities;

namespace api.services.TokenService
{
   public interface ITokenService
	{
        Task<string> CreateToken(AppUser user);
    }

}