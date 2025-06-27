using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Media;
using System.IO; // Required for Path and File operations

namespace Cybersecurity_GUI
{
    /// <summary>
    /// Main chatbot class handling conversation, quiz, and task management
    /// </summary>
    public class ChatBot
    {
        // Dependencies and state
        private MainWindow mainWindow;
        private Random random = new Random();
        public string UserName { get; private set; }
        private List<string> userInterests = new List<string>();
        private MemoryManager memory = new MemoryManager();
        private List<string> conversationHistory = new List<string>();
        private Dictionary<string, int> responseCounters = new Dictionary<string, int>();
        private List<string> replies = new List<string>();
        private List<string> ignoreWords = new List<string>();

        // Task and quiz management
        private List<Task> tasks = new List<Task>();
        private List<QuizQuestion> quizQuestions = new List<QuizQuestion>();
        public bool InQuizMode { get; private set; } = false;
        private int currentQuizQuestion = 0;
        private int quizScore = 0;

        // Delegates for response handling
        public delegate string ResponseHandler(string sentiment);
        public delegate bool QuestionMatcher(string question);

        public ChatBot(MainWindow window)
        {
            mainWindow = window;
            memory.CheckFiles();  // Ensure data files exist
            StoreIgnoreWords();    // Initialize words to ignore
            StoreReplies();        // Load default responses
            InitializeQuizQuestions(); // Setup quiz questions
            LoadTasks();           // Load saved tasks
            PlayWelcomeSound();    // Play welcome sound
        }

        /// <summary>
        /// Plays welcome sound on startup
        /// </summary>
        
        private void PlayWelcomeSound()
        {
            try
            {
                string path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "greeting.wav");
                if (File.Exists(path))
                {
                    var soundPlayer = new SoundPlayer(path);
                    soundPlayer.Play();
                    memory.LogActivity("Played welcome sound");
                }
            }
            catch (Exception ex)
            {
                memory.LogActivity($"Error playing sound: {ex.Message}");
            }
        }

        /// <summary>
        /// Validates user name contains only letters and spaces
        /// </summary>
        public bool IsValidName(string name)
        {
            return name.All(c => char.IsLetter(c) || c == ' ');
        }

        /// <summary>
        /// Saves user name and logs the event
        /// </summary>
        public void SaveUserInfo(string name)
        {
            UserName = name;
            memory.StoreUserInfo(name, "name");
            SaveConversation($"User entered name: {name}");
        }

        /// <summary>
        /// Returns conversation history
        /// </summary>
        public List<string> GetConversationHistory() => conversationHistory;

        /// <summary>
        /// Returns activity log
        /// </summary>
        public List<string> GetActivityLog() => memory.GetActivityLog();

        /// <summary>
        /// Main method for processing user input
        /// </summary>
        public string ProcessUserInput(string input)
        {
            SaveConversation($"User asked: {input}");
            string sentiment = DetectSentiment(input);
            string response = GenerateResponse(input, sentiment);
            SaveConversation($"Bot responded: {response}");
            return response;
        }

        /// <summary>
        /// Detects sentiment from user input
        /// </summary>
        private string DetectSentiment(string input)
        {
            input = input.ToLower();
            if (input.Contains("worried") || input.Contains("scared") || input.Contains("afraid") ||
                input.Contains("nervous") || input.Contains("anxious"))
                return "worried";
            if (input.Contains("angry") || input.Contains("mad") || input.Contains("frustrated") ||
                input.Contains("annoyed") || input.Contains("upset"))
                return "frustrated";
            if (input.Contains("happy") || input.Contains("excited") || input.Contains("glad") ||
                input.Contains("pleased") || input.Contains("thrilled"))
                return "happy";
            if (input.Contains("sad") || input.Contains("depressed") || input.Contains("unhappy") ||
                input.Contains("miserable") || input.Contains("down"))
                return "sad";
            if (input.Contains("interested") || input.Contains("curious") || input.Contains("want to know") ||
                input.Contains("wondering") || input.Contains("tell me about"))
                return "curious";
            return "neutral";
        }

        /// <summary>
        /// Generates appropriate response based on input and sentiment
        /// </summary>
        private string GenerateResponse(string question, string sentiment)
        {
            // Check for task-related commands first
            if (Regex.IsMatch(question, @"(add|create|set).*task", RegexOptions.IgnoreCase))
            {
                return AddTask(question);
            }

            if (Regex.IsMatch(question, @"(list|show).*tasks?", RegexOptions.IgnoreCase))
            {
                return ListTasks();
            }

            if (Regex.IsMatch(question, @"(complete|finish|done).*task", RegexOptions.IgnoreCase))
            {
                var match = Regex.Match(question, @"\d+");
                if (match.Success && int.TryParse(match.Value, out int taskNum))
                {
                    return CompleteTask(taskNum);
                }
                return "Please specify which task to complete (e.g., 'complete task 1').";
            }

            if (Regex.IsMatch(question, @"(delete|remove).*task", RegexOptions.IgnoreCase))
            {
                var match = Regex.Match(question, @"\d+");
                if (match.Success && int.TryParse(match.Value, out int taskNum))
                {
                    return DeleteTask(taskNum);
                }
                return "Please specify which task to delete (e.g., 'delete task 1').";
            }

            // Check for quiz commands
            if (Regex.IsMatch(question, @"(start|begin|take).*(quiz|test)", RegexOptions.IgnoreCase))
            {
                return StartQuiz();
            }

            if (InQuizMode && Regex.IsMatch(question, @"^\d+$"))
            {
                if (int.TryParse(question, out int answer))
                {
                    return ProcessQuizAnswer(answer);
                }
            }

            // Check for activity log requests
            if (Regex.IsMatch(question, @"(activity|history|log|what have you done)", RegexOptions.IgnoreCase))
            {
                var log = memory.GetActivityLog();
                return "Recent activity log:\n" + string.Join("\n", log);
            }

            // Existing response handlers for cybersecurity topics
            ResponseHandler passwordHandler = (sent) =>
            {
                if (!userInterests.Contains("password"))
                {
                    userInterests.Add("password");
                    memory.StoreUserInfo("password", "interest");
                }
                responseCounters["password"] = responseCounters.GetValueOrDefault("password", 0) + 1;
                var responses = new List<List<string>>
                {
                    new List<string> {
                        "Make sure to use strong, unique passwords for each account.",
                        "A good password should be at least 12 characters long and include numbers, symbols, and both uppercase and lowercase letters.",
                        "Consider using a password manager to keep track of your passwords securely.",
                        "Never share your passwords with anyone, even if they claim to be from tech support."
                    },
                    new List<string> {
                        "Password security is crucial. Did you know a strong password can significantly reduce your risk of being hacked?",
                        "A passphrase can be more secure than a password—combine multiple words for better security.",
                        "Two-factor authentication adds an extra layer of protection to your accounts.",
                        "Avoid using personal information like birthdays or names in your passwords."
                    }
                };
                int responseSet = responseCounters["password"] % responses.Count;
                return AdjustForSentiment(responses[responseSet][random.Next(responses[responseSet].Count)], sent);
            };

            ResponseHandler phishingHandler = (sent) =>
            {
                if (!userInterests.Contains("phishing"))
                {
                    userInterests.Add("phishing");
                    memory.StoreUserInfo("phishing", "interest");
                }
                responseCounters["phishing"] = responseCounters.GetValueOrDefault("phishing", 0) + 1;
                var responses = new List<List<string>>
                {
                    new List<string> {
                        "Be cautious of emails asking for personal information. Scammers often disguise themselves as trusted organizations.",
                        "Phishing emails often create a sense of urgency. Always verify before clicking links or providing information.",
                        "Check the sender's email address carefully—phishing attempts often use addresses that look similar to legitimate ones.",
                        "If an email seems suspicious, don't click any links. Instead, go directly to the company's website."
                    },
                    new List<string> {
                        "Spear phishing targets specific individuals with tailored emails—be extra cautious.",
                        "Some phishing attempts come via text messages, known as smishing.",
                        "Look for poor grammar and spelling, which are common in phishing emails.",
                        "Hover over links to see the actual URL before clicking—phishers often use fake links."
                    }
                };
                int responseSet = responseCounters["phishing"] % responses.Count;
                return AdjustForSentiment(responses[responseSet][random.Next(responses[responseSet].Count)], sent);
            };

            var responseHandlers = new Dictionary<QuestionMatcher, ResponseHandler>
            {
                { q => q.Contains("password"), passwordHandler },
                { q => q.Contains("phishing") || q.Contains("scam"), phishingHandler },
                { q => q.Contains("privacy") || q.Contains("data protection"), (sent) =>
                    {
                        if (!userInterests.Contains("privacy"))
                        {
                            userInterests.Add("privacy");
                            memory.StoreUserInfo("privacy", "interest");
                        }
                        responseCounters["privacy"] = responseCounters.GetValueOrDefault("privacy", 0) + 1;
                        var responses = new List<List<string>>
                        {
                            new List<string> {
                                "Review privacy settings on your social media accounts regularly to control what information is shared.",
                                "Be careful about what personal information you share online—once it's out there, it's hard to take back.",
                                "Use privacy-focused browsers and search engines to minimize tracking of your online activities.",
                                "Consider using a VPN to protect your online privacy, especially on public Wi-Fi networks."
                            }
                        };
                        int responseSet = responseCounters["privacy"] % responses.Count;
                        return AdjustForSentiment(responses[responseSet][random.Next(responses[responseSet].Count)], sent);
                    }
                },
                { q => q.Contains("safe browsing") || q.Contains("browsing"), (sent) =>
                    {
                        if (!userInterests.Contains("safe browsing"))
                        {
                            userInterests.Add("safe browsing");
                            memory.StoreUserInfo("safe browsing", "interest");
                        }
                        responseCounters["safe browsing"] = responseCounters.GetValueOrDefault("safe browsing", 0) + 1;
                        var responses = new List<List<string>>
                        {
                            new List<string> {
                                "Always look for the padlock icon and 'https://' in website URLs.",
                                "Keep your browser updated and avoid downloading files from untrusted sources.",
                                "Avoid entering personal information on untrusted or unknown websites.",
                                "Use browser security features like pop-up blockers and safe browsing modes."
                            }
                        };
                        int responseSet = responseCounters["safe browsing"] % responses.Count;
                        return AdjustForSentiment(responses[responseSet][random.Next(responses[responseSet].Count)], sent);
                    }
                },
                { q => q.Contains("cybersecurity") || q.Contains("security tips"), (sent) =>
                    {
                        if (!userInterests.Contains("cybersecurity"))
                        {
                            userInterests.Add("cybersecurity");
                            memory.StoreUserInfo("cybersecurity", "interest");
                        }
                        responseCounters["cybersecurity"] = responseCounters.GetValueOrDefault("cybersecurity", 0) + 1;
                        var responses = new List<List<string>>
                        {
                            new List<string> {
                                "Keep all software, including operating systems and apps, up to date with the latest security patches.",
                                "Use antivirus software and keep it updated to protect against malware.",
                                "Be cautious about sharing personal information on social media—it can be used by attackers.",
                                "Regularly back up important data to an external drive or cloud service."
                            }
                        };
                        int responseSet = responseCounters["cybersecurity"] % responses.Count;
                        return AdjustForSentiment(responses[responseSet][random.Next(responses[responseSet].Count)], sent);
                    }
                },
                { q => q.Contains("your name") || q.Contains("who are you"), _ => "I'm your Cybersecurity Awareness Chatbot, here to help you stay safe online!" },
                { q => q.Contains("remember") || q.Contains("what do you know"), _ => memory.RecallUserInfo(UserName) },
                { q => q.Contains("interest") || q.Contains("like") || q.Contains("prefer"), _ =>
                    userInterests.Count > 0 ? $"Based on our conversation, you seem interested in: {string.Join(", ", userInterests)}..." :
                                              "You haven't mentioned any specific interests yet..." },
                { q => q.Contains("history"), _ => "You can view our conversation history from the main menu or by typing 'history'." }
            };

            foreach (var handler in responseHandlers)
            {
                if (handler.Key(question.ToLower()))
                {
                    return handler.Value(sentiment);
                }
            }

            return UserReplies(question.ToLower());
        }


        /// <summary>
        /// Adjusts response based on detected sentiment
        /// </summary>
        private string AdjustForSentiment(string response, string sentiment)
        {
            switch (sentiment)
            {
                case "worried":
                    return "I understand this might be concerning. " + response + " Remember, being aware is the first step to staying safe.";
                case "frustrated":
                    return "I hear your frustration. Cybersecurity can be complex, but " + response.ToLower();
                case "happy":
                    return "Great to see your enthusiasm! " + response;
                case "sad":
                    return "I'm sorry you're feeling this way. " + response + " Taking small steps can help improve your security.";
                case "curious":
                    return "That's a great question! " + response;
                default:
                    return response;
            }
        }

        /// <summary>
        /// Generates replies for unrecognized questions
        /// </summary>
        private string UserReplies(string question)
        {
            var words = question.Split(' ', StringSplitOptions.RemoveEmptyEntries)
                               .Where(w => !ignoreWords.Contains(w))
                               .ToList();
            var matchingReplies = replies.Where(r => words.Any(w => r.ToLower().Contains(w)))
                                        .ToList();
            return matchingReplies.Count > 0 ? matchingReplies[random.Next(matchingReplies.Count)] :
                                              "I'm sorry, I don't understand that question. Please ask about cybersecurity topics.";
        }

        /// <summary>
        /// Saves conversation to history
        /// </summary>
        public void SaveConversation(string message)
        {
            conversationHistory.Add($"[{DateTime.Now:HH:mm:ss}] {message}");
            memory.SaveConversation(message);
        }

        /// <summary>
        /// Loads default replies for unrecognized questions
        /// </summary>
        private void StoreReplies()
        {
            replies.Add("Password security requires strong, unique passwords and regular changes.");
            replies.Add("Multi-factor authentication adds an extra layer of security beyond passwords.");
            replies.Add("Phishing attacks often use fake emails to steal sensitive information.");
            replies.Add("Never click on suspicious links or download attachments from unknown emails.");
            replies.Add("Ransomware encrypts files and demands payment for their release.");
            replies.Add("Social engineering manipulates people into revealing confidential information.");
            replies.Add("Malware includes viruses, worms, and trojans that harm computer systems.");
            replies.Add("Avoid entering personal information on untrusted or unknown websites.");
            replies.Add("Always check if a website uses HTTPS before entering sensitive data.");
            replies.Add("I can explain cybersecurity concepts and best practices.");
            replies.Add("Ask me about common cyber threats and how to avoid them.");
            replies.Add("Hello! How can I help with cybersecurity today?");
            replies.Add("Hi there! You can ask me about phishing, online security, or password safety.");
            replies.Add("Phishing emails often have urgent requests or too-good-to-be-true offers.");
            replies.Add("Hover over links to check their real destination before clicking.");
            replies.Add("Keep software updated to protect against known vulnerabilities.");
        }

        /// <summary>
        /// Loads words to ignore when processing questions
        /// </summary>
        private void StoreIgnoreWords()
        {
            ignoreWords.AddRange(new[] {
                "tell", "me", "about", "are", "you", "your", "whats", "can", "i", "ask",
                "", "the", "a", "an", "how", "what", "where", "when", "why", "attacks", "safety"
            });
        }

        /// <summary>
        /// Initializes quiz questions
        /// </summary>
        private void InitializeQuizQuestions()
        {
            quizQuestions.Add(new QuizQuestion(
                "What should you do if you receive an email asking for your password?",
                new List<string> {
                    "Reply with your password",
                    "Delete the email",
                    "Report the email as phishing",
                    "Ignore it"
                },
                2, // Correct answer is "Report the email as phishing"
                "You should never share your password via email. Reporting phishing emails helps protect others."
            ));

            quizQuestions.Add(new QuizQuestion(
                "True or False: Using the same password for multiple accounts is a good security practice.",
                new List<string> { "True", "False" },
                1, // Correct answer is "False"
                "Using unique passwords for each account limits damage if one account is compromised."
            ));

            quizQuestions.Add(new QuizQuestion(
                "Which of these is the strongest password?",
                new List<string> {
                    "password123",
                    "P@ssw0rd!",
                    "CorrectHorseBatteryStaple",
                    "12345678"
                },
                2,
                "Long passphrases are more secure than complex but short passwords."
            ));

            quizQuestions.Add(new QuizQuestion(
                "What does HTTPS in a website URL indicate?",
                new List<string> {
                    "The site has high traffic",
                    "The connection is encrypted",
                    "The site is government-approved",
                    "The site is free to use"
                },
                1,
                "HTTPS ensures your connection to the website is encrypted and secure."
            ));

            quizQuestions.Add(new QuizQuestion(
                "What should you do before connecting to public Wi-Fi?",
                new List<string> {
                    "Disable your firewall",
                    "Use a VPN",
                    "Share your location",
                    "Log in to all your accounts"
                },
                1,
                "A VPN encrypts your traffic on public networks."
            ));

            quizQuestions.Add(new QuizQuestion(
                "How often should you update your software?",
                new List<string> {
                    "Only when it stops working",
                    "When the manufacturer releases updates",
                    "Never, updates break things",
                    "Once every 5 years"
                },
                1,
                "Software updates often include critical security patches."
            ));

            quizQuestions.Add(new QuizQuestion(
                "What is two-factor authentication?",
                new List<string> {
                    "Using two different passwords",
                    "Verifying identity with two different methods",
                    "Having two user accounts",
                    "Logging in from two devices"
                },
                1,
                "2FA adds an extra layer of security beyond just a password."
            ));

            quizQuestions.Add(new QuizQuestion(
                "Where should you store your passwords?",
                new List<string> {
                    "In a text file on your desktop",
                    "In your email inbox",
                    "In a password manager",
                    "On a sticky note under your keyboard"
                },
                2,
                "Password managers securely store and generate strong passwords."
            ));

            quizQuestions.Add(new QuizQuestion(
                "What is phishing?",
                new List<string> {
                    "A fishing sport",
                    "A type of malware",
                    "A fraudulent attempt to obtain sensitive information",
                    "A hardware failure"
                },
                2,
                "Phishing uses deception to trick users into revealing sensitive data."
            ));

            quizQuestions.Add(new QuizQuestion(
                "True or False: You should click on links in emails from unknown senders.",
                new List<string> { "True", "False" },
                1,
                "Links in suspicious emails may lead to malicious websites."
            ));

            memory.LogActivity($"Initialized quiz with {quizQuestions.Count} questions");
        }

        /// <summary>
        /// Loads saved tasks from file
        /// </summary>
        private void LoadTasks()
        {
            try
            {
                tasks = memory.LoadTasks();
                memory.LogActivity("Loaded saved tasks");
            }
            catch (Exception ex)
            {
                SaveConversation($"Error loading tasks: {ex.Message}");
            }
        }

        /// <summary>
        /// Adds a new task with title, description and optional reminder
        /// </summary>
        public string AddTask(string title, string description, DateTime? reminderDate)
        {
            try
            {
                var task = new Task(title, description, reminderDate);
                tasks.Add(task);
                memory.SaveTasks(tasks);
                memory.LogActivity($"Added task: {title}");

                string response = $"Task added successfully!\n\nTitle: {title}\nDescription: {description}";
                if (reminderDate.HasValue)
                {
                    response += $"\nReminder set for: {reminderDate.Value:yyyy-MM-dd}";
                }
                return response;
            }
            catch (Exception ex)
            {
                memory.LogActivity($"Error adding task: {ex.Message}");
                return $"Error adding task: {ex.Message}";
            }
        }

        /// <summary>
        /// Extracts task title from natural language input
        /// </summary>
        private string ExtractTaskTitle(string input)
        {
            // Look for text after task-related keywords
            var match = Regex.Match(input, @"(?:task|add|create)\s*(.*?)(?:\s*description|$)", RegexOptions.IgnoreCase);
            if (match.Success && match.Groups[1].Value.Trim().Length > 0)
            {
                return match.Groups[1].Value.Trim();
            }
            return "New Task"; // Default title if none found
        }

        /// <summary>
        /// Extracts task description from natural language input
        /// </summary>
        private string ExtractTaskDescription(string input)
        {
            // Look for text after "description" keyword
            var match = Regex.Match(input, @"description\s*(.*)", RegexOptions.IgnoreCase);
            if (match.Success)
            {
                return match.Groups[1].Value.Trim();
            }
            return string.Empty; // No description
        }

        /// <summary>
        /// Extracts reminder date from natural language input
        /// </summary>
        private DateTime? ExtractReminderDate(string input)
        {
            // Look for common date patterns
            var match = Regex.Match(input, @"(?:remind|on)\s*(\d{1,2}[/-]\d{1,2}[/-]\d{2,4})", RegexOptions.IgnoreCase);
            if (match.Success && DateTime.TryParse(match.Groups[1].Value, out DateTime date))
            {
                return date;
            }
            return null; // No reminder date found
        }

        /// <summary>
        /// Adds task from natural language input
        /// </summary>
        public string AddTask(string input)
        {
            try
            {
                string title = ExtractTaskTitle(input);
                string description = ExtractTaskDescription(input);
                DateTime? reminderDate = ExtractReminderDate(input);

                return AddTask(title, description, reminderDate);
            }
            catch (Exception ex)
            {
                memory.LogActivity($"Error adding task: {ex.Message}");
                return $"Error adding task: {ex.Message}";
            }
        }

        /// <summary>
        /// Lists all tasks with their details
        /// </summary>
        public string ListTasks()
        {
            if (tasks.Count == 0) return "You have no tasks currently.";

            string response = "Your Current Tasks:\n\n";
            for (int i = 0; i < tasks.Count; i++)
            {
                response += $"{i + 1}. {tasks[i]}\n\n";
            }
            memory.LogActivity("Listed all tasks");
            return response;
        }

        /// <summary>
        /// Marks a task as completed
        /// </summary>
        public string CompleteTask(int taskIndex)
        {
            if (taskIndex < 1 || taskIndex > tasks.Count)
                return "Invalid task number.";

            var task = tasks[taskIndex - 1];
            task.IsCompleted = true;
            memory.SaveTasks(tasks);
            memory.LogActivity($"Completed task: {task.Title}");
            return $"Marked task as completed: '{task.Title}'";
        }

        /// <summary>
        /// Deletes a task
        /// </summary>
        public string DeleteTask(int taskIndex)
        {
            if (taskIndex < 1 || taskIndex > tasks.Count)
                return "Invalid task number.";

            var task = tasks[taskIndex - 1];
            tasks.RemoveAt(taskIndex - 1);
            memory.SaveTasks(tasks);
            memory.LogActivity($"Deleted task: {task.Title}");
            return $"Deleted task: '{task.Title}'";
        }

        /// <summary>
        /// Starts the cybersecurity quiz
        /// </summary>
        public string StartQuiz()
        {
            InQuizMode = true;
            currentQuizQuestion = 0;
            quizScore = 0;
            memory.LogActivity("Started cybersecurity quiz");
            return GetCurrentQuizQuestion();
        }

        /// <summary>
        /// Gets the current quiz question with options
        /// </summary>
        public string GetCurrentQuizQuestion()
        {
            if (!InQuizMode || currentQuizQuestion >= quizQuestions.Count)
                return "No quiz in progress.";

            var question = quizQuestions[currentQuizQuestion];
            string options = string.Join("\n",
                question.Options.Select((o, i) => $"{i + 1}. {o}"));

            return $"Question {currentQuizQuestion + 1}/{quizQuestions.Count}:\n" +
                   $"{question.Question}\n\n" +
                   $"{options}\n\n" +
                   "Enter the number of your answer:";
        }

        /// <summary>
        /// Processes quiz answer and provides feedback
        /// </summary>
        public string ProcessQuizAnswer(int answerIndex)
        {
            if (!InQuizMode) return "No quiz in progress.";
            if (currentQuizQuestion >= quizQuestions.Count) return "Quiz already completed.";

            var question = quizQuestions[currentQuizQuestion];
            bool isCorrect = (answerIndex - 1) == question.CorrectAnswerIndex;

            if (isCorrect)
            {
                quizScore++;
                memory.LogActivity($"Correct answer for question {currentQuizQuestion + 1}");
            }
            else
            {
                memory.LogActivity($"Incorrect answer for question {currentQuizQuestion + 1}");
            }

            currentQuizQuestion++;

            string response = isCorrect ? "Correct! " : "Incorrect. ";
            response += question.Explanation + "\n\n";

            if (currentQuizQuestion < quizQuestions.Count)
            {
                response += GetCurrentQuizQuestion();
            }
            else
            {
                response += $"Quiz complete! Your score: {quizScore}/{quizQuestions.Count}";
                InQuizMode = false;
            }

            return response;
        }
    }
}