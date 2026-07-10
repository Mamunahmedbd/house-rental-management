using System;
using Housing_rental.DAL;
using Housing_rental.Models;

namespace Housing_rental.BLL
{
    public class DashboardService
    {
        private readonly DashboardRepository _dashboardRepository;

        public DashboardService()
        {
            _dashboardRepository = new DashboardRepository();
        }

        public ServiceResult<DashboardSummary> GetSummary()
        {
            try
            {
                DashboardSummary summary = _dashboardRepository.GetSummary();
                return ServiceResult<DashboardSummary>.Success(summary, "Dashboard loaded successfully.");
            }
            catch (Exception ex)
            {
                return ServiceResult<DashboardSummary>.Failure("Unable to load dashboard data. " + ex.Message);
            }
        }
    }
}
