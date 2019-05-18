using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Laborator3.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Laborator3.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TasksController : ControllerBase
    {
        private TasksDbContext context;
        public TasksController(TasksDbContext context)
        {
            this.context = context;
        }

        // GET: api/Tasks
        /// <summary>
        /// Gets all the tasks in the database.
        /// </summary>
        
        /// <param name="from">Optional, filter by minimum deadline.</param>
        /// <param name="to">Optional, filter by maximum deadline.</param>
        /// <returns>A list of Task objects.</returns>
        [HttpGet]
        public IEnumerable<Models.Task> Get([FromQuery]DateTime? from, [FromQuery]DateTime? to)
        {
            IQueryable<Models.Task> result = context.Tasks.Include(t => t.Comments);

            if (from == null && to == null)
                return result;

            if (from != null)
                result = result.Where(t => t.Deadline >= from);

            if (to != null)
                result = result.Where(t => t.Deadline <= to);

            return result;
        }

        // GET: api/Tasks/5
        /// <summary>
        /// Get a task by a given ID.
        /// </summary>
        /// 
        /// <param name="id">Task ID</param>
        /// <returns>A task with a given ID</returns>
        [HttpGet("{id}", Name = "Get")]
        public IActionResult Get(int id)
        {
            var existing = context.Tasks.Include(t => t.Comments).FirstOrDefault(task => task.Id == id);
            if (existing == null)
            {
                return NotFound();
            }

            return Ok(existing);
        }

        // POST: api/Tasks
        /// <summary>
        /// Add a task
        /// </summary>
        /// /// <remarks>
        /// Sample request:
        ///
        ///     POST /tasks
        ///    {
        ///         "title": "Expedia Conference",
        ///         "description": "Attending Expedia Conference at Hilton",
        ///         "dateAdded": "2019-05-17T10:08:57.3616694",
        ///         "deadline": "2019-06-01T12:00:00",
        ///         "taskImportance": 2,
        ///         "taskState": 0,
        ///         "dateClosed": null,
        ///         "comments": [
        ///	            {
        ///		            "text": "Write down some important questions!",
        ///		            "important": true
        ///             },
        ///	            {
        ///		            "text": "Get all the relevant info for the market!",
        ///		            "important": false
        ///	            }
        ///	                    ]
        ///     }
        ///
        /// </remarks>
        /// <param name="task">The task to add.</param>
        /// <response code="201">Returns the newly created item</response>
        /// <response code="400">If the item is null</response>
        [HttpPost]
        [ProducesResponseType(201)]
        [ProducesResponseType(400)]
        public void Post([FromBody] Models.Task task)
        {
            task.DateClosed = null;
            task.DateAdded = DateTime.Now;
            context.Tasks.Add(task);
            context.SaveChanges();
        }

        // PUT: api/Tasks/5
        /// <summary>
        /// Update a task with the given ID, or create a new task if the ID does not exist.
        /// </summary>
        /// <param name="id">task ID</param>
        /// <param name="task">The object Task</param>
        /// <returns>The updated task/new created task.</returns>
        [HttpPut("{id}")]
        public IActionResult Put(int id, [FromBody] Models.Task task)
        {
            var existing = context.Tasks.AsNoTracking().FirstOrDefault(t => t.Id == id);
            if (existing == null)
            {
                task.DateClosed = null;
                task.DateAdded = DateTime.Now;
                context.Tasks.Add(task);
                context.SaveChanges();
                return Ok(task);
            }
            task.Id = id;
            if (task.TaskState == TaskState.Closed && existing.TaskState != TaskState.Closed)
                task.DateClosed = DateTime.Now;
            else if (existing.TaskState == TaskState.Closed && task.TaskState != TaskState.Closed)
                task.DateClosed = null;

            context.Tasks.Update(task);
            context.SaveChanges();
            return Ok(task);
        }

        // DELETE: api/ApiWithActions/5
        /// <summary>
        /// Delet a task by a given ID
        /// </summary>
        /// <param name="id">ID of the task to be deleted.</param>
        /// <returns>The deleted task object.</returns>
        [HttpDelete("{id}")]
        public IActionResult Delete(int id)
        {
            var existing = context.Tasks.Include(t => t.Comments).FirstOrDefault(t => t.Id == id);
            if (existing == null)
            {
                return NotFound();
            }
            context.Tasks.Remove(existing);
            context.SaveChanges();
            return Ok();
        }

    }
}