using Microsoft.EntityFrameworkCore;
using NSwag.AspNetCore;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddDbContext<TodoDb>(options =>
{
    options.UseInMemoryDatabase("TodoList");
});
builder.Services.AddDatabaseDeveloperPageExceptionFilter();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddOpenApiDocument(config =>
{
    config.DocumentName = "TodoAPI";
    config.Title = "TodoAPI v1";
    config.Version = "v1";
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseOpenApi();
    app.UseSwaggerUi(config =>
    {
        config.DocumentTitle = "TodoAPI";
        config.Path = "/swagger";
        config.DocumentPath = "/swagger/{documentName}/swagger.json";
        config.DocExpansion = "list";
    });
}

app.MapGet("/todoitems", async (TodoDb db) => await db.TodoItems.ToListAsync());

app.MapGet("/todoitems/complete", async (TodoDb db) =>
{
        await db.TodoItems
            .Where(todo => todo.IsComplete)
            .ToListAsync();
});

app.MapGet("/todoitems/{id}", async (int id, TodoDb db) =>
{
    var todo = await db.TodoItems.FindAsync(id);
    if (todo == null)
    {
        return Results.NotFound();
    }
    return Results.Ok(todo);
});

app.MapPost("/todoitems", async (Todo todo, TodoDb db) =>
{
    db.TodoItems.Add(todo);
    await db.SaveChangesAsync();
    return Results.Created($"/todoitems/{todo.Id}", todo);
});

app.MapPut("/todoitems/{id}", async (int id, Todo inputTodo, TodoDb db) =>
{
    var todo = await db.TodoItems.FindAsync(id);

    if (todo is null) return Results.NotFound();

    todo.Name = inputTodo.Name;
    todo.IsComplete = inputTodo.IsComplete;

    await db.SaveChangesAsync();

    return Results.NoContent();
});

app.MapDelete("/todoitems/{id}", async (int id, TodoDb db) =>
{
    var todo = await db.TodoItems.FindAsync(id);

    if (todo is null) return Results.NotFound();

    db.TodoItems.Remove(todo);
    await db.SaveChangesAsync();

    return Results.NoContent();
});

app.Run();
