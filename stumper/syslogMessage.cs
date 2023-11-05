// ***********************************************************************
// Assembly         : stumper
// Author           : MattEgen
// Created          : 06-21-2023
//
// Last Modified By : MattEgen
// Last Modified On : 08-06-2023
// ***********************************************************************
// <copyright file="syslogMessage.cs" company="stumper">
//     Copyright (c) . All rights reserved.
// </copyright>
// <summary></summary>
// ***********************************************************************
using System;
using System.Buffers.Text;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.PortableExecutable;
using System.Text;
using System.Text.Unicode;
using System.Threading.Tasks;
using static System.Runtime.InteropServices.JavaScript.JSType;
using System.Xml.Linq;
using System.Net;
using System.Globalization;
using static stumper.Message;
using System.Diagnostics;

//6.Syslog Message Format

//   The syslog message has the following ABNF [RFC5234] definition:

//SYSLOG - MSG = HEADER SP STRUCTURED-DATA [SP MSG]

//HEADER = PRI VERSION SP TIMESTAMP SP HOSTNAME
//                        SP APP-NAME SP PROCID SP MSGID
//      PRI             = "<" PRIVAL ">"
//      PRIVAL          = 1*3DIGIT ; range 0 .. 191
//      VERSION         = NONZERO-DIGIT 0*2DIGIT
//      HOSTNAME        = NILVALUE / 1*255PRINTUSASCII

//      APP-NAME        = NILVALUE / 1*48PRINTUSASCII
//      PROCID          = NILVALUE / 1*128PRINTUSASCII
//      MSGID           = NILVALUE / 1*32PRINTUSASCII

//      TIMESTAMP       = NILVALUE / FULL-DATE "T" FULL-TIME
//      FULL-DATE       = DATE-FULLYEAR "-" DATE-MONTH "-" DATE-MDAY
//      DATE-FULLYEAR   = 4DIGIT
//      DATE-MONTH      = 2DIGIT  ; 01 - 12
//      DATE - MDAY = 2DIGIT; 01 - 28, 01 - 29, 01 - 30, 01 - 31 based on
//                                ; month / year
//      FULL - TIME = PARTIAL - TIME TIME - OFFSET
//      PARTIAL - TIME = TIME - HOUR ":" TIME - MINUTE ":" TIME - SECOND
//                        [TIME - SECFRAC]
//      TIME - HOUR = 2DIGIT; 00 - 23
//      TIME - MINUTE = 2DIGIT; 00 - 59
//      TIME - SECOND = 2DIGIT; 00 - 59
//      TIME - SECFRAC = "." 1 * 6DIGIT
//      TIME-OFFSET     = "Z" / TIME-NUMOFFSET
//      TIME-NUMOFFSET  = ("+" / "-") TIME - HOUR ":" TIME - MINUTE

//      STRUCTURED - DATA = NILVALUE / 1 * SD - ELEMENT
//      SD - ELEMENT = "[" SD - ID * (SP SD - PARAM) "]"
//      SD - PARAM = PARAM - NAME "=" % d34 PARAM - VALUE % d34
//      SD - ID = SD - NAME
//      PARAM - NAME = SD - NAME
//      PARAM - VALUE = UTF - 8 - STRING; characters '"', '\' and
//                                     ; ']' MUST be escaped.
//      SD-NAME         = 1*32PRINTUSASCII
//                        ; except '=', SP, ']', %d34 (")

//      MSG             = MSG-ANY / MSG-UTF8
//      MSG-ANY         = *OCTET ; not starting with BOM
//      MSG-UTF8        = BOM UTF-8-STRING
//      BOM             = %xEF.BB.BF

//      UTF-8-STRING    = *OCTET ; UTF - 8 string as specified
//                        ; in RFC 3629

//      OCTET           = %d00-255
//      SP              = %d32
//      PRINTUSASCII    = %d33-126
//      NONZERO-DIGIT   = %d49-57
//      DIGIT           = %d48 / NONZERO-DIGIT
//      NILVALUE        = "-"
/// <summary>
/// The stumper namespace.
/// </summary>
namespace stumper
{
    /// <summary>
    /// Class Message.
    /// </summary>
    public class Message
    {
        #region Public Variables
        /// <summary>
        /// The header
        /// </summary>
        public MessageHeader header;

        /// <summary>
        /// Gets or sets the message standard.
        /// </summary>
        /// <value>The message standard.</value>
        public MessageStandards MessageStandard { get; set; }
        /// <summary>
        /// Enum MessageStandards
        /// </summary>
        public enum MessageStandards
        {
            /// <summary>
            /// The rf C3164
            /// </summary>
            RFC3164 = 0,
            /// <summary>
            /// The rf C5424
            /// </summary>
            RFC5424 = 1
        }

        #endregion

        #region Private Variables
        /// <summary>
        /// The message body
        /// </summary>
        private string messageBody = "Stumper - Default Test Message";

        #endregion

        #region Public Constants
        /// <summary>
        /// The nil value
        /// </summary>
        public const string NilValue = "-";

        #endregion

        #region Private Constants


        #endregion

        #region Creators
        /// <summary>
        /// Initializes a new instance of the <see cref="Message"/> class.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <param name="messageStandard">The message standard.</param>
        /// <param name="facility">The facility.</param>
        /// <param name="severity">The severity.</param>
        public Message(string message, MessageStandards messageStandard,MessageHeader.FacilityType facility, MessageHeader.SeverityLevel severity) 
        {
            header = new MessageHeader(messageStandard,facility,severity,"Stumper");
            MessageBody = message;
            MessageStandard = messageStandard;
            header.Facility = facility;
            header.Severity = severity;

            if (MessageStandard == MessageStandards.RFC3164)
            {
                header.RFC3164 = true;
            }
        }

        #endregion

        #region Public Functions
        /// <summary>
        /// Gets or sets the message body.
        /// </summary>
        /// <value>The message body.</value>
        public string MessageBody
        {
            get
            {
                return messageBody;
            }
            set
            {
                messageBody = value;
            }

        }
        #endregion
        /// <summary>
        /// Converts to string.
        /// </summary>
        /// <returns>string.</returns>
        public override string ToString()
        {
            string message = string.Empty;
            //SYSLOG-MSG = HEADER SP STRUCTURED-DATA [SP MSG]
            if(MessageStandard==MessageStandards.RFC3164)
            {
                message = string.Format("{0} {1}", header, MessageBody);
            }
            else
            {
                message = string.Format("{0} {1} {2}", header, NilValue, MessageBody);
            }
            
            return message;
        }
    }

    /// <summary>
    /// Class StructuredData.
    /// </summary>
    public class StructuredData
    {

    }

    /// <summary>
    /// Class syslogMessageHeader.
    /// </summary>
    public class MessageHeader
    {
        #region Public Variables
        /// <summary>
        /// Gets or sets the version.
        /// </summary>
        /// <value>The version.</value>
        public int Version { get; set; }
        /// <summary>
        /// Gets or sets the time stamp.
        /// </summary>
        /// <value>The time stamp.</value>
        public string TimeStamp { get; set; }


        /// <summary>
        /// Enum syslogMessageFacility
        /// </summary>
        public enum FacilityType
        {
            /// <summary>
            /// kernel messages
            /// </summary>
            KERN = 0,
            /// <summary>
            /// user-level messages
            /// </summary>
            USER = 1,
            /// <summary>
            /// mail system
            /// </summary>
            MAIL = 2,
            /// <summary>
            /// system daemons
            /// </summary>
            DAEMON = 3,
            /// <summary>
            /// security/authorization messages
            /// </summary>
            AUTH = 4,
            /// <summary>
            /// messages generated internally by syslogd
            /// </summary>
            SYSLOG = 5,
            /// <summary>
            /// line printer subsystem
            /// </summary>
            LPR = 6,
            /// <summary>
            /// network news subsystem
            /// </summary>
            NEWS = 7,
            /// <summary>
            /// UUCP subsystem
            /// </summary>
            UUCP = 8,
            /// <summary>
            /// clock daemon
            /// </summary>
            CRON = 9,
            /// <summary>
            /// security/authorization messages
            /// </summary>
            AUTHPRIV = 10,
            /// <summary>
            /// FTP daemon
            /// </summary>
            FTP = 11,
            /// <summary>
            /// NTP subsystem
            /// </summary>
            NTP = 12,
            /// <summary>
            /// log audit
            /// </summary>
            AUDIT = 13,
            /// <summary>
            /// log alert
            /// </summary>
            ALERT = 14,
            /// <summary>
            /// clock daemon (note 2)
            /// </summary>
            CLOCK = 15,
            /// <summary>
            /// local use 0  (local0)
            /// </summary>
            LOCAL0 = 16,
            /// <summary>
            /// local use 1  (local1)
            /// </summary>
            LOCAL1 = 17,
            /// <summary>
            /// local use 2  (local2)
            /// </summary>
            LOCAL2 = 18,
            /// <summary>
            /// local use 3  (local3)
            /// </summary>
            LOCAL3 = 19,
            /// <summary>
            /// local use 4  (local4)
            /// </summary>
            LOCAL4 = 20,
            /// <summary>
            /// local use 5  (local5)
            /// </summary>
            LOCAL5 = 21,
            /// <summary>
            /// local use 6  (local6)
            /// </summary>
            LOCAL6 = 22,
            /// <summary>
            /// local use 7  (local7)
            /// </summary>
            LOCAL7 = 23

        }
        /// <summary>
        /// Enum syslogMessageSeverity
        /// </summary>
        public enum SeverityLevel
        {
            /// <summary>
            /// System unusable
            /// </summary>
            EMERGENCY = 0,
            /// <summary>
            /// Action must be taken immediately
            /// </summary>
            ALERT = 1,
            /// <summary>
            /// Critical conditions
            /// </summary>
            CRITICAL = 2,
            /// <summary>
            /// Error conditions
            /// </summary>
            ERROR = 3,
            /// <summary>
            /// Warning conditions
            /// </summary>
            WARNING = 4,
            /// <summary>
            /// Normal but significant conditions
            /// </summary>
            NOTICE = 5,
            /// <summary>
            /// Informational messages
            /// </summary>
            INFORMATIONAL = 6,
            /// <summary>
            /// Debug level messages
            /// </summary>
            DEBUG = 7
        }
        /// <summary>
        /// Gets or sets the facility.
        /// </summary>
        /// <value>The facility.</value>
        public FacilityType Facility { get; set; }
        /// <summary>
        /// Gets or sets the severity.
        /// </summary>
        /// <value>The severity.</value>
        public SeverityLevel Severity { get; set; }

        /// <summary>
        /// The hostname of the provider of the log.  Preference order:1.  FQDN, 2.  Static IP address, 3.  hostname, 4.  Dynamic IP address
        /// 5.  the NILVALUE
        /// </summary>
        /// <value>The name of the host sending the message</value>
        public string HostName 
        {
            get
            {
                return hostName;
            }
            set 
            {
                hostName = value;
            }
        }

        /// <summary>
        /// Gets or sets the name of the application.
        /// </summary>
        /// <value>The name of the application.</value>
        public string AppName
        {
            get
            {
                return appName;
            }

            set
            {
                appName = value;
            }
        }

        /// <summary>
        /// Gets or sets the process identifier.
        /// </summary>
        /// <value>The process identifier.</value>
        public string ProcessId
        {
            get
            {
                return processId;
            }

            set
            {
                processId = value;
            }
        }

        /// <summary>
        /// Gets or sets the message identifier.
        /// </summary>
        /// <value>The message identifier.</value>
        public string MessageId
        {
            get
            {
                return messageId;
            }

            set
            {
                messageId = value;
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether [rf C3164].
        /// </summary>
        /// <value><c>true</c> if [rf C3164]; otherwise, <c>false</c>.</value>
        public bool RFC3164
        {
            get
            {
                return rfc3164;
            }
            set
            {
                rfc3164 = value; 
            }
        }

        #endregion

        #region Private Variables
        /// <summary>
        /// Gets or sets the pri value
        /// </summary>
        /// <value>The pri.</value>
        private string PRI { get; set; }
        /// <summary>
        /// The host name
        /// </summary>
        private string hostName = System.Net.Dns.GetHostEntry("localhost").HostName; //default to the local machine name unless overridden
        /// <summary>
        /// The application name
        /// </summary>
        private string appName = "stumper";
        /// <summary>
        /// The process identifier
        /// </summary>
        private string processId = Process.GetCurrentProcess().Id.ToString();
        /// <summary>
        /// The message identifier
        /// </summary>
        private string messageId = "1";
        /// <summary>
        /// The RFC3164
        /// </summary>
        private bool rfc3164 = false;
        #endregion

        #region Constructors
        //HEADER = PRI VERSION SP TIMESTAMP SP HOSTNAME SP APP-NAME SP PROCID SP MSGID
        /// <summary>
        /// Initializes a new instance of the <see cref="T:stumper.MessageHeader" /> class.
        /// </summary>
        /// <param name="messageStandard">The message standard.</param>
        /// <param name="facility">The facility.</param>
        /// <param name="severity">The severity.</param>
        /// <param name="appName">Name of the application.</param>
        public MessageHeader(MessageStandards messageStandard, FacilityType facility, SeverityLevel severity,string appName)
        {
            if(messageStandard == MessageStandards.RFC3164)
            {
                rfc3164= true;
            }
            this.Severity = severity;
            this.Facility = facility;
            this.Version = 1;
            this.appName = appName;
            this.PRI = CalculatePriValue();

        }

        #endregion
        /// <summary>
        /// Calculates the pri value.
        /// </summary>
        /// <returns>string.</returns>
        public string CalculatePriValue()
        {
            int pri = 0;
            string privalue = string.Empty;
            pri = ((int)Facility * 8) + (int)Severity;
            privalue = string.Format("<{0}>", pri);
            return privalue;
        }
        /// <summary>
        /// Converts to string.
        /// </summary>
        /// <returns>string.</returns>
        public override string ToString()
        {

            string messageHeader = string.Empty;
            if (rfc3164)
            {
                // We're using the *local* date-time to make this more compatible with older BSD systems that didn't specify timezone data
                this.TimeStamp = DateTime.Now.ToString("MMM dd hh:mm:ss");
                messageHeader = string.Format("{0}{1} {2}", this.PRI, this.TimeStamp, this.HostName);
            }
            else
            {
                this.TimeStamp = DateTime.UtcNow.ToString("yyyy-MM-dd'T'HH:mm:ss.fffK", DateTimeFormatInfo.InvariantInfo);
                //HEADER = PRI VERSION SP TIMESTAMP SP HOSTNAME SP APP-NAME SP PROCID SP MSGID
                messageHeader = string.Format("{0}{1} {2} {3} {4} {5} {6}", this.PRI, this.Version, this.TimeStamp, this.HostName, this.AppName, this.ProcessId, this.MessageId);
            }
            return messageHeader;
        }
    }
}
