# Database Connection Forwarding

This utility helps forward PostgreSQL and Redis connections through SSH for Dialogporten environments.

## Prerequisites

- Azure CLI installed and configured
- Appropriate Azure account access
- Bash shell environment

## Usage

### Interactive Mode

Run the script without arguments for interactive mode:
```bash
./forward.sh
```

### Command-line Arguments

You can also specify the environment and database type directly:
```bash
./forward.sh -e test -t postgres
./forward.sh -e prod -t redis
```

Available options:
- `-e`: Environment (test, yt01, staging, prod)
- `-t`: Database type (postgres, redis)
- `-h`: Show help message

## Connecting to Databases

### PostgreSQL

1. Start the forwarding tool:
```bash
./forward.sh -e test -t postgres
```
2. Once the tunnel is established, you can connect using:
   - Host: localhost
   - Port: 5432
   - Database: dialogporten
   - Username: shown in the connection string
   - Password: retrieve from Azure Key Vault

Example using psql:
```bash
psql "host=localhost port=5432 dbname=dialogporten user=<username>"
```

Example using pgAdmin:
- Host: localhost
- Port: 5432
- Database: dialogporten
- Username: (from connection string)
- Password: (from Key Vault)

### Redis

1. Start the forwarding tool:
```bash
./forward.sh -e test -t redis
```
2. Once the tunnel is established, you can connect using:
   - Host: localhost
   - Port: 6379
   - Password: shown in the connection string

Example using redis-cli:
```bash
redis-cli -h localhost -p 6379 -a "<password>"
```

Example connection string for applications:
```plaintext
redis://:<password>@localhost:6379
```

## Troubleshooting

- If you get authentication errors, ensure you're logged into the correct Azure account:
  - For test/yt01 environments, use the test subscription
  - For staging/prod environments, use the production subscription
- If the tunnel fails to establish, try running `az login` again
- Make sure you have the necessary permissions in the Azure subscription
- If the script fails to execute, ensure it has execute permissions:
  ```bash
  chmod +x forward.sh
  ```