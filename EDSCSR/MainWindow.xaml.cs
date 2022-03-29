using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace EDSCSR
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void ShowMessage(string msg)
        {
            tbStatus.AppendText("[" + DateTime.Now + "]: " + msg + "\r\n>");
            tbStatus.ScrollToEnd();
        }

        private void btnChooseFile_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            if (openFileDialog.ShowDialog() == true)
            {
                tbPathToFile.Text = openFileDialog.FileName;
                ShowMessage("Файл выбран.");
            }
        }

        private void btnPrepare_Click(object sender, RoutedEventArgs e)
        {
            if(!File.Exists(tbPathToFile.Text))
            {
                ShowMessage("Путь к Файлу указан некорректно.");
            }
            else
            {
                ShowMessage("Начата подготовка файла.");

                if (Directory.Exists("Prepared"))
                {
                    Directory.Delete("Prepared", true);
                }

                Directory.CreateDirectory("Prepared");

                string filename = tbPathToFile.Text.Split('\\').Last();
                File.Copy(tbPathToFile.Text, "Prepared" + "\\" + filename);
                ShowMessage("Файл скопирован.");

                EDS.CreateEDS("Prepared" + "\\" + filename, "Prepared");
                ShowMessage("ЭЦП готова.");

                CipherAes.EncryptData("Prepared" + "\\" + filename, "Prepared" + "\\" + filename + ".enc");
                ShowMessage("Файл зашифрован.");

                ShowMessage("Готово к отправке.");
            }
        }

        private void btnSend_Click(object sender, RoutedEventArgs e)
        {
            if(tbReceiver.Text == "")
            {
                ShowMessage("Получатель не указан.");
            }
            else if (!File.Exists(tbPathToFile.Text))
            {
                ShowMessage("Путь к Файлу указан некорректно.");
            }
            else
            {
                List<string> attachments = new List<string>();
                string file = "Prepared\\" + tbPathToFile.Text.Split('\\').Last() + ".enc";
                string sign = "Prepared\\sign";
                string key = "Prepared\\key";

                attachments.Add(file);
                attachments.Add(sign);
                attachments.Add(key);

                Email.SendEmail(tbUserEmail.Text, pbUserPassword.Password, tbReceiver.Text, tbSmtpServer.Text, Convert.ToInt32(tbSmtpPort.Text), Convert.ToBoolean(cbSmtpSsl.Text), attachments);

                ShowMessage("Отправлено.");
            }
        }

        private void btnDownloadNew_Click(object sender, RoutedEventArgs e)
        {
            int count = Email.DownloadNewEmails(tbUserEmail.Text, pbUserPassword.Password, tbPopServer.Text, Convert.ToInt32(tbPopPort.Text), Convert.ToBoolean(cbPopSsl.Text));

            switch (count)
            {
                case (0):
                    ShowMessage("Новых файлов нет.");
                    break;

                default:
                    ShowMessage("Скачано новых файлов: " + count + ".");
                    break;
            }
        }

        private void btnDecrypt_Click(object sender, RoutedEventArgs e)
        {
            string userEmail = tbUserEmail.Text;
            string path = "Messages\\" + userEmail + "\\new";

            if (!Directory.Exists(path))
            {
                ShowMessage("Нет подходящих файлов.");
            }
            else if(Directory.GetDirectories(path).Length == 0)
            {
                ShowMessage("Нет подходящих файлов.");
            }
            else
            {
                string[] directories = Directory.GetDirectories(path);
                
                foreach(string directory in directories)
                {
                    string[] files = Directory.GetFiles(directory);
                    
                    foreach(string file in files)
                    {
                        if(file.EndsWith(".enc"))
                        {
                            CipherAes.DecryptData(file, file.Substring(0, file.Length-4));
                        }
                    }

                    string decDirectory = path.Replace("\\new", "\\decrypted");
                    string newDirectory = directory.Replace("\\new\\", "\\decrypted\\");

                    if (!Directory.Exists(decDirectory))
                    {
                        Directory.CreateDirectory(decDirectory);
                    }

                    Directory.Move(directory, newDirectory);
                }
                
                ShowMessage("Расшифровано " + directories.Length + " файлов.");
            }
        }

        private void btnCheckEDS_Click(object sender, RoutedEventArgs e)
        {
            int countValid = 0;
            int countNotValid = 0;

            if(!Directory.Exists("Messages\\" + tbUserEmail.Text + "\\valid"))
            {
                Directory.CreateDirectory("Messages\\" + tbUserEmail.Text + "\\valid");
            }

            if (!Directory.Exists("Messages\\" + tbUserEmail.Text + "\\notValid"))
            {
                Directory.CreateDirectory("Messages\\" + tbUserEmail.Text + "\\notValid");
            }

            string path = "Messages\\" + tbUserEmail.Text + "\\decrypted";

            if(Directory.Exists(path))
            {
                string[] dirs = Directory.GetDirectories(path);

                foreach (string dir in dirs)
                {
                    if (EDS.CheckEDS(dir) == true)
                    {
                        string newDir = dir.Replace("\\decrypted\\", "\\valid\\");
                        Directory.Move(dir, newDir);
                        countValid++;
                    }
                    else
                    {
                        string newDir = dir.Replace("\\decrypted\\", "\\notValid\\");
                        Directory.Move(dir, newDir);
                        countNotValid++;
                    }
                }
            }

            switch (countValid+countNotValid)
            {
                case (0):
                    ShowMessage("Непроверенные файлы отсутствуют.");
                    break;

                default:
                    ShowMessage("Проверено " + (countValid + countNotValid) + " файлов.");
                    
                    if(countValid > 0)
                    {
                        ShowMessage("ЭЦП корректна: " + countValid + " файлов.");
                    }

                    if(countNotValid >0)
                    {
                        ShowMessage("ЭЦП НЕ корректна: " + countNotValid + " файлов.");

                    }
                    break;
            }
        }

        private void btnExit_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }

        private void btnOpenFolder_Click(object sender, RoutedEventArgs e)
        {
            Process.Start(Directory.GetCurrentDirectory());
        }

        private void btnTemplate1_Click(object sender, RoutedEventArgs e)
        {
            Templates.LoadFromTemplate1();

            tbUserEmail.Text = Templates.template.userEmail;
            pbUserPassword.Password = Templates.template.userPassword;

            tbSmtpServer.Text = Templates.template.smtpServer;
            tbSmtpPort.Text = Templates.template.smtpPort.ToString();
            cbSmtpSsl.SelectedIndex = Templates.template.smtpSsl ? 0 : 1;

            tbPopServer.Text = Templates.template.popServer;
            tbPopPort.Text = Templates.template.popPort.ToString();
            cbPopSsl.SelectedIndex = Templates.template.popSsl ? 0 : 1;

            tbReceiver.Text = Templates.template.receiver;

            ShowMessage("Использован Тестовый аккаунт - Янченков Никита.");
        }

        private void btnTemplate2_Click(object sender, RoutedEventArgs e)
        {
            Templates.LoadFromTemplate2();

            tbUserEmail.Text = Templates.template.userEmail;
            pbUserPassword.Password = Templates.template.userPassword;

            tbSmtpServer.Text = Templates.template.smtpServer;
            tbSmtpPort.Text = Templates.template.smtpPort.ToString();
            cbSmtpSsl.SelectedIndex = Templates.template.smtpSsl ? 0 : 1;

            tbPopServer.Text = Templates.template.popServer;
            tbPopPort.Text = Templates.template.popPort.ToString();
            cbPopSsl.SelectedIndex = Templates.template.popSsl ? 0 : 1;

            tbReceiver.Text = Templates.template.receiver;
            
            ShowMessage("Использован Тестовый аккаунт - Еремеев Никита.");
        }

        private void miModeSettings_Click(object sender, RoutedEventArgs e)
        {
            miModeWork.IsChecked = false;
            miModeSettings.IsChecked = true;

            miModeWork.IsEnabled = true;
            miModeSettings.IsEnabled = false;

            gbWork.IsEnabled = false;
            gbSettings.IsEnabled = true;

            miTemplates.IsEnabled = true;
            gbStatus.Header = "Консоль вывода [Модуль настроек аккаунта пользователя]";

            ShowMessage("Модуль настроек аккаунта пользователя включен.");
        }

        private void miModeWork_Click(object sender, RoutedEventArgs e)
        {
            if(tbUserEmail.Text == "" || pbUserPassword.Password == "" || tbSmtpServer.Text == "" || tbSmtpPort.Text == "" || tbPopServer.Text == "" || tbPopPort.Text == "")
            {
                ShowMessage("Для перехода в модуль работы с документами необходимо заполнить все поля в блоке \"Настройка аккаунта пользователя\".");
                ShowMessage("Воспользуйтесь меню \"Тестовые аккаунты\" или заполните данные вручную.");
            }
            else
            {
                miModeWork.IsChecked = true;
                miModeSettings.IsChecked = false;

                miModeWork.IsEnabled = false;
                miModeSettings.IsEnabled = true;

                gbWork.IsEnabled = true;
                gbSettings.IsEnabled = false;

                miTemplates.IsEnabled = false;
                gbStatus.Header = "Консоль вывода [Модуль работы с документами]";

                ShowMessage("Модуль работы с документами включен.");
            }
        }

        private void miClearConsole_Click(object sender, RoutedEventArgs e)
        {
            tbStatus.Text = ">";
        }
    }
}
