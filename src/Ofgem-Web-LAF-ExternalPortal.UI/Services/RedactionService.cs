using ChoETL;
using Ofgem.LAF.SharedLibrary.Extensions;
using Ofgem.LAF.SharedLibrary.Models;
using System.Text;

namespace Ofgem_Web_LAF_ExternalPortal.Services
{
    public interface IRedactionService
    {
        byte[] RedactPersonalInformation(byte[] fileBytes);
    }




    public class RedactionService(ILogger<RedactionService> logger) : IRedactionService
    {
        public byte[] RedactPersonalInformation(byte[] fileBytes)
        {
            try
            {
                // convert csv file bytes to raw declarations
                var rawDeclarations = Ofgem.LAF.SharedLibrary.Extensions.Declaration.ConvertToObject(fileBytes);

                // locate and redact any personal information
                foreach (var declaration in rawDeclarations)
                {
                    if (declaration.ECO4_or_Great_British_Insulation_Scheme_Flex_Referral_Route != "Route 3") continue;

                    if (declaration.Route_2_Proxies is "" or "N/A") continue;

                    declaration.Route_2_Proxies = "";
                }

                // convert the objects back to a byte array
                using var ms = new MemoryStream();
                using (var w = new ChoCSVWriter<RawDeclaration>(new StreamWriter(ms, Encoding.UTF8)).WithFirstLineHeader())
                {
                    w.Write(rawDeclarations);
                }

                return ms.ToArray();
            }
            catch (Exception ex)
            {
                logger.LogLafError(LogEvents.RedactionService, "{Message}", ex.Message);
                throw;
            }
        }
    }
}
