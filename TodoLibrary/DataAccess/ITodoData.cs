using TodoLibrary.Models;

namespace TodoLibrary.DataAccess
{
    public interface ITodoData
    {
        Task CompleteTodo(int todoId, int userId);
        Task<List<TodoModel>> Create(string task, int userId);
        Task Delete(int userId, int todoId);
        Task<List<TodoModel>> GetAllAssigned(int userId);
        Task<TodoModel?> GetOneAssigned(int userId, int todoId);
        Task UpdateTask(int todoId, int userId, string task);
    }
}