using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace EDSCSR
{
    class CipherAes
    {
        private static byte[] iv = new byte[] { 1, 2, 3, 4, 5, 6, 7, 8, 1, 2, 3, 4, 5, 6, 7, 8 };
        private static string cryptoKey = "NikiYaniLeetBoyzNikiYaniLeetBoyz";

        public static bool EncryptData(string sourceFile, string destinationFile)
        {
            try
            {
                FileStream inFileStream = new FileStream(sourceFile, FileMode.Open, FileAccess.Read);
                FileStream outFileStream = new FileStream(destinationFile, FileMode.Create, FileAccess.Write);
                outFileStream.SetLength(0);

                byte[] bin = new byte[100];
                long rdlen = 0;
                long totlen = inFileStream.Length; 
                int len;

                AesManaged AESProvider = new AesManaged();

                AESProvider.Key = ASCIIEncoding.ASCII.GetBytes(cryptoKey);
                AESProvider.IV = iv;
                ICryptoTransform AESEncrypt = AESProvider.CreateEncryptor(AESProvider.Key, AESProvider.IV);

                CryptoStream cryptoStream = new CryptoStream(outFileStream, AESEncrypt, CryptoStreamMode.Write);

                while (rdlen < totlen)
                {
                    len = inFileStream.Read(bin, 0, 100);
                    cryptoStream.Write(bin, 0, len);
                    rdlen = rdlen + len;
                }

                cryptoStream.Close();
                inFileStream.Close();
                outFileStream.Close();
                return true;
            }

            catch (Exception e)
            {
                MessageBox.Show(e.ToString(), "Ошибка шифрования", MessageBoxButton.OK, MessageBoxImage.Stop);
                return false;
            }
        }

        public static bool DecryptData(String sourceFile, String destinationFile)
        {
            try
            {                
                FileStream inFileStream = new FileStream(sourceFile, FileMode.Open, FileAccess.Read);
                FileStream outFileStream = new FileStream(destinationFile, FileMode.Create, FileAccess.Write);
                outFileStream.SetLength(0);
                
                byte[] bin = new byte[100]; 
                long rdlen = 0;  
                long totlen = inFileStream.Length; 
                int len;
                
                AesManaged AESProvider = new AesManaged();
                AESProvider.Key = ASCIIEncoding.ASCII.GetBytes(cryptoKey);                
                AESProvider.IV = iv;
                ICryptoTransform AESDecrypt = AESProvider.CreateDecryptor(AESProvider.Key, AESProvider.IV);

                CryptoStream cryptoStream = new CryptoStream(outFileStream, AESDecrypt, CryptoStreamMode.Write);
                                
                while (rdlen < totlen)
                {
                    len = inFileStream.Read(bin, 0, 100);
                    cryptoStream.Write(bin, 0, len);
                    rdlen = rdlen + len;
                }

                cryptoStream.Close();
                inFileStream.Close();
                outFileStream.Close();
                return true;
            }

            catch (Exception e)
            {
                MessageBox.Show(e.ToString(), "Ошибка Расшифровки", MessageBoxButton.OK, MessageBoxImage.Stop);
                return false;
            }
        }
    }
}
