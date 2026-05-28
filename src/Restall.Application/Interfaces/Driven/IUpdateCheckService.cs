using Restall.Application.DTOs;
using Restall.Application.DTOs.Results;
using Restall.Domain.Entities;

namespace Restall.Application.Interfaces.Driven;

public interface IUpdateCheckService
{
    UpdateCheckResultDto CheckReShadeUpdate(ReShade installed);
    UpdateCheckResultDto CheckRenoDXUpdate(RenoDX installed);
}