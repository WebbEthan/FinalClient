public static class ConsoleApp
{
    public static void Start()
    {
        Console.WriteLine("Choose One");
        Console.WriteLine("1. Messanger app");
        Console.WriteLine("2. Rock Paper Scissors");
        Console.WriteLine("3. Quit the program");
        switch(Console.ReadLine())
        {
            case "1":
                new Messager();
                break;
            case "2":
                new RPSClient();
                break;
            case "3":
                ThreadManager.StopThreads();
                break;
        }
    }
}