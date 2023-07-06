using AutoMapper;
using Digdir.Domain.Dialogporten.Application.Features.V1.Dialogs.Commands.Update;
using Digdir.Domain.Dialogporten.Application.Features.V1.Dialogs.Queries.Get;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;

namespace Digdir.Domain.Dialogporten.WebApi.Endpoints.V1.Dialog;

/* Since minimal apis don't support json patch documents out of the box, 
 * and we've not been able to find a good alternative library for it yet, 
 * we've decided to use the old way of doing it using controllers and 
 * NewtonsoftJson. System.Text.Json will still be used for everything 
 * else. */

[ApiController]
[Route("api/v1/dialogs")]
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
    [HttpPatch("{id}")]
    public async Task<IActionResult> Patch(Guid id, [FromBody] JsonPatchDocument<UpdateDialogDto> patchDocument, CancellationToken ct)
    {
        var dialogQueryResult = await _sender.Send(new GetDialogQuery { Id = id }, ct);
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

        var command = new UpdateDialogCommand { Id = id, Dto = updateDialogDto };
        var result = await _sender.Send(command, ct);
        return result.Match(
            success => (IActionResult)NoContent(),
            entityNotFound => NotFound(HttpContext.ResponseBuilder(StatusCodes.Status404NotFound, entityNotFound.ToValidationResults())),
            entityExists => Conflict(HttpContext.ResponseBuilder(StatusCodes.Status409Conflict, entityExists.ToValidationResults())),
            validationFailed => BadRequest(HttpContext.ResponseBuilder(StatusCodes.Status400BadRequest, validationFailed.Errors.ToList())));
    }
}
