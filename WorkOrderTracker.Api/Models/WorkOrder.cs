namespace WorkOrderTracker.Api.Models;

// This is the actual DB schema, so incomig data will come in as a CreateOrderRequest and have to be mapped to this

public class WorkOrder
{
    public int Id { get; set; }                  // Primary key (EF recognizes "Id" automatically)

    public string Title { get; set; } = "";      // Required-ish (we'll validate later)

    public string? Description { get; set; }     // Optional column (nullable)

    public string Status { get; set; } = "New";  // Simple status for now

    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
    public DateTime? LastEditedAtUtc { get; set; }

    public List<Note> Notes { get; set; } = new List<Note>(); // Navigation property for related notes
}
