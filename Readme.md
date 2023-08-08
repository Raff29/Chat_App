# Chat App

Chat App is a simple chat application that allows users to communicate with each other in real time. It is written in C# using Docker and consists of two components: a client and a server.

## Features

- Users can create an user and log in to the chat app
- Users can send and receive text messages from other users
- Users can view their chat history with other users
- Users can log out from the chat app

## Prerequisites

To run this project, you need to have the following installed on your machine:

- [Docker](^1^)

## Installation

To install this project, follow these steps:

1. Clone this repository to your local machine
2. Change the volume path in the docker-compose.yml file to match your machine's directory structure
3. Run the following command in the root directory of the project:

```
docker-compose up -d
```

## Usage

1. Connect to the client container by running the following command:
```docker-compose exec client mono Client.exe```

2. Follow the instructions on the console to create an user 
3. Once logged in, you can start chatting with other users by entering the same chatroom
4. To log out, type /logout and press enter
