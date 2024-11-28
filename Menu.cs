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
    
    /* Skapa bokning
     case 1. 
     registrera kunden  //feature/regCustumer
     
     Vart vill du åka?
     svar: land      //feature/listDestination  
     "listar länder,
     om land = list hotels i det landet,  
     mellan vilka datum?
     om hotell = lista alla room på det hotellet"
     
     Vilken stad?
     svar: stad
     
     När vill du åka?
     svar: 
     
     Välj hotell:
     svar: hotell
     
     Visa lediga rum
     Filtrera / avstånd till centrum / avstånd till strand
     / pris lågt-högt / about (pool, restaurant osv)
     
     Välj tillägg
     
     case 2. visa bokning
     case 3. ändra bokning
     case 4. boka!
     case 5. ta bort bokning
*/
    private void PrintMenu()
    {
        // Features
        Console.WriteLine("Boka resa"); 
        Console.WriteLine("1. Registrera kund/kunder");
        Console.WriteLine("2. Välj land?");
        Console.WriteLine("3. Välj hotell");
        Console.WriteLine("4. Filtrera på ratings");
        Console.WriteLine("5. Sök på boenden baserat på avstånd till centrum");
        Console.WriteLine("6. Sök på boenden baserat på avstånd till havet");
        Console.WriteLine("7. Tilläggsval (additional_choices)");
        Console.WriteLine("8. List customers (antal personer, för- och efternamn, email, telefon, födelsedata)");
        Console.WriteLine("9. Söka boenden baserat på pris (lågt till högt)");
        Console.WriteLine("10. Sök lediga rum med specificerade sökkriterier");
        Console.WriteLine("11. Ändra bokning");
        Console.WriteLine("12. Boka!");
        Console.WriteLine("0. Quit");
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
                    Console.WriteLine("Listing all");
                    _actions.ListAll();
                    break;
                case("2"):
                    Console.WriteLine("Enter id to show details about one");
                    id = Console.ReadLine();
                    if (id is not null)
                    { 
                        _actions.ShowOne(id);
                    }
                    break;
                case("3"):
                    Console.WriteLine("Enter name (required)");
                    var name = Console.ReadLine(); // required
                    Console.WriteLine("Enter slogan");
                    var slogan = Console.ReadLine(); // not required
                    if (name is not null)
                    {
                        _actions.AddOne(name, slogan);
                    }
                    break;
                case("4"):
                    Console.WriteLine("Enter id to update one");
                    id = Console.ReadLine();
                    if (id is not null)
                    { 
                        _actions.UpdateOne(id);
                    }
                    break;
                case("5"):
                    Console.WriteLine("Enter id to delete one");
                    id = Console.ReadLine();
                    if (id is not null)
                    { 
                        _actions.DeleteOne(id);
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