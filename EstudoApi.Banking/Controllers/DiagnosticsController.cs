using Microsoft.AspNetCore.Mvc;
using MediatR;
using EstudoApi.Banking.Transfer.Commands;

namespace EstudoApi.Banking.Controllers
{
    [ApiController]
    [Route("api/banking/diag")]
    public class DiagnosticsController : ControllerBase
    {
        private readonly IServiceProvider _provider;

        public DiagnosticsController(IServiceProvider provider)
        {
            _provider = provider;
        }

        [HttpGet("transfer-handler")]        
        public IActionResult CheckTransferHandler()
        {
            var handler = _provider.GetService(typeof(IRequestHandler<TransferCommand, TransferResult>));
            return Ok(new {
                handlerResolved = handler != null,
                handlerType = handler?.GetType().FullName
            });
        }
    }
}
