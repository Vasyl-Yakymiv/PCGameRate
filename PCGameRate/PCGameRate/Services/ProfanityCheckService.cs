using PCGameRate.Interfaces;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace PCGameRate.Services
{
    public class ProfanityCheckService : IProfanityCheckService
    {
        private readonly HashSet<string> _badWords;

        public ProfanityCheckService()
        {
            _badWords = LoadBadWordsFromFile("wwwroot/profanity/profanity_list.txt");
        }

        private HashSet<string> LoadBadWordsFromFile(string filePath)
        {
            var badWords = new HashSet<string>();

            try
            {
                var lines = File.ReadAllLines(filePath);
                foreach (var line in lines)
                {
                    var word = line.Trim();
                    if (!string.IsNullOrEmpty(word))
                    {
                        badWords.Add(word.ToLower());
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading bad words from file: {ex.Message}");
            }

            return badWords;
        }
        public bool ContainsProfanity(string text)
        {
            if (string.IsNullOrEmpty(text))
                return false;

            var words = text.ToLower().Split(new[] { ' ', '\n', '\r', '.', ',', ';', '!', '?' }, StringSplitOptions.RemoveEmptyEntries);

            foreach (var word in words)
            {
                if (_badWords.Contains(word))
                {
                    return true;
                }
            }

            return false;
        }
    }
}
