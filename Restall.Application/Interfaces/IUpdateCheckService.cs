using Restall.Application.DTOs;
using Restall.Domain.Entities;

namespace Restall.Application.Interfaces;

public interface IUpdateCheckService
{
    UpdateCheckResultDto CheckReShadeUpdate(ReShade installed);
    UpdateCheckResultDto CheckRenoDXUpdate(RenoDX installed);
}