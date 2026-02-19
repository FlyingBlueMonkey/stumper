![Stumper Logo](https://github.com/flyingbluemonkey/stumper/blob/master/Stumper.png?raw=true)

# stumper

A very (very) simple syslog generator.

`stumper` is a small command-line tool for generating syslog messages and sending them to a remote syslog server. It is intended for quick testing/validation of:

- Syslog ingestion pipelines (SIEM, log collectors, appliances)
- Parsers and routing rules
- Connectivity to a syslog endpoint
- Volume/repeat testing with a controlled message rate

It supports sending messages using **RFC3164** or **RFC5424** formatting.

## Quick start

```text
stumper <server> <facility> <severity> <message> [options]
```

Example:

```text
stumper 192.0.2.10 LOCAL0 INFORMATIONAL "hello from stumper"
```

## Installation

### From releases

Download the appropriate binary from the repository releases page and place it somewhere on your `PATH`.

### Build from source

This is a C#/.NET console application.

```text
git clone https://github.com/FlyingBlueMonkey/stumper.git
cd stumper

dotnet build
```

Depending on your environment you may then run it via `dotnet run` or publish a self-contained binary:

```text
dotnet publish -c Release -o out
```

## Usage

```text
stumper <server> <facility> <severity> <message> [options]
```

### Arguments

```text
<server>
    The server to send the message(s) to. Can be either an IP address or DNS name.

<facility>
    The facility code. Supported values:
    ALERT | AUDIT | AUTH | AUTHPRIV | CLOCK | CRON | DAEMON | FTP | KERN |
    LOCAL0 | LOCAL1 | LOCAL2 | LOCAL3 | LOCAL4 | LOCAL5 | LOCAL6 | LOCAL7 |
    LPR | MAIL | NEWS | NTP | SYSLOG | USER | UUCP

<severity>
    The message severity. Supported values:
    ALERT | CRITICAL | DEBUG | EMERGENCY | ERROR | INFORMATIONAL | NOTICE | WARNING

<message>
    The message to send.
    Note: if you use `--file`, this argument is ignored, but it must still be provided to satisfy the CLI.
```

### Options

```text
--file <file>
    A file to load and send to the server.
    Note: when used, the `message` argument will be ignored (but must still be provided).

--repeatFor <repeatFor>
    Send the message a specific number of times and then stop.
    Default: 1

--messageTiming <messageTiming>
    Delay (in milliseconds) between each message.
    Default: 1000

--messageStandard <RFC3164|RFC5424>
    Select the syslog message format.

--version
    Show version information.

-?, -h, --help
    Show help and usage information.
```

## Examples

Send a single message:

```text
stumper syslog.example.com LOCAL0 NOTICE "test message"
```

Send 100 messages with no delay:

```text
stumper 192.0.2.10 LOCAL0 INFORMATIONAL "load test" --repeatFor 100 --messageTiming 0
```

Send using RFC5424 formatting:

```text
stumper 192.0.2.10 LOCAL0 INFORMATIONAL "rfc5424 test" --messageStandard RFC5424
```

Send a file (each line is sent as a message, depending on implementation):

```text
stumper 192.0.2.10 LOCAL0 WARNING "ignored" --file sample.log --repeatFor 1
```

## Notes on syslog (RFC3164 vs RFC5424)

- **RFC3164** is the "classic" BSD syslog format and is commonly accepted by many syslog receivers.
- **RFC5424** is the newer, more structured format and may be required by some collectors.

If your receiver rejects messages, try switching standards with `--messageStandard`.

## Exit codes

This tool is intended to behave like a typical CLI:

- `0` on success
- non-zero on failure (invalid arguments, unable to send, file not found, etc.)

(If you need specific exit codes documented, open an issue and include the behavior you want.)

## Troubleshooting

- **Nothing shows up on the server**: verify the target host/port and that any firewall rules allow syslog traffic.
- **DNS name fails**: try using an IP address for `<server>`.
- **Receiver complains about format**: try `--messageStandard RFC3164` or `--messageStandard RFC5424`.
- **Using `--file`**: remember you still must provide a `<message>` argument (it will be ignored).

## License

No license file is currently included in this repository. If you intend others to reuse this tool, consider adding a LICENSE.
