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

    
   public async void SearchRoom()
{
    Console.Clear();
    Console.WriteLine("Sök lediga rum");

    // 1. Fråga användaren om sökkriterier
    Console.Write("Ange startdatum (yyyy-MM-dd): ");
    string startDateInput = Console.ReadLine();
    Console.Write("Ange slutdatum (yyyy-MM-dd): ");
    string endDateInput = Console.ReadLine();
    Console.Write("Ange antal personer: ");
    int numberOfGuests = int.Parse(Console.ReadLine());

    Console.WriteLine("Beskriv sällskapet:");
    for (int i = 1; i <= numberOfGuests; i++)
    {
        Console.WriteLine($"Gäst {i}:");
        Console.Write("Förnamn: ");
        string firstName = Console.ReadLine();
        Console.Write("Efternamn: ");
        string lastName = Console.ReadLine();
        Console.Write("Email: ");
        string email = Console.ReadLine();
        Console.Write("Telefonnummer: ");
        string phoneNumber = Console.ReadLine();
        Console.Write("Födelsedatum (yyyy-MM-dd): ");
        string dateOfBirth = Console.ReadLine();

        // Spara gästinformation i databasen
        await using (var cmd = _db.CreateCommand(@"
            INSERT INTO customers (firstname, lastname, email, phone_number, date_of_birth)
            VALUES ($1, $2, $3, $4, $5)
        "))
        {
            cmd.Parameters.AddWithValue(firstName);
            cmd.Parameters.AddWithValue(lastName);
            cmd.Parameters.AddWithValue(email);
            cmd.Parameters.AddWithValue(phoneNumber);
            cmd.Parameters.AddWithValue(DateTime.Parse(dateOfBirth));
            await cmd.ExecuteNonQueryAsync();
        }
    }

    // 2. Fråga om fler specifika kriterier
    Console.Write("Max avstånd till stranden (m): ");
    int maxDistanceToBeach;
    while (!int.TryParse(Console.ReadLine(), out maxDistanceToBeach))
    {
        Console.WriteLine("feeel");
    }
    
    
    Console.Write("Max avstånd till centrum (m): ");
    int maxDistanceToCenter;
    while (!int.TryParse(Console.ReadLine(), out maxDistanceToCenter))
    {
        Console.WriteLine("feeel");
    }
    
        
    
    
    
    Console.Write("Sortera på pris (lågt till högt)? (y/n): ");
    string sortByPriceInput = Console.ReadLine()?.ToLower();
    
    while (string.IsNullOrWhiteSpace(sortByPriceInput) || (sortByPriceInput != "y" && sortByPriceInput != "n"))
    {
        Console.WriteLine("ogiltigt värde");
        sortByPriceInput = Console.ReadLine()?.ToLower();
    }
    bool sortByPrice = sortByPriceInput == "y";
    
    Console.Write("Sortera på omdöme (högt till lågt)? (y/n): ");
    bool sortByRating = Console.ReadLine().ToLower() == "y";

    // 3. Bygg SQL-query
    string query = $@"
        SELECT 
            A.id AS RoomID,
            A.number_of_beds AS Beds,
            A.price AS Price,
            H.name AS Hotel,
            H.distance_to_beach AS DistanceToBeach,
            H.distance_to_center AS DistanceToCenter,
            H.rating AS Rating
        FROM 
            Accommodations A
        JOIN 
            Hotels H ON A.hotel_id = H.id
        WHERE 
            H.distance_to_beach <= {maxDistanceToBeach}
            AND H.distance_to_center <= {maxDistanceToCenter}
            AND A.number_of_beds >= {numberOfGuests}
            AND NOT EXISTS (
                SELECT 1 
                FROM Bookings B 
                WHERE B.accommodations_id = A.id
                AND (B.start_date <= '{DateTime.Parse(endDateInput):yyyy-MM-dd}' AND B.end_date >= '{DateTime.Parse(startDateInput):yyyy-MM-dd}')
            )
        ORDER BY 
            {(sortByPrice ? "A.price ASC" : sortByRating ? "H.rating DESC" : "A.id")};
    ";

    // 4. Exekvera SQL-query och visa resultat
    Console.WriteLine("\n--- Lediga rum ---");
    await using (var cmd = _db.CreateCommand(query))
    await using (var reader = await cmd.ExecuteReaderAsync())
    {
        while (await reader.ReadAsync())
        {
            Console.WriteLine(
                $"Rum {reader["RoomID"]}: {reader["Beds"]} sängar - {reader["Price"]} kr - {reader["Hotel"]} - " +
                $"{reader["DistanceToBeach"]} m från stranden - {reader["DistanceToCenter"]} m från centrum - " +
                $"{reader["Rating"]} betyg"
            );
        }
    }
}
    
    
    
    
}