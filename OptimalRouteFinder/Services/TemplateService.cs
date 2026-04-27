using OptimalRouteFinder.Data.Entities;
using OptimalRouteFinder.Data;
using OptimalRouteFinder.Models;
using Microsoft.EntityFrameworkCore;
using OptimalRouteFinder.Mapper;
using OptimalRouteFinder.ViewModels;

namespace OptimalRouteFinder.Services
{
    public class TemplateService
    {
        private readonly MapDbContext _db;

        public TemplateService(MapDbContext db)
        {
            _db = db;
        }

        // Get all template names
        public async Task<List<string>> GetAllTemplateNamesAsync(int userId)
        {
            return await _db.Templates.Where(t => t.UserId == userId)
                .OrderBy(t => t.Name)
                .Select(t => t.Name)
                .ToListAsync();
        }

        // Load template by name (cities + roads)
        public async Task<(List<City> Cities, List<RoadViewModel> Roads)> LoadTemplateAsync(string templateName)
        {
            var template = await _db.Templates
                .Include(t => t.Cities)
                .Include(t => t.Roads)
                .FirstOrDefaultAsync(t => t.Name == templateName && t.UserId == UserSession.CurrentUser.Id);

            if (template == null)
                return (new List<City>(), new List<RoadViewModel>());

            var cities = template.Cities.Select(c => c.ToModel()).ToList();

            var cityMap = cities.Zip(template.Cities, (model, entity) => new { model, entity })
                                .ToDictionary(x => x.entity.Id, x => x.model);

            var roads = template.Roads.Select(r => r.ToModel(cityMap)).ToList();

            return (cities, roads);
        }

        // Save or update template
        public async Task SaveTemplateAsync(string templateName, List<City> cities, List<RoadViewModel> roads)
        {
            if (string.IsNullOrWhiteSpace(templateName))
                throw new ArgumentException("Template name required");

            var existingTemplate = await _db.Templates
                .Include(t => t.Cities)
                .Include(t => t.Roads)
                .FirstOrDefaultAsync(t => t.Name == templateName);

            if (existingTemplate != null)
            {
                _db.Roads.RemoveRange(existingTemplate.Roads);
                _db.Cities.RemoveRange(existingTemplate.Cities);
            }
            else
            {
                existingTemplate = new Template { Name = templateName, UserId = UserSession.CurrentUser.Id };
                _db.Templates.Add(existingTemplate);
            }

            await _db.SaveChangesAsync(); // get TemplateId

            // Insert cities
            var cityEntities = cities.Select(c => c.ToEntity(existingTemplate.Id)).ToList();
            _db.Cities.AddRange(cityEntities);
            await _db.SaveChangesAsync();

            var cityNameToId = cityEntities.ToDictionary(c => c.Name, c => c.Id);

            // Insert roads
            var roadEntities = roads.Select(r => r.ToEntity(existingTemplate.Id, cityNameToId)).ToList();
            _db.Roads.AddRange(roadEntities);

            await _db.SaveChangesAsync();
        }

        // Delete template
        public async Task DeleteTemplateAsync(string templateName)
        {
            var template = await _db.Templates
                .Include(t => t.Cities)
                .Include(t => t.Roads)
                .FirstOrDefaultAsync(t => t.Name == templateName);

            if (template != null)
            {
                _db.Roads.RemoveRange(template.Roads);
                _db.Cities.RemoveRange(template.Cities);
                _db.Templates.Remove(template);

                await _db.SaveChangesAsync();
            }
        }
    }
}
