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
        public User Register(User user);
    }
}
