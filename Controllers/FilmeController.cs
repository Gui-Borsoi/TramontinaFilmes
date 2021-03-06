using TramontinaFilmes.WebApi.Dominio;
using TramontinaFilmes.WebApi.Infraestrutura;
using TramontinaFilmes.WebApi.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace TramontinaFilmes.WebApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class FilmeController : ControllerBase
    {
        private readonly ILogger<FilmeController> _logger;
        private readonly FilmesRepositorio _filmesRepositorio;

        public FilmeController(ILogger<FilmeController> logger, FilmesRepositorio filmesRepositorio)
        {
            _logger = logger;
            _filmesRepositorio = filmesRepositorio;
        }

        [HttpPost]
        public async Task<IActionResult> CadastrarAsync([FromBody] FilmeInputModel filmeInputModel, CancellationToken cancellationToken)
        {
            var filme = Filme.Criar(filmeInputModel.Titulo, filmeInputModel.Duracao, filmeInputModel.Sinopse);
            if (filme.IsFailure)
            {
                _logger.LogError("Erro ao criar filme");
                return BadRequest(filme.Error);
            }

            _logger.LogInformation("Filme {filme} criado", filme.Value.Id);

            await _filmesRepositorio.InserirAsync(filme.Value, cancellationToken);
            await _filmesRepositorio.CommitAsync(cancellationToken);
            return CreatedAtAction("RecuperarPorId", new { id = filme.Value.Id }, filme.Value.Id);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Atualizar(string id, [FromBody] FilmeInputModel filmeInputModel, CancellationToken cancellationToken)
        {
            if (!Guid.TryParse(id, out var guid))
                return BadRequest("Id inválido");
            var filme = await _filmesRepositorio.RecuperarPorIdAsync(guid, cancellationToken);
            if (filme == null)
                return NotFound();

            filme.Titulo = filmeInputModel.Titulo;
            filme.Duracao = filmeInputModel.Duracao;
            filme.Sinopse = filmeInputModel.Sinopse;

            _filmesRepositorio.Alterar(filme);
            await _filmesRepositorio.CommitAsync(cancellationToken);

            return Ok(filme);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> RecuperarPorIdAsync(string id, CancellationToken cancellationToken)
        {
            if (!Guid.TryParse(id, out var guid))
                return BadRequest("Id inválido");
            var filme = await _filmesRepositorio.RecuperarPorIdAsync(guid, cancellationToken);
            if (filme == null)
                return NotFound();
            return Ok(filme);
        }
    }
}
