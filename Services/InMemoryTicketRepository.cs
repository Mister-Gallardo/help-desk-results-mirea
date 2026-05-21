namespace HelpDesk.Results;

public sealed class InMemoryTicketRepository : ITicketRepository
{
    private readonly List<Ticket> _tickets = new()
    {
        new Ticket(1, "Принтер не печатает", "Open",   2, new DateTime(2025, 5, 10, 9,  0, 0)),
        new Ticket(2, "Нет доступа к VPN",   "InProgress", 1, new DateTime(2025, 5, 12, 14, 30, 0)),
        new Ticket(3, "Сбой почтового клиента", "Closed", 3, new DateTime(2025, 5, 15, 11, 0, 0)),
    };

    private int _nextId = 4;

    public IEnumerable<Ticket> GetAll() => _tickets.AsReadOnly();

    public Ticket? GetById(int id) => _tickets.FirstOrDefault(t => t.Id == id);

    public Ticket Create(string title, int priority)
    {
        var ticket = new Ticket(_nextId++, title, "Open", priority, DateTime.UtcNow);
        _tickets.Add(ticket);
        return ticket;
    }
}
