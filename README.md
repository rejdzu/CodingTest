# CodingTest

## Prerequisites

- .NET7 SDK
- Powershell or any console
- dev certs for dotnet in order to host project over https

#### Build the project

```pwsh
dotnet build .\src\
```

#### Run the project

```pwsh
dotnet run --project .\src\Api\Api.csproj
```

#### Run tests

```pwsh
dotnet test .\src\
```

## Description

Project is written in .NET 7. I've used default template for API project in dotnet. Application is using async/await pattern.

I've created one controller that exposes endpoint **api/beststories** which takes query parameter **n**. After running the project it can be accessed through:
- Swagger UI: [https://localhost:7035/swagger/index.html](https://localhost:7035/swagger/index.html).
- Calling the endpoint directly for example with curl: 
```pwsh
curl https://localhost:7035/api/beststories?n=20
```

Since description of the task is not clearly stating, if sorting should be done on all top stories or only the "n" subset, I've assumed that it's the later. I'm fetching n items and then sorting those entries.

## Tests

Application has almost 80% coverage. Tests include unit test for written services and controller as well as integration tests for the Hacker News API client.

## Optimizations

Main optimization for keeping number of calls to the Hacker News API is done in class **CachedHackerNewsService**. Those optimizations include:

- **Singleton**: Only one instance of CachedHackerNewsServiceDecorator is registered in DI.
- **Request coalescing**: If more than one request for the same id is triggered at the same time, only one call is made to the external API. All other calls await on the response from the initial call.
- **Caching**: Responses are cached for 30 seconds and subsequent calls are using it in the first place.

Another optimization is done inside **BestStoriesService** class, where it runs those "n" fetches in parallel. Right now that value is hardcoded to 10 calls.

## Possible improvements

- Add rest of tests to increase coverage, ie cancellation token support, etc.
- Add proper configuration for urls, hardcoded expiry timespans, how many fetches run in parallel etc.
- Add proper validation using FluentValidation or other frameworks
- Add integration tests for the API using TestServer
- Add acceptance tests
- Add docker build
- Add metrics
- Add authentication/authorization
