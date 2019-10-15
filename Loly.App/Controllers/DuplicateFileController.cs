using System.Collections.Generic;
using System.Threading.Tasks;
using Loly.App.Db.Services;
using Loly.App.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.Routing;

namespace Loly.App.Controllers
{
    [ApiController]
    [Route("api/files/duplicates")]
    public class DuplicateFileController : ControllerBase
    {
        private readonly DuplicateFilesService _service;
        private IUrlHelper _urlHelper;

        public DuplicateFileController(DuplicateFilesService service, IUrlHelperFactory urlHelperFactory,
            IActionContextAccessor actionContextAccessor)
        {
            _service = service;
            _urlHelper = urlHelperFactory.GetUrlHelper(actionContextAccessor.ActionContext);
        }

        [HttpGet(Name = "GetAllDuplicateFiles")]
        [Produces("application/json")]
        public async Task<IEnumerable<IDuplicateFile>> GetAll()
        {
            var files = await _service.Get();
            var duplicateFiles = new List<DuplicateFile>();

            foreach (var file in files)
            {
                duplicateFiles.Add(DuplicateFile.FromDbModel(_urlHelper, file));
            }
            return duplicateFiles;
        }
    }
}