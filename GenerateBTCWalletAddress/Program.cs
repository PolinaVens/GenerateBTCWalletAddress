using System;
using System.Security.Cryptography;
using System.Numerics;

namespace GenerateBTCWalletAddress
{
    class Program
    {
        static void Main()
        {

            string openkey;
            
          Console.WriteLine("Введите открытый ключ(или оставьте пустым для генерации случайного открытого ключа)");
            openkey = Console.ReadLine();
           openkey= string.IsNullOrEmpty(openkey)? GenerateOpenKey():openkey;
            string key = " ";
            while (key != "M" && key != "T"&& key != "m" && key != "t")
            {
                Console.WriteLine("Тип сети? (M)ain (T)est ");
                key = Console.ReadLine().ToString();
            }
            bool typeNetwork=key=="M"||key=="m"?true:false;
            
            Console.WriteLine("Адрес кошелька: \n"+GenerateWalletAddress(openkey, typeNetwork));
            



        }
        public static string GenerateWalletAddress(string openkey, bool typeNetwork) //Метод для генерации адреса (openkey -открытый ключ в виде hex строки,typeNetwork true для основной сети и false для тестовой)
        {
            SHA256 sha = SHA256Managed.Create();
            RIPEMD160 riPEMD160 = RIPEMD160Managed.Create();// Создаем экземпляры классов реализаций хеш алгоритмов
            byte[] byteOpenKey = StringToByteArray(openkey);
            // 1 шаг - применить к открытому ключу SHA-256
            byte[] firstStepRes = sha.ComputeHash(byteOpenKey);
            // 2 шаг - применить к результату шага 1 RIPEMD160
            byte[] secondStepRes = riPEMD160.ComputeHash(firstStepRes);
            // 3 шаг - добавить к результату шага 2 версионный бит(00 для основной сети и 6f для тестовой)
            byte[] thirdStepRes = new byte[secondStepRes.Length + 1];
            thirdStepRes[0] = typeNetwork ? (byte)0 : (byte)0x6F;
            secondStepRes.CopyTo(thirdStepRes, 1);
            // 4 шаг - два раза применить SHA-256 к результату шага 3
            byte[] fourthStepRes = sha.ComputeHash(sha.ComputeHash(thirdStepRes));
            // 5 шаг - взять первые четыре байта из шага 4(проверочная сумма адреса) и добавить их в конец результата шага 3
            byte[] fifthStepRes = new byte[thirdStepRes.Length + 4];
            thirdStepRes.CopyTo(fifthStepRes, 0);
            for (int i = 0; i < 4; i++)
                fifthStepRes[fifthStepRes.Length - 4 + i] = fourthStepRes[i];
            return ByteToBase58(fifthStepRes);//возвращаем ключ в base58 кодировке
        }
        public static byte[] StringToByteArray(string hex)//Конвертируем hex строку в byte[]
        {
            int NumberChars = hex.Length;
            byte[] bytes = new byte[NumberChars / 2];
            for (int i = 0; i < NumberChars; i += 2)
            {
                bytes[i / 2] = Convert.ToByte(hex.Substring(i, 2), 16);
            }
            return bytes;
        }
        public static string ByteToBase58(byte[] data)//Конвертируем byte[] в base58 строку
        {
            string alphabet = "123456789ABCDEFGHJKLMNPQRSTUVWXYZabcdefghijkmnopqrstuvwxyz";

            BigInteger intData = 0;
            for (int i = 0; i < data.Length; i++)
            {
                intData = intData * 256 + data[i];
            }

           
            string result = "";
            while (intData > 0)
            {
                int remainder = (int)(intData % 58);
                intData /= 58;
                result = alphabet[remainder] + result;
            }

           
            for (int i = 0; i < data.Length && data[i] == 0; i++)
            {
                result = '1' + result;
            }
            return result;
        }
        public static string GenerateOpenKey() // генерируем случайный открытый ключ
        {
            Random random = new Random();
            byte[] bytes = new byte[32];

            random.NextBytes(bytes);
            string key = BitConverter.ToString(bytes).Replace("-", "").ToLower();
            Console.WriteLine("Открытый ключ: \n" + key);
            return key;
        }
    }


}
