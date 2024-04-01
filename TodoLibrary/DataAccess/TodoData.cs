using TodoLibrary.Models;

namespace TodoLibrary.DataAccess;

public class TodoData : ITodoData
{
    private readonly ISqlDataAccess sql;

    public TodoData(ISqlDataAccess sql)
    {
        this.sql = sql;
    }

    public Task<List<TodoModel>> GetAllAssigned(int userId)
    {
        return sql.LoadData<TodoModel, dynamic>("dbo.spTodos_GetAllAssigned",
            new { UserId = userId }, "Default");
    }

    public async Task<TodoModel?> GetOneAssigned(int userId, int todoId)
    {
        var results = await sql.LoadData<TodoModel, dynamic>("dbo.spTodos_GetOneAssigned",
            new { UserId = userId, TodoId = todoId }, "Default");

        return results.FirstOrDefault();
    }

    public Task<List<TodoModel>> Create(string task, int userId)
    {
        return sql.LoadData<TodoModel, dynamic>("dbo.spTodos_Create",
            new { Task = task, UserId = userId }, "Default");
    }

    public Task UpdateTask(int todoId, int userId, string task)
    {
        return sql.SaveData<dynamic>("dbo.spTodos_UpdateTask",
            new { TodoId = todoId, Task = task, UserId = userId }, "Default");
    }

    public Task CompleteTodo(int todoId, int userId)
    {
        return sql.SaveData<dynamic>("dbo.spTodos_CompleteTodo",
            new { TodoId = todoId, UserId = userId }, "Default");
    }

    public Task Delete(int userId, int todoId)
    {
        return sql.SaveData<dynamic>("dbo.spTodos_Delete",
            new { UserId = userId, TodoId = todoId }, "Default");
    }
}
