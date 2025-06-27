using System.Collections.Generic;

namespace Cybersecurity_GUI
{
    /// <summary>
    /// Represents a quiz question with options and correct answer
    /// </summary>
    public class QuizQuestion
    {
        public string Question { get; set; }
        public List<string> Options { get; set; }
        public int CorrectAnswerIndex { get; set; }
        public string Explanation { get; set; }

        /// <summary>
        /// Creates a new quiz question
        /// </summary>
        public QuizQuestion(string question, List<string> options, int correctAnswerIndex, string explanation)
        {
            Question = question;
            Options = options;
            CorrectAnswerIndex = correctAnswerIndex;
            Explanation = explanation;
        }
    }
}