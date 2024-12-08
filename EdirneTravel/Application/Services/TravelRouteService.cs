using EdirneTravel.Core.Exception;
using EdirneTravel.Models.Dtos.TravelRoute;
using EdirneTravel.Models.Dtos;
using EdirneTravel.Models.Entities;
using EdirneTravel.Models.Repositories;
using EdirneTravel.Models.Utilities.Results;
using EdirneTravel.Application.Services.Base;
using Microsoft.EntityFrameworkCore;
using Mapster;

namespace EdirneTravel.Application.Services
{
    public class TravelRouteService :  Service<TravelRoute, TravelRouteDto>, ITravelRouteService
    {
        private readonly IRepository<TravelRoutePlace> _travelRoutePlaceRepository;
        private readonly IRepository<Place> _placeRepository;

        public TravelRouteService(
            IRepository<TravelRoute> repository,
            IRepository<TravelRoutePlace> travelRoutePlaceRepository,
            IRepository<Place> placeRepository)
            : base(repository)
        {
            _travelRoutePlaceRepository = travelRoutePlaceRepository;
            _placeRepository = placeRepository;
        }

        public IDataResult<TravelRoute> SaveTravelRouteWithPlaces(CreateTravelRouteWithPlacesDto travelRouteDto)
        {
            // Transaction başlat
            using var transaction = _repository.BeginTransactionAsync().Result;
            try
            {
                if (travelRouteDto == null)
                {
                    throw new ArgumentNullException(nameof(travelRouteDto), "TravelRouteDto cannot be null.");
                }

                TravelRoute travelRoute;

                if (travelRouteDto.Id != 0)
                {
                    // Mevcut TravelRoute güncelle
                    travelRoute = _repository.GetById(travelRouteDto.Id);
                    if (travelRoute == null)
                    {
                        throw new ResourceNotFoundException("ERR_ROUTE_NOT_FOUND");
                    }

                    travelRoute.Name = travelRouteDto.Name;
                    travelRoute.UserId = travelRouteDto.UserId;

                    // İlişkili TravelRoutePlace'leri temizle
                    var existingPlaces = _travelRoutePlaceRepository
                        .Select(trp => trp.TravelRouteId == travelRoute.Id)
                        .ToList();

                    _travelRoutePlaceRepository.DeleteRange(existingPlaces);
                }
                else
                {
                    // Yeni TravelRoute oluştur
                    travelRoute = new TravelRoute
                    {
                        Name = travelRouteDto.Name,
                        UserId = travelRouteDto.UserId
                    };

                    _repository.Insert(travelRoute);
                    _repository.SaveChanges();
                }

                // Yeni yer ilişkilerini ekle
                foreach (var placeDto in travelRouteDto.Places)
                {
                    // Place ID doğrula
                    if (!_placeRepository.Exists(p => p.Id == placeDto.PlaceId))
                    {
                        throw new ResourceNotFoundException($"Place with ID {placeDto.PlaceId} not found.");
                    }

                    var travelRoutePlace = new TravelRoutePlace
                    {
                        TravelRouteId = travelRoute.Id,
                        PlaceId = placeDto.PlaceId,
                        Sequence = placeDto.Sequence
                    };

                    _travelRoutePlaceRepository.Insert(travelRoutePlace);
                }

                _repository.SaveChanges();

                transaction.Commit();

                return new SuccessDataResult<TravelRoute>(travelRoute, "TravelRoute saved successfully with associated places");
            }
            catch (Exception ex)
            {
                transaction.Rollback();
                throw new Exception($"An error occurred while saving TravelRoute: {ex.Message}", ex);
            }
        }

        public IDataResult<TravelRouteDto> GetTravelRouteWithPlacesById(int id)
        {
            var travelRoute = _repository.GetById(id);

            if (travelRoute == null)
                throw new ResourceNotFoundException("ERR_ROUTE_NOT_FOUND");

            // TravelRoutePlace'ları çekerken Place'i de dahil ediyoruz
            var routePlacesQuery = _travelRoutePlaceRepository
                .Select(trp => trp.TravelRouteId == travelRoute.Id)
                .OrderBy(trp => trp.Sequence);

            var routePlacesWithPlace = routePlacesQuery
                .AsQueryable()   // AsQueryable eklemenize gerek olmayabilir, tip zaten IQueryable ise
                .Include(trp => trp.Place)
                .ToList();

            var places = routePlacesWithPlace.Select(trp => new TravelRoutePlaceDto
            {
                PlaceId = trp.PlaceId,
                Sequence = trp.Sequence,
                Name = trp.Place.Name,
                Description = trp.Place.Description,
                ImageData = trp.Place.ImageData,
            }).ToList();

            var dto = new TravelRouteDto
            {
                Id = travelRoute.Id,
                Name = travelRoute.Name,
                UserId = travelRoute.UserId,
                Places = places
            };

            return new SuccessDataResult<TravelRouteDto>(dto);
        }
    }
}
