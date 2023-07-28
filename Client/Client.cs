using System;
using System.Text;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.IO;
using System.Threading;
namespace Client
{

    class handlemessages
    {

        private TcpClient client;
        private bool isValidroom;
        public void startrecv(TcpClient c)
        {
            
            Thread ctThread = new Thread(receivemssg);
            this.client = c;
            this.isValidroom = false;
            ctThread.Start();
        }

        private void receivemssg()
        {
            NetworkStream stream = client.GetStream();
            Byte[] d = new Byte[1024];
            while (client.Connected)
            {
                try
                {
                    Int32 bytes = stream.Read(d, 0, d.Length);
                    string responseData = Encoding.ASCII.GetString(d, 0, bytes);

                    if (responseData == "Valid Room")
                    {
                        Console.WriteLine("Room Joined");
                        this.isValidroom = true;
                    }
                    else if (responseData == "InValid Room")
                    {
                        this.isValidroom = false;
                    }
                    else
                    {
                        Console.WriteLine(responseData);
                    }
                }

                catch(Exception e)
                {
                    client.Close();
                }
            }
        }

        public bool getValidroom()
        {
            return isValidroom;
        }
    }
    class Program
    {

        static void Main(string[] args)
        {
            Int32 port = 8080;
            NetworkStream stream = null;
            TcpClient client = null;

            try
            {

                client = new TcpClient("server", port);
                stream = client.GetStream();
                Byte[] d = new Byte[1024];
                Int32 bytes = stream.Read(d, 0, d.Length);
                string res = Encoding.ASCII.GetString(d, 0, bytes);
                Console.WriteLine(res);

                if (res == "users full")
                {
                    Console.WriteLine("DisConnected");
                    stream.Close();
                    client.Close();
                }
                else
                {
                    handlemessages h = new handlemessages();
                    h.startrecv(client);
                    
                    while (client.Connected)
                    {
                       
                        Console.WriteLine("Enter 1 for Room creation");
                        Console.WriteLine("Enter 2 to list the Rooms");
                        Console.WriteLine("Enter 3 to join a Room");
                        Console.WriteLine("Enter 4 to exit");

                        // Create a string variable and get user input from the keyboard and store it in the variable
                        string choice = Console.ReadLine();

                        if (choice == "1")
                        {
                            bool inRoom = true;
                            Byte[] data = Encoding.ASCII.GetBytes("1");
                            stream.Write(data, 0, data.Length);

                            data = new Byte[256];
                         
                            while(inRoom)
                            {
                                Console.WriteLine("Enter 1 to send Chat");
                                Console.WriteLine("Enter 2 to Leave Room");

                                string roomchoice = Console.ReadLine();

                                if(roomchoice=="1")
                                {
                                    Console.WriteLine("Enter message");
                                    string msg = Console.ReadLine();
                                    msg = "Me: " + msg;
                                    Console.WriteLine(msg);
                                    Byte[] msgdata = Encoding.ASCII.GetBytes(msg);
                                    stream.Write(msgdata, 0, msgdata.Length);
                                }
                                else
                                {
                                    //Leave that room
                                    data = Encoding.ASCII.GetBytes("LR");
                                    stream.Write(data, 0, data.Length);
                                    inRoom = false;
                                    break;
                                }
                            }
                        }

                        else if (choice == "2")
                        {

                            Byte[] data = Encoding.ASCII.GetBytes("2");
                            stream.Write(data, 0, data.Length);
                            Thread.Sleep(100);
                            
                        }
                        else if (choice == "3")
                        {
                            Console.WriteLine("Enter Room id");
                            string roomid = Console.ReadLine();
                            Byte[] data = Encoding.ASCII.GetBytes("3");
                            stream.Write(data, 0, data.Length);
                            Thread.Sleep(500);
                            data= Encoding.ASCII.GetBytes(roomid);
                            stream.Write(data, 0, data.Length);

                            Thread.Sleep(100);

                            if (h.getValidroom())
                            {

                                bool inRoom = true;
                                while (inRoom)
                                {
                                    Console.WriteLine("Enter 1 to send Chat");
                                    Console.WriteLine("Enter 2 to Leave Room");

                                    string roomchoice = Console.ReadLine();

                                    if (roomchoice == "1")
                                    {
                                        Console.WriteLine("Enter message");
                                        string msg = Console.ReadLine();
                                        msg = "Me: " + msg;
                                        Console.WriteLine(msg);
                                        Byte[] msgdata = Encoding.ASCII.GetBytes(msg);
                                        stream.Write(msgdata, 0, msgdata.Length);
                                    }
                                    else
                                    {
                                        //Leave that room
                                        data = Encoding.ASCII.GetBytes("LR");
                                        stream.Write(data, 0, data.Length);
                                        inRoom = false;
                                    }
                                }
                            }
                            else
                            {
                                Console.WriteLine("Invalid RoomID");
                            }

                        }
                        else
                        {
                            Console.WriteLine("Exiting.....");
                            Byte[] data = Encoding.ASCII.GetBytes("1");
                            data = Encoding.ASCII.GetBytes("4");
                            stream.Write(data, 0, data.Length);
                            client.Close();
                        }

                        Console.WriteLine("\n");
                    }
                }
            }

            catch (Exception e)
            {
                Console.WriteLine("SocketException: {0}", e);
            }

            finally
            {
                // Close everything.
                stream.Close();
                client.Close();
            }
        }
    }
}
