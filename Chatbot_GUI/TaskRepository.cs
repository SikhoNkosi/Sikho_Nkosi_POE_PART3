using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;

namespace Chatbot_GUI
{
    // TaskRepository using JSON file storage
    public class TaskRepository
    {
        private string filePath = "tasks.json";
        private List<TaskItem> tasks;

        // Compatibility constructor: some callers may attempt to initialize
        // repository with connection string/provider (from ChatBot). For this
        // simple JSON-backed implementation we ignore those parameters and
        // fall back to file-based storage.
        public TaskRepository(string connectionString, string provider) : this()
        {
            // intentionally left blank
        }

        public TaskRepository()
        {
            if (File.Exists(filePath))
            {
                string json = File.ReadAllText(filePath);
                tasks = JsonConvert.DeserializeObject<List<TaskItem>>(json) ?? new List<TaskItem>();
            }
            else
            {
                tasks = new List<TaskItem>();
            }
        }

        private void SaveChanges()
        {
            string json = JsonConvert.SerializeObject(tasks, Formatting.Indented);
            File.WriteAllText(filePath, json);
        }

        // Add a new task
        public int AddTask(string title, string description, DateTime? reminderDate)
        {
            int newId = tasks.Count > 0 ? tasks[tasks.Count - 1].TaskID + 1 : 1;
            tasks.Add(new TaskItem
            {
                TaskID = newId,
                Title = title,
                Description = description,
                ReminderDate = reminderDate,
                IsCompleted = false
            });
            SaveChanges();
            return newId;
        }

        // Get all tasks
        public List<TaskItem> GetAllTasks()
        {
            return tasks;
        }

        // Get a single task by ID
        public TaskItem GetTaskById(int taskId)
        {
            return tasks.Find(t => t.TaskID == taskId);
        }

        // Update reminder date
        public void UpdateTaskReminder(int taskId, DateTime? reminderDate)
        {
            var task = tasks.Find(t => t.TaskID == taskId);
            if (task != null)
            {
                task.ReminderDate = reminderDate;
                SaveChanges();
            }
        }

        // Mark task as completed
        public void CompleteTask(int taskId)
        {
            var task = tasks.Find(t => t.TaskID == taskId);
            if (task != null)
            {
                task.IsCompleted = true;
                SaveChanges();
            }
        }

        // Delete a task
        public void DeleteTask(int taskId)
        {
            tasks.RemoveAll(t => t.TaskID == taskId);
            SaveChanges();
        }
    }
}

