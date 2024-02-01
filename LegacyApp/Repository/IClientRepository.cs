using LegacyApp;
using LegacyApp.Models;

namespace LegacyApp.Repository
{
    public interface IClientRepository
    {
        public Client GetById(int id);

    }
}