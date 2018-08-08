# Socket Server Engine
C# .NET TCP/UDP Socket Server Engine

The is a sample implementation of sockets in C#
This contains a library with server to can open a TCP and/or UDP connection to accept multiple clients.

Each client is served in its own thread and will not close its connection. The server is also able to handle reconnections by the client and then able to continue to serve them.

This could be enhanced into a game server to serve mutliple users, or modified to handle resources connected to server, etc.
The idea here is to give an example how to implement both TCP and UDP on a single server, multiple client model, on a Windows service.


# Others
There exist many unrelated low level code, but I just included here just for sake of coverage.
You may or may not find them useful, but either way, use as you see fit.
