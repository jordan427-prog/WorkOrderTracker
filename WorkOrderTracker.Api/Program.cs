using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using WorkOrderTracker.Api.Data;
using WorkOrderTracker.Api.Models;


var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi

//builder.Services.AddOpenApi();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// "Default" connection string in appsettings.Development.json
// DBContext registration
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("Default")));

var app = builder.Build();

HashSet<string> allowedStatus = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
allowedStatus.Add("New");
allowedStatus.Add("InProgress");
allowedStatus.Add("Blocked");
allowedStatus.Add("Done");

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    //app.MapOpenApi();
    app.UseSwagger();
    app.UseSwaggerUI();
}

//app.UseHttpsRedirection();

// AppDbContext is ASP.NET injecting my DB context
// db.WorkOrders is the work orders table
// returns a list as JSON with 200 OK
app.MapGet("/api/workorders", async (AppDbContext db, string? status, int page = 1, int pageSize = 20) =>
{
    // Let's ensure page and page size are guarded
    if(page <1)
    {
        page = 1;
    }
    if(pageSize < 1)
    {
        pageSize = 20;
    }
    if(pageSize>100)
    {
        pageSize = 100;
    }

    // Setup SQL query
    var query = db.WorkOrders.AsQueryable();


    var st = status?.Trim();

    // Optional filter for status may be incoming
    // Add status to SQL query
    if (!string.IsNullOrWhiteSpace(st))
    {
        // Normalize to a single case so EF Core can translate to SQL (LOWER/UPPER)
        var stNormalized = st.ToUpperInvariant();
        query = query.Where(w =>
            w.Status != null &&
            w.Status.ToUpper() == stNormalized);
    }

    // Total count for pagination UI ( gives us a count(*) )
    var total = await query.CountAsync();

    //20 touples per page, so order IDs, skip previous pages, take a list of touples for that page size
    var items = await query.OrderByDescending(x=>x.Id).Skip((page-1)*pageSize).Take(pageSize).ToListAsync();

    return Results.Ok(new
    {
        total,
        page,
        pageSize,
        items
    });
})
.WithName("GetWorkOrders");

app.MapGet("/api/workorders/{id:int}", async (int id, AppDbContext db) =>
{
    var order = await db.WorkOrders.FindAsync(id);

    return order is null
    ? Results.NotFound() : Results.Ok(order);
}).WithName("GetWorkOrderById");

app.MapPost("/api/workorders", async (AppDbContext db, CreateWorkOrderRequest req) =>
{
    var title = req.Title?.Trim();
    if(string.IsNullOrWhiteSpace(title))
    {
        return Results.BadRequest("title is required");
    }
    if(title.Length>100)
    {
        return Results.BadRequest("Title must be smaller than 100 characters");
    }

    var description = req.Description?.Trim();

    WorkOrder wo = new WorkOrder
    {
        Title = title,
        Description = description,
        Status="New",
        CreatedAtUtc = DateTime.UtcNow
    };

    db.WorkOrders.Add(wo);

    await db.SaveChangesAsync();

    return Results.Created($"/api/workorders/{wo.Id}", wo);

})
.WithName("CreateWorkOrder");

app.MapPut("/api/workorders/{id:int}", async (int id,UpdateWorkOrderRequest req  ,AppDbContext db) =>
{

    var order = await db.WorkOrders.FindAsync(id);

    if (order is null)
    {
        return Results.NotFound();
    }

    var title = req.Title?.Trim();
    if (string.IsNullOrWhiteSpace(title))
    {
        return Results.BadRequest("title is required");
    }
    if (title.Length > 100)
    {
        return Results.BadRequest("Title must be smaller than 100 characters");
    }
    var status = req.Status?.Trim();

    if(string.IsNullOrWhiteSpace(status))
    {
        return Results.BadRequest("Status is needed");
    }

    if(!allowedStatus.Contains(status))
    {
        return Results.BadRequest("Status is required and must be either: Done, InProgress, Blocked, or New");
    }
    if(string.IsNullOrWhiteSpace(status))
    {
        return Results.BadRequest("Status is required");
    }

    var description = req.Description?.Trim();

    order.Title = title;
    order.Status = status;
    order.Description = description;

    await db.SaveChangesAsync();

    return Results.Ok(order);
})
.WithName("UpdateWorkOrder");

app.MapDelete("/api/workorders/{id:int}",async(int id, AppDbContext db) =>
{
    var order = await db.WorkOrders.FindAsync(id);
    if (order is null) { return Results.NotFound(); }

    db.WorkOrders.Remove(order);
    await db.SaveChangesAsync();

    return Results.NoContent();
}).WithName("DeleteWorkOrder");



app.Run();

