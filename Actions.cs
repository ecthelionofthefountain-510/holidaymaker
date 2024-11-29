using System;
using Npgsql;

namespace holidaymaker;

public class Actions
{
    NpgsqlDataSource _db;

    public Actions(NpgsqlDataSource db)
    {
        _db = db;
    }
    
    public async void RegCustomer()
    {
        Console.WriteLine("Register a new customer\n");

        Console.WriteLine("1. Enter firstname");
        string? firstName = Console.ReadLine();

        Console.WriteLine("2. Enter lastname");
        string? lastName = Console.ReadLine();

        Console.WriteLine("3. Enter email");
        string? email = Console.ReadLine();

        Console.WriteLine("4. Enter phone number");
        string? phoneNumber = Console.ReadLine();

        Console.WriteLine("5. Enter date of birth (yyyy-mm-dd)");
        string? dateOfBirthInput = Console.ReadLine();

        if (!DateTime.TryParse(dateOfBirthInput, out DateTime dateOfBirth))
        {
            Console.WriteLine("Invalid date format. Please try again");
            return;
        }
        // Insert data
        var query = "INSERT INTO customers (firstname, lastname, email, phone_number, date_of_birth ) VALUES ($1, $2, $3, $4, $5 ) RETURNING id";
        await using var cmd = _db.CreateCommand(query);

            cmd.Parameters.AddWithValue(firstName);
            cmd.Parameters.AddWithValue(lastName);
            cmd.Parameters.AddWithValue(email);
            cmd.Parameters.AddWithValue(phoneNumber);
            cmd.Parameters.AddWithValue(dateOfBirth);

            var customerId = (long)await cmd.ExecuteNonQueryAsync();
            
            Console.WriteLine($"Kund tillagd med ID: {customerId}");
    }

    public async void ListAll()
    {
        await using (var cmd = _db.CreateCommand("SELECT * FROM customers"))
        await using (var reader = await cmd.ExecuteReaderAsync())
        {
            while (await reader.ReadAsync())
            {
                Console.WriteLine($"id: {reader.GetInt32(0)} \t name: {reader.GetString(1)}");
            }
        }
    }

    public async void ShowOne(string id)
    {
        await using (var cmd = _db.CreateCommand("SELECT * FROM items WHERE id = $1"))
        {
            cmd.Parameters.AddWithValue(int.Parse(id));
            await using (var reader = await cmd.ExecuteReaderAsync())
            {
                while (await reader.ReadAsync())
                {
                    Console.WriteLine(
                        $"id: {reader.GetInt32(0)} \t name: {reader.GetString(1)} \t slogan: {reader.GetString(2)}");
                }
            }
        }
    }

    public async void AddOne(string name, string? slogan)
    {
        // Insert data
        await using (var cmd = _db.CreateCommand("INSERT INTO items (name, slogan) VALUES ($1, $2)"))
        {
            cmd.Parameters.AddWithValue(name);
            cmd.Parameters.AddWithValue(slogan);
            await cmd.ExecuteNonQueryAsync();
        }
    }

    public async void UpdateOne(string id)
    {
        Console.WriteLine("Current entry:");
        ShowOne(id);
        Console.WriteLine("Enter updated name (required)");
        var name = Console.ReadLine(); // required
        Console.WriteLine("Enter updated slogan");
        var slogan = Console.ReadLine(); // not required
        if (name is not null)
        {
            // Update data
            await using (var cmd = _db.CreateCommand("UPDATE items SET name = $2, slogan = $3 WHERE id = $1"))
            {
                cmd.Parameters.AddWithValue(int.Parse(id));
                cmd.Parameters.AddWithValue(name);
                cmd.Parameters.AddWithValue(slogan);
                await cmd.ExecuteNonQueryAsync();
            }
        }
    }

    public async void DeleteOne(string id)
    {
        // Delete data
        await using (var cmd = _db.CreateCommand("DELETE FROM items WHERE id = $1"))
        {
            cmd.Parameters.AddWithValue(int.Parse(id));
            await cmd.ExecuteNonQueryAsync();
        }
    }
}