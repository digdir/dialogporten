targetScope = 'subscription'

param input string 

output myValue string = 'I was given ${input}.'
output myValue2 string = 'I was given2 ${input}.'