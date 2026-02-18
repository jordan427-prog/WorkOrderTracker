namespace WorkOrderTracker.Api.Models;

// Note attached to a WorkOrder
public class Note
{
    public int Id { get; set; }

    // FK to WorkOrder
    public int WorkOrderId { get; set; }

    // Navigation back to parent work order
    public WorkOrder WorkOrder { get; set; } = null!;

    // Note contents
    public string Content { get; set; } = "";

    // When the note was created
    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
}