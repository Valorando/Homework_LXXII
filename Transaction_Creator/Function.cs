using Nethereum.Hex.HexTypes;
using Nethereum.Util;
using Nethereum.Web3;
using Newtonsoft.Json;

namespace Transaction_Creator_Function
{
    public class Transaction_Creator
    {
        public static async Task Transaction_Data_Create()
        {
            Console.WriteLine("--------------------------------------------------------------------------------------------------");
            Console.Write("Вставьте сюда адрес отправителя и нажмите Enter: ");
            string fromAddress = Console.ReadLine();
            Console.WriteLine("--------------------------------------------------------------------------------------------------\n");
            Console.WriteLine("[Внимание]: Выполняется проверка адреса отправителя на валидность...");
            if (string.IsNullOrEmpty(fromAddress) || fromAddress.Length != 42)
            {
                Console.WriteLine("[Ошибка]: Адрес отправителя не прошёл проверку на валидность.");
                return;
            }
            Console.WriteLine("[Успешно]: Адрес отправителя прошёл проверку на валидность.");



            Console.WriteLine("[Внимание]: Выполняется проверка адреса отправителя на достоверность...");
            Addresses_reliability_test(fromAddress);

            Console.Clear();

            Console.WriteLine("--------------------------------------------------------------------------------------------------");
            Console.Write("Вставьте сюда адрес получателя и нажмите Enter: ");
            string toAddress = Console.ReadLine();
            Console.WriteLine("--------------------------------------------------------------------------------------------------\n");
            Console.WriteLine("[Внимание]: Выполняется проверка адреса получателя на валидность...");
            if (string.IsNullOrEmpty(toAddress) || toAddress.Length != 42)
            {
                Console.WriteLine("[Ошибка]: Адрес получателя не прошёл проверку на валидность.");
                return;
            }
            Console.WriteLine("[Успешно]: Адрес получателя прошёл проверку на валидность.");


            Console.WriteLine("[Внимание]: Выполняется проверка адреса получателя на достоверность...");
            Addresses_reliability_test(toAddress);

            Console.Clear();

            Console.WriteLine("--------------------------------------------------------------------------------------------------");
            Console.Write("Введите сумму отправки, формат 0,00 и нажмите Enter: ");
            string amountETH = Console.ReadLine();
            Console.WriteLine("--------------------------------------------------------------------------------------------------\n");
            Console.WriteLine("[Внимание]: Выполняется проверка суммы на валидность...");
            if (!decimal.TryParse(amountETH, out decimal amount) || amount <= 0)
            {
                Console.WriteLine("[Ошибка]: Сумма не прошла проверку на валидность.");
                return;
            }
            Console.WriteLine("[Успешно]: Сумма прошла проверку на валидность.");

            
            Console.WriteLine("[Внимание]: Выполняется проверка на платёжеспособность...");
            Sender_solvency_test(fromAddress, amount);

            var amountWei = Web3.Convert.ToWei(amountETH);

            Console.Clear();

            string rpcUrl = "https://sepolia.infura.io/v3/591fdde4e4f340659f50e84f7f4d86fb";
            Web3 web3 = null;
            HexBigInteger gasPrice = null;
            HexBigInteger nonce = null;

            Console.WriteLine("[Внимание]: Выполняется подключение к Infura для получения доп. информации...");

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

            Console.WriteLine("[Внимание]: Выполняется получение актуальной цены gas");


            try
            {
                gasPrice = await web3.Eth.GasPrice.SendRequestAsync();
                Console.WriteLine("[Успешно]: Актуальная цена gas получена.");
            }
            catch
            {
                Console.WriteLine("[Ошибка]: Не удалось получить цену gas с ресурса.");
                return;
            }

            Console.WriteLine("[Внимание]: Выполняется получение актуального количества nonce.");

            try
            {
                nonce = await web3.Eth.Transactions.GetTransactionCount.SendRequestAsync(fromAddress);
                Console.WriteLine("[Успешно]: Актуальное количество nonce получено.");
            }
            catch
            {
                Console.WriteLine("[Ошибка]: Не удалось получить количество nonce с ресурса.");
                return;
            }

            var gasLimit = new HexBigInteger(21000);

            Console.WriteLine();
            Console.WriteLine("[Внимание]: Данные для совершения транзакции собраны и проверены, ознакомьтесь с ними.");

            Console.WriteLine($"Отправитель: {fromAddress}");
            Console.WriteLine($"Получатель: {toAddress}");
            Console.WriteLine($"К отправке: {amountWei} Wei = {amountETH} SepoliaETH.");
            Console.WriteLine($"Цена gas: {gasPrice}");
            Console.WriteLine($"Лимит gas: {gasLimit}");
            Console.WriteLine($"Количество nonce: {nonce}\n");

            Console.WriteLine("Сохранить эти данные?");
            Console.WriteLine("Чтобы сохранить данные в файл - нажмите кнопку 1");
            Console.WriteLine("Чтобы вернуться в меню - нажмите любую другую кнопку.");

            ConsoleKeyInfo key2 = Console.ReadKey(true);

            if (key2.Key == ConsoleKey.D1)
            {
                Console.Clear();
                Console.Write("Введите имя для файла(Без расширения): ");
                string fileName = Console.ReadLine();

                var transactionData = new
                {
                    From = fromAddress,
                    To = toAddress,
                    Value = amountWei,
                    Gas = gasLimit.Value,
                    GasPrice = gasPrice.Value,
                    Nonce = nonce.Value
                };

                string json = JsonConvert.SerializeObject(transactionData, Formatting.Indented);
                string desktopPath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
                string filePath = Path.Combine(desktopPath, $"{fileName}.json");
                File.WriteAllText(filePath, json);
                Console.WriteLine($"[Успешно]: Файл {fileName}.json успешно создан на рабочем столе.");
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

        private static void Sender_solvency_test(string address, decimal amount)
        {
            try
            {
                var web3 = new Web3("https://sepolia.infura.io/v3/591fdde4e4f340659f50e84f7f4d86fb");
                var balanceWei = web3.Eth.GetBalance.SendRequestAsync(address).Result;

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
