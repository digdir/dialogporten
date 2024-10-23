#!/bin/bash

# Check if required environment variables are set
if [ -z "$TOKEN_GENERATOR_USERNAME" ] || [ -z "$TOKEN_GENERATOR_PASSWORD" ] || [ -z "$API_ENVIRONMENT" ]; then
    echo "Error: TOKEN_GENERATOR_USERNAME, TOKEN_GENERATOR_PASSWORD, and API_ENVIRONMENT must be set"
    exit 1
fi

# Function to display usage information
usage() {
    echo "Usage: $0 <testdatafilepath> <tokens>"
    echo "  <testdatafilepath>: Path to the test data files"
    echo "  <tokens>: Type of tokens to generate (both, enterprise, or personal)"
    exit 1
}

# Validate arguments
if [ $# -ne 2 ]; then
    usage
fi

tokengenuser=${TOKEN_GENERATOR_USERNAME}
tokengenpasswd=${TOKEN_GENERATOR_PASSWORD}

env=""
case $API_ENVIRONMENT in
    "test")
        env="at21" ;;
    "staging")
        env="tt02" ;;
    "performance")
        env="yt01" ;;
    *)
        echo "Error: Unknown api environment $API_ENVIRONMENT"
        exit 1 ;;
esac

testdatafilepath=$1
tokens=$2

# Validate tokens argument
if [[ ! "$tokens" =~ ^(both|enterprise|personal)$ ]]; then
    echo "Error: Invalid token type. Must be 'both', 'enterprise', or 'personal'."
    usage
fi

serviceowner_datafile="$testdatafilepath/serviceowners-$API_ENVIRONMENT.csv"
serviceowner_tokenfile="$testdatafilepath/.serviceowners-with-tokens.csv"
enduser_datafile="$testdatafilepath/endusers-$API_ENVIRONMENT.csv"
enduser_tokenfile="$testdatafilepath/.endusers-with-tokens.csv"

if [ "$tokens" = "both" ] || [ "$tokens" = "enterprise" ]; then
    if [ ! -f "$serviceowner_datafile" ]; then
        echo "Error: Input file not found: $serviceowner_datafile"
        exit 1
    fi
    echo "org,orgno,scopes,resource,token" > $serviceowner_tokenfile
    while IFS=, read -r org orgno scopes resource
    do
        url="https://altinn-testtools-token-generator.azurewebsites.net/api/GetEnterpriseToken?org=$org&env=$env&scopes=$scopes&orgno=$orgno&ttl=3600"
        token=$(curl -s -f $url -u "$tokengenuser:$tokengenpasswd" )
        if [ $? -ne 0 ]; then
            echo "Error: Failed to generate enterprise token for: $env, $org, $orgno, $scopes "
            continue
        fi
        eval echo "$org,$orgno,$scopes,$resource,$token" >> $serviceowner_tokenfile
        if [ $? -ne 0 ]; then
            echo "Error: Failed to write enterprise token to file for: $env, $org, $orgno, $scopes"
        fi
    done < <(tail -n +2 $serviceowner_datafile)
fi

if [ "$tokens" = "both" ] || [ "$tokens" = "personal" ]; then
    if [ ! -f "$enduser_datafile" ]; then
        echo "Error: Input file not found: $enduser_datafile"
        exit 1
    fi
    echo "ssn,resource,scopes,token" > $enduser_tokenfile
    while IFS=, read -r ssn resource scopes
    do
        url="https://altinn-testtools-token-generator.azurewebsites.net/api/GetPersonalToken?env=$env&scopes=$scopes&pid=$ssn&ttl=3600"
        token=$(curl -s -f $url -u "$tokengenuser:$tokengenpasswd" )
        if [ $? -ne 0 ]; then
            echo "Error: Failed to generate personal token for: $ssn, $scopes "
            continue
        fi
        echo "$ssn,$resource,$scopes,$token" >> $enduser_tokenfile
        if [ $? -ne 0 ]; then
            echo "Error: Failed to write personal token to file for: $ssn, $scopes"
        fi
    done < <(tail -n +2 $enduser_datafile)
fi
