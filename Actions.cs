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
        await using (var cmd = _db.CreateCommand("INSERT INTO customers (firstname, lastname, email, phone_number, date_of_birth ) VALUES ($1, $2, $3, $4, $5 )"))
        {

            cmd.Parameters.AddWithValue(firstName);
            cmd.Parameters.AddWithValue(lastName);
            cmd.Parameters.AddWithValue(email);
            cmd.Parameters.AddWithValue(phoneNumber);
            cmd.Parameters.AddWithValue(dateOfBirth);

            await cmd.ExecuteNonQueryAsync();
            
        }
        Console.WriteLine("New customer registered.");
    }

    public async void UpdateBooking(string id)
    {
        Console.WriteLine("Enter booking ID to update: ");
        long bookingId = long.Parse(Console.ReadLine());

        Console.WriteLine("Enter new start date (yyyy-mm-dd): ");
        if (!DateTime.TryParse(Console.ReadLine(), out DateTime startDate))
        {
            Console.WriteLine("Invalid date format.");
            return;
        }

        Console.WriteLine("Enter new end date (yyyy-mm-dd): ");
        if (!DateTime.TryParse(Console.ReadLine(), out DateTime endDate))
        {
            Console.WriteLine("Invalid date format.");
            return;
        }

        string query = "UPDATE bookings SET start_date = $1, end_date = $2 WHERE id = $3";
        await using var cmd = _db.CreateCommand(query);
        cmd.Parameters.AddWithValue(startDate);
        cmd.Parameters.AddWithValue(endDate);
        cmd.Parameters.AddWithValue(bookingId);

        await cmd.ExecuteNonQueryAsync();
        Console.WriteLine("Booking updated!");
    }

    public async void CancelBooking(string id)
    {
        Console.WriteLine("Enter booking ID to cancel: ");
        long bookingId = long.Parse(Console.ReadLine());

        string query = "DELETE FROM bookings WHERE id = $1";
        await using var cmd = _db.CreateCommand(query);
        cmd.Parameters.AddWithValue(bookingId);

        await cmd.ExecuteNonQueryAsync();
        Console.WriteLine("Booking canceled.");
    }

    
    
    
}