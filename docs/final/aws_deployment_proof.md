# IronMind AWS Deployment Proof

## Deployment Summary

IronMind is deployed to AWS as a Dockerized ASP.NET Core Minimal API.

Deployment date verified:

```text
May 15, 2026
```

AWS region:

```text
ca-central-1
```

AWS account:

```text
481088927864
```

## Public Application URL

Current direct ECS task URL:

```text
http://15.223.186.37:8080
```

This endpoint is a direct public IP for the running ECS Fargate task. It can change if ECS replaces the task. For a long-lived production URL, use an Application Load Balancer or custom domain.

Health endpoint:

```text
http://15.223.186.37:8080/health
```

Database test endpoint:

```text
http://15.223.186.37:8080/db-test
```

## Verified Endpoint Results

### Health Check

Command:

```bash
curl -i http://15.223.186.37:8080/health
```

Result:

```text
HTTP/1.1 200 OK
Content-Type: application/json; charset=utf-8
Server: Kestrel

{"status":"healthy","service":"IronMind API","time":"2026-05-15T01:21:39.5784579Z"}
```

Meaning:

The ECS container is running and reachable through its public task IP.

### Database Connectivity

Command:

```bash
curl -i http://15.223.186.37:8080/db-test
```

Result:

```text
HTTP/1.1 200 OK
Content-Type: application/json; charset=utf-8
Server: Kestrel

{"database":"Connected","serverTime":"2026-05-15T01:21:40.6798661Z","message":"Database connection successful"}
```

Meaning:

The API successfully connects from ECS Fargate to the RDS PostgreSQL database.

## AWS Resources

### ECR

Repository:

```text
481088927864.dkr.ecr.ca-central-1.amazonaws.com/ironmind/api
```

Image tag:

```text
latest
```

### ECS

Cluster:

```text
ironmind-cluster
```

Service:

```text
ironmind-api-service
```

Task definition:

```text
ironmind-api:2
```

Container port:

```text
8080
```

Runtime:

```text
Linux ARM64 on AWS Fargate
```

### Public Endpoint

Current endpoint:

```text
http://15.223.186.37:8080
```

Endpoint type:

```text
Direct ECS Fargate task public IP
```

Production recommendation:

```text
Add an Application Load Balancer for a stable DNS name.
```

### RDS

Database instance:

```text
ironmind-postgres
```

Engine:

```text
PostgreSQL
```

Database name:

```text
ironmind
```

The database is not publicly accessible. It accepts PostgreSQL traffic only from the ECS API security group.

### Secrets Manager

Secrets:

```text
ironmind/prod/connection-string
ironmind/prod/jwt-secret
ironmind/prod/jwt-issuer
ironmind/prod/jwt-audience
```

The ECS task injects these secrets into the container as environment variables.

### CloudWatch Logs

Log group:

```text
/ecs/ironmind-api
```

## Deployment Flow

```text
Source code
  -> Docker image build
  -> Amazon ECR
  -> ECS Fargate task
  -> ECS public task endpoint
  -> RDS PostgreSQL
```

## Screenshot Checklist

For final submission screenshots, capture:

- GitHub repository branch `aws-ecs-deployment`
- ECR repository `ironmind/api` showing image tag `latest`
- ECS service `ironmind-api-service` with one running task
- ECS service `ironmind-api-service` with one running task
- Browser showing the current direct ECS task `/health` response
- Browser showing the current direct ECS task `/db-test` response
- RDS instance `ironmind-postgres` with available status
- Browser showing `/health` response
- Browser showing `/db-test` response
