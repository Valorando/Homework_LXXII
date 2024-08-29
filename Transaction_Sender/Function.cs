using Nethereum.Util;
using Nethereum.Web3;
using Newtonsoft.Json;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace Transaction_Sender_Function
{
    public class Transaction_Sender
    {
        public static async Task Send_Transaction()
        {
            Console.WriteLine("--------------------------------------------------------------------------------------------------");
            Console.WriteLine("Введите полный путь к .json файлу и нажмите Enter (Пример: C:\\Users\\asus\\Desktop\\tx1.json)");
            Console.Write("Поле для ввода: ");
            string path = Console.ReadLine();
            Console.WriteLine("--------------------------------------------------------------------------------------------------\n");

            Console.WriteLine("[Внимание]: Выполняется поиск .json файла по указанному пути...");

            if (!File.Exists(path))
            {
                Console.WriteLine("[Ошибка]: Файл отсутствует, либо указан некорректный путь к нему.");
                return;
            }

            Console.WriteLine("[Успешно]: Файл обнаружен.");
            Console.Clear();

            string jsonData = File.ReadAllText(path);
            dynamic transactionData = JsonConvert.DeserializeObject(jsonData);

            if (transactionData == null)
            {
                Console.WriteLine("[Ошибка]: Файл пуст.");
                return;
            }

            string fromAddress = transactionData.From;
            string toAddress = transactionData.To;
            long value = transactionData.Value;
            long gasPrice = transactionData.GasPrice;
            long gas = transactionData.Gas;
            int nonce = transactionData.Nonce;

            Console.WriteLine("[Внимание]: Выполняется проверка адреса отправителя на валидность...");
            if (string.IsNullOrEmpty(fromAddress) || fromAddress.Length != 42)
            {
                Console.WriteLine("[Ошибка]: Адрес отправителя не прошёл проверку на валидность.");
                return;
            }
            Console.WriteLine("[Успешно]: Адрес отправителя прошёл проверку на валидность.");

            Console.WriteLine("[Внимание]: Выполняется проверка адреса отправителя на достоверность...");
            Addresses_reliability_test(fromAddress);

            Console.WriteLine("[Внимание]: Выполняется проверка адреса получателя на валидность...");
            if (string.IsNullOrEmpty(toAddress) || toAddress.Length != 42)
            {
                Console.WriteLine("[Ошибка]: Адрес получателя не прошёл проверку на валидность.");
                return;
            }
            Console.WriteLine("[Успешно]: Адрес получателя прошёл проверку на валидность.");

            Console.WriteLine("[Внимание]: Выполняется проверка адреса получателя на достоверность...");
            Addresses_reliability_test(toAddress);

            Console.WriteLine("[Внимание]: Выполняется проверка суммы на валидность...");
            if (value <= 0)
            {
                Console.WriteLine("[Ошибка]: Сумма не прошла проверку на валидность.");
                return;
            }
            Console.WriteLine("[Успешно]: Сумма прошла проверку на валидность.");

            Console.WriteLine("[Внимание]: Выполняется проверка на платёжеспособность...");
            await Sender_solvency_test(fromAddress, value);


            Console.WriteLine("[Внимание]: Выполняется проверка gasPrice, gasLimit и nonce на валидность...");

            if (gasPrice <= 0)
            {
                Console.WriteLine("[Ошибка]: Значение gasPrice должно быть выше нуля.");
                return;
            }

            if (gas <= 0)
            {
                Console.WriteLine("[Ошибка]: Значение gasLimit должно быть выше нуля.");
                return;
            }

            if (nonce < 0)
            {
                Console.WriteLine("[Ошибка]: Значение nonce не должно быть ниже нуля.");
                return;
            }

            Console.WriteLine("[Успешно]: Значения gasPrice, gasLimit и nonce прошли проверку на валидность.");

            string rpcUrl = "https://sepolia.infura.io/v3/591fdde4e4f340659f50e84f7f4d86fb";
            Web3 web3 = null;

            Console.WriteLine("[Внимание]: Выполняется подключение к Infura для подготовки соединения к отправке транзакции...");

            try
            {

                web3 = new Web3(rpcUrl);
                Console.WriteLine("[Успешно]: Подключение с Infura установлено.");
            }
            catch
            {
                Console.WriteLine("[Ошибка]: Не удалось подключится к Infura.");
                return;
            }

            Console.Clear();

            Console.WriteLine("Данные которые будут использоватся для этой транзакции.");
            Console.WriteLine($"Отправитель: {fromAddress}");
            Console.WriteLine($"Получатель: {toAddress}");
            Console.WriteLine($"К отправке: {value}");
            Console.WriteLine($"Цена gas: {gasPrice}");
            Console.WriteLine($"Лимит gas: {gas}");
            Console.WriteLine($"Количество nonce: {nonce}\n");

            Console.WriteLine("[Внимание]: Для для подписания транзакции требуется запустить автономный модуль Transaction Signer.");
            Console.WriteLine("[Внимание]: Запустите модуль Transaction_Signer.exe, после чего нажмите кнопку 1 здесь.");
            Console.WriteLine("[Внимание]: Если вы нажмёте любую другую кнопку, то вас вернёт в меню.\n");

            ConsoleKeyInfo key = Console.ReadKey(true);
            if (key.Key == ConsoleKey.D1)
            {
                IPAddress serverIP = IPAddress.Parse("127.0.0.1");
                int serverPort = 8888;

                using (Socket client = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp))
                {
                    Console.WriteLine("[Внимание]: Выполняется подключение к Transaction Signer...");

                    try
                    {
                        client.Connect(new IPEndPoint(serverIP, serverPort));
                        Console.WriteLine("[Успешно]: Подключение с Transaction Signer установлено.");
                    }
                    catch
                    {
                        Console.WriteLine("[Ошибка]: Не удалось подключится к Transaction Signer.");
                        return;
                    }

                    Console.WriteLine("[Внимание]: Отправляем данные Transaction Signer.");

                    byte[] data;
                    string signedTransaction;

                    try
                    {
                        string message = $"{fromAddress}|{toAddress}|{value}|{gasPrice}|{gas}|{nonce}";
                        data = Encoding.UTF8.GetBytes(message);
                        client.Send(data);
                        Console.WriteLine("[Успешно]: Данные отправлены Transaction Signer.");
                    }
                    catch
                    {
                        Console.WriteLine("[Ошибка]: Не удалось отправить данные Transaction Signer.");
                        return;
                    }

                    Console.WriteLine("[Внимание]: Ожидаем ответа от Transaction Signer.");

                    try
                    {
                        data = new byte[256];
                        int bytes = client.Receive(data);
                        signedTransaction = Encoding.UTF8.GetString(data, 0, bytes);
                        Console.WriteLine("[Успешно]: Получен ответ от Transaction Signer.");

                        if (string.IsNullOrWhiteSpace(signedTransaction))
                        {
                            Console.WriteLine("[Ошибка]: Ответ от Transaction Signer не содержит символов.");
                            return;
                        }
                    }
                    catch
                    {
                        Console.WriteLine("[Ошибка]: Не удалось получить ответ от Transaction Signer.");
                        return;
                    }


                    Console.WriteLine("[Внимание]: Выполняется отправка транзакции в сеть...");

                    try
                    {
                        string txHash = await web3.Eth.Transactions.SendRawTransaction.SendRequestAsync(signedTransaction);
                        Console.WriteLine("[Успешно]: Транзакция отправлена в сеть, Хэш: " + txHash);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"{ex.Message}");
                        return;
                    }
                }
            }
            else
            {
                return;
            }
        }

        private static void Addresses_reliability_test(string address)
        {
            try
            {
                var web3 = new Web3("https://sepolia.infura.io/v3/591fdde4e4f340659f50e84f7f4d86fb");
                var balanceWei = web3.Eth.GetBalance.SendRequestAsync(address).Result;
                Console.WriteLine("[Успешно]: Адрес прошёл проверку на достоверность.");
            }
            catch (Exception)
            {
                Console.WriteLine("[Ошибка]: Адрес не прошёл проверку на достоверность.");
            }
        }

        private static async Task Sender_solvency_test(string address, decimal amount)
        {
            try
            {
                var web3 = new Web3("https://sepolia.infura.io/v3/591fdde4e4f340659f50e84f7f4d86fb");
                var balanceWei = await web3.Eth.GetBalance.SendRequestAsync(address);

                decimal balanceInEther = UnitConversion.Convert.FromWei(balanceWei);

                if (balanceInEther < amount)
                {
                    Console.WriteLine("[Ошибка]: Отправитель не прошёл проверку на платёжеспособность.");
                    return;
                }
                Console.WriteLine("[Успешно]: Отправитель прошёл проверку на платёжеспособность.");
            }
            catch (Exception)
            {
                Console.WriteLine("[Ошибка]: Не удалось проверить баланс отправителя.");
            }
        }
    }
}