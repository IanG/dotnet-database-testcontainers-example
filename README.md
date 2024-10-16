# dotnet-database-testcontainers-example

## Introduction

When creating integration tests it can often be that external resources such as databases are:

- Based upon a [SQLite](https://www.sqlite.org/) database (via `Microsoft.EntityFrameworkCore.Sqlite`)
- Based upon an in-memory database (via `Microsoft.EntityFrameworkCore.InMemory`) 

In both of these cases you are not testing against the actual database you would be using in your production environment.  If you run against [PostgreSQL](https://www.postgresql.org/) in production, you should really be running your integration tests against the same database platform.

The challenge of this as close-to-production style of integration testing is having a PostgreSQL database consistently available without having to perform a lot of manual intervention.  This is before we even begin to consider how that database would be seeded with a schema and data to support whatever tests we intend to run.

### Introducing Test Containers

The [Test Containers](https://testcontainers.com/) open source project solves this problem by providing throwaway instances of databases, message brokers and a whole host of other services via [Docker](https://www.docker.com/).  Test Containers allow us to:

- Mirror as close to production infrastructure in our integration tests 
- Programatically spin up and configure containers before our integration tests commence
- Automatically destroy containers upon completion of integration tests

You can view all the supported module/container types [here](https://testcontainers.com/modules/) and you will find great supporting documentation for use with the .NET Framework [here](https://dotnet.testcontainers.org/).

For the purposes of this example we want to use a PostgreSQL container loaded with movie data to support our `/api/movies` endpoint in our `API` project.

**Note**: to use this project you will need to ensure you have Docker installed and running on your workstation.

## Running/Debugging The Application

### Setting up a PostgreSQL instance for development/debugging

There is `docker-compose.yml` file in the root of the cloned repository.  If you do:

```
docker compose up -d
```
This will start the database.   If you do:

```
docker compose down --volumes
```
This will stop the PostgreSQL container and remove its associated volumes.  If you wish to keep re-using the container once it is created just omit the `--volumes` part of the above command and your data will remain between up/down operations.

#### How is the database schema and data created ?

In the root of the cloned repository you will find the `etc/docker-entrypoint-initdb.d` directory.  This directory is mounted into the container when it is created.  This directory contains a file called `01-create-movies-db.sql` which will be executed within the container the first time it starts.  This script creates:

- A new database called `movies`
- A new user called `moviesuser` with a password and appropriate permissions
- Connects to the `movies` database and creates the tables and data required by the application

The `appsettings.json` file in the `API` project has a connection string called `MoviesDb` that can connect to this container whilst running and debugging.

## The `test/Integration` Project.

In this project we depend upon the following Test Containers nuget package:

- `Testcontainers.PostgreSql`

This nuget package provides the ability to create short-lived PostgreSQL docker image configured with:
- A specific PostreSQL image/version
- A named database with a username and password
- Port Bindings
- Volume Bindings

The tests in this project make use of [xUnit](https://xunit.net/) and [Fluent Assertions](https://fluentassertions.com/) to orchestrate our tests.  

### MoviesControllerTests 

If we look at the test 

- `TestingContainersExample.Tests.Integration.API.Controllers.MoviesControllerTests` 
 
we want to have our database available for the lifetime of the test suite.  We can use a `WebApplicationFactory` alongside a `PostgreSqlContainer` test to achieve this.

### How does the test PostgreSQL container get created ?

The test class implements/extends `IClassFixture<IntegrationTestWebApplicationFactory>`.  xUnit [class fixtures](https://xunit.net/docs/shared-context) are a shared context that exists for all tests in the class. In our case this is:

- A `WebApplicationFactory` hosting our API.
- An instance of a `PostgreSqlContainer` we can connect to from our `MoviesDbContext` in the service collection of the web application.

In the constructor of `IntegrationTestWebApplicationFactory` we use a `PostgreSqlBuilder` to define what our PostgreSQL image should contain.  You will see it:

- Uses the very latest PostgreSQL image `postgres:latest`
- Names the database `movies`
- Creates a user called `moviesuser` with an associated password
- Mounts the `scripts/docker-entrypint-initdb.d` directory of the project into `/docker-entrypoint-initdb.d` within the container.  This directory contains the script `01-create-movies-db-data.sql` which will create our schema objects and data when the container starts.
- Assigns a random external port from the container which can be used to connect to the database

This class also implements `IAsyncLifetime` and when the instance is given to the test class the `InitializeAsync()` method is called.   This will trigger the PostgreSQL Test Containers instance to start.  When the test class finishes with the fixture the `DisposeAsync()` method will be called.  This stops the PostgreSQL test container and destroys/removes it from your docker instance.

#### How Does The Web Application Wire To The Database In The Test Container ?

This same class extends `WebApplicationFactory<Program>`.  When the overriden `ConfigureWebHost` method is invoked it:

- Finds and removes the `DbContextOptions<MoviesDbContext>` which was already loaded into the service collection with `services.AddDbContext<MoviesDbContext>` in `program.cs`
- Calls `services.AddDbContext<MoviesDbContext>` to re-create the `MoviesDbContext` using the connection string obtained from the test container.

As we define the container with `.WithPortBinding(5432, assignRandomHostPort: true)` this means this PostgresSQL instance will not collide with any other instances of Postgres you may have running in Docker (especially if they use the default port `5432`)

### MoviesServiceTests

This test provides a slightly different approach to using the PostgreSQL test container.  Within this test we are only testing the `TestingContainersExample.Common.Services.MovieService` class so we don't need the whole application to be spun up - we only require a `MoviesDbContext` in order to exercise the service.

If you look at `TestingContainersExample.Tests.Integration.Fixtures.MoviesDbContextFixture` you will see that this class only provides a mechanism to obtain a `MoviesDbContext`. The test goes on to create an instance of the `MoviesService` to which it can provide the `MoviesDbContext` created by the fixture class.

## Conclusion

Using test containers provides a very nice mechanism to allow you to test your code in a way that more closely matches your production infrastructure.  In this instance we are only making use of a database test container but if you require other services like Kafka, RabbitMq, Redis etc. they are all supported.   Thanks for looking. 