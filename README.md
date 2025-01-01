# MTCG
Monster Trading Card Game (MTCG) is a project I developed as part of my studies at UAS Technikum Vienna for the course Software Engineering Lab 1. The application implements a REST server that serves as the backend for the game.

## Setup
The application uses a PostgreSQL database to persist data. The simplest way to set it up is by using Docker:

```bash
docker run -d --rm --name postgresdb -e POSTGRES_USER=swen1 \
-e POSTGRES_PASSWORD=passwordswen1 -p 5432:5432 -v pgdata:/var/lib/postgresql/data \
postgres
```

Next, a database named `mtcg` must be created:

```bash
docker exec -it postgresdb bash
psql -U swen1
CREATE DATABASE mtcg;
```

## Usage
After starting, the application automatically attempts to connect to the PostgreSQL server and creates all the tables required by the application in the `mtcg` database. Once the setup is complete, the application listens for incoming connections on port `10001`.
