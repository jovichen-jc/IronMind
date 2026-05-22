# IronMind — Port Reference

| Port | Protocol | Environment | Used By | Description |
| --- | --- | --- | --- | --- |
| 5235 | HTTP | Local development | ASP.NET Core (Kestrel) | Default HTTP port when running `dotnet run` locally. Open `http://localhost:5235/swagger` to access the API. Defined in `IronMind.API/Properties/launchSettings.json`. |
| 7222 | HTTPS | Local development | ASP.NET Core (Kestrel) | Default HTTPS port when running with the `https` launch profile. Access via `https://localhost:7222`. Defined in `IronMind.API/Properties/launchSettings.json`. |
| 8080 | HTTP | Docker / AWS ECS Fargate | Container runtime | Port the API listens on inside the Docker container. Set via `ENV ASPNETCORE_URLS=http://+:8080` in the Dockerfile and mirrored in `aws/ecs-task-definition.json`. Not used during local development. |
| 5432 | TCP | Local development / AWS RDS | PostgreSQL | Standard PostgreSQL port. Used by EF Core via the Npgsql driver when connecting to a local database or the RDS instance. Specified in the connection string under `ConnectionStrings:Default`. |
