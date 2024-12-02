    using System;

    namespace holidaymaker;

public class Menu
{
    Actions _actions;
    public Menu(Actions actions)
    {
        _actions = actions;
        PrintMenu();
    }
    
    private void PrintMenu()
    {
        // Features
        Console.WriteLine("Boka resa"); 
        Console.WriteLine("1. Registrera kund/kunder");
        Console.WriteLine("2. Sök lediga rum");
        Console.WriteLine("3. Välj rum och extra alternativ");
        Console.WriteLine("4. Spara bokning");
        Console.WriteLine("5. Avsluta");
        AskUser();
    }

    private async void AskUser()
    {
        var response = Console.ReadLine();
        if (response is not null)
        {
            string? id; // define for multiple use below
            
            switch (response)
            {
                case("1"):
                    _actions.RegCustomer();
                    break;
                case("2"):
                    _actions.SearchRooms();
                    break;
                case("3"):
                    _actions.AddRoomAndOptions();
                    break;
                case("4"):
                    _actions.SaveBooking();
                    break;
                case("5"):
                    Console.WriteLine("Quitting");
                    Environment.Exit(0);
                    break;
            }

            PrintMenu();
        }
        
    }
    
}