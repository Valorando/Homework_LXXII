using Transaction_Sender_Function;
class Menu
{
    static async Task Main(string[] args)
    {
        Console.WriteLine("Добро пожаловать в Transaction Sender!");
        Console.WriteLine("Эта программа выполняет отправку транзакций в сеть.\n");

        while(true)
        {
            Console.WriteLine("Чтобы начать подготовку к отправке транзакции - нажмите кнопку 1");
            Console.WriteLine("Чтобы закрыть программу - нажмите любую другую кнопку");
            ConsoleKeyInfo key = Console.ReadKey(true);

            if (key.Key == ConsoleKey.D1)
            {
                Console.Clear();
                await Transaction_Sender.Send_Transaction();
                Console.WriteLine();
            }
            else
            {
                Environment.Exit(0);
            }
        }
    }
}