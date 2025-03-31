using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Net;
using System.Net.Sockets;

namespace FileSharingServer
{
    internal class Program
    {
        static void Main(string[] args)
        {
            TcpListener server = new TcpListener(IPAddress.Any, 5000);
            server.Start();
            Console.WriteLine("Server đang lắng nghe trên cổng 5000...");

            while(true)
            {
                TcpClient client = server.AcceptTcpClient();
                try
                {
                    NetworkStream stream = client.GetStream();
                    try
                    {
                        FileStream filestream = new FileStream("received_file.txt", FileMode.Create);
                        try
                        {
                            stream.CopyTo(filestream);
                            Console.WriteLine("File đã nhận xong!");
                        }
                        finally
                        {
                            filestream.Close();
                        }
                    }
                    finally
                    {
                        stream.Close();
                    }  
                }
                finally 
                {
                    client.Dispose();
                }
            }
        }
    }
}
