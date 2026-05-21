using HelpDesk.Results.Models;

namespace HelpDesk.Results.Services;

public interface ITicketRepository
{
    IEnumerable<Ticket> GetAll();
    Ticket? GetById(int id);
    Ticket Create(string title, int priority);
}
