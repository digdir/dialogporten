//using FastEndpoints;
//using MediatR;
//using Microsoft.AspNetCore.Authorization;

//namespace Digdir.Domain.Dialogporten.WebApi.Endpoints.V1.Dialogue;

//[AllowAnonymous]
//[HttpPatch("dialogue/{id}")]
//public sealed class PatchDialogueEndpoint : Endpoint<UpdateDialogueRequest>
//{
//    private readonly ISender _sender;

//    public PatchDialogueEndpoint(ISender sender)
//    {
//        _sender = sender ?? throw new ArgumentNullException(nameof(sender));
//    }

//    public override Task HandleAsync(UpdateDialogueRequest req, CancellationToken ct)
//    {
//        return Task.CompletedTask;
//    }
//}