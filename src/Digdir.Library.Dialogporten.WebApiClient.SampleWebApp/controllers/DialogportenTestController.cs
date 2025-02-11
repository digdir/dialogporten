using Altinn.ApiClients.Dialogporten.Services;
using Microsoft.AspNetCore.Mvc;

namespace Digdir.Library.Dialogporten.WebApiClient.SampleWebApp.controllers;

[ApiController]
[Route("[controller]")]
public class DialogportenTestController : ControllerBase
{
    private readonly IDialogTokenVerifier _dialogTokenVerifier;
    public DialogportenTestController(IDialogTokenVerifier dialogTokenVerifier)
    {
        _dialogTokenVerifier = dialogTokenVerifier;
    }
    [HttpGet]
    public string Get()
    {
        return "This is the Dialogporten Test Controller";
    }

    [HttpGet]
    [Route("verify/{token}")]
    public bool Verify(string token)
    {
        return _dialogTokenVerifier.Verify(token);
    }
}
