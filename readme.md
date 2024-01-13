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
> This library is in alpha version of development and might not be stable. Bugs, crashes, and breaking changes are expected. Please do not use in production till `v0.1` is officially released.

# Installation
Support for UPM and NuGet coming soon.

## Method #1: Git Submodule
If your project is a git repository, use the following command to clone this repository as a submodule:
`git submodule add https://github.com/muaazali/si-net`

Submodules can be fetched from the remote origin to easily update the library.
> [!NOTE]
> Read more about Git Submodules [here](https://git-scm.com/book/en/v2/Git-Tools-Submodules).

## Method #2: Download
Clone/Download the reposity into the project folder.
> [!NOTE]
> For Unity, clone/download the repository into the `Assets` folder.

> [!IMPORTANT]  
> Make sure your version of .NET has built-in support for `Newtonsoft.JSON`, or install it yourself. Unity 2021+ already support it.

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

> [!IMPORTANT]
> Make sure to use the `Client.Send()` methods *AFTER* the server has been initialized properly and the client has connected to it. Otherwise, the `Client.Send()` method might send packets which will not be processed by the server.<br></br>
> Use `Client.OnConnectedToServer` event to check when a client has successfully established a connection with the server. After this event has been triggered, all messages will be properly processed.

# Documentation
Detailed documentation coming soon. Refer to [Usage](#usage) for now.

# Planned Features
## v0.1
- (COMPLETED) Support for better client connection events. Indicate other clients when a new client has joined.
- (COMPLETED) Add better exception handling.
- Polish for release.
## v0.1.1
- Add full compatibility with other socket libraries.
## v0.2
- Support for rooms.
- Support for RPCs.
## v0.3
- Support for WebSockets.
- Support for server-side middlewares/preprocessors.
## v0.4
- Support for UDP Sockets.
