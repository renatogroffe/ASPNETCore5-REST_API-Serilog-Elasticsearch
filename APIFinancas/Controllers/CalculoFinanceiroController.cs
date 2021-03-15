using System.Net;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using APIFinancas.Models;

namespace APIFinancas.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class CalculoFinanceiroController : ControllerBase
    {
        private readonly ILogger<CalculoFinanceiroController> _logger;

        public CalculoFinanceiroController(ILogger<CalculoFinanceiroController> logger)
        {
            _logger = logger;
        }

        [HttpGet("juroscompostos")]
        [ProducesResponseType(typeof(Emprestimo), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(FalhaCalculo), (int)HttpStatusCode.BadRequest)]
        public ActionResult<Emprestimo> Get(
            double valorEmprestimo, int numMeses, double percTaxa)
        {
            _logger.LogInformation(
                "Recebida nova requisição|" +
               $"Valor do empréstimo: {valorEmprestimo}|" +
               $"Número de meses: {numMeses}|" +
               $"% Taxa de Juros: {percTaxa}");

            if (valorEmprestimo <= 0)
                return GerarResultParamInvalido("Valor do Empréstimo");

            if (numMeses <= 0)
                return GerarResultParamInvalido("Número de Meses");

            if (percTaxa <= 0)
                return GerarResultParamInvalido("Percentual da Taxa de Juros");

            double valorFinalJuros =
                CalculoFinanceiro.CalcularValorComJurosCompostos(
                    valorEmprestimo, numMeses, percTaxa);
            _logger.LogInformation($"Valor Final com Juros: {valorFinalJuros}");
            LogStatusHttpRequest(HttpStatusCode.OK);

            return new Emprestimo()
            {
                ValorEmprestimo = valorEmprestimo,
                NumMeses = numMeses,
                TaxaPercentual = percTaxa,
                ValorFinalComJuros = valorFinalJuros
            };
        }

        private BadRequestObjectResult GerarResultParamInvalido(
            string nomeCampo)
        {
            var erro = $"O {nomeCampo} deve ser maior do que zero!";
            _logger.LogError(erro);

            LogStatusHttpRequest(HttpStatusCode.BadRequest);

            return new BadRequestObjectResult(
                new FalhaCalculo() { Mensagem = erro });
        }

        private void LogStatusHttpRequest(HttpStatusCode status)
        {
            var codStatus = (int)status;
            var mensagem = $"/calculofinanceiro/juroscompostos {codStatus} {status}";

            if (codStatus >= 400)
                _logger.LogError(mensagem);
            else
                _logger.LogInformation(mensagem);
        }
    }
}