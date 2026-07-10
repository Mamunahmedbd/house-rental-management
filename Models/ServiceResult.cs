namespace Housing_rental.Models
{
    public class ServiceResult
    {
        public bool IsSuccess { get; protected set; }
        public string Message { get; protected set; }

        public static ServiceResult Success(string message)
        {
            return new ServiceResult { IsSuccess = true, Message = message };
        }

        public static ServiceResult Failure(string message)
        {
            return new ServiceResult { IsSuccess = false, Message = message };
        }
    }

    public class ServiceResult<T> : ServiceResult
    {
        public T Data { get; private set; }

        public static ServiceResult<T> Success(T data, string message)
        {
            return new ServiceResult<T> { IsSuccess = true, Data = data, Message = message };
        }

        public new static ServiceResult<T> Failure(string message)
        {
            return new ServiceResult<T> { IsSuccess = false, Message = message };
        }
    }
}
