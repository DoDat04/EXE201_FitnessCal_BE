using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FitnessCal.BLL.DTO.FoodDTO.Request
{
    public class UploadFileRequest
    {
        [FromForm(Name = "file")]
        public IFormFile File { get; set; } = default!;
    }
}
