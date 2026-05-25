# MyRedis 
A lightweight, in-memory key-value cache server built from scratch in **C# (.NET 9)**, inspired by Redis. MyRedis implements the **RESP (Redis Serialization Protocol)**, making it compatible with standard Redis clients.

---

## 📌 Table of Contents

- [Overview](#overview)
- [Architecture](#architecture)
- [Project Structure](#project-structure)
- [Supported Commands](#supported-commands)
- [Getting Started](#getting-started)
  - [Prerequisites](#prerequisites)
  - [Build & Run](#build--run)
  - [Connecting with redis-cli](#connecting-with-redis-cli)
- [How It Works](#how-it-works)
- [Tech Stack](#tech-stack)
- [Contributing](#contributing)

---

## Overview

MyRedis is a from-scratch implementation of a Redis-like in-memory cache server. It listens for TCP connections, parses commands using the RESP protocol, executes them against an in-memory data store, and returns responses — just like the real Redis.

This project is a deep dive into:
- TCP networking in .NET
- The Redis RESP wire protocol
- In-memory data structures
- Command pattern design

---

## Architecture

```
┌─────────────────────────────────────────────────┐
│                   Client                        │
│           (redis-cli / any TCP client)          │
└───────────────────────┬─────────────────────────┘
                        │  TCP Connection (RESP)
                        ▼
┌─────────────────────────────────────────────────┐
│               TcpServer (Server/)               │
│     Accepts connections, spawns handlers        │
└───────────────────────┬─────────────────────────┘
                        │
                        ▼
┌─────────────────────────────────────────────────┐
│             RespParser (Protocol/)              │
│   Deserializes raw bytes → command + args       │
└───────────────────────┬─────────────────────────┘
                        │
                        ▼
┌─────────────────────────────────────────────────┐
│           Command Handlers (Commands/)          │
│   SET, GET, DEL, EXISTS, EXPIRE, TTL, PING …   │
└───────────────────────┬─────────────────────────┘
                        │
                        ▼
┌─────────────────────────────────────────────────┐
│              DataStore (Core/)                  │
│     Thread-safe in-memory dictionary + TTL      │
└─────────────────────────────────────────────────┘
```

---

## Project Structure

```
MyRedis/
├── Program.cs              # Entry point — starts the TCP server
├── MyRedis.csproj          # .NET 9 project file
├── MyRedis.slnx            # Solution file
│
├── Server/                 # TCP server & client connection handling
│   └── TcpServer.cs
│
├── Protocol/               # RESP protocol parsing & serialization
│   └── RespParser.cs
│
├── Commands/               # Individual command implementations
│   ├── SetCommand.cs
│   ├── GetCommand.cs
│   ├── DelCommand.cs
│   └── ... (more commands)
│
└── Core/                   # Core data store and shared utilities
    └── DataStore.cs
```

---

## Supported Commands

| Command              | Description                                      |
|----------------------|--------------------------------------------------|
| `PING`               | Returns `PONG` — health check                    |
| `SET key value`      | Store a string value                             |
| `GET key`            | Retrieve a string value                          |
| `DEL key [key ...]`  | Delete one or more keys                          |
| `EXISTS key`         | Check if a key exists                            |
| `EXPIRE key seconds` | Set a TTL (time-to-live) on a key                |
| `TTL key`            | Get remaining TTL for a key                      |

> More commands may be supported — check the `Commands/` folder for the full list.

---

## Getting Started

### Prerequisites

- [.NET 9 SDK](https://dotnet.microsoft.com/download/dotnet/9.0)
- (Optional) `redis-cli` for testing

### Build & Run

```bash
# Clone the repository
git clone https://github.com/ashokkandpal/MyRedis.git
cd MyRedis

# Build the project
dotnet build

# Run the server (defaults to port 6379)
dotnet run
```

You should see:
```
MyRedis Server Starting...
```

### Connecting with redis-cli

Once the server is running, you can connect using the standard Redis CLI:

```bash
redis-cli -p 6379
```

Try it out:
```
127.0.0.1:6379> PING
PONG

127.0.0.1:6379> SET name "Ashok"
OK

127.0.0.1:6379> GET name
"Ashok"

127.0.0.1:6379> EXPIRE name 30
(integer) 1

127.0.0.1:6379> TTL name
(integer) 29

127.0.0.1:6379> DEL name
(integer) 1
```

---

## How It Works

1. **TCP Server** — `TcpServer` listens on a port and accepts client connections. Each client connection is handled on its own thread/task.

2. **RESP Parser** — Incoming bytes are parsed according to the [Redis Serialization Protocol (RESP)](https://redis.io/docs/reference/protocol-spec/). Arrays of bulk strings are decoded into a command name and its arguments.

3. **Command Dispatch** — The parsed command is dispatched to the appropriate handler class in the `Commands/` folder (e.g., `SetCommand`, `GetCommand`).

4. **Data Store** — Commands read from and write to the `DataStore` in `Core/`, which is a thread-safe in-memory dictionary. Keys with TTL are tracked and expired automatically.

5. **RESP Response** — The command handler serializes its result back into RESP format and writes it to the client socket.

---

## Tech Stack

- **Language:** C# 12
- **Runtime:** .NET 9
- **Protocol:** RESP (Redis Serialization Protocol)
- **Storage:** In-memory (`Dictionary` with TTL support)
- **Networking:** `System.Net.Sockets.TcpListener`

---

## Contributing

Contributions, issues, and feature requests are welcome!

1. Fork the repository
2. Create your feature branch: `git checkout -b feature/my-feature.`
3. Commit your changes: `git commit -m 'Add some feature'`
4. Push to the branch: `git push origin feature/my-feature`
5. Open a Pull Request

---

> Built with ❤️ by [Ashok Kandpal](https://github.com/ashokkandpal)
