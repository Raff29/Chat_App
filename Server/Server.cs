using System;

namespace backend
{
    
        using System;
        using System.Net;
        using System.Net.Sockets;
        using System.Collections.Generic;
        using System.IO;
        using System.Threading;

    class Room
    {
        private List<string> messages;
        private int Roomid;
        private List<TcpClient> clients;

        public Room(int roomid,TcpClient c)
        {
            Roomid = roomid;
            messages = new List<string>();
            clients = new List<TcpClient>();
            clients.Add(c);
        }

        public void AddMessage(string message)
        {
            messages.Add(message);
        }

        public void addRoom(int roomid)
        {
            Roomid = roomid;
        }

        public void addClient(TcpClient c)
        {
            clients.Add(c);
        }

        public void removeClient(TcpClient c)
        {
            clients.Remove(c);
        }

        public int GetRoomid() {

            return Roomid;
        }


        public List<TcpClient> getclients()
        {
            return clients;
        
        }

        public List<string> getmessages()
        {
            return messages;
        }
    }
 

    class Server
    {

        public static int user_count;
        public static int room_id;
        static void Main(string[] args)
        {

            Int32 port = 8080;
            TcpListener server = null;
            List<Room> rooms = null;

           
            try
            {
                server = new TcpListener(IPAddress.Any, port);
                server.Start();

                Byte[] bytes = new Byte[256];
                String data = null;
                rooms = new List<Room>();
                user_count = 0;
                room_id = 1;
                while (true)
                {
                    TcpClient client = server.AcceptTcpClient();
                    Console.WriteLine("connected");

                    if (user_count != 10)
                    {

                        handleClient c = new handleClient();
                        Server.user_count+=1;
                        NetworkStream stream = client.GetStream();
                        data = "Welcome";
                        byte[] msg = System.Text.Encoding.ASCII.GetBytes(data);
                        // Send back a response.
                        stream.Write(msg, 0, msg.Length);
                        c.startClient(client, rooms, "user" + user_count,stream);
                    }
                    else
                    {
                        NetworkStream stream = client.GetStream();
                        data = "users full";
                        byte[] msg = System.Text.Encoding.ASCII.GetBytes(data);
                        // Send back a response.
                        stream.Write(msg, 0, msg.Length);
                        client.Close();
                    }

                  
                }

            }
            catch (SocketException e)
            {
                Console.WriteLine("SocketException: {0}", e);
            }
            finally
            {
                // Stop listening for new clients.
                server.Stop();
            }


            
        }
    }

    class handleClient
    {
        TcpClient client;
        string clientname;
        List<Room> roomslist;
        NetworkStream stream;
        
        public void startClient(TcpClient c, List<Room> r,string un,NetworkStream N)
        {
            this.client = c;
            Thread ctThread = new Thread(startChat);
            this.clientname = un;
            this.roomslist = r;
            this.stream = N;
            ctThread.Start();
        }

        private void startChat()
        {
            string data = null;
            byte[] bytes = new byte[1024];
            while (true)
            {
                data = null;

                try
                {
                    if (client.Connected)
                    {
                        int i;
                        data = null;
                       
                             i = stream.Read(bytes, 0, bytes.Length);
                        
                            // Translate data bytes to a ASCII string.
                            data = System.Text.Encoding.ASCII.GetString(bytes, 0, i);
                            Console.WriteLine("Received: {0}", data);

                        if (data == "1")
                        {
                            //create Room
                            Room r = new Room(Server.room_id, client);
                            Server.room_id += 1;
                            roomslist.Add(r);
                            data = "Room Created";
                            byte[] msg = System.Text.Encoding.ASCII.GetBytes(data);
                            // Send back a response.
                            stream.Write(msg, 0, msg.Length);
                            Console.WriteLine("Sent: {0}", data);
                            bool inRoom = true;
                            byte[] roombytes = null;
                            while (inRoom)
                            {
                                data = null;
                                if (roombytes == null)
                                {
                                    roombytes = new byte[2048];
                                }
                                int j;
                                j = stream.Read(roombytes, 0, roombytes.Length);

                                // Translate data bytes to a ASCII string.
                                data = System.Text.Encoding.ASCII.GetString(roombytes, 0, j);
                                if (data == "LR")
                                {
                                    inRoom = false;
                                    r.removeClient(client);
                                    Console.WriteLine("Leaving Room");
                                }
                                else
                                {


                                    data = data.Remove(0, 4);
                                    data = this.clientname + ": " + data;
                                    r.AddMessage(data);
                                    Console.WriteLine("ReceivedMessage: {0}", data);


                                    foreach (TcpClient c in r.getclients())
                                    {
                                        if (!c.Equals(client))
                                        {
                                            NetworkStream n = c.GetStream();
                                            byte[] b = System.Text.Encoding.ASCII.GetBytes(data);
                                            n.Write(b, 0, b.Length);

                                        }
                                    }



                                }





                            }
                        }

                        else if (data == "2")
                        {
                            int room_count = 1;
                            bool isFirst = true;
                            byte[] b=null;
                            foreach (Room r in roomslist)
                            {
                                if(isFirst)
                                {
                                   b= System.Text.Encoding.ASCII.GetBytes("\n"+"Rooms List"+"\n" +"Room: " + room_count + "    " + "Room id: " + r.GetRoomid());
                                    isFirst = false;
                                }

                                else
                                {
                                    b = System.Text.Encoding.ASCII.GetBytes("Room: " + room_count + "    " + "Room id: " + r.GetRoomid());
                                }
                                stream.Write(b, 0, b.Length);
                                Thread.Sleep(100);
                                room_count++;
                            }
                        }

                        else if (data == "3")
                        {
                            Byte[] d = new Byte[1024];
                            Int32 byt = stream.Read(d, 0, d.Length);
                            string responseData = System.Text.Encoding.ASCII.GetString(d, 0, byt);
                            Console.WriteLine("RoomId is:  {0}", responseData);
                            bool isvalid = false;
                            Room temp = null;
                            foreach (Room r in roomslist)
                            {
                                if (r.GetRoomid() == Int32.Parse(responseData))
                                {
                                    isvalid = true;
                                    temp = r;
                                    break;
                                }

                            }

                            if (isvalid)
                            {
                                Console.WriteLine("Valid room");
                                byte[] b = System.Text.Encoding.ASCII.GetBytes("Valid Room");
                                stream.Write(b, 0, b.Length);
                                bool inRoom = true;
                                byte[] roombytes = null;
                                temp.addClient(client);
                                foreach (string s in temp.getmessages())
                                {
                                    byte[] msgbyte = System.Text.Encoding.ASCII.GetBytes(s);
                                    stream.Write(msgbyte, 0, msgbyte.Length);
                                    Thread.Sleep(5);
                                }
                                while (inRoom)
                                {
                                    data = null;
                                    if (roombytes == null)
                                    {
                                        roombytes = new byte[2048];
                                    }
                                    int j;
                                    j=stream.Read(roombytes, 0, roombytes.Length);

                                    // Translate data bytes to a ASCII string.
                                    data = System.Text.Encoding.ASCII.GetString(roombytes, 0, j);
                                    if (data == "LR")
                                    {
                                        inRoom = false;
                                        temp.removeClient(client);
                                        Console.WriteLine("Leaving Room");
                                    }
                                    else
                                    {


                                        data = data.Remove(0, 4);
                                        data = this.clientname + ": " + data;
                                        temp.AddMessage(data);
                                        Console.WriteLine("ReceivedMessage: {0}", data);


                                        foreach (TcpClient c in temp.getclients())
                                        {
                                            if (!c.Equals(client))
                                            {
                                                NetworkStream n = c.GetStream();
                                                byte[] msgbyte = System.Text.Encoding.ASCII.GetBytes(data);
                                                n.Write(msgbyte, 0, msgbyte.Length);

                                            }
                                        }



                                    }





                                }
                            }

                            else
                            {
                                Console.WriteLine("Invalid room");
                                byte[] b = System.Text.Encoding.ASCII.GetBytes("InValid Room");
                                stream.Write(b, 0, b.Length);
                            }


                        }

                        else
                        {
                            client.Close();
                            Server.user_count -= 1;
                            Console.WriteLine(Server.user_count);
                            break;
                        }

                          
                        
                    }
                    //else
                    //{
                    //    Server.user_count -= 1;
                    //    break;
                    //}
                }

                catch (Exception ex)
                {
                    foreach(Room r in roomslist)
                    {
                        foreach(TcpClient c in r.getclients())
                        {
                            if(c.Equals(client))
                            {
                                r.removeClient(c);
                            }
                        }
                    }
                    client.Close();
                    Server.user_count -= 1;
                    Console.WriteLine(Server.user_count);
                }


            }

        }
    }
           
 
}
