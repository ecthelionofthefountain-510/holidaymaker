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
        Console.WriteLine("**************************************");
        Console.WriteLine("**                                  **");
        Console.WriteLine("** WELCOME TO BOOKINGSIMULATOR 3000 **");
        Console.WriteLine("**                                  **");
        Console.WriteLine("**          Choose a number         **");
        Console.WriteLine("**       1. Register customer       **");
        Console.WriteLine("**       2. Search rooms            **");
        Console.WriteLine("**       3. Add room and options    **");
        Console.WriteLine("**       4. Update booking          **");
        Console.WriteLine("**       5. Cancel booking          **");
        Console.WriteLine("**       6. Save booking            **");
        Console.WriteLine("**       9. Quit                    **");
        Console.WriteLine("**       10. Show booking           **");
        Console.WriteLine("**                                  **");
        Console.WriteLine("**************************************");
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
                    _actions.SearchRooms();
                    _currentStartDate = _actions.CurrentStartDate;
                    _currentEndDate = _actions.CurrentEndDate;
                    break;
                case("3"):
                    _actions.AddRoomAndOptions();
                    break;
                case("4"):
                    _actions.UpdateBooking();
                    break;
                case("5"):
                    _actions.CancelBooking();
                    break;
                case("6"):
                    _actions.SaveBooking();
                    break;
                case("9"):
                    Console.WriteLine("Quitting");
                    Environment.Exit(0);
                    break;
                case("10"):
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