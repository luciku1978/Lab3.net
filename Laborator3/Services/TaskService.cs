using Laborator3.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Task = Laborator3.Models.Task;

namespace Laborator3.Services
{
     public interface ITaskService
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="from"></param>
        /// <param name="to"></param>
        /// <returns></returns>
        IEnumerable<Task> GetAll(DateTime? from, DateTime? to);

        Task Create(Task task);

        Task Upsert(int id, Task task);

        Task Delete(int id);

        Task GetById(int id);
    }

    public class TaskService : ITaskService
    {
        private TasksDbContext context;

        public TaskService(TasksDbContext context)
        {
            this.context = context;
        }

        public Task Create(Task task)
        {
            task.DateClosed = null;
            task.DateAdded = DateTime.Now;
            context.Tasks.Add(task);
            context.SaveChanges();
            return task;
        }


        public Task Delete(int id)
        {
            var existing = context.Tasks.Include(t => t.Comments).FirstOrDefault(t => t.Id == id);
            if (existing == null)
            {
                return null;
            }
            context.Tasks.Remove(existing);
            context.SaveChanges();

            return existing;
        }

        public IEnumerable<Task> GetAll(DateTime? from=null, DateTime? to=null)
        {
            IQueryable<Task> result = context.Tasks.Include(t => t.Comments);

            if (from == null && to == null)
                return result;

            if (from != null)
                result = result.Where(t => t.Deadline >= from);

            if (to != null)
                result = result.Where(t => t.Deadline <= to);

            return result;
        }

        public Task GetById(int id)
        {
            // sau context.Tasks.Find()
            return context.Tasks
                .Include(c => c.Comments )
                .FirstOrDefault(t => t.Id == id);
        }

        public Task Upsert(int id, Task task)
        {
            var existing = context.Tasks.AsNoTracking().FirstOrDefault(t => t.Id == id);
            if (existing == null)
            {
                task.DateClosed = null;
                task.DateAdded = DateTime.Now;
                context.Tasks.Add(task);
                context.SaveChanges();
                return task;
            }
            task.Id = id;
            if (task.TaskState == TaskState.Closed && existing.TaskState != TaskState.Closed)
                task.DateClosed = DateTime.Now;
            else if (existing.TaskState == TaskState.Closed && task.TaskState != TaskState.Closed)
                task.DateClosed = null;

            context.Tasks.Update(task);
            context.SaveChanges();
            return task;
        }
    }
}
