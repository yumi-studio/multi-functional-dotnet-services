using System;
using YumiStudio.YumiDotNet.Domain.Entities;

namespace YumiStudio.YumiDotNet.Domain.Interfaces;

public interface ITokenRepository : IRepository<Token, Guid>
{

}
