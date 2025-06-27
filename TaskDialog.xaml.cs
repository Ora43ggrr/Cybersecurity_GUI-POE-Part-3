using System;
using System.Windows;

namespace Cybersecurity_GUI
{
    /// <summary>
    /// Dialog window for adding new tasks
    /// </summary>
    public partial class TaskDialog : Window
    {
        public string TaskTitle { get; private set; }
        public string TaskDescription { get; private set; }
        public DateTime? ReminderTime { get; private set; }

        public TaskDialog()
        {
            InitializeComponent();
            // Set default date to today
            ReminderDatePicker.SelectedDate = DateTime.Today;
        }

        private void AddTaskButton_Click(object sender, RoutedEventArgs e)
        {
            // Validate title is provided
            if (string.IsNullOrWhiteSpace(TitleTextBox.Text))
            {
                MessageBox.Show("Please enter a task title", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            // Store task details
            TaskTitle = TitleTextBox.Text.Trim();
            TaskDescription = DescriptionTextBox.Text.Trim();
            ReminderTime = ReminderDatePicker.SelectedDate;

            // Close dialog with success
            DialogResult = true;
            Close();
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            // Close dialog without saving
            DialogResult = false;
            Close();
        }
    }
}