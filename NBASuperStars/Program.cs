using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace NBASuperStars
{
    class Program
    {
        const string jsonFilePathDefaultPath = @"../../../NBAJson.json";
        const string csvSaveDefaultPath = @"../../../PlayersFile";

        /// <summary>
        /// The program's entry point
        /// </summary>
        static void Main()
        {
            Program program = new Program();
            var result = program.GetInputData();

            if (!result.isDataCorrect)
            {
                Console.WriteLine("Please provide valid data!");
                return;
            }

            List<Player> players = program.ParseJson(result.jsonFilePath);
            if (players != null)
            {
                IEnumerable<Player> filteredPlayers = program.FilterPlayers(result.maxNumberOfYears, result.minRating, players);
                try
                {
                    program.SaveToSCV(filteredPlayers, result.csvFilePath);
                }
                catch (IOException e)
                {
                    Console.WriteLine($"An exception occured while writing the CSV file: {e.Message}");
                }
                catch (UnauthorizedAccessException uae)
                {
                    Console.WriteLine($"An exception occured while trying to access the path: {uae.Message}");
                }
            }

            Console.ReadKey(true);
        }

        /// <summary>
        /// Method for receiving all the input data
        /// </summary>
        /// <returns>the input data as a value tuple object</returns>
        public (string jsonFilePath, int maxNumberOfYears, int minRating, string csvFilePath, bool isDataCorrect) GetInputData()
        {
            bool isDataCorrect = true;
            Console.Write("Enter a valid path to a JSON file: ");
            string jsonFilePath = Console.ReadLine();

            int maxNumberOfYears = 0;
            Console.Write("Enter the maximum number of years the player should have: ");
            isDataCorrect = int.TryParse(Console.ReadLine(), out maxNumberOfYears);

            int minRating = 0;
            Console.Write("Enter the minimum rating the player should have: ");
            isDataCorrect = int.TryParse(Console.ReadLine(), out minRating);

            Console.Write("Enter a valid path to save the file: ");
            string csvFilePath = Console.ReadLine();

            if (string.IsNullOrEmpty(jsonFilePath))
            {
                jsonFilePath = jsonFilePathDefaultPath;
            }

            if (string.IsNullOrEmpty(csvFilePath))
            {
                csvFilePath = csvSaveDefaultPath;
            }

            return (jsonFilePath, maxNumberOfYears, minRating, csvFilePath, isDataCorrect);
        }

        /// <summary>
        /// Parse JSON data and map it to a specific class' properites
        /// </summary>
        /// <param name="path">the path to the json file</param>
        /// <returns>list of the parsed objects</returns>
        public List<Player> ParseJson(string path)
        {
            if (!File.Exists(path))
            {
                Console.WriteLine("There is no such a file");
                return null;
            }

            List<Player> players = null;
            using (StreamReader r = new StreamReader(path))
            {
                string json = r.ReadToEnd();
                try
                {
                    players = JsonConvert.DeserializeObject<List<Player>>(json);
                }
                catch (JsonException je)
                {
                    Console.WriteLine($"An exception occured while trying to parse the JSON file: {je.Message}");
                }
            }

            return players;
        }

        /// <summary>
        /// Filter all the data
        /// </summary>
        /// <param name="maxYears">the maximum number of years the player has played in the league to qualify.</param>
        /// <param name="minRating">the minimum rating the player should have to qualify.</param>
        /// <param name="fullPlayerCollection">the collection to filter</param>
        /// <returns></returns>
        public IEnumerable<Player> FilterPlayers(int maxYears, int minRating, List<Player> fullPlayerCollection)
        {
            return fullPlayerCollection.Where(player =>
            {
                DateTime date = new DateTime(player.PlayingSince, 1, 1);
                var years = DateTime.Today.Year - date.Year;
                if (years >= maxYears && player.Rating >= minRating)
                {
                    return true;
                }
                return false;
            }).OrderByDescending(p => p.Rating);
        }

        /// <summary>
        /// Save to CSV file
        /// </summary>
        /// <param name="players">the collection of players to save</param>
        /// <param name="path">the path where to save the collection</param>
        public void SaveToSCV(IEnumerable<Player> players, string path)
        {
            //Local function for checking if file exists
            bool CheckFile()
            {
                if (File.Exists(path))
                {
                    Console.Write("There is a file with this name! Would like to override it? Y/N: ");
                    char result = (char)Console.Read();
                    result = char.ToLower(result);

                    if (result == 'y')
                    {
                        return true;
                    }
                    return false;
                }
                return true;
            }

            if (CheckFile())
            {
                using (StreamWriter sw = new StreamWriter(path))
                {
                    sw.WriteLine("Name, Rating");
                    foreach (Player player in players)
                    {
                        sw.WriteLine($"{player.Name}, {player.Rating}");
                    }
                }

                Console.WriteLine($"The file was created successully in directory: {path}!");
                return;
            }
            Console.WriteLine("The file was not created!");
        }
    }
}
