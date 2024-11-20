# Enduser get dialogs

## Introduction
This directory holds a performance test for all GET endpoints for `api/v1/enduser/dialogs` 

## Prerequisites
- [K6 prerequisites](../../README.md#Prerequisites)

## Test file
- enduser-search.js

## Test description
The test has a list of enduser (ssn) with pregenerated tokens, and the following endpoints are visited in
sequence for each enduser:
- api/v1/enduser/dialogs?Party=urn:altinn:person:identifier-no:`<ssn>`
- api/v1/enduser/dialogs/`<dialogId>`
- api/v1/enduser/dialogs/`<dialogId>`/activities
- api/v1/enduser/dialogs/`<dialogId>`/activities/`<activityId>`
- api/v1/enduser/dialogs/`<dialogId>`/seenlog
- api/v1/enduser/dialogs/`<dialogId>`/seenlog/`<seenlogId>`
- api/v1/enduser/dialogs/`<dialogId>`/transmissions
- api/v1/enduser/dialogs/`<dialogId>`/transmissions/`<transmissionId>`
- api/v1/enduser/dialogs/`<dialogId>`/labellog

## Run test
### From cli
Go to this directory:  
```shell
cd tests/k6/tests/enduser/performance
```  
Generate tokens (get your `tokegeneratoruser/password` in advance):  
```shell
TOKEN_GENERATOR_USERNAME=<username> \
TOKEN_GENERATOR_PASSWORD=****** API_ENVIRONMENT=yt01 \
../../scripts/generate_tokens.sh ../../performancetest_data both
```
Run a test:
```shell
k6 run enduser-search.js -e API_VERSION=v1 \
-e API_ENVIRONMENT=<test|staging|yt01> \
--vus=<vus> --duration=<duration>
```
See the k6 documentation for more usage information
### From Github Actions
Go to your [github actions](https://github.com/digdir/dialogporten/actions/workflows/dispatch-k6-performance.yml), select `Run workflow`and fill in the required parameters. Tag the performance test with a descriptive name

### Github Action with act
Run the github action locally.  
[Install act](https://nektosact.com/installation/),
go to the root of the repo, and create a `.secrets`file matching the github secrets used, like:  
```file
TOKEN_GENERATOR_USERNAME:**
TOKEN_GENERATOR_PASSWORD:**
K6_CLOUD_PROJECT_ID=**
K6_CLOUD_TOKEN=**
K6_PROMETHEUS_RW_USERNAME=**
K6_PROMETHEUS_RW_PASSWORD=**
K6_PROMETHEUS_RW_SERVER_URL=**

```
Run act:
```shell
act workflow_dispatch -j k6-performance -s GITHUB_TOKEN=`gh auth token` \
--container-architecture linux/amd64 --artifact-server-path $HOME/.act \ 
--input vus=<vus> --input duration=<duration> \ 
--input testSuitePath=tests/k6/tests/enduser/performance/enduser-search.js \ 
--input tokens=personal
```

## Test Results
Test results can be found in github action run log and in appinsights. We are prepared for exporting results to grafana, but so far results are exported to a private grafana instance only, as can be seen from the `.secrets`listed earlier 

## TODO
Fix reporting

