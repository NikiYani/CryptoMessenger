using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Security.Cryptography;
using System.IO;

namespace EDSCSR
{
    static class EDS
    {
        private static string hashAlgorithm = "SHA256";

        public static void CreateEDS(string pathFile, string pathDestination)
        {
            byte[] documentContent = File.ReadAllBytes(pathFile);   // Исходный файл в виде байтов.
            RSA rsa = RSA.Create(); // Создание пары ключей (открытый/закрытый). 
            RSAPKCS1SignatureFormatter rsaFormatter = new RSAPKCS1SignatureFormatter(rsa); // Создание RSAPKCS1SignatureFormatter объекта и передача ему экземпляра RSA для передачи закрытого ключа.
            rsaFormatter.SetHashAlgorithm(hashAlgorithm); //Установка хэш-алгоритма на SHA256.
            byte[] hashDocumentContent = SHA256.Create().ComputeHash(documentContent);  // Создание хэша исходного файла.
            byte[] signContent = rsaFormatter.CreateSignature(hashDocumentContent); //Создание ЭЦП.
            
            File.WriteAllText(pathDestination + "\\key", rsa.ToXmlString(false));
            File.WriteAllBytes(pathDestination + "\\sign", signContent);
        }

        public static bool CheckEDS(string path)
        {
            bool result = false;

            string key = File.ReadAllText(path + "\\key");
            byte[] sign = File.ReadAllBytes(path + "\\sign");
            byte[] data = new byte[0];

            string[] files = Directory.GetFiles(path);

            foreach (string file in files)
            {
                if(file.EndsWith(".enc"))
                {
                    data = SHA256.Create().ComputeHash(File.ReadAllBytes(file.Substring(0, file.Length-4)));
                }
            }

            RSA rsa = RSA.Create();
            rsa.FromXmlString(key);
            RSAPKCS1SignatureDeformatter rsaDeformatter = new RSAPKCS1SignatureDeformatter(rsa);
            rsaDeformatter.SetHashAlgorithm(hashAlgorithm);
            
            if (rsaDeformatter.VerifySignature(data, sign))
            {
                result = true;
            }
            else
            {
                result = false;
            }

            return result;
        }
    }
}
