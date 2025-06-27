using System;
using System.Diagnostics;
using System.IO;
using System.Media;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Cybersecurity_GUI
{
    public partial class MainWindow : Window
    {
        private ChatBot chatbot;
        private readonly string debugLogPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "debug.log");

        public MainWindow()
        {
            InitializeComponent();
            try
            {
                LogDebug("Starting application initialization.");
                chatbot = new ChatBot(this);
                LoadLogo();
                PlayGreetingSound();
                CurrentTimeText.Text = DateTime.Now.ToString("HH:mm:ss");
                LogDebug("Application initialized successfully.");
                UpdateStatus("Ready");
            }
            catch (Exception ex)
            {
                UpdateStatus($"Initialization error: {ex.Message}", true);
                LogDebug($"Initialization error: {ex.Message}");
                MessageBox.Show($"Initialization error: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // =============== UPDATED AUDIO PLAYBACK ===============
        private void PlayGreetingSound()
        {
            try
            {
                string audioPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "greeting.wav");

                if (!File.Exists(audioPath))
                {
                    UpdateStatus("Greeting sound file not found.", true);
                    Debug.WriteLine($"Audio file not found at: {audioPath}");
                    return;
                }

                // Try MediaPlayer first
                try
                {
                    var mediaPlayer = new MediaPlayer();
                    mediaPlayer.Open(new Uri(audioPath));
                    mediaPlayer.Volume = 0.8;
                    mediaPlayer.Play();
                    UpdateStatus("Greeting sound played.");
                    Debug.WriteLine("Audio played via MediaPlayer");
                }
                // Fallback to SoundPlayer
                catch (Exception)
                {
                    new SoundPlayer(audioPath).Play();
                    Debug.WriteLine("Audio played via SoundPlayer fallback");
                }
            }
            catch (Exception ex)
            {
                UpdateStatus($"Audio error: {ex.Message}", true);
                LogDebug($"Audio error: {ex.Message}");
            }
        }
        // =============== END AUDIO SECTION ===============

        private void LogDebug(string message)
        {
            try
            {
                File.AppendAllText(debugLogPath, $"[{DateTime.Now:HH:mm:ss}] {message}\n");
            }
            catch { /* Ignore logging errors */ }
        }

        private void UpdateStatus(string message, bool isError = false)
        {
            StatusText.Text = message;
            StatusText.Foreground = isError ? (SolidColorBrush)FindResource("ErrorColor") : Brushes.White;
            LogDebug($"Status: {message}{(isError ? " (Error)" : "")}");
        }

        private void LoadLogo()
        {
            try
            {
                string logoPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "pic.png");
                if (File.Exists(logoPath))
                {
                    LogoImage.Source = new BitmapImage(new Uri(logoPath));
                    UpdateStatus("Logo loaded successfully.");
                }
                else
                {
                    UpdateStatus("Logo image not found.", true);
                }
            }
            catch (Exception ex)
            {
                UpdateStatus($"Error loading logo: {ex.Message}", true);
                LogDebug($"Error loading logo: {ex.Message}");
            }
        }

        private void ShowView(FrameworkElement view)
        {
            GreetingView.Visibility = Visibility.Collapsed;
            MenuView.Visibility = Visibility.Collapsed;
            view.Visibility = Visibility.Visible;
            UpdateStatus($"Showing view: {view.Name}");
            LogDebug($"Showing view: {view.Name}");
        }

        private void SubmitNameButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string name = UserNameInput.Text.Trim();
                LogDebug($"Name entered: '{name}'");
                if (string.IsNullOrWhiteSpace(name) || !chatbot.IsValidName(name))
                {
                    UpdateStatus("Invalid name! Use letters and spaces only.", true);
                    LogDebug("Invalid name entered.");
                    MessageBox.Show("Please enter a valid name (letters and spaces only).", "Invalid Input", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                chatbot.SaveUserInfo(name);
                UserNameDisplay.Text = $"Name: {name}";
                WelcomeMessage.Text = $"Hello, {name}! I'm here to help you stay safe online.";
                ShowView(MenuView);
                UpdateStatus($"Name saved: {name}. Showing MenuView.");
                LogDebug($"Name saved: {name}. Showing MenuView.");

                AskQuestionsBtn.IsEnabled = true;
                TaskManagementBtn.IsEnabled = true;
                QuizBtn.IsEnabled = true;
                ViewHistoryBtn.IsEnabled = true;
                ActivityLogBtn.IsEnabled = true;
                ExitBtn.IsEnabled = true;
            }
            catch (Exception ex)
            {
                UpdateStatus($"Error saving name: {ex.Message}", true);
                LogDebug($"Error in SubmitNameButton_Click: {ex.Message}");
                MessageBox.Show($"Error: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void AskQuestionsBtn_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                LogDebug("AskQuestionsBtn clicked.");
                UpdateStatus("AskQuestionsBtn clicked.");
                ContentPanel.Children.Clear();

                var textBlock = new TextBlock
                {
                    Text = "You can ask about:\n1. Password Safety\n2. Phishing Attacks\n3. Safe Browsing\n4. General Cybersecurity\n\n" +
                           "You can also:\n- Add tasks (e.g., 'add task to update passwords')\n- Set reminders (e.g., 'remind me in 7 days')\n" +
                           "- Type 'quiz' to start the cybersecurity quiz\n- Type 'history' to view conversation history\n- Type 'exit' to return to menu",
                    Foreground = (SolidColorBrush)FindResource("ChatbotTextColor"),
                    Margin = new Thickness(10),
                    TextWrapping = TextWrapping.Wrap
                };

                var inputBox = new TextBox
                {
                    Name = "UserInputBox",
                    Width = 300,
                    Margin = new Thickness(10, 5, 10, 5)
                };
                inputBox.KeyDown += (s, args) =>
                {
                    if (args.Key == Key.Enter && Keyboard.Modifiers != ModifierKeys.Shift)
                    {
                        args.Handled = true;
                        ProcessUserInput(inputBox);
                    }
                };

                var sendButton = new Button
                {
                    Content = "Send",
                    Width = 80,
                    Margin = new Thickness(10, 5, 10, 5)
                };
                sendButton.Click += (s, args) => ProcessUserInput(inputBox);

                ContentPanel.Children.Add(textBlock);
                ContentPanel.Children.Add(inputBox);
                ContentPanel.Children.Add(sendButton);
                ShowView(MenuView);
                UpdateStatus("Showing chat prompt.");
                LogDebug("Showing chat prompt.");
            }
            catch (Exception ex)
            {
                UpdateStatus($"Error in AskQuestionsBtn_Click: {ex.Message}", true);
                LogDebug($"Error in AskQuestionsBtn_Click: {ex.Message}");
            }
        }

        private void TaskManagementBtn_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                LogDebug("TaskManagementBtn clicked.");
                UpdateStatus("Showing task management options.");

                ContentPanel.Children.Clear();

                var taskOptions = new StackPanel
                {
                    Orientation = Orientation.Vertical,
                    Margin = new Thickness(10)
                };

                var addTaskBtn = new Button
                {
                    Content = "Add New Task",
                    Margin = new Thickness(0, 5, 0, 5),
                    Width = 150
                };
                addTaskBtn.Click += (s, args) => ShowAddTaskDialog();

                var listTasksBtn = new Button
                {
                    Content = "View All Tasks",
                    Margin = new Thickness(0, 5, 0, 5),
                    Width = 150
                };
                listTasksBtn.Click += (s, args) => ShowTaskList();

                var taskInput = new TextBox
                {
                    Margin = new Thickness(0, 10, 0, 5),
                    Width = 300,
                    ToolTip = "Type task commands like 'add task to update passwords by Friday'"
                };
                var processTaskBtn = new Button
                {
                    Content = "Process Task Command",
                    Margin = new Thickness(0, 5, 0, 5),
                    Width = 150
                };
                processTaskBtn.Click += (s, args) => ProcessTaskCommand(taskInput.Text);

                taskOptions.Children.Add(new TextBlock
                {
                    Text = "Task Management",
                    FontSize = 16,
                    FontWeight = FontWeights.Bold,
                    Foreground = (SolidColorBrush)FindResource("ChatbotTextColor"),
                    Margin = new Thickness(0, 0, 0, 10)
                });

                taskOptions.Children.Add(addTaskBtn);
                taskOptions.Children.Add(listTasksBtn);
                taskOptions.Children.Add(new TextBlock
                {
                    Text = "Or enter task command:",
                    Margin = new Thickness(0, 10, 0, 5)
                });
                taskOptions.Children.Add(taskInput);
                taskOptions.Children.Add(processTaskBtn);

                ContentPanel.Children.Add(taskOptions);
                ShowView(MenuView);
            }
            catch (Exception ex)
            {
                UpdateStatus($"Error in TaskManagementBtn_Click: {ex.Message}", true);
                LogDebug($"Error in TaskManagementBtn_Click: {ex.Message}");
            }
        }

        private void ShowAddTaskDialog()
        {
            try
            {
                var dialog = new TaskDialog();
                if (dialog.ShowDialog() == true)
                {
                    string response = chatbot.AddTask(dialog.TaskTitle, dialog.TaskDescription, dialog.ReminderTime);
                    ContentPanel.Children.Add(new TextBlock
                    {
                        Text = $"ChatBot: {response}",
                        Foreground = (SolidColorBrush)FindResource("ChatbotTextColor"),
                        Margin = new Thickness(10, 5, 10, 5),
                        TextWrapping = TextWrapping.Wrap
                    });
                    UpdateStatus("Task added successfully.");
                }
            }
            catch (Exception ex)
            {
                UpdateStatus($"Error adding task: {ex.Message}", true);
                LogDebug($"Error adding task: {ex.Message}");
            }
        }

        private void ShowTaskList()
        {
            try
            {
                string tasks = chatbot.ListTasks();
                ContentPanel.Children.Add(new TextBlock
                {
                    Text = tasks,
                    Foreground = (SolidColorBrush)FindResource("ChatbotTextColor"),
                    Margin = new Thickness(10, 5, 10, 5),
                    TextWrapping = TextWrapping.Wrap
                });
                UpdateStatus("Tasks listed successfully.");
            }
            catch (Exception ex)
            {
                UpdateStatus($"Error listing tasks: {ex.Message}", true);
                LogDebug($"Error listing tasks: {ex.Message}");
            }
        }

        private void ProcessTaskCommand(string command)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(command))
                {
                    UpdateStatus("Empty task command.", true);
                    return;
                }

                string response = chatbot.ProcessUserInput(command);
                ContentPanel.Children.Add(new TextBlock
                {
                    Text = $"ChatBot: {response}",
                    Foreground = (SolidColorBrush)FindResource("ChatbotTextColor"),
                    Margin = new Thickness(10, 5, 10, 5),
                    TextWrapping = TextWrapping.Wrap
                });
                UpdateStatus("Task command processed.");
            }
            catch (Exception ex)
            {
                UpdateStatus($"Error processing task command: {ex.Message}", true);
                LogDebug($"Error processing task command: {ex.Message}");
            }
        }

        private void QuizBtn_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                LogDebug("QuizBtn clicked.");
                UpdateStatus("Starting quiz...");

                ContentPanel.Children.Clear();

                string quizStart = chatbot.StartQuiz();

                var instructions = new TextBlock
                {
                    Text = "Cybersecurity Quiz\n\nAnswer the questions by typing the number of your choice.",
                    FontSize = 16,
                    FontWeight = FontWeights.Bold,
                    Foreground = (SolidColorBrush)FindResource("ChatbotTextColor"),
                    Margin = new Thickness(10),
                    TextAlignment = TextAlignment.Center
                };
                ContentPanel.Children.Add(instructions);

                var questionBlock = new TextBlock
                {
                    Text = quizStart,
                    Foreground = Brushes.White,
                    Margin = new Thickness(10, 5, 10, 5),
                    TextWrapping = TextWrapping.Wrap
                };
                ContentPanel.Children.Add(questionBlock);

                var inputBox = new TextBox
                {
                    Name = "QuizInputBox",
                    Width = 300,
                    Margin = new Thickness(10, 5, 10, 5)
                };
                inputBox.KeyDown += (s, args) =>
                {
                    if (args.Key == Key.Enter)
                    {
                        ProcessQuizAnswer(inputBox);
                    }
                };

                var submitBtn = new Button
                {
                    Content = "Submit Answer",
                    Width = 120,
                    Margin = new Thickness(10, 5, 10, 5)
                };
                submitBtn.Click += (s, args) => ProcessQuizAnswer(inputBox);

                ContentPanel.Children.Add(inputBox);
                ContentPanel.Children.Add(submitBtn);

                ShowView(MenuView);
            }
            catch (Exception ex)
            {
                UpdateStatus($"Error starting quiz: {ex.Message}", true);
                LogDebug($"Error starting quiz: {ex.Message}");
            }
        }

        private void ProcessQuizAnswer(TextBox inputBox)
        {
            try
            {
                if (!int.TryParse(inputBox.Text, out int answer))
                {
                    UpdateStatus("Please enter a valid number.", true);
                    return;
                }

                string response = chatbot.ProcessUserInput(inputBox.Text);
                inputBox.Text = "";

                ContentPanel.Children.Add(new TextBlock
                {
                    Text = $"ChatBot: {response}",
                    Foreground = (SolidColorBrush)FindResource("ChatbotTextColor"),
                    Margin = new Thickness(10, 5, 10, 5),
                    TextWrapping = TextWrapping.Wrap
                });

                if (!chatbot.InQuizMode)
                {
                    ContentPanel.Children.Add(new TextBlock
                    {
                        Text = "Quiz complete! Type 'quiz' to start again or 'menu' to return.",
                        Foreground = (SolidColorBrush)FindResource("ChatbotTextColor"),
                        Margin = new Thickness(10, 10, 10, 5),
                        FontWeight = FontWeights.Bold
                    });
                }

                UpdateStatus("Quiz answer processed.");
            }
            catch (Exception ex)
            {
                UpdateStatus($"Error processing quiz answer: {ex.Message}", true);
                LogDebug($"Error processing quiz answer: {ex.Message}");
            }
        }

        private void ViewHistoryBtn_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                LogDebug("ViewHistoryBtn clicked.");
                UpdateStatus("ViewHistoryBtn clicked.");
                ContentPanel.Children.Clear();
                var history = chatbot.GetConversationHistory();
                if (history.Count == 0)
                {
                    ContentPanel.Children.Add(new TextBlock
                    {
                        Text = "No history yet. Start chatting to build history!",
                        Foreground = (SolidColorBrush)FindResource("ChatbotTextColor"),
                        Margin = new Thickness(10, 5, 10, 5)
                    });
                }
                else
                {
                    foreach (var entry in history)
                    {
                        ContentPanel.Children.Add(new TextBlock
                        {
                            Text = entry,
                            Foreground = Brushes.White,
                            Margin = new Thickness(10, 5, 10, 5)
                        });
                    }
                }
                ShowView(MenuView);
                UpdateStatus("Showing conversation history.");
                LogDebug("Showing conversation history.");
            }
            catch (Exception ex)
            {
                UpdateStatus($"Error in ViewHistoryBtn_Click: {ex.Message}", true);
                LogDebug($"Error in ViewHistoryBtn_Click: {ex.Message}");
            }
        }

        private void ActivityLogBtn_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                LogDebug("ActivityLogBtn clicked.");
                UpdateStatus("Showing activity log.");

                ContentPanel.Children.Clear();
                var log = chatbot.GetActivityLog();

                if (log.Count == 0)
                {
                    ContentPanel.Children.Add(new TextBlock
                    {
                        Text = "No activity logged yet.",
                        Foreground = (SolidColorBrush)FindResource("ChatbotTextColor"),
                        Margin = new Thickness(10, 5, 10, 5)
                    });
                }
                else
                {
                    foreach (var entry in log)
                    {
                        ContentPanel.Children.Add(new TextBlock
                        {
                            Text = entry,
                            Foreground = Brushes.White,
                            Margin = new Thickness(10, 2, 10, 2),
                            FontFamily = new FontFamily("Consolas")
                        });
                    }
                }
                ShowView(MenuView);
            }
            catch (Exception ex)
            {
                UpdateStatus($"Error in ActivityLogBtn_Click: {ex.Message}", true);
                LogDebug($"Error in ActivityLogBtn_Click: {ex.Message}");
            }
        }

        private void ExitBtn_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                LogDebug("ExitBtn clicked.");
                UpdateStatus("ExitBtn clicked. Closing application.");
                chatbot.SaveConversation($"User {chatbot.UserName} exited the application");
                ContentPanel.Children.Clear();
                ContentPanel.Children.Add(new TextBlock
                {
                    Text = $"Goodbye {chatbot.UserName}! Stay safe online.",
                    Foreground = (SolidColorBrush)FindResource("ChatbotTextColor"),
                    Margin = new Thickness(10, 5, 10, 5)
                });
                System.Threading.Tasks.Task.Delay(2000).ContinueWith(_ => Dispatcher.Invoke(() => Application.Current.Shutdown()));
            }
            catch (Exception ex)
            {
                UpdateStatus($"Error in ExitBtn_Click: {ex.Message}", true);
                LogDebug($"Error in ExitBtn_Click: {ex.Message}");
            }
        }

        private async void ProcessUserInput(TextBox inputBox)
        {
            try
            {
                string input = inputBox.Text.Trim();
                if (string.IsNullOrEmpty(input))
                {
                    UpdateStatus("Empty input detected.", true);
                    LogDebug("Empty input detected.");
                    return;
                }

                LogDebug($"User input: '{input}'");
                ContentPanel.Children.Add(new TextBlock
                {
                    Text = $"{chatbot.UserName}: {input}",
                    Foreground = (SolidColorBrush)FindResource("UserTextColor"),
                    Margin = new Thickness(10, 5, 10, 5)
                });
                inputBox.Text = "";

                if (input.ToLower() == "exit")
                {
                    chatbot.SaveConversation($"User {chatbot.UserName} exited the application");
                    ContentPanel.Children.Add(new TextBlock
                    {
                        Text = $"ChatBot: Goodbye {chatbot.UserName}! Stay safe online.",
                        Foreground = (SolidColorBrush)FindResource("ChatbotTextColor"),
                        Margin = new Thickness(10, 5, 10, 5)
                    });
                    UpdateStatus("Exiting via chat input.");
                    LogDebug("Exiting via chat input.");
                    await System.Threading.Tasks.Task.Delay(2000);
                    Application.Current.Shutdown();
                    return;
                }

                if (input.ToLower() == "history")
                {
                    ViewHistoryBtn_Click(null, null);
                    return;
                }

                if (input.ToLower() == "menu")
                {
                    ShowView(MenuView);
                    return;
                }

                UpdateStatus("Processing user input...");
                await System.Threading.Tasks.Task.Delay(1000);
                string response = chatbot.ProcessUserInput(input);
                ContentPanel.Children.Add(new TextBlock
                {
                    Text = $"ChatBot: {response}",
                    Foreground = (SolidColorBrush)FindResource("ChatbotTextColor"),
                    Margin = new Thickness(10, 5, 10, 5),
                    TextWrapping = TextWrapping.Wrap
                });
                UpdateStatus("Response displayed.");
                LogDebug($"Response displayed: {response}");
            }
            catch (Exception ex)
            {
                UpdateStatus($"Error processing input: {ex.Message}", true);
                LogDebug($"Error processing input: {ex.Message}");
                ContentPanel.Children.Add(new TextBlock
                {
                    Text = $"Error: {ex.Message}",
                    Foreground = (SolidColorBrush)FindResource("ErrorColor"),
                    Margin = new Thickness(10, 5, 10, 5)
                });
            }
        }
    }
}