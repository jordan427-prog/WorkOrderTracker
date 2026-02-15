namespace WorkOrderTracker.Api.Models;

public record UpdateWorkOrderRequest
    (
    String Title,
    String? Description,
    String Status
    );
