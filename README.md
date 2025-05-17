# IRC Server

A simple IRC server written in C# (.NET 8).
Listens on port 6667 and supports basic IRC commands.

## Features

- Handles multiple clients
- Supports NICK, USER, PRIVMSG, PING, QUIT, and LIST commands
- Broadcasts messages to all users in the channel

## Getting Started

### Prerequisites

- [.NET 8 SDK](https://dotnet.microsoft.com/download)
- [Docker](https://www.docker.com/) and [Docker Compose](https://docs.docker.com/compose/)

### Build and Run with Docker

1. **Build the Docker image:**
   ```sh
   docker compose build
    ```

2.  **Run the server:**

    ```sh
    docker compose up
    ```

    The server will listen on port 6667 inside the container.

3.  **Expose port to host (optional):**
    To connect from your host, ensure your `compose.yaml` file contains the following under the `services` section for your server:

    ```yaml
    ports:
      - "6667:6667"
    ```

### Connect to the Server

Use my IRC client to connect to `localhost:6667` (or your server's IP).

### Example Commands

/nick yourname
/msg #mychannel Hello world!
/list
/quit