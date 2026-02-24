using System.Threading.Tasks;

namespace Restall.Services;

public interface IParseService
{
    const string RenoDXUrl = "https://github.com/clshortfuse/renodx/releases/tags/";
    const string GithubUrl = "https://github.com/";
    
    Task FetchAllRenoDXVersionsAsync();
    
    Task FetchAllReShadeVersionsAsync();
    
}