﻿// Copyright 2022 Valters Melnalksnis
// Licensed under the Apache License 2.0.
// See LICENSE file in the project root for full license information.

namespace VMelnalksnis.Testcontainers.Keycloak.Tests;

[CollectionDefinition("Keycloak")]
public sealed class KeycloakCollection : ICollectionFixture<KeycloakFixture>;
