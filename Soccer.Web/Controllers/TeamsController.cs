﻿using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Soccer.Web.Data;
using Soccer.Web.Data.Entities;
using Soccer.Web.Helpers;
using Soccer.Web.Models;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Soccer.Web.Controllers
{
    public class TeamsController : Controller
    {
        private readonly DataContext _context;
        private readonly IImageHelper _imageHelper;
        private readonly IConverterHelper _converterHelper;

        public IImageHelper ImageHelper { get; }
        public IConverterHelper ConverterHelper { get; }

        public TeamsController(
            DataContext context,
            IImageHelper imageHelper,
            IConverterHelper converterHelper)

        {
            _context = context;
            _imageHelper = imageHelper;
            _converterHelper = converterHelper;
        }

        public async Task<IActionResult> Index()
        {
            return View(await _context.Teams.ToListAsync());
        }

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            TeamEntity teamEntity = await _context.Teams
                .FirstOrDefaultAsync(m => m.Id == id);
            if (teamEntity == null)
            {
                return NotFound();
            }

            return View(teamEntity);
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(TeamViewModel model)
        {
            if (ModelState.IsValid)
            {
                string path = string.Empty;

                if (model.LogoFile != null)
                {
                    path = await _imageHelper.UploadImageAsync(model.LogoFile, "Teams");
                }

                var team = _converterHelper.ToTeamEntity(model, path, true);
                _context.Add(team);

                try
                {
                    await _context.SaveChangesAsync();
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    if (ex.InnerException.Message.Contains("duplicate"))
                    {
                        ModelState.AddModelError(string.Empty, "Already there is a record with the same name.");
                    }
                    else
                    {
                        ModelState.AddModelError(string.Empty, ex.InnerException.Message);
                    }
                }
            }

            return View(model);
        }


        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            TeamEntity teamEntity = await _context.Teams.FindAsync(id);
            if (teamEntity == null)
            {
                return NotFound();
            }

            var teamViewModel = _converterHelper.ToTeamViewModel(teamEntity);
            return View(teamViewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, TeamViewModel teamViewModel)
        {
            if (id != teamViewModel.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                string path = teamViewModel.LogoPath;

                if (teamViewModel.LogoFile != null)
                {
                    path = await _imageHelper.UploadImageAsync(teamViewModel.LogoFile, "Teams");
                }

                var team = _converterHelper.ToTeamEntity(teamViewModel, path, false);
                _context.Update(team);
                try
                {
                    await _context.SaveChangesAsync();
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {

                    if (ex.InnerException.Message.Contains("duplicate"))
                    {
                        ModelState.AddModelError(string.Empty, $"Already exists the team: {teamViewModel.Name}");
                    }
                    else
                    {
                        ModelState.AddModelError(string.Empty, ex.InnerException.Message);
                    }

                }

            }
            return View(teamViewModel);
        }

        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            TeamEntity teamEntity = await _context.Teams
                .FirstOrDefaultAsync(m => m.Id == id);
            if (teamEntity == null)
            {
                return NotFound();
            }

            _context.Teams.Remove(teamEntity);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

    }
}
