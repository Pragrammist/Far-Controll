using System;
using System.Net;
using System.Text;
using System.IO;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;

namespace Controller
{
    class Program
    {
        //static readonly Encoding encoding = Encoding.UTF8;
        static void Main()
        {
            

            //Start();

            
        }
        static async Task<string> GetStrs()
        {
            await Task.Delay(100);
            return "eee";
        } 
        static void Start()
        {
            string adress = "http://44a5-95-174-125-219.ngrok.io/";
            //bool end = false;
            HttpListener listener = new();

            listener.Prefixes.Add(adress);

            Console.WriteLine("Ожидание подключений...");
            //Task.Run(() => end = bool.Parse(Console.ReadLine()));


            listener.Start();
            try
            {
                while (true)
                {

                    HttpListenerContext context = listener.GetContext();
                    HttpListenerRequest request = context.Request;
                    string result;
                    using (var reader = new StreamReader(request.InputStream))
                    {
                        result = reader.ReadToEnd().Trim(' ', '\n');
                    }
                    Console.WriteLine(result);

                    string command = Console.ReadLine().Trim(' ', '\n');
                    if (command.ToLower() == "end")
                    {
                        break;
                    }
                    HttpListenerResponse response = context.Response;

                    byte[] buffer = Encoding.UTF8.GetBytes(command);
                    response.ContentLength64 = buffer.Length;
                    Stream output = response.OutputStream;
                    output.Write(buffer, 0, buffer.Length);
                    output.Close();
                }
            }
            catch (WebException web)
            {
                Console.WriteLine("message: {0}\nresponse: {1}\nstatus: {2}", web.Message, web.Response, web.Status);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            finally
            {
                listener.Stop();
            }
        }

    }


   
}

