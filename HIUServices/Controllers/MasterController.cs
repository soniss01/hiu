
using HIUServices.Entities.Requests.Masters;
using HIUServices.Repostitories.Masters.Interfaces;
using HIUServices.Repostitories.Requests.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace HIUServices.Controllers
{
    [ApiController]
    [Route("api/master/v1")]
    [Tags("Master APIs")]
    public class MasterController : ControllerBase
    {
        private readonly IMasterService masterService;
        private readonly IConfiguration configuration;
        private readonly ILogger<MasterController> logger;
        //donot read
        public MasterController(IMasterService masterService, IConfiguration configuration, ILogger<MasterController> logger)
        {
            this.masterService = masterService;
            this.configuration = configuration;
            this.logger = logger;
            logger.LogInformation("=================================================");
            logger.LogInformation("Master API Started at:" + DateTime.Now);
        }

        [HttpGet]
        [Route("get/purpose-of-use")]
        public async Task<ActionResult<List<PurposeOfUse>>> GetPurposeOfUse()
        {
            try
            {
                var purposeOfUses = await masterService.GetPurposeOfUse();
                return Ok(purposeOfUses);
            }
            catch (Exception ex)
            {
                logger.LogError("Error:" + ex.Message + " " + ex.InnerException + "" + ex.StackTrace);
                return Ok(ex.Message + " " + ex.InnerException + "" + ex.StackTrace);
            }
        }

        [HttpGet]
        [Route("get/hi-types")]
        public async Task<ActionResult<List<PurposeOfUse>>> GetHiTypes()
        {
            try
            {
                var hiTypes = await masterService.GetHiTypes();
                return Ok(hiTypes);
            }
            catch (Exception ex)
            {
                logger.LogError("Error:" + ex.Message + " " + ex.InnerException + "" + ex.StackTrace);
                return Ok(ex.Message + " " + ex.InnerException + "" + ex.StackTrace);
            }
        }
    }
}
