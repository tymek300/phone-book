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

        void CreateDatabaseConnection()
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
                    Console.WriteLine("Table 'Persons' created successfully.");
                }
                else
                {
                    connection = new SQLiteConnection($"Data Source={path};Version=3;");
                    connection.Open();
                }

                commandHolder = connection.CreateCommand();
            }
            catch (Exception)
            {
                throw new Exception("Problem ocurred while creating connection with database.");
            }
        }

        void FetchPersonsFromDatabase()
        {
            try
            {
                commandHolder.CommandText = "SELECT * FROM Persons";
                var reader = commandHolder.ExecuteReader();

                personsList.Clear();

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
            catch(Exception)
            {
                throw new Exception("Problem occurred while fetching records from database.");
            }
        }

        public void DisplayListMembers(int elementsAmount = 4)
        {
            Console.Clear();

            char actionKey;

            int index = 1;

            FetchPersonsFromDatabase();

            try
            {
                if (personsList.Count == 0)
                {
                    throw new InvalidOperationException("Table Persons in database are empty. No elements to display.");
                }

                do
                {
                    Console.Clear();

                    Console.WriteLine("Use {< >} to navigate between pages. \nPress any other key to exit.");

                    for (int i = (index - 1) * elementsAmount; i < index * elementsAmount && i < personsList.Count; i++)
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

                    actionKey = Console.ReadKey().KeyChar;

                    if (actionKey == '>')
                    {
                        index = index + 1 > Math.Ceiling(personsList.Count / System.Convert.ToDecimal(elementsAmount)) ? 1 : index + 1;
                    }
                    else if (actionKey == '<')
                    {
                        index = index - 1 < 1 ? (int)Math.Ceiling(personsList.Count / System.Convert.ToDecimal(elementsAmount)) : index - 1;
                    }
                }
                while (actionKey == '<' || actionKey == '>');

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

        public int SelectFromListMembers(int elementsAmount = 4)
        {
            Console.Clear();

            char actionKey;

            int index = 1;

            int selectedPerson = 0;

            FetchPersonsFromDatabase();

            try
            {
                if (personsList.Count == 0)
                {
                    throw new InvalidOperationException("Table Persons in database are empty. No elements to select.");
                }

                while (true)
                {
                    Console.Clear();
                    Console.WriteLine("Use {< >} to navigate between sites and {[ ]} to change selected user.\n Press any other key to exit.");

                    for (int i = (index - 1) * elementsAmount; i < index * elementsAmount && i < personsList.Count; i++)
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

                    actionKey = Console.ReadKey().KeyChar;

                    switch (actionKey)
                    {
                        case '>':
                            index = index + 1 > Math.Ceiling(personsList.Count / 4.0) ? 1 : index + 1;
                            selectedPerson = (index - 1) * elementsAmount;
                            continue;
                        case '<':
                            index = index - 1 < 1 ? (int)Math.Ceiling(personsList.Count / 4.0) : index - 1;
                            selectedPerson = (index - 1) * elementsAmount;
                            continue;
                        case ']':
                            selectedPerson = selectedPerson + 1 >= index * elementsAmount || selectedPerson + 1 >= personsList.Count ? (index - 1) * elementsAmount : selectedPerson + 1;
                            continue;
                        case '[':
                            selectedPerson = selectedPerson - 1 < (index - 1) * elementsAmount ? index * elementsAmount - 1 : selectedPerson - 1;
                            if (selectedPerson > personsList.Count)
                            {
                                selectedPerson = personsList.Count - 1;
                            }
                            continue;
                        case (char)13:
                            Console.Clear();
                            commandHolder.Reset();

                            return personsList[selectedPerson].ID;
                        default:
                            return -1;
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


        Dictionary<string, string> GetValidatedUserInput()
        {
            Dictionary<string, string> userInputs = new Dictionary<string, string>();

            string nameSurnameValidationPattern = "^[A-Z][a-z]+(?:\\s[A-Z][a-z]+)*$";
            Regex nameSurnameValidation = new Regex(nameSurnameValidationPattern);

            string dateValidationPattern = "^(?:\\d{4}[-/]\\d{2}[-/]\\d{2})$";
            Regex dateValidation = new Regex(dateValidationPattern);

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
                        if (!new PhoneAttribute().IsValid(userInputs["phone_number"]) || !new StringLengthAttribute(12).IsValid(userInputs["phone_number"]))
                        {
                            userInputs.Remove("phone_number");
                            throw new ValidationException("Field 'phone_number' must be a correctly provided string. Only numbers and separators are allowed.");
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

        public void AddToList()
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

        public void DeleteFromList()
        {
            Console.Clear();

            int personToDelete;

            char actionKey;



            int selectedOption = 0;

            string[] options = { "YES", "NO" };


            try
            {
                while (true)
                {
                    personToDelete = SelectFromListMembers();

                    if (personToDelete == -1)
                    {
                        commandHolder.Reset();
                        Console.Clear();
                        return;
                    }

                    while (true)
                    {
                        Console.Clear();
                        Console.WriteLine("Use {[ ]} to change selected option.\n Press any other key to exit.");

                        Console.WriteLine($"Are you sure you want to delete user with ID: {personToDelete}");

                        for (int i = 0; i < options.Length; i++)
                        {
                            if (i == selectedOption)
                            {
                                Console.ForegroundColor = ConsoleColor.Green;

                                Console.WriteLine(options[i]);

                                Console.ResetColor();
                                continue;
                            }

                            Console.WriteLine(options[i]);
                        }

                        actionKey = Console.ReadKey().KeyChar;

                        switch (actionKey)
                        {
                            case ']':
                                selectedOption = selectedOption + 1 >= options.Length ? 0 : selectedOption + 1;
                                continue;
                            case '[':
                                selectedOption = selectedOption - 1 < 0 ? options.Length - 1 : selectedOption - 1;
                                continue;
                            case (char)13:
                                if (selectedOption == 0)
                                {
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

                                }
                                else
                                {
                                    commandHolder.Reset();
                                    Console.Clear();
                                    return;
                                }

                                break;
                            default:
                                commandHolder.Reset();
                                Console.Clear();
                                return;
                        }

                        break;
                    }

                    Console.WriteLine("User deleted. Do you want to delete another? Press Y for Yes, any other key to exit.");

                    commandHolder.Reset();

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

        public void ModifyListMember()
        {
            Console.Clear();

            int personToModify;

            char actionKey;


            Dictionary<string, string> userInputs;


            int selectedOption = 0;

            string[] options = { "YES", "NO" };

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

                    while (true)
                    {
                        Console.Clear();
                        Console.WriteLine("Use {[ ]} to change selected option.\n Press any other key to exit.");

                        Console.WriteLine($"Are you sure you want to modify data of user with ID: {personToModify}");

                        for (int i = 0; i < options.Length; i++)
                        {
                            if (i == selectedOption)
                            {
                                Console.ForegroundColor = ConsoleColor.Green;

                                Console.WriteLine(options[i]);

                                Console.ResetColor();
                                continue;
                            }

                            Console.WriteLine(options[i]);
                        }

                        actionKey = Console.ReadKey().KeyChar;

                        switch (actionKey)
                        {
                            case ']':
                                selectedOption = selectedOption + 1 >= options.Length ? 0 : selectedOption + 1;
                                continue;
                            case '[':
                                selectedOption = selectedOption - 1 < 0 ? options.Length - 1 : selectedOption - 1;
                                continue;
                            case (char)13:
                                if (selectedOption == 0)
                                {
                                    try
                                    {
                                        commandHolder.CommandText = $"UPDATEs Persons SET name = @name, surname = @surname, phone_number = @phone_number, mail = @mail, date_of_birth = @date_of_birth WHERE person_id = @personID";

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
                                        Console.WriteLine("An error occurred while modyfying the user: " + ex.Message);
                                        Console.WriteLine("Press any key to continue.");
                                        Console.ReadKey();
                                        return;
                                    }

                                }
                                else
                                {
                                    commandHolder.Reset();
                                    Console.Clear();
                                    return;
                                }

                                break;
                            default:
                                commandHolder.Reset();
                                Console.Clear();
                                return;
                        }

                        break;
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
            while (true)
            {
                try
                {
                    Console.WriteLine("Choose operation:");
                    Console.WriteLine("1. Delete user (selectable)");
                    Console.WriteLine("2. Add new user");
                    Console.WriteLine("3. Display all users");
                    Console.WriteLine("4. Modify user (selectable)");
                    Console.WriteLine("6. Terminate the program");

                    char choice = Console.ReadKey().KeyChar;
                    Console.WriteLine();

                    switch (choice)
                    {
                        case '1':
                            DeleteFromList();
                            break;
                        case '2':
                            AddToList();
                            break;
                        case '3':
                            DisplayListMembers();
                            break;
                        case '4':
                            ModifyListMember();
                            break;
                        case '5':
                            
                            break;
                        case '6':
                            Console.WriteLine("End of the program.");
                            return;
                        default:
                            Console.Clear();
                            Console.WriteLine("Option you selected is not available.");
                            break;
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Unknown error occurred. Please restart the program or contact our support team. Error communicate: {ex.Message}");
                }
               
            }
        }
    }
}
