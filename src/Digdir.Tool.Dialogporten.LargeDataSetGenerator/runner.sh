#!/bin/bash

# Initial dates
START_DATE="1980-01-02"
END_DATE="1980-03-28"
export DIALOG_AMOUNT="10000"
export CONN_STRING="Server=localhost;Port=5432;Database=dialogporten;User ID=postgres;Password=supersecret;Include Error Detail=True;"

for (( i=1; i<=100; i++ ))
do
  # Set environment variables
  export FROM_DATE="$START_DATE"
  export TO_DATE="$END_DATE"

  echo "Loop $i: FROM_DATE=$FROM_DATE, TO_DATE=$TO_DATE"

  # Run the dotnet command (commented out for testing the date ranges)
  dotnet run -c Release

  # Update dates for the next loop
  # Calculate the new START_DATE: 2nd of the next month after the current TO_DATE
  START_DATE=$(date -j -f "%Y-%m-%d" "$END_DATE" "+%Y-%m-02" | xargs -I{} date -j -v+1m -f "%Y-%m-%d" {} "+%Y-%m-%d")
  # Calculate the new END_DATE: 3 months later on the 28th
  END_DATE=$(date -j -f "%Y-%m-%d" "$START_DATE" "+%Y-%m-%d" | xargs -I{} date -j -v+2m -v+26d -f "%Y-%m-%d" {} "+%Y-%m-%d")
done

