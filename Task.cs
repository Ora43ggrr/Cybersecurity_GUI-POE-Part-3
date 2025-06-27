using System;

namespace Cybersecurity_GUI
{
    /// <summary>
    /// Represents a user task with title, description, and optional reminder
    /// </summary>
    public class Task
    {
        /// <summary>Title/name of the task</summary>
        public string Title { get; set; }

        /// <summary>Detailed description of the task</summary>
        public string Description { get; set; }

        /// <summary>Optional date/time for reminder (null if no reminder)</summary>
        public DateTime? ReminderDate { get; set; }

        /// <summary>Whether the task has been completed</summary>
        public bool IsCompleted { get; set; }

        /// <summary>When the task was created</summary>
        public DateTime CreatedDate { get; }

        /// <summary>
        /// Creates a new task
        /// </summary>
        public Task(string title, string description, DateTime? reminderDate = null)
        {
            Title = title;
            Description = description;
            ReminderDate = reminderDate;
            IsCompleted = false;
            CreatedDate = DateTime.Now;
        }

        /// <summary>
        /// Gets human-readable information about the reminder
        /// </summary>
        public string GetReminderInfo()
        {
            if (!ReminderDate.HasValue) return "No reminder set";

            TimeSpan timeUntilReminder = ReminderDate.Value - DateTime.Now;
            if (timeUntilReminder.TotalDays >= 1)
                return $"Reminder in {(int)timeUntilReminder.TotalDays} days";
            else if (timeUntilReminder.TotalHours >= 1)
                return $"Reminder in {(int)timeUntilReminder.TotalHours} hours";
            else
                return "Reminder due soon!";
        }

        /// <summary>
        /// Formats task for display with emoji icons
        /// </summary>
        public override string ToString()
        {
            string status = IsCompleted ? "✓" : " ";
            string reminderInfo = ReminderDate.HasValue ?
                $"\n   ⏰ {GetReminderInfo()} (Due: {ReminderDate.Value:yyyy-MM-dd})" : "";
            return $"[{status}] {Title}" +
                   $"\n   📝 {Description}" +
                   $"\n   📅 Created: {CreatedDate:yyyy-MM-dd}" +
                   reminderInfo;
        }
    }
}