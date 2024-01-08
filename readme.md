# Si-Net: Simplified Networking
A lightweight asynchronous socket-based networking library built to serve as an alternative to socket.io for C# .NET with a strong support for Unity.

## Table of Contents
1. [Overview](#overview)
3. [Installation](#installation)
4. [Usage](#usage)
5. [Documentation](#documentation)
6. [Planned Features](#planned-features)

# Overview
.NET has it's fair share of networking libraries but nothing with an API as simplified as [socket.io](https://socket.io/) libraries for JS. Si-Net aims to serve as a beginner-friendly networking option for applications that aim to use real-time socket communication.

Si-Net, as the name suggests, provides simplified public facing methods to easily communicate between a server and multiple clients.

> [!WARNING]  
> This library is in alpha version of development and is not stable. Bugs, crashes, and breaking changes are expected. Please do not use in production till `v0.1-stable` is officially released.

# Installation
Support for UPM and NuGet coming soon.

## Recommended Method: Git Submodule
Inside your project repository:
`git submodule add https://github.com/chaconinc/DbConnector`

## Alternative Method:
Clone/Download the reposity into the project folder.
> For Unity, clone/download the repository into the `Assets` folder.

> [!IMPORTANT]  
> Make sure your version of .NET has built-in support for `Newtonsoft.JSON`, or install it yourself. Unity 2021.3.10+ already support it.

# Usage
SiNet classes use the namespace `SiNet`.
```csharp
using SiNet;
```
## Server
All server-sided classes will need to use the namespace `SiNet.Server`.
```csharp
using SiNet.Server;
``` 
To create a basic server that listens to an event from clients and performs an action on it:
```csharp
Server server = new Server(new ServerConnectionSettings(5000));

server.On("HELLO_SERVER_FROM_CLIENT", (ConnectedClientInfo clientInfo, Message message) => {
    Console.WriteLine($"Client says: {message.data}");
    server.Send(clientInfo, "HELLO_CLIENT_FROM_SERVER", "What's up? Only you will see this message, not the other clients.");
    server.SendToAll("HELLO_ALL_CLIENTS", "Hello world! A new one joined us!");
});

```

## Client
All client-sided classes will need to use the namespace `SiNet.Client`.
```csharp
using SiNet.Client;
``` 
To create a basic client that connects to the server running your local machine, and listens to some events from the server and performs an action on it:
```csharp
Client client = new Client(new ClientConnectionSettings("127.0.0.1", 5000));

client.On("HELLO_CLIENT_FROM_SERVER", (Message message) => {
    Console.WriteLine($"Server says: {message.data}");
    client.Send("THANK_YOU_SERVER", "Thank you server, very cool.");
});
client.On("HELLO_ALL_CLIENTS", (Message message) => {
    Console.WriteLine($"Server told all clients: {message.data}");
});

client.Send("HELLO_SERVER_FROM_CLIENT", "Hey server!");

```

# Documentation
Detailed documentation coming soon. Refer to [Usage](#usage) for now.

# Planned Features
## v0.1.1
- Support for better client connection events. Indicate other clients when a new client has joined.
- Add better exception handling.
- Better documentation.
## v0.2
- Support for rooms.
- Support for RPCs.
## v0.3
- Support for server-side middlewares/preprocessors.
## v0.4
- Support for UDP Sockets.
