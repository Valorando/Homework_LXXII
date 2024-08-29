using Nethereum.Hex.HexTypes;
using Nethereum.RPC.Eth.DTOs;
using Nethereum.Web3;
using Nethereum.Web3.Accounts;
using System.Net;
using System.Net.Sockets;
using System.Text;

class Transaction_Signer
{
    static async Task Main(string[] args)
    {
        Console.WriteLine("Добро пожаловать в автономный модуль Transaction Signer!");
        Console.WriteLine("Эта программа выполняет построение и подписание транзакций.\n");

        IPAddress ipAddress = IPAddress.Parse("127.0.0.1");
        int port = 8888;

        TcpListener listener = new TcpListener(ipAddress, port);
        listener.Start();

        while (true)
        {
            try
            {
                TcpClient client = await listener.AcceptTcpClientAsync();
                Console.WriteLine("[Успешно]: Подключение с Transaction Sender установлено.");

                _ = ProcessClientAsync(client);
            }
            catch
            {
                Console.WriteLine($"[Ошибка]: Подключение с Transaction Sender не установлено.");
            }
        }
    }

    private static async Task ProcessClientAsync(TcpClient client)
    {
        try
        {
            Console.WriteLine("[Внимание]: Ожидается получение данных от Transaction Sender...");

            using (NetworkStream stream = client.GetStream())
            {
                byte[] data = new byte[256];
                int bytes = await stream.ReadAsync(data, 0, data.Length);
                string message = Encoding.UTF8.GetString(data, 0, bytes);

                Console.WriteLine("[Успешно]: Данные от Transaction Sender получены.");

                string[] parts = message.Split('|');

                if (parts.Length != 6)
                {
                    Console.WriteLine("[Ошибка]: Полученные данные не являются корректными.");
                    return;
                }

                string fromAddress = parts[0];
                string toAddress = parts[1];
                long value = long.Parse(parts[2]);
                long gasPrice = long.Parse(parts[3]);
                long gas = long.Parse(parts[4]);
                int nonce = int.Parse(parts[5]);

                Console.WriteLine("[Внимание]: Для подписания транзакции вам необходимо ввести приватный ключ отправителя.");
                Console.Write("Поле для ввода: ");
                string privateKey = Console.ReadLine();

                if (string.IsNullOrWhiteSpace(privateKey))
                {
                    Console.WriteLine("[Ошибка]: Введённое вами значение ключа не является корректным.");
                    return;
                }

                string rpcUrl = "https://sepolia.infura.io/v3/591fdde4e4f340659f50e84f7f4d86fb";
                var account = new Account(privateKey);
                Web3 web3 = null;

                Console.WriteLine("[Внимание]: Выполняется подключение к Infura для получения доп. информации...");

                try
                {

                    web3 = new Web3(account, rpcUrl);
                    Console.WriteLine("[Успешно]: Подключение с Infura установлено.");
                }
                catch
                {
                    Console.WriteLine("[Ошибка]: Не удалось подключится к Infura.");
                    return;
                }

                var transactionInput = new TransactionInput
                {
                    From = fromAddress,
                    To = toAddress,
                    Value = new HexBigInteger(value),
                    GasPrice = new HexBigInteger(gasPrice),
                    Gas = new HexBigInteger(gas),
                    Nonce = new HexBigInteger(nonce)
                };

                Console.WriteLine("[Внимание]: Выполняется создание подписи...");

                try
                {
                    var transactionManager = web3.TransactionManager;
                    var rawTransaction = await transactionManager.SignTransactionAsync(transactionInput);

                    byte[] response = Encoding.UTF8.GetBytes(rawTransaction);
                    await stream.WriteAsync(response, 0, response.Length);

                    Console.WriteLine("[Успешно]: Транзакция подписана.");
                    Console.Clear();

                    Console.WriteLine("Добро пожаловать в автономный модуль Transaction Signer!");
                    Console.WriteLine("Эта программа выполняет построение и подписание транзакций.\n");
                }
                catch
                {
                    Console.WriteLine("[Ошибка]: Не удалось подписать транзакцию.");
                }
            }
        }
        catch
        {
            Console.WriteLine($"[Ошибка]: Не удалось получить данные от Transaction Sender.");
            return;
        }
    }
}