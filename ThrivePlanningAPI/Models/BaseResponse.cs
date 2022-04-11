using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ThrivePlanningAPI.Models
{
    public interface IBaseResponse
    {
        public bool Successful { get; set; }
        public string Error { get; set; }
    }

    public abstract class BaseResponse : IBaseResponse
    {
        protected BaseResponse()
        {
            Successful = false;
            Error = "Unknown error";
        }

        public bool Successful { get; set; }
        public string Error { get; set; }
    }
}