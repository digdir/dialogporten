#!/bin/bash

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
        echo "Unknown api environment $API_ENVIRONMENT" ;;
esac

testdatafilepath=$1
tokens=$2

serviceowner_datafile="$testdatafilepath/serviceowners-$API_ENVIRONMENT.csv"
serviceowner_tokenfile="$testdatafilepath/.serviceowners-with-tokens.csv"
enduser_datafile="$testdatafilepath/endusers-$API_ENVIRONMENT.csv"
enduser_tokenfile="$testdatafilepath/.endusers-with-tokens.csv"

if [ "$tokens" = "both" ] || [ "$tokens" = "enterprise" ]; then
    echo "org,orgno,scopes,resource,token" > $serviceowner_tokenfile
    while IFS=, read -r org orgno scopes resource
    do
        url="https://altinn-testtools-token-generator.azurewebsites.net/api/GetEnterpriseToken?org=$org&env=$env&scopes=$scopes&orgno=$orgno&ttl=3600"
        token=$(curl -s $url -u "$tokengenuser:$tokengenpasswd" )
        echo "$org,$orgno,$scopes,$resource,$token" >> $serviceowner_tokenfile
    done < <(tail -n +2 $serviceowner_datafile)
fi

if [ "$tokens" = "both" ] || [ "$tokens" = "personal" ]; then
    echo "ssn,resource,scopes,token" > $enduser_tokenfile
    while IFS=, read -r ssn resource scopes
    do
        url="https://altinn-testtools-token-generator.azurewebsites.net/api/GetPersonalToken?env=$env&scopes=$scopes&pid=$ssn&ttl=3600"
        token=$(curl -s $url -u "$tokengenuser:$tokengenpasswd" )
        echo "$ssn,$resource,$scopes,$token" >> $enduser_tokenfile
    done < <(tail -n +2 $enduser_datafile)
fi
