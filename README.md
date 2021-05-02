# auth experiment

Some experiment to see if I can get a working .NET service with keycloak auth and basic monitoring up and running.

## infra

- start everything up with `docker compose up`
- the services will be reachable via:

| service    | port        |
| ---------- | ----------- |
| service    | 5000 & 5001 |
| prometheus | 9090        |
| grafana    | 3030        |
| keycloak   | 8080        |
| postgres   | 5432        |

## issues

### Auth in docker
Auth works, if you start the .NET service locally via IDE or from the command line. It will not work when the service is started as docker container. Currently the oidc library for .NET accepts only one address for the auth provider. If you run keycloak in docker and the .NET service outside, everything is fine: authservice and your browser both use `localhost:8080` to contact keycloak. If the .NET service also runs inside docker, it needs to contact keycloak via its container name `keycloak` while the browser still needs `localhost`. The reference to a `MetadataAddress` from which the .NET service can pull its configuration will not make a difference.

### cookies
Currently the browser will set a cookie instead of using refresh and access tokens, which I would have preferred here. 
