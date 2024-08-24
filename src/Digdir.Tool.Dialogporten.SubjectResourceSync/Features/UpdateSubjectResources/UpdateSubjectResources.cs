using System;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace Digdir.Tool.Dialogporten.SubjectResourceSync.Features.UpdateSubjectResources
{
    internal sealed class UpdateSubjectResources
    {
        private readonly ILogger _logger;

        public UpdateSubjectResources(ILoggerFactory loggerFactory)
        {
            _logger = loggerFactory.CreateLogger<UpdateSubjectResources>();
        }

        [Function("UpdateSubjectResources")]
        public void Run([TimerTrigger("0 */1 * * * *")] TimerInfo timerInfo)
        {
                       
        }
    }
}
