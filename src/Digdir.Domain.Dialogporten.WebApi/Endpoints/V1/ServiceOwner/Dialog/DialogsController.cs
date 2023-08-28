using AutoMapper;
using Digdir.Domain.Dialogporten.Application.Features.V1.ServiceOwner.Dialogs.Commands.Update;
using Digdir.Domain.Dialogporten.Application.Features.V1.ServiceOwner.Dialogs.Queries.Get;
using Digdir.Domain.Dialogporten.WebApi.Common;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;

namespace Digdir.Domain.Dialogporten.WebApi.Endpoints.V1.ServiceOwner.Dialog;

/* Since minimal apis don't support json patch documents out of the box, 
 * and we've not been able to find a good alternative library for it yet, 
 * we've decided to use the old way of doing it using controllers and 
 * NewtonsoftJson. System.Text.Json will still be used for everything 
 * else. */

[ApiController]
[Route("api/v1/serviceowner/dialogs")]
[Tags(ServiceOwnerGroup.RoutePrefix)]
public sealed class DialogsController : ControllerBase
{
    private readonly ISender _sender;
    private readonly IMapper _mapper;

    public DialogsController(ISender sender, IMapper mapper)
    {
        _sender = sender ?? throw new ArgumentNullException(nameof(sender));
        _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
    }

    [AllowAnonymous]
    [HttpPatch("{dialogId}")]
    public async Task<IActionResult> Patch(
        [FromRoute] Guid dialogId,
        [FromHeader(Name = Constants.IfMatch)] Guid? etag,
        [FromBody] JsonPatchDocument<UpdateDialogDto> patchDocument,
        CancellationToken ct)
    {
        var dialogQueryResult = await _sender.Send(new GetDialogQuery { DialogId = dialogId }, ct);
        if (dialogQueryResult.TryPickT1(out var entityNotFound, out var dialog))
        {
            return NotFound(HttpContext.ResponseBuilder(StatusCodes.Status404NotFound, entityNotFound.ToValidationResults()));
        }

        // Remove all existing activities, since this list is append only and
        // existing activities should not be considered in the patch request.
        dialog.Activities.Clear();

        var updateDialogDto = _mapper.Map<UpdateDialogDto>(dialog);
        patchDocument.ApplyTo(updateDialogDto, ModelState);
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var command = new UpdateDialogCommand { Id = dialogId, ETag = etag, Dto = updateDialogDto };
        var result = await _sender.Send(command, ct);
        return result.Match(
            success => (IActionResult)NoContent(),
            entityNotFound => NotFound(HttpContext.ResponseBuilder(StatusCodes.Status404NotFound, entityNotFound.ToValidationResults())),
            validationFailed => BadRequest(HttpContext.ResponseBuilder(StatusCodes.Status400BadRequest, validationFailed.Errors.ToList())),
            domainError => UnprocessableEntity(HttpContext.ResponseBuilder(StatusCodes.Status422UnprocessableEntity, domainError.ToValidationResults())),
            concurrencyError => new ObjectResult(HttpContext.ResponseBuilder(StatusCodes.Status412PreconditionFailed)) { StatusCode = StatusCodes.Status412PreconditionFailed }
        );
    }
}
