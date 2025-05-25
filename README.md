# SignalR demo

This demo showcases the use of **SignalR**, a real-time communication library for ASP.NET Core that allows server-side code to push content to connected clients instantly.

## Features

- Real-time messaging between server and multiple clients
- Supports WebSockets, Server-Sent Events, and Long Polling automatically
- Easy to integrate with ASP.NET Core applications
- Enables chat apps, live notifications, real-time dashboards, and more

## Basic Example

```csharp
// Hub class
public class ChatHub : Hub
{
    public async Task SendMessage(string user, string message)
    {
        await Clients.All.SendAsync("ReceiveMessage", user, message);
    }
}
```

## How to run

1. Create a new ASP.NET Core project.
2. Add SignalR package: `Microsoft.AspNetCore.SignalR`
3. Define a Hub class as shown above.
4. Configure SignalR in or `Program.cs`:

```csharp
app.MapHub<ChatHub>("/Chat");
```

5. Connect from a client (JavaScript example):

```javascript
const connection = new signalR.HubConnectionBuilder()
    .withUrl("/chathub")
    .build();

connection.on("ReceiveMessage", (user, message) => {
    console.log(`${user}: ${message}`);
});

connection.start().catch(err => console.error(err));
```

---

This demo provides the foundation to build real-time web apps easily with SignalR.
