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
    
    public async Task RegCustomer()
    {
        Console.WriteLine("Register a new customer\n");

        Console.WriteLine("1. Enter firstname");
        string? firstName = Console.ReadLine();
        if (string.IsNullOrWhiteSpace(firstName))
        {
            Console.WriteLine("Firstname cannot be empty.");
            return;
        }

        Console.WriteLine("2. Enter lastname");
        string? lastName = Console.ReadLine();
        if (string.IsNullOrWhiteSpace(lastName))
        {
            Console.WriteLine("Lastname cannot be empty.");
            return;
        }

        Console.WriteLine("3. Enter email");
        string? email = Console.ReadLine();
        if (string.IsNullOrWhiteSpace(email))
        {
            Console.WriteLine("Email cannot be empty.");
            return;
        }

        Console.WriteLine("4. Enter phone number");
        string? phoneNumber = Console.ReadLine();

        Console.WriteLine("5. Enter date of birth (yyyy-mm-dd)");
        string? dateOfBirthInput = Console.ReadLine();
        if (!DateTime.TryParse(dateOfBirthInput, out DateTime dateOfBirth))
        {
            Console.WriteLine("Invalid date format. Please try again");
            return;
        }

        var query = "INSERT INTO customers (firstname, lastname, email, phone_number, date_of_birth) VALUES ($1, $2, $3, $4, $5)";
        await using (var cmd = _db.CreateCommand(query))
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
    
    public async Task SearchRooms()
{
    Console.WriteLine("Search available rooms\n");

    Console.WriteLine("Enter start date (yyyy-mm-dd): ");
    if (!DateTime.TryParse(Console.ReadLine(), out DateTime startDate))
    {
        Console.WriteLine("Invalid start date format. Please try again.");
        return;
    }

    Console.WriteLine("Enter end date (yyyy-mm-dd): ");
    if (!DateTime.TryParse(Console.ReadLine(), out DateTime endDate))
    {
        Console.WriteLine("Invalid end date format. Please try again.");
        return;
    }

    Console.WriteLine("Enter max distance to beach: (50-2000)");
    if (!int.TryParse(Console.ReadLine(), out int distanceToBeach) || distanceToBeach < 50 || distanceToBeach > 2000)
    {
        Console.WriteLine("Invalid distance to beach. Please enter a value between 50 and 2000.");
        return;
    }

    Console.WriteLine("Enter max distance to center: (50-1200)");
    if (!int.TryParse(Console.ReadLine(), out int distanceToCenter) || distanceToCenter < 50 || distanceToCenter > 1200)
    {
        Console.WriteLine("Invalid distance to center. Please enter a value between 50 and 1200.");
        return;
    }
    
    Console.WriteLine($"Max distance to beach: {distanceToBeach}m");
    Console.WriteLine($"Max distance to center: {distanceToCenter}m");

    string query = @"
        SELECT a.id, country, h.name, a.number_of_beds, a.price, h.distance_to_beach, h.distance_to_center
        FROM accommodations a 
        JOIN hotels h ON h.id = a.hotel_id
        JOIN locations l ON h.location_id = l.id
        WHERE a.is_available = TRUE
        AND h.distance_to_beach <= @distanceToBeach
        AND h.distance_to_center <= @distanceToCenter
        AND a.id NOT IN (
            SELECT accommodations_id
            FROM bookings
            WHERE (start_date, end_date) OVERLAPS (@startDate, @endDate)
        )";

    await using var cmd = _db.CreateCommand(query);
    cmd.Parameters.AddWithValue("@startDate", startDate);
    cmd.Parameters.AddWithValue("@endDate", endDate);
    cmd.Parameters.AddWithValue("@distanceToBeach", distanceToBeach);
    cmd.Parameters.AddWithValue("@distanceToCenter", distanceToCenter);

    await using var reader = await cmd.ExecuteReaderAsync();
    bool roomsFound = false;

    Console.WriteLine("Available rooms:");
    while (await reader.ReadAsync())
    {
        roomsFound = true;
        Console.WriteLine(
            $"ID: {reader.GetInt64(0)}, Country: {reader.GetString(1)}, Hotel: {reader.GetString(2)}, Beds: {reader.GetInt32(3)}, Price: {reader.GetDouble(4)}, Distance to beach: {reader.GetInt32(5)}m, Distance to center: {reader.GetInt32(6)}m");
    }

    if (!roomsFound)
    {
        Console.WriteLine("No rooms available for the selected dates.");
    }
}

public async Task AddRoomAndOptions()
{
    Console.WriteLine("Enter your customer ID: ");
    if (!long.TryParse(Console.ReadLine(), out long customerId))
    {
        Console.WriteLine("Invalid customer ID. Please try again.");
        return;
    }

    Console.WriteLine("Choose your room (enter ID): ");
    if (!long.TryParse(Console.ReadLine(), out long accommodationId))
    {
        Console.WriteLine("Invalid room ID. Please try again.");
        return;
    }

    Console.WriteLine("Enter start date (yyyy-mm-dd): ");
    if (!DateTime.TryParse(Console.ReadLine(), out DateTime startDate))
    {
        Console.WriteLine("Invalid start date format. Please try again.");
        return;
    }

    Console.WriteLine("Enter end date (yyyy-mm-dd): ");
    if (!DateTime.TryParse(Console.ReadLine(), out DateTime endDate))
    {
        Console.WriteLine("Invalid end date format. Please try again.");
        return;
    }

    Console.WriteLine("Do you want an extra bed? (yes/no): ");
    bool extraBed = Console.ReadLine()?.ToLower() == "yes";

    Console.WriteLine("Do you want full pension? (yes/no): ");
    bool fullBoard = Console.ReadLine()?.ToLower() == "yes";

    Console.WriteLine("Do you want half pension? (yes/no): ");
    bool halfBoard = Console.ReadLine()?.ToLower() == "yes";

    string query = @"
        INSERT INTO bookings (customer_id, accommodations_id, start_date, end_date, extra_bed, full_board, half_board) 
        VALUES ($1, $2, $3, $4, $5, $6, $7) 
        RETURNING id";

    await using var cmd = _db.CreateCommand(query);
    cmd.Parameters.AddWithValue(customerId);
    cmd.Parameters.AddWithValue(accommodationId);
    cmd.Parameters.AddWithValue(startDate);
    cmd.Parameters.AddWithValue(endDate);
    cmd.Parameters.AddWithValue(extraBed);
    cmd.Parameters.AddWithValue(fullBoard);
    cmd.Parameters.AddWithValue(halfBoard);

    var bookingId = (long)await cmd.ExecuteScalarAsync();
    Console.WriteLine($"Room and options saved with booking ID: {bookingId}");
}

  

    public async Task UpdateBooking()
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

    public async Task CancelBooking()
    {
        Console.WriteLine("Enter booking ID to cancel: ");
        long bookingId = long.Parse(Console.ReadLine());

        string query = "DELETE FROM bookings WHERE id = $1";
        await using var cmd = _db.CreateCommand(query);
        cmd.Parameters.AddWithValue(bookingId);

        await cmd.ExecuteNonQueryAsync();
        Console.WriteLine("Booking canceled.");
    }

    public async Task SaveBooking()
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
  
}
  

   
    
    