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
    echo "  <limit>: limit number of tokens to generate. 0 means generate all"
    echo "  <ttl>: Time to live in seconds for the generated tokens"
    echo "Example: $0 /path/to/testdata both 10 3600"
    exit 1
}

# Validate arguments
if [ $# -ne 4 ]; then
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
    "yt01")
        env="yt01" ;;
    *)
        echo "Error: Unknown api environment $API_ENVIRONMENT"
        exit 1 ;;
esac

testdatafilepath=$1
tokens=$2
limit=$3
ttl=$4

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
    generated=0
    while IFS=, read -r org orgno scopes resource
    do
        if [ $limit -gt 0 ] && [ $generated -ge $limit ]; then
            break
        fi
        url="https://altinn-testtools-token-generator.azurewebsites.net/api/GetEnterpriseToken?org=$org&env=$env&orgno=$orgno&ttl=$ttl"
        token=$(curl -s -f --get --data-urlencode "scopes=$scopes" $url -u "$tokengenuser:$tokengenpasswd" )
        if [ $? -ne 0 ]; then
            echo "Error: Failed to generate enterprise token for: $env, $org, $orgno, $scopes "
            continue
        fi
        echo "$org,$orgno,$scopes,$resource,$token" >> $serviceowner_tokenfile
        status=$?
        if [ $status -ne 0 ]; then
            echo "Error: Failed to write enterprise token to file for: $env, $org, $orgno, $scopes"
        else
            ((generated++))
        fi
    done < <(tail -n +2 $serviceowner_datafile)
fi

if [ "$tokens" = "both" ] || [ "$tokens" = "personal" ]; then
    if [ ! -f "$enduser_datafile" ]; then
        echo "Error: Input file not found: $enduser_datafile"
        exit 1
    fi
    echo "ssn,resource,scopes,token" > $enduser_tokenfile
    generated=0
    while IFS=, read -r ssn resource scopes
    do
        if [ $limit -gt 0 ] && [ $generated -ge $limit ]; then
            break
        fi
        url="https://altinn-testtools-token-generator.azurewebsites.net/api/GetPersonalToken?env=$env&scopes=$scopes&pid=$ssn&ttl=$ttl"
        token=$(curl -s -f $url -u "$tokengenuser:$tokengenpasswd" )
        if [ $? -ne 0 ]; then
            echo "Error: Failed to generate personal token for: $ssn, $scopes "
            continue
        fi
        echo "$ssn,$resource,$scopes,$token" >> $enduser_tokenfile
        status=$?
        if [ $status -ne 0 ]; then
            echo "Error: Failed to write personal token to file for: $ssn, $scopes"
        else
            ((generated++))
        fi
    done < <(tail -n +2 $enduser_datafile)
fi
