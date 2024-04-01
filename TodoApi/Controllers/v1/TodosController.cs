using Asp.Versioning;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.Threading.Tasks;
using TodoLibrary.DataAccess;
using TodoLibrary.Models;

namespace TodoApi.Controllers.v1;

[Route("api/v{version:apiVersion}/[controller]")]
[ApiController]
[ApiVersion("1.0")]
public class TodosController : ControllerBase
{
    private readonly ITodoData data;
    private readonly ILogger<TodosController> logger;

    public TodosController(ITodoData data, ILogger<TodosController> logger)
    {
        this.data = data;
        this.logger = logger;
    }

    /// <summary>
    /// Retrieves the current user's ID from the user claims.
    /// </summary>
    /// <returns>The user's ID as an integer.</returns>
    /// <exception cref="FormatException">Thrown if the user ID is not in a valid format.</exception>
    private int GetUserId()
    {
        var userIdText = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;
        return int.Parse(userIdText);
    }

    /// <summary>
    /// Handles the GET request to retrieve all assigned Todo items for the current user.
    /// </summary>
    /// <returns>A collection of TodoModel objects or a BadRequest in case of an exception.</returns>
    // GET api/Todos
    [HttpGet]
    public async Task<ActionResult<IEnumerable<TodoModel>>> Get()
    {
        logger.LogInformation("GET: api/Todos");

        try
        {
            var output = await data.GetAllAssigned(GetUserId());

            return Ok(output);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, $"The GET call to api/Todos failed.");
            return BadRequest();
        }
    }

    /// <summary>
    /// Handles the GET request to retrieve a specific Todo item for the current user.
    /// </summary>
    /// <param name="todoId">The ID of the Todo item to retrieve.</param>
    /// <returns>The TodoModel object or a BadRequest in case of an exception.</returns>
    // GET api/Todos/5
    [HttpGet("{todoId}")]
    public async Task<ActionResult<TodoModel>> Get(int todoId)
    {
        logger.LogInformation("GET: api/Todos/{todoId}", todoId);

        try
        {
            var output = await data.GetOneAssigned(GetUserId(), todoId);

            return Ok(output);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "The GET call to api/Todos/{TodoId} failed.", todoId);
            return BadRequest();
        }
    }

    /// <summary>
    /// Handles the POST request to create a new Todo item for the current user.
    /// </summary>
    /// <param name="task">The task description of the new Todo item.</param>
    /// <returns>The created TodoModel object or a BadRequest in case of an exception.</returns>
    //POST api/Todos
    [HttpPost]
    public async Task<ActionResult<TodoModel>> Post([FromBody] string task)
    {
        logger.LogInformation("POST: api/Todos (Task: {Task})", task);

        try
        {
            var output = await data.Create(task, GetUserId());

            return Ok(output);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "The POST call to api/Todos failed. Task value was {Task}.", task);
            return BadRequest();
        }
    }

    /// <summary>
    /// Handles the PUT request to mark a Todo item as completed for the current user.
    /// </summary>
    /// <param name="todoId">The ID of the Todo item to mark as completed.</param>
    /// <returns>An OkResult for success or a BadRequest in case of an exception.</returns>
    //PUT api/Todos/5/Complete
    [HttpPut("{todoId}/Complete")]
    public async Task<IActionResult> Complete(int todoId)
    {
        logger.LogInformation("PUT: api/Todos/{TodoId}/Complete", todoId);

        try
        {
            await data.CompleteTodo(todoId, GetUserId());
            return Ok();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "The PUT call to api/Todos/{TodoId}/Complete failed.", todoId);
            return BadRequest();
        }
    }

    /// <summary>
    /// Handles the PUT request to update a specific Todo item for the current user.
    /// </summary>
    /// <param name="todoId">The ID of the Todo item to update.</param>
    /// <param name="task">The new task description for the Todo item.</param>
    /// <returns>An OkResult for success or a BadRequest in case of an exception.</returns>
    //PUT api/Todos/5
    [HttpPut("{todoId}")]
    public async Task<IActionResult> Put(int todoId, [FromBody] string task)
    {
        logger.LogInformation("PUT: api/Todos/{TodoId} (Task: {Task})", todoId, task);

        try
        {
            await data.UpdateTask(todoId, GetUserId(), task);
            return Ok();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "The PUT call to api/Todos/{TodoId} failed. Task value was: {Task}.",
                todoId, task);
            return BadRequest();
        }
    }

    /// <summary>
    /// Handles the DELETE request to remove a specific Todo item for the current user.
    /// </summary>
    /// <param name="todoId">The ID of the Todo item to delete.</param>
    /// <returns>An OkResult for success or a BadRequest in case of an exception.</returns>
    //DELETE api/Todos/5
    [HttpDelete("{todoId}")]
    public async Task<IActionResult> Delete(int todoId)
    {
        logger.LogInformation("DELETE: api/Todos/{TodoId}", todoId);

        try
        {
            await data.Delete(GetUserId(), todoId);
            return Ok();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "The DELETE call to api/Todos/{TodoId} failed.", todoId);
            return BadRequest();
        }
    }
}
