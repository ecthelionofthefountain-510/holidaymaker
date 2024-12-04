    using System;

    namespace holidaymaker;

public class Menu
{
    Actions _actions;
    private int? _currentCustomerId;
    private int? _currentRoomId;
    private DateTime? _currentStartDate;
    private DateTime? _currentEndDate;
    public Menu(Actions actions)
    {
        _actions = actions;
        PrintMenu();
    }

    private void PrintMenu()
    {
        Console.WriteLine("\x1b[38;5;32m**************************************\x1b[0m"); // Mjuk grön för ramen
        Console.WriteLine("\x1b[38;5;32m** \x1b[38;5;39m///////////////////////////////\x1b[38;5;32m  **\x1b[0m"); // Mjuk blå för linjerna
        Console.WriteLine("\x1b[40;5;32m** \x1b[38;5;45m      BOOKING SIMULATOR 3000    \x1b[40;5;32m **\x1b[0m"); // Mörkblå för rubriktext
        Console.WriteLine("\x1b[38;5;32m**                                  **\x1b[0m"); 
        Console.WriteLine("\x1b[38;5;32m**          \x1b[38;5;208mChoose a number         \x1b[38;5;32m**\x1b[0m"); // Mjuk orange för instruktioner
        Console.WriteLine("\x1b[38;5;32m**       \x1b[38;5;39m1. Register customer       \x1b[38;5;32m**\x1b[0m"); // Mjuk blå för alternativen
        Console.WriteLine("\x1b[38;5;32m**       \x1b[38;5;39m2. Search rooms            \x1b[38;5;32m**\x1b[0m");
        Console.WriteLine("\x1b[38;5;32m**       \x1b[38;5;39m3. Add room and options    \x1b[38;5;32m**\x1b[0m");
        Console.WriteLine("\x1b[38;5;32m**       \x1b[38;5;39m4. Update booking          \x1b[38;5;32m**\x1b[0m");
        Console.WriteLine("\x1b[38;5;32m**       \x1b[38;5;39m5. Cancel booking          \x1b[38;5;32m**\x1b[0m");
        Console.WriteLine("\x1b[38;5;32m**       \x1b[38;5;39m6. Save booking            \x1b[38;5;32m**\x1b[0m");
        Console.WriteLine("\x1b[38;5;32m**       \x1b[38;5;39m7. Show booking            \x1b[38;5;32m**\x1b[0m");
        Console.WriteLine("\x1b[38;5;32m**       \x1b[38;5;190m8. Quit                   \x1b[38;5;32m**\x1b[0m"); // Röd för avslutning
        Console.WriteLine("\x1b[38;5;32m**                                  **\x1b[0m");
        Console.WriteLine("\x1b[38;5;32m** \x1b[38;5;39m///////////////////////////////\x1b[38;5;32m  **\x1b[0m");
        Console.WriteLine("\x1b[38;5;32m**************************************\x1b[0m");

        AskUser();
    }

    private async Task AskUser()
    {
        var response = Console.ReadLine();
        if (response is not null)
        {
            string? id; // define for multiple use below
            
            switch (response)
            {
                case("1"):
                    Console.Clear();
                    _actions.RegCustomer();
                    break;
                case("2"):
                    Console.Clear();
                    _actions.SearchRooms();
                    _currentStartDate = _actions.CurrentStartDate;
                    _currentEndDate = _actions.CurrentEndDate;
                    break;
                case("3"):
                    Console.Clear();
                    _actions.AddRoomAndOptions();
                    break;
                case("4"):
                    Console.Clear();
                    _actions.UpdateBooking();
                    break;
                case("5"):
                    Console.Clear();
                    _actions.CancelBooking();
                    break;
                case("6"):
                    Console.Clear();
                    _actions.SaveBooking();
                    break;
                case("7"):
                    Console.Clear();
                    Console.Write("Enter Accommodation ID: ");
                    if (int.TryParse(Console.ReadLine(), out int accommodation_id))
                    { 
                        _actions.ShowBooking(accommodation_id); // Call ShowBooking with the parsed ID
                    }
                    else
                    {
                        Console.WriteLine("Invalid ID. Please enter a numeric value.");
                    }
                    break;
                case("8"):
                    Console.Clear();
                    Console.WriteLine("Quitting");
                    Environment.Exit(0);
                    break;
            }

            PrintMenu();
        }
        
    }
    private void ShowCurrentState()
    {
        Console.WriteLine($"Aktuell kund-ID: {_currentCustomerId}");
        Console.WriteLine($"Valt rum-ID: {_currentRoomId}");
        Console.WriteLine($"Startdatum: {_currentStartDate:yyyy-MM-dd}");
        Console.WriteLine($"Slutdatum: {_currentEndDate:yyyy-MM-dd}");
    }
    
}