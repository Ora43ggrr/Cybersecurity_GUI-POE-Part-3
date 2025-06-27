using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Cybersecurity_GUI
{
    /// <summary>
    /// Handles all data persistence including conversation history, tasks, and activity logs
    /// </summary>
    public class MemoryManager
    {
        // File paths for storing data
        private readonly string memoryFilePath;  // Stores conversation history
        private readonly string tasksFilePath;   // Stores user tasks
        private readonly string activityLogPath; // Stores activity log

        public MemoryManager()
        {
            // Initialize file paths in the application directory
            string baseDir = AppDomain.CurrentDomain.BaseDirectory;
            memoryFilePath = Path.Combine(baseDir, "ChatbotMemory.txt");
            tasksFilePath = Path.Combine(baseDir, "TasksData.txt");
            activityLogPath = Path.Combine(baseDir, "ActivityLog.txt");

            // Ensure files exist when manager is created
            CheckFiles();
        }

        /// <summary>
        /// Creates data files if they don't exist
        /// </summary>
        public void CheckFiles()
        {
            try
            {
                if (!File.Exists(memoryFilePath)) File.Create(memoryFilePath).Close();
                if (!File.Exists(tasksFilePath)) File.Create(tasksFilePath).Close();
                if (!File.Exists(activityLogPath)) File.Create(activityLogPath).Close();
            }
            catch (Exception ex)
            {
                throw new IOException($"Error creating data files: {ex.Message}");
            }
        }

        /// <summary>
        /// Stores user information in memory file
        /// </summary>
        public void StoreUserInfo(string info, string infoType)
        {
            try
            {
                // Read all existing lines
                var existingLines = new List<string>(File.ReadAllLines(memoryFilePath));
                bool found = false;

                // Update existing entry if found
                for (int i = 0; i < existingLines.Count; i++)
                {
                    if (existingLines[i].StartsWith($"{infoType}:"))
                    {
                        existingLines[i] = $"{infoType}:{info}";
                        found = true;
                        break;
                    }
                }

                // Add new entry if not found
                if (!found)
                {
                    existingLines.Add($"{infoType}:{info}");
                }

                // Write all lines back to file
                File.WriteAllLines(memoryFilePath, existingLines);
                LogActivity($"Stored user info: {infoType}={info}");
            }
            catch (Exception ex)
            {
                throw new IOException($"Error storing information: {ex.Message}");
            }
        }

        /// <summary>
        /// Recalls stored user information
        /// </summary>
        public string RecallUserInfo(string userName)
        {
            try
            {
                var lines = File.ReadAllLines(memoryFilePath);
                // Get all stored interests
                var interests = lines.Where(line => line.StartsWith("interest:"))
                                    .Select(line => line.Substring("interest:".Length))
                                    .ToList();

                LogActivity($"Recalled user info for {userName}");

                return interests.Count > 0
                    ? $"I remember you're interested in {string.Join(" and ", interests)}. Would you like to know more about these topics?"
                    : "I don't have any specific information stored about your interests yet.";
            }
            catch (Exception ex)
            {
                throw new IOException($"Error recalling information: {ex.Message}");
            }
        }

        /// <summary>
        /// Saves conversation messages with timestamps
        /// </summary>
        public void SaveConversation(string message)
        {
            try
            {
                File.AppendAllLines(memoryFilePath, new[] { $"[{DateTime.Now:HH:mm:ss}] {message}" });
                LogActivity($"Saved conversation: {message}");
            }
            catch (Exception ex)
            {
                throw new IOException($"Error saving conversation: {ex.Message}");
            }
        }

        /// <summary>
        /// Saves all tasks to file
        /// </summary>
        public void SaveTasks(List<Task> tasks)
        {
            try
            {
                // Format: Title|Description|ReminderDate|IsCompleted
                var lines = tasks.Select(t =>
                    $"{t.Title}|{t.Description}|{(t.ReminderDate.HasValue ? t.ReminderDate.Value.ToString("o") : "null")}|{t.IsCompleted}"
                ).ToList();

                File.WriteAllLines(tasksFilePath, lines);
                LogActivity($"Saved {tasks.Count} tasks");
            }
            catch (Exception ex)
            {
                throw new IOException($"Error saving tasks: {ex.Message}");
            }
        }

        /// <summary>
        /// Loads tasks from file
        /// </summary>
        public List<Task> LoadTasks()
        {
            try
            {
                var tasks = new List<Task>();
                foreach (var line in File.ReadAllLines(tasksFilePath))
                {
                    if (string.IsNullOrWhiteSpace(line)) continue;

                    // Parse task components
                    var parts = line.Split('|');
                    DateTime? reminderDate = parts[2] == "null" ? null : DateTime.Parse(parts[2]);

                    // Create task object
                    var task = new Task(parts[0], parts[1], reminderDate)
                    {
                        IsCompleted = bool.Parse(parts[3])
                    };
                    tasks.Add(task);
                }

                LogActivity($"Loaded {tasks.Count} tasks");
                return tasks;
            }
            catch (Exception ex)
            {
                throw new IOException($"Error loading tasks: {ex.Message}");
            }
        }

        /// <summary>
        /// Logs system activities
        /// </summary>
        public void LogActivity(string activity)
        {
            try
            {
                File.AppendAllText(activityLogPath, $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] {activity}\n");
            }
            catch
            {
                // Silently fail if logging isn't working
            }
        }

        /// <summary>
        /// Retrieves activity log entries
        /// </summary>
        public List<string> GetActivityLog(int maxEntries = 10)
        {
            try
            {
                return File.ReadAllLines(activityLogPath)
                    .Reverse() // Newest first
                    .Take(maxEntries)
                    .Reverse() // Restore original order
                    .ToList();
            }
            catch
            {
                return new List<string> { "Could not load activity log" };
            }
        }
    }
}