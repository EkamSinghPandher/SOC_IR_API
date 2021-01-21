﻿using Microsoft.EntityFrameworkCore;
using SOC_IR.Data;
using SOC_IR.Dtos.Company;
using SOC_IR.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SOC_IR.Services.CompanyService
{
    public class CompanyService : ICompanyService
    {
        private readonly DataContext _context;
        public CompanyService(DataContext context)
        {
            _context = context;
        }

        async Task<ServiceResponse<List<GetCompanyAdminDto>>> ICompanyService.CreateCompany(CreateCompanyDto companyDto)
        {
            ServiceResponse<List<GetCompanyAdminDto>> response = new ServiceResponse<List<GetCompanyAdminDto>>();
            if (_context.Companies.First(a => a.companyName == companyDto.companyName) != null)
            {
                response.Success = false;
                response.Message = "This company already has an account";
                return response;
            }

            string finalString = new IDGenerator.IDGenerator().generate();

            Company newCompany = new Company(finalString, companyDto.companyName, companyDto.companyTier, companyDto.companyDescription, true);
            await _context.Companies.AddAsync(newCompany);
            await _context.SaveChangesAsync();
            List<Company> newCompanyList = await _context.Companies.ToListAsync();
            List<GetCompanyAdminDto> data = newCompanyList.Select(a => new GetCompanyAdminDto(a.companyId, a.companyName, a.companyTier, a.companyDescription, a.isActive)).ToList();
            response.Data = data;
            return response;
        }

        async Task<ServiceResponse<List<GetCompanyAdminDto>>> ICompanyService.DeleteCompany(string id)
        {
            Company removed = await _context.Companies.FirstAsync(a => a.companyId == id);
            List<CompanyUser> users = await _context.CompanyUsers.Where(a => a.companyId == id).ToListAsync();
            List<CompanyPost> posts = await _context.CompanyPosts.Where(a => a.companyId == id).ToListAsync();
            List<CompanyPostRequest> requests = await _context.CompanyPostRequests.Where(a => a.companyId == id).ToListAsync();
            _context.Companies.Remove(removed);
            _context.CompanyUsers.RemoveRange(users);
            _context.CompanyPosts.RemoveRange(posts);
            _context.CompanyPostRequests.RemoveRange(requests);
            await _context.SaveChangesAsync();

            ServiceResponse<List<GetCompanyAdminDto>> response = new ServiceResponse<List<GetCompanyAdminDto>>();
            List<Company> newCompanyList = await _context.Companies.ToListAsync();
            List<GetCompanyAdminDto> data = newCompanyList.Select(a => new GetCompanyAdminDto(a.companyId, a.companyName, a.companyTier, a.companyDescription, a.isActive)).ToList();
            response.Data =data;
            return response;
        }

        async Task<ServiceResponse<List<GetCompanyAdminDto>>> ICompanyService.GetCompanyAdmin()
        {
            ServiceResponse<List<GetCompanyAdminDto>> response = new ServiceResponse<List<GetCompanyAdminDto>>();
            List<Company> newCompanyList = await _context.Companies.ToListAsync();
            List<GetCompanyAdminDto> data = newCompanyList.Select(a => new GetCompanyAdminDto(a.companyId, a.companyName, a.companyTier, a.companyDescription, a.isActive)).ToList();
            response.Data = data;
            return response;
        }

        async Task<ServiceResponse<List<GetCompanyStudentDto>>> ICompanyService.GetCompanyStudent()
        {
            ServiceResponse<List<GetCompanyStudentDto>> response = new ServiceResponse<List<GetCompanyStudentDto>>();
            List<Company> newCompanyList = await _context.Companies.ToListAsync();
            List<GetCompanyStudentDto> data = newCompanyList.Select(a => new GetCompanyStudentDto(a.companyId, a.companyName, a.companyDescription)).ToList();
            response.Data = data;
            return response;
        }

        async Task<ServiceResponse<GetCompanyAdminDto>> ICompanyService.UpdateCompany(UpdateCompanyDto updatedCompanyDto)
        {
            Company update = await _context.Companies.FirstAsync(a => a.companyId == updatedCompanyDto.companyId);
            Company updated = new Company(updatedCompanyDto.companyId, updatedCompanyDto.companyName, updatedCompanyDto.companyTier,
                updatedCompanyDto.companyDescription, updatedCompanyDto.isActive);


            if (update.companyName != updated.companyName)
            {
                List<CompanyPost> posts = await _context.CompanyPosts.Where(a => a.companyId == updatedCompanyDto.companyId).ToListAsync();
                posts.ForEach(a => a.companyUpdated(updatedCompanyDto.companyName));
                List<CompanyPostRequest> requests = await _context.CompanyPostRequests.Where(a => a.companyId == updatedCompanyDto.companyId).ToListAsync();
                posts.ForEach(a => a.companyUpdated(updatedCompanyDto.companyName));
                requests.ForEach(a => a.companyUpdated(updatedCompanyDto.companyName));
                _context.CompanyPosts.UpdateRange(posts);
                _context.CompanyPostRequests.UpdateRange(requests);
            }

            _context.Companies.Update(updated);
            await _context.SaveChangesAsync();
            ServiceResponse<GetCompanyAdminDto> response= new ServiceResponse<GetCompanyAdminDto>();
            GetCompanyAdminDto data = new GetCompanyAdminDto(updated.companyId, updated.companyName, updated.companyTier, updated.companyDescription, updated.isActive);
            response.Data = data;
            return response;
        }

        async Task<ServiceResponse<GetCompanyAdminDto>> ICompanyService.ArchiveCompany(string companyId)
        {
            Company company = await _context.Companies.FirstAsync(a => a.companyId == companyId);
            List<CompanyPost> companyPosts = await _context.CompanyPosts.Where(a => a.companyId == company.companyId).ToListAsync();
            companyPosts.ForEach(a => a.archivePost());
            List<CompanyPostRequest> requests = await _context.CompanyPostRequests.Where(a => a.companyId == company.companyId).ToListAsync();
            List<CompanyUser> companyUsers = await _context.CompanyUsers.Where(a => a.companyId == company.companyId).ToListAsync();
            companyUsers.ForEach(a => a.archiveUser());

            _context.CompanyPosts.UpdateRange(companyPosts);
            _context.CompanyPostRequests.RemoveRange(requests);

            _context.Companies.Update(company);
            await _context.SaveChangesAsync();
            ServiceResponse<GetCompanyAdminDto> response = new ServiceResponse<GetCompanyAdminDto>();
            GetCompanyAdminDto data = new GetCompanyAdminDto(company.companyId, company.companyName, company.companyTier, company.companyDescription, company.isActive);
            response.Data = data;
            return response;
        }
    }
}
