﻿using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Models;
using Servicos;

namespace MVC.Controllers
{
    public class CidadesController : Controller
    {
        // GET: Cidades
        public async Task<IActionResult> Index()
        {
            return View(await BuscaCidade.BuscarTodasCidades());
        }

        #region cadastrar cidade
        // GET: Cidades/Create
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Nome")] Cidade cidade)
        {
            if (ModelState.IsValid)
            {
                var result = await BuscaCidade.BuscarCidadePeloNome(cidade.Nome);

                if (result == null)
                {
                    BuscaCidade.CadastrarCidade(cidade);
                }
                else
                {
                    return Conflict("Cidade ja cadastrada");
                }

                return RedirectToAction(nameof(Index));
            }
            return View(cidade);
        }
        #endregion


        #region editar cidade
        // GET: Cidades/Edit/5
        public async Task<IActionResult> Edit(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var cidade = await BuscaCidade.BuscarCidadePeloId(id);
            if (cidade == null)
            {
                return NotFound();
            }
            return View(cidade);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(string id, [Bind("Id,Nome")] Cidade cidade)
        {
            if (id != cidade.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
               
                    var result = await BuscaCidade.BuscarCidadePeloNome(cidade.Nome);

                    if (result == null)
                    {
                        BuscaCidade.UpdateCidade(id, cidade);
                    }
                    else
                    {
                        return Conflict("Cidade ja cadastrada");
                    }

                return RedirectToAction(nameof(Index));
            }
            return View(cidade);
        }
        #endregion


        #region deletar cidade
        // GET: Cidades/Delete/5
        public async Task<IActionResult> Delete(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var cidade = await BuscaCidade.BuscarCidadePeloId(id);

            if (cidade == null)
            {
                return NotFound();
            }

            return View(cidade);
        }

        // POST: Cidades/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(string id)
        {
            var cidade = await BuscaCidade.BuscarCidadePeloId(id);
            BuscaCidade.RemoverCidade(id);
            return RedirectToAction(nameof(Index));
        }
        #endregion
    }
}
