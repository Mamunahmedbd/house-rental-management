using System.Configuration;
using System.Data.SqlClient;

namespace Housing_rental.DAL
{
    public static class DbConnectionFactory
    {
        public static SqlConnection CreateConnection()
        {
            string connectionString = ConfigurationManager
                .ConnectionStrings["HouseRentalDB"]
                .ConnectionString;

            return new SqlConnection(connectionString);
        }
    }
}
