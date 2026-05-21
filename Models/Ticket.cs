namespace HelpDesk.Results;

public record Ticket(int Id, string Title, string Status, int Priority, DateTime CreatedAt);
