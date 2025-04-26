using HIUServices.Entities;
using System.Runtime.InteropServices;

namespace HIUServices.Helpers
{
    public static class CustomApiResponse
    {
        internal static object GetResponse(int statusCode, string message, [Optional] object data,[Optional] EntityValidation[] modelState)
        {
            var msg = new ApiResponse();
            msg.status_code = statusCode;
            msg.message = message;
            msg.data = data;
            msg.errors = modelState;
            return msg;
        }
    }
}
