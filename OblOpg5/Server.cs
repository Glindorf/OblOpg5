using OblOpg1;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace OblOpg5
{
    public class Server
    {
        private static List<Beer> _beers = new List<Beer>();

        public void Start()
        {
            TcpListener server = null;
            try
            {
                Int32 port = 4646;
                IPAddress localAddr = IPAddress.Loopback;
                server = new TcpListener(localAddr, port);
                server.Start();
                Console.WriteLine("Server started");
                while (true)
                {
                    TcpClient connectionSocket = server.AcceptTcpClient();
                    Task.Run(() =>
                    {
                        TcpClient tempSocket = connectionSocket;
                        DoClient(tempSocket);
                    });
                }

                server.Stop();
            }
            catch (SocketException e)
            {
                Console.WriteLine("SocketException: {0}", e);
            }

        }

        private void DoClient(TcpClient connectionSocket)
        {

            Console.WriteLine("Server activated");
            Stream ns = connectionSocket.GetStream();

            StreamReader sr = new StreamReader(ns);
            StreamWriter sw = new StreamWriter(ns);
            sw.AutoFlush = true;
            bool keepGoing = true;
            while (keepGoing)
            {
                string message = sr.ReadLine();
                switch (message)
                {
                    case "Stop":
                        keepGoing = false;
                        break;
                    case "HentAlle": // henter alle beer-objekter fra serveren, linie to er tom
                        foreach (var item in _beers)
                        {
                            sw.WriteLine(item.ToString());
                        }
                        break;
                    case "Hent": // henter et beer-objekt med pågældende id, fx Hent 2
                        try
                        {
                            int id = Convert.ToInt32(sr.ReadLine());
                            sw.WriteLine(_beers.Find(beer => beer.Id == id));
                        }
                        catch (Exception e)
                        {
                            Console.WriteLine("Fejl i input");
                        }
                        break;
                    case "Gem": // gemmer et beer-objekt - Skriv øl i JSON-format fx  {"Id":33,"Name":"Mums","Price":33,"Abv":6}
                        string beerJson = sr.ReadLine().Trim();
                        if (beerJson.Length != 0)
                        {
                            try
                            {
                                Beer newBeer = JsonConvert.DeserializeObject<Beer>(beerJson);
                                _beers.Add(newBeer);
                            }
                            catch (Exception e)
                            {
                              //  sw.WriteLine("Matcher ikke Json-objekt");
                              sw.WriteLine(e);
                            }
                        }
                        break;
                    default: // ikke muligt
                        sw.WriteLine("Fejl");
                        break;
                }
            }

            ns.Close();
            connectionSocket.Close();



        }
    }
}
