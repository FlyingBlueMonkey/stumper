# stumper
## a very (very) simple syslog generator.

Usage:
```
 stumper <server> <facility> <severity> <message> [options]
```
Arguments:
```
<server>                                                                                                 The server to send the message(s) to.  Can be either an IP address or name

<ALERT|AUDIT|AUTH|AUTHPRIV|CLOCK|CRON|DAEMON|FTP|KERN|LOCAL0|LOCAL1|LOCAL2|LOCAL3|LOCAL4|LOCAL5|LOCAL6|  The facility code

LOCAL7|LPR|MAIL|NEWS|NTP|SYSLOG|USER|UUCP>

<ALERT|CRITICAL|DEBUG|EMERGENCY|ERROR|INFORMATIONAL|NOTICE|WARNING>                                      The message severity

<message>                                                                                                The message to send
```

Options:
```
--file <file>                        A file to load and send to the server.  Note:  When used, the 'message' argument will be ignored but must still be provided

--repeatFor <repeatFor>              Sending the message a specific number of times, and then stop.  Default is 1

--messageTiming <messageTiming>      The amount of time, in milliseconds, to delay between each message.  Default is 1000

--messageStandard <RFC3164|RFC5424>  Option to change RFC standard for the message(s)

--version                            Show version information

-?, -h, --help                       Show help and usage information
```
