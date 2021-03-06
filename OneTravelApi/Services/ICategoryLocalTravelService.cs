﻿using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using OneTravelApi.DataLayer;
using OneTravelApi.EntityLayer;
using OneTravelApi.Responses;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace OneTravelApi.Services
{
    public interface ICategoryLocalTravelService : IBaseService<CategoryLocalTravel>
    {

    }

    public class CategoryLocalTravelService : ICategoryLocalTravelService
    {
        private readonly IRepository<CategoryLocalTravel> _repository;
        private readonly IMapper _mapper;
        private readonly ILogger<CategoryLocalTravelService> _logger;

        public CategoryLocalTravelService(IRepository<CategoryLocalTravel> repository, IMapper mapper, ILogger<CategoryLocalTravelService> logger)
        {
            _repository = repository;
            _mapper = mapper;
            _logger = logger;
        }

        public IActionResult GetAll(int? pageSize, int? pageNumber, string q = null)
        {

            var response = new ListModelResponse<CategoryLocalTravel>
            {
                PageSize = (int)pageSize,
                PageNumber = (int)pageNumber
            };
            var query = _repository.Query().Include(x => x.City).Skip((response.PageNumber - 1) * response.PageSize)
                .Take(response.PageSize).ToList();

            if (!string.IsNullOrEmpty(q) && query.Any())
            {
                q = q.ToLower();
                query = query.Where(x =>
                                    x.LocalName.ToLower().Contains(q) ||
                                    x.LocalCode.ToLower().Contains(q)).ToList();
            }

            try
            {
                response.Model = query;

                response.Message = string.Format("Total of records: {0}", response.Model.Count());
            }
            catch (Exception ex)
            {
                response.DidError = true;
                response.ErrorMessage = ex.Message;
                _logger.LogError(ex.InnerException.ToString());
            }

            return response.ToHttpResponse();
        }

        public async Task<IActionResult> GetAsync(int id)
        {
            var response = new SingleModelResponse<CategoryLocalTravel>();

            try
            {
                var entity = await _repository.Query().Include(x => x.City).FirstOrDefaultAsync(x => x.Id == id);

                if (entity == null)
                {
                    response.DidError = true;
                    response.ErrorMessage = ResponseMessageConstants.NotFound;
                    return response.ToHttpResponse();
                }

                response.Model = entity;
            }
            catch (Exception ex)
            {
                response.DidError = true;
                response.ErrorMessage = ex.Message;
                _logger.LogError(ex.InnerException.ToString());
            }

            return response.ToHttpResponse();
        }

        public async Task<IActionResult> Create(CategoryLocalTravel resource)
        {
            var response = new SingleModelResponse<CategoryLocalTravel>();

            if (resource == null)
            {
                response.DidError = true;
                response.ErrorMessage = ResponseMessageConstants.NotFound;
                return response.ToHttpResponse();
            }

            try
            {
                var entity = new CategoryLocalTravel();
                _mapper.Map(resource, entity);

                var entityAdded = await _repository.AddAsync(entity);

                response.Model = entityAdded;
                response.Message = ResponseMessageConstants.Success;
            }
            catch (Exception ex)
            {
                response.DidError = true;
                response.ErrorMessage = ex.ToString();
                _logger.LogError(ex.InnerException.ToString());
            }

            return response.ToHttpResponse();
        }

        public async Task<IActionResult> Update(int id, CategoryLocalTravel resource)
        {
            var response = new SingleModelResponse<CategoryLocalTravel>();

            if (resource == null)
            {
                response.DidError = true;
                response.ErrorMessage = ResponseMessageConstants.NotFound;
                return response.ToHttpResponse();
            }
            try
            {
                var entity = await _repository.FindAsync(x => x.Id == id);

                if (entity == null)
                {
                    response.DidError = true;
                    response.ErrorMessage = ResponseMessageConstants.NotFound;
                    return response.ToHttpResponse();
                }

                entity.LocalName = resource.LocalName;
                entity.LocalCode = resource.LocalCode;
                entity.IdCity = resource.IdCity;
                entity.Description = resource.Description;
                entity.IsActive = resource.IsActive;

                await _repository.UpdateAsync(entity);

                response.Model = entity;
                response.Message = ResponseMessageConstants.Success;
            }
            catch (Exception ex)
            {
                response.DidError = true;
                response.ErrorMessage = ex.ToString();
                _logger.LogInformation(ex.Message);
                _logger.LogTrace(ex.InnerException.ToString());
            }

            return response.ToHttpResponse();
        }

        public async Task<IActionResult> Delete(int id)
        {
            var response = new SingleModelResponse<CategoryLocalTravel>();

            try
            {
                var entity = await _repository.DeleteAsync(id);

                response.Model = entity;
                response.Message = ResponseMessageConstants.Delete;
            }
            catch (Exception ex)
            {
                response.DidError = true;
                response.ErrorMessage = ex.Message;
                _logger.LogError(ex.InnerException.ToString());
            }

            return response.ToHttpResponse();
        }
    }
}