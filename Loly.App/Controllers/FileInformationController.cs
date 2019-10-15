using System.Collections.Generic;
using System.Threading.Tasks;
using Loly.App.Db.Services;
using Loly.Models;
using Microsoft.AspNetCore.Mvc;

namespace Loly.App.Controllers
{
    [ApiController]
    [Route("api/files")]
    public class FileInformationController : ControllerBase
    {
        private FilesService _service;

        public FileInformationController(FilesService service)
        {
            _service = service;
        }
        
        [HttpGet(Name = "GetAllFiles")]
        [Produces("application/json")]
        public async Task<IEnumerable<IFile>> GetAll()
        {
            return await _service.Get();
        }

        [HttpGet("{id}", Name = "GetFile")]
        [Produces("application/json")]
        public async Task<IActionResult> Get(string id)
        {
            var file = await _service.Get(id);
            if (file == null)
                return NotFound();

            return Ok(file);
        }
    }
}