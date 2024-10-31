#!/usr/bin/env node

import { program } from 'commander';
import inquirer from 'inquirer';
import { execSync, spawn } from 'child_process';
import chalk from 'chalk';
import boxen from 'boxen';

// Constants
const ENVIRONMENTS = {
  test: 'test',
  yt01: 'yt01',
  staging: 'staging',
  prod: 'prod'
} as const;

const DB_TYPES = {
  postgres: 'postgres',
  redis: 'redis'
} as const;

const SUBSCRIPTION_MAP = {
  [ENVIRONMENTS.test]: 'Dialogporten-Test',
  [ENVIRONMENTS.yt01]: 'Dialogporten-Test',
  [ENVIRONMENTS.staging]: 'Dialogporten-Staging',
  [ENVIRONMENTS.prod]: 'Dialogporten-Prod'
} as const;

// Types
type Environment = keyof typeof ENVIRONMENTS;
type DbType = keyof typeof DB_TYPES;
type ResourceInfo = {
  name: string;
  hostname: string;
  port: number;
  connectionString: string;
};

// Utility functions
const log = {
  info: (message: string) => console.log(chalk.blue('ℹ'), message),
  success: (message: string) => console.log(chalk.green('✓'), message),
  warning: (message: string) => console.log(chalk.yellow('⚠'), message),
  error: (message: string) => console.log(chalk.red('✖'), message),
  title: (message: string) => console.log('\n', chalk.bold.cyan(message)),
  connectionInfo: (info: { title: string, value: string }) => {
    console.log(
      chalk.dim(info.title.padEnd(20, ' ')),
      chalk.bold(info.value)
    );
  }
};

const executeCommand = (command: string, timeoutMs: number = 30000): string => {
  try {
    return execSync(command, { encoding: 'utf-8', timeout: timeoutMs }).trim();
  } catch (error) {
    if (error.code === 'ETIMEDOUT') {
      throw new Error(`${chalk.red('Command timed out after')} ${timeoutMs}ms: ${command}`);
    }
    throw new Error(`${chalk.red('Command execution failed:')} ${command}\n${(error as Error).message}`);
  }
};

// Azure CLI checks
const checkAzureCliInstallation = (): void => {
  try {
    execSync('az --version', { stdio: 'ignore' });
  } catch (error) {
    throw new Error(
      'Azure CLI is not installed. Please visit: https://docs.microsoft.com/en-us/cli/azure/install-azure-cli'
    );
  }
};

const getSubscriptionId = (env: Environment): string => {
  const subscriptionName = SUBSCRIPTION_MAP[env];
  
  try {
    const subId = executeCommand(
      `az account show --subscription "${subscriptionName}" --query id -o tsv`
    );

    if (!subId) {
      throw new Error('Empty subscription ID');
    }

    return subId;
  } catch (error) {
    throw new Error(
      `Could not find subscription '${subscriptionName}'. Please ensure you are logged in to the correct Azure account.`
    );
  }
};

// Resource information fetchers
const getPostgresInfo = (env: Environment, subscriptionId: string): ResourceInfo => {
  const name = executeCommand(
    `az postgres flexible-server list --subscription "${subscriptionId}" --query "[?tags.Environment=='${env}' && tags.Product=='Dialogporten'] | [0].name" -o tsv`
  );

  if (!name) {
    throw new Error('Postgres server not found');
  }

  const hostname = `${name}.postgres.database.azure.com`;
  const port = 5432;
  
  const credentials = JSON.parse(executeCommand(
    `az postgres flexible-server show --resource-group dp-be-${env}-rg --name ${name} --query "{username: administratorLogin}" -o json`
  ));

  return {
    name,
    hostname,
    port,
    connectionString: `postgresql://${credentials.username}:<retrieve-password-from-keyvault>@${hostname}:${port}/dialogporten`
  };
};

const getRedisInfo = (env: Environment, subscriptionId: string): ResourceInfo => {
  const name = executeCommand(
    `az redis list --subscription "${subscriptionId}" --query "[?tags.Environment=='${env}' && tags.Product=='Dialogporten'] | [0].name" -o tsv`
  );

  if (!name) {
    throw new Error('Redis server not found');
  }

  const hostname = `${name}.redis.cache.windows.net`;
  const port = 6379;

  const credentials = JSON.parse(executeCommand(
    `az redis list-keys --resource-group dp-be-${env}-rg --name ${name} --query "{password: primaryKey}" -o json`
  ));

  return {
    name,
    hostname,
    port,
    connectionString: `redis://:${credentials.password}@${hostname}:${port}`
  };
};

// SSH tunnel setup
const setupSshTunnel = (env: Environment, resourceInfo: ResourceInfo): void => {
  let tunnelEstablished = false;
  const sshProcess = spawn('az', [
    'ssh',
    'vm',
    '-g', `dp-be-${env}-rg`,
    '-n', `dp-be-${env}-ssh-jumper`,
    '--',
    '-L', `${resourceInfo.port}:${resourceInfo.hostname}:${resourceInfo.port}`,
    '-o', 'ExitOnForwardFailure=yes'
  ], { stdio: 'inherit' });

  // Cleanup on script exit
  const cleanup = () => {
    if (sshProcess.pid) {
      process.kill(sshProcess.pid);
    }
  };
  process.on('SIGINT', cleanup);
  process.on('SIGTERM', cleanup);

  sshProcess.on('error', (error) => {
    throw new Error(`Failed to start SSH tunnel: ${error.message}`);
  });

  sshProcess.on('exit', (code) => {
    if (code !== 0 && !tunnelEstablished) {
      throw new Error('SSH tunnel failed to establish');
    }
  });

  // Verify tunnel after a short delay
  setTimeout(() => {
    if (sshProcess.pid) {
      tunnelEstablished = true;
      log.success('SSH tunnel established successfully');
    }
  }, 2000);
};

// Main function
const forwardConnection = async (options: { environment?: Environment; type?: DbType }): Promise<void> => {
  try {
    log.title('Database Connection Forwarder');
    
    checkAzureCliInstallation();
    log.success('Azure CLI is installed');

    const answers = await inquirer.prompt([
      {
        type: 'list',
        name: 'environment',
        message: 'Select environment:',
        choices: Object.values(ENVIRONMENTS).map(env => ({
          name: chalk.cyan(env.toUpperCase()),
          value: env
        })),
        when: !options.environment
      },
      {
        type: 'list',
        name: 'type',
        message: 'Select database type:',
        choices: Object.values(DB_TYPES).map(type => ({
          name: chalk.yellow(type.toUpperCase()),
          value: type
        })),
        when: !options.type
      }
    ]);

    const env = options.environment || answers.environment;
    if (!Object.values(ENVIRONMENTS).includes(env)) {
      throw new Error(`Invalid environment: ${env}`);
    }

    const dbType = options.type || answers.type;
    if (!Object.values(DB_TYPES).includes(dbType)) {
      throw new Error(`Invalid database type: ${dbType}`);
    }

    const confirmation = await inquirer.prompt([
      {
        type: 'confirm',
        name: 'proceed',
        message: boxen(
          [
            chalk.bold.cyan('Please confirm your selection:'),
            '',
            `${chalk.dim('Environment:')}  ${chalk.bold.cyan(env.toUpperCase())}`,
            `${chalk.dim('Database:')}    ${chalk.bold.yellow(dbType.toUpperCase())}`,
          ].join('\n'),
          {
            padding: 1,
            margin: 1,
            borderStyle: 'round',
            borderColor: 'yellow'
          }
        ),
        default: true
      }
    ]);

    if (!confirmation.proceed) {
      log.warning('Operation cancelled by user');
      process.exit(0);
    }

    log.info(`Setting up connection for ${chalk.bold(env)} environment`);

    const subscriptionId = getSubscriptionId(env);
    execSync(`az account set --subscription "${subscriptionId}"`, { stdio: 'inherit' });
    log.success('Azure subscription set');

    let resourceInfo: ResourceInfo;
    switch (dbType) {
      case DB_TYPES.postgres:
        resourceInfo = getPostgresInfo(env, subscriptionId);
        break;
      case DB_TYPES.redis:
        resourceInfo = getRedisInfo(env, subscriptionId);
        break;
      default:
        throw new Error(`Unsupported resource type: ${dbType}`);
    }

    log.title('Connection Details');
    console.log(boxen(
      [
        chalk.bold.cyan(`${dbType.toUpperCase()} Connection Info`),
        '',
        `${chalk.dim('Server:')}    ${resourceInfo.hostname}`,
        `${chalk.dim('Port:')}      ${resourceInfo.port}`,
        '',
        chalk.dim('Connection String:'),
        chalk.bold(resourceInfo.connectionString)
      ].join('\n'),
      {
        padding: 1,
        margin: 1,
        borderStyle: 'round',
        borderColor: 'cyan'
      }
    ));

    log.info('Starting SSH tunnel...');
    setupSshTunnel(env, resourceInfo);
  } catch (error) {
    if (error instanceof Error) {
      log.error(error.message);
    } else {
      log.error('An unknown error occurred');
    }
    process.exit(1);
  }
};

// CLI setup
program
  .name('forward')
  .description('Forward PostgreSQL or Redis connections through SSH')
  .option('-e, --environment <env>', 'Environment (test, yt01, staging, prod)')
  .option('-t, --type <type>', 'Database type (postgres, redis)')
  .action(forwardConnection);

program.parse();