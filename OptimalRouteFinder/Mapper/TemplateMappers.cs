using OptimalRouteFinder.Data.Entities;
using OptimalRouteFinder.Models;
using OptimalRouteFinder.ViewModels;

namespace OptimalRouteFinder.Mapper
{
    public static class TemplateMappers
    {
        public static City ToModel(this CityEntity entity)
        {
            return new City
            {
                Name = entity.Name,
                X = entity.X,
                Y = entity.Y,
                IsChecked = false,
            };
        }

        public static CityEntity ToEntity(this City model, int templateId)
        {
            return new CityEntity
            {
                Name = model.Name,
                X = model.X,
                Y = model.Y,
                TemplateId = templateId
            };
        }

        public static RoadViewModel ToModel(this RoadEntity entity, Dictionary<int, City> cityMap)
        {
            return new RoadViewModel
            {
                From = cityMap[entity.FromCityId],
                To = cityMap[entity.ToCityId],
                Distance = entity.Distance
            };
        }

        public static RoadEntity ToEntity(this RoadViewModel model, int templateId, Dictionary<string, int> cityNameToId)
        {
            return new RoadEntity
            {
                TemplateId = templateId,
                FromCityId = cityNameToId[model.From.Name],
                ToCityId = cityNameToId[model.To.Name],
                Distance = model.Distance
            };
        }
    }
}
