// ***********************************************************************
// Assembly         : stumper
// Author           : MattEgen
// Created          : 06-21-2023
//
// Last Modified By : MattEgen
// Last Modified On : 11-05-2023
// ***********************************************************************
// <copyright file="Program.cs" company="stumper">
//     Copyright (c) . All rights reserved.
// </copyright>
// <summary></summary>
// ***********************************************************************
using System.Net.Sockets;
using System.Net;
using System.Text;
using System.CommandLine;
using System;
using System.Runtime.CompilerServices;
using System.Diagnostics.Contracts;
using System.ComponentModel;
using System.Threading.Tasks.Dataflow;
using System.CommandLine.Invocation;

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
        /// Main as an asynchronous operation.
        /// </summary>
        /// <param name="args">The arguments.</param>
        /// <returns>A Task representing the asynchronous operation.</returns>
        static async Task Main(string[] args)
        {
            /* New Code:  Adding support for System.CommandLine features
             * Arguements: server, message, facility, severity
             * Options: messageStandard, repeatMessage(count), file(filePath,randomizeMessages), runTime(timeInMinutes),messageTiming(timingInSeconds)
             */


            RootCommand rootCommand = new RootCommand("A simple (very simple) syslog generator"); //This is where it all starts
            //Define Arguements
            var serverArgument = new Argument<string>(name: "server", description: "The server to send the message(s) to.  Can be either an IP address or name");
            var facilityArgument = new Argument<MessageHeader.FacilityType>(name: "facility", description: "The facility code");
            var severityArgument = new Argument<MessageHeader.SeverityLevel>(name: "severity", description: "The message severity");
            var messageArgument = new Argument<string>(name: "message", description: "The message to send");

            //Define Options
            var fileOption = new Option<FileInfo?>(name: "--file",description: "A file to load and send to the server.  Note:  When used, the 'message' arguement will be ignored but must still be provided");
            var runForOption = new Option<int>(name: "--runFor", description: "Send the message for a specific period of time in seconds, and then stop");
            runForOption.IsHidden = true; //Hidden for now to finish implementation
            var repeatForOption = new Option<int>(name: "--repeatFor", description: "Sending the message a specific number of times, and then stop.  Default is 1");
            var messageTimingOption = new Option<int>(name:"--messageTiming",description:"The amount of time, in milliseconds, to delay between each message.  Default is 1000");
            var rfcStandardOption = new Option<Message.MessageStandards>(name: "--messageStandard", "Option to change RFC standard for the message(s)");

            //Add the arguements to the rootCommand.  Arguments are required and ordinal position matters
            rootCommand.AddArgument(serverArgument);
            rootCommand.AddArgument(facilityArgument);
            rootCommand.AddArgument(severityArgument);
            rootCommand.AddArgument(messageArgument);
            
            //Add the options to the rootCommand.  Options are optional and ordinal position doesn't matter
            rootCommand.AddOption(fileOption);
            rootCommand.AddOption(runForOption);
            rootCommand.AddOption(repeatForOption);
            rootCommand.AddOption(messageTimingOption);
            rootCommand.AddOption(rfcStandardOption);

           rootCommand.SetHandler(async (string server, MessageHeader.FacilityType facility, MessageHeader.SeverityLevel severity, string message, FileInfo? file, int runFor, int repeatFor, int messageTiming) =>
                {
                    SendMessage(server, facility, severity, message,file,runFor,repeatFor,messageTiming);
                },
                serverArgument,facilityArgument,severityArgument,messageArgument, fileOption, runForOption,repeatForOption,messageTimingOption);

            await rootCommand.InvokeAsync(args);

        }

        /// <summary>
        /// Sends the message.
        /// </summary>
        /// <param name="server">The server.</param>
        /// <param name="facility">The facility.</param>
        /// <param name="severity">The severity.</param>
        /// <param name="messageToSend">The message to send.</param>
        /// <param name="file">The file.</param>
        /// <param name="runFor">The run for.</param>
        /// <param name="repeatFor">The repeat for.</param>
        /// <param name="messageTiming">The message timing.</param>
        /// <param name="standard">The standard.</param>
        static async void SendMessage(string server, MessageHeader.FacilityType facility, MessageHeader.SeverityLevel severity, string messageToSend, FileInfo? file = null, int runFor = 1, int repeatFor = 1, int messageTiming = 1000, Message.MessageStandards standard = Message.MessageStandards.RFC5424)
        {

            IPAddress ipAddress = await ResolveAddress(server); //Check / Resolve the server to an IpAddress
            if (file != null)
            {
                // a path to a file has been provided.  open the file, retrieve the strings in it, and send them.
                List<string> lines = File.ReadLines(file.FullName).ToList(); //Open the file one time and read its contents in to a variable
                for (int x = 0; x < repeatFor; x++) // Repeat sending the messages in the string for the value of repeatFor.  We'll send it at least once.
                {
                    foreach (string line in lines) //step through the file contents one by one
                    {
                        SendMessage(server, facility, severity, line); // send the message.
                        Thread.Sleep(messageTiming); //wait this amount of time between messages.
                    }
                }
            }
            else
            {
                //No file path received, use the messageToSend variable instead
                for (int x = 0; x < repeatFor; x++) // Repeat sending the messages in the string for the value of repeatFor.  We'll send it at least once.
                {
                        SendMessage(ipAddress, facility, severity, messageToSend);
                        Thread.Sleep(messageTiming); //wait this amount of time between messages.

                }

            }
        }
        /// <summary>
        /// Sends the message.
        /// </summary>
        /// <param name="ipAddress">The ip address.</param>
        /// <param name="facility">The facility.</param>
        /// <param name="severity">The severity.</param>
        /// <param name="messageToSend">The message to send.</param>
        static async void SendMessage(IPAddress ipAddress, MessageHeader.FacilityType facility, MessageHeader.SeverityLevel severity, string messageToSend)
        {
            Message message = new Message(messageToSend, Message.MessageStandards.RFC5424, facility, severity);
            IPEndPoint ipEndPoint = new(ipAddress, 514); //Create a new IPEndPoint with that address as the target and use port 514
            using Socket client = new(
                ipEndPoint.AddressFamily,
                SocketType.Dgram,
                ProtocolType.Udp);  //Open a UDP socket TODO: Add option for TCP?

            client.Connect(ipEndPoint);
            // Send the message
            Console.WriteLine(String.Format("Sending:'{0}'", message.ToString())); // logging line
            Byte[] sendBytes = Encoding.ASCII.GetBytes(message.ToString()); //Convert the message to byte array
            client.Send(sendBytes, sendBytes.Length, SocketFlags.None);  //Send the message
            client.Dispose();  //Clean up the client object
        }

        /// <summary>
        /// Resolves an address to an Ip
        /// </summary>
        /// <param name="server">The server.</param>
        /// <returns>IPAddress.</returns>
        private static async Task<IPAddress> ResolveAddress(string server)
        {
            IPAddress ipAddress;
            //check if we were given an IP address or a URL and set object appropriately
            if (!IPAddress.TryParse(server, out ipAddress))
            {
                //server is a uri so we need to look it up in DNS
                IPHostEntry ipHostInfo = await Dns.GetHostEntryAsync(server);
                //use the first IP address in the DNS results and set our IPAddress object to that
                ipAddress = IPAddress.Parse(ipHostInfo.AddressList[0].ToString());
            }

            return ipAddress;
        }
    }
}

