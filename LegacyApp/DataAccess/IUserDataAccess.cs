using System;
using LegacyApp.Models;

namespace LegacyApp.DataAccess
{
    public interface IUserDataAccess
    {
        public void AddUser(User user);
    }
}