using System;
namespace LegacyApp.DataAccess;

public class UserDataAccessProxy: IUserDataAccess
{
    public void AddUser(User user)
    {
        UserDataAccess.AddUser(user);
    }
}

