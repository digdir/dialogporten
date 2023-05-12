using Azure.Storage.Queues;

var cts = new CancellationTokenSource();
var storageConnectionString = "DefaultEndpointsProtocol=http;AccountName=devstoreaccount1;AccountKey=Eby8vdM02xNOcqFlqUwJPLlmEtlCDXJ1OUzFT50uSRZ6IFsuFq2UVErCz4I6tq/K1SZFPTOtr/KBHBeksoGMGw==;BlobEndpoint=http://127.0.0.1:10000/devstoreaccount1;QueueEndpoint=http://127.0.0.1:10001/devstoreaccount1;TableEndpoint=http://127.0.0.1:10002/devstoreaccount1;";
var client = new QueueClient(storageConnectionString, "outbox-queue");

while (!cts.IsCancellationRequested)
{ 
    var message = await client.ReceiveMessageAsync(cancellationToken: cts.Token);
    Console.WriteLine(message.Value.Body.ToString());
    await client.DeleteMessageAsync(message.Value.MessageId, message.Value.PopReceipt, cts.Token);
}