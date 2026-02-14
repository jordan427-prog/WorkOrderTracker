using Microsoft.EntityFrameworkCore;
using WorkOrderTracker.Api.Models;

namespace WorkOrderTracker.Api.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    // All new models need to be registered here
    public DbSet<WorkOrder> WorkOrders => Set<WorkOrder>();
}
