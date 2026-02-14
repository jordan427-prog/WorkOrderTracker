namespace WorkOrderTracker.Api.Models;

public class WorkOrder
{
    public int Id { get; set; }                  // Primary key (EF recognizes "Id" automatically)

    public string Title { get; set; } = "";      // Required-ish (we'll validate later)

    public string? Description { get; set; }     // Optional column (nullable)

    public string Status { get; set; } = "New";  // Simple status for now

    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
}
