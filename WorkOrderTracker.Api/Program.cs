using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
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
app.MapGet("/api/workorders", async (AppDbContext db) =>
{
    var orders = await db.WorkOrders.OrderByDescending(w => w.Id).ToListAsync();
     return Results.Ok(orders);
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
        return Results.BadRequest("Status is required");
    }

    var description = req.Description?.Trim();

    var order = await db.WorkOrders.FindAsync(id);

    if (order is null)
    {
        return Results.NotFound();
    }

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

