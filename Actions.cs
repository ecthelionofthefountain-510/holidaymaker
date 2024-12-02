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
    public async Task SearchRooms()
    {
        Console.WriteLine("Sök lediga rum\n");
        Console.WriteLine("Från datum: (yyyy-mm-dd)");
        string? startDate = Console.ReadLine();
        Console.WriteLine("Till datum: (yyyy-mm-dd)");
        string? endDate = Console.ReadLine();

        
        if (string.IsNullOrWhiteSpace(startDate) || string.IsNullOrWhiteSpace(endDate))
        {
            Console.WriteLine("Ogiltigt datum. Försök igen.");
            return;
        }

        string query = @"
   SELECT accommodations.id, country, hotels.name, accommodations.number_of_beds, accommodations.price
    FROM accommodations
    JOIN hotels ON hotels.id = accommodations.hotel_id
    JOIN locations ON hotels.location_id = locations.id
    LEFT JOIN bookings ON accommodations.id = bookings.accommodations_id
WHERE
        (bookings.start_date IS NULL OR bookings.end_date IS NULL)
        OR (bookings.end_date < @StartDate OR bookings.start_date > @EndDate)
    GROUP BY accommodations.id, country, hotels.name, accommodations.number_of_beds, accommodations.price;
    ";

        // Lägg till parametrarna för datum
        await using var cmd = _db.CreateCommand(query);
        cmd.Parameters.AddWithValue("@StartDate", DateTime.Parse(startDate));
        cmd.Parameters.AddWithValue("@EndDate", DateTime.Parse(endDate));

        await using var reader = await cmd.ExecuteReaderAsync();

        Console.WriteLine("Lediga rum:");
        while (await reader.ReadAsync())
        {
            Console.WriteLine(
                $"ID: {reader.GetInt64(0)}, Country: {reader.GetString(1)}, Hotell: {reader.GetString(2)}, Sängar: {reader.GetInt32(3)}, Pris: {reader.GetDouble(4)}");
        }
    }




    public async void AddRoomAndOptions()
    {
        Console.WriteLine("Enter your customer ID: ");
        long? customerId = long.Parse(Console.ReadLine());
        
        Console.WriteLine("Choose your room (enter ID)");
        long? accommodationId = long.Parse(Console.ReadLine());

        Console.WriteLine("Do you want an extra bed? (yes/no): ");
        bool extraBed = Console.ReadLine()?.ToLower() == "yes";

        Console.WriteLine("Do you want full pension? (yes/no): ");
        bool fullBoard = Console.ReadLine()?.ToLower() == "yes";

        Console.WriteLine("Do you want half pension? (yes/no): ");
        bool halfBoard = Console.ReadLine()?.ToLower() == "yes";

        string query = @"
            INSERT INTO bookings (customer_id, accommodations_id, extra_bed, full_board, half_board) 
            VALUES ($1, $2, $3, $4, $5) 
            RETURNING id";
        
        await using var cmd = _db.CreateCommand(query);
        cmd.Parameters.AddWithValue(customerId);
        cmd.Parameters.AddWithValue(accommodationId);
        cmd.Parameters.AddWithValue(extraBed);
        cmd.Parameters.AddWithValue(fullBoard);
        cmd.Parameters.AddWithValue(halfBoard);
        var bookingId = (long)await cmd.ExecuteScalarAsync();

        Console.WriteLine($"Rooms and extra choices saved into booking ID: {bookingId}");
    }

    public async void SaveBooking()
    {
        Console.WriteLine("Enter customer-ID to booking: ");
        long customerId = long.Parse(Console.ReadLine());

        Console.WriteLine("Enter booking-ID: ");
        long bookingId = long.Parse(Console.ReadLine());

        string query = "INSERT INTO booking_customers (booking_id, customer_id) VALUES ($1, $2)";
        await using var cmd = _db.CreateCommand(query);
        cmd.Parameters.AddWithValue(bookingId);
        cmd.Parameters.AddWithValue(customerId);
        await cmd.ExecuteNonQueryAsync();

        Console.WriteLine("Booking saved!");
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