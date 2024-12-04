using System;
using System.Data;
using Npgsql;

namespace holidaymaker;

public class Actions
{
    NpgsqlDataSource _db;
    public long? CurrentCustomerId { get; private set; }
    public long? CurrentRoomId { get; private set; }
    public DateTime? CurrentStartDate { get; private set; }
    public DateTime? CurrentEndDate { get; private set; }

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

        var customerId = (long)await cmd.ExecuteScalarAsync();
            
        Console.WriteLine($"Kund tillagd med ID: {customerId}");
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

    CurrentStartDate = startDate;

    Console.WriteLine("Enter end date (yyyy-mm-dd): ");
    if (!DateTime.TryParse(Console.ReadLine(), out DateTime endDate))
    {
        Console.WriteLine("Invalid end date format. Please try again.");
        return;
    }

    CurrentEndDate = endDate;

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
        SELECT a.id, country, h.name, a.number_of_beds, h.rating, a.price, h.distance_to_beach, h.distance_to_center
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
        )ORDER BY a.price ASC, h.rating DESC";

    await using var cmd = _db.CreateCommand(query);
    cmd.Parameters.AddWithValue("@startDate", startDate);
    cmd.Parameters.AddWithValue("@endDate", endDate);
    cmd.Parameters.AddWithValue("@distanceToBeach", distanceToBeach);
    cmd.Parameters.AddWithValue("@distanceToCenter", distanceToCenter);

    await using var reader = await cmd.ExecuteReaderAsync();
    bool roomsFound = false;

    Console.WriteLine("Available rooms:\n");
    while (await reader.ReadAsync())
    {
        roomsFound = true;
        Console.WriteLine(
            $"ID: {reader.GetInt64(0), -5} Country: {reader.GetString(1), -15} Hotel: {reader.GetString(2), -25} Beds: {reader.GetInt32(3), -5} Rating: {reader.GetDouble(4), -5} Price: {reader.GetDouble(5), -8} Distance to beach (m): {reader.GetInt32(6), -10} Distance to center (m): {reader.GetInt32(7), -10}");
    }

    if (!roomsFound)
    {
        Console.WriteLine("No rooms available for the selected dates.");
    }
}

    public async void ShowBooking(int accommodations_id)
    {
        string query =
            @" SELECT h.name, b.start_date, b.end_date, b.booking_price, b.half_board, b.full_board, b.extra_bed
        FROM bookings b 
        JOIN accommodations a ON b.accommodations_id = a.id
        JOIN hotels h ON a.hotel_id = h.id
        WHERE a.id = @accommodations_id";
        
        
        // Skapa och konfigurera kommandot
        await using var cmd = _db.CreateCommand(query);
        cmd.Parameters.AddWithValue("@accommodations_id", accommodations_id); // Binda accommodationId till parameter $1

        // Kör frågan och läs resultaten
        await using var reader = await cmd.ExecuteReaderAsync();
        Console.WriteLine("Bokningar för vald boende:");

        bool hasResults = false;

        while (await reader.ReadAsync())
        {
            hasResults = true;
            Console.WriteLine(
                $"Hotel: {reader.GetString(0)}, " +
                $"Start Date: {reader.GetDateTime(1).ToShortDateString()}, " +
                $"End Date: {reader.GetDateTime(2).ToShortDateString()}, " +
                $"Price: {reader.GetDecimal(3)}, " +
                $"Half Board: {reader.GetBoolean(4)}, " +
                $"Full Board: {reader.GetBoolean(5)}, " +
                $"Extra Bed: {reader.GetBoolean(6)}");
        }

        if (!hasResults)
        {
            Console.WriteLine("Inga bokningar hittades för detta boende.");
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
    string userInput = Console.ReadLine();
    bool extraBed;
    
    switch (userInput)
    {
        case "yes":
            extraBed = true;
            break;
        case "no":
            extraBed = false;
            break;
        default:
            Console.WriteLine("Invalid input. Defaulting to no extra bed.");
            extraBed = false;
            break;
    } ;

    Console.WriteLine("Do you want full pension? (yes/no): ");
    string fullPensionInput = Console.ReadLine();
    bool fullBoard;

    switch (fullPensionInput)
    {
        case "yes":
            fullBoard = true;
            break;
        case "no":
            fullBoard = false;
            break;
        default:
            Console.WriteLine("Invalid input. Defaulting to no full pension.");
            fullBoard = false;
            break;
    }

    bool halfBoard = false;
    if (fullPensionInput == "no")
    {
        Console.WriteLine("Do you want half pension? (yes/no): ");
        string halfBoardInput = Console.ReadLine();
        
        
        switch (halfBoardInput)
        {
            case "yes":
                halfBoard = true;
                break;
            case "no":
                halfBoard = false;
                break;
            default:
                Console.WriteLine("Invalid input. Defaulting to no full pension.");
                halfBoard = false;
                break;
        }
    }
    
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
  

   
    
    