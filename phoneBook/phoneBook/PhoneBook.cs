﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Xml.Linq;
using System.Data.SQLite;
using System.ComponentModel.DataAnnotations;
using System.Data.SqlClient;
using System.Text.RegularExpressions;
using System.Reflection;

namespace ksiazkaZDanymi
{
    internal class PhoneBook
    {
        private string path;

        private string databaseName;

        private SQLiteConnection connection;

        private SQLiteCommand commandHolder;

        private List<Person> personsList = new List<Person>();

        private int recordsAmount;

        public PhoneBook(string databaseName)
        {
            this.databaseName = databaseName;


            try
            {
                CreateDatabaseConnection();

                ShowMenu();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Unknown error occurred. Please restart the program or contact our support team. Error communicate: {ex.Message}");
            }
            
        }


        //HELPER FUNCTIONS
        private int SelectFromListMembers(int elementsAmount = 4)
        {
            Console.Clear();

            ConsoleKey actionKey;

            int index = 1;

            int selectedPerson = 0;


            try
            {
                FetchPersonsFromDatabase(elementsAmount, 0);

                while (true)
                {
                    Console.Clear();
                    Console.WriteLine("Use {← and →} to navigate between sites and {↑ and ↓} to change selected user.\n Press ESC to exit.");

                    for (int i = 0; i < personsList.Count; i++)
                    {

                        if (i == selectedPerson)
                        {
                            Console.ForegroundColor = ConsoleColor.Green;

                            Console.WriteLine(
                            $"{personsList[i].ID}. {{ \n \t" +
                            $"Name: {personsList[i].Name} \n \t" +
                            $"Surname: {personsList[i].Surname} \n \t" +
                            $"Phone Number: {personsList[i].PhoneNumber} \n \t" +
                            $"Mail: {personsList[i].Email} \n \t" +
                            $"Date of Birth: {personsList[i].DateOfBirth} \n" +
                            $"}}");

                            Console.ResetColor();
                            continue;
                        }

                        Console.WriteLine(
                        $"{personsList[i].ID}. {{ \n \t" +
                        $"Name: {personsList[i].Name} \n \t" +
                        $"Surname: {personsList[i].Surname} \n \t" +
                        $"Phone Number: {personsList[i].PhoneNumber} \n \t" +
                        $"Mail: {personsList[i].Email} \n \t" +
                        $"Date of Birth: {personsList[i].DateOfBirth} \n" +
                        $"}}");

                    }

                    actionKey = Console.ReadKey().Key;

                    switch (actionKey)
                    {
                        case ConsoleKey.RightArrow:
                            index = index + 1 > Math.Ceiling(recordsAmount / System.Convert.ToDecimal(elementsAmount)) ? 1 : index + 1;
                            selectedPerson = 0;

                            FetchPersonsFromDatabase(elementsAmount, (index - 1) * 4);
                            continue;
                        case ConsoleKey.LeftArrow:
                            index = index - 1 < 1 ? (int)Math.Ceiling(recordsAmount / System.Convert.ToDecimal(elementsAmount)) : index - 1;
                            selectedPerson = 0;

                            FetchPersonsFromDatabase(elementsAmount, (index - 1) * 4);
                            continue;
                        case ConsoleKey.UpArrow:
                            selectedPerson = selectedPerson - 1 < 0 ? personsList.Count - 1 : selectedPerson - 1;
                            continue;
                        case ConsoleKey.DownArrow:
                            selectedPerson = selectedPerson + 1 >= personsList.Count ? 0 : selectedPerson + 1;
                            if (selectedPerson > personsList.Count)
                            {
                                selectedPerson = personsList.Count - 1;
                            }
                            continue;
                        case ConsoleKey.Enter:
                            Console.Clear();
                            commandHolder.Parameters.Clear();

                            return personsList[selectedPerson].ID;
                        case ConsoleKey.Escape:
                            return -1;
                        default:
                            continue;
                    }
                }
            }
            catch (InvalidOperationException)
            {
                throw;
            }
            catch (Exception)
            {
                throw;
            }
            finally
            {
                Console.Clear();
            }

        }
        private int SelectFromGivenOptions(List<string> options, List<string> communicates)
        {
            int selectedOption = 0;
            ConsoleKey actionKey;

            communicates.Add("Use {↑ and ↓} to change selected option, ENTER to choose.");
            communicates.Add("Press ESC to exit.");
            communicates.Add("\n");

            while (true)
            {
                Console.Clear();

                foreach (var communicate in communicates)
                {
                    Console.WriteLine(communicate);
                }


                for (int i = 0; i < options.Count; i++)
                {
                    if (i == selectedOption)
                    {
                        Console.ForegroundColor = ConsoleColor.Green;

                        Console.WriteLine(" > " + options[i]);

                        Console.ResetColor();
                        continue;
                    }

                    Console.WriteLine(options[i]);
                }

                actionKey = Console.ReadKey().Key;

                switch (actionKey)
                {
                    case ConsoleKey.DownArrow:
                        selectedOption = selectedOption + 1 >= options.Count ? 0 : selectedOption + 1;
                        continue;
                    case ConsoleKey.UpArrow:
                        selectedOption = selectedOption - 1 < 0 ? options.Count - 1 : selectedOption - 1;
                        continue;
                    case ConsoleKey.Enter:
                        return selectedOption;
                    case ConsoleKey.Escape:
                        return -1;
                    default:
                        continue;
                }
            }
        }
        private Dictionary<string, string> GetValidatedUserInput()
        {
            Dictionary<string, string> userInputs = new Dictionary<string, string>();

            string nameSurnameValidationPattern = @"^[A-ZĄĆĘŁŃÓŚŹŻ][a-ząćęłńóśźż]+$";
            Regex nameSurnameValidation = new Regex(nameSurnameValidationPattern);

            string dateValidationPattern = @"^(19\d{2}|20[01]\d|202[0-4])[-/](0[1-9]|1[0-2])[-/](0[1-9]|[12][0-9]|3[01])$";
            Regex dateValidation = new Regex(dateValidationPattern);

            string phoneNumberValidationPattern = @"^\d{9}$|^\d{3}-\d{3}-\d{3}$";
            Regex phoneNumberValidation = new Regex(phoneNumberValidationPattern);

            while (true)
            {
                try
                {
                    if (!userInputs.ContainsKey("name"))
                    {
                        Console.WriteLine("Enter new user name: ");
                        userInputs["name"] = Console.ReadLine();
                        if (!nameSurnameValidation.IsMatch(userInputs["name"]))
                        {
                            userInputs.Remove("name");
                            throw new ValidationException("Field 'name' must be a correctly provided string. Only letters are allowed.");
                        }
                    }

                    if (!userInputs.ContainsKey("surname"))
                    {
                        Console.WriteLine("Enter new user surname: ");
                        userInputs["surname"] = Console.ReadLine();
                        if (!nameSurnameValidation.IsMatch(userInputs["surname"]))
                        {
                            userInputs.Remove("surname");
                            throw new ValidationException("Field 'surname' must be a correctly provided string. Only letters are allowed.");
                        }
                    }

                    if (!userInputs.ContainsKey("phone_number"))
                    {
                        Console.WriteLine("Enter new user phone number (eg. 222-222-222): ");
                        userInputs["phone_number"] = Console.ReadLine();
                        if (!phoneNumberValidation.IsMatch(userInputs["phone_number"]))
                        {
                            userInputs.Remove("phone_number");
                            throw new ValidationException("Field 'phone_number' must be a correctly provided string. Only numbers and separators ('-') are allowed, maximum length = 9 or 12.");
                        }
                    }

                    if (!userInputs.ContainsKey("mail"))
                    {
                        Console.WriteLine("Enter new user mail: ");
                        userInputs["mail"] = Console.ReadLine();
                        if (!new EmailAddressAttribute().IsValid(userInputs["mail"]))
                        {
                            userInputs.Remove("mail");
                            throw new ValidationException("Field 'mail' must be a correctly provided mail format.");
                        }
                    }

                    if (!userInputs.ContainsKey("date_of_birth"))
                    {
                        Console.WriteLine("Enter new user birth date (eg. 2012-02-12): ");
                        userInputs["date_of_birth"] = Console.ReadLine();
                        if (!dateValidation.IsMatch(userInputs["date_of_birth"]))
                        {
                            userInputs.Remove("date_of_birth");
                            throw new ValidationException("Field 'dateOfBirth' must be a correctly provided date format eg. (YYYY-MM-DD or YYYY/MM/DD).");
                        }
                    }

                    break;
                }
                catch (ValidationException ex)
                {
                    Console.WriteLine("Some provided data are incorrect: " + ex.Message);
                    Console.WriteLine("Enter this field again.");
                    Console.WriteLine("Press any button to continue.");
                    Console.ReadKey();
                    Console.Clear();
                    continue;
                }
                catch (Exception)
                {
                    throw;
                }
            }

            return userInputs;
        }


        //BASIC FUNCTIONS
        private void CreateDatabaseConnection()
        {
            try
            {
                path = Path.GetFullPath(Path.Combine("..", "..", "..", databaseName));

                if (!File.Exists(path))
                {
                    SQLiteConnection.CreateFile(path);
                    Console.WriteLine($"Database file '{databaseName}' created at: {path}");

                    connection = new SQLiteConnection($"Data Source={path};Version=3;");
                    connection.Open();

                    string createTableQuery = @"
                    CREATE TABLE IF NOT EXISTS Persons (
                    person_id INTEGER PRIMARY KEY AUTOINCREMENT,
                    name TEXT NOT NULL,
                    surname TEXT NOT NULL,
                    phone_number TEXT NOT NULL,
                    mail TEXT NOT NULL,
                    date_of_birth TEXT NOT NULL
                    );";

                    commandHolder = new SQLiteCommand(createTableQuery, connection);
                    commandHolder.ExecuteNonQuery();
                    Console.WriteLine("Table 'Persons' created successfully. \nPress any button to continue.");
                    Console.ReadKey();
                }
                else
                {
                    connection = new SQLiteConnection($"Data Source={path};Version=3;");
                    connection.Open();
                }

                commandHolder = connection.CreateCommand();

                commandHolder.CommandText = "SELECT COUNT(*) FROM Persons";
                recordsAmount = System.Convert.ToInt32(commandHolder.ExecuteScalar());

                Console.OutputEncoding = System.Text.Encoding.UTF8;
            }
            catch (Exception)
            {
                throw new Exception("Problem ocurred while creating connection with database.");
            }
        }
        private void FetchPersonsFromDatabase(int limit, int offset, string orderBy = null)
        {
            try
            {
                personsList.Clear();

                commandHolder.CommandText = orderBy == null ? $"SELECT * FROM Persons " : $"SELECT * FROM Persons ORDER BY {orderBy} ";
                commandHolder.CommandText += $"LIMIT {limit} OFFSET {offset}";
                var reader = commandHolder.ExecuteReader();

                while (reader.Read())
                {
                    personsList.Add(Person.CreateUser(
                    Convert.ToInt32(reader[0]),
                    reader[1].ToString(),
                    reader[2].ToString(),
                    reader[3].ToString(),
                    reader[4].ToString(),
                    reader[5].ToString()));
                }

                reader.Close();

            }
            catch(Exception ex)
            {
                throw new Exception("Problem occurred while fetching records from database: \n" + ex.Message);
            }

            if (personsList.Count == 0)
            {
                throw new InvalidOperationException("Table Persons in database are empty. No elements to select/display. You need to add at least one user before using this function.");
            }
        }
        private void DisplayListMembers(bool displaySorted = false, int elementsAmount = 4)
        {
            Console.Clear();

            List<string> columns = new List<string>();
            int selectedOption = 0;

            ConsoleKey actionKey;

            int index = 1;

            try
            {
                FetchPersonsFromDatabase(elementsAmount, 0);

                if (displaySorted == true)
                {
 
                    commandHolder.Reset();
                    commandHolder.CommandText = "PRAGMA table_info(Persons)";
                    var reader = commandHolder.ExecuteReader();

                    while (reader.Read())
                    {
                        columns.Add(reader["name"].ToString());
                    }

                    reader.Close();

                    while (true)
                    {
                        Console.Clear();
                        Console.WriteLine("Select the column by which the records should be sorted.");
                        Console.WriteLine("Use {↑ and ↓} to change selected option, ENTER to choose. Press any other key to exit.");
                        Console.WriteLine();

                        for (int i = 0; i < columns.Count; i++)
                        {
                            if (i == selectedOption)
                            {
                                Console.ForegroundColor = ConsoleColor.Green;

                                Console.WriteLine(" > " + columns[i]);

                                Console.ResetColor();
                                continue;
                            }

                            Console.WriteLine(columns[i]);
                        }

                        actionKey = Console.ReadKey().Key;

                        switch (actionKey)
                        {
                            case ConsoleKey.DownArrow:
                                selectedOption = selectedOption + 1 >= columns.Count ? 0 : selectedOption + 1;
                                continue;
                            case ConsoleKey.UpArrow:
                                selectedOption = selectedOption - 1 < 0 ? columns.Count - 1 : selectedOption - 1;
                                continue;
                            case ConsoleKey.Enter:
                                commandHolder.Reset();
                                break;
                            default:
                                commandHolder.Reset();
                                Console.Clear();
                                return;
                        }

                        break;
                    }
                }

                while (true)
                {
                    Console.Clear();
                    Console.WriteLine("Use {← and →} to navigate between pages. \nPress ESC to exit.");

                    for (int i = 0; i < personsList.Count; i++)
                    {
                        Console.WriteLine(
                        $"{personsList[i].ID}. {{ \n \t" +
                        $"Name: {personsList[i].Name} \n \t" +
                        $"Surname: {personsList[i].Surname} \n \t" +
                        $"Phone Number: {personsList[i].PhoneNumber} \n \t" +
                        $"Mail: {personsList[i].Email} \n \t" +
                        $"Date of Birth: {personsList[i].DateOfBirth} \n" +
                        $"}}");
                    }

                    actionKey = Console.ReadKey().Key;

                    switch (actionKey)
                    {
                        case ConsoleKey.RightArrow:
                            index = index + 1 > Math.Ceiling(recordsAmount / System.Convert.ToDecimal(elementsAmount)) ? 1 : index + 1;
                            break;
                        case ConsoleKey.LeftArrow:
                            index = index - 1 < 1 ? (int)Math.Ceiling(recordsAmount / System.Convert.ToDecimal(elementsAmount)) : index - 1;
                            break;
                        case ConsoleKey.Escape:
                            return;
                        default:
                            continue;

                    }

                    if (displaySorted == true)
                    {
                        FetchPersonsFromDatabase(elementsAmount, (index - 1) * 4, columns[selectedOption]);
                    }
                    else
                    {
                        FetchPersonsFromDatabase(elementsAmount, (index - 1) * 4);
                    }
                }
            }
            catch (InvalidOperationException ex)
            {
                Console.WriteLine(ex.Message);
                Console.WriteLine("Press any key to continue.");
                Console.ReadKey();
            }
            catch(Exception)
            {
                throw;
            }
            finally
            {
                Console.Clear();
            }
        }
        private void AddToList()
        {
            Console.Clear();

            Dictionary<string, string> userInputs = GetValidatedUserInput();

            try
            {
                commandHolder.CommandText = $"INSERT INTO Persons (name, surname, phone_number, mail, date_of_birth) VALUES (@name, @surname, @phone_number, @mail, @date_of_birth)";

                commandHolder.Parameters.AddWithValue("@name", userInputs["name"]);
                commandHolder.Parameters.AddWithValue("@surname", userInputs["surname"]);
                commandHolder.Parameters.AddWithValue("@phone_number", userInputs["phone_number"]);
                commandHolder.Parameters.AddWithValue("@mail", userInputs["mail"]);
                commandHolder.Parameters.AddWithValue("@date_of_birth", userInputs["date_of_birth"]);

                commandHolder.ExecuteNonQuery();

                Console.WriteLine("User successfully added.");
            }
            catch(Exception ex)
            {
                Console.WriteLine("An error occurred while adding the user: " + ex.Message);
            }
            finally
            {
                Console.WriteLine("Press any button to return.");
                Console.ReadKey();
                Console.Clear();

                commandHolder.Reset();
            }
        }
        private void DeleteFromList()
        {
            Console.Clear();

            int personToDelete;


            int selectedOption = 0;
            List<string> options = new List<string> { "YES", "NO" };
            List<string> communicates = new List<string>();

            try
            {
                while (true)
                {
                    personToDelete = SelectFromListMembers();

                    if (personToDelete == -1)
                    {
                        commandHolder.Parameters.Clear();
                        Console.Clear();
                        return;
                    }

                    communicates.Add($"Are you sure you want to delete user with ID: {personToDelete}");

                    selectedOption = SelectFromGivenOptions(options, communicates);

                    switch (selectedOption)
                    {
                        case 0:
                            try
                            {
                                commandHolder.CommandText = "DELETE FROM Persons WHERE person_id = @person_id";
                                commandHolder.Parameters.AddWithValue("@person_id", personToDelete);
                                commandHolder.ExecuteNonQuery();
                            }
                            catch (Exception ex)
                            {
                                Console.WriteLine("An error occurred while deleting the user: " + ex.Message);
                                Console.WriteLine("Press any key to continue.");
                                Console.ReadKey();
                                return;
                            }
                            break;
                        case 1:
                            commandHolder.Parameters.Clear();
                            Console.Clear();
                            return;
                    }

                    Console.WriteLine("User deleted. Do you want to delete another? Press Y for Yes, any other key to exit.");

                    commandHolder.Parameters.Clear();

                    if (Console.ReadKey().Key != ConsoleKey.Y)
                    {
                        break;
                    }
                }
            }
            catch(InvalidOperationException ex)
            {
                Console.WriteLine(ex.Message);
                Console.WriteLine("Press any key to continue.");
                Console.ReadKey();
            }
            catch(Exception)
            {
                throw;
            }
            finally
            {
                Console.Clear();
            }


        }
        private void ModifyListMember()
        {
            Console.Clear();

            int personToModify;


            Dictionary<string, string> userInputs;


            int selectedOption = 0;
            List<string> options = new List<string> { "YES", "NO" };
            List<string> communicates = new List<string>();

            try
            {
                while (true)
                {
                    personToModify = SelectFromListMembers();

                    if (personToModify == -1)
                    {
                        commandHolder.Reset();
                        Console.Clear();
                        return;
                    }

                    userInputs = GetValidatedUserInput();

                    communicates.Add($"Are you sure you want to modify user with ID: {personToModify}");

                    selectedOption = SelectFromGivenOptions(options, communicates);

                    switch (selectedOption)
                    {
                        case 0:
                            try
                            {
                                commandHolder.CommandText = $"UPDATE Persons SET name = @name, surname = @surname, phone_number = @phone_number, mail = @mail, date_of_birth = @date_of_birth WHERE person_id = @personID";

                                commandHolder.Parameters.AddWithValue("@personID", personToModify);
                                commandHolder.Parameters.AddWithValue("@name", userInputs["name"]);
                                commandHolder.Parameters.AddWithValue("@surname", userInputs["surname"]);
                                commandHolder.Parameters.AddWithValue("@phone_number", userInputs["phone_number"]);
                                commandHolder.Parameters.AddWithValue("@mail", userInputs["mail"]);
                                commandHolder.Parameters.AddWithValue("@date_of_birth", userInputs["date_of_birth"]);

                                commandHolder.ExecuteNonQuery();
                            }
                            catch (Exception ex)
                            {
                                Console.WriteLine("An error occurred while modifying the user: " + ex.Message);
                                Console.WriteLine("Press any key to continue.");
                                Console.ReadKey();
                                return;
                            }
                            break;
                        case 1:
                            commandHolder.Parameters.Clear();
                            Console.Clear();
                            return;
                    }

                    Console.WriteLine("User modified. Do you want to modify another person data? Press Y for Yes, any other key to exit.");

                    commandHolder.Reset();

                    if (Console.ReadKey().Key != ConsoleKey.Y)
                    {
                        break;
                    }
                }
            }
            catch (InvalidOperationException ex)
            {
                Console.WriteLine(ex.Message);
                Console.WriteLine("Press any key to continue.");
                Console.ReadKey();
            }
            catch (Exception)
            {
                throw;
            }
            finally
            {
                Console.Clear();
            }
        }
        private void ShowMenu()
        {
            int selectedOption = 0;

            List<string> options = new List<string>
            {
                "Delete user (selectable)",
                "Add new user",
                "Display all users",
                "Modify user (selectable)",
                "Sort users (selectable)",
                "Terminate the program"
            };

            List<string> communicates = new List<string>
            {
                "Welcome to the program 'Phone Book'.",
                "This is simple utility working on MySQLite which provides methods to perform particular operations on records concerned persons in database.",
                "Choose what you want to do by selecting option from the list below."
            };

            while (true)
            {
                try
                {

                    selectedOption = SelectFromGivenOptions(options, communicates);

                    switch (selectedOption)
                    {
                        case 0:
                            DeleteFromList();
                            break;
                        case 1:
                            AddToList();
                            break;
                        case 2:
                            DisplayListMembers();
                            break;
                        case 3:
                            ModifyListMember();
                            break;
                        case 4:
                            DisplayListMembers(true);
                            break;
                        case 5:
                        case -1:
                            Console.WriteLine("End of the program.");
                            return;
                    }

                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Unknown error occurred. Please restart the program or contact our support team. Error communicate: {ex.Message}");
                    break;
                }
            }
        }
    }
}
