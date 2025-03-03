﻿using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Models;
using OfficeOpenXml;
using Servicos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;

namespace MVC.Controllers
{
    public class DocumentoController : Controller
    {
        private static IWebHostEnvironment _hostEnvironment;
        public static List<List<string>> rotas = new();
        public static List<string> cabecalho = new();
        public static List<string> servicos = new();
        public static string nomeDoServico;
        public static string cidade;
        public static string downloadDocumento;

        public DocumentoController(IWebHostEnvironment hostEnvironment)
        {
            _hostEnvironment = hostEnvironment;
        }

        public IActionResult Upload()
        {
            return View();
        }
        public IActionResult UploadDocumento()
        {
            var documento = HttpContext.Request.Form.Files; //faz o upload do arquivo que foi selecionado

            if (documento.Count > 0)
            {
                List<List<string>> rotaServico = new();
                List<string> cabecalhoDoArquivo = new();

                ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
                using ExcelPackage lerDocumento = new(documento[0].OpenReadStream()); //faz a leitura do documento que fez upload
                ExcelWorksheet planilha = lerDocumento.Workbook.Worksheets.FirstOrDefault(); //pega a primeira planilha do documento.

                var linhaCount = planilha.Dimension.End.Row; //conto quantas linhas tem no arquivo
                var colunaCount = planilha.Dimension.End.Column; //conto quantas colunas tem no arquivo

                int colunaDoCep = 0;
                int colunaDoServico = 0;

                for (var coluna = 1; coluna <= colunaCount; coluna++)
                {
                    var aux = planilha.Cells[1, coluna].Value.ToString(); //travo a linha como 1 e faço um for pra pegar os nomes que estão em todas as colunas dessa linha. 



                    cabecalhoDoArquivo.Add(aux); //Monto o cabeçalho colocando as informaoes nessa lita.

                    if (aux.ToUpper() == "CEP")
                        colunaDoCep = coluna - 1; //descubro qual a coluna do CEP pra poder ordenar depois.

                    if (aux.ToUpper() == "SERVIÇO") //descubro qual a coluna do Serviço pra poder selecionar depois.
                        colunaDoServico = coluna;
                }

                cabecalho = cabecalhoDoArquivo;


                planilha.Cells[2, 1, linhaCount, colunaCount] //seleciona o intervalo de celulas que preciso
                        .Sort(colunaDoCep, false); //informo a coluna que quero ordenar (coluna do cep que ja sei qual é). Coloco false pra não fazer ordenação decrescente.


                List<string> servico = new();

                for (var linha = 1; linha < linhaCount; linha++)
                {
                    List<string> conteudoDaLinha = new();
                    conteudoDaLinha.Add(" ");

                    for (var column = 1; column <= colunaCount; column++)
                    {
                        servico.Add(planilha.Cells[linha, colunaDoServico].Value.ToString().ToUpper());

                        var conteudo = planilha.Cells[linha, column].Value?.ToString() ?? "";
                        conteudoDaLinha.Add(conteudo.ToString());
                    }

                    rotaServico.Add(conteudoDaLinha);
                }

                servicos = servico.Distinct().ToList();
                servicos.RemoveAt(0);

                rotas = rotaServico;

                return RedirectToAction(nameof(Filtros));
            }

            return RedirectToAction(nameof(Upload));
        }

        public async Task<IActionResult> Filtros()
        {
            IEnumerable<Cidade> cidades = await BuscaCidade.BuscarTodasCidades();

            ViewBag.Cidades = cidades;
            ViewBag.Servicos = servicos;

            return View();
        }

        [HttpPost]
        public IActionResult BuscarEquipePelaCidadeAtual()
        {
            nomeDoServico = Request.Form["nomeDoServico"].ToString();
            cidade = Request.Form["Filtros"].ToString();

            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Index()
        {

            IEnumerable<Equipe> equipe = await BuscaEquipe.BuscarEquipePelaCidadeId(cidade);

            ViewBag.Cabecalho = cabecalho;
            ViewBag.Equipes = equipe;
            return View();


        }

        public async Task<IActionResult> Create()
        {
            var equipeSelecionada = Request.Form["equipe"].ToList();
            var cabecalhoSelecionado = Request.Form["cabecalho"].ToList();

            if (equipeSelecionada.Count == 0 || cabecalhoSelecionado.Count == 0)
                return RedirectToAction(nameof(Index));

            List<Equipe> equipesSelecionadas = new();

            foreach (var equipeId in equipeSelecionada)
            {
                var equipe = await BuscaEquipe.BuscarEquipePeloId(equipeId);
                equipesSelecionadas.Add(equipe);
            }

            var cidadeSelecionada = await BuscaCidade.BuscarCidadePeloId(cidade);   

            await ExportarDocumento.Write(rotas, cabecalhoSelecionado, equipesSelecionadas, nomeDoServico, cidadeSelecionada, _hostEnvironment.WebRootPath);
            var nomeDoArquivo = $"Rotas {cidadeSelecionada.Nome}.docx";
            downloadDocumento = $"{_hostEnvironment.ContentRootPath}//{nomeDoArquivo}";

            return View();
        }

        public FileResult Download()
        {
            var nomeDoArquivo = downloadDocumento.Split("//").ToList();
            var arquivoGerado = System.IO.File.ReadAllBytes(downloadDocumento);
            return File(arquivoGerado, "application/octet-stream", nomeDoArquivo.Last().ToString());
        }
    }
}