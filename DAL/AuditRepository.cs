using System.Data.SqlClient;

namespace Housing_rental.DAL
{
    public class AuditRepository
    {
        public void Add(int? userId, string actionName, string tableName, string recordId, string description)
        {
            const string sql = @"
INSERT INTO AuditLogs (UserId, ActionName, TableName, RecordId, Description, CreatedAt)
VALUES (@UserId, @ActionName, @TableName, @RecordId, @Description, GETDATE());";

            using (SqlConnection connection = DbConnectionFactory.CreateConnection())
            using (SqlCommand command = new SqlCommand(sql, connection))
            {
                command.Parameters.Add(SqlHelper.Parameter("@UserId", userId));
                command.Parameters.Add(SqlHelper.Parameter("@ActionName", actionName));
                command.Parameters.Add(SqlHelper.Parameter("@TableName", tableName));
                command.Parameters.Add(SqlHelper.Parameter("@RecordId", recordId));
                command.Parameters.Add(SqlHelper.Parameter("@Description", description));

                connection.Open();
                command.ExecuteNonQuery();
            }
        }
    }
}
