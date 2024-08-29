using Transaction_Creator_Function;

class Menu
{
    static async Task Main(string[] args)
    {
        Console.WriteLine("Добро пожаловать в Transaction Creator!");
        Console.WriteLine("Эта программа выполняет сбор и проверку информации для совершения транзакций.\n");

        while(true)
        {
            Console.WriteLine("Чтобы начать подготовку данных для транзакции - нажмите кнопку 1");
            Console.WriteLine("Чтобы закрыть программу - нажмите любую другую кнопку");
            ConsoleKeyInfo key = Console.ReadKey(true);

            if(key.Key == ConsoleKey.D1)
            {
                Console.Clear();
                await Transaction_Creator.Transaction_Data_Create();
                Console.WriteLine();
            }
            else
            {
                Environment.Exit(0);
            }
        }
    }
}