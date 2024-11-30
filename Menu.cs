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
        Console.WriteLine("2. Search available rooms");
        Console.WriteLine("3. Choose room and extra options");
        Console.WriteLine("4. Show and save booking");
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