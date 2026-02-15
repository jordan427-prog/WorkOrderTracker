namespace WorkOrderTracker.Api.Models;

// This is the mapper for what comes from the body
// When a work order onject is passed as JSON from the front-end, we take it as one of these objects
// Then we map it to a WorkOrder to use internally

public record CreateWorkOrderRequest(
    string Title,
    string? Description
);
