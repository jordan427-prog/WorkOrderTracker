using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using WorkOrderTracker.Api.Data;
using WorkOrderTracker.Api.Models;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;


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

builder.Services.AddCors(options =>
{
    options.AddPolicy("ViteDev", p =>
        p.WithOrigins("http://localhost:5173")
         .AllowAnyHeader()
         .AllowAnyMethod());
});


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

app.UseCors("ViteDev");



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
        Status = "New",
        CreatedAtUtc = DateTime.UtcNow,
        LastEditedAtUtc = null
    };

    db.WorkOrders.Add(wo);

    await db.SaveChangesAsync();

    return Results.Created($"/api/workorders/{wo.Id}", wo);

})
.WithName("CreateWorkOrder");
static void AddErrors(Dictionary<string, List<string>> errors, string key, string errorMessage)
{
    if (!errors.ContainsKey(key))
    {
        errors[key] = new List<string>();
    }
    errors[key].Add(errorMessage);
}

app.MapPut("/api/workorders/{id:int}", async (int id, UpdateWorkOrderRequest req, AppDbContext db) =>
{
    Dictionary<string, List<string>> errorList = new Dictionary<string, List<string>>(StringComparer.OrdinalIgnoreCase);

    var order = await db.WorkOrders.FindAsync(id);

    if (order is null)
    {
        AddErrors(errorList, "Id", $"No order with{id} was found");
    }

    var title = req.Title?.Trim();
    if (string.IsNullOrWhiteSpace(title))
    {
        AddErrors(errorList, "Title", "Title is required");
    }
    if (!string.IsNullOrWhiteSpace(title) && title.Length > 100)
    {
        AddErrors(errorList, "Title", "Title must be smaller than 100 characters");
    }
    var status = req.Status?.Trim();

    if (string.IsNullOrWhiteSpace(status))
    {
        AddErrors(errorList, "Status", "Status is required");
    }

    if (!string.IsNullOrWhiteSpace(status) && !allowedStatus.Contains(status))
    {
        AddErrors(errorList, "Status", "Status must be either: " + string.Join(",", allowedStatus));
    }

    if (errorList.Count > 0)
    {
        return Results.BadRequest(errorList);
    }

    var description = req.Description?.Trim();

    // standardize statuses to the allowed status with case insensitivity, so we can be sure the DB has consistent values for status
    string stat = allowedStatus.First(s => s.Equals(status, StringComparison.OrdinalIgnoreCase))!;
    // order is guaranteed not null here due to error check above
    // title and status are guaranteed not null due to error check above
    order!.Title = title!;
    order.Status = stat;
    order.Description = description;
    order.LastEditedAtUtc = DateTime.UtcNow;

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

// APIs for Notes

app.MapGet("/api/workorders/{workOrderId:int}/notes", async (int workOrderId, AppDbContext db) =>
{
    var wo = await db.WorkOrders.FindAsync(workOrderId);

    if(wo is null)
    {
        return Results.NotFound();
    }
    var q = db.Notes.AsQueryable();

    q = q.Where(n => n.WorkOrderId.Equals(workOrderId));

    var count = await  q.CountAsync();
    var items = await q
    .OrderByDescending(n => n.Id)
    .Select(n => new
    {
        n.Id,
        n.WorkOrderId,
        n.Content,
        n.CreatedAtUtc
    })
    .ToListAsync();

    return Results.Ok(new
    {
        count,
        items
    });
}).WithName("GetWorkOrderNotes");

app.MapPost("/api/workorders/{workOrderId:int}/notes", async (int workOrderId, CreateNote dto, AppDbContext db) =>
{
    Dictionary<string, List<string>> errors = new Dictionary<string, List<string>>();

    var wo = await db.WorkOrders.FindAsync(workOrderId);

    if(wo is null) 
    {
        AddErrors(errors, "Parent work order", "Work order does not exist");
        //return Results.NotFound("Work order not found"); 
    }

    if(string.IsNullOrWhiteSpace(dto.Content))
    {
        AddErrors(errors, "Content", "A note requires content and may not be empty");
        //return Results.BadRequest("Note content is empty");
    }
    if (!string.IsNullOrWhiteSpace(dto.Content))
    {
        var cont = dto.Content.Trim();

        if (cont.Length > 1000)
        {
            AddErrors(errors, "Content", "Length of note content should be less than or equal to 1000 character");
        }
    }

    if(errors.Count>0)
    {
        return Results.BadRequest(errors);
    }

    var note = new Note
    {
        Content = dto.Content.Trim(),
        WorkOrderId = workOrderId
    };

    db.Notes.Add(note);
    await db.SaveChangesAsync();
    return Results.Created($"/api/workorders/{workOrderId}/notes/{note.Id}", new
    {
        note.Id,
        note.WorkOrderId,
        note.Content,
        note.CreatedAtUtc
    });

}).WithName("CreateNote");



app.Run();

