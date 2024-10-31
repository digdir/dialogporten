# Database Connection Forwarding

This utility helps forward PostgreSQL and Redis connections through SSH for Dialogporten environments.

## Prerequisites

- Azure CLI installed and configured
- Appropriate Azure account access
- Node.js >= 20.0.0

## Installation

1. Install dependencies:
```bash
npm install
```

2. Make sure you're logged into the correct Azure account:
- For test/yt01: Use the test account
- For staging/prod: Use the production account

## Usage

### Using the Node.js Tool (Recommended)

Run the interactive tool:
```bash
npm start
```

Or with command-line arguments:
```bash
npm start -- -e test -t postgres
npm start -- -e prod -t redis
```
## Connecting to Databases

### PostgreSQL

1. Start the forwarding tool:
```bash
npm start
```
2. Select your environment and choose 'postgres'
3. Once the tunnel is established, you can connect using:
   - Host: localhost
   - Port: 5432
   - Database: dialogporten
   - Username: will be shown in the connection string
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
npm start
```
2. Select your environment and choose 'redis'
3. Once the tunnel is established, you can connect using:
   - Host: localhost
   - Port: 6379
   - Password: will be shown in the connection string

Example using redis-cli:
```bash
# Set password in environment variable  
export REDIS_PASSWORD="<password>"  
redis-cli -h localhost -p 6379 -a "$REDIS_PASSWORD"  
```

Example connection string for applications:
```plaintext
redis://:<password>@localhost:6379
```

## Troubleshooting

- If you get authentication errors, ensure you're logged into the correct Azure account
- For test/yt01 environments, use the test subscription
- For staging/prod environments, use the production subscription
- If the tunnel fails to establish, try running `az login` again
- Make sure you have the necessary permissions in the Azure subscription