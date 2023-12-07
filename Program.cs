ThreadManager.StartThreads();
Console.WriteLine("Choose One");
Console.WriteLine("1. Messanger app");
Console.WriteLine("2. Rock Paper Scissors");
switch(Console.ReadLine())
{
    case "1":
        new Messager();
        break;
    case "2":
        new RPSClient();
        break;
}