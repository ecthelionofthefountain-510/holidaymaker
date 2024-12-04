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
    
    
public async void AddRoomAndOptions()
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
    
    // Kontrollera om rummet är upptaget
    string availabilityQuery = @"
    SELECT COUNT(*)
    FROM bookings
    WHERE accommodations_id = @accommodationId
      AND (@startDate < end_date AND @endDate > start_date)";
    
    await using var availabilityCmd = _db.CreateCommand(availabilityQuery);
    availabilityCmd.Parameters.AddWithValue("@accommodationId", accommodationId);
    availabilityCmd.Parameters.AddWithValue("@startDate", startDate);
    availabilityCmd.Parameters.AddWithValue("@endDate", endDate);

    var existingBookings = (long)await availabilityCmd.ExecuteScalarAsync();
    if (existingBookings > 0)
    {
        Console.WriteLine("The room is occupied on the selected dates. Please choose another room.");
        return;
    }

    
    Console.WriteLine("Do you want an extra bed? (yes/no): ");
    string userInput = Console.ReadLine();
    bool extraBed = userInput.ToLower() switch
    {
        "yes" => true,
        "no" => false,
        _ => false
    };

    if (userInput.ToLower() != "yes" && userInput.ToLower() != "no")
    {
        Console.WriteLine("Invalid input. Defaulting to no extra bed.");
    }

    Console.WriteLine("Do you want full pension? (yes/no): ");
    string fullPensionInput = Console.ReadLine();
    bool fullBoard = fullPensionInput.ToLower() switch
    {
        "yes" => true,
        "no" => false,
        _ => false
    };

    if (fullPensionInput.ToLower() != "yes" && fullPensionInput.ToLower() != "no")
    {
        Console.WriteLine("Invalid input. Defaulting to no full pension.");
    }

// Hantera "half pension" bara om full pension är "no"
    bool halfBoard = false;
    if (!fullBoard)
    {
        Console.WriteLine("Do you want half pension? (yes/no): ");
        string halfBoardInput = Console.ReadLine();
        halfBoard = halfBoardInput.ToLower() switch
        {
            "yes" => true,
            "no" => false,
            _ => false
        };

        if (halfBoardInput.ToLower() != "yes" && halfBoardInput.ToLower() != "no")
        {
            Console.WriteLine("Invalid input. Defaulting to no half pension.");
        }
    }

    Console.WriteLine($"Extra Bed: {extraBed}, Full Pension: {fullBoard}, Half Pension: {halfBoard}");
    
    string priceQuery = @"
    SELECT a.price + h.price AS booking_price
    FROM accommodations a
    JOIN hotels h ON a.hotel_id = h.id
    WHERE a.id = @accommodationId";
    
    await using var priceCmd = _db.CreateCommand(priceQuery);
    priceCmd.Parameters.AddWithValue("@accommodationId", accommodationId);
    
    object? result = await priceCmd.ExecuteScalarAsync();
    if (result == null || !(result is double bookingPrice))
    {
        Console.WriteLine("Failed to retrieve the booking price. Please try again.");
        return;
    }
    
    string query = @"
        INSERT INTO bookings (customer_id, accommodations_id, start_date, end_date, booking_price, extra_bed, full_board, half_board) 
        VALUES (@customerId, @accommodationId, @startDate, @endDate, @bookingPrice, @extraBed, @fullBoard, @halfBoard) 
        RETURNING id";

    await using var cmd = _db.CreateCommand(query);
    cmd.Parameters.AddWithValue("@customerId", customerId);
    cmd.Parameters.AddWithValue("@accommodationId", accommodationId);
    cmd.Parameters.AddWithValue("@startDate", startDate);
    cmd.Parameters.AddWithValue("@endDate", endDate);
    cmd.Parameters.AddWithValue("@bookingPrice", bookingPrice);
    cmd.Parameters.AddWithValue("@extraBed", extraBed);
    cmd.Parameters.AddWithValue("@fullBoard", fullBoard);
    cmd.Parameters.AddWithValue("@halfBoard", halfBoard);

    var bookingId = (long)await cmd.ExecuteScalarAsync();
    Console.WriteLine($"Room and options saved with booking ID: {bookingId}, Total Price: {bookingPrice}");
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
  

   
    
    