[![Nuget](https://img.shields.io/nuget/v/VMelnalksnis.Testcontainers.Keycloak?label=VMelnalksnis.Testcontainers.Keycloak)](https://www.nuget.org/packages/VMelnalksnis.Testcontainers.Keycloak/)

# Testcontainers module for Keycloak 
A pre-configured [testcontainers](https://github.com/testcontainers/testcontainers-dotnet) container for [Keycloak](https://github.com/keycloak/keycloak).

## Usage

In addition to the default container configuration, it is also possible to specify realms, clients and users that should be created before the container is considered started:
```csharp
var client = new Client("demoapp", new("http://localhost:8000/*")) { Secret = "client_secret" };
var user = new User("john.doe", "password123");
var realmConfiguration = new RealmConfiguration("demorealm", new List<Client> { client }, new List<User> { user });
var keycloakConfiguration = new KeycloakTestcontainerConfiguration { Realms = new[] { realmConfiguration } };

await using var keycloak = new TestcontainersBuilder<KeycloakTestcontainer>()
    .WithKeycloak(keycloakConfiguration)
    .Build();

await keycloak.StartAsync();
```
After starting the container, additional realm information is available:
```csharp
var realm = keycloak.Realms.Single();
var realmBaseAddress = realm.ServerRealm;
var oidcMetadat = realm.Metadata;
```
