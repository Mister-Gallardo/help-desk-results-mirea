namespace HelpDesk.Results;

public interface ITicketRepository
{
    IEnumerable<Ticket> GetAll();
    Ticket? GetById(int id);
    Ticket Create(string title, int priority);
}
