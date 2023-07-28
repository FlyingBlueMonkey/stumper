// ***********************************************************************
// Assembly         : stumper
// Author           : MattEgen
// Created          : 06-21-2023
//
// Last Modified By : MattEgen
// Last Modified On : 06-22-2023
// ***********************************************************************
// <copyright file="Program.cs" company="stumper">
//     Copyright (c) . All rights reserved.
// </copyright>
// <summary></summary>
// ***********************************************************************
using System.Net.Sockets;
using System.Net;
using System.Text;
using System;
using System.Runtime.CompilerServices;

/// <summary>
/// The stumper namespace.
/// </summary>
namespace stumper
{
    /// <summary>
    /// Class Program.
    /// </summary>
    internal class Program
    {
        /// <summary>
        /// The message
        /// </summary>

        /// <summary>
        /// Defines the entry point of the application.
        /// </summary>
        /// <param name="args">The arguments.</param>
        static void Main(string[] args)
        {
            // Call the async version of the Main method. Since we're calling networking functions async we need an async handler
            MainAsync(args).Wait();

        }

        /// <summary>
        /// Main as an asynchronous operation.
        /// </summary>
        /// <param name="args">The arguments.</param>
        /// <returns>A Task representing the asynchronous operation.</returns>
        static async Task MainAsync(string[] args)
        {
            int x = 0;
            while (true)
            {
                string messageToSend = args[1] + " " + x.ToString();
                Message message = new Message(messageToSend, Message.MessageStandards.RFC5424, MessageHeader.FacilityType.local4, MessageHeader.SeverityLevel.Debug);
                IPAddress ipAddress;
                //check if we were given an IP address or a URL and set object appropriately
                if (!IPAddress.TryParse(args[0], out ipAddress))
                {
                    //arg[0] is a uri so we need to look it up in DNS
                    IPHostEntry ipHostInfo = await Dns.GetHostEntryAsync(args[0]);
                    //use the first IP address in the DNS results and set our IPAddress object to that
                    ipAddress = IPAddress.Parse(ipHostInfo.AddressList[0].ToString());
                }
                IPEndPoint ipEndPoint = new(ipAddress, 514);
                using Socket client = new(
                    ipEndPoint.AddressFamily,
                    SocketType.Dgram,
                    ProtocolType.Udp);

                client.Connect(ipEndPoint);
                // Send the message
                Console.WriteLine(String.Format("Sending:'{0}'", message.ToString()));
                Byte[] sendBytes = Encoding.ASCII.GetBytes(message.ToString());
                client.Send(sendBytes, sendBytes.Length, SocketFlags.None);

                client.Dispose();
                x++;
                Thread.Sleep(1000);
            }
        }
    }
}

