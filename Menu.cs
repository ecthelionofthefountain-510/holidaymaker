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
        Console.WriteLine("** Choose option **");
        Console.WriteLine("1. Register customer");
        Console.WriteLine("2. Search rooms");
        Console.WriteLine("3. Add room and options");
        Console.WriteLine("4. Update booking");
        Console.WriteLine("5. Cancel booking");
        Console.WriteLine("6. Save booking");
        Console.WriteLine("9. Quit");
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
                    Console.WriteLine("Enter id to show details about one");
                    id = Console.ReadLine();
                    if (id is not null)
                    { 
                        _actions.ShowOne(id);
                    }
                    break;
                case ("3"):
                    
                    break;
                case("4"):
                    Console.WriteLine("Enter id to update booking");
                    id = Console.ReadLine();
                    if (id is not null)
                    { 
                        _actions.UpdateBooking(id);
                    }
                    break;
                case("5"):
                    Console.WriteLine("Enter id to cancel a booking");
                    id = Console.ReadLine();
                    if (id is not null)
                    { 
                        _actions.CancelBooking(id);
                    }
                    break;
                case("9"):
                    Console.WriteLine("Quitting");
                    Environment.Exit(0);
                    break;
            }

            PrintMenu();
        }
        
    }
    
}