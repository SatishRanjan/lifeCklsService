using LifeCklsModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LifeCicklsService.Services
{
    public interface IUserService
    {
        public UserProfile? Login(string username, string password);
        public UserProfile? FindByUserName(string userName);
        public UserProfile Register(UserRegistrationRequest user);
    }
}
