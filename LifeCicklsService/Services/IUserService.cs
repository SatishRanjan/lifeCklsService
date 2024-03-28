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
        public ConnectionRequest? Connect(ConnectionRequest connectionRequest);
        public bool IsConnected(ConnectionRequest connectionRequest);
        public bool IsConnectionPending(ConnectionRequest connectionRequest);
        public List<ConnectionRequest> GetConnectionRequests(string userName);
        public string UpdateConnectionOutcome(ConnectionRequestResult connectionResult);
        public List<Connection> GetConnections(string userName);
    }
}
