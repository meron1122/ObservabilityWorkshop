var builder = WebApplication.CreateBuilder(args);

// Add Aspire service defaults (telemetry, health checks, service discovery)
builder.AddAppDefaults();
builder.AddDefaultHealthChecks();


// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

builder.Services.AddSingleton<TodoRepository>();
builder.Services.AddSingleton<TodoMetrics>();

var app = builder.Build();


app.MapOpenApi();


app.UseHttpsRedirection();
app.MapDefaultEndpoints();


//GET
app.MapGet("/todos", (TodoRepository repo) => repo.GetAll())
    .WithName("GetAllTodos")
    .WithDescription("Gets all todo items");

//POST
app.MapPost("/todos", async (TodoItem newTodo, TodoRepository repo, HttpClient client, ILogger<Program> logger,TodoMetrics todoMetrics) =>
    {
        logger.LogInformation("Call backofficeAPI");
        await client.GetAsync("https+http://backofficeapi/weatherforecast");

        var todo = repo.Add(newTodo);
        todoMetrics.TodoCreated();
        
        return Results.Created($"/todos/{todo.Id}", todo);
    })
    .WithName("CreateTodo")
    .WithDescription("Creates a new todo item");

//DELETE
app.MapDelete("/todos/{id}", (int id, TodoRepository repo) =>
    {
        var removed = repo.Remove(id);
        return removed ? Results.Ok() : Results.NotFound();
    })
    .WithName("DeleteTodo")
    .WithDescription("Deletes a todo item");







app.Run();


// Todo item model
public class TodoItem
{
    public int Id { get; set; }
    public string? Title { get; set; }
    public bool IsComplete { get; set; }
}