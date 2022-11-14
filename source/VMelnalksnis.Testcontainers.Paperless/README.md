# Testcontainers module for Paperless 
A pre-configured [testcontainers](https://github.com/testcontainers/testcontainers-dotnet) container for [paperless-ngx](https://github.com/paperless-ngx/paperless-ngx).

## Usage

Paperless does not have any special configuration, but requires Redis.
```csharp
var redis = await new TestcontainersBuilder<RedisTestcontainer>()
    .WithDatabase(new RedisTestcontainerConfiguration())
    .Build()
    .StartAsync();

var paperless = await new TestcontainersBuilder<PaperlessTestcontainer>()
    .WithPaperless(new(), redis)
    .Build()
    .StartAsync();
```
A helper method `GetAdminToken` is available, which created a new API token for the default admin user:
```csharp
var token = await paperless.GetAdminToken();
```
